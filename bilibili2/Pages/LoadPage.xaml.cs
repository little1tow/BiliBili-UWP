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

            switch (new Random().Next(1,16))
            {
                case 1:
                    txt_Load.Text = "子曰:要善用鼠标滚轮";
                    break;
                case 2:
                    txt_Load.Text = "(。・`ω´・) 卖个萌";
                    break;
                case 3:
                    txt_Load.Text = "登录后能进入会员的世界哦";
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
                case 9:
                    txt_Load.Text = "只有帅哥才能看到这句话";
                    break;
                case 10:
                    txt_Load.Text = "C#是世界上最好的语言！";
                    break;
                case 11:
                    txt_Load.Text = "看到不爽的弹幕就要屏蔽掉";
                    break;
                case 12:
                    txt_Load.Text = "书上说，看到白学家就要打死";
                    break;
                case 13:
                    txt_Load.Text = "有妹子用这软件吗?";
                    break;
                case 14:
                    txt_Load.Text = "看到这句话的自动为长者+1S";
                    break;
                case 15:
                    txt_Load.Text = "年轻人不要整天习习蛤蛤的(好像打错字了";
                    break;
                default:
                    break;
            }
            try
            {
                await RegisterBackgroundTask();
                if (!CheckNetworkHelper.CheckNetwork())
                {
                    new MessageDialog("请检查网络连接！").ShowAsync();
                }
            }
            catch (Exception)
            {
            }
           

        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

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
