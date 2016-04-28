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
    public sealed partial class DiliDiliPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public DiliDiliPage()
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
            cb_Year.SelectedIndex = 0;
            cb_Month.SelectedIndex=1;
            ControlLoaded = true;
            GetListInfo("201604");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (wc!=null)
            {
                wc = null;
            }
        }
        WebClientClass wc;
        private async void GetListInfo(string date)
        {
            try
            {
                pr_load.Visibility = Visibility.Visible;
                wc = new WebClientClass();
                string url = string.Format("http://www.dilidili.com/anime/{0}/", date);
                string results = await wc.GetResultsUTF8Encode(new Uri(url));
                MatchCollection mc = Regex.Matches(results, @"<div class=""item"">.*?<div class=""content"">.*?<div class=""title"">.*?<a href=""(.*?)"" target=""_blank"">(.*?)</a>.*?<span class=""data""><b>年代：</b>(.*?)</span>.*?</div>.*?<p><b>地区：</b>(.*?)</p>.*?<p><b>类型：</b>(.*?)</p>.*?<p><b>看点：</b>(.*?)</p>.*?<p><b>简介：</b>(.*?)</p>.*?</div>.*?<div class=""cover"">.*?<a href=""(.*?)"" target=""_blank"">.*?<img src=""(.*?)"">.*?</a>.*?</div>", RegexOptions.Singleline);
                List<DiliModel> listModel = new List<DiliModel>();
                foreach (Match item in mc)
                {
                    listModel.Add(new DiliModel()
                    {
                        url = item.Groups[1].Value,
                        title = item.Groups[2].Value,
                        date = item.Groups[3].Value,
                        area = item.Groups[4].Value,
                        tag = item.Groups[5].Value,
                        shortDesc = item.Groups[6].Value,
                        desc = item.Groups[7].Value,
                        img = item.Groups[9].Value
                    });
                }
                list_Info.ItemsSource = listModel;
            }
            catch (Exception)
            {
                messShow.Show("读取失败",3000);
            }
            finally
            {
                pr_load.Visibility = Visibility.Collapsed;
            }
             
        }

        bool ControlLoaded = false;
        private void cb_Year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ControlLoaded)
            {
                return;
            }
            if (cb_Year.SelectedIndex!=7&&cb_Year.SelectedIndex!=8&&cb_Month.SelectedIndex!=-1)
            {
                GetListInfo((cb_Year.SelectedItem as ComboBoxItem).Tag.ToString()+ (cb_Month.SelectedItem as ComboBoxItem).Tag.ToString());
            }
            if (cb_Year.SelectedIndex==7|| cb_Year.SelectedIndex == 8)
            {
                GetListInfo((cb_Year.SelectedItem as ComboBoxItem).Tag.ToString());
            }
        }

        private void cb_Month_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ControlLoaded)
            {
                return;
            }
            if (cb_Year.SelectedIndex != 7 && cb_Year.SelectedIndex != 8 && cb_Month.SelectedIndex != -1)
            {
                GetListInfo((cb_Year.SelectedItem as ComboBoxItem).Tag.ToString() + (cb_Month.SelectedItem as ComboBoxItem).Tag.ToString());
            }
            if (cb_Year.SelectedIndex == 7 || cb_Year.SelectedIndex == 8)
            {
                GetListInfo((cb_Year.SelectedItem as ComboBoxItem).Tag.ToString());
            }

        }


        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(DiliInfo), e.ClickedItem as DiliModel);
        }
    }

    public class DiliModel
    {
        public string url { get; set; }
        public string title { get; set; }
        public string date { get; set; }
        public string area { get; set; }
        public string tag { get; set; }
        public string desc { get; set; }
        public string shortDesc { get; set; }
        public string img { get; set; }
    }

}
