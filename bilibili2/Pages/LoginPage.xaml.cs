using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace bilibili2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public event GoBackHandler LoginEd;
        public LoginPage()
        {
            this.InitializeComponent();
            //BackEvent
            SystemNavigationManager.GetForCurrentView().BackRequested += LoginPage_BackRequested;
        }

        private void LoginPage_BackRequested(object sender, BackRequestedEventArgs e)
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetPass();
        }

        private async void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            if (txt_User.Text.Length == 0)
            {
                MessageDialog md = new MessageDialog("账号或密码不能为空！");
                await md.ShowAsync();
            }
            else
            {
                if (cb_SavaPass.IsChecked == true)
                {
                    SavePass();
                    container.Values["AutoLogin"] = "true";
                }
                else
                {
                    container.Values["AutoLogin"] = "fasle";
                }
                sc.IsEnabled = false;
                btn_Login.Content = "正在登录";
                pr_Load.Visibility = Visibility.Visible;
                 string result= await  ApiHelper.LoginBilibili(txt_User.Text, txt_Pass.Password);
                if (result=="登录成功")
                {
                    LoginEd();
                    BackEvent();
                }
                else
                {
                    await new MessageDialog(result).ShowAsync();
                }
                pr_Load.Visibility = Visibility.Collapsed;
                btn_Login.Content = "登录";
                sc.IsEnabled = true;
            }

        }
        ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        private void GetPass()
        {
            //用户
            if (container.Values["UserName"] != null && container.Values["UserPass"] != null)
            {
                txt_User.Text = container.Values["UserName"].ToString();
                txt_Pass.Password = container.Values["UserPass"].ToString();
            }
            else
            {
                container.Values["UserName"] = "";
                container.Values["UserPass"] = "";
            }
        }
        private void SavePass()
        {
            container.Values["UserName"] = txt_User.Text;
            container.Values["UserPass"] = txt_Pass.Password;
        }

        private void btn_GetCookies_Click(object sender, RoutedEventArgs e)
        {
            HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
            //httpBaseProtocolFilter.CookieManager.SetCookie(new HttpCookie("access_key", "bilibili.com", access_key));
            var cookies = httpBaseProtocolFilter.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
            
            foreach (HttpCookie item in cookies)
            {
                txt_Status.Text += "\r\n" + item.Domain + "\t" + item.Name + "\t" + item.Value;
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

        private void Login_Pass_LostFocus(object sender, RoutedEventArgs e)
        {
            KanZheNi.Visibility = Visibility.Visible;
            BuKanZheNi.Visibility = Visibility.Collapsed;
        }

        private void Login_Pass_GotFocus(object sender, RoutedEventArgs e)
        {
            KanZheNi.Visibility = Visibility.Collapsed;
            BuKanZheNi.Visibility = Visibility.Visible;
        }

        private async void Login_Pass_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (txt_User.Text.Length == 0)
            {
                MessageDialog md = new MessageDialog("账号或密码不能为空！");
                await md.ShowAsync();
            }
            else
            {
                if (cb_SavaPass.IsChecked == true)
                {
                    SavePass();
                    container.Values["AutoLogin"] = "true";
                }
                else
                {
                    container.Values["AutoLogin"] = "fasle";
                }
                sc.IsEnabled = false;
                btn_Login.Content = "正在登录";
                pr_Load.Visibility = Visibility.Visible;
                string result = await ApiHelper.LoginBilibili(txt_User.Text, txt_Pass.Password);
                if (result == "登录成功")
                {
                    LoginEd();
                    BackEvent();
                }
                else
                {
                    await new MessageDialog(result).ShowAsync();
                }
                pr_Load.Visibility = Visibility.Collapsed;
                btn_Login.Content = "登录";
                sc.IsEnabled = true;
            }
        }
    }
}
