using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class DiliInfo : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public DiliInfo()
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
            grid.DataContext = e.Parameter as DiliModel;
            GetList((e.Parameter as DiliModel).url);
        }

        private void list_Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(TestPlayerPage),( e.ClickedItem as DModel).url);
        }
        WebClientClass wc;
        private async void GetList(string name)
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string url = string.Format("http://www.dilidili.com{0}", name);
                string results = await wc.GetResultsUTF8Encode(new Uri(url));
                MatchCollection mc = Regex.Matches(results, @"<li><a href=""(.*?)"" target=""_self""><em><span>(.*?)</span>(.*?)</em></a></li>", RegexOptions.Multiline);
                List<DModel> listModel = new List<DModel>();
                foreach (Match item in mc)
                {
                    listModel.Add(new DModel()
                    {
                        url = item.Groups[1].Value,
                       num=item.Groups[2].Value,
                       desc=item.Groups[3].Value
                    });
                }
                list_Video.ItemsSource = listModel;
            }
            catch (Exception)
            {
                messShow.Show("读取失败", 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }

        }
      
        private class DModel
        {
            public string url { get; set; }
            public string num { get; set; }
            public string desc { get; set; }
        }
    }
}
