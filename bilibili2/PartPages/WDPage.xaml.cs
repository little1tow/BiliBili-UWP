using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.PartPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WDPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public WDPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                ViewBox_num.Width = ActualWidth / 2 - 20;
                ViewBox2_num.Width = ActualWidth / 2 - 20;
            }
            else
            {
                int i = Convert.ToInt32(ActualWidth / 170);
                ViewBox_num.Width = ActualWidth / i - 15;
                ViewBox2_num.Width = ActualWidth / i - 15;
            }
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
            if (e.NavigationMode == NavigationMode.New)
            {
                PageNum = 1;
                GetDYHome();
                GetDT();
            }
        }

        private HttpClient hc;
        /// <summary>
        /// Banner，推荐，最新
        /// </summary>
        private async void GetDYHome()
        {
            try
            {
                pro_Bar.Visibility = Visibility.Visible;
                using (hc = new HttpClient())
                {
                    HttpResponseMessage hr = await hc.GetAsync(new Uri("http://app.bilibili.com/api/region2/129.json"));
                    hr.EnsureSuccessStatusCode();
                    var encodeResults = await hr.Content.ReadAsBufferAsync();
                    string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);
                    DHModel model = JsonConvert.DeserializeObject<DHModel>(results);
                    DHModel model2 = JsonConvert.DeserializeObject<DHModel>(model.result.ToString());
                    List<DHModel> BannerModel = JsonConvert.DeserializeObject<List<DHModel>>(model2.banners.ToString());
                    //List<DHModel> RecommendsModel = JsonConvert.DeserializeObject<List<DHModel>>(model2.recommends.ToString());
                    //List<DHModel> NewsModel = JsonConvert.DeserializeObject<List<DHModel>>(model2.news.ToString());
                    home_flipView.Items.Clear();
                    //GridView_TJ.Items.Clear();
                    //GridView_New.Items.Clear();
                    foreach (DHModel item in BannerModel)
                    {
                        if (item.aid != null || item.img != null)
                        {
                            home_flipView.Items.Add(item);
                        }
                    }
                    //GridView_TJ.ItemsSource = RecommendsModel;
                    //GridView_New.ItemsSource = NewsModel;
                }
                pro_Bar.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }
        }
        /// <summary>
        /// 动态
        /// </summary>
        int PageNum = 1;
        private async void GetDT()
        {
            pro_Bar.Visibility = Visibility.Visible;
            try
            {
                using (hc = new HttpClient())
                {
                    if (PageNum == 1)
                    {
                        GridView_DT.Items.Clear();
                    }
                    CanLoad = false;
                    HttpResponseMessage hr = await hc.GetAsync(new Uri("http://www.bilibili.com/index/ding/129.json?page=" + PageNum + "&pagesize=24&rnd=" + new Random().Next(1, 9999)));
                    hr.EnsureSuccessStatusCode();

                    // var encodeResults = await hr.Content.ReadAsBufferAsync();
                    string results = await hr.Content.ReadAsStringAsync();
                    DHModel model = JsonConvert.DeserializeObject<DHModel>(results);
                    List<DHModel> DTModel = JsonConvert.DeserializeObject<List<DHModel>>(model.list.ToString());
                    foreach (DHModel item in DTModel)
                    {
                        GridView_DT.Items.Add(item);
                    }
                    CanLoad = true;
                    pro_Bar.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);
                await md.ShowAsync();
            }
        }
        //加载
        int PageNum_LZ = 1;
        private async void GetLZNew(int order)
        {
            pro_Bar.Visibility = Visibility.Visible;
            try
            {
                CanLoad = false;
                using (hc = new HttpClient())
                {
                    #region
                    string uri = "";
                    switch (order)
                    {
                        case 0:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=20&page=" + PageNum_LZ + "&pagesize=50&order=default";
                            break;
                        case 1:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=20&page=" + PageNum_LZ + "&pagesize=50&order=damku";
                            break;
                        case 2:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=20&page=" + PageNum_LZ + "&pagesize=50&order=hot";
                            break;
                        case 3:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=20&page=" + PageNum_LZ + "&pagesize=50&order=review";
                            break;
                        case 4:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=20&page=" + PageNum_LZ + "&pagesize=50&order=stow";
                            break;
                        default:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=20&page=" + PageNum_LZ + "&pagesize=50&order=default";
                            break;
                    }
                    #endregion
                    if (PageNum_LZ == 1)
                    {
                        LZ_NewList.Items.Clear();
                    }
                    HttpResponseMessage hr = await hc.GetAsync(new Uri(uri));
                    hr.EnsureSuccessStatusCode();
                    string results = await hr.Content.ReadAsStringAsync();
                    DHModel model = JsonConvert.DeserializeObject<DHModel>(results);
                    JObject json = JObject.Parse(model.list.ToString());
                    List<DHModel> ReList = new List<DHModel>();
                    //LZ_NewList.Items.Clear();
                    for (int i = 0; i < 50; i++)
                    {
                        LZ_NewList.Items.Add(new DHModel
                        {
                            aid = (string)json[i.ToString()]["aid"],
                            title = (string)json[i.ToString()]["title"],
                            pic = (string)json[i.ToString()]["pic"],
                            author = (string)json[i.ToString()]["author"],
                            play = (string)json[i.ToString()]["play"],
                            video_review = (string)json[i.ToString()]["video_review"],
                        });
                    }
                    CanLoad = true;
                    PageNum_LZ++;
                    pro_Bar.Visibility = Visibility.Collapsed;
                    //LZ_NewList.ItemsSource = ReList;
                }

            }
            catch (Exception)
            {
                pro_Bar.Visibility = Visibility.Collapsed;
            }
        }
        int PageNum_WJ = 1;
        private async void GetWJList(int order)
        {
            pro_Bar.Visibility = Visibility.Visible;
            try
            {
                CanLoad = false;
                using (hc = new HttpClient())
                {
                    #region
                    string uri = "";
                    switch (order)
                    {
                        case 0:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=154&page=" + PageNum_WJ + "&pagesize=50&order=default";
                            break;
                        case 1:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=154&page=" + PageNum_WJ + "&pagesize=50&order=damku";
                            break;
                        case 2:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=154&page=" + PageNum_WJ + "&pagesize=50&order=hot";
                            break;
                        case 3:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=154&page=" + PageNum_WJ + "&pagesize=50&order=review";
                            break;
                        case 4:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=154&page=" + PageNum_WJ + "&pagesize=50&order=stow";
                            break;
                        default:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=154&page=" + PageNum_WJ + "&pagesize=50&order=default";
                            break;
                    }
                    #endregion
                    if (PageNum_WJ == 1)
                    {
                        WJ_NewList.Items.Clear();
                    }
                    HttpResponseMessage hr = await hc.GetAsync(new Uri(uri));
                    hr.EnsureSuccessStatusCode();
                    string results = await hr.Content.ReadAsStringAsync();
                    DHModel model = JsonConvert.DeserializeObject<DHModel>(results);
                    JObject json = JObject.Parse(model.list.ToString());
                    List<DHModel> ReList = new List<DHModel>();
                    for (int i = 0; i < 50; i++)
                    {
                        WJ_NewList.Items.Add(new DHModel
                        {
                            aid = (string)json[i.ToString()]["aid"],
                            title = (string)json[i.ToString()]["title"],
                            pic = (string)json[i.ToString()]["pic"],
                            author = (string)json[i.ToString()]["author"],
                            play = (string)json[i.ToString()]["play"],
                            video_review = (string)json[i.ToString()]["video_review"],
                        });
                    }
                    CanLoad = true;
                    PageNum_WJ++;
                    pro_Bar.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                pro_Bar.Visibility = Visibility.Collapsed;
            }
        }
        int PageNum_GC = 1;
        private async void GetGCList(int order)
        {
            pro_Bar.Visibility = Visibility.Visible;
            try
            {
                CanLoad = false;
                using (hc = new HttpClient())
                {
                    #region
                    string uri = "";
                    switch (order)
                    {
                        case 0:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=156&page=" + PageNum_GC + "&pagesize=50&order=default";
                            break;
                        case 1:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=156&page=" + PageNum_GC + "&pagesize=50&order=damku";
                            break;
                        case 2:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=156&page=" + PageNum_GC + "&pagesize=50&order=hot";
                            break;
                        case 3:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=156&page=" + PageNum_GC + "&pagesize=50&order=review";
                            break;
                        case 4:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=156&page=" + PageNum_GC + "&pagesize=50&order=stow";
                            break;
                        default:
                            uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=156&page=" + PageNum_GC + "&pagesize=50&order=default";
                            break;
                    }
                    #endregion
                    if (PageNum_GC == 1)
                    {
                        GC_NewList.Items.Clear();
                    }
                    HttpResponseMessage hr = await hc.GetAsync(new Uri(uri));
                    hr.EnsureSuccessStatusCode();
                    string results = await hr.Content.ReadAsStringAsync();
                    DHModel model = JsonConvert.DeserializeObject<DHModel>(results);
                    JObject json = JObject.Parse(model.list.ToString());
                    List<DHModel> ReList = new List<DHModel>();
                    for (int i = 0; i < 50; i++)
                    {
                        GC_NewList.Items.Add(new DHModel
                        {
                            aid = (string)json[i.ToString()]["aid"],
                            title = (string)json[i.ToString()]["title"],
                            pic = (string)json[i.ToString()]["pic"],
                            author = (string)json[i.ToString()]["author"],
                            play = (string)json[i.ToString()]["play"],
                            video_review = (string)json[i.ToString()]["video_review"],
                        });
                    }
                    CanLoad = true;
                    PageNum_GC++;
                    pro_Bar.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                pro_Bar.Visibility = Visibility.Collapsed;
            }
        }



        //用于跳转
        private void GridView_TJ_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), ((DHModel)e.ClickedItem).aid);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), ((DHModel)home_flipView.SelectedItem).aid);
        }

        private void ZH_HotList_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), ((DHModel)e.ClickedItem).aid);
        }

      
        //判断是不是在加载中
        bool CanLoad = true;
        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (CanLoad)
                {
                    GetDT();
                }
            }
        }

        //用了判断是否已经加载相关信息
        bool LZ_Load = false;
        bool WJ_Load = false;
        bool GC_Load = false;
        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 0)
            {
                cb_Bar.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_New.IsEnabled = false;
                btn_Comment.IsEnabled = true;
                btn_Danmaku.IsEnabled = true;
                btn_Play.IsEnabled = true;
                btn_Sc.IsEnabled = true;
                cb_Bar.Visibility = Visibility.Visible;
            }
            switch (pivot.SelectedIndex)
            {
                case 1:
                    if (!LZ_Load)
                    {
                        GetLZNew(0);
                        LZ_Load = true;
                    }
                    break;
                case 2:
                    if (!WJ_Load)
                    {
                        GetWJList(0);
                        WJ_Load = true;
                    }
                    break;
                case 3:
                    if (!GC_Load)
                    {
                        GetGCList(0);
                        GC_Load = true;
                    }
                    break;

                default:
                    break;
            }

        }

        //用来判断按什么排序
        int LZ_Order = 0;
        int WJ_Order = 0;
        int GC_Order = 0;
        //order 0为默认,1按弹幕,2按播放,3按评论,4按收藏
        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            btn_New.IsEnabled = false;
            btn_Comment.IsEnabled = true;
            btn_Danmaku.IsEnabled = true;
            btn_Play.IsEnabled = true;
            btn_Sc.IsEnabled = true;
            switch (pivot.SelectedIndex)
            {
                case 1:
                    PageNum_LZ = 1;
                    LZ_Order = 0;
                    GetLZNew(0);
                    break;
                case 2:
                    PageNum_WJ = 1;
                    WJ_Order = 0;
                    GetWJList(0);
                    break;
                case 3:
                    GC_Order = 0;
                    PageNum_GC = 1;
                    GetGCList(0);
                    break;
                default:
                    break;
            }
        }
        private void btn_Danmaku_Click(object sender, RoutedEventArgs e)
        {
            btn_New.IsEnabled = true;
            btn_Comment.IsEnabled = true;
            btn_Danmaku.IsEnabled = false;
            btn_Play.IsEnabled = true;
            btn_Sc.IsEnabled = true;
            switch (pivot.SelectedIndex)
            {
                case 1:
                    PageNum_LZ = 1;
                    LZ_Order = 1;
                    GetLZNew(1);
                    break;
                case 2:
                    PageNum_WJ = 1;
                    WJ_Order = 1;
                    GetWJList(1);
                    break;
                case 3:
                    GC_Order = 1;
                    PageNum_GC = 1;
                    GetGCList(1);
                    break;
                default:
                    break;
            }
        }
        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            btn_New.IsEnabled = true;
            btn_Comment.IsEnabled = true;
            btn_Danmaku.IsEnabled = true;
            btn_Play.IsEnabled = false;
            btn_Sc.IsEnabled = true;
            switch (pivot.SelectedIndex)
            {
                case 1:
                    //GridView_New.Items.Clear();
                    PageNum_LZ = 1;
                    GetLZNew(2);
                    LZ_Order = 2;
                    break;
                case 2:
                    //GridView_New.Items.Clear();
                    PageNum_WJ = 1;
                    LZ_Order = 2;
                    GetWJList(2);
                    break;
                case 3:
                    GC_Order = 2;
                    PageNum_GC = 1;
                    GetGCList(2);
                    break;
                default:
                    break;
            }
        }
        private void btn_Comment_Click(object sender, RoutedEventArgs e)
        {
            btn_New.IsEnabled = true;
            btn_Comment.IsEnabled = false;
            btn_Play.IsEnabled = true;
            btn_Sc.IsEnabled = true;
            switch (pivot.SelectedIndex)
            {
                case 1:
                    //GridView_New.Items.Clear();
                    PageNum_LZ = 1;
                    LZ_Order = 3;
                    GetLZNew(3);
                    break;
                case 2:
                    PageNum_WJ = 1;
                    WJ_Order = 3;
                    GetWJList(3);
                    break;
                case 3:
                    GC_Order = 3;
                    PageNum_GC = 1;
                    GetGCList(3);
                    break;
                default:
                    break;
            }
        }
        private void btn_Sc_Click(object sender, RoutedEventArgs e)
        {
            btn_New.IsEnabled = true;
            btn_Comment.IsEnabled = true;
            btn_Danmaku.IsEnabled = true;
            btn_Play.IsEnabled = true;
            btn_Sc.IsEnabled = false;
            switch (pivot.SelectedIndex)
            {
                case 1:
                    //GridView_New.Items.Clear();
                    PageNum_LZ = 1;
                    LZ_Order = 4;
                    GetLZNew(4);
                    break;
                case 2:
                    //GridView_New.Items.Clear();
                    PageNum_WJ = 1;
                    WJ_Order = 4;
                    GetWJList(4);
                    break;
                case 3:
                    GC_Order = 4;
                    PageNum_GC = 1;
                    GetGCList(4);
                    break;
                default:
                    break;
            }
        }
        //加载更多
        private void sv_LZ_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_LZ.VerticalOffset == sv_LZ.ScrollableHeight)
            {
                if (CanLoad)
                {
                    GetLZNew(LZ_Order);
                }
            }
        }
        private void sv_WJ_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_WJ.VerticalOffset == sv_WJ.ScrollableHeight)
            {
                if (CanLoad)
                {
                    GetWJList(WJ_Order);
                }
            }
        }
        private void sc_GC_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sc_GC.VerticalOffset == sc_GC.ScrollableHeight)
            {
                if (CanLoad)
                {
                    GetGCList(WJ_Order);
                }
            }
        }



        //刷新
        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 1:
                    PageNum_LZ = 1;
                    GetLZNew(LZ_Order);
                    break;
                case 2:
                    PageNum_WJ = 1;
                    GetWJList(WJ_Order);
                    break;
                case 3:
                    PageNum_GC = 1;
                    GetGCList(GC_Order);
                    break;
                default:
                    break;
            }
        }

        private void btn_Refresh_DT_Click(object sender, RoutedEventArgs e)
        {
            GetDT();
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            GetDYHome();
            GetDT();
        }
    }
}
