using AMSAPP.helpers;
using HtmlAgilityPack;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AMSAPP
{
    public partial class RMAlertWindow : Window
    {


        public RMAlertWindow()
        {
            //WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            LoadFields();
            InitializeScreenRefreshTimer();
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            //this.Width = desktopWorkingArea.Width;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom;
            doubleAni.From = desktopWorkingArea.Bottom;
            doubleAni.To = desktopWorkingArea.Bottom - this.Height;
        }    

        private void LoadFields()
        {
            TimesheetData.Text = ((App)Application.Current).timesheetAlert;
           


            try
            {

                    var image = AMSUtil.GetAvatar();
                    if(image != null)
                        avatar.Source = image;


                
            }catch(Exception ex)
            {

            }
        }

      

      

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        private void InitializeScreenRefreshTimer()
        {
            System.Windows.Threading.DispatcherTimer ScreenRefreshTimer = new System.Windows.Threading.DispatcherTimer();
            ScreenRefreshTimer.Tick += new EventHandler(ScreenRefreshTimer_Tick);
            ScreenRefreshTimer.Interval = new TimeSpan(0, 0, 60);
            ScreenRefreshTimer.Start();
        }
        private void ScreenRefreshTimer_Tick(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnNoNeed_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RouteMentorNoNeed = DateTime.Now.ToString("yyyy-MM-dd");
            Properties.Settings.Default.Save();
            btnCreate.Visibility = Visibility.Hidden;
            btnNoNeed.Visibility = Visibility.Hidden;
            
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RouteMentorHelper.AddRequestChrome();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                
            }
        }
    }
}