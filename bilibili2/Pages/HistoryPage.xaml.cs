using bilibili2.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace bilibili2.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        public delegate void GoBackHandler();
        public event GoBackHandler BackEvent;
        public HistoryPage()
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
        private int pageNum_His = 1;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bg.Color = ((SolidColorBrush)this.Frame.Tag).Color;
            if (e.NavigationMode== NavigationMode.New)
            {
                pageNum_His = 1;
                User_ListView_History.Items.Clear();
                 GetHistoryInfo();
            }
           
        }

        private void User_ListView_History_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoInfoPage), ((GetHistoryModel)e.ClickedItem).aid);
        }
        bool More = true;
        private  void sv_Ho_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if ((sender as ScrollViewer).VerticalOffset == (sender as ScrollViewer).ScrollableHeight)
            {
                if (More)
                {
                    GetHistoryInfo();
                }
            }

        }

        private async void GetHistoryInfo()
        {
            try
            {
                More = false;
                pro_Load.Visibility = Visibility.Visible;
                UserClass getLogin = new UserClass();
                User_load_more_history.Content = "正在加载";
                List<GetHistoryModel> lsModel = await getLogin.GetHistory(pageNum_His);
                if (lsModel != null)
                {
                    foreach (GetHistoryModel item in lsModel)
                    {
                        User_ListView_History.Items.Add(item);
                    }
                }
                else
                {
                    User_load_more_history.Visibility = Visibility.Collapsed;
                }
                User_load_more_history.Content = "";
                pageNum_His++;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
            finally
            {
                pro_Load.Visibility = Visibility.Collapsed;
                More = true;
            }
          
        }

    }
}
