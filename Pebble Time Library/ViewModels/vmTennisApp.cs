using Pebble_Time_Manager.Common;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.UI.Xaml;
using Pebble_Time_Manager.Connector;

namespace Pebble_Time_Manager.ViewModels
{
    public class vmTennisApp : INotifyPropertyChanged
    {
        #region Constructor

        public vmTennisApp()
        {
            //Initialise properties
            TryInUse = false;

            TryCommand = new RelayCommand(Try);
        }

        #endregion

        #region Fields


        #endregion

        #region Methods

        /// <summary>
        /// Try
        /// </summary>
        /// <param name="obj"></param>
        private void Try(object obj)
        {
            Pebble_Time_Manager.Helper.Purchases.getReference().TryUse("pebble_tennis");
            TryInUse = true;

            NotifyPropertyChanged("Purchased");
            NotifyPropertyChanged("TryLeft");
        }

        #endregion

        #region Properties

        /// <summary>
        /// True if sports app purchased
        /// </summary>
        public bool Purchased
        {
            get
            {
                //return false;
                return TryInUse || Pebble_Time_Manager.Helper.Purchases.getReference().Available("pebble_tennis");
            }
            set
            {
                NotifyPropertyChanged("Purchased");
                NotifyPropertyChanged("TryLeft");
            }
        }

        /// <summary>
        /// True if a sports app try is left
        /// </summary>
        public bool TryLeft
        {
            get
            {
                Pebble_Time_Manager.Helper.Purchases _Purchases = Pebble_Time_Manager.Helper.Purchases.getReference();

                //return false;
                return !TryInUse && 
                    (_Purchases.TryAvailable("pebble_tennis") > 0) &&
                    !_Purchases.Available("pebble_tennis"); 
            }
        }

        /// <summary>
        /// Returns the number of tries
        /// </summary>
        public int TriesLeft
        {
            get
            {
                //return false;
                return Pebble_Time_Manager.Helper.Purchases.getReference().TryAvailable("pebble_tennis");
            }
        }
        
        /// <summary>
                 /// A try is in use
                 /// </summary>
        public bool TryInUse
        {
            get; set;
        }

        #endregion

        #region Commands

        public RelayCommand TryCommand
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
