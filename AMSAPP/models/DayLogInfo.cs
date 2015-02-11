using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMSAPP.models
{
    public class DayLogInfo
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Leave { get; set; }
        public string Comments { get; set; }

        public string Buffer { get; set; }

        public TimeSpan BufferTimespan
        {
            get
            {

                if (!string.IsNullOrWhiteSpace(Buffer))
                {
                    try
                    {
                        var result = TimeSpan.ParseExact(Buffer, @"hh\:mm", null);

                        return result;
                    }
                    catch (Exception)
                    {

                    }
                }

                return new TimeSpan(0, 0, 0);
            }
        }


        public double workingCount
        {
            get 
            { 
                if(Leave == "Full Day")
                {
                    return 0;
                }
                else if (Leave == "Half Day")
                {
                    return 0.5;
                }
                else if (Leave == "Working")
                {
                    return 1;
                }

                return 0;
            }
        }
        
    }
}
