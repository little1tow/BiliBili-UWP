using bilibili2.Class;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
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

namespace bilibili2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoadPage : Page
    {
        public LoadPage()
        {
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            switch (new Random().Next(1,9))
            {
                case 1:
                    txt_Load.Text = "子曰:要善用鼠标滚轮";
                    break;
                case 2:
                    txt_Load.Text = "(。・`ω´・) 卖个萌";
                    break;
                case 3:
                    txt_Load.Text = "2.0真的比1.0好用！";
                    break;
                case 4:
                    txt_Load.Text = "你造吗，每过60秒，1分钟就过去了";
                    break;
                case 5:
                    txt_Load.Text = "→_→ 橙子是个帅哥";
                    break;
                case 6:
                    txt_Load.Text = "BUG什么最讨厌了 o(￣ヘ￣*o)";
                    break;
                case 7:
                    txt_Load.Text = "23333333";
                    break;
                case 8:
                    txt_Load.Text = "哔哩哔哩动画 UWP";
                    break;
                default:
                    break;
            }
            await RegisterBackgroundTask();

        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //SqlHelper sql = new SqlHelper();
            //sql.CreateShieldingTable();
            //sql.InsertShieldingValue("ces",0);
            //sql.InsertShieldingValue("ces2", 0);
            //sql.GetShieldingText();
            //await new MessageDialog(sql.QueryValue()).ShowAsync();
            await Task.Delay(3000);
            this.Frame.Navigate(typeof(MainPage));
        }


        private async Task RegisterBackgroundTask()
        {
            var task = await RegisterBackgroundTask(
                typeof(bilibili2.BackTask.BackgroundTask),
                "BackgroundTask",
                new TimeTrigger(15, false),
                null);

            task.Progress += TaskOnProgress;
            task.Completed += TaskOnCompleted;
        }

        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(Type taskEntryPoint,
                                                                        string taskName,
                                                                        IBackgroundTrigger trigger,
                                                                        IBackgroundCondition condition)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.Denied)
            {
                return null;
            }

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint.FullName
            };

            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();

            Debug.WriteLine($"Task {taskName} registered successfully.");

            return task;
        }

        private void TaskOnProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            Debug.WriteLine($"Background {sender.Name} TaskOnProgress.");
        }

        private void TaskOnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine($"Background {sender.Name} TaskOnCompleted.");
        }


    }
}
