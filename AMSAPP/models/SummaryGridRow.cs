using AMSAPP.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMSAPP
{
    public class SummaryGridRow : ViewModelBase
    {
        private string _comments;

        private string _AMScomments;

        public DateTime Date { get; set; }
        public string WeekDay { get; set; }
        
        public int Day { get; set; }

        public string Duration { get; set; }

        public string AMScomments { set { _AMScomments = value; } }

        public string Comments { get
            {
                if (!String.IsNullOrWhiteSpace(_comments))
                {
                    if (String.IsNullOrWhiteSpace(_AMScomments))
                    {
                        return  _comments;
                    }else
                    {
                        return _AMScomments + Environment.NewLine + _comments;
                    }
                }

                return _AMScomments;
            }
            set {
                _comments = value;
                OnPropertyChanged("Comments");
            }
        }

        public string Color { get; set; }
    }
}
