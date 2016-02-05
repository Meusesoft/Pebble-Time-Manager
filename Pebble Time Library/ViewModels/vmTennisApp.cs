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
using Tennis_Statistics.ViewModels;

namespace Pebble_Time_Manager.ViewModels
{
    public class vmTennisApp : INotifyPropertyChanged
    {
        #region Constructor

        public vmTennisApp()
        {
            //Initialise properties
            TryInUse = false;
            TennisVisible = false;

            TryCommand = new RelayCommand(Try);
            SwitchCommand = new RelayCommand(Switch);
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

        /// <summary>
        /// Request a switch of the server
        /// </summary>
        /// <param name="obj"></param>
        private void Switch(object obj)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.TennisCommand] = "switch";
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

        private vmMatchState _vmMatch;
        /// <summary>
        /// The last match state
        /// </summary>
        public vmMatchState vmMatch
        {
            get
            {
                return _vmMatch;
            }

            set
            {
                _vmMatch = value;
                _vmMatch.NewState += _vmMatch_NewState;
                NotifyPropertyChanged("vmMatch");
            }
        }

        private void _vmMatch_NewState(object sender, EventArgs e)
        {
            NotifyPropertyChanged("CommandSwitch");
            NotifyPropertyChanged("CommandResume");
            NotifyPropertyChanged("CommandStop");
            NotifyPropertyChanged("CommandSuspend");
            NotifyPropertyChanged("CommandExtend");
            NotifyPropertyChanged("CommandDelete");
            NotifyPropertyChanged("CommandShare");
        }

        private bool _TennisVisible;
        public bool TennisVisible
        {
            get
            {
                return _TennisVisible;
            }
            set
            {
                _TennisVisible = value;
                NotifyPropertyChanged("TennisVisible");
                NotifyPropertyChanged("CommandSwitch");
                NotifyPropertyChanged("CommandResume");
                NotifyPropertyChanged("CommandStop");
                NotifyPropertyChanged("CommandSuspend");
                NotifyPropertyChanged("CommandExtend");
                NotifyPropertyChanged("CommandDelete");
                NotifyPropertyChanged("CommandShare");
            }
        }

        public bool CommandSwitch
        {
            get
            {
                if (vmMatch==null) return false;
                return vmMatch.Switch && _TennisVisible;
            }
        }

        public bool CommandResume
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.Paused && _TennisVisible;
            }
        }

        public bool CommandStop
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.Stoppable && _TennisVisible;
            }
        }

        public bool CommandSuspend
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.Stoppable && _TennisVisible;
            }
        }

        public bool CommandExtend
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.IsExtendPossible && _TennisVisible;
            }
        }

        public bool CommandDelete
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.Completed && _TennisVisible;
            }
        }
        public bool CommandShare
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.Completed && _TennisVisible;
            }
        }

        #endregion

        #region Commands

        public RelayCommand TryCommand
        {
            get;
            private set;
        }

        public RelayCommand SwitchCommand
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
