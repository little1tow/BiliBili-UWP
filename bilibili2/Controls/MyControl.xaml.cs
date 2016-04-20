using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2
{
    public sealed partial class MyControl : UserControl
    {
        public delegate void PlayHandler(string aid);
        public event PlayHandler PlayEvent;
        public event PlayHandler ErrorEvent;
        public MyControl()
        {
            this.InitializeComponent();
        }


        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 500)
            {
                ViewBox_num.Width = ActualWidth / 2 - 20;
                ViewBox2_num.Width = ActualWidth / 2 - 20;
                double d = ((ViewBox2_num.Width + 12) / 1.08) * 2;
                home_GridView_FJ.Height = d;
                home_GridView_DH.Height = d;
                home_GridView_YYWD.Height = d;
                home_GridView_WD.Height = d;
                home_GridView_YX.Height = d;
                home_GridView_KJ.Height = d;
                home_GridView_GC.Height = d;
                home_GridView_YL.Height = d;
                home_GridView_DY.Height = d;
                home_GridView_DSJ.Height = d;
                home_GridView_SS.Height = d;
                //PageCount = 4;
            }
            else
            {
                if (this.ActualWidth <= 800)
                {
                    ViewBox_num.Width = ActualWidth / 3 - 20;
                    ViewBox2_num.Width = ActualWidth / 3 - 20;
                    double d = ((ViewBox2_num.Width + 12) / 1.08) * 2;
                    home_GridView_FJ.Height = d;
                    home_GridView_DH.Height = d;
                    home_GridView_YYWD.Height = d;
                    home_GridView_WD.Height = d;
                    home_GridView_YX.Height = d;
                    home_GridView_KJ.Height = d;
                    home_GridView_GC.Height = d;
                    home_GridView_YL.Height = d;
                    home_GridView_DY.Height = d;
                    home_GridView_DSJ.Height = d;
                    home_GridView_SS.Height = d;
                }
                else
                {
                    int i = Convert.ToInt32(ActualWidth / 200);
                    ViewBox_num.Width = ActualWidth / i - 15;
                    ViewBox2_num.Width = ActualWidth / i - 15;
                    double d = ((ViewBox2_num.Width + 12) / 1.08);
                    home_GridView_FJ.Height = d;
                    home_GridView_DH.Height = d;
                    home_GridView_YYWD.Height = d;
                    home_GridView_WD.Height = d;
                    home_GridView_YX.Height = d;
                    home_GridView_KJ.Height = d;
                    home_GridView_GC.Height = d;
                    home_GridView_YL.Height = d;
                    home_GridView_DY.Height = d;
                    home_GridView_DSJ.Height = d;
                    home_GridView_SS.Height = d;
                }
            }
        }
        public void SetListView(string results, GridView ls)
        {
            try
            {
                ls.Items.Clear();
                InfoModel model = JsonConvert.DeserializeObject<InfoModel>(results);
                List<InfoModel> ban = JsonConvert.DeserializeObject<List<InfoModel>>(model.list.ToString());
                for (int i = 0; i <12; i++)
                {
                    ls.Items.Add(ban[i]);
                }
              
            }
            catch (Exception)
            {
                //ErrorEvent(ex.Message);
            }
        }


        WebClientClass wc = new WebClientClass();
        public async void SetHomeInfo()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                string fj = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=13&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(fj, home_GridView_FJ);
                // string banner = await wc.GetResults(new Uri("http://www.bilibili.com/index/slideshow.json"));
                string dh = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=1&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd="+new Random().Next(1000,9999)));
                SetListView(dh, home_GridView_DH);
             
                string yy = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=3&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(yy, home_GridView_YYWD);
                string wd = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=20&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(wd, home_GridView_WD);
                string yx = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=4&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(yx, home_GridView_YX);
                string kj = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=36&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(kj, home_GridView_KJ);
                string YL = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=5&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(YL, home_GridView_YL);
                string GC = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=119&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(GC, home_GridView_GC);
                string DY = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=23&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(DY, home_GridView_DY);
                string DSJ = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=11&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(DSJ, home_GridView_DSJ);
                string SS = await wc.GetResults(new Uri("http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=155&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999)));
                SetListView(SS, home_GridView_SS);
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.Message);

            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void items_listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(VideoInfoPage));
        }
        private void home_GridView_FJ_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlayEvent((e.ClickedItem as InfoModel).aid);
        }

        private async void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            GridView gridview=null;
            string uri = string.Empty;
            switch ((sender as HyperlinkButton).Tag.ToString())
            {
                case "FJ":
                    gridview = home_GridView_FJ;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=13&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "DH":
                    gridview = home_GridView_DH;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=1&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "YY":
                    gridview = home_GridView_YYWD;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=3&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "WD":
                    gridview = home_GridView_WD;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=20&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "YX":
                    gridview = home_GridView_YX;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=4&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "KJ":
                    gridview = home_GridView_KJ;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=36&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "YL":
                    gridview = home_GridView_YL;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=5&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "GC":
                    gridview = home_GridView_GC;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=119&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "DY":
                    gridview = home_GridView_DY;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=23&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "DSJ":
                    gridview = home_GridView_DSJ;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=11&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                case "SS":
                    gridview = home_GridView_SS;
                    uri = "http://api.bilibili.com/list?type=json&appkey=422fd9d7289a1dd9&tid=155&page=1&pagesize=" + 20 + "&order=hot&ver=2&rnd=" + new Random().Next(1000, 9999);
                    break;
                default:
                    break;
            }
            if (gridview.Items==null|| gridview.Items.Count==0)
            {
                string fj = await wc.GetResults(new Uri(uri));
                SetListView(fj, gridview);
            }
            else
            {
                var lists = (from a in gridview.Items select a).ToArray();
                gridview.Items.Clear();
                for (int i = lists.Length - 1; i >= 0; i--)
                {
                    gridview.Items.Add(lists[i]);
                }
                lists = null;
            }
           
        }
    }
}
