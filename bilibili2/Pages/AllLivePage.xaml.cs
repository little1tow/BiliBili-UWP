using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class AllLivePage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public AllLivePage()
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                ViewBox_num.Width = ActualWidth / 2 - 20;
            }
            else
            {
                int i = Convert.ToInt32(ActualWidth / 200);
                ViewBox_num.Width = ActualWidth / i-15;
                //ViewBox2_num.Width = 200;
            }

        }

        bool Hot = false;
        bool Yz = false;
        bool Sh = false;
        bool Dj = false;
        bool Wl = false;
        bool dzjz = false;
        bool Dy = false;

        int PageNum = 1;
        public async void GetZBInfo()
        {
            try
            {
                CanLoad = false;
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://api.bilibili.com/live/room_list?page=" + PageNum + "&pagesize=30&status=LIVE"));
                InfoModel model = new InfoModel();
                model = JsonConvert.DeserializeObject<InfoModel>(results);
                JObject json = JObject.Parse(model.list.ToString());
                List<InfoModel> ReList = new List<InfoModel>();
                for (int i = 0; i < 20; i++)
                {
                    live_HOT.Items.Add(new InfoModel
                    {
                        room_id = (string)json[i.ToString()]["room_id"],
                        title = (string)json[i.ToString()]["title"],
                        cover = (string)json[i.ToString()]["cover"],
                        uname = (string)json[i.ToString()]["uname"],
                        online = (string)json[i.ToString()]["online"],
                        face = (string)json[i.ToString()]["face"],
                    });
                }
                Hot = true;
                CanLoad = true;
                PageNum++;
            }
            catch (Exception)
            {
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        public async void GetYz()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://live.bilibili.com/otaku?page=1&ajax=1"));
                InfoModel data = JsonConvert.DeserializeObject<InfoModel>(results);
                //JObject json = JObject.Parse(results);
                List<InfoModel> model = JsonConvert.DeserializeObject<List<InfoModel>>(data.data.ToString());
                foreach (InfoModel item in model)
                {
                    live_Yz.Items.Add(item);
                }
                Yz = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        public async void GetSh()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://live.bilibili.com/ent-life?page=1&ajax=1"));
                InfoModel data = JsonConvert.DeserializeObject<InfoModel>(results);
                //JObject json = JObject.Parse(results);
                List<InfoModel> model = JsonConvert.DeserializeObject<List<InfoModel>>(data.data.ToString());
                foreach (InfoModel item in model)
                {
                    live_Sh.Items.Add(item);
                }
                Sh = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        public async void GetDj()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://live.bilibili.com/single?page=1&ajax=1"));
                InfoModel data = JsonConvert.DeserializeObject<InfoModel>(results);
                //JObject json = JObject.Parse(results);
                List<InfoModel> model = JsonConvert.DeserializeObject<List<InfoModel>>(data.data.ToString());
                foreach (InfoModel item in model)
                {
                    live_Dj.Items.Add(item);
                }
                Dj = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        public async void GetWl()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://live.bilibili.com/online?page=1&ajax=1"));
                InfoModel data = JsonConvert.DeserializeObject<InfoModel>(results);
                //JObject json = JObject.Parse(results);
                List<InfoModel> model = JsonConvert.DeserializeObject<List<InfoModel>>(data.data.ToString());
                foreach (InfoModel item in model)
                {
                    live_Wl.Items.Add(item);
                }
                Wl = true;
            }
            catch (Exception)
            {
            }
            finally {
                pr_Load.Visibility = Visibility.Collapsed;
            }
            
        }

        public async void GetDzj()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://live.bilibili.com/e-sports?page=1&ajax=1"));
                InfoModel data = JsonConvert.DeserializeObject<InfoModel>(results);
                //JObject json = JObject.Parse(results);
                List<InfoModel> model = JsonConvert.DeserializeObject<List<InfoModel>>(data.data.ToString());
                foreach (InfoModel item in model)
                {
                    live_Dzj.Items.Add(item);
                }
                dzjz = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        public async void GetDy()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://live.bilibili.com/movie?page=1&ajax=1"));
                InfoModel data = JsonConvert.DeserializeObject<InfoModel>(results);
                //JObject json = JObject.Parse(results);
                List<InfoModel> model = JsonConvert.DeserializeObject<List<InfoModel>>(data.data.ToString());
                foreach (InfoModel item in model)
                {
                    live_Dy.Items.Add(item);
                }
                Dy = true;
            }
            catch (Exception)
            {
            }
            finally {
                pr_Load.Visibility = Visibility.Collapsed;
            }

        }

        private void live_HOT_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(LiveInfoPage), ((InfoModel)e.ClickedItem).room_id);
            // this.Frame.Navigate(typeof(LivePlayerPage), ((InfoModel)e.ClickedItem).room_id);
            //livePlayVideo(((InfoModel)e.ClickedItem).room_id);
        }

        bool CanLoad = true;
        private void Sc_Live_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (Sc_Live.VerticalOffset == Sc_Live.ScrollableHeight)
            {
                if (CanLoad)
                {
                    GetZBInfo();
                }
            }
        }

        private void Sc_Yz_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    if (!Hot)
                    {
                        GetZBInfo();
                    }
                    break;
                case 1:
                    if (!Yz)
                    {
                        GetYz();
                    }
                    break;
                case 2:
                    if (!Sh)
                    {
                        GetSh();
                    }
                    break;
                case 3:
                    if (!Dj)
                    {
                        GetDj();
                    }
                    break;
                case 4:
                    if (!Wl)
                    {
                        GetWl();
                    }
                    break;
                case 5:
                    if (!dzjz)
                    {
                        GetDzj();
                    }
                    break;
                case 6:
                    if (!Dy)
                    {
                        GetDy();
                    }
                    break;
                default:
                    break;
            }
        }

        private void btn_Go_Click(object sender, RoutedEventArgs e)
        {

            //this.Frame.Navigate(typeof(LivePlayerPage), txt_RoomID);
        }

        private void Pull_RefreshInvoked(DependencyObject sender, object args)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    PageNum = 1;
                    live_HOT.Items.Clear();
                    GetZBInfo();
                    break;
                case 1:
                    live_Yz.Items.Clear();
                    GetYz();
                    break;
                case 2:
                    live_Sh.Items.Clear();
                    GetSh();
                    break;
                case 3:
                    live_Dj.Items.Clear();
                    GetDj();
                    break;
                case 4:
                    live_Wl.Items.Clear();
                    GetWl();
                    break;
                case 5:
                    live_Dzj.Items.Clear();
                    GetDzj();
                    break;
                case 6:
                    live_Dy.Items.Clear();
                    GetDy();
                    break;
                default:
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    PageNum = 1;
                    live_HOT.Items.Clear();
                    GetZBInfo();
                    break;
                case 1:
                    live_Yz.Items.Clear();
                    GetYz();
                    break;
                case 2:
                    live_Sh.Items.Clear();
                    GetSh();
                    break;
                case 3:
                    live_Dj.Items.Clear();
                    GetDj();
                    break;
                case 4:
                    live_Wl.Items.Clear();
                    GetWl();
                    break;
                case 5:
                    live_Dzj.Items.Clear();
                    GetDzj();
                    break;
                case 6:
                    live_Dy.Items.Clear();
                    GetDy();
                    break;
                default:
                    break;
            }
        }

        private void live_Dzj_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(LiveInfoPage), ((InfoModel)e.ClickedItem).roomid);
        }

    }
}
