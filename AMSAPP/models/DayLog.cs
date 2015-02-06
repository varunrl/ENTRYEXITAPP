using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMSAPP.models
{
    public  class DayLog
    {
        public DateTime Date { get; set; }

        public int Day
        {
            get
            {
                return this.Date.Day; ;
            }
        }

        public string WeekDay
        {
            get
            {
                return this.Date.DayOfWeek.ToString();
            }
        }

        public string Title { get; set; }

        public string Class { get; set; }

        public string Color { get; set; }

        public DayLogInfo info { get; set; }

    }
}
