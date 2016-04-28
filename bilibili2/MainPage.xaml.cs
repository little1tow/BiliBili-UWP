using bilibili2.Class;
using bilibili2.Pages;
using bilibili2.PartPages;
using JyUserFeedback;
using JyUserFeedback.view;
using JyUserInfo;
using JyUserInfo.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace bilibili2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        Frame rootFrame = (Window.Current.Content as Frame);

        public MainPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            pivot_Home.SelectedIndex = 2;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            home_Items.PlayEvent += Home_Items_PlayEvent;
            home_Items.ErrorEvent += Home_Items_ErrorEvent;
            liveinfo.ErrorEvent += Home_Items_ErrorEvent;
            liveinfo.PlayEvent += Liveinfo_PlayEvent;
            JyFeedbackControl.FeedbackImageRequested += async delegate
            {
                var fileOpenPicker = new FileOpenPicker();
                fileOpenPicker.FileTypeFilter.Add(".png");
                fileOpenPicker.FileTypeFilter.Add(".jpg");

                var file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    _jyUserFeedbackSdkManager.UploadPicture(ApiHelper.JyAppkey, ApiHelper.JySecret, file);
                }
            };
            //this.RequestedTheme = ElementTheme.Dark;
        }

        private void Liveinfo_PlayEvent(string aid)
        {
            infoFrame.Navigate(typeof(LiveInfoPage),aid);
        }

        string navInfo = string.Empty;
        private SettingHelper settings = new SettingHelper();
        DispatcherTimer timer = new DispatcherTimer();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                GetFeedInfo();
                GetLoadInfo();
                SetHomeInfo();
                SetWeekInfo();
                home_Items.SetHomeInfo();
                timer.Interval = new TimeSpan(0, 0, 5);
                timer.Start();
                timer.Tick += Timer_Tick;

            }
           
            ChangeTheme();
            ChangeDrak();
            navInfo = infoFrame.GetNavigationState();
            infoFrame.Tag = (SolidColorBrush)top_grid.Background;
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (await HasMessage())
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    bor_HasMessage.Visibility = Visibility.Visible;
                });
            }
            else
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    bor_HasMessage.Visibility = Visibility.Collapsed;
                });
            }
        }

        private async Task<bool> HasMessage()
        {
            try
            {
                 wc = new WebClientClass();
                // http://message.bilibili.com/api/msg/query.room.list.do?access_key=a36a84cc8ef4ea2f92c416951c859a25&actionKey=appkey&appkey=c1b107428d337928&build=414000&page_size=100&platform=android&ts=1461404884000&sign=5e212e424761aa497a75b0fb7fbde775
                string url = string.Format("http://message.bilibili.com/api/notify/query.notify.count.do?_device=wp&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=411005&platform=android&ts={2}", ApiHelper.access_key, ApiHelper._appKey, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                MessageModel model = JsonConvert.DeserializeObject<MessageModel>(results);
                if (model.code == 0)
                {
                    MessageModel list = JsonConvert.DeserializeObject<MessageModel>(model.data.ToString());
                    if (list.reply_me != 0||list.chat_me!=0|| list.notify_me!=0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
                //messShow.Show("读取通知失败", 3000);
            }
        }

        private void GetSetting()
        {

        }
        //首页错误
        private void Home_Items_ErrorEvent(string aid)
        {
            messShow.Show("读取首页信息失败\r\n" + aid, 3000);
        }
        //首页跳转
        private void Home_Items_PlayEvent(string aid)
        {
            infoFrame.Navigate(typeof(VideoInfoPage), aid);

            //jinr.From = this.ActualWidth;
            //storyboardPopIn.Begin();
        }
        //双击退出
        bool IsClicks = false;
        private async void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (infoFrame.Content != null)
            {
                e.Handled = true;
                if (infoFrame.CanGoBack)
                {
                    // e.Handled = true;
                    infoFrame.GoBack();
                }
                else
                {
                    // e.Handled = true;
                    tuic.To = this.ActualWidth;
                    storyboardPopOut.Begin();
                }
            }
            else
            {
                if (e.Handled == false)
                {
                    if (IsClicks)
                    {
                        Application.Current.Exit();
                    }
                    else
                    {
                        IsClicks = true;
                        e.Handled = true;
                        txt_GG.Text = "再按一次退出程序";
                        grid_GG.Visibility = Visibility.Visible;
                        await Task.Delay(1500);
                        IsClicks = false;
                        grid_GG.Visibility = Visibility.Collapsed;
                    }
                }
            }
            //Frame rootFrame = Window.Current.Content as Frame;

        }

        //首页调整页面
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            pivot_Home.SelectedIndex = Convert.ToInt32((sender as Button).Tag);
        }
        //更新界面
        public void UpdateUI()
        {
            btn_Bangumi.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Find.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Home.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Live.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Update.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Part.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Bangumi.FontWeight = FontWeights.Normal;
            btn_Find.FontWeight = FontWeights.Normal;
            btn_Home.FontWeight = FontWeights.Normal;
            btn_Live.FontWeight = FontWeights.Normal;
            btn_Update.FontWeight = FontWeights.Normal;
            btn_Part.FontWeight = FontWeights.Normal;
            switch (pivot_Home.SelectedIndex)
            {
                case 0:
                    btn_Live.Foreground = new SolidColorBrush(Colors.White);
                    btn_Live.FontWeight = FontWeights.Bold;
                    break;
                case 1:
                    btn_Bangumi.Foreground = new SolidColorBrush(Colors.White);
                    btn_Bangumi.FontWeight = FontWeights.Bold;
                    break;
                case 2:
                    btn_Home.Foreground = new SolidColorBrush(Colors.White);
                    btn_Home.FontWeight = FontWeights.Bold;
                    break;
                case 3:
                    btn_Part.Foreground = new SolidColorBrush(Colors.White);
                    btn_Part.FontWeight = FontWeights.Bold;
                    break;
                case 4:
                    btn_Update.Foreground = new SolidColorBrush(Colors.White);
                    btn_Update.FontWeight = FontWeights.Bold;
                    break;
                case 5:
                    btn_Find.Foreground = new SolidColorBrush(Colors.White);
                    btn_Find.FontWeight = FontWeights.Bold;
                    break;
                default:
                    break;
            }

        }
        //打开汉堡菜单
        private void btn_OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sp_View.IsPaneOpen)
            {
                sp_View.IsPaneOpen = false;
            }
            else
            {
                sp_View.IsPaneOpen = true;
            }
        }
        // pivot改变
        private async void pivot_Home_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
            switch (pivot_Home.SelectedIndex)
            {
                case 0:
                 
                    if (!liveinfo.isLoaded)
                    {
                        GetLiveBanner();
                        liveinfo.GetLiveInfo();

                    }
                    break;
                case 1:
                    if (!LoadBan)
                    {
                        LoadBan = false;
                        await GetBanUpdate();
                        await GetBanBanner();
                        await GetBanTJ();
                        LoadBan = true;
                    }
                    if (!liveinfo.isLoaded)
                    {
                        GetLiveBanner();
                        liveinfo.GetLiveInfo();
                    }
                    break;
                case 2:
                    if (!LoadBan)
                    {
                        LoadBan = false;
                        await GetBanUpdate();
                        await GetBanBanner();
                        await GetBanTJ();
                        LoadBan = true;
                    }
                    break;
                case 3:
                    if (!LoadDT)
                    {
                        GetDt();
                    }
                    if (!LoadBan)
                    {
                        LoadBan = false;
                        await GetBanUpdate();
                        await GetBanBanner();
                        await GetBanTJ();
                        LoadBan = true;
                    }
                    break;
                case 4:
                    if (!LoadDT)
                    {
                        GetDt();
                    }
                    break;
                case 5:
                    if (!LoadHot)
                    {
                        GetHotKeyword();
                    }
                    break;
                default:
                    break;
            }

        }

        //侧滑来源http://www.cnblogs.com/hebeiDGL/p/4775377.html
        #region  从屏幕左侧边缘滑动屏幕时，打开 SplitView 菜单

        // SplitView 控件模板中，Pane部分的 Grid
        Grid PaneRoot;

        //  引用 SplitView 控件中， 保存从 Pane “关闭” 到“打开”的 VisualTransition
        //  也就是 <VisualTransition From="Closed" To="OpenOverlayLeft"> 这个 
        VisualTransition from_ClosedToOpenOverlayLeft_Transition;

        private void Border_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;

            // 仅当 SplitView 处于 Overlay 模式时（窗口宽度最小时）
            if (sp_View.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                if (PaneRoot == null)
                {
                    // 找到 SplitView 控件中，模板的父容器
                    Grid grid = FindVisualChild<Grid>(sp_View);

                    PaneRoot = grid.FindName("PaneRoot") as Grid;

                    if (from_ClosedToOpenOverlayLeft_Transition == null)
                    {
                        // 获取 SplitView 模板中“视觉状态集合”
                        IList<VisualStateGroup> stateGroup = VisualStateManager.GetVisualStateGroups(grid);

                        //  获取 VisualTransition 对象的集合。
                        IList<VisualTransition> transitions = stateGroup[0].Transitions;

                        // 找到 SplitView.IsPaneOpen 设置为 true 时，播放的 transition
                        from_ClosedToOpenOverlayLeft_Transition = transitions?.Where(train => train.From == "Closed" && train.To == "OpenOverlayLeft").First();
                    }
                }


                // 默认为 Collapsed，所以先显示它
                PaneRoot.Visibility = Visibility.Visible;

                // 当在 Border 上向右滑动，并且滑动的总距离需要小于 Panel 的默认宽度。否则会脱离左侧窗口，继续向右拖动
                if (e.Cumulative.Translation.X >= 0 && e.Cumulative.Translation.X < sp_View.OpenPaneLength)
                {
                    CompositeTransform ct = PaneRoot.RenderTransform as CompositeTransform;
                    ct.TranslateX = (e.Cumulative.Translation.X - sp_View.OpenPaneLength);
                }
            }
        }

        private void Border_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;

            // 仅当 SplitView 处于 Overlay 模式时（窗口宽度最小时）
            if (sp_View.DisplayMode == SplitViewDisplayMode.Overlay && PaneRoot != null)
            {
                // 因为当 IsPaneOpen 为 true 时，会通过 VisualStateManager 把 PaneRoot.Visibility  设置为
                // Visibility.Visible，所以这里把它改为 Visibility.Collapsed，以回到初始状态
                PaneRoot.Visibility = Visibility.Collapsed;

                // 恢复初始状态 
                CompositeTransform ct = PaneRoot.RenderTransform as CompositeTransform;


                // 如果大于 MySplitView.OpenPaneLength 宽度的 1/2 ，则显示，否则隐藏
                if ((sp_View.OpenPaneLength + ct.TranslateX) > sp_View.OpenPaneLength / 2)
                {
                    sp_View.IsPaneOpen = true;

                    // 因为上面设置 IsPaneOpen = true 会再次播放向右滑动的动画，所以这里使用 SkipToFill()
                    // 方法，直接跳到动画结束状态
                    from_ClosedToOpenOverlayLeft_Transition?.Storyboard?.SkipToFill();

                }

                ct.TranslateX = 0;
            }
        }


        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            int count = Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }
        #endregion
        //页面大小改变
        private async void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!settings.SettingContains("HideTitleBar"))
            {
                settings.SetSettingValue("HideTitleBar", true);
            }
            ApplicationView av = ApplicationView.GetForCurrentView();
            switch (av.Orientation)
            {
                case ApplicationViewOrientation.Landscape:
                    if ((bool)settings.GetSettingValue("HideTitleBar"))
                    {
                        if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(StatusBar).ToString()))
                        {
                            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                            await statusBar.HideAsync();
                        }
                    }
                    break;
                case ApplicationViewOrientation.Portrait:
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(StatusBar).ToString()))
                    {
                        StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                        await statusBar.ShowAsync();
                    }
                    break;
                default:
                    break;
            }

            if (this.ActualWidth <= 640)
            {
                fvLeft.Visibility = Visibility.Collapsed;
                fvRight.Visibility = Visibility.Collapsed;
                grid_c_left.Width = new GridLength(0, GridUnitType.Auto);
                grid_c_right.Width = new GridLength(0, GridUnitType.Auto);
                grid_c_center.Width = new GridLength(1, GridUnitType.Star);
                fvLeft_Ban.Visibility = Visibility.Collapsed;
                fvRight_Ban.Visibility = Visibility.Collapsed;
                grid_c_left_Ban.Width = new GridLength(0, GridUnitType.Auto);
                grid_c_right_Ban.Width = new GridLength(0, GridUnitType.Auto);
                grid_c_center_Ban.Width = new GridLength(1, GridUnitType.Star);
                fvLeft_Live.Visibility = Visibility.Collapsed;
                fvRight_Live.Visibility = Visibility.Collapsed;
                grid_c_left_Live.Width = new GridLength(0, GridUnitType.Auto);
                grid_c_right_Live.Width = new GridLength(0, GridUnitType.Auto);
                grid_c_center_Live.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                fvLeft.Visibility = Visibility.Visible;
                fvRight.Visibility = Visibility.Visible;
                grid_c_left.Width = new GridLength(1, GridUnitType.Star);
                grid_c_right.Width = new GridLength(1, GridUnitType.Star);
                grid_c_center.Width = new GridLength(0, GridUnitType.Auto);
                fvLeft_Ban.Visibility = Visibility.Visible;
                fvRight_Ban.Visibility = Visibility.Visible;
                grid_c_left_Ban.Width = new GridLength(1, GridUnitType.Star);
                grid_c_right_Ban.Width = new GridLength(1, GridUnitType.Star);
                grid_c_center_Ban.Width = new GridLength(0, GridUnitType.Auto);
                fvLeft_Live.Visibility = Visibility.Visible;
                fvRight_Live.Visibility = Visibility.Visible;
                grid_c_left_Live.Width = new GridLength(1, GridUnitType.Star);
                grid_c_right_Live.Width = new GridLength(1, GridUnitType.Star);
                grid_c_center_Live.Width = new GridLength(0, GridUnitType.Auto);
            }

            if (this.ActualWidth < 1000)
            {
                top_txt_Header.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                top_txt_Header.HorizontalAlignment = HorizontalAlignment.Center;
            }

            if (this.ActualWidth < 640)
            {
                //double i = (double)test.ActualWidth;
                test.Width = double.NaN;
            }
            else
            {
                int i = Convert.ToInt32(pivot_Home.ActualWidth / 500);
                test.Width = pivot_Home.ActualWidth / i - 20;
            }
        }
        //打开搜索框
        private void btn_GoFind_Click(object sender, RoutedEventArgs e)
        {
            if (top_txt_find.Visibility == Visibility.Collapsed)
            {
                btn_GoFind.Visibility = Visibility.Collapsed;
                top_txt_find.Visibility = Visibility.Visible;
            }
            else
            {
                btn_GoFind.Visibility = Visibility.Visible;
                top_txt_find.Visibility = Visibility.Collapsed;
            }

        }
        //打开分区
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            switch (int.Parse((sender as HyperlinkButton).Tag.ToString()))
            {
                case 0:
                    pivot_Home.SelectedIndex = 0;
                    break;
                case 1:
                    infoFrame.Navigate(typeof(FJPage));
                    break;
                case 2:
                    infoFrame.Navigate(typeof(DHPage));
                    break;
                case 3:
                    infoFrame.Navigate(typeof(YYPage));
                    break;
                case 4:
                    infoFrame.Navigate(typeof(WDPage));
                    break;
                case 5:
                    infoFrame.Navigate(typeof(KJPage));
                    break;
                case 6:
                    infoFrame.Navigate(typeof(YXPage));
                    break;
                case 7:
                    infoFrame.Navigate(typeof(GCPage));
                    break;
                case 8:
                    infoFrame.Navigate(typeof(YLPage));
                    break;
                case 9:
                    infoFrame.Navigate(typeof(DYPage));
                    break;
                case 10:
                    infoFrame.Navigate(typeof(DSJPage));
                    break;
                case 11:
                    infoFrame.Navigate(typeof(SSPage));
                    break;
                default:
                    break;
            }
            //jinr.From = this.ActualWidth;
        }
        //子页面后退
        private void MainPage_BackEvent()
        {
            tuic.To = this.ActualWidth;
            storyboardPopOut.Begin();
        }
        //子页面后退动画完成
        private void StoryboardPopOut_Completed(object sender, object e)
        {
            infoFrame.ContentTransitions = null;
            infoFrame.Content = null;
            infoFrame.SetNavigationState(navInfo);
            //infoFrame.CacheSize = 0;
            //int i=  infoFrame.BackStackDepth;
            //string a = string.Empty;
            dh.TranslateX = 0;
        }
        //首页Banner选择改变
        private void home_flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (home_flipView.Items.Count == 0 || fvLeft.Items.Count == 0 || fvRight.Items.Count == 0)
            {
                return;
            }
            if (fvLeft.Visibility == Visibility.Collapsed || fvRight.Visibility == Visibility.Collapsed)
            {
                return;
            }
            if (this.home_flipView.SelectedIndex == 0)
            {
                this.fvLeft.SelectedIndex = this.fvLeft.Items.Count - 1;
                this.fvRight.SelectedIndex = 1;
            }
            else if (this.home_flipView.SelectedIndex == 1)
            {
                this.fvLeft.SelectedIndex = 0;
                this.fvRight.SelectedIndex = this.fvRight.Items.Count - 1;
            }
            else if (this.home_flipView.SelectedIndex == this.home_flipView.Items.Count - 1)
            {
                this.fvLeft.SelectedIndex = this.fvLeft.Items.Count - 2;
                this.fvRight.SelectedIndex = 0;
            }
            else if ((this.home_flipView.SelectedIndex < (this.home_flipView.Items.Count - 1)) && this.home_flipView.SelectedIndex > -1)
            {
                this.fvLeft.SelectedIndex = this.home_flipView.SelectedIndex - 1;
                this.fvRight.SelectedIndex = this.home_flipView.SelectedIndex + 1;
            }
            else
            {
                return;
            }
        }
        //番剧Banner选择改变
        private void home_flipView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (home_flipView_Ban.Items.Count == 0 || fvLeft_Ban.Items.Count == 0 || fvRight_Ban.Items.Count == 0)
            {
                return;
            }
            if (fvLeft_Ban.Visibility == Visibility.Collapsed || fvRight_Ban.Visibility == Visibility.Collapsed)
            {
                return;
            }
            if (this.home_flipView_Ban.SelectedIndex == 0)
            {
                this.fvLeft_Ban.SelectedIndex = this.fvLeft_Ban.Items.Count - 1;
                this.fvRight_Ban.SelectedIndex = 1;
            }
            else if (this.home_flipView_Ban.SelectedIndex == 1)
            {
                this.fvLeft_Ban.SelectedIndex = 0;
                this.fvRight_Ban.SelectedIndex = this.fvRight_Ban.Items.Count - 1;
            }
            else if (this.home_flipView_Ban.SelectedIndex == this.home_flipView_Ban.Items.Count - 1)
            {
                this.fvLeft_Ban.SelectedIndex = this.fvLeft_Ban.Items.Count - 2;
                this.fvRight_Ban.SelectedIndex = 0;
            }
            else if ((this.home_flipView_Ban.SelectedIndex < (this.home_flipView_Ban.Items.Count - 1)) && this.home_flipView_Ban.SelectedIndex > -1)
            {
                this.fvLeft_Ban.SelectedIndex = this.home_flipView_Ban.SelectedIndex - 1;
                this.fvRight_Ban.SelectedIndex = this.home_flipView_Ban.SelectedIndex + 1;
            }
            else
            {
                return;
            }
        }
        //Banner的加载
        public void SetListView(string results)
        {
            try
            {
                BannerModel model = JsonConvert.DeserializeObject<BannerModel>(results);
                List<BannerModel> ban = JsonConvert.DeserializeObject<List<BannerModel>>(model.data.ToString());
                var li = from a in ban where a.type != 1 select a;
                home_flipView.ItemsSource = li;
                fvLeft.ItemsSource = li;
                fvRight.ItemsSource = li;
                this.home_flipView.SelectedIndex = 0;
                if (fvLeft.Visibility != Visibility.Collapsed || fvRight.Visibility != Visibility.Collapsed)
                {
                    this.fvLeft.SelectedIndex = this.fvLeft.Items.Count - 1;
                    this.fvRight.SelectedIndex = this.home_flipView.SelectedIndex + 1;
                }
            }
            catch (Exception)
            {
            }
        }

        WebClientClass wc = new WebClientClass();
        public async void SetHomeInfo()
        {
            try
            {
                // string banner = await wc.GetResults(new Uri("http://www.bilibili.com/index/slideshow.json"));
                string banner = await wc.GetResults(new Uri("http://app.bilibili.com/x/banner?plat=4&build=412001"));
                SetListView(banner);
            }
            catch (Exception ex)
            {
                messShow.Show("读取Banner失败！\r\n" + ex.Message, 3000);
                //MessageDialog md = new MessageDialog("读取首页信息失败！\r\n"+ex.Message);
                //await md.ShowAsync();
            }

            // GetZBInfo();
        }
        //用户登录或跳转
        private void btn_UserInfo_Click(object sender, RoutedEventArgs e)
        {
            if (txt_UserName.Text == "请登录")
            {
                infoFrame.Navigate(typeof(LoginPage));

                //jinr.From = this.ActualWidth;
                //storyboardPopIn.Begin();
            }
            else
            {
                infoFrame.Navigate(typeof(UserInfoPage));
            }
            //this.Frame.Navigate(typeof(LoginPage));
        }
        //用户登录成功，读取用户信息
        private void MainPage_LoginEd()
        {
            GetLoadInfo();
        }
        //读取用户信息
        private async void GetLoadInfo()
        {
            UserClass getLogin = new UserClass();
            if (!getLogin.IsLogin())
            {
                //设置是否存在
                if (container.Values["UserName"] != null && container.Values["UserPass"] != null && container.Values["AutoLogin"] != null)
                {
                    //用户名、密码是否为空
                    if (container.Values["AutoLogin"].ToString() == "true" && container.Values["UserName"].ToString() != "" && container.Values["UserPass"].ToString() != "")
                    {
                        //读取登录结果
                        string result = await ApiHelper.LoginBilibili(container.Values["UserName"].ToString(), container.Values["UserPass"].ToString());
                        GetLoginInfoModel model = await getLogin.GetUserInfo();
                        if (model != null)
                        {
                            txt_UserName.Text = model.name;
                            txt_Sign.Visibility = Visibility.Visible;
                            txt_Sign.Text = model.RankStr;
                            img_user.ImageSource = new BitmapImage(new Uri(model.face));
                        }
                        messShow.Show(result, 3000);
                    }
                    else
                    {
                        txt_UserName.Text = "请登录";
                        txt_Sign.Visibility = Visibility.Collapsed;
                        img_user.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/other/NoAvatar.png"));
                    }
                }
                else
                {
                    container.Values["UserName"] = "";
                    container.Values["UserPass"] = "";
                    container.Values["AutoLogin"] = "";
                    txt_UserName.Text = "请登录";
                    txt_Sign.Visibility = Visibility.Collapsed;

                    img_user.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/other/NoAvatar.png"));
                }
            }
            else
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
                ApiHelper.access_key = await FileIO.ReadTextAsync(file);
                GetLoginInfoModel model = await getLogin.GetUserInfo();
                if (model != null)
                {
                    txt_UserName.Text = model.name;
                    txt_Sign.Visibility = Visibility.Visible;
                    txt_Sign.Text = model.RankStr;
                    img_user.ImageSource = new BitmapImage(new Uri(model.face));
                }
            }
        }

        bool LoadDT = false;
        int DT_PageNum = 1;
        //加载动态
        private async void GetDt()
        {
            UserClass getLogin = new UserClass();
            if (getLogin.IsLogin())
            {
                try
                {
                    pr_Load_DT.Visibility = Visibility.Visible;
                    DT_0.Visibility = Visibility.Collapsed;
                    DT_1.Visibility = Visibility.Collapsed;
                    DT_Info.Visibility = Visibility.Visible;
                    DT_noLoad.Visibility = Visibility.Collapsed;
                    user_GridView_Bangumi.ItemsSource = await getLogin.GetUserBangumi();
                    if (user_GridView_Bangumi.Items.Count == 0)
                    {
                        DT_0.Visibility = Visibility.Visible;
                    }
                    List<GetAttentionUpdate> list_Attention = await getLogin.GetUserAttentionUpdate(DT_PageNum);
                    DT_PageNum++;
                    //User_ListView_Attention.ItemsSource = list_Attention;
                    foreach (GetAttentionUpdate item in list_Attention)
                    {
                        User_ListView_Attention.Items.Add(item);
                    }
                    if (User_ListView_Attention.Items.Count == 0)
                    {
                        DT_1.Visibility = Visibility.Visible;
                    }
                    LoadDT = true;
                }
                catch (Exception)
                {
                    messShow.Show("读取动态失败", 3000);
                }
                finally
                {
                    pr_Load_DT.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                DT_Info.Visibility = Visibility.Collapsed;
                DT_noLoad.Visibility = Visibility.Visible;
                LoadDT = false;
                DT_PageNum = 1;
            }
        }
        //读取搜索热词
        bool LoadHot = false;
        public async void GetHotKeyword()
        {
            try
            {

                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://www.bilibili.com/search?action=hotword&main_ver=v1"));
                HotModel model = JsonConvert.DeserializeObject<HotModel>(results);
                List<HotModel> ban = JsonConvert.DeserializeObject<List<HotModel>>(model.list.ToString());

                list_Hot.ItemsSource = ban;
                LoadHot = true;
            }
            catch (Exception ex)
            {
                messShow.Show("读取搜索热词失败\r\n" + ex.Message, 3000);
                LoadHot = false;
            }

        }
        //汉堡菜单的点击
        private void list_Menu_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((e.ClickedItem as StackPanel).Tag == null)
            {
                return;
            }
            bool isLogin = new UserClass().IsLogin();
            switch ((e.ClickedItem as StackPanel).Tag.ToString())
            {
                case "M_Drak_Light":
                    if (RequestedTheme== ElementTheme.Dark)
                    {
                        settings.SetSettingValue("Drak", false);
                        txt_D_L.Text = "夜间模式";
                        font_D_L.Glyph = "\uE708";
                        RequestedTheme = ElementTheme.Light;
                    }
                    else
                    {
                        settings.SetSettingValue("Drak", true);
                        txt_D_L.Text = "日间模式";
                        font_D_L.Glyph = "\uE706";
                        RequestedTheme = ElementTheme.Dark;
                    }
                    ChangeTitbarColor();
                    break;
                case "Favbox":
                    if (isLogin)
                    {
                        infoFrame.Navigate(typeof(FavPage));
                    }
                    else
                    {
                        messShow.Show("请先登录！", 3000);
                    }
                    break;
                case "History":
                    if (isLogin)
                    {
                        infoFrame.Navigate(typeof(HistoryPage));
                    }
                    else
                    {
                        messShow.Show("请先登录！", 3000);
                    }
                    break;
                case "Message":
                    if (isLogin)
                    {
                        infoFrame.Navigate(typeof(MessagePage));
                    }
                    else
                    {
                        messShow.Show("请先登录！", 3000);
                    }
                    break;
                case "Download":
                    break;
                case "Setting":
                    infoFrame.Navigate(typeof(SettingPage));
                    break;
                case "Feedback":
                    Feedback();
                    break;
                default:
                    break;
            }

        }
        //触发主题改变
        private void MainPage_ChangeDrak()
        {
            ChangeDrak();
        }

        //改变主题
        private void ChangeTheme()
        {
            string ThemeName = string.Empty;
            if (settings.SettingContains("Theme"))
            {
                ThemeName = settings.GetSettingValue("Theme") as string;
            }
            else
            {
                ThemeName = "Pink";
                settings.SetSettingValue("Theme", "Pink");
            }
            ResourceDictionary newDictionary = new ResourceDictionary();
            switch (ThemeName)
            {
                case "Red":
                    newDictionary.Source = new Uri("ms-appx:///Theme/RedTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Blue":
                    newDictionary.Source = new Uri("ms-appx:///Theme/BlueTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Green":
                    newDictionary.Source = new Uri("ms-appx:///Theme/GreenTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Pink":
                    newDictionary.Source = new Uri("ms-appx:///Theme/PinkTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Purple":
                    newDictionary.Source = new Uri("ms-appx:///Theme/PurpleTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                case "Yellow":
                    newDictionary.Source = new Uri("ms-appx:///Theme/YellowTheme.xaml", UriKind.RelativeOrAbsolute);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                    if (txt_D_L.Text == "日间模式")
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        RequestedTheme = ElementTheme.Dark;
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                default:
                    break;
            }
            tuic.To = this.ActualWidth;
            storyboardPopOut.Begin();
            ChangeTitbarColor();
        }
        private void ChangeTitbarColor()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                // StatusBar.GetForCurrentView().HideAsync();
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
                statusBar.BackgroundOpacity = 100;
            }
            //电脑标题栏颜色
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
            titleBar.ForegroundColor = Color.FromArgb(255, 254, 254, 254);//Colors.White纯白用不了。。。
            titleBar.ButtonHoverBackgroundColor = ((SolidColorBrush)menu_DarkBack.Background).Color;
            titleBar.ButtonBackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
            titleBar.ButtonForegroundColor = Color.FromArgb(255, 254, 254, 254);
            titleBar.InactiveBackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
            titleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)top_grid.Background).Color;
            infoFrame.Tag = (SolidColorBrush)top_grid.Background;
        }
        private void ChangeDrak()
        {
            if (!settings.SettingContains("Drak"))
            {
                settings.SetSettingValue("Drak", false);
            }
            if ((bool)settings.GetSettingValue("Drak"))
            {
                RequestedTheme = ElementTheme.Dark;
                txt_D_L.Text = "日间模式";
                font_D_L.Glyph = "\uE706";
            }
            else
            {
                RequestedTheme = ElementTheme.Light;
                txt_D_L.Text = "夜间模式";
                font_D_L.Glyph = "\uE708";
            }
             ChangeTitbarColor();
        }
        //动态加载更多
        bool Moreing = true;
        private async void sc_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sc.VerticalOffset == sc.ScrollableHeight)
            {
                if (User_load_more.Content.ToString() == "没有更多了...")
                {
                    return;
                }
                if (Moreing)
                {
                    Moreing = false;
                    User_load_more.IsEnabled = false;
                    User_load_more.Content = "加载中..";
                    List<GetAttentionUpdate> list_Attention = await new UserClass().GetUserAttentionUpdate(DT_PageNum);
                    //User_ListView_Attention.ItemsSource = list_Attention;
                    DT_PageNum++;
                    foreach (GetAttentionUpdate item in list_Attention)
                    {
                        User_ListView_Attention.Items.Add(item);
                    }
                    User_load_more.IsEnabled = true;
                    User_load_more.Content = "加载更多";
                    if (list_Attention.Count == 0)
                    {
                        User_load_more.IsEnabled = false;
                        User_load_more.Content = "没有更多了...";
                    }

                    Moreing = true;
                }
            }

        }
        //点击动态
        private void User_ListView_Attention_ItemClick(object sender, ItemClickEventArgs e)
        {
            infoFrame.Navigate(typeof(VideoInfoPage), (e.ClickedItem as GetAttentionUpdate).aid);
        }
        //Banner点击
        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (((BannerModel)home_flipView.SelectedItem).type == 2)
            {
                infoFrame.Navigate(typeof(WebViewPage), ((BannerModel)home_flipView.SelectedItem).value);
                //jinr.From = this.ActualWidth;
            }
            if (((BannerModel)home_flipView.SelectedItem).type == 3)
            {
                infoFrame.Navigate(typeof(BanInfoPage), ((BannerModel)home_flipView.SelectedItem).value);
                //KeyValuePair<string, bool> info = new KeyValuePair<string, bool>(((BannerModel)home_flipView.SelectedItem).value, true);
                //this.Frame.Navigate(typeof(BangumiInfoPage), info);
                // this.Frame.Navigate(typeof(WebViewPage), ((BannerModel)home_flipView.SelectedItem).value);
            }
        }
        //打开话题
        private void Find_btn_Topic_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActualWidth > 500)
            {
                sp_Find.IsPaneOpen = true;
                GetTopic();
            }
            else
            {
                infoFrame.Navigate(typeof(TopicPage));
                //jinr.From = this.ActualWidth;
            }

        }
        //读取话题
        private async void GetTopic()
        {
            try
            {
                pr_Load_Topic.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://www.bilibili.com/index/slideshow.json"));
                TopicModel model = JsonConvert.DeserializeObject<TopicModel>(results);
                list_Topic.ItemsSource = JsonConvert.DeserializeObject<List<TopicModel>>(model.list.ToString());
            }
            catch (Exception ex)
            {
                messShow.Show("读取话题失败\r\n" + ex.Message, 3000);
            }
            finally
            {
                pr_Load_Topic.Visibility = Visibility.Collapsed;
            }
        }
        //infoFrame跳转
        private void infoFrame_Navigated(object sender, NavigationEventArgs e)
        {
            switch ((e.Content as Page).Tag.ToString())
            {
                case "视频信息":
                    (infoFrame.Content as VideoInfoPage).BackEvent += MainPage_BackEvent;
                    break;
                case "网页浏览":
                    (infoFrame.Content as WebViewPage).BackEvent += MainPage_BackEvent;
                    break;
                case "登录":
                    (infoFrame.Content as LoginPage).BackEvent += MainPage_BackEvent;
                    (infoFrame.Content as LoginPage).LoginEd += MainPage_LoginEd;
                    break;
                case "话题":
                    (infoFrame.Content as TopicPage).BackEvent += MainPage_BackEvent;
                    break;
                case "排行榜":
                    (infoFrame.Content as RankPage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧信息":
                    (infoFrame.Content as BanInfoPage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧更新时间表":
                    (infoFrame.Content as BanTimelinePage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧索引":
                    (infoFrame.Content as BanTagPage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧Tag":
                    (infoFrame.Content as BanByTagPage).BackEvent += MainPage_BackEvent;
                    break;
                case "全部追番":
                    (infoFrame.Content as UserBangumiPage).BackEvent += MainPage_BackEvent;
                    break;
                case "用户中心":
                    (infoFrame.Content as UserInfoPage).BackEvent += MainPage_BackEvent;
                    (infoFrame.Content as UserInfoPage).ExitEvent += MainPage_ExitEvent;
                    break;
                case "查看评论":
                    (infoFrame.Content as CommentPage).BackEvent += MainPage_BackEvent;
                    break;
                case "搜索结果":
                    (infoFrame.Content as SearchPage).BackEvent += MainPage_BackEvent;
                    break;
                case "收藏夹":
                    (infoFrame.Content as FavPage).BackEvent += MainPage_BackEvent;
                    break;
                case "设置":
                    (infoFrame.Content as SettingPage).BackEvent += MainPage_BackEvent;
                    (infoFrame.Content as SettingPage).ChangeTheme += MainPage_ChangeTheme;
                    (infoFrame.Content as SettingPage).ChangeDrak += MainPage_ChangeDrak;
                    (infoFrame.Content as SettingPage).Feedback += delegate {
                        Feedback();
                    };
                    break;
                case "播放器":
                    (infoFrame.Content as PlayerPage).BackEvent += MainPage_BackEvent;
                    break;
                case "历史":
                    (infoFrame.Content as HistoryPage).BackEvent += MainPage_BackEvent;
                    break;
                case "番剧":
                    (infoFrame.Content as FJPage).BackEvent += MainPage_BackEvent;
                    break;
                case "动画":
                    (infoFrame.Content as DHPage).BackEvent += MainPage_BackEvent;
                    break;
                case "音乐":
                    (infoFrame.Content as YYPage).BackEvent += MainPage_BackEvent;
                    break;
                case "舞蹈":
                    (infoFrame.Content as WDPage).BackEvent += MainPage_BackEvent;
                    break;
                case "科技":
                    (infoFrame.Content as KJPage).BackEvent += MainPage_BackEvent;
                    break;
                case "游戏":
                    (infoFrame.Content as YXPage).BackEvent += MainPage_BackEvent;
                    break;
                case "鬼畜":
                    (infoFrame.Content as GCPage).BackEvent += MainPage_BackEvent;
                    break;
                case "娱乐":
                    (infoFrame.Content as YLPage).BackEvent += MainPage_BackEvent;
                    break;
                case "电影":
                    (infoFrame.Content as DYPage).BackEvent += MainPage_BackEvent;
                    break;
                case "电视剧":
                    (infoFrame.Content as DSJPage).BackEvent += MainPage_BackEvent;
                    break;
                case "时尚":
                    (infoFrame.Content as SSPage).BackEvent += MainPage_BackEvent;
                    break;
                case "消息中心":
                    (infoFrame.Content as MessagePage).BackEvent += MainPage_BackEvent;
                    break;
                case "全部直播":
                    (infoFrame.Content as AllLivePage).BackEvent += MainPage_BackEvent;
                    break;
                case "直播间":
                    (infoFrame.Content as LiveInfoPage).BackEvent += MainPage_BackEvent;
                    break;
                case "dili":
                    (infoFrame.Content as DiliDiliPage).BackEvent += MainPage_BackEvent;
                    break;
                case "dili-info":
                    (infoFrame.Content as DiliInfo).BackEvent += MainPage_BackEvent;
                    break;
                case "搜索直播":
                    (infoFrame.Content as SearchLivePage).BackEvent += MainPage_BackEvent;
                    break;
                default:
                    break;
            }
        }

        private void MainPage_ExitEvent()
        {
            tuic.To = this.ActualWidth;
            storyboardPopOut.Begin();
            GetLoadInfo();
        }

        //主题更换
        private void MainPage_ChangeTheme()
        {
            ChangeTheme();
        }

        //试试手气
        private void Find_btn_Random_Click(object sender, RoutedEventArgs e)
        {
            infoFrame.Navigate(typeof(VideoInfoPage), new Random().Next(2000000, 4999999).ToString());
            //jinr.From = this.ActualWidth;
            //storyboardPopIn.Begin();
        }
        //infoFrame跳转动画
        private void infoFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            dh.TranslateX = 0;
            EdgeUIThemeTransition edge = new EdgeUIThemeTransition();
            if (e.NavigationMode == NavigationMode.New)
            {
                edge.Edge = EdgeTransitionLocation.Right;
                TransitionCollection tc = new TransitionCollection();
                tc.Add(edge);
                infoFrame.ContentTransitions = tc;
            }
        }
        //点击排行榜
        private void Find_btn_Rank_Click(object sender, RoutedEventArgs e)
        {
            infoFrame.Navigate(typeof(RankPage));
        }
        //番剧时间表点击
        private void list_0_ItemClick(object sender, ItemClickEventArgs e)
        {
            infoFrame.Navigate(typeof(BanInfoPage), (e.ClickedItem as BangumiTimeLineModel).season_id);
        }
        int taday = 0;
        public void SetWeekInfo()
        {
            date_2.Text = DateTime.Now.AddDays(-2).Date.Month + "月" + DateTime.Now.AddDays(-2).Date.Day + "日";
            date_3.Text = DateTime.Now.AddDays(-3).Date.Month + "月" + DateTime.Now.AddDays(-3).Date.Day + "日";
            date_4.Text = DateTime.Now.AddDays(-4).Date.Month + "月" + DateTime.Now.AddDays(-4).Date.Day + "日";
            date_5.Text = DateTime.Now.AddDays(-5).Date.Month + "月" + DateTime.Now.AddDays(-5).Date.Day + "日";
            date_6.Text = DateTime.Now.AddDays(-6).Date.Month + "月" + DateTime.Now.AddDays(-6).Date.Day + "日";

            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    week_0.Text = "周一";
                    week_1.Text = "周日";
                    week_2.Text = "周六";
                    week_3.Text = "周五";
                    week_4.Text = "周四";
                    week_5.Text = "周三";
                    week_5.Text = "周二";
                    taday = 1;
                    break;
                case DayOfWeek.Tuesday:
                    week_1.Text = "周一";
                    week_2.Text = "周日";
                    week_3.Text = "周六";
                    week_4.Text = "周五";
                    week_5.Text = "周四";
                    week_6.Text = "周三";
                    week_0.Text = "周二";
                    taday = 2;
                    break;
                case DayOfWeek.Wednesday:
                    week_2.Text = "周一";
                    week_3.Text = "周日";
                    week_4.Text = "周六";
                    week_5.Text = "周五";
                    week_6.Text = "周四";
                    week_0.Text = "周三";
                    week_1.Text = "周二";
                    taday = 3;
                    break;
                case DayOfWeek.Thursday:
                    week_3.Text = "周一";
                    week_4.Text = "周日";
                    week_5.Text = "周六";
                    week_6.Text = "周五";
                    week_0.Text = "周四";
                    week_1.Text = "周三";
                    week_2.Text = "周二";
                    taday = 4;
                    break;
                case DayOfWeek.Friday:
                    week_4.Text = "周一";
                    week_5.Text = "周日";
                    week_6.Text = "周六";
                    week_0.Text = "周五";
                    week_1.Text = "周四";
                    week_2.Text = "周三";
                    week_3.Text = "周二";
                    taday = 5;
                    break;
                case DayOfWeek.Saturday:
                    week_5.Text = "周一";
                    week_6.Text = "周日";
                    week_0.Text = "周六";
                    week_1.Text = "周五";
                    week_2.Text = "周四";
                    week_3.Text = "周三";
                    week_4.Text = "周二";
                    taday = 6;
                    break;
                case DayOfWeek.Sunday:
                    week_6.Text = "周一";
                    week_0.Text = "周日";
                    week_1.Text = "周六";
                    week_2.Text = "周五";
                    week_3.Text = "周四";
                    week_4.Text = "周三";
                    week_5.Text = "周二";
                    taday = 0;
                    break;
                default:
                    break;
            }
        }
        //时间表
        public async void GetBangumiTimeLine()
        {
            try
            {
                pr_Load_Ban.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://app.bilibili.com/bangumi/timeline_v2"));
                BangumiTimeLineModel model = new BangumiTimeLineModel();
                model = JsonConvert.DeserializeObject<BangumiTimeLineModel>(results);
                List<BangumiTimeLineModel> ban = JsonConvert.DeserializeObject<List<BangumiTimeLineModel>>(model.list.ToString());
                list_0.Items.Clear();
                list_1.Items.Clear();
                list_2.Items.Clear();
                list_3.Items.Clear();
                list_4.Items.Clear();
                list_5.Items.Clear();
                list_6.Items.Clear();
                list_7.Items.Clear();
                foreach (BangumiTimeLineModel item in ban)
                {
                    switch (item.weekday)
                    {
                        case -1:
                            list_7.Items.Add(item);
                            break;
                        case 0:
                            switch (taday)
                            {
                                case 0:
                                    list_0.Items.Add(item);
                                    break;
                                case 1:
                                    list_1.Items.Add(item);
                                    break;
                                case 2:
                                    list_2.Items.Add(item);
                                    break;
                                case 3:
                                    list_3.Items.Add(item);
                                    break;
                                case 4:
                                    list_4.Items.Add(item);
                                    break;
                                case 5:
                                    list_5.Items.Add(item);
                                    break;
                                case 6:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 1:
                            switch (taday)
                            {
                                case 1:
                                    list_0.Items.Add(item);
                                    break;
                                case 2:
                                    list_1.Items.Add(item);
                                    break;
                                case 3:
                                    list_2.Items.Add(item);
                                    break;
                                case 4:
                                    list_3.Items.Add(item);
                                    break;
                                case 5:
                                    list_4.Items.Add(item);
                                    break;
                                case 6:
                                    list_5.Items.Add(item);
                                    break;
                                case 0:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 2:
                            switch (taday)
                            {
                                case 2:
                                    list_0.Items.Add(item);
                                    break;
                                case 3:
                                    list_1.Items.Add(item);
                                    break;
                                case 4:
                                    list_2.Items.Add(item);
                                    break;
                                case 5:
                                    list_3.Items.Add(item);
                                    break;
                                case 6:
                                    list_4.Items.Add(item);
                                    break;
                                case 0:
                                    list_5.Items.Add(item);
                                    break;
                                case 1:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 3:
                            switch (taday)
                            {
                                case 3:
                                    list_0.Items.Add(item);
                                    break;
                                case 4:
                                    list_1.Items.Add(item);
                                    break;
                                case 5:
                                    list_2.Items.Add(item);
                                    break;
                                case 6:
                                    list_3.Items.Add(item);
                                    break;
                                case 0:
                                    list_4.Items.Add(item);
                                    break;
                                case 1:
                                    list_5.Items.Add(item);
                                    break;
                                case 2:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 4:
                            switch (taday)
                            {
                                case 4:
                                    list_0.Items.Add(item);
                                    break;
                                case 5:
                                    list_1.Items.Add(item);
                                    break;
                                case 6:
                                    list_2.Items.Add(item);
                                    break;
                                case 0:
                                    list_3.Items.Add(item);
                                    break;
                                case 1:
                                    list_4.Items.Add(item);
                                    break;
                                case 2:
                                    list_5.Items.Add(item);
                                    break;
                                case 3:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 5:
                            switch (taday)
                            {
                                case 5:
                                    list_0.Items.Add(item);
                                    break;
                                case 6:
                                    list_1.Items.Add(item);
                                    break;
                                case 0:
                                    list_2.Items.Add(item);
                                    break;
                                case 1:
                                    list_3.Items.Add(item);
                                    break;
                                case 2:
                                    list_4.Items.Add(item);
                                    break;
                                case 3:
                                    list_5.Items.Add(item);
                                    break;
                                case 4:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 6:
                            switch (taday)
                            {
                                case 6:
                                    list_0.Items.Add(item);
                                    break;
                                case 0:
                                    list_1.Items.Add(item);
                                    break;
                                case 1:
                                    list_2.Items.Add(item);
                                    break;
                                case 2:
                                    list_3.Items.Add(item);
                                    break;
                                case 3:
                                    list_4.Items.Add(item);
                                    break;
                                case 4:
                                    list_5.Items.Add(item);
                                    break;
                                case 5:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                }
                pr_Load_Ban.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog("读取番剧更新失败\r\n" + ex.Message);
                await md.ShowAsync();
            }
        }
        //索引
        public async void GetTagInfo()
        {
            try
            {
                pr_Load_Ban.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string uri = "http://bangumi.bilibili.com/api/tags?_device=wp&_ulv=10000&appkey=422fd9d7289a1dd9&build=411005&page=" + 1 + "&pagesize=60&platform=android&ts=" + ApiHelper.GetTimeSpen + "000";
                string sign = ApiHelper.GetSign(uri);
                uri += "&sign=" + sign;
                string results = await wc.GetResults(new Uri(uri));
                JObject jo = JObject.Parse(results);
                List<TagModel> list = JsonConvert.DeserializeObject<List<TagModel>>(jo["result"].ToString());
                gridview_List.ItemsSource = list;
            }
            catch (Exception ex)
            {
                await new MessageDialog("读取索引信息失败！\r\n" + ex.Message).ShowAsync();
            }
            finally
            {
                pr_Load_Ban.Visibility = Visibility.Collapsed;
                // IsLoading = false;
            }
        }
        //番剧时间表点击
        private void Ban_btn_Timeline_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActualWidth > 500)
            {
                B_Timeline.Visibility = Visibility.Visible;
                gridview_List.Visibility = Visibility.Collapsed;
                sp_Bangumi.IsPaneOpen = true;
                GetBangumiTimeLine();
            }
            else
            {
                infoFrame.Navigate(typeof(BanTimelinePage));
            }
        }
        //索引点击
        private void gridview_List_ItemClick(object sender, ItemClickEventArgs e)
        {
            infoFrame.Navigate(typeof(BanByTagPage), new string[] { (e.ClickedItem as TagModel).tag_id.ToString(), (e.ClickedItem as TagModel).tag_name });
        }
        //索引表点击
        private void Ban_btn_Tag_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActualWidth > 500)
            {
                B_Timeline.Visibility = Visibility.Collapsed;
                gridview_List.Visibility = Visibility.Visible;
                sp_Bangumi.IsPaneOpen = true;
                GetTagInfo();
            }
            else
            {
                infoFrame.Navigate(typeof(BanTagPage));
            }
        }
        //追番点击
        private void user_GridView_Bangumi_ItemClick(object sender, ItemClickEventArgs e)
        {
            infoFrame.Navigate(typeof(BanInfoPage), (e.ClickedItem as GetUserBangumi).season_id);
        }
        //番剧最近更新
        bool LoadBan = false;
        private async Task GetBanUpdate()
        {
            try
            {
                wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://bangumi.bilibili.com/api/app_index_page"));
                BannumiIndexModel model = JsonConvert.DeserializeObject<BannumiIndexModel>(results);
                JObject json = JObject.Parse(model.result.ToString());
                List<BannumiIndexModel> ban = JsonConvert.DeserializeObject<List<BannumiIndexModel>>(json["latestUpdate"]["list"].ToString());
                GridView_Bangumi_NewUpdate.ItemsSource = ban;
                // LoadBan = true;
            }
            catch (Exception ex)
            {
                messShow.Show("读取番剧最近更新失败\r\n" + ex.Message, 3000);
                //LoadBan = false;
            }
        }
        //番剧更新点击
        private void GridView_Bangumi_NewUpdate_ItemClick(object sender, ItemClickEventArgs e)
        {
            infoFrame.Navigate(typeof(BanInfoPage), (e.ClickedItem as BannumiIndexModel).season_id);
        }
        //读取我的追番
        private void Ban_btn_MyBan_Click(object sender, RoutedEventArgs e)
        {
            if (new UserClass().IsLogin())
            {
                infoFrame.Navigate(typeof(UserBangumiPage), UserClass.Uid);
            }
            else
            {
                messShow.Show("请先登录", 3000);
            }
        }
        //读取番剧Banner
        private async Task GetBanBanner()
        {
            try
            {
                wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://bangumi.bilibili.com/api/app_index_page_v2?rnd" + new Random().Next(1000, 9999)));
                BanBannerModel model = JsonConvert.DeserializeObject<BanBannerModel>(results);
                if (model.code == 0)
                {
                    JObject jo = JObject.Parse(results);
                    List<BanBannerModel> list = JsonConvert.DeserializeObject<List<BanBannerModel>>(jo["result"]["banners"].ToString());
                    home_flipView_Ban.ItemsSource = list;
                    fvLeft_Ban.ItemsSource = list;
                    fvRight_Ban.ItemsSource = list;
                    this.home_flipView_Ban.SelectedIndex = 0;
                    if (fvLeft_Ban.Visibility != Visibility.Collapsed || fvRight_Ban.Visibility != Visibility.Collapsed)
                    {
                        this.fvLeft_Ban.SelectedIndex = this.fvLeft_Ban.Items.Count - 1;
                        this.fvRight_Ban.SelectedIndex = this.home_flipView_Ban.SelectedIndex + 1;
                    }
                }
                else
                {
                    messShow.Show("读取番剧Banner失败！" + model.message, 3000);
                }
            }
            catch (Exception ex)
            {
                messShow.Show("读取番剧Banner失败！" + ex.Message, 3000);
                //throw;
            }

        }
        //读取番剧推荐
        string Page_BanTJ = "-1";
        private async Task GetBanTJ()
        {
            try
            {
                Ban_TJ_more.Text = "正在加载...";
                wc = new WebClientClass();
                string uri = "http://bangumi.bilibili.com/api/bangumi_recommend?_device=wp&appkey=422fd9d7289a1dd9&build=411005&cursor=" + Page_BanTJ + "&pagesize=10&platform=android&ts=" + ApiHelper.GetTimeSpen;
                uri += "&sign=" + ApiHelper.GetSign(uri);
                string results = await wc.GetResults(new Uri(uri));
                BanTJModel model = JsonConvert.DeserializeObject<BanTJModel>(results);
                if (model.code == 0)
                {
                    JObject jo = JObject.Parse(results);
                    List<BanTJModel> list = JsonConvert.DeserializeObject<List<BanTJModel>>(model.result.ToString());
                    foreach (BanTJModel item in list)
                    {
                        list_Ban_TJ.Items.Add(item);
                    }
                    if (list.Count != 0)
                    {
                        Page_BanTJ = (list[list.Count - 1] as BanTJModel).cursor;
                        Ban_TJ_more.Text = "加载更多";
                    }
                    else
                    {
                        Ban_TJ_more.Text = "没有更多了...";
                    }
                }
                else
                {
                    messShow.Show("读取番剧推荐失败！" + model.message, 3000);
                }

            }
            catch (Exception ex)
            {
                messShow.Show("读取番剧推荐失败！" + ex.Message, 3000);
                // throw;
            }

        }
        //番剧Banner点击
        private void btn_Banner_Ban_Click(object sender, RoutedEventArgs e)
        {
            string tag = Regex.Match((home_flipView_Ban.SelectedItem as BanBannerModel).link, @"^http://bangumi.bilibili.com/anime/category/(.*?)$").Groups[1].Value;
            if (tag.Length != 0)
            {
                infoFrame.Navigate(typeof(BanByTagPage), new string[] { tag, (home_flipView_Ban.SelectedItem as BanBannerModel).title });
                return;
            }
            string ban = Regex.Match((home_flipView_Ban.SelectedItem as BanBannerModel).link, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
            if (ban.Length != 0)
            {
                infoFrame.Navigate(typeof(BanInfoPage), ban);
                return;
            }
            infoFrame.Navigate(typeof(WebViewPage), (home_flipView_Ban.SelectedItem as BanBannerModel).link);
        }
        //番剧推荐加载更多
        bool LoadBaning = false;
        private async void sc_Ban_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sc_Ban.VerticalOffset == sc_Ban.ScrollableHeight)
            {
                if (!LoadBaning && Ban_TJ_more.Text != "没有更多了...")
                {
                    LoadBaning = true;
                    await GetBanTJ();
                    LoadBaning = false;
                }
            }
        }
        //番剧推荐点击
        private void list_Ban_TJ_ItemClick(object sender, ItemClickEventArgs e)
        {
            //妈蛋，B站就一定要返回个链接么,就不能返回个类型加参数吗
            string tag = Regex.Match((e.ClickedItem as BanTJModel).link, @"^http://bangumi.bilibili.com/anime/category/(.*?)$").Groups[1].Value;
            if (tag.Length != 0)
            {
                infoFrame.Navigate(typeof(BanByTagPage), new string[] { tag, (e.ClickedItem as BanTJModel).title });
                return;
            }
            string ban = Regex.Match((e.ClickedItem as BanTJModel).link, @"^http://bangumi.bilibili.com/anime/(.*?)$").Groups[1].Value;
            if (ban.Length != 0)
            {
                infoFrame.Navigate(typeof(BanInfoPage), ban);
                return;
            }
            //
            string aid = Regex.Match((e.ClickedItem as BanTJModel).link, @"^http://www.bilibili.com/video/av(.*?)/$").Groups[1].Value;
            if (aid.Length != 0)
            {
                infoFrame.Navigate(typeof(VideoInfoPage), aid);
                return;
            }
            infoFrame.Navigate(typeof(WebViewPage), (e.ClickedItem as BanTJModel).link);
        }
        //搜索
        private void txt_auto_Find_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender.Text.Length == 0)
            {
                //top_txt_find.Visibility = Visibility.Collapsed;
                //top_btn_find.Visibility = Visibility.Visible;
                //mainFrame.Navigate(typeof(SeasonPage));
            }
            else
            {
                this.infoFrame.Navigate(typeof(SearchPage), txt_auto_Find.Text);
            }
        }
        //搜索文字改变
        private async void txt_auto_Find_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text.Length != 0)
            {
                sender.ItemsSource = await GetSugges(sender.Text);
            }
            else
            {
                sender.ItemsSource = null;
            }
        }
        //搜索点击待选框
        private void txt_auto_Find_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            txt_auto_Find.Text = args.SelectedItem as string;
        }

        public async Task<ObservableCollection<String>> GetSugges(string text)
        {
            try
            {
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://s.search.bilibili.com/main/suggest?suggest_type=accurate&sub_type=tag&main_ver=v1&term=" + text));
                JObject json = JObject.Parse(results);
                // json["result"]["tag"].ToString();
                List<SuggesModel> list = JsonConvert.DeserializeObject<List<SuggesModel>>(json["result"]["tag"].ToString());
                ObservableCollection<String> suggestions = new ObservableCollection<string>();
                foreach (SuggesModel item in list)
                {
                    suggestions.Add(item.value);
                }
                return suggestions;
            }
            catch (Exception)
            {
                return new ObservableCollection<string>();
            }

        }
        public class SuggesModel
        {
            public string name { get; set; }
            public string value { get; set; }
        }
        //话题点击
        private void list_Topic_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Regex.IsMatch(((TopicModel)e.ClickedItem).link, "/video/av(.*)?[/|+](.*)?"))
            {
                string a = Regex.Match(((TopicModel)e.ClickedItem).link, "/video/av(.*)?[/|+](.*)?").Groups[1].Value;
                this.infoFrame.Navigate(typeof(VideoInfoPage), a);
            }
            else
            {
                if (Regex.IsMatch(((TopicModel)e.ClickedItem).link, @"live.bilibili.com/(.*?)"))
                {
                    string a = Regex.Match(((TopicModel)e.ClickedItem).link + "a", "live.bilibili.com/(.*?)a").Groups[1].Value;
                    // livePlayVideo(a);
                }
                else
                {
                    this.infoFrame.Navigate(typeof(WebViewPage), ((TopicModel)e.ClickedItem).link);
                }
            }
        }

        private async void top_txt_find_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.Text.Length != 0)
            {
                sender.ItemsSource = await GetSugges(sender.Text);
            }
            else
            {
                sender.ItemsSource = null;
            }
        }

        private void top_txt_find_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender.PlaceholderText != "搜索关键字或AV号")
            {
                return;
            }
            if (sender.Text.Length == 0)
            {
                //top_txt_find.Visibility = Visibility.Collapsed;
                //top_btn_find.Visibility = Visibility.Visible;
                //mainFrame.Navigate(typeof(SeasonPage));
            }
            else
            {
                infoFrame.Navigate(typeof(SearchPage), top_txt_find.Text);
            }
        }

        private void top_txt_find_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            top_txt_find.Text = args.SelectedItem as string;
        }
        //搜索热词点击
        private void list_Hot_ItemClick(object sender, ItemClickEventArgs e)
        {
            infoFrame.Navigate(typeof(SearchPage), (e.ClickedItem as HotModel).keyword);
        }
        //dilidili点击
        private void btn_dilidili_Click(object sender, RoutedEventArgs e)
        {
            infoFrame.Navigate(typeof(DiliDiliPage));
        }
        //打开直播关注
        private async void btn_live_Atton_Click(object sender, RoutedEventArgs e)
        {
            UserClass UserInfo = new UserClass();
            if (UserInfo.IsLogin())
            {
                grid_AttenLive.Visibility = Visibility.Visible;
                sp_Live.IsPaneOpen = true;
                pro_Load_live.Visibility = Visibility.Visible;
                list_AttLive.ItemsSource = await new UserClass().GetAttentionLive();
                pro_Load_live.Visibility = Visibility.Collapsed;
            }
            else
            {
                messShow.Show("请先登录!", 3000);
            }


        }
        private void list_AttLive_ItemClick(object sender, ItemClickEventArgs e)
        {
            infoFrame.Navigate(typeof(LiveInfoPage),(e.ClickedItem as GetAttentionLive).roomid);
        }
        //刷新动态
        private void btn_refresh_Atton_Click(object sender, RoutedEventArgs e)
        {
            DT_PageNum = 1;
            User_ListView_Attention.Items.Clear();
            GetDt();
        }
        //打开全部直播
        private void btn_live_All_Click(object sender, RoutedEventArgs e)
        {
            infoFrame.Navigate(typeof(AllLivePage));
        }
        //读取直播Banner
        private async void GetLiveBanner()
        {
            wc = new WebClientClass();
            string url = string.Format("http://live.bilibili.com/AppIndex/home?_device=wp&_ulv=10000&access_key={0}&appkey={1}&build=411005&platform=android&scale=xxhdpi", ApiHelper.access_key, ApiHelper._appKey);
            url += "&sign=" + ApiHelper.GetSign(url);
            string results = await wc.GetResults(new Uri(url));
            HomeLiveModel model = JsonConvert.DeserializeObject<HomeLiveModel>(results);
            if (model.code == 0)
            {
                HomeLiveModel dataModel = JsonConvert.DeserializeObject<HomeLiveModel>(model.data.ToString());
                List<HomeLiveModel> bannerModel = JsonConvert.DeserializeObject<List<HomeLiveModel>>(dataModel.banner.ToString());
                home_flipView_Live.ItemsSource = bannerModel;
                fvLeft_Live.ItemsSource = bannerModel;
                fvRight_Live.ItemsSource = bannerModel;
                this.home_flipView_Live.SelectedIndex = 0;
                try
                {
                    if (fvLeft_Live.Visibility != Visibility.Collapsed || fvRight_Live.Visibility != Visibility.Collapsed)
                    {
                        this.fvLeft_Live.SelectedIndex = this.fvLeft_Live.Items.Count - 1;
                        this.fvRight_Live.SelectedIndex = this.home_flipView_Live.SelectedIndex + 1;
                    }
                }
                catch (Exception)
                {
                }
                
            }
        }
        //直播Banner点击
        private void btn_Banner_Live_Click(object sender, RoutedEventArgs e)
        {
            string ban = Regex.Match((home_flipView_Live.SelectedItem as HomeLiveModel).link, @"^bilibili://live/(.*?)$").Groups[1].Value;
            if (ban.Length != 0)
            {
                infoFrame.Navigate(typeof(LiveInfoPage), ban);
                return;
            }
        }
        //直播Banner选择改变
        private void home_flipView_Live_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (home_flipView_Live.Items.Count == 0 || fvLeft_Live.Items.Count == 0 || fvRight_Live.Items.Count == 0)
            {
                return;
            }
            if (fvLeft_Live.Visibility == Visibility.Collapsed || fvRight_Live.Visibility == Visibility.Collapsed)
            {
                return;
            }
            if (this.home_flipView_Live.SelectedIndex == 0)
            {
                this.fvLeft_Live.SelectedIndex = this.fvLeft_Live.Items.Count - 1;
                this.fvRight_Live.SelectedIndex = 1;
            }
            else if (this.home_flipView_Live.SelectedIndex == 1)
            {
                this.fvLeft_Live.SelectedIndex = 0;
                this.fvRight_Live.SelectedIndex = this.fvRight_Live.Items.Count - 1;
            }
            else if (this.home_flipView_Live.SelectedIndex == this.home_flipView_Live.Items.Count - 1)
            {
                this.fvLeft_Live.SelectedIndex = this.fvLeft_Live.Items.Count - 2;
                this.fvRight_Live.SelectedIndex = 0;
            }
            else if ((this.home_flipView_Live.SelectedIndex < (this.home_flipView_Live.Items.Count - 1)) && this.home_flipView_Live.SelectedIndex > -1)
            {
                this.fvLeft_Live.SelectedIndex = this.home_flipView_Live.SelectedIndex - 1;
                this.fvRight_Live.SelectedIndex = this.home_flipView_Live.SelectedIndex + 1;
            }
            else
            {
                return;
            }
        }



        #region 九幽反馈
        private UserInfo _userInfo;
        private readonly JyUserFeedbackSDKManager _jyUserFeedbackSdkManager = new JyUserFeedbackSDKManager();
        private async void Feedback()
        {
            if (_userInfo==null)
            {
                var userInfo = await JyUserInfoManager.QuickLogin(ApiHelper.JyAppkey);
                if (userInfo.isLoginSuccess)
                {
                    _userInfo = userInfo;
                    _jyUserFeedbackSdkManager.ShowFeedBack(Feedback_Grid, false, ApiHelper.JyAppkey, _userInfo.U_Key);
                    bor_HasFeedback.Visibility = Visibility.Collapsed;
                }
                else
                {
                    await new MessageDialog("打开反馈失败，请重试").ShowAsync();
                }
            }
            else
            {
                _jyUserFeedbackSdkManager.ShowFeedBack(Feedback_Grid, false, ApiHelper.JyAppkey, _userInfo.U_Key);
            }
        }
        private async void GetFeedInfo()
        {
        
            var userInfo = await JyUserInfoManager.QuickLogin(ApiHelper.JyAppkey);
            if (userInfo.isLoginSuccess)
            {
                _userInfo = userInfo;
                var newFeedBackRemindCount = await _jyUserFeedbackSdkManager.GetNewFeedBackRemindCount(ApiHelper.JyAppkey, _userInfo.U_Key);
                if (newFeedBackRemindCount!=0)
                {
                    bor_HasFeedback.Visibility = Visibility.Visible;
                }
                else
                {
                    bor_HasFeedback.Visibility = Visibility.Collapsed;
                }
            }

        }
        #endregion

        #region 下拉刷新
        private void pr_Live_RefreshInvoked(DependencyObject sender, object args)
        {
            GetLiveBanner();
            liveinfo.GetLiveInfo();
        }

        private async void pr_Bangumi_RefreshInvoked(DependencyObject sender, object args)
        {
            LoadBan = false;
            await GetBanUpdate();
            await GetBanBanner();
            await GetBanTJ();
            LoadBan = true;
        }

        private void pr_Home_RefreshInvoked(DependencyObject sender, object args)
        {
            SetHomeInfo();
            home_Items.SetHomeInfo();
        }

        private void pr_Atton_RefreshInvoked(DependencyObject sender, object args)
        {
            DT_PageNum = 1;
            User_ListView_Attention.Items.Clear();
            GetDt();
        }

        #endregion

        private void btn_Live_Search_Click(object sender, RoutedEventArgs e)
        {
            infoFrame.Navigate(typeof(SearchLivePage));
        }
    }
}
