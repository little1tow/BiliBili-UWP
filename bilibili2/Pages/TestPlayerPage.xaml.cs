using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Display;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPlayerPage : Page
    {
        public TestPlayerPage()
        {
            this.InitializeComponent();
        }

        private DisplayRequest dispRequest = null;


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetPlayInfo(e.Parameter as string, 2);
            if (dispRequest == null)
            {
                // 用户观看视频，需要保持屏幕的点亮状态
                dispRequest = new DisplayRequest();
                dispRequest.RequestActive(); // 激活显示请求
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (dispRequest!=null)
            {
                dispRequest = null;
            }
        }
        private void btn_GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        public async void GetPlayInfo(string mid, int quality)
        {
            //http://interface.bilibili.com/playurl?platform=android&cid=5883400&quality=2&otype=json&appkey=422fd9d7289a1dd9&type=mp4
            try
            {
                WebClientClass wc = new WebClientClass();
                    string results = await wc.GetResults(new Uri("http://interface.bilibili.com/playurl?platform=android&cid=" + mid + "&quality=" + 2 + "&otype=json&appkey=422fd9d7289a1dd9&type=mp4"));
                VideoUriModel model = JsonConvert.DeserializeObject<VideoUriModel>(results);
                    List<VideoUriModel> model1 = JsonConvert.DeserializeObject<List<VideoUriModel>>(model.durl.ToString());
                    mediaElment.Source = new Uri(model1[0].url);
            }
            catch (Exception)
            {
                MessageDialog md = new MessageDialog("视频地址获取失败！", "错误");
                await md.ShowAsync();
            }
        }

        public class VideoUriModel
        {
            public string format { get; set; }//视频类型

            public object durl { get; set; }//视频信息

            public string url { get; set; }//视频地址

            public object backup_url { get; set; }//视频备份地址
        }
    }
}
