using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;

namespace AMSAPP
{
    public enum AccessEventType
    {
        Entry = 0,
        Exit = 1,
        Unknown = 2

    }

    public enum TimeSheetStatus
    {
        None = 0,
        Draft = 1,
        Submitted = 2

    }

    

    internal static class AMSUtil
    {
        #region LastEventData

        public static AMSEvent GetLastEventAMS()
        {
            return ParseLastEventFromString(GetLastEventStringFromAMS());
        }

        private static string GetLastEventStringFromAMS()
        {
            WebClient wc = new WebClient();
            wc.UseDefaultCredentials = true;

            string data = wc.DownloadString(new Uri(Properties.Settings.Default.AMSWhereAMI));

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_ctl00_ASPxSplitter1_Content_ContentSplitter_MainContent_mapHimPanel_locationStatusLabel']");

            var mysds = node.InnerText;

            return mysds;
        }

        private static AMSEvent ParseLastEventFromString(string eventData)
        {
            AMSEvent accessEvent = new AMSEvent();
            accessEvent.EventType = (byte)GetEvenTTypeFromString(eventData);
            accessEvent.EventOn = GetEventDateFromString(eventData);
            accessEvent.Description = getEventDescriptionFromString(eventData);
            accessEvent.ElapsedTime = GetElapsedTimeFromAMS();
            return accessEvent;
        }

        private static AccessEventType GetEvenTTypeFromString(string eventData)
        {
            if (eventData.Contains("is currently inside EY premises"))
            {
                return AccessEventType.Entry;
            }
            else if (eventData.Contains("is currently outside EY premises"))
            {
                return AccessEventType.Exit;
            }
            return AccessEventType.Unknown;
        }

        private static string getEventDescriptionFromString(string eventData)
        {
            string retVal = "";
            if (eventData.Length > 24)
            {
                retVal = eventData.Substring(0, 24);
                eventData = eventData.Replace(retVal, "");
            }
            retVal = retVal + eventData.Split('.')[0];
            return retVal;
        }

        private static DateTime GetEventDateFromString(string eventData)
        {
            DateTime lasteventDate = new DateTime();
            try
            {
                int firstcolon = eventData.IndexOf(':');
                var datetimestring = eventData.Substring(firstcolon + 1, eventData.Length - (firstcolon + 1));
                var temp = datetimestring.Trim().Split(' ');
                lasteventDate = DateTime.ParseExact(temp[0] + " " + temp[2] + " " + temp[3],  "M/d/yyyy h:mm tt" , CultureInfo.InvariantCulture);

            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
            } 
            
            return lasteventDate;
        }

        #endregion LastEventData


        public static  List<DayOfWeek> GetTimesheetAlertWeeks()
        {
           var days =  Properties.Settings.Default.TimesheetAlertDays.Split(',');
            var dayEnums = new List<DayOfWeek>();
            foreach(var day in days)
            {
                if (!string.IsNullOrWhiteSpace(day))
                {
                    DayOfWeek dayEnum;
                    Enum.TryParse<DayOfWeek>(day, true, out dayEnum);
                    if (dayEnum != null)
                    {
                        dayEnums.Add((DayOfWeek)dayEnum);
                    }
                }
            }
            return dayEnums;
        }

        public static void RemoveTimesheetAlertWeeks(string day)
        {
            if (!string.IsNullOrWhiteSpace(day))
            {
                var days = new List<string>(Properties.Settings.Default.TimesheetAlertDays.Split(','));
                days.Remove(day);
                Properties.Settings.Default.TimesheetAlertDays = String.Join(",", days);
                Properties.Settings.Default.Save();
            }

        }

        public static BitmapImage GetAvatar()
        {

            if(Properties.Settings.Default.Avatar != null &&  Properties.Settings.Default.AvatarRefreshTime != null)
            {
                if((Properties.Settings.Default.AvatarRefreshTime - DateTime.Now).Days <1)
                {
                    return Properties.Settings.Default.Avatar;
                }
            }
            var path = GetImagePath();

            if (path != null)
            {

                var image = new BitmapImage();
                int BytesToRead = 100;
                WebRequest request = WebRequest.Create(new Uri(path, UriKind.Absolute));
                request.Timeout = -1;
                request.UseDefaultCredentials = true;
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                BinaryReader reader = new BinaryReader(responseStream);
                MemoryStream memoryStream = new MemoryStream();

                byte[] bytebuffer = new byte[BytesToRead];
                int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

                while (bytesRead > 0)
                {
                    memoryStream.Write(bytebuffer, 0, bytesRead);
                    bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
                }

                image.BeginInit();
                memoryStream.Seek(0, SeekOrigin.Begin);

                image.StreamSource = memoryStream;
                image.EndInit();
                Properties.Settings.Default.Avatar = image;
                Properties.Settings.Default.AvatarRefreshTime = DateTime.Now;
                Properties.Settings.Default.Save();
                return image;
            }
            else
                return null;
        }

