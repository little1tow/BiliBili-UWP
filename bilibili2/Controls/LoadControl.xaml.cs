using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace bilibili2.Controls
{
    public sealed partial class LoadControl : UserControl
    {
        public LoadControl()
        {
            this.InitializeComponent();
            timer.Interval = new TimeSpan(0,0,0,0,100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            img.Source = new BitmapImage(new Uri("ms-appx:///Assets/other/bili_anim_tv_chan_" + new Random().Next(1, 6) + ".png"));
        }

        public DispatcherTimer timer = new DispatcherTimer();

    }
}
