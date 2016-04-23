using bilibili2.Class;
using bilibili2.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

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
        }
        string aid = "";
        bool Back = false;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            if (e.NavigationMode == NavigationMode.New||Back)
            {
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
               GetFavBox();
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //Video_Grid_Info.DataContext = null;
            //Video_UP.DataContext = null;
            //Video_data.DataContext = null;
            //ListView_Comment_Hot.Items.Clear();
        }
        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                Video_Grid_Info.DataContext = null;
                Video_UP.DataContext = null;
                Video_data.DataContext = null;
                ListView_Comment_Hot.Items.Clear();
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
                string uri = string.Format("http://app.bilibili.com/x/view?_device=wp&_ulv=10000&access_key={0}&aid={1}&appkey=422fd9d7289a1dd9&build=411005&plat=4&platform=android&ts={2}",ApiHelper.access_key,aid,ApiHelper.GetTimeSpen);
                uri += "&sign="+ApiHelper.GetSign(uri);
                //string results = await wc.GetResults(new Uri("http://api.bilibili.com/view?type=json&appkey=422fd9d7289a1dd9&id=" + aid + "&batch=1&"+ApiHelper.access_key+"&rnd=" + new Random().Next(1, 9999)));
                string results = await wc.GetResults(new Uri(uri));
                VideoModel model = new VideoModel();
                model = JsonConvert.DeserializeObject<VideoModel>(results);
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
                //基础信息
                VideoModel InfoModel = JsonConvert.DeserializeObject<VideoModel>(model.data.ToString());
                //UP信息
                VideoModel UpModel = JsonConvert.DeserializeObject<VideoModel>(InfoModel.owner.ToString());
                //数据信息
                VideoModel DataModel = JsonConvert.DeserializeObject<VideoModel>(InfoModel.stat.ToString());
                //关注信息
                VideoModel AttentionModel = JsonConvert.DeserializeObject<VideoModel>(InfoModel.req_user.ToString());
                UpModel.pubdate = InfoModel.pubdate;
                Video_Grid_Info.DataContext = InfoModel;
                Video_UP.DataContext = UpModel;
                Video_data.DataContext = DataModel;
               //top_txt_Header.Text = model.typename + "/AV" + aid;
                List<VideoModel> ban = JsonConvert.DeserializeObject<List<VideoModel>>(InfoModel.pages.ToString());
                foreach (VideoModel item in ban)
                {
                    item.title = InfoModel.title;
                    item.aid = aid;
                }
                Video_List.ItemsSource = ban;
                List<string> _tag = JsonConvert.DeserializeObject<List<string>>(InfoModel.tags.ToString());
                //string[] _tag = model.tag.Split(',');
                foreach (string item in _tag)
                {
                    HyperlinkButton hy = new HyperlinkButton();
                    hy.Content = item;
                    hy.Margin = new Thickness(0, 0, 10, 0);
                    hy.Click += Hy_Click;
                    grid_tag.Children.Add(hy);
                }
                if (AttentionModel.attention==1)
                {
                    txt_guanzhu.Text = "已关注";
                }
                else
                {
                    txt_guanzhu.Text = "关注";
                }
                //只能读取第一个收藏夹信息 - -！
                //if (AttentionModel.favorite == "1")
                //{
                //    btn_Favbox.Icon = new SymbolIcon(Symbol.SolidStar);
                //    btn_Favbox.Label = "已收藏";
                //}
                //else
                //{
                //    btn_Favbox.Icon = new SymbolIcon(Symbol.OutlineStar);
                //    btn_Favbox.Label = "收藏";
                //}

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

        private void Hy_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SearchPage), (sender as HyperlinkButton).Content.ToString());
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
                    CommentModel modelLV = JsonConvert.DeserializeObject<CommentModel>(model1.level_info.ToString());
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
                        rpid = item.rpid,
                        current_level=modelLV.current_level
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
                    CommentModel model1 = JsonConvert.DeserializeObject<CommentModel>(item.member.ToString());
                    CommentModel model2 = JsonConvert.DeserializeObject<CommentModel>(item.content.ToString());
                    CommentModel modelLV = JsonConvert.DeserializeObject<CommentModel>(model1.level_info.ToString());
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
                        rpid = item.rpid,
                        current_level= modelLV.current_level
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
            Back = true;
            this.Frame.Navigate(typeof(VideoInfoPage), (e.ClickedItem as RecommendModel).id);
            Video_Grid_Info.DataContext = null;
            Video_UP.DataContext = null;
            Video_data.DataContext = null;
            ListView_Comment_Hot.Items.Clear();
        }

        private void Video_List_ItemClick(object sender, ItemClickEventArgs e)
        {
            List<VideoModel> list = (List<VideoModel>)Video_List.ItemsSource;
            KeyValuePair<List<VideoModel>, int> Par = new KeyValuePair<List<VideoModel>, int>(list, list.IndexOf((VideoModel)e.ClickedItem));
            PostHistory();
            this.Frame.Navigate(typeof(PlayerPage), Par);
            //this.Frame.Navigate(typeof(PlayerPage), (e.ClickedItem as VideoModel).cid);
        }

        private void btn_playP1_Click(object sender, RoutedEventArgs e)
        {
            List<VideoModel> list= (List<VideoModel>)Video_List.ItemsSource;
            KeyValuePair<List<VideoModel>, int> Par = new KeyValuePair<List<VideoModel>, int>(list,0);
            PostHistory();
            this.Frame.Navigate(typeof(PlayerPage),Par);
        }

        private void btn_TB_1_Click(object sender, RoutedEventArgs e)
        {
            TouBi(1);
        }

        private void btn_TB_2_Click(object sender, RoutedEventArgs e)
        {
            TouBi(2);
        }

        private void btn_No_Click(object sender, RoutedEventArgs e)
        {
            grid_Tb.Hide();
        }

        private async void Video_ListView_Favbox_ItemClick(object sender, ItemClickEventArgs e)
        {
            UserClass getLogin = new UserClass();
            if (getLogin.IsLogin())
            {
                try
                {
                    WebClientClass wc = new WebClientClass();
                    Uri ReUri = new Uri("http://api.bilibili.com/x/favourite/video/add");
                  
                    string QuStr = "jsonp=jsonp&fid=" + ((FavboxModel)e.ClickedItem).fid + "&aid=" + aid;

                    string result = await wc.PostResults(ReUri, QuStr);
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        messShow.Show("收藏成功！", 2000);
                        GetFavBox();
                    }
                    else
                    {
                        if ((int)json["code"] == 11007)
                        {
                            messShow.Show("视频已经收藏！", 2000);
                            //MessageDialog md = new MessageDialog("视频已经收藏！");
                            //await md.ShowAsync();
                        }
                        else
                        {
                            messShow.Show("收藏失败！\r\n"+ result, 2000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    messShow.Show("收藏失败！"+ex.Message, 2000);
                }
            }
            else
            {
                messShow.Show("请先登录！", 2000);
            }
        }

        private void Video_Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetVideoInfo(aid);
        }

        private async void btn_VideoInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog md = new MessageDialog("AV号：" + aid + "\r\n分P数量：" + Video_List.Items.Count, "视频信息");
            await md.ShowAsync();
        }

        private async void btn_GoBrowser_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.bilibili.com/video/av" + aid));
        }

        private async void btn_SaveImage_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker save = new FileSavePicker();
            save.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            save.FileTypeChoices.Add("图片", new List<string>() { ".jpg" });
            save.SuggestedFileName = "bili_img_" + aid;
            StorageFile file = await save.PickSaveFileAsync();
            if (file != null)
            {
                //img_Image
                WebClientClass wc = new WebClientClass();
                IBuffer bu = await wc.GetBuffer(new Uri((Video_Grid_Info.DataContext as VideoInfoModel).pic));
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, bu);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                messShow.Show("保存成功",3000);
            }
        }

        /// <summary>
        /// 读取收藏夹信息，用于收藏视频
        /// </summary>
        private async void GetFavBox()
        {
            UserClass getLogin = new UserClass();
            if (getLogin.IsLogin())
            {
                try
                {
                    string results = await new WebClientClass().GetResults(new Uri("http://api.bilibili.com/x/favourite/folder?jsonp=jsonp&&rnd=" + new Random().Next(1, 9999)));
                    FavboxModel model = JsonConvert.DeserializeObject<FavboxModel>(results);
                    List<FavboxModel> ban = JsonConvert.DeserializeObject<List<FavboxModel>>(model.data.ToString());
                    Video_ListView_Favbox.ItemsSource = ban;
                }
                catch (Exception ex)
                {
                    FavBox_Header.Text = "获取失败！" + ex.Message;
                }
            }
            else
            {
                FavBox_Header.Text = "请先登录！";
                Video_ListView_Favbox.IsEnabled = false;
            }
        }
        /// <summary>
        /// 投币
        /// </summary>
        /// <param name="num">数量</param>
        public async void TouBi(int num)
        {
            UserClass getLogin = new UserClass();
            if (getLogin.IsLogin())
            {
                try
                {
                    WebClientClass wc = new WebClientClass();
                    Uri ReUri = new Uri("http://www.bilibili.com/plus/comment.php");
                    string QuStr = "aid=" + aid + "&rating=100&player=1&multiply=" + num;
                    string result = await wc.PostResults(ReUri, QuStr);
                    if (result == "OK")
                    {
                        messShow.Show("投币成功！", 3000);
                    }
                    else
                    {
                        messShow.Show("投币失败！"+ result, 3000);
                    }
                }
                catch (Exception ex)
                {
                    messShow.Show("投币时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                messShow.Show("请先登录!",3000);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            txt_Com_1.Text += ((Button)sender).Content.ToString();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            txt_Com.Text += ((Button)sender).Content.ToString();
        }
        int ps = 1;
        string rootsid = "";
        private async void ListView_Comment_New_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.ActualWidth>500)
            {
                sp_Comment.IsPaneOpen = true;
                ListView_Flyout.Items.Clear();
                ps = 1;
                rootsid = ((CommentModel)e.ClickedItem).rpid;
                await GetComments(aid, ((CommentModel)e.ClickedItem).rpid);
            }
            else
            {
                object[] o = new object[] { (CommentModel)e.ClickedItem ,aid};
                this.Frame.Navigate(typeof(CommentPage), o);
            }
        }
        private async Task GetComments(string aid, string rootid)
        {
            try
            {
                Comment_loading.Visibility = Visibility.Visible;
                btn_Load_More.Content = "加载中....";
                btn_Load_More.IsEnabled = false;
                WebClientClass  wc = new WebClientClass();
                Random r = new Random();
                string results = await wc.GetResults(new Uri("http://api.bilibili.com/x/reply/reply?oid=" + aid + "&pn=1&ps=20&root=" + rootid + "&type=1&r=" + r.Next(1000, 99999)));
                CommentModel model = JsonConvert.DeserializeObject<CommentModel>(results);
                CommentModel model3 = JsonConvert.DeserializeObject<CommentModel>(model.data.ToString());
                List<CommentModel> ban = JsonConvert.DeserializeObject<List<CommentModel>>(model3.replies.ToString());
                ListView_Flyout.Items.Clear();
                foreach (CommentModel item in ban)
                {
                    CommentModel model1 = new CommentModel();
                    model1 = JsonConvert.DeserializeObject<CommentModel>(item.member.ToString());
                    CommentModel model2 = new CommentModel();
                    model2 = JsonConvert.DeserializeObject<CommentModel>(item.content.ToString());
                    CommentModel modelLV = JsonConvert.DeserializeObject<CommentModel>(model1.level_info.ToString());
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
                        rpid = item.rpid,
                        current_level=modelLV.current_level
                    };
                    ListView_Flyout.Items.Add(resultsModel);
                    if (ban.Count == 0)
                    {
                        btn_Load_More.Content = "加载完了...";
                        btn_Load_More.IsEnabled = false;
                    }
                    else
                    {
                        btn_Load_More.Content = "加载更多";
                        btn_Load_More.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                messShow.Show("读取评论失败\r\n" + ex.Message, 3000);
            }
            finally
            {
                Comment_loading.Visibility = Visibility.Collapsed;
            }
        }

        private async Task GetComments(string aid, string rootid, int num)
        {
            try
            {
                Comment_loading.Visibility = Visibility.Visible;
                btn_Load_More.Content = "加载中....";
                btn_Load_More.IsEnabled = false;
                WebClientClass wc = new WebClientClass();
                Random r = new Random();
                string results = await wc.GetResults(new Uri("http://api.bilibili.com/x/reply/reply?oid=" + aid + "&pn=" + num + "&ps=20&root=" + rootid + "&type=1&r=" + r.Next(1000, 99999)));
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
                    CommentModel modelLV = JsonConvert.DeserializeObject<CommentModel>(model1.level_info.ToString());
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
                        rpid = item.rpid,
                        current_level=modelLV.current_level
                    };
                    ListView_Flyout.Items.Add(resultsModel);
                }
                if (ban.Count == 0)
                {
                    btn_Load_More.Content = "加载完了...";
                    btn_Load_More.IsEnabled = false;
                }
                else
                {
                    btn_Load_More.Content = "加载更多";
                    btn_Load_More.IsEnabled = true;
                }

            }
            catch (Exception ex)
            {
                messShow.Show("读取评论失败\r\n" + ex.Message,3000);
            }
            finally
            {
                Comment_loading.Visibility = Visibility.Collapsed;
            }
        }


        private void btn_Favbox_Click(object sender, RoutedEventArgs e)
        {
             flyout_Favbox.ShowAt(btn_Favbox);
        }
        string root = "";
        private async void btn_AttUp_Click(object sender, RoutedEventArgs e)
        {
            UserClass getUser = new UserClass();
            if (getUser.IsLogin())
            {
                try
                {
                    Uri ReUri;
                    if (txt_guanzhu.Text == "关注")
                    {
                        ReUri = new Uri("http://space.bilibili.com/ajax/friend/AddAttention");
                    }
                    else
                    {
                        ReUri = new Uri("http://space.bilibili.com/ajax/friend/DelAttention");
                    }
                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://space.bilibili.com/");
                    var response = await hc.PostAsync(ReUri, new HttpStringContent("mid=" + (Video_UP.DataContext as VideoModel).mid, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((bool)json["status"])
                    {
                        if (txt_guanzhu.Text == "关注")
                        {
                            messShow.Show("关注成功",3000);
                            txt_guanzhu.Text = "已关注";
                        }
                        else
                        {
                            messShow.Show("取消关注成功", 3000);
                            txt_guanzhu.Text = "关注";
                        }
                    }
                    else
                    {
                        messShow.Show("关注失败\r\n" + result, 3000);
                        txt_guanzhu.Text = "关注";
                    }

                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message,"关注时发生错误").ShowAsync();
                }
            }
            else
            {
                messShow.Show("请先登录",3000);
                //MessageDialog md = new MessageDialog("请先登录！");
               // await md.ShowAsync();
            }

        }

        private void ListView_Flyout_ItemClick(object sender, ItemClickEventArgs e)
        {
            root = (e.ClickedItem as CommentModel).rpid;
            txt_Com_1.Text = "回复 @" + (e.ClickedItem as CommentModel).uname + ":";
        }
        //头像点击
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            CommentModel model = (sender as HyperlinkButton).DataContext as CommentModel;
            this.Frame.Navigate(typeof(UserInfoPage), model.mid);
        }

        private async void btn_Zan_Click(object sender, RoutedEventArgs e)
        {
            string rpid = ((sender as HyperlinkButton).DataContext as CommentModel).rpid;
            UserClass getUser = new UserClass();
            if (getUser.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/reply/action");

                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    string sendString = "";
                    if (((sender as HyperlinkButton).Content as TextBlock).Text == "赞同")
                    {
                        sendString = "jsonp=jsonp&oid=" + aid + "&type=1&rpid=" + rpid + "&action=1";
                    }
                    else
                    {
                        sendString = "jsonp=jsonp&oid=" + aid + "&type=1&rpid=" + rpid + "&action=0";
                    }
                    var response = await hc.PostAsync(ReUri, new HttpStringContent(sendString, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        if (((sender as HyperlinkButton).Content as TextBlock).Text == "赞同")
                        {
                            ((sender as HyperlinkButton).Content as TextBlock).Text = "取消赞";
                        }
                        else
                        {
                            ((sender as HyperlinkButton).Content as TextBlock).Text = "赞同";
                        }
                    }
                    else
                    {
                        if ((int)json["code"] == 12007)
                        {
                            messShow.Show("已经点赞了", 3000);
                        }
                        else
                        {
                            messShow.Show("点赞失败" + result, 3000);
                        }
                    }

                }
                catch (Exception)
                {
                }
            }
            else
            {
                messShow.Show("请先登录!", 3000);
            }
        }

        private async void btn_Load_More_Click(object sender, RoutedEventArgs e)
        {
            if (canLoad)
            {
                canLoad = false;
                ps++;
                await GetComments(aid, rootsid, ps);
                canLoad = true;
            }
        }
        bool canLoad = true;
        private async void sv1_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sv1.VerticalOffset == sv1.ScrollableHeight)
            {
                if (canLoad)
                {
                    canLoad = false;
                    ps++;
                    await GetComments(aid, rootsid, ps);
                    canLoad = true;
                }
            }
        }

        private async void btn_SendComment_Click(object sender, RoutedEventArgs e)
        {
            UserClass getUser = new UserClass();
            if (getUser.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/reply/add");
                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    string QuStr = "plat=6&jsonp=jsonp&message=" + Uri.EscapeDataString(txt_Com.Text) + "&type=1&oid=" + aid;
                    var response = await hc.PostAsync(ReUri, new HttpStringContent(QuStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        menu_Comment.IsChecked = false;
                        menu_Good.IsChecked = false;
                        menu_Time.IsEnabled = false;
                        menu_Comment.IsEnabled = true;
                        menu_Good.IsEnabled = true;
                        pageNum = 1;
                        ListView_Comment_New.Items.Clear();
                        GetVideoComment_New(aid, 0);
                        messShow.Show("已发送评论!",3000);
                    }
                    else
                    {
                        messShow.Show("评论失败\r\n"+ result, 3000); 
                    }

                }
                catch (Exception ex)
                {
                    messShow.Show("评论时发生错误\r\n" + ex.Message, 3000);
                }
            }
            else
            {
                messShow.Show("请先登录" , 3000);
              
            }
        }

        private async void brn_SendComment_1_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Com_1.Text.Length == 0)
            {
                messShow.Show("内容不能为空",3000);
                return;
            }
            UserClass getUser = new UserClass();
            if (getUser.IsLogin())
            {
                try
                {
                    Uri ReUri = new Uri("http://api.bilibili.com/x/reply/add");
                    HttpClient hc = new HttpClient();
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    if (root == "")
                    {
                        root = rootsid;
                    }
                    //jsonp=jsonp&message=(%E2%8C%92%E2%96%BD%E2%8C%92)&parent=95828061&root=95828061&type=1&plat=1&oid=4376012
                    string QuStr = "plat=6&jsonp=jsonp&message=" + Uri.EscapeDataString(txt_Com_1.Text) + "&parent=" + rootsid + "&root=" + root + "&type=1&plat=6&oid=" + aid;
                    var response = await hc.PostAsync(ReUri, new HttpStringContent(QuStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    if ((int)json["code"] == 0)
                    {
                        await GetComments(aid, rootsid);
                        messShow.Show("评论成功！", 3000);
                    }
                    else
                    {
                        messShow.Show("评论失败！\r\n" + result,3000);
                    }

                }
                catch (Exception ex)
                {
                    messShow.Show("评论时发生错误\r\n"+ex.Message, 3000);
                }
            }
            else
            {
                messShow.Show("请先登录!",3000);
            }
        }

        private void btn_UP_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserInfoPage), ((VideoModel)Video_UP.DataContext).mid);

        }

        private void btn_Share_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
            pack.SetText(string.Format("我正在BiliBili看{0}\r\n地址：http://www.bilibili.com/video/av{1}", Video_Title.Text, aid));
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack); // 保存 DataPackage 对象到剪切板
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
            messShow.Show("已将内容复制到剪切板", 3000);
        }

        private async void PostHistory()
        {
            try
            {
                WebClientClass wc = new WebClientClass();
                string url = string.Format("http://api.bilibili.com/x/history/add?_device=wp&_ulv=10000&access_key={0}&appkey={1}&build=411005&platform=android", ApiHelper.access_key, ApiHelper._appKey);
                url += "&sign=" + ApiHelper.GetSign(url);
                string result = await wc.PostResults(new Uri(url), "aid=" + aid);
            }
            catch (Exception)
            {
            }
        }
    }
}
