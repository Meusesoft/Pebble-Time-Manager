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
            URL = new Uri("https://apps.getpebble.com/en_US/watchfaces");

            StoreAppsCommand = new RelayCommand(OpenStoreApps);
            StoreFacesCommand = new RelayCommand(OpenStoreFaces);
            StoreSearchCommand = new RelayCommand(OpenStoreSearch);
            DownloadCommand = new RelayCommand(StartDownloadItem);

            this.DownloadAvailable = false;
        }

        #endregion

        #region Properties

        private Uri _URL;
        public Uri URL
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
        public bool DownloadAvailable
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
            URL = new Uri("https://apps.getpebble.com/en_US/watchfaces");
        }

        private void OpenStoreApps(object obj)
        {
            URL = new Uri("https://apps.getpebble.com/en_US/watchapps");
        }

        private void OpenStoreSearch(object obj)
        {
            URL = new Uri("https://apps.getpebble.com/en_US/search");
        }

        public void CheckDownloadableItem(Uri URL)
        {
            DownloadAvailable = URL.AbsolutePath.Contains("en_US/application/");
        }

        private void StartDownloadItem(object obj)
        {
            if (StartDownload != null) StartDownload(this, EventArgs.Empty);
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

        #region Events

        public delegate void StartDownloadEventHandler(object sender, EventArgs e);
        public event StartDownloadEventHandler StartDownload;

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
