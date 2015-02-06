using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using HtmlAgilityPack;
using MahApps.Metro.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AMSAPP.models;
using Xceed.Wpf.Toolkit;
using System.Runtime.InteropServices;
using System.IO;

namespace AMSAPP
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class Summary : MetroWindow
    {
        private HtmlDocument doc = new HtmlDocument();
        private ItemsChangeObservableCollection<SummaryGridRow> obsSummaryGrid;
  

        public Summary()
        {
            try
            {
                InitializeComponent();
               
                Logger.Log("InitializeComponent done");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                MySlider.Value = Properties.Settings.Default.MinTimeSpan;
                LoadMonthsFromWeb();
                LoadName();

                //MySlider.IsEnabled = false;
                LoadToday();
                LoadSettings();
                CheckTimesheet();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void LoadToday()
        {
            try
            {
                DataContext tetst = new DataContext();
                
                dataGrid1.ItemsSource = tetst.ComputerEvents.ToList().Where(x => x.EventOn.Date == DateTime.Now.Date).ToList();
                dataGrid2.ItemsSource = tetst.AMSEvents.ToList().Where(x => x.EventOn.Date == DateTime.Now.Date).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }


        private void LoadName()
        {
            try
            {
                string name = AMSUtil.LoadName(doc);
                if (!String.IsNullOrWhiteSpace(name))
                {
                    this.Title = name;
                }
               
                    
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void CheckTimesheet()
        {

            var result =  AMSUtil.CheckTimesheet();

            if (result == TimeSheetStatus.None)
            {
                lblTimesheetStatus.Content = "Time Sheet Not Submitted";
            }else if (result == TimeSheetStatus.Draft)
            {
                lblTimesheetStatus.Content = "Time Sheet in draft. Please submit";
            }
            else if (result == TimeSheetStatus.Submitted)
            {
                lblTimesheetStatus.Content = "Good Job. Time Sheet Submitted";
            }

            //MessageBox.Show(this, lblTimesheetStatus.Content.ToString(),"TimeSheet Entry");
        }
      

        private void LoadMonthsFromWeb()
        {
            try
            {
                WebClient wc = new WebClient();
                wc.UseDefaultCredentials = true;
                var data = wc.DownloadString(new Uri(Properties.Settings.Default.AMSCalendar));
                Logger.Log("Data Downloaded from " + Properties.Settings.Default.AMSCalendar);              
                doc.LoadHtml(data);
                var monthList = AMSUtil.GetMonthNames(doc);
                cbMonths.ItemsSource = monthList;
                cbMonths.SelectedIndex = monthList.Count - 1;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

       

        private void cbMonths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            try
            {
                var comboBox = sender as ComboBox;

                string month = comboBox.SelectedItem as string;
                Logger.Log("Selected Month " + month);
                LoadMonthGrid(month);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void LoadMonthGrid(string month)
        {
            var monthNode = GetMonthNode(month);

            var year = Convert.ToInt32(month.Split(',')[1]);
            Logger.Log("Year = " + year);
            var iMonth = DateTime.ParseExact(month.Split(',')[0], "MMMM", CultureInfo.InvariantCulture).Month;
            Logger.Log("MonthNumber = " + iMonth);

            if (monthNode != null)
            {
                var dayLogs = ExtractDayLogs(monthNode, year, iMonth);
                var displayList = dayLogs.Select(x => GetGridValue(x)).ToList();
                Logger.Log("displayList count" + displayList.Count());

                obsSummaryGrid = new ItemsChangeObservableCollection<SummaryGridRow>(displayList);
                monthDataGrid.ItemsSource = obsSummaryGrid;
                ShowAverage(dayLogs);

                Logger.Log("completed month selection change");
            }
        }

        private static List<DayLog> ExtractDayLogs(HtmlNode monthNode, int year, int iMonth)
        {
            DataContext dts = new DataContext();
            var monthGrid = monthNode.ParentNode.NextSibling.SelectSingleNode(".//td[@class='dxMonthGrid']");
            if (monthGrid == null)
            {
                Logger.Log("monthGrid is null");
            }
            else
            {
                Logger.Log("monthGrid is not null");
            }
            var tds = monthGrid.SelectNodes(string.Format(".//*[contains(@class,'{0}')]", "dxeCalendarDay_DevEx"));

            Logger.Log("Days found in monthGrid : " + tds.Count());
            var dayLogs = new List<DayLog>();
            foreach (var td in tds)
            {
                int day;
                if (td.Attributes["Class"] != null && td.Attributes["Class"].Value.Contains("dxeCalendarOtherMonth_DevEx"))
                {
                    Logger.Log("skipping other month date : " + td.InnerText);
                    continue;
                }

                if (Int32.TryParse(td.InnerText, out day))
                {

                    var info = dts.GetDayLogForDay(new DateTime(year, iMonth, day));
                    if (td.Attributes["Class"] != null && td.Attributes["Class"].Value.Contains("dxeCalendarToday_DevEx"))
                    {
                        dayLogs.Add(new DayLog
                        {
                            Date = new DateTime(year, iMonth, day),
                            Title = "",
                            Class = td.Attributes["Class"] != null ? td.Attributes["Class"].Value : "",
                            Color = td.Attributes["bgcolor"] != null ? td.Attributes["bgcolor"].Value : "white",
                           info  = info
                        }

                                );
                        Logger.Log("Added Today");
                    }
                    else
                    {
                        dayLogs.Add(new DayLog
                        {
                            Date = new DateTime(year, iMonth, day),
                            Title = td.Attributes["Title"] != null ? td.Attributes["Title"].Value : "",
                            Class = td.Attributes["Class"] != null ? td.Attributes["Class"].Value : "",
                            Color = td.Attributes["bgcolor"] != null ? td.Attributes["bgcolor"].Value : "white",
                            info  = info
                        }


                                );
                        Logger.Log("Added DayLog " + day);
                    }
                }
                else
                {
                    Logger.Log("Can not parse to integer : " + td.InnerText);
                }
            }
            Logger.Log("dayLogs count" + dayLogs.Count());
            return dayLogs;
        }

        private SummaryGridRow GetGridValue(DayLog dayLog)
        {
            TimeSpan time = new TimeSpan();
            string duration = "";
            string amsComment = "";
            if (dayLog.Title.Length >= 8 && TimeSpan.TryParse(dayLog.Title.Substring(0, 8), out time))
            {
                duration = time.ToString();
                if (dayLog.Title.Length > 8)
                {
                    amsComment = dayLog.Title.Substring(8, dayLog.Title.Length - 8);
                } 
                if (amsComment.Trim().Length == 0)
                {
                    if (dayLog.Class.Contains("dxeCalendarWeekend_DevEx"))
                    {
                        amsComment = "Weekend";
                    }
                }
            }
            else
            {
                amsComment = dayLog.Title;
               
            }
            amsComment = amsComment.Replace(":", "").Trim();
            var comment = "";
            if(dayLog.info != null )
            {
                if(!string.IsNullOrWhiteSpace(dayLog.info.Comments))
                {
                    comment = dayLog.info.Comments;
                }
                if (dayLog.info.BufferTimespan > TimeSpan.Zero)
                {
                    if(!string.IsNullOrEmpty(comment))
                    {
                        comment += Environment.NewLine;
                    }
                    comment +=  "Added buffer " + dayLog.info.BufferTimespan.ToString(@"hh\:mm"); 
                    duration = (time + dayLog.info.BufferTimespan).ToString();
                }
            }
            
            //var color = GetValueFromStyle(dayLog.Style, "BACKGROUND-COLOR");
            return new SummaryGridRow { Date = dayLog.Date, WeekDay = dayLog.WeekDay, Day = dayLog.Day, Duration = duration, Comments = comment, Color = dayLog.Color, AMScomments = amsComment };
        }

       

        private void ShowAverage(List<DayLog> dayLogs)
        {
            try { 
                TimeSpan sum = new TimeSpan();
                double count = 0;
                int daysRemaining = 0;
                foreach (var dayLog in dayLogs)
                {

                    if (dayLog.info != null)
                    {
                        sum += dayLog.info.BufferTimespan;

                    }

                    TimeSpan time = new TimeSpan();
                    if (dayLog.Title.Trim().Length >= 8)
                    {
                        if (TimeSpan.TryParse(dayLog.Title.Substring(0, 8), out time))
                        {
                            sum += time;

                        }
                    }

                    if (dayLog.info != null && dayLog.info.Leave != "None")
                    {
                        count += dayLog.info.workingCount;

                    }
                    else if (!(dayLog.Class.Contains("dxeCalendarWeekend_DevEx")) && dayLog.Day < DateTime.Now.Day && !dayLog.Class.Contains("dxeCalendarOtherMonth_DevEx"))
                    {
                       
                        if (dayLog.Title.Contains("Leave Half Day"))
                        {
                            count += 0.5;
                        }
                        else if (dayLog.Title.Contains("Public Holiday"))
                        {

                        }
                        else if (dayLog.Title.Contains("Leave"))
                        {

                        }
                        else if (dayLog.Title.Contains("Offday"))
                        {

                        }
                        else
                        {
                            count++;
                        }
                    }else
                    {
                        if (!dayLog.Class.Contains("dxeCalendarWeekend_DevEx") )
                        {
                            daysRemaining++;
                        }
                    }
                }
                int average = 0;
                if (count > 0)
                {
                    average = (int)(sum.TotalMinutes / count);
                }
                TimeSpan averageTimespan = new TimeSpan(0, 0, average, 0, 0);
                lblAverage.Content = FormatTimeSpan(averageTimespan);
                lblTotalDays.Content = count.ToString();
                lblRemainingDays.Content = daysRemaining.ToString();
                if (daysRemaining > 0)
                {
                    MySlider.IsEnabled = true;
                    CalculateRequired((int)MySlider.Value);
                    lblrequired.Visibility = Visibility.Visible;
                }
                else
                {
                    MySlider.IsEnabled = false;
                    lblrequired.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
        }

        private HtmlNode GetMonthNode(string month)
        {
            var monthNodes = doc.DocumentNode.SelectNodes("//td[@class='dxeCalendarHeader_DevEx']");

            foreach (var monthNode in monthNodes)
            {
                if (monthNode.SelectSingleNode(".//span").InnerText == month)
                {
                    Logger.Log("Month Node found for month " + month,LogType.Info);
                    return monthNode;
                }
            }
            Logger.Log("Month Node not found for month " + month,LogType.Critical);
            return null;
        }

       

        private void MySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = sender as Slider;

                int selectedValue = (int)slider.Value;
                CalculateRequired(selectedValue);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void CalculateRequired(int selectedValue)
        {
            try
            {
                runExpected.Text = FormatTimeSpan(TimeSpan.FromMinutes(selectedValue));
                if (lblTotalDays.Content.ToString() != "0" && lblRemainingDays.Content.ToString() != "0")
                {
                    double totalDays = (Convert.ToDouble(lblTotalDays.Content) + Convert.ToDouble(lblRemainingDays.Content));
                    TimeSpan currentAverage = TimeSpan.Parse(lblAverage.Content.ToString());

                    int requiredMinutes = (int)((Convert.ToDouble((selectedValue * totalDays)) - (Convert.ToDouble(currentAverage.TotalMinutes) * Convert.ToDouble(lblTotalDays.Content))) / Convert.ToDouble(lblRemainingDays.Content));

                    lblrequired.Content = FormatTimeSpan(TimeSpan.FromMinutes(requiredMinutes));
                }else
                {
                    lblrequired.Content = FormatTimeSpan(TimeSpan.FromMinutes(selectedValue));
                }
            }
           
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
        private string FormatTimeSpan(TimeSpan time)
        {
            return ((time < TimeSpan.Zero) ? "-" : "") + time.ToString(@"hh\:mm");
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).SummaryWindowOpen = false;
        }

        private string GetValueFromStyle(string style, string key)
        {
            if (!string.IsNullOrWhiteSpace(style))
            {
                var stylePairs = style.Split(';');
                foreach (var stylePair in stylePairs)
                {
                    if (stylePair.Contains(key))
                    {
                        var nameValues = stylePair.Split(':');
                        if (nameValues.Count() > 1)
                        {
                            return nameValues[1];
                        }
                    }
                }
            }
            return null;
        }

        private void monthDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            try
            {
                DataGridTextColumn textColumn = e.Column as DataGridTextColumn;
                if (textColumn != null)
                {
                    Style style = new Style(textColumn.ElementStyle.TargetType, textColumn.ElementStyle.BasedOn);
                    var bbSetter = new Setter(
                        DataGrid.HorizontalAlignmentProperty,
                        HorizontalAlignment.Center);
                    style.Setters.Add(bbSetter);
                    textColumn.ElementStyle = style;
                }
                if (e.Column.Header.ToString() == "Date")
                {
                    e.Cancel = true;
                }
                if (e.Column.Header.ToString() == "Color")
                {
                    e.Cancel = true;
                }
                else if (e.Column.Header.ToString() == "Day")
                {
                    e.Column.Width = 50;
                }
                else if (e.Column.Header.ToString() == "WeekDay")
                {
                    e.Column.Width = 100;
                }
                else if (e.Column.Header.ToString() == "Duration")
                {
                    e.Column.Width = 100;
                }
                else if (e.Column.Header.ToString() == "Comment")
                {
                    e.Column.Width = 300;
                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex);
            }
           
        }

        private void monthDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
           
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => AlterRow(e)));
            
                      
        }

        private void AlterRow(DataGridRowEventArgs e)
        {
            try
            {
                SummaryGridRow RowDataContaxt = new SummaryGridRow();
                RowDataContaxt = Cast(RowDataContaxt, e.Row.DataContext);
                if (RowDataContaxt != null)
                {
                    var cell = GetCell(monthDataGrid, e.Row, 2);
                    SolidColorBrush myBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(RowDataContaxt.Color);
                    cell.Background = myBrush;

                    cell = GetCell(monthDataGrid, e.Row, 0);
                    var textBlock = cell.Content as TextBlock;
                    if (textBlock.Text == "Saturday" || textBlock.Text == "Sunday")
                    {
                        textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("Maroon");
                        //cell.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("Red");
                    }

                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex);
            }
        }

        private static T Cast<T>(T typeHolder, Object x)
        {
            // typeHolder above is just for compiler magic
            // to infer the type to cast x to
            return (T)x;
        }

        public static DataGridRow GetRow(DataGrid grid, int index)
        {
            var row = grid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;

            if (row == null)
            {
                // may be virtualized, bring into view and try again
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? GetVisualChild<T>(v);
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static DataGridCell GetCell(DataGrid host, DataGridRow row, int columnIndex)
        {
            if (row == null) return null;

            var presenter = GetVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null) return null;

            // try to get the cell but it may possibly be virtualized
            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            if (cell == null)
            {
                // now try to bring into view and retreive the cell
                host.ScrollIntoView(row, host.Columns[columnIndex]);
                cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            }
            return cell;

        }

        private void LoadSettings()
        {
            lblRefreshInterval.Content = Properties.Settings.Default.RefreshInterval;
            sliderRefreshInterval.Value = Properties.Settings.Default.RefreshInterval;
            lblMinHours.Content = TimeSpan.FromMinutes(Properties.Settings.Default.MinTimeSpan).ToString(); 
            sliderMinHours.Value = Properties.Settings.Default.MinTimeSpan;


            SetCheckbox();
            

        }

        private void SetCheckbox()
        {
            var alertDays = AMSUtil.GetTimesheetAlertWeeks();
            if(alertDays.Contains(DayOfWeek.Friday))
            {
                chkFriday.IsChecked = true;
            }
            if (alertDays.Contains(DayOfWeek.Thursday))
            {
                chkThursday.IsChecked = true;
            }
            if (alertDays.Contains(DayOfWeek.Wednesday))
            {
                chkWednesday.IsChecked = true;
            }
            if (alertDays.Contains(DayOfWeek.Tuesday))
            {
                chkTuesday.IsChecked = true;
            }
            if (alertDays.Contains(DayOfWeek.Monday))
            {
                chkMonday.IsChecked = true;
            }
        }

        private void sliderRefreshInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           
            var slider = sender as Slider;
            if (slider.Value == slider.Minimum)
            {
                return;
            }
            lblRefreshInterval.Content = (int)slider.Value;
            Properties.Settings.Default.RefreshInterval = (int)slider.Value;
            Properties.Settings.Default.Save();
        }

        private void sliderMinHours_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider.Value == slider.Minimum)
            {
                return;
            }
            lblMinHours.Content = TimeSpan.FromMinutes((int)slider.Value).ToString(); 
            Properties.Settings.Default.MinTimeSpan = (int)slider.Value;
            MySlider.Value = Properties.Settings.Default.MinTimeSpan;
            Properties.Settings.Default.Save();

        }


        private async void monthDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (monthDataGrid.SelectedItem == null) return;
            var selectedPerson = monthDataGrid.SelectedItem as SummaryGridRow;

            DataContext test = new DataContext();
            var info = test.GetDayLogForDay(selectedPerson.Date);

            DayDetails details = new DayDetails(info);
            details.ShowDialog();
            if(details.ok == true)
            {
                if(!string.IsNullOrWhiteSpace(details.Comments))
                {
                    obsSummaryGrid.Single(x => x.Day == selectedPerson.Day).Comments = details.Comments;
                }
                DataContext tetst = new DataContext();
                tetst.AddDayLog(new DayLogInfo
                {
                    Date = selectedPerson.Date,
                    Leave = details.LeaveStatus,
                    Comments = details.Comments,
                    Buffer = details.Buffer.ToString(@"hh\:mm")
                });

                LoadMonthGrid(cbMonths.SelectedItem.ToString());
            }
            //await this.ShowMessageAsync("This is the title", "Some message");

            
            //await this.ShowMetroDialogAsync(dialog);

            //var result = await this.ShowInputAsync("Hello!", "Comments : ");

            //if (result != null)
            //{

            //    obsSummaryGrid.Single(x => x.Day == selectedPerson.Day).Comments = result;
                
            //}
                

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            AMSUtil.AddTimesheetAlertWeeks(chk.Content.ToString());
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            AMSUtil.RemoveTimesheetAlertWeeks(chk.Content.ToString());
        }

        private async void export_Click(object sender, RoutedEventArgs e)
        {
            await ExportToExcel();
        }

        private async Task ExportToExcel()
        {
            try
            {
                monthDataGrid.SelectAllCells();
                monthDataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, monthDataGrid);
                String resultat = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                Clipboard.Clear();
                //String result = (string)Clipboard.GetData(DataFormats.Text);
                monthDataGrid.UnselectAllCells();
                string fileName = @"C:\Temp\AMS" + Guid.NewGuid().ToString() + ".csv";
                Directory.CreateDirectory(Path.GetDirectoryName(@"C:\Temp\AMS.csv"));
                System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
                file.WriteLine(resultat);
                file.Close();
                //await this.ShowMessageAsync("Export to excel ", "check file " + fileName);
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook wb = excel.Workbooks.Open(fileName);
                excel.Visible = true;
            }
            catch (Exception)
            {
                
                
            }     
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
            ((App)Application.Current).SummaryWindowOpen = false;
        }

     
     

    }
}