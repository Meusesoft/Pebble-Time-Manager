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
    public class vmSportApp : INotifyPropertyChanged
    {
        #region Constructor

        public vmSportApp()
        {
            //Initialise properties
            _Duration = "-";
            _Distance = "-";
            _Pace = "-";
            _NotRunning = 0;
            TryInUse = false;

            //Initialise commands
            StartActivityCommand = new RelayCommand(StartActivity);
            StopActivityCommand = new RelayCommand(StopActivity);
            PauseActivityCommand = new RelayCommand(PauseActivity);
            ResumeActivityCommand = new RelayCommand(ResumeActivity);
            ShareActivityCommand = new RelayCommand(ShareActivity);
            TryCommand = new RelayCommand(Try);

            //Initialise local settings
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.PaceDuration] = "-";
            localSettings.Values[Constants.PacePace] = "-";
            localSettings.Values[Constants.PaceDistance] = "-";
            if (!localSettings.Values.Keys.Contains(Constants.PacePaused)) localSettings.Values[Constants.PacePaused] = false;
            if (!localSettings.Values.Keys.Contains(Constants.PaceSwitchPaused)) localSettings.Values[Constants.PaceSwitchPaused] = false;
            if (localSettings.Values.Keys.Contains(Constants.BackgroundCommunicatieIsRunning)) IsRunning = (bool)localSettings.Values[Constants.BackgroundCommunicatieIsRunning];
            if (localSettings.Values.Keys.Contains(Constants.PaceGPX)) Shareable = (bool)localSettings.Values[Constants.PaceGPX];


            //Initialise timer
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += _timer_Tick;

            if (IsRunning)
            {
                localSettings.Values[Constants.BackgroundCommunicatieIsRunning] = false;

                _Timer.Start();
            }

            if (localSettings.Values.Keys.Contains(Constants.Miles))
            {
                Miles = (bool)localSettings.Values[Constants.Miles];
            }
            else
            {
                Miles = !System.Globalization.RegionInfo.CurrentRegion.IsMetric;
            }
        }

        #endregion

        #region Fields

        private string _Duration;
        private string _Pace;
        private string _Distance;
        private bool _Paused;
        private bool _PaceSwitchPaused;
        private bool _IsRunning;
        private DispatcherTimer _Timer;
        private bool _Miles;
        private int _NotRunning;

        #endregion

        #region Properties

        /// <summary>
        /// The duration of the activity
        /// </summary>
        public String Duration
        {
            get
            {
                return _Duration;
            }
            set
            {
                _Duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        /// <summary>
        /// The duration of the activity
        /// </summary>
        public String Distance
        {
            get
            {
                return _Distance;
            }
            set
            {
                _Distance = value;
                NotifyPropertyChanged("Distance");
            }
        }

        /// <summary>
        /// The duration of the activity
        /// </summary>
        public String Pace
        {
            get
            {
                return _Pace;
            }
            set
            {
                _Pace = value;
                NotifyPropertyChanged("Pace");
            }
        }

        /// <summary>
        /// The activity paused state
        /// </summary>
        public bool Paused
        {
            get
            {
                return _Paused;
            }
            set
            {
                _Paused = value;
                NotifyPropertyChanged("Paused");
                NotifyPropertyChanged("Resumeable");
                NotifyPropertyChanged("Pauseable");
            }
        }

        /// <summary>
        /// The state of the pause switch 
        /// </summary>
        public bool PaceSwitchPaused
        {
            get
            {
                return _PaceSwitchPaused;
            }
            set
            {
                _PaceSwitchPaused = value;
                NotifyPropertyChanged("PaceSwitchPaused");
                NotifyPropertyChanged("Resumeable");
                NotifyPropertyChanged("Pauseable");
            }
        }

        /// <summary>
        /// The state of the background task 
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return _IsRunning;
            }
            set
            {
                _IsRunning = value;

                NotifyPropertyChanged("IsRunning");
                NotifyPropertyChanged("Resumeable");
                NotifyPropertyChanged("Pauseable");
                NotifyPropertyChanged("Shareable");
            }
        }

        /// <summary>
        /// The state of the background task 
        /// </summary>
        public bool Resumeable
        {
            get
            {
                return (Paused && IsRunning);
            }

        }

        /// <summary>
        /// The state of the background task 
        /// </summary>
        public bool Pauseable
        {
            get
            {
                return (!Paused && IsRunning);
            }

        }

        private bool _Shareable;
        /// <summary>
        /// The state of the background task 
        /// </summary>
        public bool Shareable
        {
            get
            {
                return _Shareable;
            }
            private set
            {
                if (_Shareable == false && value == true && !IsRunning && _Timer!=null) _Timer.Stop();
                _Shareable = value;
                NotifyPropertyChanged("Shareable");
            }

        }

        /// <summary>
        /// If true, sports app will use miles as distance measure
        /// </summary>
        public bool Miles
        {
            get
            {
                return _Miles;
            }
            set
            {
                _Miles = value;

                //save setting
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.Keys.Contains(Constants.Miles))
                {
                    localSettings.Values[Constants.Miles] = _Miles;
                }
                else
                {
                    localSettings.Values.Add(Constants.Miles, _Miles);
                }

                NotifyPropertyChanged("Miles");
            }
        }

        /// <summary>
        /// True if sports app purchased
        /// </summary>
        public bool Purchased
        {
            get
            {
                //return false;
                return TryInUse || Pebble_Time_Manager.Helper.Purchases.getReference().Available("pebble_sports");
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
                    (_Purchases.TryAvailable("pebble_sports") > 0) &&
                    !_Purchases.Available("pebble_sports"); 
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
                return Pebble_Time_Manager.Helper.Purchases.getReference().TryAvailable("pebble_sports");
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

        #region Methods

        /// <summary>
        /// Timer processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Tick(object sender, object e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            
            Duration = (String)localSettings.Values[Constants.PaceDuration];
            Pace = (String)localSettings.Values[Constants.PacePace];
            Distance = (String)localSettings.Values[Constants.PaceDistance];

            if (localSettings.Values.Keys.Contains(Constants.BackgroundCommunicatieIsRunning)) IsRunning = (bool)localSettings.Values[Constants.BackgroundCommunicatieIsRunning];
            if (localSettings.Values.Keys.Contains(Constants.PacePaused)) Paused = (bool)localSettings.Values[Constants.PacePaused];
            if (localSettings.Values.Keys.Contains(Constants.PaceGPX)) Shareable = (bool)localSettings.Values[Constants.PaceGPX];

            if (!IsRunning)
            {
                _NotRunning++;

                if (_NotRunning > 15) _Timer.Stop();
            }
            else
            {
                _NotRunning = 0;
            }

            //Check if error occurred
            if (localSettings.Values.Keys.Contains(Constants.BackgroundCommunicatieError) &&
                (int)localSettings.Values[Constants.BackgroundCommunicatieError] == (int)BCState.ConnectionFailed)
            {
                ExtendedEventArgs _fe = new ExtendedEventArgs();
                _fe.Error = "Connection with Pebble Time failed. Try again.";
                OnFatalError(_fe);

                _Timer.Stop();

                localSettings.Values[Constants.BackgroundCommunicatieError] = (int)BCState.OK;
            }
        }

        /// <summary>
        /// Start the activity
        /// </summary>
        /// <param name="obj"></param>
        private async void StartActivity(object obj)
        {
            if (!Purchased) return;

            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.BackgroundPace] = true;

            _NotRunning = 0;

            _Timer.Start();

            try
            {
                PebbleConnector _pc = PebbleConnector.GetInstance();
                await _pc.StartBackgroundTask(PebbleConnector.Initiator.Pace);
            }
            catch (Exception exc)
            {
                ExtendedEventArgs _e = new ExtendedEventArgs();
                _e.Error = exc.Message;
                OnFatalError(_e);
            }
        }

        /// <summary>
        /// Stop the activity
        /// </summary>
        /// <param name="obj"></param>
        private void StopActivity(object obj)
        {
            //var localSettings = ApplicationData.Current.LocalSettings;
            //localSettings.Values[Constants.BackgroundPace] = false;

            PebbleConnector _pc = PebbleConnector.GetInstance();
            _pc.StopBackgroundTask(PebbleConnector.Initiator.Pace);
        }

        /// <summary>
        /// Pause activity
        /// </summary>
        /// <param name="obj"></param>
        private void PauseActivity(object obj)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.PaceSwitchPaused] = true;
        }

        /// <summary>
        /// Resume activity
        /// </summary>
        /// <param name="obj"></param>
        private void ResumeActivity(object obj)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.PaceSwitchPaused] = true;
        }

        /// <summary>
        /// Try
        /// </summary>
        /// <param name="obj"></param>
        private void Try(object obj)
        {
            Pebble_Time_Manager.Helper.Purchases.getReference().TryUse("pebble_sports");
            TryInUse = true;

            NotifyPropertyChanged("Purchased");
            NotifyPropertyChanged("TryLeft");
        }
        
        /// <summary>
                 /// Share activity
                 /// </summary>
                 /// <param name="obj"></param>
        private void ShareActivity(object obj)
        {


        }
        #endregion

        #region Commands

        public RelayCommand StartActivityCommand
        {
            get;
            private set;
        }

        public RelayCommand StopActivityCommand
        {
            get;
            private set;
        }

        public RelayCommand PauseActivityCommand
        {
            get;
            private set;
        }

        public RelayCommand ResumeActivityCommand
        {
            get;
            private set;
        }

        public RelayCommand ShareActivityCommand
        {
            get;
            private set;
        }

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

        #region ErrorHandling

        // A delegate type for hooking up change notifications.
        public delegate void ErrorEventHandler(object sender, ExtendedEventArgs e);

        /// <summary>
        /// The event client can use to be notified when a fatal error occurrs
        /// </summary>
        public event ErrorEventHandler FatalError;

        // Invoke the ConnectionError event; 
        protected virtual void OnFatalError(ExtendedEventArgs e)
        {
            if (FatalError != null)
                FatalError(this, e);
        }

        #endregion
    }

    public class ExtendedEventArgs : EventArgs
    {
        public String Error { get; set; }
        public object Value { get; set; }
    }
}
