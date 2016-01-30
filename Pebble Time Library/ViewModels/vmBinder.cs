using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Tennis_Statistics.ViewModels;
using Pebble_Time_Manager.Connector;
using Windows.UI.Xaml;
using Windows.Storage;
using Pebble_Time_Manager.Common;

namespace Pebble_Time_Manager.ViewModels
{
    public class vmBinder : INotifyPropertyChanged
    {
        #region Constructor

        public vmBinder()
        {
            TimeLineSynchronizer = new Connector.TimeLineSynchronizer();
            WatchFaces = new vmWatchFaces();
            WatchApps = new vmWatchApps();
            Sport = new vmSportApp();
            Tennis = new vmTennisApp();
            Log = new ObservableCollection<string>();
            Commands = new vmCommands();

            //NotificationsHandler = new Connector.NotificationsHandler();
            //Applications = new vmApps();


            TimeLineSynchronizer.Log.CollectionChanged += Log_CollectionChanged;

            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();

            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            ClearCommand = new RelayCommand(ClearLog);
        }

        private void Log_CollectionChanged1(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Fields

        private DispatcherTimer _Timer;

        #endregion

        #region Properties

        public vmCommands Commands { get; set; }

        public vmWatchFaces WatchFaces { get; set; }

        public vmWatchApps WatchApps { get; set; }

        public vmSportApp Sport { get; set; }

        int _PageSelected = 1;
        private int PageSelected
        {
            get
            {
                return _PageSelected;
            }
            set
            {
                _PageSelected = value;
                NotifyPropertyChanged("PageWatchFace");
                NotifyPropertyChanged("PageWatchApp");
                NotifyPropertyChanged("PageConnect");
                NotifyPropertyChanged("PageSettings");
            }
        }

        public bool PageWatchFace
        {
            get
            {
                return (PageSelected == 1);
            }
            set
            {
                PageSelected = 1;
            }
        }

        public bool PageWatchApp
        {
            get
            {
                return (PageSelected == 2);
            }
            set
            {
                PageSelected = 2;
            }
        }

        public bool PageConnect
        {
            get
            {
                return (PageSelected == 3);
            }
            set
            {
                PageSelected = 3;
            }
        }

        public bool PageSettings
        {
            get
            {
                return (PageSelected == 4);
            }
            set
            {
                PageSelected = 4;
            }
        }

        private vmNewMatch _newMatch;
        public vmNewMatch vmNewMatch
        {
            get
            {
                return _newMatch;
            }

            set
            {
                _newMatch = value;
                NotifyPropertyChanged("vmNewMatch");
            }
        }

        private vmMatchState _vmMatch;
        public vmMatchState vmMatch
        {
            get
            {
                return _vmMatch;
            }

            set
            {
                _vmMatch = value;
                NotifyPropertyChanged("vmMatch");
            }
        }

        private vmTennisApp _vmTennisApp;
        public vmTennisApp Tennis
        {
            get
            {
                return _vmTennisApp;
            }

            set
            {
                _vmTennisApp = value;
                NotifyPropertyChanged("Tennis");
            }
        }
        private bool _IsConnected;
        /// <summary>
        /// True if background communication task is running and Pebble is connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }

            set
            {
                if (_IsConnected != value)
                {
                    _IsConnected = value;
                    NotifyPropertyChanged("IsConnected");
                }
            }
        }

        //public vmApps Applications { get; set; }

        public TimeLineSynchronizer TimeLineSynchronizer { get; set; }

        //public NotificationsHandler NotificationsHandler { get; set; }

        public ObservableCollection<String> Log { get; set; }

        #endregion

        #region Commands

        public RelayCommand ConnectCommand
        {
            get;
            private set;
        }

        public RelayCommand DisconnectCommand
        {
            get;
            private set;
        }

        public RelayCommand ClearCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Connect to Pebble Time and start a new background communication task
        /// </summary>
        private async void Connect(object obj)
        {
            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                Log.Add("Connecting...");

                await _pc.StartBackgroundTask(PebbleConnector.Initiator.Manual);
            }
            catch (Exception exp)
            {
                _vmBinder.Log.Add("An exception occurred while connecting.");
                _vmBinder.Log.Add(exp.Message);
            }
        }

        /// <summary>
        /// Disconnect Pebble Time and stop background communication task
        /// </summary>
        private void Disconnect(object obj)
        {
            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                Log.Add("Disconnecting...");

                _pc.StopBackgroundTask(PebbleConnector.Initiator.Manual);
            }
            catch (Exception exp)
            {
                Log.Add("An exception occurred while disconnecting.");
                Log.Add(exp.Message);
            }
        }

        private void ClearLog(object obj)
        {
            Log.Clear();
        }

        #endregion

        #region Event handlers

        void Log_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    Log.Add((String)item);
                }
            }
        }

        /// <summary>
        /// Check if log is available and process it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Timer_Tick(object sender, object e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            try
            {
                //Update connection status
                if (localSettings.Values.ContainsKey(Constants.BackgroundCommunicatieIsRunning))
                {
                    IsConnected = (bool)localSettings.Values[Constants.BackgroundCommunicatieIsRunning];
                }

                //Process logging
                if (Log != null)
                {
                    if (ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.BackgroundCommunicatieLog))
                    {
                        String Log = (String)ApplicationData.Current.LocalSettings.Values[Constants.BackgroundCommunicatieLog];
                        ApplicationData.Current.LocalSettings.Values[Constants.BackgroundCommunicatieLog] = "";

                        if (Log.Length > 0)
                        {
                            string[] lines = Log.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                            foreach (string line in lines)
                            {
                                if (line.Length > 0) this.Log.Add(line);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                ApplicationData.Current.LocalSettings.Values.Remove(Constants.BackgroundCommunicatieLog);
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
        
        private static vmBinder _vmBinder;
        /// <summary>
        /// Returns the global instance of the PebbleConnector class
        /// </summary>
        /// <returns></returns>

        public static vmBinder GetInstance()
        {
            if (_vmBinder == null) _vmBinder = new vmBinder();

            return _vmBinder;
        }


    }
}
