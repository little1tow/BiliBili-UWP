using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class BanTimelinePage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public BanTimelinePage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
           
        }
       
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode== NavigationMode.New)
            {
                SetWeekInfo();
                GetBangumiTimeLine();
                bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
                //top_Grid.Background = (SolidColorBrush)this.Frame.Tag;
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
        int taday = 0;
        public void SetWeekInfo()
        {
            date_2.Text = DateTime.Now.AddDays(-2).Date.Month + "月" + DateTime.Now.AddDays(-2).Date.Day + "日";
            date_3.Text = DateTime.Now.AddDays(-3).Date.Month + "月" + DateTime.Now.AddDays(-3).Date.Day + "日";
            date_4.Text = DateTime.Now.AddDays(-4).Date.Month + "月" + DateTime.Now.AddDays(-4).Date.Day + "日";
            date_5.Text = DateTime.Now.AddDays(-5).Date.Month + "月" + DateTime.Now.AddDays(-5).Date.Day + "日";
            date_6.Text = DateTime.Now.AddDays(-6).Date.Month + "月" + DateTime.Now.AddDays(-6).Date.Day + "日";

            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    week_0.Text = "周一";
                    week_1.Text = "周日";
                    week_2.Text = "周六";
                    week_3.Text = "周五";
                    week_4.Text = "周四";
                    week_5.Text = "周三";
                    week_5.Text = "周二";
                    taday = 1;
                    break;
                case DayOfWeek.Tuesday:
                    week_1.Text = "周一";
                    week_2.Text = "周日";
                    week_3.Text = "周六";
                    week_4.Text = "周五";
                    week_5.Text = "周四";
                    week_6.Text = "周三";
                    week_0.Text = "周二";
                    taday = 2;
                    break;
                case DayOfWeek.Wednesday:
                    week_2.Text = "周一";
                    week_3.Text = "周日";
                    week_4.Text = "周六";
                    week_5.Text = "周五";
                    week_6.Text = "周四";
                    week_0.Text = "周三";
                    week_1.Text = "周二";
                    taday = 3;
                    break;
                case DayOfWeek.Thursday:
                    week_3.Text = "周一";
                    week_4.Text = "周日";
                    week_5.Text = "周六";
                    week_6.Text = "周五";
                    week_0.Text = "周四";
                    week_1.Text = "周三";
                    week_2.Text = "周二";
                    taday = 4;
                    break;
                case DayOfWeek.Friday:
                    week_4.Text = "周一";
                    week_5.Text = "周日";
                    week_6.Text = "周六";
                    week_0.Text = "周五";
                    week_1.Text = "周四";
                    week_2.Text = "周三";
                    week_3.Text = "周二";
                    taday = 5;
                    break;
                case DayOfWeek.Saturday:
                    week_5.Text = "周一";
                    week_6.Text = "周日";
                    week_0.Text = "周六";
                    week_1.Text = "周五";
                    week_2.Text = "周四";
                    week_3.Text = "周三";
                    week_4.Text = "周二";
                    taday = 6;
                    break;
                case DayOfWeek.Sunday:
                    week_6.Text = "周一";
                    week_0.Text = "周日";
                    week_1.Text = "周六";
                    week_2.Text = "周五";
                    week_3.Text = "周四";
                    week_4.Text = "周三";
                    week_5.Text = "周二";
                    taday = 0;
                    break;
                default:
                    break;
            }
        }
        //时间表
        public async void GetBangumiTimeLine()
        {
            try
            {
                pr_Load_Ban.Visibility = Visibility.Visible;
               WebClientClass  wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://app.bilibili.com/bangumi/timeline_v2"));
                BangumiTimeLineModel model = new BangumiTimeLineModel();
                model = JsonConvert.DeserializeObject<BangumiTimeLineModel>(results);
                List<BangumiTimeLineModel> ban = JsonConvert.DeserializeObject<List<BangumiTimeLineModel>>(model.list.ToString());
                list_0.Items.Clear();
                list_1.Items.Clear();
                list_2.Items.Clear();
                list_3.Items.Clear();
                list_4.Items.Clear();
                list_5.Items.Clear();
                list_6.Items.Clear();
                list_7.Items.Clear();
                foreach (BangumiTimeLineModel item in ban)
                {
                    switch (item.weekday)
                    {
                        case -1:
                            list_7.Items.Add(item);
                            break;
                        case 0:
                            switch (taday)
                            {
                                case 0:
                                    list_0.Items.Add(item);
                                    break;
                                case 1:
                                    list_1.Items.Add(item);
                                    break;
                                case 2:
                                    list_2.Items.Add(item);
                                    break;
                                case 3:
                                    list_3.Items.Add(item);
                                    break;
                                case 4:
                                    list_4.Items.Add(item);
                                    break;
                                case 5:
                                    list_5.Items.Add(item);
                                    break;
                                case 6:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 1:
                            switch (taday)
                            {
                                case 1:
                                    list_0.Items.Add(item);
                                    break;
                                case 2:
                                    list_1.Items.Add(item);
                                    break;
                                case 3:
                                    list_2.Items.Add(item);
                                    break;
                                case 4:
                                    list_3.Items.Add(item);
                                    break;
                                case 5:
                                    list_4.Items.Add(item);
                                    break;
                                case 6:
                                    list_5.Items.Add(item);
                                    break;
                                case 0:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 2:
                            switch (taday)
                            {
                                case 2:
                                    list_0.Items.Add(item);
                                    break;
                                case 3:
                                    list_1.Items.Add(item);
                                    break;
                                case 4:
                                    list_2.Items.Add(item);
                                    break;
                                case 5:
                                    list_3.Items.Add(item);
                                    break;
                                case 6:
                                    list_4.Items.Add(item);
                                    break;
                                case 0:
                                    list_5.Items.Add(item);
                                    break;
                                case 1:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 3:
                            switch (taday)
                            {
                                case 3:
                                    list_0.Items.Add(item);
                                    break;
                                case 4:
                                    list_1.Items.Add(item);
                                    break;
                                case 5:
                                    list_2.Items.Add(item);
                                    break;
                                case 6:
                                    list_3.Items.Add(item);
                                    break;
                                case 0:
                                    list_4.Items.Add(item);
                                    break;
                                case 1:
                                    list_5.Items.Add(item);
                                    break;
                                case 2:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 4:
                            switch (taday)
                            {
                                case 4:
                                    list_0.Items.Add(item);
                                    break;
                                case 5:
                                    list_1.Items.Add(item);
                                    break;
                                case 6:
                                    list_2.Items.Add(item);
                                    break;
                                case 0:
                                    list_3.Items.Add(item);
                                    break;
                                case 1:
                                    list_4.Items.Add(item);
                                    break;
                                case 2:
                                    list_5.Items.Add(item);
                                    break;
                                case 3:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 5:
                            switch (taday)
                            {
                                case 5:
                                    list_0.Items.Add(item);
                                    break;
                                case 6:
                                    list_1.Items.Add(item);
                                    break;
                                case 0:
                                    list_2.Items.Add(item);
                                    break;
                                case 1:
                                    list_3.Items.Add(item);
                                    break;
                                case 2:
                                    list_4.Items.Add(item);
                                    break;
                                case 3:
                                    list_5.Items.Add(item);
                                    break;
                                case 4:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 6:
                            switch (taday)
                            {
                                case 6:
                                    list_0.Items.Add(item);
                                    break;
                                case 0:
                                    list_1.Items.Add(item);
                                    break;
                                case 1:
                                    list_2.Items.Add(item);
                                    break;
                                case 2:
                                    list_3.Items.Add(item);
                                    break;
                                case 3:
                                    list_4.Items.Add(item);
                                    break;
                                case 4:
                                    list_5.Items.Add(item);
                                    break;
                                case 5:
                                    list_6.Items.Add(item);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                }
                pr_Load_Ban.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog("读取番剧更新失败\r\n" + ex.Message);
                await md.ShowAsync();
            }
        }
       //番剧时间表点击
        private void list_0_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(BanInfoPage), (e.ClickedItem as BangumiTimeLineModel).season_id);
        }
    }
}
