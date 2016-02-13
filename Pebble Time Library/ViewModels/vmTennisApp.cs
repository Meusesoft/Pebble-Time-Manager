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
            StopCommand = new RelayCommand(Stop);
            DeleteCommand = new RelayCommand(Delete);
            SuspendCommand = new RelayCommand(Suspend);
            ResumeCommand = new RelayCommand(Resume);
            ShareCommand = new RelayCommand(Share);
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

        private void Stop(object obj)
        {
            if (OnStop != null) OnStop(this, EventArgs.Empty);
        }

        private void Suspend(object obj)
        {
            if (OnSuspend != null) OnSuspend(this, EventArgs.Empty);
        }

        private void Resume(object obj)
        {
            if (OnResume != null) OnResume(this, EventArgs.Empty);
        }

        private void Share(object obj)
        {
            if (OnShare != null) OnShare(this, EventArgs.Empty);
        }

        private void Delete(object obj)
        {
            TryInUse = false;
            NotifyPropertyChanged("Purchased");
            NotifyPropertyChanged("TryLeft");

            if (OnDelete != null) OnDelete(this, EventArgs.Empty);
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

        public void NotifyVisibility()
        {
            NotifyPropertyChanged("SwitchVisible");
            NotifyPropertyChanged("ResumeVisible");
            NotifyPropertyChanged("StopVisible");
            NotifyPropertyChanged("SuspendVisible");
            NotifyPropertyChanged("ExtendVisible");
            NotifyPropertyChanged("DeleteVisible");
            NotifyPropertyChanged("ShareVisible");
        }

        private void _vmMatch_NewState(object sender, EventArgs e)
        {
            NotifyVisibility();
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
                NotifyVisibility();
            }
        }

        public bool SwitchVisible
        {
            get
            {
                if (vmMatch==null) return false;
                return vmMatch.Switch && _TennisVisible;
            }
        }

        public bool ResumeVisible
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.Paused && _TennisVisible;
            }
        }

        public bool StopVisible
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.Stoppable && _TennisVisible;
            }
        }

        public bool SuspendVisible
        {
            get
            {
                if (vmMatch == null) return false;
                return !vmMatch.Paused && vmMatch.Stoppable && _TennisVisible;
            }
        }

        public bool ExtendVisible
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.IsExtendPossible && _TennisVisible;
            }
        }

        public bool DeleteVisible
        {
            get
            {
                if (vmMatch == null) return false;
                return vmMatch.Completed && _TennisVisible;
            }
        }
        public bool ShareVisible
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

        public RelayCommand StopCommand
        {
            get;
            private set;
        }

        public RelayCommand ResumeCommand
        {
            get;
            private set;
        }

        public RelayCommand ExtendCommand
        {
            get;
            private set;
        }

        public RelayCommand ShareCommand
        {
            get;
            private set;
        }

        public RelayCommand SuspendCommand
        {
            get;
            private set;
        }

        public RelayCommand DeleteCommand
        {
            get;
            private set;
        }

        #endregion

        #region Events

        public delegate void StopEventHandler(object sender, EventArgs e);
        public event StopEventHandler OnStop;

        public delegate void ResumeEventHandler(object sender, EventArgs e);
        public event ResumeEventHandler OnResume;

        public delegate void SuspendEventHandler(object sender, EventArgs e);
        public event SuspendEventHandler OnSuspend;

        public delegate void ShareEventHandler(object sender, EventArgs e);
        public event ShareEventHandler OnShare;

        public delegate void DeleteEventHandler(object sender, EventArgs e);
        public event DeleteEventHandler OnDelete;

        public delegate void ExtendEventHandler(object sender, EventArgs e);
        public event ExtendEventHandler OnExtend;

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
