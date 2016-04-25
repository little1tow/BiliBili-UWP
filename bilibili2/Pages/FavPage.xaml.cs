using bilibili2.Class;
using System;
using System.Collections.Generic;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FavPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public FavPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private UserClass getLogin=new UserClass();
        private int pageNum = 1;
        private int MaxPage = 0;
        private string fid = "";
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            if (e.NavigationMode == NavigationMode.New)
            {
                List<GetUserFovBox> favList = await getLogin.GetUserFovBox();
                cb_favbox.ItemsSource = favList;
                if (favList.Count==0||favList==null)
                {
                    notInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    notInfo.Visibility = Visibility.Collapsed;
                }
                if (e.Parameter != null)
                {
                    var d = from a in favList where a.fav_box == (string)e.Parameter select a;
                    cb_favbox.SelectedIndex = favList.IndexOf(d.ToList()[0]);
                }
                else
                {
                    cb_favbox.SelectedIndex = 0;
                }
            }

        }
        private async void GetFavouriteBoxVideo()
        {
            pr_Load.Visibility = Visibility.Visible;
            loading = true;
            try
            {
                getLogin = new UserClass();
                List<GetFavouriteBoxsVideoModel> lsModel = await getLogin.GetFavouriteBoxVideo(fid, pageNum);
                if (lsModel != null)
                {
                    foreach (GetFavouriteBoxsVideoModel item in lsModel)
                    {
                        MaxPage = item.pages;
                        User_ListView_FavouriteVideo.Items.Add(item);
                    }
                    //为下一页做准备
                    pageNum++;
                    if (pageNum > MaxPage)
                    {
                        User_load_more.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    MessageDialog md = new MessageDialog("信息获取失败！");
                    await md.ShowAsync();
                    User_load_more.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("读取收藏夹信息失败", ex.Message).ShowAsync();
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
                loading = false;
            }

        }

        private void User_load_more_Click(object sender, RoutedEventArgs e)
        {
            GetFavouriteBoxVideo();
        }

        private void User_ListView_FavouriteVideo_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), ((GetFavouriteBoxsVideoModel)e.ClickedItem).aid);
        }
        bool loading = false;
        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (!loading)
                {
                    GetFavouriteBoxVideo();
                }
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

        private void cb_favbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_favbox.SelectedItem!=null)
            {
                pageNum = 1;
                MaxPage = 0;
                fid = (cb_favbox.SelectedItem as GetUserFovBox).fav_box;
                top_txt_Header.Text = (cb_favbox.SelectedItem as GetUserFovBox).name;
                User_ListView_FavouriteVideo.Items.Clear();
                GetFavouriteBoxVideo();
            }
          
        }
    }
}
