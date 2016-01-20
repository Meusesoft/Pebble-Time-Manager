using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using Microsoft.Live;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Tennis_Statistics.ViewModels
{
    [DataContract]
    public class vmPlayer
    {
        [DataMember]
        public String ID { get; set; }

        private String m_Name;
        [DataMember]
        public String Name { 
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        [DataMember]
        public String ProfileImage { get; set; }

        [DataMember]
        public bool LocalPlayer { get; set; }

        [DataMember]
        public DateTime LastMatch { get; set; }

        private BitmapImage _ProfilePicture;
        /// <summary>
        /// The source for the user its profile image from its Microsoft Account
        /// </summary>
        public BitmapImage ProfilePicture
        {
            get
            {
                if (_ProfilePicture == null)
                {
                    _ProfilePicture = new BitmapImage();
                    SetProfileImageSource(_ProfilePicture, true);
                  //  t.Start();
                  //  t.Wait();
                }

                return _ProfilePicture;
            }
            set
            {
                _ProfilePicture = value;
                NotifyPropertyChanged("ProfilePicture");
            }
        }


        #region Methods

        /// <summary>
        /// Initiate the download of the user its profile picture
        /// </summary>
        public async Task LoadProfilePicture()
        {
            try
            {
                StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
                StorageFile File = await LocalFolder.CreateFileAsync(ID + ".png", CreationCollisionOption.ReplaceExisting);

                var Downloader = new Windows.Networking.BackgroundTransfer.BackgroundDownloader();
                var DownloadOperation = Downloader.CreateDownload(new Uri(String.Format("https://apis.live.net/v5.0/{0}/picture", ID)), File);
                await ProcessDownloadProfilePicture(DownloadOperation, File);

                NotifyPropertyChanged("ProfilePicture");
            }
            catch (Exception e)
            {
                ExtendedEventArgs eea = new ExtendedEventArgs();
                eea.Error = String.Format("An error occurred while accessing the profile picture of your Microsoft account: {0}", e.Message);
                OnConnectionError(eea);
            }
        }

        /// <summary>
        /// Perform the download of the user its profile picture asynchronously
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="File"></param>
        /// 
        private async Task ProcessDownloadProfilePicture(Windows.Networking.BackgroundTransfer.DownloadOperation operation, StorageFile File)
        {
            try
            {
                var asyncOperation = operation.StartAsync();
                await asyncOperation;
            }
            catch (Exception e)
            {
                ExtendedEventArgs eea = new ExtendedEventArgs();
                eea.Error = String.Format("An error occurred while accessing the profile picture of your Microsoft account: {0}", e.Message);
                OnConnectionError(eea);
            }
        }

        /// <summary>
        /// Load the profile picture from the local storage asynchronously
        /// </summary>
        /// <param name="destination"></param>
        public async Task<bool> SetProfileImageSource(BitmapImage destination, bool Retry)
        {
            try
            {
                StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
                StorageFile File = await LocalFolder.GetFileAsync(ID + ".png");

                FileRandomAccessStream stream = (FileRandomAccessStream)await File.OpenAsync(FileAccessMode.Read);
                destination.SetSource(stream);

                Retry = false;
            }
            catch (System.IO.FileNotFoundException)
            {
                Retry = true;
            }
            catch (Exception)
            {          
            }

            if (Retry)
            {
                Tennis_Statistics.Helpers.Settings _Settings = Tennis_Statistics.Helpers.Settings.GetInstance();
                object value = _Settings.Get("ConnectedToMicrosoftAccount");
                if (value is bool)
                {
                    if ((bool)value)
                    {
                        await LoadProfilePicture();
                        await SetProfileImageSource(destination, Retry);
                    }
                }
            }

            NotifyPropertyChanged("ProfilePicture");

            return true;
        }

        #endregion

        #region Events

        // A delegate type for hooking up change notifications.
        public delegate void ConnectionErrorEventHandler(object sender, ExtendedEventArgs e);

        /// <summary>
        /// The event client can use to be notified when the connection to the Microsoft account fails
        /// </summary>
        public event ConnectionErrorEventHandler ConnectionError;

        // Invoke the ConnectionError event; 
        protected virtual void OnConnectionError(ExtendedEventArgs e)
        {
            
            if (ConnectionError != null)
                ConnectionError(this, e);
        }

        #endregion

        #region Commands

        public RelayCommand StartActivity
        {
            get;
            private set;
        }

        public RelayCommand StopActivity
        {
            get;
            private set;
        }

        public RelayCommand PauseActivity
        {
            get;
            private set;
        }

        public RelayCommand ResumeActivity
        {
            get;
            private set;
        }

        private void ExecuteMyCommand()
        {
            // Do something 
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

    public class ExtendedEventArgs : EventArgs
    {
        public String Error { get; set; }
        public object Value { get; set; }
    }
}
