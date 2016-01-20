using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.Serialization;


namespace Pebble_Time_Manager.ViewModels
{
    public class vmApp : INotifyPropertyChanged
    {
        #region Fields

        private string _Name;
        private string _ID;
        private bool _Selected;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the application
        /// </summary>
        [DataMember]
        public String Name 
        { 
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// ID of the application
        /// </summary>
        [DataMember]
        public String ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
                NotifyPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Icon
        /// </summary>
        /*public ImageSource AppIcon
        {
            get;
            set;
        }*/

        /// <summary>
        /// Selected state of this application
        /// </summary>
        [IgnoreDataMemberAttribute]
        public Boolean Selected
        {
            get
            {
                Boolean flag;
                try
                {
                    flag = AccessoryManager.IsNotificationEnabledForApplication(this.ID);
                }
                catch (Exception)
                {
                    return false;
                }
                return flag;
            }
            set
            {
                try
                {
                    if (value)
                    {
                        AccessoryManager.EnableNotificationsForApplication(this.ID);
                    }
                    else
                    {
                        AccessoryManager.DisableNotificationsForApplication(this.ID);
                    }
                }
                catch (Exception)
                {
                }

                NotifyPropertyChanged("Selected");
            }
        }

        #endregion

        #region Methods

        public async Task PopulateImageData()
        {
            /*(BitmapImage bitmapImage = new BitmapImage();
            IRandomAccessStreamWithContentType randomAccessStreamWithContentType = await AppIconStream.OpenReadAsync();
            try
            {
                await bitmapImage.SetSourceAsync(randomAccessStreamWithContentType);
                this.AppIcon = bitmapImage;
                this.NotifyPropertyChanged("");
            }
            finally
            {
                if (randomAccessStreamWithContentType != null)
                {
                    randomAccessStreamWithContentType.Dispose();
                }
            }*/
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
