using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TopicPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public TopicPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += TopicPage_BackRequested;
        }

        private void TopicPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            BackEvent();
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            BackEvent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.New)
            {
                GetTopic();
            }
        }
        private async void GetTopic()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://www.bilibili.com/index/slideshow.json"));
                TopicModel model = JsonConvert.DeserializeObject<TopicModel>(results);
                list_Topic.ItemsSource = JsonConvert.DeserializeObject<List<TopicModel>>(model.list.ToString());
            }
            catch (Exception ex)
            {
                messShow.Show("读取话题失败\r\n" + ex.Message, 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }
    }
}
