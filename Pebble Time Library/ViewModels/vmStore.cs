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
        #region Constructor

        public vmStore()
        {
            URL = "https://apps.getpebble.com/en_US/watchfaces";

            StoreAppsCommand = new RelayCommand(OpenStoreApps);
            StoreFacesCommand = new RelayCommand(OpenStoreFaces);
            StoreSearchCommand = new RelayCommand(OpenStoreSearch);
        }

        #endregion


        #region Properties

        private String _URL;
        public String URL
        {
            get
            {
                return _URL;
            }
            set
            {
                _URL = value;
                NotifyPropertyChanged("URL");
            }
        }

        private bool _DownloadAvailable;
        public bool DownloadAvailabele
        {
            get
            {
                return _DownloadAvailable;
            }
            set
            {
                _DownloadAvailable = value;
                NotifyPropertyChanged("DownloadAvailable");
            }
        }

        #endregion

        #region Methods

        private void OpenStoreFaces(object obj)
        {
            URL = "https://apps.getpebble.com/en_US/watchfaces";
        }

        private void OpenStoreApps(object obj)
        {
            URL = "https://apps.getpebble.com/en_US/watchapps";
        }

        private void OpenStoreSearch(object obj)
        {
            URL = "https://apps.getpebble.com/en_US/search";
        }

        #endregion

        #region Commands

        public RelayCommand StoreFacesCommand
        {
            get;
            private set;
        }

        public RelayCommand StoreAppsCommand
        {
            get;
            private set;
        }

        public RelayCommand StoreSearchCommand
        {
            get;
            private set;
        }

        public RelayCommand DownloadCommand
        {
            get;
            private set;
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
