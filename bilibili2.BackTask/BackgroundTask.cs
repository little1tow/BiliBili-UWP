using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Web.Http;

namespace bilibili2.BackTask
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        public async  void Run(IBackgroundTaskInstance taskInstance)
        {
            //通知
            ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
            bool Update = true;
            if (container.Values["UpdateCT"]!=null)
            {
                Update = (bool)container.Values["UpdateCT"];
            }
            else
            {
                container.Values["UpdateCT"] = true;
            }
            if (Update)
            {
                var deferral = taskInstance.GetDeferral();
                await GetLatestNews();
                deferral.Complete();
            }
            else
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
            }
        }

        private IAsyncOperation<string> GetLatestNews()
        {
            try
            {
                return AsyncInfo.Run(token => GetNews());
            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }

        private async Task<string> GetNews()
        {
            try
            {
                var response = await GetUserAttentionUpdate();
               
                if (response != null)
                {
                    //var news = response.Data.Take(5).ToList();
                    UpdatePrimaryTile(response);
                    //UpdateSecondaryTile(response);
                }

            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }


        private void UpdatePrimaryTile(List<GetAttentionUpdate> news)
        {
            if (news == null || !news.Any())
            {
                return;
            }

            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueueForWide310x150(true);
                updater.EnableNotificationQueueForSquare150x150(true);
                updater.EnableNotificationQueueForSquare310x310(true);
                updater.EnableNotificationQueue(true);
                updater.Clear();

                foreach (var n in news)
                {
                    var doc = new XmlDocument();
                    var xml = string.Format(TileTemplateXml, n.pic, n.title, n.description);
                    doc.LoadXml(WebUtility.HtmlDecode(xml), new XmlLoadSettings
                    {
                        ProhibitDtd = false,
                        ValidateOnParse = false,
                        ElementContentWhiteSpace = false,
                        ResolveExternals = false
                    });

                    updater.Update(new TileNotification(doc));
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// 关注动态
        /// </summary>
        /// <returns></returns>
        private async Task<List<GetAttentionUpdate>> GetUserAttentionUpdate()
        {
                try
                {
                    using (HttpClient hc = new HttpClient())
                    {
                        HttpResponseMessage hr = await hc.GetAsync(new Uri("http://api.bilibili.com/x/feed/pull?jsonp=jsonp&ps=20&type=1&pn=0&rnd="+new Random().Next(1,9999)));
                        hr.EnsureSuccessStatusCode();
                        string results = await hr.Content.ReadAsStringAsync();
                        //一层
                        GetAttentionUpdate model1 = JsonConvert.DeserializeObject<GetAttentionUpdate>(results);
                        if (model1.code == 0)
                        {
                            //二层
                            GetAttentionUpdate model2 = JsonConvert.DeserializeObject<GetAttentionUpdate>(model1.data.ToString());
                            //三层
                            List<GetAttentionUpdate> ls = JsonConvert.DeserializeObject<List<GetAttentionUpdate>>(model2.feeds.ToString());
                            //四层
                            List<GetAttentionUpdate> lsModel = new List<GetAttentionUpdate>();
                            foreach (GetAttentionUpdate item in ls)
                            {
                                GetAttentionUpdate m = JsonConvert.DeserializeObject<GetAttentionUpdate>(item.addition.ToString());
                                m.page = model2.page;
                                lsModel.Add(m);
                            }
                        List<GetAttentionUpdate> lsModel2 = new List<GetAttentionUpdate>();
                        for (int i = 0; i < 5; i++)
                        {
                            lsModel2.Add(lsModel[i]);
                        }
                            return lsModel2;
                        }
                        else
                        {
                            return null;
                        }
                    }

                }
                catch (Exception)
                {
                    return null;
                }
        }

        private const string TileTemplateXml = @"
<tile branding='name'> 
  <visual version='3'>
    <binding template='TileMedium'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileWide'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileLarge'>
      <image src='{0}' placement='peek'/>
      <text>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
  </visual>
</tile>";
        private class GetAttentionUpdate
        {
            //必须有登录Cookie
            //Josn：http://api.bilibili.com/x/feed/pull?jsonp=jsonp&ps=20&type=1&pn=1
            //第一层
            public int code { get; set; }//状态，0为正常
            public object data { get; set; }//数据，包含第二层
                                            //第二层
            public object feeds { get; set; }//结果，包含第三层
            public object page { get; set; }//结果数量，包含第三层
                                            //第三层
            public string add_id { get; set; }//视频ID
            public object source { get; set; }//作者信息，包含第四层
            public object addition { get; set; }//视频信息，包含第四层
                                                //第四层
            public string author { get; set; }//上传人员
            public string mid { get; set; }//上传人员ID
            public string aid { get; set; }//视频ID
            public string title { get; set; }//标题
            public string play { get; set; }//播放数
            public string video_review { get; set; }//弹幕数
            public string create { get; set; }//上传时间
            public string pic { get; set; }//封面
            public string description { get; set; }
            public string Create
            {
                get
                {
                    DateTime dt = Convert.ToDateTime(create);

                    if (dt.Date == DateTime.Now.Date)
                    {
                        TimeSpan ts = DateTime.Now - dt;
                        return ts.Hours + "小时前";
                    }
                    else
                    {
                        return create;
                    }
                }
            }

        }
    }
}
