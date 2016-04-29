using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class UserBangumiPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public UserBangumiPage()
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

        string uid = "";
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            if (e.NavigationMode == NavigationMode.New)
            {
                list.Items.Clear();
                pageNum = 1;
                maxPage = 0;
                uid = e.Parameter as string;
                GetUserBangumi(uid, pageNum);
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            maxPage = 0;
            pageNum = 1;
        }
        int maxPage = 0;
        int pageNum = 1;
        public async void GetUserBangumi(string uid, int PageNum)
        {
            try
            {
                IsLoading = true;
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://space.bilibili.com/ajax/Bangumi/getList?mid=" + uid + "&pagesize=10&page=" + pageNum));
                //一层
                GetUserBangumi model1 = JsonConvert.DeserializeObject<GetUserBangumi>(results);
                if (model1.status)
                {
                    //二层
                    GetUserBangumi model2 = JsonConvert.DeserializeObject<GetUserBangumi>(model1.data.ToString());
                    maxPage = model2.pages;
                    //三层
                    List<GetUserBangumi> lsModel = JsonConvert.DeserializeObject<List<GetUserBangumi>>(model2.result.ToString());
                    foreach (GetUserBangumi item in lsModel)
                    {
                        list.Items.Add(item);
                    }
                    pageNum++;
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
                pr_Load.Visibility = Visibility.Collapsed;
                IsLoading = false;
            }
        }

        bool IsLoading = false;
        private void sc_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sc.VerticalOffset == sc.ScrollableHeight)
            {
                if (pageNum <= maxPage && !IsLoading)
                {
                    GetUserBangumi(uid, pageNum);
                }
            }
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(BanInfoPage), ((GetUserBangumi)e.ClickedItem).season_id);
        }
    }
}
