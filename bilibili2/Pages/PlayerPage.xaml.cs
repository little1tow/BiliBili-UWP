using bilibili2.Class;
using bilibili2.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供
//bilibili2.Pages.PlayerPage
namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayerPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public PlayerPage()
        {
            this.InitializeComponent();
            CoreWindow.GetForCurrentThread().KeyDown += PlayerPage_KeyDown;
        }

        private void PlayerPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = true;
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Space:
                    if (btn_Pause.Visibility == Visibility.Visible)
                    {
                        mediaElement.Pause();
                    }
                    else
                    {
                        mediaElement.Play();
                    }
                    break;
                case Windows.System.VirtualKey.Left:
                   // mediaElement.Position.Subtract(new TimeSpan(0, 0, 3));
                    slider3.Value = slider.Value - 3;
                    messShow.Show(mediaElement.Position.Hours.ToString("00") + ":" + mediaElement.Position.Minutes.ToString("00") + ":" + mediaElement.Position.Seconds.ToString("00"), 3000);
                    break;
                case Windows.System.VirtualKey.Up:
                    mediaElement.Volume += 0.1;
                    messShow.Show("音量:"+mediaElement.Volume.ToString("P"), 3000);
                    break;
                case Windows.System.VirtualKey.Right:
                    slider3.Value = slider.Value + 3;
                    messShow.Show(mediaElement.Position.Hours.ToString("00") + ":" + mediaElement.Position.Minutes.ToString("00") + ":" + mediaElement.Position.Seconds.ToString("00"), 3000);
                    break;
                case Windows.System.VirtualKey.Down:
                    mediaElement.Volume-= 0.1;
                    messShow.Show("音量:" + mediaElement.Volume.ToString("P"), 3000);
                    break;
                case Windows.System.VirtualKey.Escape:
                    if (btn_Full.Visibility==Visibility.Collapsed)
                    {
                        setting.SetSettingValue("Full", false);
                        btn_Full.Visibility = Visibility.Visible;
                        btn_ExitFull.Visibility = Visibility.Collapsed;
                        ApplicationView.GetForCurrentView().ExitFullScreenMode();
                    }
                    break;
                case Windows.System.VirtualKey.F11:
                    if (btn_ExitFull.Visibility== Visibility.Collapsed)
                    {
                        setting.SetSettingValue("Full", true);
                        btn_Full.Visibility = Visibility.Collapsed;
                        btn_ExitFull.Visibility = Visibility.Visible;
                        ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    }
                    break;
                default:
                    break;
            }

         
        }

        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer datetimer = new DispatcherTimer();//用于更新时间
        SettingHelper setting = new SettingHelper();
        List<MyDanmaku.DanMuModel> DanMuPool = null;
        List<VideoModel> VideoList = new List<VideoModel>();//视频列表
        string device = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
        int PlayP = 0;//播放第几P
        bool LoadDanmu = true;
        private bool Is = false;
        string Cid = string.Empty;
        string Aid = string.Empty;
        private DisplayRequest dispRequest = null;//保持屏幕常亮
        SqlHelper sql = new SqlHelper();
       
        long LastPost = 0;
        bool lastPostVIs = false;
        protected override async  void OnNavigatedTo(NavigationEventArgs e)
        {
            sql.CreateTable();
            datetimer.Interval = new TimeSpan(0, 0, 1);
            datetimer.Tick += Datetimer_Tick;
            datetimer.Start();
            VideoList = ((KeyValuePair<List<VideoModel>, int>)e.Parameter).Key;
            PlayP= ((KeyValuePair<List<VideoModel>, int>)e.Parameter).Value;
            if (VideoList.Count<=1)
            {
                btn_Menu.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_Menu.Visibility = Visibility.Visible;
                gridview_List.ItemsSource = VideoList;
                gridview_List.SelectedIndex = PlayP;
            }
            GetSetting();
           
            await PlayVideo(VideoList[PlayP]);
            Is = true;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();
            mediaElement.Play();
        }

        private async Task PlayVideo(VideoModel model)
        {
            if (model.path!=null)
            {
               PlayLocalOld(model);
                return;
            }
            progress.Visibility = Visibility.Visible;
            top_Title.Text = model.title + " " + model.page;
            pro_Num.Text = "填充弹幕中...";
            Cid = model.cid;
            Aid = model.aid;
            if (sql.ValuesExists(Cid))
            {
                menu_LastPost.IsEnabled = true;
                LastPost = sql.QueryValue(Cid);
            }
            else
            {
                menu_LastPost.IsEnabled = false;
                sql.InsertValue(Cid);
            }
            lastPostVIs = false;
            DanMuPool = await GetDM(model.cid,false,false,string.Empty);
            pro_Num.Text = "读取视频信息...";
            await GetPlayInfo(model.cid, top_cb_Quality.SelectedIndex+1);
        }

        private async void PlayLocalOld(VideoModel model)
        {
            try
            {
                Cid = model.cid;
                if (sql.ValuesExists(Cid))
                {
                    menu_LastPost.IsEnabled = true;
                    LastPost = sql.QueryValue(Cid);
                }
                else
                {
                    menu_LastPost.IsEnabled = false;
                    sql.InsertValue(Cid);
                }
                lastPostVIs = false;
                top_Title.Text = model.title;
                StorageFile file = await StorageFile.GetFileFromPathAsync(model.path);
                
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mediaElement.SetSource(stream, file.ContentType);
                if (model.IsOld==true)
                {
                    DanMuPool = await GetDM(model.cid, true,true,string.Empty);
                }
                else
                {
                    DanMuPool = await GetDM(model.cid, true,false, (await file.GetParentAsync()).Path);
                }
                Send_text_Comment.PlaceholderText = "Sorry，本地视频暂不支持发送弹幕...";
                Send_text_Comment.IsEnabled = false;
                Send_btn_Send.IsEnabled = false;
                top_cb_Quality.Visibility =  Visibility.Collapsed;
                //btn_EPList.IsEnabled = false;
                //GetPlayhistory(model.path);
            }
            catch (Exception)
            {
                messShow.Show("读取本地视频失败！", 3000);
            }
        }
      
        private async void Datetimer_Tick(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                top_txt_Time.Text = DateTime.Now.ToLocalTime().ToString("HH:mm");
                switch (CheckNetworkHelper.CheckInternetConnectionType())
                {
                    case InternetConnectionType.WwanConnection:
                        top_txt_NetType.Text = "移动数据";
                        top_txt_NetType.Foreground = new SolidColorBrush(Colors.OrangeRed);
                        break;
                    case InternetConnectionType.WlanConnection:
                        top_txt_NetType.Text = "WIFI";
                        top_txt_NetType.Foreground = new SolidColorBrush(Colors.White);
                        break;
                    default:
                        break;
                }
             
            });
         
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //取消屏幕常亮
            if (dispRequest != null)
            {
                dispRequest = null;
            }
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            timer.Stop();
            datetimer.Stop();
        }
        //读取设置
        private void GetSetting()
        {

            DanDis_Get();

            if (dispRequest == null)
            {
                // 用户观看视频，需要保持屏幕的点亮状态
                dispRequest = new DisplayRequest();
                dispRequest.RequestActive(); // 激活显示请求
            }
            DisplayInformation.AutoRotationPreferences =(DisplayOrientations)5;

            if (setting.SettingContains("Full"))
            {
                if ((bool)setting.GetSettingValue("Full"))
                {
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    btn_Full.Visibility = Visibility.Collapsed;
                    btn_ExitFull.Visibility = Visibility.Visible;
                    tw_AutoFull.IsOn = true;
                }
                else
                {
                    btn_Full.Visibility = Visibility.Visible;
                    btn_ExitFull.Visibility = Visibility.Collapsed;
                    tw_AutoFull.IsOn = false;
                }
            }
            else
            {
                if (device == "Windows.Mobile")
                {
                    setting.SetSettingValue("Full", true);
                    btn_Full.Visibility = Visibility.Collapsed;
                    btn_ExitFull.Visibility = Visibility.Visible;
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    tw_AutoFull.IsOn = true;
                }
                else
                {
                    setting.SetSettingValue("Full", false);
                    btn_Full.Visibility = Visibility.Visible;
                    btn_ExitFull.Visibility = Visibility.Collapsed;
                    tw_AutoFull.IsOn = false;
                }
            }

            //弹幕字体
            if (setting.SettingContains("FontFamily"))
            {
                switch ((string)setting.GetSettingValue("FontFamily"))
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
                setting.SetSettingValue("FontFamily", "默认");
                cb_Font.SelectedIndex = 0;
            }

            if (setting.SettingContains("Quality"))
            {
                top_cb_Quality.SelectedIndex = (int)setting.GetSettingValue("Quality") ;
            }
            else
            {
                top_cb_Quality.SelectedIndex = 1;
            }

            if (setting.SettingContains("PlayerLight"))
            {
                slider_L.Value = (double)setting.GetSettingValue("PlayerLight");
            }
            else
            {
                slider_L.Value = 1;
            }
            if (setting.SettingContains("PlayerVolume"))
            {
                slider_V.Value = (double)setting.GetSettingValue("PlayerVolume");
            }
            else
            {
                slider_V.Value = 1;
            }

            if (setting.SettingContains("DanVisRoll"))
            {
                if ((bool)setting.GetSettingValue("DanVisRoll"))
                {
                    menu_setting_gd.IsChecked = false;
                }
                else
                {
                    menu_setting_gd.IsChecked = true;
                }
            }
            else
            {
                menu_setting_gd.IsChecked= false;
            }

            if (setting.SettingContains("DanVisTop"))
            {
                if ((bool)setting.GetSettingValue("DanVisTop"))
                {
                    menu_setting_top.IsChecked = false;
                }
                else
                {
                    menu_setting_top.IsChecked = true;
                }
            }
            else
            {
                menu_setting_top.IsChecked = false;
            }

            if (setting.SettingContains("DanVisButtom"))
            {
                if ((bool)setting.GetSettingValue("DanVisButtom"))
                {
                    menu_setting_buttom.IsChecked = false;
                }
                else
                {
                    menu_setting_buttom.IsChecked = true;
                }
            }
            else
            {
                menu_setting_buttom.IsChecked = false;
            }


            if (setting.SettingContains("OpenDanmu"))
            {
                if (!(bool)setting.GetSettingValue("OpenDanmu"))
                {
                    danmu.Visibility = Visibility.Collapsed;
                    btn_Open_CloseDanmu.Foreground = new SolidColorBrush(Colors.Gray);
                    LoadDanmu = false;
                    setting.SetSettingValue("OpenDanmu", false);
                }
            }
            else
            {
                danmu.Visibility = Visibility.Visible;
                btn_Open_CloseDanmu.Foreground = new SolidColorBrush(Colors.White);
                LoadDanmu = true;
                setting.SetSettingValue("OpenDanmu", true);
            }

            if (setting.SettingContains("DanmuJianju"))
            {
                slider_DanmuJianju.Value = (double)setting.GetSettingValue("DanmuJianju");
            }
            else
            {
                slider_DanmuJianju.Value = 0;
            }

            if (setting.SettingContains("DanmuTran"))
            {
                slider_DanmuTran.Value = (double)setting.GetSettingValue("DanmuTran");
            }
            else
            {
                slider_DanmuTran.Value = 100;
            }
           
            if (setting.SettingContains("DanmuSpeed"))
            {
                slider_DanmuSpeed.Value =(double)setting.GetSettingValue("DanmuSpeed");
            }
            else
            {
                slider_DanmuSpeed.Value = 12;
            }
            if (setting.SettingContains("DanmuSize"))
            {
                slider_DanmuSize.Value = (double)setting.GetSettingValue("DanmuSize");
            }
            else
            {
                if (device == "Windows.Mobile")
                {
                    slider_DanmuSize.Value = 16;
                }
                else
                {
                    slider_DanmuSize.Value = 22;
                }
            }
        }

        public async Task GetPlayInfo(string mid, int quality)
        {
            try
            {
                pro_Num.Text = "读取视频地址...";
                WebClientClass wc = new WebClientClass();
                string url = "http://interface.bilibili.com/playurl?platform=android&cid=" + mid + "&quality=" + quality + "&otype=json&appkey=422fd9d7289a1dd9&type=mp4";
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                VideoUriModel model = JsonConvert.DeserializeObject<VideoUriModel>(results);
                List<VideoUriModel> model1 = JsonConvert.DeserializeObject<List<VideoUriModel>>(model.durl.ToString());
                mediaElement.Source = new Uri(model1[0].url);
                pro_Num.Text = "开始缓冲视频...";
            }
            catch (Exception)
            {
                MessageDialog md = new MessageDialog("视频地址获取失败！", "错误");
                await md.ShowAsync();
            }
        }

        public async Task<List<MyDanmaku.DanMuModel>> GetDM(string cid,bool IsLocal,bool IsOld,string path)
        {
            List<MyDanmaku.DanMuModel> ls = new List<MyDanmaku.DanMuModel>();
            try
            {

                string a = string.Empty;
                if (!IsLocal)
                {
                   a=  await new WebClientClass().GetResults(new Uri("http://comment.bilibili.com/" + cid + ".xml"));
                }
                else
                {
                    if (IsOld)
                    {
                        StorageFolder folder = ApplicationData.Current.LocalFolder;
                        StorageFolder DownFolder = await folder.GetFolderAsync("DownLoad");
                        StorageFolder DownFolder2 = await KnownFolders.VideosLibrary.GetFolderAsync("Bili-Download");
                        StorageFile file = await DownFolder.CreateFileAsync(cid + ".xml", CreationCollisionOption.OpenIfExists);
                        StorageFile file2 = await DownFolder2.CreateFileAsync(cid + ".xml", CreationCollisionOption.OpenIfExists);
                        string filesOne = await FileIO.ReadTextAsync(file);
                        string filesTwo = await FileIO.ReadTextAsync(file2);
                        if (filesOne.Length != 0)
                        {
                            a = filesOne;
                        }
                        else
                        {
                            a = filesTwo;
                        }
                    }
                    else
                    {
                        //StorageFolder folder = ApplicationData.Current.LocalFolder;
                        StorageFolder DownFolder = await StorageFolder.GetFolderFromPathAsync(path);
                        //StorageFolder DownFolder2 = await KnownFolders.VideosLibrary.GetFolderAsync("Bili-Download");
                        StorageFile file = await DownFolder.CreateFileAsync(cid + ".xml", CreationCollisionOption.OpenIfExists);
                        
                        string files = await FileIO.ReadTextAsync(file);
                        a = files;
                    }
                }
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(a);
                XmlElement el = xdoc.DocumentElement;
                XmlNodeList xml = el.ChildNodes;
                foreach (XmlNode item in xml)
                {
                    if (item.Attributes["p"] != null)
                    {
                        string heheda = item.Attributes["p"].Value;
                        string[] haha = heheda.Split(',');
                        ls.Add(new MyDanmaku.DanMuModel
                        {
                            DanTime = decimal.Parse(haha[0]),
                            DanMode = haha[1],
                            DanSize = haha[2],
                            _DanColor = haha[3],
                            DanSendTime = haha[4],
                            DanPool = haha[5],
                            DanID = haha[6],
                            DanRowID = haha[7],
                            DanText = item.InnerText
                        });
                    }
                }
                return ls;
            }
            catch (Exception)
            {
                return ls;
            }

        }

        private async void Timer_Tick(object sender, object e)
        {
           await  this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
               slider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
               slider3.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
               slider.Value = mediaElement.Position.TotalSeconds;
               txt_Post.Text = mediaElement.Position.Hours.ToString("00") + ":" + mediaElement.Position.Minutes.ToString("00") + ":" + mediaElement.Position.Seconds.ToString("00") + "/" + mediaElement.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Seconds.ToString("00");
               sql.UpdateValue(Cid,Convert.ToInt32(mediaElement.Position.TotalSeconds));
           });
            if (mediaElement.CurrentState== MediaElementState.Playing&& LoadDanmu)
            {
                if (DanMuPool != null)
                {
                    foreach (var item in DanMuPool)
                    {
                        if (!DanDis_Dis(item.DanText))
                        {
                            if (Convert.ToInt32(item.DanTime) == Convert.ToInt32(mediaElement.Position.TotalSeconds))
                            {
                                if (item.DanMode == "5")
                                {
                                    danmu.AddTopButtomDanmu(item, true, false);
                                }
                                else
                                {
                                    if (item.DanMode == "4")
                                    {
                                        danmu.AddTopButtomDanmu(item, false, false);
                                    }
                                    else
                                    {
                                        danmu.AddGunDanmu(item, false);
                                    }
                                }

                            }
                        }
                        
                    }
                }


            }
        }

        private void top_btn_Back_Click(object sender, RoutedEventArgs e)
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

        #region 手势
        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            mediaElement.Pause();
            //progress.Visibility = Visibility.Visible;
            double X = e.Delta.Translation.X;
            if (X>0)
            {
                double dd = X/ this.ActualWidth;
                double d = dd* slider.Maximum;
                slider.Value += d;
            }
            else
            {
                double dd = Math.Abs(X) / this.ActualWidth;
                double d = dd * slider.Maximum;
                slider.Value-= d;
            }
            TimeSpan ts = new TimeSpan(0,0,Convert.ToInt32(slider.Value));
            txt_Post.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/" + mediaElement.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Seconds.ToString("00");
            messShow.Show(mediaElement.Position.Hours.ToString("00") + ":" + mediaElement.Position.Minutes.ToString("00") + ":" + mediaElement.Position.Seconds.ToString("00"), 3000);
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            mediaElement.Play();
            
            double X = e.Cumulative.Translation.X;
        }

        private void ss_Light_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            double Y = e.Delta.Translation.Y;
            if (Y > 0)
            {
                double dd = Y / ss_Light.ActualHeight;
                double d = dd * slider_L.Maximum;
                slider_L.Value -= d;
            }
            else
            {
                double dd = Math.Abs(Y) /ss_Light.ActualHeight;
                double d = dd * slider_L.Maximum;
                slider_L.Value += d;
            }
        }

        private void ss_Light_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ss_Volume_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            double Y = e.Delta.Translation.Y;
            if (Y > 0)
            {
                double dd = Y / ss_Volume.ActualHeight;
                double d = dd * slider_V.Maximum;
                slider_V.Value -= d;
            }
            else
            {
                double dd = Math.Abs(Y) / ss_Volume.ActualHeight;
                double d = dd * slider_V.Maximum;
                slider_V.Value += d;
            }
            messShow.Show("音量:" + mediaElement.Volume.ToString("P"), 3000);
        }
        #endregion

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                danmu.ClearDanmu();
                mediaElement.Position = new TimeSpan(0, 0, Convert.ToInt32(slider.Value));
                
            }
               // mediaElement.Position = new TimeSpan(0, 0, Convert.ToInt32(slider.Value));
        }
        private void slider3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.ClearDanmu();
            TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(slider3.Value));
            txt_Post.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/" + mediaElement.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Seconds.ToString("00");
            if (mediaElement.CurrentState== MediaElementState.Playing)
            {
                mediaElement.Pause();
                slider.Value = slider3.Value;
                mediaElement.Position = new TimeSpan(0, 0, Convert.ToInt32(slider.Value));
                mediaElement.Play();
            }
            else
            {
                slider.Value = slider3.Value;
                mediaElement.Position = new TimeSpan(0, 0, Convert.ToInt32(slider.Value));
            }
            
        }

        /**播放器**/
        private void mediaElement_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            pro_Num.Text = mediaElement.BufferingProgress.ToString("P");
        }

        private async void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            danmu.state = mediaElement.CurrentState;
            switch (mediaElement.CurrentState)
            {
                case MediaElementState.Closed:
                    btn_Play.Visibility = Visibility.Visible;
                    btn_Pause.Visibility = Visibility.Collapsed;
                   
                    break;
                case MediaElementState.Opening:
                    danmu.IsPlaying = false;
                    btn_Play.Visibility = Visibility.Visible;
                    btn_Pause.Visibility = Visibility.Collapsed;
                    progress.Visibility = Visibility.Visible;

                    break;
                case MediaElementState.Buffering:
                    btn_Play.Visibility = Visibility.Collapsed;
                    btn_Pause.Visibility = Visibility.Visible;
                    danmu.IsPlaying = false;
                    progress.Visibility = Visibility.Visible;

                    break;
                case MediaElementState.Playing:
                  

                    btn_Play.Visibility = Visibility.Collapsed;
                    btn_Pause.Visibility = Visibility.Visible;
                    danmu.IsPlaying = true;
                    progress.Visibility = Visibility.Collapsed;
                    timer.Start();
                    if (!lastPostVIs&& LastPost != 0)
                    {
                        TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(LastPost));
                        txt_LastPo.Text = "上次播放到" + ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                        btn_LastPost.Visibility = Visibility.Visible;
                        lastPostVIs = true;
                        await Task.Delay(3000);
                        btn_LastPost.Visibility = Visibility.Collapsed;
                    }
                    
                    break;
                case MediaElementState.Paused:
                    btn_Play.Visibility = Visibility.Visible;
                    btn_Pause.Visibility = Visibility.Collapsed;
                    danmu.IsPlaying = false;
                    progress.Visibility = Visibility.Collapsed;
                    timer.Stop();
                    
                    break;
                case MediaElementState.Stopped:
                    btn_Play.Visibility = Visibility.Visible;
                    btn_Pause.Visibility = Visibility.Collapsed;
                    danmu.IsPlaying = false;
                    progress.Visibility = Visibility.Collapsed;
                    timer.Stop();
                   
                    break;
                default:
                    break;
            }
        }

        private void mediaElement_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {

              slider2.Value = mediaElement.DownloadProgress * 100;

        }

        private async void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (cb_setting_1.IsChecked.Value)
            {
                mediaElement.Play();
                danmu.ClearDanmu();
                return;
            }
            grid_Top.Visibility = Visibility.Visible;
            grid_SendDanmu.Visibility = Visibility.Visible;
            grid_PlayInfo.Visibility = Visibility.Visible;
            if (gridview_List.SelectedIndex== gridview_List.Items.Count-1)
            {
                if (cb_setting_2.IsChecked.Value)
                {
                    gridview_List.SelectedIndex = 0;
                }
                else
                {
                    grid_end.Visibility = Visibility.Visible;
                }
            }
            else
            {
                grid_next.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                grid_next.Visibility = Visibility.Collapsed;
                gridview_List.SelectedIndex += 1;
            }

        }

        private void ss_Volume_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (grid_Top.Visibility==Visibility.Visible)
            {
                grid_Top.Visibility = Visibility.Collapsed;
                grid_SendDanmu.Visibility = Visibility.Collapsed;
                grid_PlayInfo.Visibility = Visibility.Collapsed;
            }
            else
            {
                grid_Top.Visibility = Visibility.Visible;
                grid_SendDanmu.Visibility = Visibility.Visible;
                grid_PlayInfo.Visibility = Visibility.Visible;
            }

        }

        private void ss_Volume_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (mediaElement.CurrentState == MediaElementState.Playing)
            {
                mediaElement.Pause();
            }
            else
            {
                mediaElement.Play();
            }
        }

        private void Send_btn_Send_Click(object sender, RoutedEventArgs e)
        {
            SenDanmuKa();
            //danmu.AddGunDanmu(new MyDanmaku.DanMuModel { DanText = Send_text_Comment.Text, _DanColor = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), DanSize = "25" },true);
        }
        /// <summary>
        /// 发送弹幕
        /// </summary>
        public async void SenDanmuKa()
        {
            if (Send_text_Comment.Text.Length == 0)
            {
                messShow.Show("弹幕内容不能为空!",3000);
                return;
            }
            if (!new UserClass().IsLogin())
            {
                return;
            }
            try
            {
                Uri ReUri = new Uri("http://interface.bilibili.com/dmpost?cid=" + Cid + "&aid=" + Aid + "&pid=1");
                int modeInt = 1;
                if (Send_cb_Mode.SelectedIndex == 2)
                {
                    modeInt = 4;
                }
                if (Send_cb_Mode.SelectedIndex == 1)
                {
                    modeInt = 5;
                }
                string Canshu = "message=" + Send_text_Comment.Text + "&pool=0&playTime=" + mediaElement.Position.TotalSeconds.ToString() + "&cid=" + Cid + "&date=" + DateTime.Now.ToString() + "&fontsize=25&mode=" + modeInt + "&rnd=933253860&color=" + ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag;
                string result = await new WebClientClass().PostResults(ReUri, Canshu);
                long code = long.Parse(result);

                if (code < 0)
                {
                    messShow.Show("已发送弹幕！"+result, 3000);
                }
                else
                {
                    messShow.Show("已发送弹幕！",3000);
                    if (modeInt == 1)
                    {
                        danmu.AddGunDanmu(new MyDanmaku.DanMuModel { DanText = Send_text_Comment.Text, _DanColor = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), DanSize = "25" }, true);
                    }
                    if (modeInt == 4)
                    {
                        danmu.AddTopButtomDanmu(new MyDanmaku.DanMuModel { DanText = Send_text_Comment.Text, _DanColor = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), DanSize = "25" }, false, true);
                    }
                    if (modeInt == 5)
                    {
                        danmu.AddTopButtomDanmu(new MyDanmaku.DanMuModel { DanText = Send_text_Comment.Text, _DanColor = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), DanSize = "25" }, true, true);
                    }
                    Send_text_Comment.Text = string.Empty;
                }

            }
            catch (Exception ex)
            {
                messShow.Show( "发送弹幕发生错误！" + ex.Message, 3000);
            }
        }


        private void slider_L_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setting.SetSettingValue("PlayerLight",slider_L.Value);
            double a = (1 - slider_L.Value) * 100;
            bor_Back.Background = new SolidColorBrush(new Color() { R = 0, G = 0, B = 0, A = Convert.ToByte(a) });
        }

        private void slider_V_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            setting.SetSettingValue("PlayerVolume", slider_V.Value);
        }

        #region 常用按钮操作
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
        private void btn_Full_Click(object sender, RoutedEventArgs e)
        {
            setting.SetSettingValue("Full", true);
            btn_Full.Visibility = Visibility.Collapsed;
            btn_ExitFull.Visibility = Visibility.Visible;
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        private void btn_ExitFull_Click(object sender, RoutedEventArgs e)
        {
            setting.SetSettingValue("Full", false);
            btn_Full.Visibility = Visibility.Visible;
            btn_ExitFull.Visibility = Visibility.Collapsed;
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
        }

        private void btn_Open_CloseDanmu_Click(object sender, RoutedEventArgs e)
        {
            if (danmu.Visibility == Visibility.Collapsed)
            {
                danmu.Visibility = Visibility.Visible;
                btn_Open_CloseDanmu.Foreground = new SolidColorBrush(Colors.White);
                LoadDanmu = true;
                setting.SetSettingValue("OpenDanmu",true);
            }
            else
            {
                danmu.Visibility = Visibility.Collapsed;
                btn_Open_CloseDanmu.Foreground = new SolidColorBrush(Colors.Gray);
                LoadDanmu = false;
                setting.SetSettingValue("OpenDanmu", false);
            }
        }

        private void menu_Play_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = true;
            grid_setting_Play.Visibility = Visibility.Visible;
            grid_setting_SelectVideo.Visibility = Visibility.Collapsed;
            grid_setting_Danmu.Visibility = Visibility.Collapsed;
            grid_setting_DanmuDis.Visibility = Visibility.Collapsed;
        }
        private void menu_Danmu_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = true;
            grid_setting_Play.Visibility = Visibility.Collapsed;
            grid_setting_SelectVideo.Visibility = Visibility.Collapsed;
            grid_setting_Danmu.Visibility = Visibility.Visible;
            grid_setting_DanmuDis.Visibility = Visibility.Collapsed;
        }

        private void menu_DisDanmu_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
            splitView.IsPaneOpen = true;
            grid_setting_Play.Visibility = Visibility.Collapsed;
            grid_setting_SelectVideo.Visibility = Visibility.Collapsed;
            grid_setting_Danmu.Visibility = Visibility.Collapsed;
            grid_setting_DanmuDis.Visibility = Visibility.Visible;
            foreach (var item in danmu.GetScreenDanmu())
            {
                list_DisDanmu.Items.Add(item);
            }
            //list_DisDanmu.ItemsSource=  danmu.GetScreenDanmu();

        }
        private void btn_Menu_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = true;
            grid_setting_Play.Visibility = Visibility.Collapsed;
            grid_setting_SelectVideo.Visibility = Visibility.Visible;
            grid_setting_Danmu.Visibility = Visibility.Collapsed;
            grid_setting_DanmuDis.Visibility = Visibility.Collapsed;
        }

        private async void menu_About_Click(object sender, RoutedEventArgs e)
        {
            int count=0;
            if (DanMuPool!=null)
            {
                count = DanMuPool.Count;
            }
            string message = "视频标题：" + VideoList[PlayP].title + "\r\n分P数量：" + VideoList.Count + "\r\n弹幕池数量：" + count + "\r\n视频宽度：" + mediaElement.NaturalVideoWidth + "\r\n视频高度：" + mediaElement.NaturalVideoHeight + "\r\n视频长度：" + mediaElement.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.NaturalDuration.TimeSpan.Seconds.ToString("00") + "\r\n缓冲进度：" + mediaElement.DownloadProgress.ToString("P");
            await new MessageDialog(message, "视频信息").ShowAsync();
        }

        #endregion
        #region 设置
        private void slider_DanmuJianju_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.SetSpacing(slider_DanmuJianju.Value);
            setting.SetSettingValue("DanmuJianju", slider_DanmuJianju.Value);
        }
        private void tw_AutoFull_Toggled(object sender, RoutedEventArgs e)
        {
            if (tw_AutoFull.IsOn)
            {
                setting.SetSettingValue("Full", true);
            }
            else
            {
                setting.SetSettingValue("Full", false);
            }
        }
        private void slider_DanmuTran_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.Tran= slider_DanmuTran.Value/100;
            setting.SetSettingValue("DanmuTran", slider_DanmuTran.Value);
        }

        private void slider_DanmuSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.Speed = Convert.ToInt32(slider_DanmuSpeed.Value);
            setting.SetSettingValue("DanmuSpeed", slider_DanmuSpeed.Value);

        }

        private void slider_DanmuSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            danmu.fontSize = slider_DanmuSize.Value;
            setting.SetSettingValue("DanmuSize", slider_DanmuSize.Value);
        }

        private void menu_setting_top_Click(object sender, RoutedEventArgs e)
        {

                danmu.SetDanmuVisibility(false, MyDanmaku.DanmuMode.Top);
                setting.SetSettingValue("DanVisTop", false);

        }

        private void menu_setting_buttom_Click(object sender, RoutedEventArgs e)
        {
                danmu.SetDanmuVisibility(false, MyDanmaku.DanmuMode.Buttom);
                setting.SetSettingValue("DanVisButtom", false);
        }

        private void menu_setting_gd_Checked(object sender, RoutedEventArgs e)
        {
                danmu.SetDanmuVisibility(false, MyDanmaku.DanmuMode.Roll);
                setting.SetSettingValue("DanVisRoll", false);
        }

        private void menu_setting_gd_Unchecked(object sender, RoutedEventArgs e)
        {
            danmu.SetDanmuVisibility(true, MyDanmaku.DanmuMode.Roll);
            setting.SetSettingValue("DanVisRoll", true);
        }

        private void menu_setting_top_Unchecked(object sender, RoutedEventArgs e)
        {
            danmu.SetDanmuVisibility(true, MyDanmaku.DanmuMode.Top);
            setting.SetSettingValue("DanVisTop", true);
        }

        private void menu_setting_buttom_Unchecked(object sender, RoutedEventArgs e)
        {
            danmu.SetDanmuVisibility(true, MyDanmaku.DanmuMode.Buttom);
            setting.SetSettingValue("DanVisButtom", true);
        }

        private void cb_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string a = (cb_Font.SelectedItem as ComboBoxItem).Content.ToString();
            danmu.fontFamily = a;
            setting.SetSettingValue("FontFamily", a);
        }
        #endregion

        #region 选集操作
        private async void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Is)
            {
                PlayP = gridview_List.SelectedIndex;
                await PlayVideo(VideoList[PlayP]);
            }
        }

        private async void top_cb_Quality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Is)
            {
                await PlayVideo(VideoList[PlayP]);
            }
            setting.SetSettingValue("Quality", top_cb_Quality.SelectedIndex);
        }

        #endregion

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            DanDis_Add(txt_Dis.Text, false);
            txt_Dis.Text = "";
            var s= danmu.GetScreenDanmu();
            foreach (var item in s)
            {
                if (DanDis_Dis(item.DanText))
                {
                    danmu.RemoveDanmu(item);
                }
            }
          
        }
        /// <summary>
        /// 弹幕屏蔽
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Dis_Remove_Click(object sender, RoutedEventArgs e)
        {
            foreach (MyDanmaku.DanMuModel item in list_DisDanmu.SelectedItems)
            {
                DanDis_Add(item.DanID, true);
                danmu.RemoveDanmu(item);
                list_DisDanmu.Items.Remove(item);
              
            }
        }
        #region 弹幕屏蔽
        List<string> Guanjianzi = new List<string>();
        List<string> Yonghu = new List<string>();
        private void DanDis_Get()
        {
            if (setting.SettingContains("Guanjianzi")&& setting.SettingContains("Yonghu"))
            {
                string a = (string)setting.GetSettingValue("Guanjianzi");
                string b = (string)setting.GetSettingValue("Guanjianzi");
                if (a.Length!=0)
                {
                   
                    Guanjianzi = a.Split('|').ToList();
                    Yonghu = b.Split('|').ToList();
                    Guanjianzi.Remove(string.Empty);
                    Yonghu.Remove(string.Empty);
                }
            }
            else
            {
                setting.SetSettingValue("Guanjianzi",string.Empty);
                setting.SetSettingValue("Yonghu", string.Empty);
            }

       }
        private bool DanDis_Dis(string text)
        {
            var a = (from sb in Guanjianzi where text.Contains(sb) select sb).ToList();
            var b = (from sb in Yonghu where text.Contains(sb) select sb).ToList();
            if (b.Count!= 0||a.Count!=0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void DanDis_Add(string text,bool IsYonghu)
        {
            if (IsYonghu)
            {
                string b = (string)setting.GetSettingValue("Yonghu") + "|" + text;
                setting.SetSettingValue("Yonghu", b);
                Yonghu.Add(text);
            }
            else
            {
                string a = (string)setting.GetSettingValue("Guanjianzi")+"|"+text;
                setting.SetSettingValue("Guanjianzi", a);
                Guanjianzi.Add(text);
            }

        }

        #endregion
       
        private void tw_Background_Toggled(object sender, RoutedEventArgs e)
        {
            if (tw_AutoFull.IsOn)
            {
                mediaElement.AudioCategory = AudioCategory.BackgroundCapableMedia;
            }
            else
            {
                mediaElement.AudioCategory = AudioCategory.ForegroundOnlyMedia;
            }

        }

        private void Send_text_Comment_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.Key== Windows.System.VirtualKey.Enter)
            {
                SenDanmuKa();
            }
        }

        //投币
        private void btn_TB_1_Click(object sender, RoutedEventArgs e)
        {
            TouBi(1);
        }

        private void btn_No_Click(object sender, RoutedEventArgs e)
        {
            grid_Tb.Hide();
        }
        public async void TouBi(int num)
        {
            UserClass getLogin = new UserClass();
            if (getLogin.IsLogin())
            {
                try
                {
                    WebClientClass wc = new WebClientClass();
                    Uri ReUri = new Uri("http://www.bilibili.com/plus/comment.php");
                    string QuStr = "aid=" + Aid + "&rating=100&player=1&multiply=" + num;
                    string result = await wc.PostResults(ReUri, QuStr);
                    if (result == "OK")
                    {
                        messShow.Show("投币成功！", 3000);
                    }
                    else
                    {
                        messShow.Show("投币失败！" + result, 3000);
                    }
                }
                catch (Exception ex)
                {
                    messShow.Show("投币时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                messShow.Show("请先登录!", 3000);
            }
        }

        private void btn_RePlay_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
            danmu.ClearDanmu();
            grid_end.Visibility = Visibility.Collapsed;
        }

        private void btn_LastPost_Click(object sender, RoutedEventArgs e)
        {
            if (LastPost != 0)
            {
                mediaElement.Position = new TimeSpan(0, 0, Convert.ToInt32(LastPost));
                btn_LastPost.Visibility = Visibility.Collapsed;
            }
        }

        private void menu_LastPost_Click(object sender, RoutedEventArgs e)
        {
            if (LastPost!=0)
            {
                mediaElement.Position = new TimeSpan(0, 0, Convert.ToInt32(LastPost));
                btn_LastPost.Visibility = Visibility.Collapsed;
            }
        }
    }
    //进度转换
    public sealed class PostThumbToolTipValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int i = 0;
            int.TryParse(value.ToString(), out i);
            TimeSpan ts = new TimeSpan(0, 0, i);
            return ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
