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
using Windows.System.Display;
using Windows.UI;
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
        private DisplayRequest dispRequest = null;//保持屏幕常亮
        protected override async  void OnNavigatedTo(NavigationEventArgs e)
        {
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
            progress.Visibility = Visibility.Visible;
            top_Title.Text = model.title + " " + PlayP;
            pro_Num.Text = "填充弹幕中...";
            DanMuPool = await GetDM(model.cid);
            pro_Num.Text = "读取视频信息...";
            await GetPlayInfo(model.cid, top_cb_Quality.SelectedIndex+1);
        }

        private async void Datetimer_Tick(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                top_txt_Time.Text = DateTime.Now.ToLocalTime().ToString("HH:mm");
            });
            //App.Current.NetworkType
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //取消屏幕常亮
            if (dispRequest != null)
            {
                dispRequest = null;
            }
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
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

            if (setting.SettingContains("Full"))
            {
                if ((bool)setting.GetSettingValue("Full"))
                {
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    btn_Full.Visibility = Visibility.Collapsed;
                    btn_ExitFull.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_Full.Visibility = Visibility.Visible;
                    btn_ExitFull.Visibility = Visibility.Collapsed;
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
                }
                else
                {
                    setting.SetSettingValue("Full", false);
                    btn_Full.Visibility = Visibility.Visible;
                    btn_ExitFull.Visibility = Visibility.Collapsed;
                }
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
                slider_DanmuSize.Value = 22;
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

        public async Task<List<MyDanmaku.DanMuModel>> GetDM(string cid)
        {
            List<MyDanmaku.DanMuModel> ls = new List<MyDanmaku.DanMuModel>();
            try
            {
                string a =await new WebClientClass().GetResults(new Uri("http://comment.bilibili.com/" + cid + ".xml"));
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
            top_Title.Text = X.ToString();
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

        private void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
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

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {

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
            danmu.AddGunDanmu(new MyDanmaku.DanMuModel { DanText = Send_text_Comment.Text, _DanColor = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), DanSize = "25" },true);
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
            setting.SetSettingValue("DanmuSzie", slider_DanmuSize.Value);
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
                setting.SetSettingValue("Guanjianzi",b);
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
    }
}
