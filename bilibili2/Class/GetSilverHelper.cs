/**
——————此类由 座子狮 提供——————
**/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace bilibili2.Class
{
    public class GetSilverHelper
    {
        private Windows.UI.Xaml.DispatcherTimer timer = new Windows.UI.Xaml.DispatcherTimer(); //用于定时发送心跳包
        public delegate void identifying(heartdata data,string result);
        public delegate void ErrorInfo( string result);
        public event identifying guazievent;//可以领取瓜子时引发的事件

        public GetSilverHelper()
        {
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += send_heart;
        }

        public void StartHeart()
        {
            timer.Start();
        }
        public void EndHeart()
        {
            timer.Stop();
        }

        /// <summary>
        /// 读取验证码
        /// </summary>
        /// <returns></returns>
        public async Task<BitmapImage> GetCaptcha()
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    HttpResponseMessage hr =await hc.GetAsync(new Uri("http://live.bilibili.com/FreeSilver/getCaptcha"));
                    hr.EnsureSuccessStatusCode();
                    var result = await hr.Content.ReadAsBufferAsync();
                    var steam = result.AsStream();
                    var img = new BitmapImage();
                    await img.SetSourceAsync(steam.AsRandomAccessStream());
                    return img;
                }          
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 领取瓜子
        /// </summary>
        /// <param name="vcode"></param>
        /// <returns></returns>
        public async Task<getSilverresult> GetSliver(string vcode)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    HttpResponseMessage hr = await hc.GetAsync(new Uri("http://live.bilibili.com/freeSilver/getAward?captcha=" + vcode));
                    hr.EnsureSuccessStatusCode();
                    var result =await hr.Content.ReadAsStringAsync();
                    getSilverresult model = JsonConvert.DeserializeObject<getSilverresult>(result);
                    if (model.code == 0)
                    {
                        
                        //瓜子领取成功
                        //{"code":0,"msg":"ok","data":{"silver":640,"awardSilver":30,"getVote":1,"svote":18,"vote":null}}
                    }
                    if (model.code == -400)
                    {
                        //验证码错误
                        //{"code": -400,"msg": "验证码错误", "data": []}
                    }
                    return model;
                    // 
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 发送心跳
        /// </summary>
        private async void SendHeart()
        {
            try
            {
                using (HttpClient hc=new HttpClient())
                {
                    HttpResponseMessage hr =await hc.GetAsync(new Uri("http://live.bilibili.com/freeSilver/heart"));
                    hr.EnsureSuccessStatusCode();
                    var result =await hr.Content.ReadAsStringAsync();
                    heartdata model = JsonConvert.DeserializeObject<heartdata>(result);
                    if (model.code == 1)
                    {
                        //心跳正常
                    }
                    if (model.code == 0)
                    {
                        //可以领瓜子了
                        guazievent(model, result);//激活事件
                    }
                    if (model.code == -400)
                    {
                        //用户还没有签到，无法发送心跳
                    }
                    if (model.code == 403)
                    {
                        //非法心跳(就是一分钟之内发送了多个心跳),用户在一分钟之内换了两个直播间会触发这个
                    }

                }
                
            }
            catch (Exception)
            {

            }
        }

        private void send_heart(object sender, object e)
        {
            SendHeart();
        }

        public class heartdata
        {
            //Josn：http://live.bilibili.com/freeSilver/heart
            //这个API在用户看直播的时候每分钟请求一次,需要cookie
            //第一层
            public int code { set; get; }
            public string msg { set; get; }
            public object data { set; get; }//第二层object
            //第二层
            public int minute { set; get; }
            public int silver { set; get; }
            public int vote { set; get; }
        }
        public class getSilverresult
        {
            //Josn：http://live.bilibili.com/freeSilver/getAward?captcha=56
            //这个API在用户看直播的时候每分钟请求一次,需要cookie
            //第一层
            public int code { set; get; }
            public string msg { set; get; }
            public object data { set; get; }//第二层object
            //第二层
            public int minute { set; get; }
            public int silver { set; get; }
            public int vote { set; get; }
        }

    }
}