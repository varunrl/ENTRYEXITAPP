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
    public partial class TimesheetAlertWindow : Window
    {


        public TimesheetAlertWindow()
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
            ScreenRefreshTimer.Interval = new TimeSpan(0, 0, 10);
            ScreenRefreshTimer.Start();
        }
        private void ScreenRefreshTimer_Tick(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}