    private static string GetImagePath()
    {
                try 
	            {	        
		            WebClient wc = new WebClient();
                            wc.UseDefaultCredentials = true;
                            var data = wc.DownloadString(new Uri(Properties.Settings.Default.MySite));
                            HtmlDocument doc1 = new HtmlDocument();
                            doc1.LoadHtml(data);
                            var Node = doc1.DocumentNode.SelectSingleNode("//img[@class='ms-profile-image']");

                            if(Node != null)
                            {
                                return Node.Attributes["src"].Value;
                            }
	            }
	            catch (Exception)
	            {
		
		            return null;
	            }
        return null;
    }

        public static void AddTimesheetAlertWeeks(string day)
        {
            if (!string.IsNullOrWhiteSpace(day))
            {
                var days = new List<string>(Properties.Settings.Default.TimesheetAlertDays.Split(','));
                days.Remove(day);
                days.Add(day);
                Properties.Settings.Default.TimesheetAlertDays = String.Join(",", days);
                Properties.Settings.Default.Save();
            }

        }

        public static TimeSheetStatus CheckTimesheet()
        {
            
            
            WebClient wc = new WebClient();
            wc.UseDefaultCredentials = true;
            var data = wc.DownloadString(new Uri(Properties.Settings.Default.TimesheetUrl));
            HtmlDocument doc1 = new HtmlDocument();
            doc1.LoadHtml(data);
            Logger.Log("Downloded data from " + Properties.Settings.Default.TimesheetUrl);


            var Node = doc1.DocumentNode.SelectSingleNode("//span[@id='ctl00_contentPH_ucTimesheetHeader_lblPHrsTotal']");
            if(Node != null)
            {
                string hours = Node.InnerText;
            }

            var statusNode = doc1.DocumentNode.SelectSingleNode("//span[@id='ctl00_contentPH_ucTimesheetHeader_lblStatus']");

            if (statusNode != null)
            {
                
                if (string.IsNullOrWhiteSpace(statusNode.InnerText))
                {
                    return TimeSheetStatus.None;
                } else if (statusNode.InnerText.Trim().ToLower().Contains("Draft".ToLower()))
                {
                    return TimeSheetStatus.Draft;
                }
                else if (statusNode.InnerText.Trim().ToLower().Contains("Submitted".ToLower()))
                {
                    return TimeSheetStatus.Submitted;
                }
               
            }
            else
            {
                return TimeSheetStatus.None;
                Logger.Log("statusNode is null");
            }            

            return TimeSheetStatus.None;
        }

        private static TimeSpan? GetElapsedTimeFromAMS()
        {
            TimeSpan? res = null;

            try
            {
                WebClient wc = new WebClient();
                wc.UseDefaultCredentials = true;
                var data = wc.DownloadString(new Uri(Properties.Settings.Default.AMSCalendar));
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                //
                var node = doc.DocumentNode.SelectSingleNode("//td[@class='dxeCalendarDay_DevEx dxeCalendarToday_DevEx']");
                if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                {
                    node = doc.DocumentNode.SelectSingleNode("//td[@class='dxeCalendarDay_DevEx dxeCalendarWeekend_DevEx dxeCalendarToday_DevEx']");
                }

                res = TimeSpan.Parse(node.Attributes["title"].Value);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
            }

            return res;
        }


        public static string LoadName(HtmlDocument html)
        {
            var headerNode = html.DocumentNode.SelectSingleNode("//td[@class='dxrpHeader_DevEx']");
            if (headerNode != null)
            {
                var nameNode = headerNode.SelectSingleNode(".//span");
                if (nameNode != null)
                {
                    if (!string.IsNullOrWhiteSpace(nameNode.InnerText))
                    {
                        return nameNode.InnerText.Replace("Attendance :", "");
                    }
                }
            }
            return null;
        }

        public static List<string> GetMonthNames(HtmlDocument html)
        {
             
            try
            {
                var monthList = new List<string>();
                var monthNodes = html.DocumentNode.SelectNodes("//td[@class='dxeCalendarHeader_DevEx']");

                foreach (var monthNode in monthNodes)
                {
                    var month = monthNode.SelectSingleNode(".//span").InnerText;
                    monthList.Add(month);
                }
                Logger.Log("Loaded Months " + monthList.Count);
                return monthList;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
        }
        
    }
}