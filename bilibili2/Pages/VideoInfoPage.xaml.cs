using bilibili2.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoInfoPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public VideoInfoPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            SystemNavigationManager.GetForCurrentView().BackRequested += VideoInfoPage_BackRequested;
        }
        string aid = "";
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // if (e.NavigationMode== NavigationMode.New)
            // {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            video_Error_Null.Visibility = Visibility.Collapsed;
                video_Error_User.Visibility = Visibility.Collapsed;
                aid = "";
                top_txt_Header.Text = "AV" + e.Parameter as string;
                pivot.SelectedIndex = 0;
                pageNum = 1;
                loadComment = false;
                 loadAbout = false;
                grid_tag.Children.Clear();
                ListView_Comment_New.Items.Clear();
                UpdateUI();
                aid = e.Parameter as string;
                GetVideoInfo(aid);
               
            //}
         
        }
        private void VideoInfoPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                e.Handled = true;
                this.Frame.GoBack();
            }
            else
            {
                e.Handled = true;
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
        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            pivot.SelectedIndex = Convert.ToInt32((sender as Button).Tag);
        }

        bool loadComment = false;
        bool loadAbout = false;
        public void UpdateUI()
        {
            btn_About.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_Coment.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_info.Foreground = new SolidColorBrush(new Color() { A = 178, G = 255, B = 255, R = 255 });
            btn_About.FontWeight = FontWeights.Normal;
            btn_Coment.FontWeight = FontWeights.Normal;
            btn_info.FontWeight = FontWeights.Normal;
            switch (pivot.SelectedIndex)
            {
                case 0:
                    btn_info.Foreground = new SolidColorBrush(Colors.White);
                    btn_info.FontWeight = FontWeights.Bold;
                    com_bar.Visibility = Visibility.Visible;
                    btn_Sort.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    btn_Coment.Foreground = new SolidColorBrush(Colors.White);
                    btn_Coment.FontWeight = FontWeights.Bold;
                    com_bar.Visibility = Visibility.Collapsed;
                    btn_Sort.Visibility = Visibility.Visible;
                    if (!loadComment)
                    {
                        GetVideoComment_New(aid,orderBy);
                        loadComment = true;
                    }
                    break;
                case 2:
                    btn_About.Foreground = new SolidColorBrush(Colors.White);
                    btn_About.FontWeight = FontWeights.Bold;
                    com_bar.Visibility = Visibility.Collapsed;
                    btn_Sort.Visibility = Visibility.Collapsed;
                    if (!loadAbout)
                    {
                        GetRecommend();
                        loadAbout = true;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 读取视频信息
        /// </summary>
        /// <param name="aid"></param>
        private async void GetVideoInfo(string aid)
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
               
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://api.bilibili.com/view?type=json&appkey=422fd9d7289a1dd9&id=" + aid + "&batch=1&"+ApiHelper.access_key+"&rnd=" + new Random().Next(1, 9999)));
                VideoInfoModel model = new VideoInfoModel();
                model = JsonConvert.DeserializeObject<VideoInfoModel>(results);
                if (model.code== -403)
                {
                    video_Error_User.Visibility = Visibility.Visible;
                    return;
                }
                if (model.code==-404)
                {
                    video_Error_Null.Visibility = Visibility.Visible;
                    return;
                }
                Video_Grid_Info.DataContext = model;
               //top_txt_Header.Text = model.typename + "/AV" + aid;
                List<VideoInfoModel> ban = JsonConvert.DeserializeObject<List<VideoInfoModel>>(model.list.ToString());
                foreach (VideoInfoModel item in ban)
                {
                    item.title = model.title;
                    item.aid = aid;
                }
                string[] _tag = model.tag.Split(',');
                foreach (string item in _tag)
                {
                    HyperlinkButton hy = new HyperlinkButton();
                    hy.Content = item;
                    hy.Margin = new Thickness(0, 0, 10, 0);
                    grid_tag.Children.Add(hy);
                }
                Video_List.ItemsSource = ban;
                GetVideoComment_Hot();
            }
            catch (Exception ex)
            {
                //throw;
                messShow.Show("读取视频信息\r\n"+ex.Message,3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }

        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            if (btn_Open.Content.ToString()=="展开")
            {
                txt_Desc.MaxLines = 0;
                btn_Open.Content = "收缩";
            }
            else
            {
                txt_Desc.MaxLines = 2;
                btn_Open.Content = "展开";
            }
           
        }

        int orderBy = 0;
        private int pageNum= 1;
        private async void GetVideoComment_New(string aid, int order)
        {
            try
            {
                CanLoad = false;
                pr_Load.Visibility = Visibility.Visible;
                txt_More.Foreground = new SolidColorBrush(Colors.Gray);
                txt_More.Text = "正在加载...";
                WebClientClass wc= new WebClientClass();
                Random r = new Random();
                string results = await wc.GetResults(new Uri("http://api.bilibili.com/x/reply?jsonp=jsonp&type=1&sort=" + order + "&oid=" + aid + "&pn=" + pageNum + "&nohot=1&ps=20&r=" + r.Next(1000, 99999)));
                CommentModel model = JsonConvert.DeserializeObject<CommentModel>(results);
                CommentModel model3 = JsonConvert.DeserializeObject<CommentModel>(model.data.ToString());
                //Video_Grid_Info.DataContext = model;
                List<CommentModel> ban = JsonConvert.DeserializeObject<List<CommentModel>>(model3.replies.ToString());
                foreach (CommentModel item in ban)
                {
                    CommentModel model1 = new CommentModel();
                    model1 = JsonConvert.DeserializeObject<CommentModel>(item.member.ToString());
                    CommentModel model2 = new CommentModel();
                    model2 = JsonConvert.DeserializeObject<CommentModel>(item.content.ToString());
                    CommentModel resultsModel = new CommentModel()
                    {
                        avatar = model1.avatar,
                        message = model2.message,
                        plat = model2.plat,
                        floor = item.floor,
                        uname = model1.uname,
                        mid = model1.mid,
                        ctime = item.ctime,
                        like = item.like,
                        rcount = item.rcount,
                        rpid = item.rpid
                    };
                    ListView_Comment_New.Items.Add(resultsModel);
                }
                pageNum++;
                if (ban.Count == 0)
                {
                    txt_More.Foreground = new SolidColorBrush(Colors.Gray);
                    txt_More.Text = "没有更多了...";
                }
                if (txt_More.Text!= "没有更多了...")
                {
                    txt_More.Foreground = new SolidColorBrush(Colors.Black);
                    txt_More.Text = "加载更多";
                }
            }
            catch (Exception ex)
            {
                messShow.Show("读取评论失败!\r\n"+ex.Message,3000);
            }
            finally
            {
                CanLoad = true;
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private async void GetVideoComment_Hot()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                ListView_Comment_Hot.Items.Clear();
                WebClientClass wc = new WebClientClass();
                Random r = new Random();
                string results = await wc.GetResults(new Uri("http://api.bilibili.com/x/reply?jsonp=jsonp&type=1&sort=" + 2 + "&oid=" + aid + "&pn=" + 1 + "&nohot=1&ps=5&r=" + r.Next(1000, 99999)));
                CommentModel model = JsonConvert.DeserializeObject<CommentModel>(results);
                CommentModel model3 = JsonConvert.DeserializeObject<CommentModel>(model.data.ToString());
                //Video_Grid_Info.DataContext = model;
                List<CommentModel> ban = JsonConvert.DeserializeObject<List<CommentModel>>(model3.replies.ToString());
                foreach (CommentModel item in ban)
                {
                    CommentModel model1 = new CommentModel();
                    model1 = JsonConvert.DeserializeObject<CommentModel>(item.member.ToString());
                    CommentModel model2 = new CommentModel();
                    model2 = JsonConvert.DeserializeObject<CommentModel>(item.content.ToString());
                    CommentModel resultsModel = new CommentModel()
                    {
                        avatar = model1.avatar,
                        message = model2.message,
                        plat = model2.plat,
                        floor = item.floor,
                        uname = model1.uname,
                        mid = model1.mid,
                        ctime = item.ctime,
                        like = item.like,
                        rcount = item.rcount,
                        rpid = item.rpid
                    };
                    ListView_Comment_Hot.Items.Add(resultsModel);
                }
            }
            catch (Exception ex)
            {
                messShow.Show("读取热门评论失败!\r\n" + ex.Message, 3000);
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void menu_Time_Click(object sender, RoutedEventArgs e)
        {
            menu_Comment.IsChecked = false;
            menu_Good.IsChecked = false;
            //menu_Time.IsEnabled = false;
            menu_Time.IsEnabled = false;
            menu_Comment.IsEnabled = true;
            menu_Good.IsEnabled = true;
            orderBy = 0;
            pageNum = 1;
            ListView_Comment_New.Items.Clear();
            GetVideoComment_New(aid, orderBy);
        }

        private void menu_Good_Click(object sender, RoutedEventArgs e)
        {
            menu_Time.IsChecked = false;
            menu_Comment.IsChecked = false;
            //menu_Time.IsEnabled = true;
            menu_Time.IsEnabled = true;
            menu_Comment.IsEnabled = true;
            menu_Good.IsEnabled = false;
            orderBy = 2;
            pageNum = 1;
            ListView_Comment_New.Items.Clear();
            GetVideoComment_New(aid, orderBy);
        }

        private void menu_Comment_Click(object sender, RoutedEventArgs e)
        {
            menu_Time.IsChecked = false;
            menu_Good.IsChecked = false;
            //menu_Time.IsEnabled = true;
            menu_Time.IsEnabled = true;
            menu_Comment.IsEnabled = false;
            menu_Good.IsEnabled = true;
            orderBy = 1;
            pageNum = 1;
            ListView_Comment_New.Items.Clear();
            GetVideoComment_New(aid, orderBy);
        }
        bool CanLoad = true;
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sc.VerticalOffset == sc.ScrollableHeight)
            {
                if (CanLoad)
                {
                    GetVideoComment_New(aid, orderBy);
                }
            }
        }

        private async void GetRecommend()
        {
            try
            {
                pr_Load.Visibility = Visibility.Visible;
                WebClientClass wc = new WebClientClass();
                string results = await wc.GetResults(new Uri("http://comment.bilibili.com/recommend," + aid));
                List<RecommendModel> ban = JsonConvert.DeserializeObject<List<RecommendModel>>(results);
                list_About.ItemsSource = ban;
            }
            catch (Exception ex)
            {
                messShow.Show("读取相关视频失败\r\n" + ex.Message, 3000);
                //throw;
            }
            finally
            {
                pr_Load.Visibility = Visibility.Collapsed;
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), (e.ClickedItem as RecommendModel).id);
        }

        private void Video_List_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(TestPlayerPage), (e.ClickedItem as VideoInfoModel).cid);
        }

        private void btn_playP1_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TestPlayerPage), (Video_List.Items[0] as VideoInfoModel).cid);
        }
    }
}
