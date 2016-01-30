using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace Pebble_Time_Manager.ViewModels
{
    public class vmStore : INotifyPropertyChanged
    {

        #region Properties

        private bool _ClearLog;
        public bool ClearLog
        {
            get
            {
                return _ClearLog;
            }
            set
            {
                _ClearLog = value;
                NotifyPropertyChanged("ClearLog");
            }
        }



        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


    }
}
