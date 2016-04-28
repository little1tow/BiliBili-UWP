using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
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

        string url ;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            url = e.Parameter as string;
            if (dispRequest == null)
            {
                // 用户观看视频，需要保持屏幕的点亮状态
                dispRequest = new DisplayRequest();
                dispRequest.RequestActive(); // 激活显示请求
            }
            DisplayInformation.AutoRotationPreferences = (DisplayOrientations)5;
            string urls=await GetDiliVideoUri(url);
            if (urls!=string.Empty)
            {
                mediaElment.Source = new Uri(urls);
            }
            else
            {
                await new MessageDialog("读取地址失败").ShowAsync();
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (dispRequest!=null)
            {
                dispRequest = null;
            }
            DisplayInformation.AutoRotationPreferences =  DisplayOrientations.None;
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

        public async Task<string> GetDiliVideoUri(string URL)
        {
            try
            {
                HttpClient hc = new HttpClient();
            hc.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Linux; Android 5.0; SM-N9100 Build/LRX21V) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/37.0.0.0 Mobile Safari/537.36 MicroMessenger/6.0.2.56_r958800.520 NetType/WIFI");
            hc.DefaultRequestHeaders.Add("Referer", URL);
            HttpResponseMessage hr = await hc.GetAsync(new Uri(URL));
            hr.EnsureSuccessStatusCode();
            //   string content =await hr.Content.ReadAsStringAsync();
            var encodeResults = await hr.Content.ReadAsBufferAsync();
            string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);
            Match mc = Regex.Match(results, @"<iframe src=""(.*?)""", RegexOptions.Multiline);
            HttpResponseMessage hr1 = await hc.GetAsync(new Uri(mc.Groups[1].Value));
            hr1.EnsureSuccessStatusCode();
            string a = await hr1.Content.ReadAsStringAsync();
            Match mc2 = Regex.Match(a, @"var vid=""(.*?)"";
var hd2=""(.*?)"";
var typ=""(.*?)"";
var sign=""(.*?)"";
var ulk=""(.*?)"";");
            Match mc3 = Regex.Match(a, @"&tmsign=(.*?)&ajax");
            string vid, hd2, typ, sign, ulk;
            vid = mc2.Groups[1].Value;
            hd2 = mc2.Groups[2].Value;
            typ = mc2.Groups[3].Value;
            sign = mc2.Groups[4].Value;
            ulk = mc2.Groups[5].Value;
            string url = "https://player.005.tv:60000/parse.php?h5url=null&type=" + typ + "&vid=" + vid + "&hd=" + 3 + "&sign=" + sign + "&tmsign=" + mc3.Groups[1].Value + "&ajax=1&userlink=" + ulk;
            HttpResponseMessage hr3 = await hc.GetAsync(new Uri(url));
            hr3.EnsureSuccessStatusCode();
            string c = await hr3.Content.ReadAsStringAsync();
            return c;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
