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
        }
    }
}
