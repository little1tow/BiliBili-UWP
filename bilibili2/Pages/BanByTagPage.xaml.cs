using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BanByTagPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public BanByTagPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            SystemNavigationManager.GetForCurrentView().BackRequested += BanInfoPage_BackRequested;
        }
        private void BanInfoPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                BackEvent();
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

        private string[] getTid = new string[2];
        private int getPage = 1;
        private int getType = 1;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.New)
            {
                getTid = e.Parameter as string[];

                btn_type1.IsEnabled = false;
                btn_type2.IsEnabled = true;
                btn_type3.IsEnabled = true;
                btn_type4.IsEnabled = true;
                top_txt_Header.Text = getTid[1];
                pr_Load.Visibility = Visibility.Visible;
                await GetTagInfo(getPage, getTid[0], 0);
                pr_Load.Visibility = Visibility.Collapsed;

            }
            
        }

        private async Task GetTagInfo(int page, string tid, int type)
        {
            try
            {
                    WebClientClass wc = new WebClientClass();
                    string results = await wc.GetResults(new Uri(String.Format("http://bangumi.bilibili.com/api/get_season_by_tag?indexType={0}&page={1}&pagesize=20&tag_id={2}", type, page, tid)));
                    BanSeasonTagModel model = JsonConvert.DeserializeObject<BanSeasonTagModel>(results);
                    List<BanSeasonTagModel> ls = JsonConvert.DeserializeObject<List<BanSeasonTagModel>>(model.result.ToString());
                    foreach (BanSeasonTagModel item in ls)
                    {
                        ls_Tag.Items.Add(item);
                    }
                    getPage++;
                    if (model.pages < getPage)
                    {
                        btn_LoadMore.IsEnabled = false;
                        btn_LoadMore.Content = "加载完了...";
                    }
            }
            catch (Exception)
            {
                messShow.Show("读取失败，请重试",3000);
            }
        }

        private void ls_Tag_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(BanInfoPage), (e.ClickedItem as BanSeasonTagModel).season_id);
            // this.Frame.Navigate(typeof(BanSeasonNewPage),(e.ClickedItem as BanSeasonTagModel).season_id);
        }

        private async void btn_type1_Click(object sender, RoutedEventArgs e)
        {
            ls_Tag.Items.Clear();
            btn_LoadMore.IsEnabled = true;
            btn_LoadMore.Content = "加载更多";
            getPage = 1;
            getType = 0;
            btn_type1.IsEnabled = false;
            btn_type2.IsEnabled = true;
            btn_type3.IsEnabled = true;
            btn_type4.IsEnabled = true;
           
            pr_Load.Visibility = Visibility.Visible;
            await GetTagInfo(getPage, getTid[0], getType);

            pr_Load.Visibility = Visibility.Collapsed;

        }

        private async void btn_type2_Click(object sender, RoutedEventArgs e)
        {
            ls_Tag.Items.Clear();
            btn_LoadMore.IsEnabled = true;
            btn_LoadMore.Content = "加载更多";
            getPage = 1;
            getType = 1;
            btn_type1.IsEnabled = true;
            btn_type2.IsEnabled = false;
            btn_type3.IsEnabled = true;
            btn_type4.IsEnabled = true;

            pr_Load.Visibility = Visibility.Visible;
            await GetTagInfo(getPage, getTid[0], getType);

            pr_Load.Visibility = Visibility.Collapsed;
        }

        private async void btn_type3_Click(object sender, RoutedEventArgs e)
        {
            ls_Tag.Items.Clear();
            btn_LoadMore.IsEnabled = true;
            btn_LoadMore.Content = "加载更多";
            getPage = 1;
            getType = 2;
            btn_type1.IsEnabled = true;
            btn_type2.IsEnabled = true;
            btn_type3.IsEnabled = false;
            btn_type4.IsEnabled = true;

            pr_Load.Visibility = Visibility.Visible;
            await GetTagInfo(getPage, getTid[0], getType);

            pr_Load.Visibility = Visibility.Collapsed;
        }

        private async void btn_type4_Click(object sender, RoutedEventArgs e)
        {
            ls_Tag.Items.Clear();
            btn_LoadMore.IsEnabled = true;
            btn_LoadMore.Content = "加载更多";
            getPage = 1;
            getType = 3;
            btn_type1.IsEnabled = true;
            btn_type2.IsEnabled = true;
            btn_type3.IsEnabled = true;
            btn_type4.IsEnabled = false;
       
            pr_Load.Visibility = Visibility.Visible;
            await GetTagInfo(getPage, getTid[0], getType);
            pr_Load.Visibility = Visibility.Collapsed;
        }

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {

        }
        bool More = true;
        private async void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (More)
                {
                    More = false;
                    btn_LoadMore.IsEnabled = false;
                    btn_LoadMore.Content = "正在加载";
                    await GetTagInfo(getPage, getTid[0], getType);
                    if (btn_LoadMore.Content.ToString() != "加载完了...")
                    {
                        btn_LoadMore.IsEnabled = true;
                        btn_LoadMore.Content = "加载更多";
                    }
                    More = true;
                }
            }
        }


    }
}
