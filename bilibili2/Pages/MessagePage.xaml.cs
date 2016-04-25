using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Newtonsoft.Json;
using System.Text.RegularExpressions;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MessagePage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public MessagePage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        WebClientClass wc;
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
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (wc!=null)
            {
                wc =null;
            }
        }
        private void btn_HF_Click(object sender, RoutedEventArgs e)
        {
            pivot.SelectedIndex = Convert.ToInt32((sender as Button).Tag);
        }

        public void UpdateUI()
        {
            btn_HF.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_At.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Zan.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_TZ.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_SX.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_HF.FontWeight = FontWeights.Normal;
            btn_At.FontWeight = FontWeights.Normal;
            btn_Zan.FontWeight = FontWeights.Normal;
            btn_SX.FontWeight = FontWeights.Normal;
            btn_TZ.FontWeight = FontWeights.Normal;

            switch (pivot.SelectedIndex)
            {
                case 0:
                    btn_HF.Foreground = new SolidColorBrush(Colors.White);
                    btn_HF.FontWeight = FontWeights.Bold;
                    break;
                case 1:
                    btn_At.Foreground = new SolidColorBrush(Colors.White);
                    btn_At.FontWeight = FontWeights.Bold;
                    break;
                case 2:
                    btn_Zan.Foreground = new SolidColorBrush(Colors.White);
                    btn_Zan.FontWeight = FontWeights.Bold;
                    break;
                case 3:
                    btn_TZ.Foreground = new SolidColorBrush(Colors.White);
                    btn_TZ.FontWeight = FontWeights.Bold;
                    break;
                case 4:
                    btn_SX.Foreground = new SolidColorBrush(Colors.White);
                    btn_SX.FontWeight = FontWeights.Bold;
                    break;
                default:
                    break;
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
            switch (pivot.SelectedIndex)
            {
                case 0:
                    GetReply();
                    break;
                case 1:
                  
                    break;
                case 2:
                    break;
                case 3:
                    GetNotify();
                    break;
                case 4:
                    GetChatMe();
                    break;
                default:
                    break;
            }
        }

        private async void GetReply()
        {
            try
            {
                wc = new WebClientClass();
                string url = string.Format("http://message.bilibili.com/api/notify/query.replyme.list.do?_device=wp&&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=410005&data_type=1&page_size=40&platform=android&ts={2}", ApiHelper.access_key, ApiHelper._appKey, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                MessageReplyModel model = JsonConvert.DeserializeObject<MessageReplyModel>(results);
                if (model.code == 0)
                {
                    List<MessageReplyModel> list = JsonConvert.DeserializeObject<List<MessageReplyModel>>(model.data.ToString());
                    foreach (var item in list)
                    {
                        MessageReplyModel models = JsonConvert.DeserializeObject<MessageReplyModel>(item.publisher.ToString());
                        item.mid = models.mid;
                        item.name = models.name;
                        item.face = models.face;
                    }
                    list_Reply.ItemsSource = list;
                }
                else
                {
                    messShow.Show("读取失败" + model.message, 3000);
                }
            }
            catch (Exception)
            {
                messShow.Show("读取失败", 3000);
            }
            
        }

        private async void GetNotify()
        {
            try
            {
                wc = new WebClientClass();
                //http://message.bilibili.com/api/notify/query.sysnotify.list.do?_device=android&_hwid=68fc5d795c256cd1&_ulv=10000&access_key=a36a84cc8ef4ea2f92c416951c859a25&actionKey=appkey&appkey=c1b107428d337928&build=414000&data_type=1&page_size=40&platform=android&ts=1461404973000&sign=fc3b4e26348a1204e2064e7712d10179
                string url = string.Format("http://message.bilibili.com/api/notify//query.sysnotify.list.do?_device=wp&&_ulv=10000&access_key={0}&actionKey=appkey&appkey={1}&build=410005&data_type=1&page_size=40&platform=android&ts={2}", ApiHelper.access_key, ApiHelper._appKey, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                MessageReplyModel model = JsonConvert.DeserializeObject<MessageReplyModel>(results);
                if (model.code == 0)
                {
                    List<MessageReplyModel> list = JsonConvert.DeserializeObject<List<MessageReplyModel>>(model.data.ToString());
                    list_Notify.ItemsSource = list;
                }
                else
                {
                    messShow.Show("读取失败" + model.message, 3000);
                }
            }
            catch (Exception)
            {
                messShow.Show("读取失败", 3000);
            }
            
        }

        private async void GetChatMe()
        {
            try
            {
                wc = new WebClientClass();
                // http://message.bilibili.com/api/msg/query.room.list.do?access_key=a36a84cc8ef4ea2f92c416951c859a25&actionKey=appkey&appkey=c1b107428d337928&build=414000&page_size=100&platform=android&ts=1461404884000&sign=5e212e424761aa497a75b0fb7fbde775
                string url = string.Format("http://message.bilibili.com/api/msg/query.room.list.do?access_key={0}&actionKey=appkey&appkey={1}&build=411005&page_size=100&platform=android&ts={2}", ApiHelper.access_key, ApiHelper._appKey, ApiHelper.GetTimeSpen);
                url += "&sign=" + ApiHelper.GetSign(url);
                string results = await wc.GetResults(new Uri(url));
                MessageChatModel model = JsonConvert.DeserializeObject<MessageChatModel>(results);
                if (model.code == 0)
                {
                    List<MessageChatModel> list = JsonConvert.DeserializeObject<List<MessageChatModel>>(model.data.ToString());
                    list_ChatMe.ItemsSource = list;
                }
                else
                {
                    messShow.Show("读取失败" + model.message, 3000);
                }
            }
            catch (Exception)
            {
                messShow.Show("读取失败", 3000);
            }
          
        }

        private void list_Reply_ItemClick(object sender, ItemClickEventArgs e)
        {
            string aid = Regex.Match((e.ClickedItem as MessageReplyModel).link, @"^http://www.bilibili.com/video/av(.*?)/$").Groups[1].Value;
            if (aid.Length != 0)
            {
                this.Frame.Navigate(typeof(VideoInfoPage), aid);
                return;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageReplyModel model = (sender as HyperlinkButton).DataContext as MessageReplyModel;
            this.Frame.Navigate(typeof(UserInfoPage), model.mid);
        }

        private void list_Notify_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((e.ClickedItem as MessageReplyModel).link!=null)
            {
                this.Frame.Navigate(typeof(WebViewPage), (e.ClickedItem as MessageReplyModel).link);
            }

        }

        private void list_ChatMe_ItemClick(object sender, ItemClickEventArgs e)
        {
            messShow.Show("聊天功能待完成...",3000);
        }
    }
}
