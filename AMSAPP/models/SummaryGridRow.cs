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


        public string WeekDay { get; set; }
        
        public int Day { get; set; }

        public string Duration { get; set; }

        public string Comments { get
            {
                return _comments;
            }
            set {
                _comments = value;
                OnPropertyChanged("Comments");
            }
        }

        public string Color { get; set; }
    }
}
