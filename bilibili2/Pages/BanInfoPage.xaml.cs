using bilibili2.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BanInfoPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public BanInfoPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }
       

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                BackEvent();
            }
        }

        WebClientClass wc;
        string banID = "";
        //bool IsBan = false;
        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            if (e.NavigationMode == NavigationMode.New)
            {
                font_icon.Glyph = "\uE006";
                btn_concern.Label = "订阅";
                banID = e.Parameter as string;
                //IsBan = ((KeyValuePair<string, bool>)e.Parameter).Value;
                GetBangumiInfo(banID);
            }
        }

        public async void SaveHiss(string id, string title, string type)
        {
            try
            {
                XmlDocument HisDoc = new XmlDocument();
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile xmlfile = await folder.CreateFileAsync("History.xml", CreationCollisionOption.OpenIfExists);
                string results = await FileIO.ReadTextAsync(xmlfile);
                if (results == "")
                {
                    await FileIO.WriteTextAsync(xmlfile, @"<History></History>");
                    results = @"<History></History>";
                }
                HisDoc.LoadXml(results);
                XmlElement el = HisDoc.DocumentElement;
                XmlElement x = HisDoc.CreateElement("info");
                x.SetAttribute("p", id);
                x.SetAttribute("type", type);
                x.SetAttribute("date", DateTime.Now.ToString());
                x.SetAttribute("title", title);
                el.AppendChild(x);
                await FileIO.WriteTextAsync(xmlfile, HisDoc.InnerXml);
            }
            catch (Exception)
            {
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            wc = null;
         
        }
        public async void GetBangumiInfo(string banID)
        {
            //string uri = "http://bangumi.bilibili.com/api/season?_device=wp&_ulv=10000&build=411005&platform=android&appkey=422fd9d7289a1dd9&ts="+APIHelper.GetTimeSpen+ "000&type=sp&sp_id=56719";
            //string sign=  APIHelper.GetSign(uri);
            //uri += "&sign=" + sign;
            try
            {
                pr_load.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string uri = "";
                    uri = string.Format("http://bangumi.bilibili.com/api/season?_device=wp&access_key={2}&_ulv=10000&build=411005&platform=android&appkey=422fd9d7289a1dd9&ts={0}000&type=bangumi&season_id={1}", ApiHelper.GetTimeSpen, banID, ApiHelper.access_key);
                uri += "&sign=" + ApiHelper.GetSign(uri);
                string result = await wc.GetResults(new Uri(uri));
                BangumiInfoModel model = new BangumiInfoModel();
                if ((int)JObject.Parse(result)["code"] == 0)
                {
                    model = JsonConvert.DeserializeObject<BangumiInfoModel>(JObject.Parse(result)["result"].ToString());
                    grid_Info.DataContext = model;
                    BangumiInfoModel m = JsonConvert.DeserializeObject<BangumiInfoModel>(model.user_season.ToString());
                    if (m.attention == 0)
                    {
                        font_icon.Glyph = "\uE006";
                        btn_concern.Label = "订阅";
                    }
                    else
                    {
                        font_icon.Glyph = "\uE00B";
                        btn_concern.Label = "取消订阅";
                    }

                    List<BangumiInfoModel> list = JsonConvert.DeserializeObject<List<BangumiInfoModel>>(model.episodes.ToString());
                    List<BangumiInfoModel> list2 = new List<BangumiInfoModel>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Num = i;
                        list2.Add(list[i]);
                    }
                    list_E.ItemsSource = list2;
                    List<BangumiInfoModel> list_CV = JsonConvert.DeserializeObject<List<BangumiInfoModel>>(model.actor.ToString());
                    txt_CV.Text = "";
                    foreach (BangumiInfoModel item in list_CV)
                    {
                        txt_CV.Text += string.Format("{0}:{1}\r\n", item.role, item.actor);
                    }
                    List<BangumiInfoModel> list_Tag = JsonConvert.DeserializeObject<List<BangumiInfoModel>>(model.tags.ToString());
                    Grid_tag.Children.Clear();
                    foreach (BangumiInfoModel item in list_Tag)
                    {
                        HyperlinkButton btn = new HyperlinkButton();
                        btn.DataContext = item;
                        btn.Margin = new Thickness(0, 0, 10, 0);
                        btn.Content = item.tag_name;
                        btn.Click += Btn_Click;
                        Grid_tag.Children.Add(btn);
                    }
                }
                if ((int)JObject.Parse(result)["code"] == -3)
                {
                    messShow.Show("密钥注册失败，请联系作者",3000);
                }
                if ((int)JObject.Parse(result)["code"] == 10)
                {
                    messShow.Show(JObject.Parse(result)["message"].ToString(), 3000);
                }
            }
            catch (Exception ex)
            {
                messShow.Show("发生错误\r\n" + ex.Message, 3000);
            }
            finally
            {
                pr_load.Visibility = Visibility.Collapsed;

            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            string tid = ((sender as HyperlinkButton).DataContext as BangumiInfoModel).tag_id.ToString();
            string name = ((sender as HyperlinkButton).DataContext as BangumiInfoModel).tag_name.ToString();
            if (tid != null)
            {
                this.Frame.Navigate(typeof(BanByTagPage), new string[]{tid,name});
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bor_Width.Width = Width / 3;
        }

        private void grid_E_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (cb_IsPlay.IsChecked.Value)
            {
                BangumiInfoModel model = e.ClickedItem as BangumiInfoModel;
                List<VideoModel> listVideo = new List<VideoModel>();
                List<BangumiInfoModel> ls = ((List<BangumiInfoModel>)list_E.ItemsSource).OrderByDescending(s => Convert.ToDouble(s.Num)).ToList();
                foreach (BangumiInfoModel item in ls)
                {
                    listVideo.Add(new VideoModel() { aid = item.av_id, title = txt_Name.Text, cid = item.danmaku.ToString(), page = item.index, part = item.index_title ?? "" });
                }
                KeyValuePair<List<VideoModel>, int> list = new KeyValuePair<List<VideoModel>, int>(listVideo, ls.IndexOf(e.ClickedItem as BangumiInfoModel));
                PostHistory(model.av_id);
                this.Frame.Navigate(typeof(PlayerPage), list);
            }
            else
            {
                this.Frame.Navigate(typeof(VideoInfoPage), (e.ClickedItem as BangumiInfoModel).av_id);
            }
        }
        private async void PostHistory(string aid)
        {
            try
            {
                WebClientClass wc = new WebClientClass();
                string url = string.Format("http://api.bilibili.com/x/history/add?_device=wp&_ulv=10000&access_key={0}&appkey={1}&build=411005&platform=android", ApiHelper.access_key, ApiHelper._appKey);
                url += "&sign=" + ApiHelper.GetSign(url);
                string result = await wc.PostResults(new Uri(url), "aid=" + aid);
            }
            catch (Exception)
            {
            }
        }
        //private async void btn_OK_Click(object sender, RoutedEventArgs e)
        //{
        //    using (DownMangentClass wc = new DownMangentClass())
        //    {
        //        if (list_E.SelectedItems.Count != 0)
        //        {
        //            //循环读取全部选中的项目
        //            foreach (BangumiInfoModel item in list_E.SelectedItems)
        //            {
        //                int quality = cb_Qu.SelectedIndex + 1;//清晰度1-3
        //                string Downurl = await wc.GetVideoUri(item.danmaku.ToString(), quality);//取得视频URL
        //                if (Downurl != null)
        //                {
        //                    DownMangentClass.DownModel model = new DownMangentClass.DownModel()
        //                    {
        //                        mid = item.danmaku.ToString(),
        //                        title = (grid_Info.DataContext as BangumiInfoModel).title,
        //                        part = item.index,
        //                        url = Downurl,
        //                        aid = item.av_id,
        //                        danmuUrl = "http://comment.bilibili.com/" + item.danmaku + ".xml",
        //                        quality = quality,
        //                        downloaded = false,
        //                        partTitle = item.index_title ?? item.index
        //                    };
        //                    wc.StartDownload(model);
        //                    //StartDownload(model);
        //                }
        //                else
        //                {
        //                    MessageDialog md = new MessageDialog(item.title + "\t视频地址获取失败");
        //                    await md.ShowAsync();
        //                }
        //            }
        //            grid_GG.Visibility = Visibility.Visible;
        //            txt_GG.Text = "任务加入下载队列";
        //            list_E.SelectionMode = ListViewSelectionMode.None;
        //            list_E.IsItemClickEnabled = true;
        //            com_bar.Visibility = Visibility.Visible;
        //            com_bar_Down.Visibility = Visibility.Collapsed;
        //            await Task.Delay(2000);
        //            grid_GG.Visibility = Visibility.Collapsed;
        //        }
        //        else
        //        {
        //            list_E.SelectionMode = ListViewSelectionMode.None;
        //            list_E.IsItemClickEnabled = true;
        //            com_bar.Visibility = Visibility.Visible;
        //            com_bar_Down.Visibility = Visibility.Collapsed;
        //        }
        //    }
        //}

        private void btn_Down_Click(object sender, RoutedEventArgs e)
        {
            com_bar.Visibility = Visibility.Collapsed;
            com_bar_Down.Visibility = Visibility.Visible;
            list_E.SelectionMode = ListViewSelectionMode.Multiple;
            list_E.IsItemClickEnabled = false;

        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            com_bar.Visibility = Visibility.Visible;
            com_bar_Down.Visibility = Visibility.Collapsed;
            list_E.SelectionMode = ListViewSelectionMode.None;
            list_E.IsItemClickEnabled = true;
        }

        private void btn_More_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Desc.MaxLines == 3)
            {
                txt_Desc.MaxLines = 0;
                btn_More.Content = "收缩";
            }
            else
            {
                txt_Desc.MaxLines = 3;
                btn_More.Content = "展开";
            }
        }

        private void btn_More__Click(object sender, RoutedEventArgs e)
        {
            if (txt_CV.MaxLines == 3)
            {
                txt_CV.MaxLines = 0;
                btn_More.Content = "收缩";
            }
            else
            {
                txt_CV.MaxLines = 3;
                btn_More.Content = "展开";
            }
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
             GetBangumiInfo(banID);
        }

        private async void btn_concern_Click(object sender, RoutedEventArgs e)
        {
            UserClass getLogin = new UserClass();
            wc = new WebClientClass();
            if (getLogin.IsLogin())
            {
                try
                {
                    if (btn_concern.Label == "订阅")
                    {
                        //http://www.bilibili.com/api_proxy?app=bangumi&action=/concern_season&season_id=779
                        string results = await wc.GetResults(new Uri("http://www.bilibili.com/api_proxy?app=bangumi&action=/concern_season&season_id=" + banID));
                        JObject json = JObject.Parse(results);
                        if ((int)json["code"] == 0)
                        {
                            font_icon.Glyph = "\uE00B";
                            btn_concern.Label = "取消订阅";
                            messShow.Show("订阅成功!", 3000);
                            //MessageDialog md = new MessageDialog("订阅成功！");
                            //  await md.ShowAsync();

                        }
                        else
                        {
                            messShow.Show("订阅失败!", 3000);
                        }
                    }
                    else
                    {
                        //http://www.bilibili.com/api_proxy?app=bangumi&action=/concern_season&season_id=779

                        string results = await wc.GetResults(new Uri("http://www.bilibili.com/api_proxy?app=bangumi&action=/unconcern_season&season_id=" + banID));
                        JObject json = JObject.Parse(results);
                        if ((int)json["code"] == 0)
                        {
                            font_icon.Glyph = "\uE006";
                            btn_concern.Label = "订阅";
                            messShow.Show("取消订阅成功!", 3000);
                            
                        }
                        else
                        {
                            messShow.Show("取消订阅失败!", 3000);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageDialog md = new MessageDialog("订阅操作失败！");

                    await md.ShowAsync();
                }
            }
            else
            {
                MessageDialog md = new MessageDialog("先登录好伐", "(´・ω・`) ");
                await md.ShowAsync();
            }
        }

        private async Task<bool> GetIsConcern(string sid)
        {
            try
            {
                string results = await wc.GetResults(new Uri("http://www.bilibili.com/api_proxy?app=bangumi&action=/user_season_status&season_id=" + sid + new Random().Next(1, 9999)));

                JObject json = JObject.Parse(results);
                if ((int)json["result"]["attention"] == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText(string.Format("我正在BiliBili追{0},一起来看吧\r\n地址：http://bangumi.bilibili.com/anime/{1}", txt_Name.Text, banID));
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack); // 保存 DataPackage 对象到剪切板
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
            messShow.Show("已将内容复制到剪切板",3000);
        }
    }
}
