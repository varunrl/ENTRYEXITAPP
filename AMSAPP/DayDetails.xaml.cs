using AMSAPP.models;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace AMSAPP
{
    /// <summary>
    /// Interaction logic for DayDetails.xaml
    /// </summary>
    public partial class DayDetails : Window
    {
        public string LeaveStatus { get; set; }
        public string Comments { get; set; }
        public TimeSpan Buffer { get; set; }

        public bool ok { get; set; }

        public DayDetails()
        {
            InitializeComponent();
            this.Owner = ((App)Application.Current).Summary;
        }


        public DayDetails(DayLogInfo info) : this()
        {
            if (info != null)
            {
                if(info.BufferTimespan > TimeSpan.Zero)
                {
                    this.txtCBuffer.Text = info.Buffer;
                }
                this.LeaveStatus = info.Leave;
                this.Comments = info.Comments;

                if (LeaveStatus == "Full Day")
                {
                    rbfull.IsChecked = true;
                    rbHalf.IsChecked = false;
                    rbNone.IsChecked = false;
                    rbWorking.IsChecked = false;
                }
                else if (LeaveStatus == "Half Day")
                {
                    rbfull.IsChecked = false;
                    rbHalf.IsChecked = true;
                    rbNone.IsChecked = false;
                    rbWorking.IsChecked = false;
                }
                else if (LeaveStatus == "Working")
                {
                    rbfull.IsChecked = false;
                    rbHalf.IsChecked = true;
                    rbNone.IsChecked = false;
                    rbWorking.IsChecked = true;
                }
                else if (LeaveStatus == "None")
                {
                    rbfull.IsChecked = false;
                    rbHalf.IsChecked = false;
                    rbNone.IsChecked = true;
                    rbWorking.IsChecked = false;
                }
                this.txtComents.Text = this.Comments;
            }
            
        }
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = this.Owner.Top + ((this.Owner.Height / 2) - (this.Height/2));
            this.Left = this.Owner.Left + ((this.Owner.Width / 2) - (this.Width / 2));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Comments = this.txtComents.Text;
            if (this.txtCBuffer.Value != null)
            {

                try
                {
                    var result = TimeSpan.ParseExact(this.txtCBuffer.Value.ToString(), @"hh\:mm", null);

                    if (result != null)
                    {
                        this.Buffer = result;
                    }
                }
                catch (Exception)
                {

                    System.Windows.MessageBox.Show("Bufer not in correct format","Error!");
                    return;
                }
            }
            this.Hide();
        }

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.ok = true;
            RadioButton ck = sender as RadioButton;
            if (ck.IsChecked.Value)
                LeaveStatus = ck.Content.ToString(); 
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.ok = false;
            this.Hide();
        }
    }
}
