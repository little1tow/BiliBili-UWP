using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public sealed partial class BanTagPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public BanTagPage()
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.New )
            {
                GetTagInfo();
            }
            
        }

        //索引
        public async void GetTagInfo()
        {
            try
            {
                pr_Load_Ban.Visibility =Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string uri = "http://bangumi.bilibili.com/api/tags?_device=wp&_ulv=10000&appkey=422fd9d7289a1dd9&build=411005&page=" + 1 + "&pagesize=60&platform=android&ts=" + ApiHelper.GetTimeSpen + "000";
                string sign = ApiHelper.GetSign(uri);
                uri += "&sign=" + sign;
                string results = await wc.GetResults(new Uri(uri));
                JObject jo = JObject.Parse(results);
                List<TagModel> list = JsonConvert.DeserializeObject<List<TagModel>>(jo["result"].ToString());
                gridview_List.ItemsSource = list;
            }
            catch (Exception ex)
            {
                messShow.Show("读取索引信息失败！\r\n" + ex.Message, 3000);
                
            }
            finally
            {
                pr_Load_Ban.Visibility = Visibility.Collapsed;
                // IsLoading = false;
            }
        }

        //索引点击
        private void gridview_List_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(BanByTagPage), new string[] { (e.ClickedItem as TagModel).tag_id.ToString(), (e.ClickedItem as TagModel).tag_name });
        }

    }
}
