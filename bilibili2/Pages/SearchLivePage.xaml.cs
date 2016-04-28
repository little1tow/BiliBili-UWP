using bilibili2.Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchLivePage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public SearchLivePage()
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
            if (e.Parameter!=null)
            {
                txt_Find.Text = e.Parameter as string;
                GetSearchResults(e.Parameter as string);
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            live_List.ItemsSource = null;
            list_User.ItemsSource = null;
        }

        private void btn_Liveing_Click(object sender, RoutedEventArgs e)
        {
            pivot.SelectedIndex = Convert.ToInt32((sender as Button).Tag);
        }

        //偷懒。。。pagesize就直接写100.。。。
        private async void GetSearchResults(string keyword)
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                //http://live.bilibili.com/AppSearch/index?_device=wp&_ulv=10000&access_key={0}&appkey={1}&build=411005&keyword={2}&page=1&pagesize=20&platform=android&type=all
                WebClientClass wc = new WebClientClass();
                string url = string.Format("http://live.bilibili.com/AppSearch/index?_device=wp&_ulv=10000&access_key={0}&appkey={1}&build=411005&keyword={2}&page=1&pagesize=100&platform=android&type=all", ApiHelper.access_key, ApiHelper._appKey, WebUtility.UrlEncode(keyword));
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                SearchLiveModel model = JsonConvert.DeserializeObject<SearchLiveModel>(results);
                if (model.code == 0)
                {
                    SearchLiveModel dataModel = JsonConvert.DeserializeObject<SearchLiveModel>(model.data.ToString());
                    SearchLiveModel roomModel = JsonConvert.DeserializeObject<SearchLiveModel>(dataModel.room.ToString());
                    SearchLiveModel UserModel = JsonConvert.DeserializeObject<SearchLiveModel>(dataModel.user.ToString());
                    btn_Liveing.Content = "正在直播(" + roomModel.total_room + ")";
                    btn_User.Content = "主播(" + UserModel.total_user + ")";
                    live_List.ItemsSource = JsonConvert.DeserializeObject<List<SearchLiveModel>>(roomModel.list.ToString());
                    list_User.ItemsSource = JsonConvert.DeserializeObject<List<SearchLiveModel>>(UserModel.list.ToString());
                }
                else
                {
                    messShow.Show(model.message, 3000);
                }
            }
            catch (Exception)
            {
                messShow.Show("读取信息失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
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
                ViewBox_num.Width = ActualWidth / i - 15;
            }
        }

        private void live_HOT_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(LiveInfoPage), (e.ClickedItem as SearchLiveModel).roomid);
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }
        public void UpdateUI()
        {
            btn_Liveing.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_User.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });

            btn_Liveing.FontWeight = FontWeights.Normal;
            btn_User.FontWeight = FontWeights.Normal;

            switch (pivot.SelectedIndex)
            {
                case 0:
                    btn_Liveing.Foreground = new SolidColorBrush(Colors.White);
                    btn_Liveing.FontWeight = FontWeights.Bold;
                    break;
                case 1:
                    btn_User.Foreground = new SolidColorBrush(Colors.White);
                    btn_User.FontWeight = FontWeights.Bold;
                    break;
                default:
                    break;
            }
        }
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(LiveInfoPage),(e.ClickedItem as SearchLiveModel).roomid);
        }

        private void txt_Find_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (txt_Find.Text.Length==0)
            {
                messShow.Show("搜索内容不能为空！",3000);
                return;
            }
            GetSearchResults(txt_Find.Text);
        }
    }
}
