using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public SearchPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private int pageNum = 1;
        private string keyword = "";
        private int pageNum_Up = 1;
        private int pageNum_Ban = 1;
        private int pageNum_Sp = 1;

        WebClientClass wc;
        //搜索视频
        public async void SeachVideo()
        {
            try
            {
                object a = (cb_part.SelectedItem as ComboBoxItem).Tag;
                pr_Loading.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string uri = "";
                if (a == null)
                {
                    uri = "http://api.bilibili.com/search?_device=wp&appkey=422fd9d7289a1dd9&build=411005&keyword=" + keyword + "&main_ver=v3&page=" + pageNum + "&pagesize=20&platform=android&search_type=video&source_type=0" + ((cb_OrderBy.SelectedItem as ComboBoxItem).Tag == null ? "" : "&order=" + (cb_OrderBy.SelectedItem as ComboBoxItem).Tag);
                }
                else
                {
                    uri = "http://api.bilibili.com/search?_device=wp&appkey=422fd9d7289a1dd9&build=411005&keyword=" + keyword + "&main_ver=v3&page=" + pageNum + "&pagesize=20&platform=android&search_type=video&source_type=0&tids=" + a.ToString() + ((cb_OrderBy.SelectedItem as ComboBoxItem).Tag == null ? "" : "&order=" + (cb_OrderBy.SelectedItem as ComboBoxItem).Tag);
                }
                string sign = ApiHelper.GetSign(uri);
                uri += "&sign=" + sign;
                string results = await wc.GetResults(new Uri(uri));
                SVideoModel model = JsonConvert.DeserializeObject<SVideoModel>(results);
                if (model.code == 0)
                {
                    List<SVideoModel> ls = JsonConvert.DeserializeObject<List<SVideoModel>>(model.result.ToString());
                    foreach (SVideoModel item in ls)
                    {
                        Seach_listview_Video.Items.Add(item);
                    }
                    pageNum++;
                }
                if (model.code == -3)
                {
                    await new MessageDialog("API Sign注册失败！请联系作者！").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                grid_GG.Visibility = Visibility.Visible;
                txt_GG.Text = "搜索视频失败\r\n" + ex.Message;
                await Task.Delay(2000);
                grid_GG.Visibility = Visibility.Collapsed;
            }
            finally
            {
                pr_Loading.Visibility = Visibility.Collapsed;
            }

        }
        //搜索番剧

        public async void SeachBangumi()
        {
            try
            {
                object a = (cb_part.SelectedItem as ComboBoxItem).Tag;
                pr_Loading.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string uri = "http://api.bilibili.com/search?_device=wp&appkey=422fd9d7289a1dd9&build=411005&keyword=" + keyword + "&main_ver=v3&page=" + pageNum + "&pagesize=20&platform=android&search_type=bangumi&source_type=0";
                string sign = ApiHelper.GetSign(uri);
                uri += "&sign=" + sign;
                string results = await wc.GetResults(new Uri(uri));
                SBanModel model = JsonConvert.DeserializeObject<SBanModel>(results);
                if (model.code == 0)
                {
                    List<SBanModel> ls = JsonConvert.DeserializeObject<List<SBanModel>>(model.result.ToString());
                    foreach (SBanModel item in ls)
                    {
                        Seach_listview_Ban.Items.Add(item);
                    }
                    pageNum_Ban++;
                }
                if (model.code == -3)
                {
                    await new MessageDialog("API Sign注册失败！请联系作者！").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                grid_GG.Visibility = Visibility.Visible;
                txt_GG.Text = "搜索番剧失败\r\n" + ex.Message;
                await Task.Delay(2000);
                grid_GG.Visibility = Visibility.Collapsed;
            }
            finally
            {
                pr_Loading.Visibility = Visibility.Collapsed;
            }

        }
        //搜索UP
        public async void SeachUP()
        {
            try
            {
                object a = (cb_part.SelectedItem as ComboBoxItem).Tag;
                pr_Loading.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string uri = "http://api.bilibili.com/search?_device=wp&appkey=422fd9d7289a1dd9&build=411005&keyword=" + keyword + "&main_ver=v3&page=" + pageNum + "&pagesize=20&platform=android&search_type=upuser&source_type=0";
                string sign = ApiHelper.GetSign(uri);
                uri += "&sign=" + sign;
                string results = await wc.GetResults(new Uri(uri));
                SUpModel model = JsonConvert.DeserializeObject<SUpModel>(results);
                if (model.code == 0)
                {
                    List<SUpModel> ls = JsonConvert.DeserializeObject<List<SUpModel>>(model.result.ToString());
                    foreach (SUpModel item in ls)
                    {
                        Seach_listview_Up.Items.Add(item);
                    }
                    pageNum_Up++;
                }
                if (model.code == -3)
                {
                    await new MessageDialog("API Sign注册失败！请联系作者！").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                grid_GG.Visibility = Visibility.Visible;
                txt_GG.Text = "搜索UP主失败\r\n" + ex.Message;
                await Task.Delay(2000);
                grid_GG.Visibility = Visibility.Collapsed;
            }
            finally
            {
                pr_Loading.Visibility = Visibility.Collapsed;
            }

        }
        //搜索专题
        public async void SeachSp()
        {
            try
            {
                pr_Loading.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string uri = "http://api.bilibili.com/search?_device=wp&appkey=422fd9d7289a1dd9&build=411005&keyword=" + keyword + "&main_ver=v3&page=" + pageNum + "&pagesize=20&platform=android&search_type=special&source_type=0";
                string sign = ApiHelper.GetSign(uri);
                uri += "&sign=" + sign;
                string results = await wc.GetResults(new Uri(uri));
                SSpModel model = JsonConvert.DeserializeObject<SSpModel>(results);
                if (model.code == 0)
                {
                    List<SSpModel> ls = JsonConvert.DeserializeObject<List<SSpModel>>(model.result.ToString());
                    foreach (SSpModel item in ls)
                    {
                        Seach_listview_Sp.Items.Add(item);
                    }
                    pageNum_Sp++;
                }
                if (model.code == -3)
                {
                    await new MessageDialog("API Sign注册失败！请联系作者！").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                grid_GG.Visibility = Visibility.Visible;
                txt_GG.Text = "搜索专题失败\r\n" + ex.Message;
                await Task.Delay(2000);
                grid_GG.Visibility = Visibility.Collapsed;
            }
            finally
            {
                pr_Loading.Visibility = Visibility.Collapsed;
            }

        }
        //开始搜索
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            if (e.NavigationMode == NavigationMode.New)
            {
                keyword = Uri.EscapeDataString((string)e.Parameter);
                text_Title.Text ="搜索 "+ (string)e.Parameter;
                pageNum = 1;
                pageNum_Up = 1;
                pageNum_Ban = 1;
                pageNum_Sp = 1;
                Seach_listview_Video.Items.Clear();
                Seach_listview_Ban.Items.Clear();
                Seach_listview_Sp.Items.Clear();
                Seach_listview_Up.Items.Clear();
                SeachVideo();
                SeachUP();
                SeachBangumi();
                SeachSp();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                keyword = "";
                cb_OrderBy.SelectedIndex = 0;
                cb_part.SelectedIndex = 0;
                wc = null;
            }

        }

        private void User_load_more_Click(object sender, RoutedEventArgs e)
        {
            SeachVideo();
        }



        private void Seach_listview_Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), ((SVideoModel)e.ClickedItem).aid);
        }

        private void Seach_listview_Up_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(UserInfoPage), ((SUpModel)e.ClickedItem).mid);
        }

        private void Up_load_more_Click(object sender, RoutedEventArgs e)
        {
            SeachUP();
        }

        private void Ban_load_more_Click(object sender, RoutedEventArgs e)
        {
            SeachBangumi();
        }

        private void Seach_listview_Ban_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(BanInfoPage), ((SBanModel)e.ClickedItem).season_id);
            //this.Frame.Navigate(typeof(BanSeasonNewPage), ((SeachBanModel)e.ClickedItem).season_id);
        }

        private void Seach_listview_Sp_ItemClick(object sender, ItemClickEventArgs e)
        {
            //this.Frame.Navigate(typeof(BanSeasonPage), ((SSpModel)e.ClickedItem).spid);
        }

        private void Sp_load_more_Click(object sender, RoutedEventArgs e)
        {
            SeachSp();
        }

        bool InfoLoading = false;
        private void sv_SP_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                if (!InfoLoading)
                {
                    User_load_more.IsEnabled = false;
                    User_load_more.Content = "正在加载";
                    SeachVideo();
                }
            }
        }

        private void btn_Filter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cb_part_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (keyword != "")
            {
                pageNum = 1;
                Seach_listview_Video.Items.Clear();
                SeachVideo();
            }

        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 0)
            {
                btn_Filter.IsEnabled = true;
            }
            else
            {
                btn_Filter.IsEnabled = false;
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
    }
}
