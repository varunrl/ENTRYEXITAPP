using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace AMSAPP.helpers
{
    public static class RouteMentorHelper
    {

        public static MyCabData checkRM()
        {

            
            var data = GetRMDataChrome();
            var currentDate = data.SingleOrDefault(x => x.Date == DateTime.Now.Date);

            return currentDate;
        }
        public static void AddRequestChrome()
        {
            var userName = Properties.Settings.Default.RouteMentorUserName;
            var password = Properties.Settings.Default.RouteMentorPassword;
            var logOut = Properties.Settings.Default.RouteMentorLogoutTime;
            IWebDriver driver;
            ChromeOptions cOptions = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            cOptions.AddArgument("test-type");
            cOptions.AddArgument("start-maximized");
            cOptions.AddArgument("--js-flags=--expose-gc");
            cOptions.AddArgument("--enable-precise-memory-info");
            cOptions.AddArgument("--disable-popup-blocking");
            cOptions.AddArgument("--disable-default-apps");
            cOptions.AddArgument("headless");
            driver = new ChromeDriver(service,cOptions);
            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password) || logOut == DateTime.MinValue)
            {
                driver.Quit();
                throw new Exception("RouteMentorNotConfigured");
            }

            try
            {
                

                driver.Url = Properties.Settings.Default.RouteMentorAddUrl;
                driver.FindElement(By.Id("user_email")).SendKeys(userName);
                driver.FindElement(By.Id("user_pwd")).SendKeys(password);
                driver.FindElement(By.Name("Login_submit")).Click();
                Thread.Sleep(3000);
                var logoutCtrl = driver.FindElement(By.Name("outtime"));
                var selectElement = new SelectElement(logoutCtrl);
                var logoutTime = logOut.ToString("HH:mm");
                selectElement.SelectByText(logoutTime);
                //remove adddays
                var dateString = DateTime.Now.ToString("yyyy-MM-dd");
                var dateDiv = driver.FindElement(By.Id(dateString));
                dateDiv.Click();
                Thread.Sleep(2000);
                driver.FindElement(By.Id("btn_save")).Click();
                Thread.Sleep(3000);

            }
            catch (Exception)
            {

                throw;
            }finally
            {
                driver.Quit();
            }


        }

        private static List<MyCabData> GetRMDataChrome()
        {
            var userName = Properties.Settings.Default.RouteMentorUserName;
            var password = Properties.Settings.Default.RouteMentorPassword;
            var noneed = Properties.Settings.Default.RouteMentorNoNeed;
            IWebDriver driver;
            
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeOptions cOptions = new ChromeOptions();
            cOptions.AddArgument("test-type");
            cOptions.AddArgument("start-maximized");
            cOptions.AddArgument("--js-flags=--expose-gc");
            cOptions.AddArgument("--enable-precise-memory-info");
            cOptions.AddArgument("--disable-popup-blocking");
            cOptions.AddArgument("--disable-default-apps");
            cOptions.AddArgument("headless");
            driver = new ChromeDriver(service,cOptions);

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password) || noneed == DateTime.Now.ToString("yyyy-MM-dd"))
            {
                driver.Quit();
                throw new Exception("RouteMentorNotConfigured");
            }

            try
            {
               
                

                driver.Url = Properties.Settings.Default.RouteMentorListUrl;
                driver.FindElement(By.Id("user_email")).SendKeys(userName);
                driver.FindElement(By.Id("user_pwd")).SendKeys(password);
                driver.FindElement(By.Name("Login_submit")).Click();
                Thread.Sleep(3000);

                var success = false;
                try
                {
                    IWebElement elemAlert = driver.FindElement(By.XPath("//div[@class='alert alert-error ']"));

                }
                catch (Exception ex)
                {

                    success = true;
                }
                if (success)
                {
                    IWebElement elemTable = driver.FindElement(By.XPath("//div[@id='content-table']//table[1]"));
                    List<IWebElement> lstTrElem = new List<IWebElement>(elemTable.FindElements(By.TagName("tr")));



                    var data = new List<MyCabData>();

                    // Traverse each row
                    foreach (var elemTr in lstTrElem)
                    {
                        // Fetch the columns from a particuler row
                        List<IWebElement> lstTdElem = new List<IWebElement>(elemTr.FindElements(By.TagName("td")));
                        if (lstTdElem.Count > 3)
                        {
                            MyCabData item = new MyCabData();

                            item.SLNo = lstTdElem[0].Text;
                            item.strDate = lstTdElem[1].Text;
                            item.Login = lstTdElem[2].Text;
                            item.Logout = lstTdElem[3].Text;
                            data.Add(item);
                        }
                    }
                   

                    return data;
                }
                else
                {
                    AMSAPP.Properties.Settings.Default.RouteMentorNoNeed = DateTime.Now.ToString("yyyy-MM-dd");
                    AMSAPP.Properties.Settings.Default.Save();
                    throw new Exception("Login Failed");
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                driver.Quit();
            }

            
        }
    }

    public class MyCabData
    {
        public string SLNo { get; set; }
        public string strDate { get; set; }
        public DateTime? Date
        {
            get
            {

                if (!string.IsNullOrWhiteSpace(this.strDate))
                {
                    DateTime temp;
                    var success = DateTime.TryParseExact(this.strDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out temp);

                    if (success)
                    {
                        return temp;
                    }
                }
                return null;

            }
        }
        public string Login { get; set; }

        public string Logout { get; set; }

        public string LogoutTime
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(Logout) && Logout.Trim().Length > 5)
                {
                    return Logout.Trim().Substring(0, 5);
                }
                return "";
            }
        }
    }
}
