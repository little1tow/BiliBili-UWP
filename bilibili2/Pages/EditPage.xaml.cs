using Newtonsoft.Json.Linq;
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
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public EditPage()
        {
            this.InitializeComponent();
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
            GetLoginInfoModel model = e.Parameter as GetLoginInfoModel;
            dt_Date.MaxYear = new DateTime(2015, 12, 31);
            dt_Date.Date = Convert.ToDateTime(model.birthday);
            txt_UserName.Text = model.name;
            txt_Sign.Text = model.sign;
            switch (model.sex)
            {
                case "保密":
                    rb_B.IsChecked = true;
                    break;
                case "男":
                    rb_N.IsChecked = true;
                    break;
                case "女":
                    rb_V.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private async void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sex = "保密";
                if (rb_B.IsChecked.Value)
                {
                    sex = "保密";
                }
                if (rb_N.IsChecked.Value)
                {
                    sex = "男";
                }
                if (rb_V.IsChecked.Value)
                {
                    sex = "女";
                }
                Uri ReUri = new Uri("https://account.bilibili.com/site/UpdateSetting");
                HttpClient hc = new HttpClient();
                hc.DefaultRequestHeaders.Referer = new Uri("https://account.bilibili.com/site/setting");
                string QuStr = string.Format("uname={0}&sign={1}&sex={2}&birthday={3}", txt_UserName.Text, txt_Sign.Text, sex, dt_Date.Date.Year + "-" + dt_Date.Date.Month + "-" + dt_Date.Date.Day);
                var response = await hc.PostAsync(ReUri, new HttpStringContent(QuStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(result);
                if ((string)json["data"] == "成功更新你的资料")
                {
                    this.Frame.GoBack();
                }
                else
                {
                    await new MessageDialog((string)json["data"]).ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

        }

    }
}
