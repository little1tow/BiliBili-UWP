using bilibili2.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

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
        public event GoBackHandler ExitEvent;
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
            btn_Attention.Visibility = Visibility.Collapsed;
            btn_CannelAttention.Visibility = Visibility.Collapsed;
            user_GridView_FovBox.ItemsSource = null;
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            sp.IsPaneOpen = false;
            if (e.Parameter == null)
            {
                Uid = UserClass.Uid;
                btn_Attention.Visibility = Visibility.Collapsed;
                btn_More.Visibility = Visibility.Visible;
                btn_Edit.Visibility = Visibility.Visible;
                fav.Visibility = Visibility.Visible;
                user_GridView_FovBox.Visibility = Visibility.Visible;
                UserClass getUser = new UserClass();
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
                pr_Load.Visibility = Visibility.Visible;
                await GetUserInfo();
                await GetDt();
                if (UserClass.AttentionList.Contains(Uid))
                {
                    btn_Attention.Visibility = Visibility.Collapsed;
                    btn_CannelAttention.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_Attention.Visibility = Visibility.Visible;
                    btn_CannelAttention.Visibility = Visibility.Collapsed;
                }
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
            if (model.approve)
            {
                txt_RZ.Visibility = Visibility.Visible;
            }
            else
            {
                txt_RZ.Visibility = Visibility.Collapsed;
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
            this.Frame.Navigate(typeof(FavPage), (e.ClickedItem as GetUserFovBox).fav_box);
        }

        private void btn_AttBangumi_Click(object sender, RoutedEventArgs e)
        {
            if (Uid.Length != 0)
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
                if (Uid.Length == 0)
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
            grid_ASubit.Visibility = Visibility.Collapsed;
            grid_ACoin.Visibility = Visibility.Collapsed;
            sp.IsPaneOpen = true;
            page = 1;
            MaxPage = 0;
            list_AUser.Items.Clear();
            GetUserAttention(page);
        }

        private async void Ban_btn_Sub_Click(object sender, RoutedEventArgs e)
        {
            grid_AUser.Visibility = Visibility.Collapsed;
            grid_ASubit.Visibility = Visibility.Visible;
            grid_ACoin.Visibility = Visibility.Collapsed;
            txt_Load.IsEnabled = true;
            txt_Load.Content = "加载更多";

            sp.IsPaneOpen = true;
            getPage = 1;
            list_ASubit.Items.Clear();
            await GetSubInfo(1, Uid);
        }

        private int getPage = 1;
        private async Task GetSubInfo(int page, string uid)
        {
            try
            {
                pr_Load_ASubit.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                txt_Load.IsEnabled = false;
                txt_Load.Content = "加载中...";
                string results = await wc.GetResults(new Uri("http://space.bilibili.com/ajax/member/getSubmitVideos?mid=" + uid + "&pagesize=20" + "&page=" + page));
                //一层
                GetUserSubmit model1 = JsonConvert.DeserializeObject<GetUserSubmit>(results);
                //二层
                GetUserSubmit model2 = JsonConvert.DeserializeObject<GetUserSubmit>(model1.data.ToString());
                //三层
                List<GetUserSubmit> lsModel = JsonConvert.DeserializeObject<List<GetUserSubmit>>(model2.vlist.ToString());
                foreach (GetUserSubmit item in lsModel)
                {
                    list_ASubit.Items.Add(item);
                }
                
                getPage++;
                if (model2.pages < getPage)
                {
                    txt_Load.IsEnabled = false;
                    txt_Load.Content = "加载完了...";
                }

            }
            catch (Exception)
            {
            }
            finally
            {
                if (list_ASubit.Items.Count == 0)
                {
                    txt_Load.IsEnabled = false;
                    txt_Load.Content = "没有投稿...";
                }
                pr_Load_ASubit.Visibility = Visibility.Collapsed;
            }
        }
        bool subLoading = false;
        private async void sv1_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (txt_Load.Content.ToString() == "加载完了..."|| txt_Load.Content.ToString() == "没有投稿...")
            {
                return;
            }
            if (sv1.VerticalOffset == sv1.ScrollableHeight)
            {
                if (!subLoading)
                {
                    subLoading = true;
                    await GetSubInfo(getPage, Uid);
                    subLoading = false;
                }
            }
        }

        private void list_ASubit_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), (e.ClickedItem as GetUserSubmit).aid);
        }

        private async void GetPutCoin()
        {
            try
            {
                pr_Load_ACoin.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                txt_Load.IsEnabled = false;
                txt_Load.Content = "加载中...";
                string results = await wc.GetResults(new Uri("http://space.bilibili.com/ajax/member/getCoinVideos?mid=" + Uid + "&pagesize=100" + page+"&rnd="+new Random().Next(1,9999)));
                //一层
                GetUserSubmit model1 = JsonConvert.DeserializeObject<GetUserSubmit>(results);
                //二层
                GetUserSubmit model2 = JsonConvert.DeserializeObject<GetUserSubmit>(model1.data.ToString());
                //三层
                List<GetUserSubmit> lsModel = JsonConvert.DeserializeObject<List<GetUserSubmit>>(model2.list.ToString());
                list_ACoin.ItemsSource = lsModel;
                txt_Load_Coin.IsEnabled = false;
                txt_Load_Coin.Content = "加载完了...";
            }
            catch (Exception)
            {
            }
            finally
            {
                if (list_ACoin.Items.Count == 0)
                {
                    txt_Load_Coin.IsEnabled = false;
                    txt_Load_Coin.Content = "没有投稿...";
                }
                pr_Load_ACoin.Visibility = Visibility.Collapsed;
            }
        }

        private async void btn_Attention_Click(object sender, RoutedEventArgs e)
        {
            UserClass getUser = new UserClass();
            if (getUser.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://space.bilibili.com/ajax/friend/AddAttention");
                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://space.bilibili.com/");
                    var response = await hc.PostAsync(ReUri, new HttpStringContent("mid=" + Uid, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((bool)json["status"])
                    {
                        btn_Attention.Visibility = Visibility.Collapsed;
                        btn_CannelAttention.Visibility = Visibility.Visible;
                        await getUser.GetUserInfo();
                    }
                    else
                    {
                        btn_Attention.Visibility = Visibility.Visible;
                        btn_CannelAttention.Visibility = Visibility.Collapsed;
                        MessageDialog md = new MessageDialog("关注失败！\r\n"+ result);
                        await md.ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageDialog md = new MessageDialog("关注发生错误！\r\n"+ex.Message);
                    await md.ShowAsync();
                }
            }
            else
            {
                MessageDialog md = new MessageDialog("请先登录！");
                await md.ShowAsync();
            }
        }

        private async void btn_CannelAttention_Click(object sender, RoutedEventArgs e)
        {
            UserClass getUser = new UserClass();
            if (getUser.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://space.bilibili.com/ajax/friend/DelAttention");
                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://space.bilibili.com/");
                    var response = await hc.PostAsync(ReUri, new HttpStringContent("mid=" + Uid, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((bool)json["status"])
                    {
                        btn_Attention.Visibility = Visibility.Visible;
                        btn_CannelAttention.Visibility = Visibility.Collapsed;
                        await getUser.GetUserInfo();
                    }
                    else
                    {
                        btn_Attention.Visibility = Visibility.Collapsed;
                        btn_CannelAttention.Visibility = Visibility.Visible;
                        MessageDialog md = new MessageDialog("关注失败！\r\n" + result);
                        await md.ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageDialog md = new MessageDialog("取消关注发生错误！\r\n" + ex.Message);
                    await md.ShowAsync();
                }
            }
            else
            {
                MessageDialog md = new MessageDialog("请先登录！");
                await md.ShowAsync();
            }
        }

        private async void btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            List<HttpCookie> listCookies = new List<HttpCookie>();
            listCookies.Add(new HttpCookie("sid", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("DedeUserID", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("DedeUserID__ckMd5", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("SESSDATA", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("LIVE_LOGIN_DATA", ".bilibili.com", "/"));
            listCookies.Add(new HttpCookie("LIVE_LOGIN_DATA__ckMd5", ".bilibili.com", "/"));
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            foreach (HttpCookie cookie in listCookies)
            {
                filter.CookieManager.DeleteCookie(cookie);
            }
            ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
            container.Values["AutoLogin"] = "false";
            ApiHelper.access_key = string.Empty;
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, string.Empty);
            ExitEvent();
        }

        private void Ban_btn_Coin_Click(object sender, RoutedEventArgs e)
        {
            grid_AUser.Visibility = Visibility.Collapsed;
            grid_ASubit.Visibility = Visibility.Collapsed;
            grid_ACoin.Visibility = Visibility.Visible;
            GetPutCoin();
            sp.IsPaneOpen = true;
        }
    }
}
