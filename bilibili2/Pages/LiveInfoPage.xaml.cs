using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using bilibili2.Class;
using Windows.UI.Popups;
using Windows.Web.Http;
using Newtonsoft.Json.Linq;
using Windows.System.Display;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveInfoPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public LiveInfoPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        WebClientClass wc;
        string rommID = string.Empty;
        bool LoadDanmu = true;
        DispatcherTimer time = new DispatcherTimer();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            GetSetting();
            grid_Error.Visibility = Visibility.Collapsed;
            pivot.SelectedIndex = 0;
            stack_Comment.Children.Clear();
            time.Interval = new TimeSpan(0, 0, 0, 0, 500);
            danmu.ClearDanmu();
            time.Tick += Time_Tick;
            Video_UP.DataContext = null;
            mediaElement.Source = null;
            rommID = e.Parameter as string;
            btn_Sign.IsEnabled = true;
            btn_OpenSendGift.IsEnabled = true;
            btn_OpenEmoji.IsEnabled = true;
            txt_Comment.IsEnabled = true;
            btn_SendComment.IsEnabled = true;
            GetRoomInfo(rommID);
            if (new UserClass().IsLogin())
            {
                GetUserInfo();
                GetSignInfo();
             
            }
            else
            {
                UserModel data = new UserModel()
                {
                    uname = "请登录",
                    silver = 0,
                    gold = 0,
                    face = "ms-appx:///Assets/other/NoAvatar.png"
                };
                txt_Sign.Text = "未登录";
                grid_UserInfo.DataContext = data;
                btn_Sign.IsEnabled = false;
                btn_OpenSendGift.IsEnabled = false;
                btn_OpenEmoji.IsEnabled = false;
                txt_Comment.IsEnabled = false;
                btn_SendComment.IsEnabled = false;
            }
            GetGiftTop(rommID);
            GetFansTop(rommID);
            
        }

        private async void Time_Tick(object sender, object e)
        {
            GetComment();
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                if (grid_Full.Visibility==Visibility.Visible)
                {
                    txt_Timer.Text = DateTime.Now.ToString("HH-mm");
                }
            });
        }

        private DisplayRequest dispRequest = null;
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (wc != null)
            {
                wc = null;
            }
            if (dispRequest != null)
            {
                // 用户暂停了视频，则不需要保持屏幕的点亮状态
                dispRequest.RequestRelease(); // 停用显示请求
                dispRequest = null;
            }
            time.Stop();
            if (hr!=null)
            {
                hr.EndHeart();
                hr = null;
            }
         
            stack_Comment.Children.Clear();
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

        private void btn_Live_Click(object sender, RoutedEventArgs e)
        {
            pivot.SelectedIndex = Convert.ToInt32((sender as Button).Tag);
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            btn_Live.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Info.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Weitou.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Fans.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });

            btn_Live.FontWeight = FontWeights.Normal;
            btn_Info.FontWeight = FontWeights.Normal;
            btn_Weitou.FontWeight = FontWeights.Normal;
            btn_Fans.FontWeight = FontWeights.Normal;

            switch (pivot.SelectedIndex)
            {
                case 0:
                    btn_Live.Foreground = new SolidColorBrush(Colors.White);
                    btn_Live.FontWeight = FontWeights.Bold;
                    break;
                case 1:
                    btn_Info.Foreground = new SolidColorBrush(Colors.White);
                    btn_Info.FontWeight = FontWeights.Bold;
                    break;
                case 2:
                    btn_Weitou.Foreground = new SolidColorBrush(Colors.White);
                    btn_Weitou.FontWeight = FontWeights.Bold;
                    break;
                case 3:
                    btn_Fans.Foreground = new SolidColorBrush(Colors.White);
                    btn_Fans.FontWeight = FontWeights.Bold;
                    break;
                default:
                    break;
            }
        }


        private async void GetRoomInfo(string room_id)
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/AppRoom/index?_device=wp&appkey={0}&build=411005&access_key={1}&platform=android&room_id={2}&ts={3}", ApiHelper._appKey, ApiHelper.access_key, room_id, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                LiveInfoModel model = JsonConvert.DeserializeObject<LiveInfoModel>(results);
                if (model.code == 0)
                {
                    LiveInfoModel info = JsonConvert.DeserializeObject<LiveInfoModel>(model.data.ToString());
                    Video_UP.DataContext = info;
                    LiveInfoModel meta = JsonConvert.DeserializeObject<LiveInfoModel>(info.meta.ToString());
                    List<LiveInfoModel> giftList = JsonConvert.DeserializeObject<List<LiveInfoModel>>(info.roomgifts.ToString());
                    foreach (var item in giftList)
                    {
                        LiveInfoModel coin_type = JsonConvert.DeserializeObject<LiveInfoModel>(item.coin_type.ToString());
                        item.silver = coin_type.silver;
                        item.gold = coin_type.gold;
                    }
                    gridview_Gifts.ItemsSource = giftList;
                    string b = @"<head><style>p{font-family:""微软雅黑"";}</style></head>";
                    web_Desc.NavigateToString(b+meta.description);
                    GetSliver();
                    if (info.is_attention == 1)
                    {
                        txt_guanzhu.Text = "已关注";
                    }
                    else
                    {
                        txt_guanzhu.Text = "关注";
                    }
                    if (info.status == "LIVE")
                    {
                        GetLiveUrl();
                       
                    }
                    else
                    {
                        grid_Error.Visibility = Visibility.Visible;
                        txt_ErrorInfo.Text = info.prepare ?? "主播正在换女装";
                    }
                }
                else
                {
                    grid_Error.Visibility = Visibility.Visible;
                    txt_ErrorInfo.Text = model.message;
                }
            }
            catch (Exception ex)
            {
                grid_Error.Visibility = Visibility.Visible;
                txt_ErrorInfo.Text = "读取错误" + ex.Message;
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }

        }

        private async void GetGiftTop(string room_id)
        {
            try
            {
                list_Gift_Top.Items.Clear();
                wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/AppRoom/getGiftTop?_device=wp&appkey={0}&build=411005&access_key={1}&platform=android&room_id={2}&ts={3}", ApiHelper._appKey, ApiHelper.access_key, room_id, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                LiveRankModel model = JsonConvert.DeserializeObject<LiveRankModel>(results);
                if (model.code == 0)
                {
                    //可以查看个人排名
                    LiveRankModel info = JsonConvert.DeserializeObject<LiveRankModel>(model.data.ToString());
                    List<LiveRankModel> rank = JsonConvert.DeserializeObject<List<LiveRankModel>>(info.list.ToString());
                    int i = 0;
                    foreach (var item in rank)
                    {
                        switch (i)
                        {
                            case 0:
                                item.PColor = new SolidColorBrush(Colors.OrangeRed);
                                break;
                            case 1:
                                item.PColor = new SolidColorBrush(Colors.LightBlue);
                                break;
                            case 2:
                                item.PColor = new SolidColorBrush(Colors.Orange);
                                break;
                            default:
                                break;
                        }
                        item.rank = i + 1;
                        list_Gift_Top.Items.Add(item);
                        i++;
                    }

                }
                else
                {
                    grid_Error.Visibility = Visibility.Visible;
                    txt_ErrorInfo.Text = model.message;
                }
            }
            catch (Exception)
            {

            }

        }

        private async void GetFansTop(string room_id)
        {
            try
            {
                list_Fans_Top.Items.Clear();
                wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/AppRoom/medalRankList?_device=wp&appkey={0}&build=411005&access_key={1}&platform=android&room_id={2}&ts={3}", ApiHelper._appKey, ApiHelper.access_key, room_id, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                LiveRankModel model = JsonConvert.DeserializeObject<LiveRankModel>(results);
                if (model.code == 0)
                {
                    //可以查看个人排名
                    LiveRankModel info = JsonConvert.DeserializeObject<LiveRankModel>(model.data.ToString());
                    List<LiveRankModel> rank = JsonConvert.DeserializeObject<List<LiveRankModel>>(info.list.ToString());
                    int i = 0;
                    foreach (var item in rank)
                    {
                        switch (i)
                        {
                            case 0:
                                item.PColor = new SolidColorBrush(Colors.OrangeRed);
                                break;
                            case 1:
                                item.PColor = new SolidColorBrush(Colors.LightBlue);
                                break;
                            case 2:
                                item.PColor = new SolidColorBrush(Colors.Orange);
                                break;
                            default:
                                break;
                        }
                        item.rank = i + 1;
                        list_Fans_Top.Items.Add(item);
                        i++;
                    }

                }
                else
                {
                    grid_Error.Visibility = Visibility.Visible;
                    txt_ErrorInfo.Text = model.message;
                }
            }
            catch (Exception)
            {

            }

        }

        private async void btn_AttUp_Click(object sender, RoutedEventArgs e)
        {
            if (!new UserClass().IsLogin())
            {
                await new MessageDialog("请先登录！").ShowAsync();
            }
            try
            {
                Uri ReUri = new Uri("http://live.bilibili.com/liveact/attention");
                int types;
                if (txt_guanzhu.Text == "关注")
                {
                    types = 1;
                }
                else
                {
                    types = 0;
                }
                HttpClient hc = new HttpClient();
                hc.DefaultRequestHeaders.Referer = new Uri("http://live.bilibili.com/");
                var response = await hc.PostAsync(ReUri, new HttpStringContent("uid=" + (Video_UP.DataContext as LiveInfoModel).mid + "&type=" + types, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(result);
                if ((int)json["code"] == 0)
                {
                    if (txt_guanzhu.Text == "关注")
                    {
                        txt_guanzhu.Text = "已关注";
                    }
                    else
                    {
                        txt_guanzhu.Text = "关注";
                    }
                }
                else
                {
                    await new MessageDialog("关注失败" + json["msg"]).ShowAsync();
                }

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "关注时发生错误").ShowAsync();
            }


        }

        private  async void GetSignInfo()
        {
            try
            {
                wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/sign/GetSignInfo?rnd={0}", new Random().Next(1, 9999));
                string results = await wc.GetResults(new Uri(url));
                SignModel model = JsonConvert.DeserializeObject<SignModel>(results);
                if (model.code == 0)
                {
                    SignModel data = JsonConvert.DeserializeObject<SignModel>(model.data.ToString());
                    if (data.status == 0)
                    {
                        btn_Sign.IsEnabled = true;
                        txt_Sign.Text = "签到";
                    }
                    else
                    {
                        txt_Sign.Text = "已签到";
                        btn_Sign.IsEnabled = false;
                    }
                }
                else
                {
                    await new MessageDialog(model.msg).ShowAsync();
                }
            }
            catch (Exception)
            {
            }
            
        }

        private async void UserSign()
        {
            try
            {
                wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/sign/doSign?rnd={0}", new Random().Next(1, 9999));
                string results = await wc.GetResults(new Uri(url));
                SignModel model = JsonConvert.DeserializeObject<SignModel>(results);
                if (model.code == 0)
                {
                    SignModel data = JsonConvert.DeserializeObject<SignModel>(model.data.ToString());
                    txt_Sign.Text = "已签到";
                    btn_Sign.IsEnabled = false;
                    await new MessageDialog(data.text).ShowAsync();
                    GetUserInfo();
                }
                else
                {
                    await new MessageDialog(model.msg).ShowAsync();
                }
            }
            catch (Exception)
            {
                await new MessageDialog("签到时发生错误").ShowAsync();
            }
          

        }

        private class SignModel
        {
            public int code { get; set; }
            public string msg { get; set; }
            public object data { get; set; }

            public string text { get; set; }
            public int status { get; set; }//1已签到，0未签到
            public int taskStatus { get; set; }

            //用于用户
            public string uname { get; set; }
            public string face { get; set; }
            public double silver { get; set; }
            public double gold { get; set; }
            public int vip { get; set; }//0为false,1为true
            public int svip { get; set; }//0为false,1为true
            public int user_level { get; set; }//现在等级
            public int user_next_level { get; set; }//下一等级
        }
        private class UserModel
        {
            public string code { get; set; }
            public string msg { get; set; }
            public object data { get; set; }

            public string uname { get; set; }
            public string face { get; set; }
            public double silver { get; set; }
            public double gold { get; set; }
            public int vip { get; set; }//0为false,1为true
            public int svip { get; set; }//0为false,1为true
            public int user_level { get; set; }//现在等级
            public int user_next_level { get; set; }//下一等级
        }
        private async void GetUserInfo()
        {
            try
            {
                wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/User/getUserInfo?rnd={0}", new Random().Next(1, 9999));
                string results = await wc.GetResults(new Uri(url));
                UserModel model = JsonConvert.DeserializeObject<UserModel>(results);
                if (model.code == "REPONSE_OK")
                {
                    UserModel data = JsonConvert.DeserializeObject<UserModel>(model.data.ToString());
                    grid_UserInfo.DataContext = data;
                }
                else
                {
                    await new MessageDialog(model.msg).ShowAsync();
                }
            }
            catch (Exception)
            {

            }
           

        }

        private async void GetLiveUrl()
        {
            try
            {
                wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://live.bilibili.com/api/playurl?platform=h5&cid=" + rommID + "&rnd=" + new Random().Next(1, 9999)));
                JObject json = JObject.Parse(results);
                mediaElement.Source = new Uri((string)json["data"]);
                time.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void mediaElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (media_Controls.Visibility == Visibility.Visible)
            {
                media_Controls.Visibility = Visibility.Collapsed;
                media_top_Control.Visibility = Visibility.Collapsed;
            }
            else
            {
                media_Controls.Visibility = Visibility.Visible;
                media_top_Control.Visibility = Visibility.Visible;
            }
        }

        private void mediaElement_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            pro_Num.Text = mediaElement.BufferingProgress.ToString("P");
        }

        private void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (mediaElement.CurrentState)
            {
                case MediaElementState.Closed:
                    btn_Play.Visibility = Visibility.Visible;
                    btn_Pause.Visibility = Visibility.Collapsed;
                    LoadDanmu = false;
                    break;
                case MediaElementState.Opening:
                    btn_Play.Visibility = Visibility.Visible;
                    btn_Pause.Visibility = Visibility.Collapsed;
                    progress.Visibility = Visibility.Visible;
                    LoadDanmu = false;
                    break;
                case MediaElementState.Buffering:
                    btn_Play.Visibility = Visibility.Collapsed;
                    btn_Pause.Visibility = Visibility.Visible;
                    progress.Visibility = Visibility.Visible;
                    LoadDanmu = false;
                    break;
                case MediaElementState.Playing:
                    btn_Play.Visibility = Visibility.Collapsed;
                    btn_Pause.Visibility = Visibility.Visible;
                    progress.Visibility = Visibility.Collapsed;
                    LoadDanmu = true;
                    break;
                case MediaElementState.Paused:
                    btn_Play.Visibility = Visibility.Visible;
                    btn_Pause.Visibility = Visibility.Collapsed;
                    progress.Visibility = Visibility.Collapsed;
                    LoadDanmu = false;
                    break;
                case MediaElementState.Stopped:
                    btn_Play.Visibility = Visibility.Visible;
                    btn_Pause.Visibility = Visibility.Collapsed;
                    progress.Visibility = Visibility.Collapsed;
                    LoadDanmu = false;
                    break;
                default:
                    break;
            }
        }

        private void grid_Error_Tapped(object sender, TappedRoutedEventArgs e)
        {
            grid_Error.Visibility = Visibility.Collapsed;
            GetRoomInfo(rommID);
            GetGiftTop(rommID);
            GetFansTop(rommID);
        }

        List<string> loaded = new List<string>();
        private async void GetComment()
        {
            try
            {
                //http://live.bilibili.com/AppRoom/msg?_device=android&_hwid=68fc5d795c256cd1&appkey=c1b107428d337928&build=414000&platform=android&room_id=23058&sign=4bf8088300d9f4c90b62264c4a87585d
                wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/AppRoom/msg?_device=wp&appkey={0}&build=411005&access_key={1}&platform=android&room_id={2}&ts={3}", ApiHelper._appKey, ApiHelper.access_key, rommID, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                Model models = JsonConvert.DeserializeObject<Model>(results);
                if (models.code == 0)
                {
                    Model data = JsonConvert.DeserializeObject<Model>(models.data.ToString());
                    List<Model> model = JsonConvert.DeserializeObject<List<Model>>(data.room.ToString());
                    foreach (var item in model)
                    {
                        if (!loaded.Contains(item.nickname + item.timeline + item.text))
                        {
                            string vip = string.Empty;
                            if (item.vip == 1)
                            {
                                vip = "[爷]";
                            }
                            if (item.svip == 1)
                            {
                                vip = "[大爷]";
                            }
                            AddComment(vip + item.nickname + ":" + item.text, false);
                            loaded.Add(item.nickname + item.timeline + item.text);
                            if (LoadDanmu&&!DanDis_Dis(item.text))
                            {
                                danmu.AddGunDanmu(new Controls.MyDanmaku.DanMuModel() { DanText = item.text, DanSize = "25", _DanColor = "16777215" }, false);
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {

            }


        }


        public class Model
        {
            public int code { get; set; }
            public string message { get; set; }
            public object data { get; set; }
            public object room { get; set; }

            public string text { get; set; }
            public string timeline { get; set; }
            public string nickname { get; set; }
            public string uid { get; set; }
            public int svip { get; set; }
            public int vip { get; set; }
        }

        private void AddComment(string content, bool Myself)
        {
            if (stack_Comment.Children.Count>100)
            {
                stack_Comment.Children.Clear();
            }
            TextBlock tx = new TextBlock();
            tx.Margin = new Thickness(5);
            tx.Text = content;
            if (Myself)
            {
                tx.Foreground = new SolidColorBrush(Colors.Blue);
            }
            tx.TextWrapping = TextWrapping.Wrap;
            tx.IsTextSelectionEnabled = true;
            stack_Comment.Children.Add(tx);
            sc.ScrollToVerticalOffset(sc.ScrollableHeight);
        }

       

        private void btn_OpenCloseDanmu_Click(object sender, RoutedEventArgs e)
        {
            if (danmu.Visibility == Visibility.Collapsed)
            {
                danmu.Visibility = Visibility.Visible;
                btn_OpenCloseDanmu.Foreground = new SolidColorBrush(Colors.White);
                LoadDanmu = true;
            }
            else
            {
                danmu.Visibility = Visibility.Collapsed;
                btn_OpenCloseDanmu.Foreground = new SolidColorBrush(Colors.Gray);
                LoadDanmu = false;
            }
        }

        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
        }

        private void btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.CanPause)
            {
                mediaElement.Pause();
            }
        }

        private void btn_SendComment_Click(object sender, RoutedEventArgs e)
        {
            SendDanmu();
        }

        public async void SendDanmu()
        {
            if (txt_Comment.Text.Length == 0)
            {
                AddComment("弹幕内容不能为空！", true);
                return;
            }
            try
            {
                WebClientClass wc = new WebClientClass();
                DateTime timeStamp = new DateTime(1970, 1, 1); //得到1970年的时间戳
                long time = (DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000000;
                string sendText = string.Format("color=16777215&fontsize=25&mode=1&msg={0}&rnd={1}&roomid={2}", txt_Comment.Text, time, rommID);
                string result = await wc.PostResults(new Uri("http://live.bilibili.com/msg/send"), sendText);
                JObject jb = JObject.Parse(result);
                if ((int)jb["code"] == 0)
                {
                    AddComment("我:" + txt_Comment.Text, true);
                    if (LoadDanmu)
                    {
                        danmu.AddGunDanmu(new Controls.MyDanmaku.DanMuModel() { DanText = txt_Comment.Text, DanSize = "25", _DanColor = "16777215" }, true);
                    }
                    txt_Comment.Text = string.Empty;
                    btn_SendComment.IsEnabled = false;
                    await Task.Delay(3000);
                    btn_SendComment.IsEnabled = true;
                }
                else
                {
                    AddComment("弹幕发送失败", true);
                }
            }
            catch (Exception)
            {
                AddComment("弹幕发送出现错误", true);
            }
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            txt_Comment.Text += ((Button)sender).Content.ToString();
        }

        private void btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            sp.IsPaneOpen = true;

        }
        SettingHelper setting = new SettingHelper();
        private void slider_DanmuJianju_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.SetSpacing(slider_DanmuJianju.Value);
            setting.SetSettingValue("DanmuJianju-Live", slider_DanmuJianju.Value);
        }
        private void slider_DanmuTran_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.Tran = slider_DanmuTran.Value / 100;
            setting.SetSettingValue("DanmuTran-Live", slider_DanmuTran.Value);
        }

        private void slider_DanmuSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.Speed = Convert.ToInt32(slider_DanmuSpeed.Value);
            setting.SetSettingValue("DanmuSpeed-Live", slider_DanmuSpeed.Value);

        }

        private void slider_DanmuSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.fontSize = slider_DanmuSize.Value;
            setting.SetSettingValue("DanmuSize-Live", slider_DanmuSize.Value);
        }

        private void cb_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string a = (cb_Font.SelectedItem as ComboBoxItem).Content.ToString();
            danmu.fontFamily = a;
            setting.SetSettingValue("FontFamily-Live", a);
        }
        //读取设置
        string device = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
        private void GetSetting()
        {
            DanDis_Get();
            if (dispRequest == null)
            {
                // 用户观看视频，需要保持屏幕的点亮状态
                dispRequest = new DisplayRequest();
                dispRequest.RequestActive(); // 激活显示请求
            }
            //DisplayInformation.AutoRotationPreferences = (DisplayOrientations)5;


            //弹幕字体
            if (setting.SettingContains("FontFamily-Live"))
            {
                switch ((string)setting.GetSettingValue("FontFamily-Live"))
                {
                    case "默认":
                        cb_Font.SelectedIndex = 0;
                        break;
                    case "雅黑":
                        cb_Font.SelectedIndex = 1;
                        break;
                    case "黑体":
                        cb_Font.SelectedIndex = 2;
                        break;
                    case "楷体":
                        cb_Font.SelectedIndex = 3;
                        break;
                    case "宋体":
                        cb_Font.SelectedIndex = 4;
                        break;
                    case "等线":
                        cb_Font.SelectedIndex = 5;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                setting.SetSettingValue("FontFamily-Live", "默认");
                cb_Font.SelectedIndex = 0;
            }


            if (setting.SettingContains("DanmuJianju-Live"))
            {
                slider_DanmuJianju.Value = (double)setting.GetSettingValue("DanmuJianju-Live");
            }
            else
            {
                slider_DanmuJianju.Value = 0;
            }

            if (setting.SettingContains("DanmuTran-Live"))
            {
                slider_DanmuTran.Value = (double)setting.GetSettingValue("DanmuTran-Live");
            }
            else
            {
                slider_DanmuTran.Value = 100;
            }

            if (setting.SettingContains("DanmuSpeed-Live"))
            {
                slider_DanmuSpeed.Value = (double)setting.GetSettingValue("DanmuSpeed-Live");
            }
            else
            {
                slider_DanmuSpeed.Value = 8;
            }
            if (setting.SettingContains("DanmuSize-Live"))
            {
                slider_DanmuSize.Value = (double)setting.GetSettingValue("DanmuSize-Live");
            }
            else
            {
                if (device == "Windows.Mobile")
                {
                    slider_DanmuSize.Value = 14;
                }
                else
                {
                    slider_DanmuSize.Value = 18;
                }
            }
        }

        private void btn_Full_Click(object sender, RoutedEventArgs e)
        {
            btn_Full.Visibility = Visibility.Collapsed;
            btn_ExitFull.Visibility = Visibility.Visible;
            //Video_UP.Height = 0;
            //grid_Info.Width = 0;
            //row_2.Height = GridLength.Auto;
            //column_2.Width = GridLength.Auto;
            UIElement en = sp;
            grid_Full.Visibility = Visibility.Visible;
            grid_NotFull.Children.Remove(sp);
            grid_Full.Children.Add(en);
            DisplayInformation.AutoRotationPreferences = (DisplayOrientations)5;
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            // 
        }

        private void btn_ExitFull_Click(object sender, RoutedEventArgs e)
        {
            btn_Full.Visibility = Visibility.Visible;
            btn_ExitFull.Visibility = Visibility.Collapsed;

            UIElement en = sp;
            grid_Full.Children.Remove(sp);
            grid_Full.Visibility = Visibility.Collapsed;
            grid_NotFull.Children.Add(en);

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            ApplicationView.GetForCurrentView().ExitFullScreenMode();

        }


        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            DanDis_Add(txt_Dis.Text, false);
            txt_Dis.Text = "";
            var s = danmu.GetScreenDanmu();
            foreach (var item in s)
            {
                if (DanDis_Dis(item.DanText))
                {
                    danmu.RemoveDanmu(item);
                }
            }
        }

        List<string> Guanjianzi = new List<string>();
        private void DanDis_Get()
        {
            if (setting.SettingContains("Guanjianzi-Live"))
            {
                string a = (string)setting.GetSettingValue("Guanjianzi-Live");
                txt_Dis.Text = a;
                if (a.Length != 0)
                {
                    Guanjianzi = a.Split('|').ToList();
                    Guanjianzi.Remove(string.Empty);
                }

            }
            else
            {
                setting.SetSettingValue("Guanjianzi-Live", string.Empty);

            }

        }
        private bool DanDis_Dis(string text)
        {
            var a = (from sb in Guanjianzi where text.Contains(sb) select sb).ToList();
            if (a.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void DanDis_Add(string text, bool IsYonghu)
        {
            string a = (string)setting.GetSettingValue("Guanjianzi-Live") + "|" + text;
            setting.SetSettingValue("Guanjianzi-Live", a);
            Guanjianzi.Add(text);
        }

        #region 瓜子领取相关
        GetSilverHelper hr;
        private void GetSliver()
        {
            hr = new GetSilverHelper();
            hr.StartHeart();
            hr.guazievent += Hr_guazievent;
        }
        bool CanGetSliver = false;
        private void Hr_guazievent(GetSilverHelper.heartdata data, string result)
        {
            btn_GetSliver.Foreground = new SolidColorBrush(Colors.Gold);
            CanGetSliver = true;
        }

        private async void btn_GetSliver_Click(object sender, RoutedEventArgs e)
        {
            if (CanGetSliver)
            {
                grid_Chptcha.Visibility = Visibility.Visible;
                img_Chptcha.Source = await hr.GetCaptcha();
            }

        }

        private async void btn_Yes_Click(object sender, RoutedEventArgs e)
        {
            grid_Chptcha.Visibility = Visibility.Collapsed;
            GetSilverHelper.getSilverresult resluts= await hr.GetSliver(txt_Chptcha.Text);
            if (resluts.code==0)
            {
                await new MessageDialog("领取成功").ShowAsync();
            }
            else
            {
                await new MessageDialog(resluts.msg).ShowAsync();
            }

            CanGetSliver = false;
            btn_GetSliver.Foreground = new SolidColorBrush(Colors.White);
            GetUserInfo();
        }


        #endregion

        private void btn_Sign_Click(object sender, RoutedEventArgs e)
        {
            UserSign();
        }

        private void gridview_Gifts_ItemClick(object sender, ItemClickEventArgs e)
        {
            LiveInfoModel model= (e.ClickedItem as LiveInfoModel);
            if (model.silver!=null&&model.silver!=string.Empty)
            {
                rb_Gold.Visibility = Visibility.Visible;
                rb_Sliver.Visibility = Visibility.Visible;
                rb_Sliver.IsChecked = true;
            }
            else
            {
                
                rb_Gold.Visibility = Visibility.Visible;
                rb_Sliver.Visibility = Visibility.Collapsed;
                rb_Gold.IsChecked = true;
            }
            txt_Num.Text = "1";
            grid_SendGift.DataContext = model;
            grid_SendGift.Visibility = Visibility.Visible;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //请求一直失败，不知道timestamp及token参数具体信息
                //POST giftId=1&roomid=5269&ruid=1998535&num=1&coinType=silver&Bag_id=0&timestamp=1461739448&rnd=1461739410&token=829848ca3f1a4aa620d2a4b54db59490a8149fe5
                LiveInfoModel model = (LiveInfoModel)grid_SendGift.DataContext;
                string coinType = "gold";
                if (rb_Gold.IsChecked.Value)
                {
                    coinType = "gold";
                }
                else
                {
                    coinType = "silver";
                }
                string postContent = string.Format(
                    "giftId={0}&roomid={1}&ruid={2}&num={3}&coinType={4}&Bag_id=0&rnd={5}",
                    model.id, rommID, (Video_UP.DataContext as LiveInfoModel).mid, int.Parse(txt_Num.Text), coinType, ApiHelper.GetTimeSpen);
                //postContent += "&token=" + ApiHelper.GetSign(postContent);
                HttpClient hc = new HttpClient();
                hc.DefaultRequestHeaders.Referer = new Uri("http://live.bilibili.com/");
                HttpResponseMessage hr = await hc.PostAsync(new Uri("http://live.bilibili.com/gift/send"), new HttpStringContent(postContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                hr.EnsureSuccessStatusCode();
                string results = await hr.Content.ReadAsStringAsync();
                SignModel reInfo = JsonConvert.DeserializeObject<SignModel>(results);
                if (reInfo.code == 0)
                {
                    AddComment("成功投送礼物！", true);
                    GetUserInfo();
                }
                else
                {
                    AddComment("礼物投送失败：" + reInfo.msg, true);
                }
            }
            catch (Exception)
            {
                AddComment("礼物投送发生错误", true);
            }
            finally
            {
                grid_SendGift.Visibility = Visibility.Collapsed;
            }
          
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            grid_SendGift.Visibility = Visibility.Collapsed;
        }

        private async void img_Chptcha_Tapped(object sender, TappedRoutedEventArgs e)
        {
            img_Chptcha.Source = await hr.GetCaptcha();
        }

    }
}
