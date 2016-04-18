using bilibili2.Class;
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
    public sealed partial class UserInfoPage : Page
    {
        public UserInfoPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
           
        }
      

        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        private void btn_Back_Click(object sender, RoutedEventArgs e)
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
        string Uid = string.Empty;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Uid = "";
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            sp.IsPaneOpen = false;
            if (e.Parameter == null)
            {
                btn_Attention.Visibility = Visibility.Collapsed;
                btn_More.Visibility = Visibility.Visible;
                btn_Edit.Visibility = Visibility.Visible;
                fav.Visibility = Visibility.Visible;
                user_GridView_FovBox.Visibility = Visibility.Visible;
                UserClass getUser = new UserClass();
                user_GridView_FovBox.ItemsSource = null;
                pr_Load.Visibility = Visibility.Visible;
                await GetUserInfo();
                await GetDt();
                user_GridView_FovBox.ItemsSource = await getUser.GetUserFovBox();
                pr_Load.Visibility = Visibility.Collapsed;
            }
            else
            {
                Uid = e.Parameter as string;
                btn_Attention.Visibility = Visibility.Visible;
                btn_More.Visibility = Visibility.Collapsed;
                btn_Edit.Visibility = Visibility.Collapsed;
                fav.Visibility = Visibility.Collapsed;
                user_GridView_FovBox.Visibility = Visibility.Collapsed;
                user_GridView_FovBox.ItemsSource = null;
                pr_Load.Visibility = Visibility.Visible;
                await GetUserInfo();
                await GetDt();
                pr_Load.Visibility = Visibility.Collapsed;
            }
            
        }

        private async Task GetUserInfo()
        {
            UserClass getUser = new UserClass();
            GetLoginInfoModel model = new GetLoginInfoModel();
            if (Uid.Length != 0)
            {
                model = await getUser.GetUserInfo(Uid);
            }
            else
            {
                model = await getUser.GetUserInfo();
            }
            grid_Info.DataContext = model;
            if (model.current_level <= 3)
            {
                bor_Level.Background = new SolidColorBrush(new Windows.UI.Color() { R = 48, G = 161, B = 255, A = 200 });
            }
            else
            {
                if (model.current_level <= 6)
                {
                    bor_Level.Background = new SolidColorBrush(new Windows.UI.Color() { R = 255, G = 48, B = 48, A = 200 });
                }
                else
                {
                    bor_Level.Background = new SolidColorBrush(new Windows.UI.Color() { R = 255, G = 199, B = 45, A = 200 });
                }
            }
        }


        private void ToggleMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async Task GetDt()
        {
            UserClass getLogin = new UserClass();
            try
            {
                DT_0.Visibility = Visibility.Collapsed;
                if (Uid.Length != 0)
                {
                    user_GridView_Bangumi.ItemsSource = await getLogin.GetUserBangumi(Uid);
                }
                else
                {
                    user_GridView_Bangumi.ItemsSource = await getLogin.GetUserBangumi();
                }
               
                if (user_GridView_Bangumi.Items.Count == 0)
                {
                    DT_0.Visibility = Visibility.Visible;
                }

            }
            catch (Exception)
            {
                await new MessageDialog("读取动态失败").ShowAsync();
            }
        }

        private void user_GridView_Bangumi_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(BanInfoPage), (e.ClickedItem as GetUserBangumi).season_id);
        }

        private void user_GridView_FovBox_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void btn_AttBangumi_Click(object sender, RoutedEventArgs e)
        {
            if (Uid.Length!=0)
            {
                this.Frame.Navigate(typeof(UserBangumiPage), Uid);
            }
            else
            {
                this.Frame.Navigate(typeof(UserBangumiPage), UserClass.Uid);
            }
        }

        bool IsLoading = false;
        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv.VerticalOffset == sv.ScrollableHeight)
            {
                if (page <= MaxPage && !IsLoading)
                {
                    GetUserAttention(page);
                }
            }
           
           
        }

        int page = 1;
        int MaxPage = 0;
        public async void GetUserAttention(int pageNum)
        {
            try
            {
                pr_Load_AUser.Visibility = Visibility.Visible;
                IsLoading = true;
                WebClientClass wc = new WebClientClass();
                string mid = "";
                if (Uid.Length==0)
                {
                    mid = UserClass.Uid;
                }
                else
                {
                    mid = Uid;
                }
                string results = await wc.GetResults(new Uri("http://space.bilibili.com/ajax/friend/GetAttentionList?mid=" + mid + "&pagesize=20&page=" + pageNum));
                //一层
                GetUserFovBox model1 = JsonConvert.DeserializeObject<GetUserFovBox>(results);
                if (model1.status)
                {
                    //二层
                    GetUserAttention model2 = JsonConvert.DeserializeObject<GetUserAttention>(model1.data.ToString());
                    MaxPage = model2.pages;
                    //三层
                    List<GetUserAttention> lsModel = JsonConvert.DeserializeObject<List<GetUserAttention>>(model2.list.ToString());
                    foreach (GetUserAttention item in lsModel)
                    {
                        list_AUser.Items.Add(item);
                    }
                    page++;
                }
                else
                {
                    await new MessageDialog("读取信息失败！").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog("读取信息失败！\r\n" + ex.Message).ShowAsync();
            }
            finally
            {
                pr_Load_AUser.Visibility = Visibility.Collapsed;
                IsLoading = false;
            }
        }

        private void list_AUser_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(UserInfoPage), ((GetUserAttention)e.ClickedItem).fid);
          
        }

        private void Ban_btn_User_Click(object sender, RoutedEventArgs e)
        {
            grid_AUser.Visibility = Visibility.Visible;
            sp.IsPaneOpen = true;
            page = 1;
            MaxPage = 0;
            list_AUser.Items.Clear();
            GetUserAttention( page);
        }
    }
}
