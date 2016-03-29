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
using P3bble;
using Windows.UI.Popups;
using Pebble_Time_Library.Connector;

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
            Store = new vmStore();

#if WINDOWS_PHONE_APP
            NotificationsHandler = new Connector.NotificationsHandler();
#endif
            //Applications = new vmApps();

            PageWatchFace = true;

            TimeLineSynchronizer.Log.CollectionChanged += Log_CollectionChanged;

            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();

            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            ClearCommand = new RelayCommand(ClearLog);
            ResyncCommand = new RelayCommand(Resync);
            SynchronizeCommand = new RelayCommand(SynchronizeCalender);
            RestoreCommand = new RelayCommand(Restore);
            BackupCommand = new RelayCommand(Backup);
            AssociateCommand = new RelayCommand(Associate);
            UndoAssociationCommand = new RelayCommand(UndoAssociation);
            UpdateCommand = new RelayCommand(Update);
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

        public vmStore Store { get; set; }

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
                NotifyPropertyChanged("PageStore");
                NotifyPropertyChanged("PageSettings");
                NotifyPropertyChanged("PagePace");
                NotifyPropertyChanged("PageTennis");
                NotifyPropertyChanged("Title");
            }
        }

        private bool? _IsMobile;
        public bool IsMobile
        {
            get
            {
                if (_IsMobile.HasValue) return _IsMobile.Value;

                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                _IsMobile = (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile");
                return _IsMobile.Value;
            }
        }

        public String Title
        {
            get
            {
                String PageName = "";
                String Result = "";

                switch (PageSelected)
                {
                    case 1: PageName = "WatchFaces"; break;
                    case 2: PageName = "WatchApps"; break;
                    case 3: PageName = "Connect"; break;
                    case 4: PageName = "Settings"; break;
                    case 5: PageName = "Store"; break;
                    case 6: PageName = "Tennis"; break;
                    case 7: PageName = "Pace"; break;
                }

                if (IsMobile)
                {
                    Result = String.Format("{0}", PageName);
                }
                else
                {
                    Result = String.Format("Pebble Time Manager - {0}", PageName);
                }

                return Result;
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

        public bool PageStore
        {
            get
            {
                return (PageSelected == 5);
            }
            set
            {
                PageSelected = 5;
            }
        }

        public bool PageTennis
        {
            get
            {
                return (PageSelected == 6);
            }
            set
            {
                PageSelected = 6;
            }
        }

        public bool PagePace
        {
            get
            {
                return (PageSelected == 7);
            }
            set
            {
                PageSelected = 7;
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

#if WINDOWS_PHONE_APP
        public NotificationsHandler NotificationsHandler { get; set; }
#endif

        public WipeHandler WipeHandler { get; set; }

        public ObservableCollection<String> Log { get; set; }



        public String Version
        {
            get
            {
                var PackageVersion = Windows.ApplicationModel.Package.Current.Id.Version;

                String Result = String.Format("version {0}.{1}.{2}", PackageVersion.Major, PackageVersion.Minor, PackageVersion.Build);

                return Result;
            }
        }

        public String Publisher
        {
            get
            {
#if WINDOWS_PHONE_APP
                return Windows.ApplicationModel.Package.Current.Id.Publisher;
#endif
#if WINDOWS_UWP
                return Windows.ApplicationModel.Package.Current.PublisherDisplayName;
#endif
            }
        }

        public String PackageDisplayName
        {
            get
            {
#if WINDOWS_PHONE_APP
                return Windows.ApplicationModel.Package.Current.Id.Name;
#endif
#if WINDOWS_UWP
                return Windows.ApplicationModel.Package.Current.DisplayName;
#endif
            }
        }
        #endregion

        #region Commands

        public RelayCommand UpdateCommand
        {
            get;
            private set;
        }

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
        public RelayCommand ResyncCommand
        {
            get;
            private set;
        }

        public RelayCommand SynchronizeCommand
        {
            get;
            private set;
        }

        public RelayCommand BackupCommand
        {
            get;
            private set;
        }

        public RelayCommand RestoreCommand
        {
            get;
            private set;
        }

        public RelayCommand AssociateCommand
        {
            get;
            private set;
        }

        public RelayCommand UndoAssociationCommand
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
                if (_IsConnecting) return;

                _IsConnecting = true;

                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                Log.Add("Connecting...");

                await _pc.StartBackgroundTask(PebbleConnector.Initiator.Manual);
            }
            catch (Exception exp)
            {
                _vmBinder.Log.Add("An exception occurred while connecting.");
                _vmBinder.Log.Add(exp.Message);
            }

            _IsConnecting = false;
        }
        private bool _IsConnecting;

        /// <summary>
        /// Disconnect Pebble Time and stop background communication task
        /// </summary>
        private void Disconnect(object obj)
        {
            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                Log.Add("Disconnecting...");
                if (WipeHandler != null && WipeHandler.IsConnected)
                {
                    WipeHandler.Disconnect();
                }
                else
                {
                    _pc.StopBackgroundTask(PebbleConnector.Initiator.Manual);
                }
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

        private async void Resync(object obj)
        {
            try
            {
                _vmBinder.Log.Clear();

                _vmBinder.Log.Add("Initiating resync...");

                WipeHandler = new WipeHandler(Log, TimeLineSynchronizer);
                await WipeHandler.Wipe();

                PebbleConnector _pc = PebbleConnector.GetInstance();
                IsConnected = _pc.IsConnected;
            }
            catch (Exception exp)
            {
                Log.Add("An exception occurred while resyncing.");
                Log.Add(exp.Message);
            }
        }

        private async void SynchronizeCalender(object obj)
        {
            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                await _pc.StartBackgroundTask(PebbleConnector.Initiator.Synchronize);
            }
            catch (Exception exp)
            {
                Log.Add("An exception occurred while synchronizing.");
                Log.Add(exp.Message);
            }
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
                    bool bConnected = false;

                    bConnected = (bool)localSettings.Values[Constants.BackgroundCommunicatieIsRunning];

                    if (WipeHandler != null && !bConnected) bConnected = WipeHandler.IsConnected;

                    IsConnected = bConnected;
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

#region Backup/Restore

        /// <summary>
        /// The last time a backup has been made
        /// </summary>
        public DateTime LastBackup
        {
            get
            {
                try
                {
                    var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                    if (roamingSettings.Values.Keys.Contains("lastbackup"))
                    {
                        String LastBackup = (string)roamingSettings.Values["lastbackup"];
                        return DateTime.Parse(LastBackup);
                    }
                }
                catch (Exception) { }

                return DateTime.MinValue;
            }

            set
            {
                var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                if (roamingSettings.Values.Keys.Contains("lastbackup"))
                {
                    roamingSettings.Values["lastbackup"] = value.ToString();
                }
                else
                {
                    roamingSettings.Values.Add("lastbackup", value.ToString());
                }

                NotifyPropertyChanged("LastBackup");
            }
        }

        private bool _BackupBusy;
        public bool BackupBusy
        {
            get
            {
                return _BackupBusy;
            }
            set
            {
                _BackupBusy = value;
                NotifyPropertyChanged("BackupBusy");
            }
        }

        private string _BackupStatus;
        public string BackupStatus
        {
            get
            {
                return _BackupStatus;
            }
            set
            {
                _BackupStatus = value;
                NotifyPropertyChanged("BackupStatus");
            }
        }

        private async void Backup(object obj)
        {
            try
            {
                PebbleConnector _pc = PebbleConnector.GetInstance();

                BackupBusy = true;
                BackupStatus = "Backing up watch items";

                bool Result = await _pc.WatchItems.Backup();

                if (Result)
                {
                    LastBackup = DateTime.Now;
                    BackupStatus = "Backup success";
                }
                else
                {
                    BackupStatus = "Backup failed";
                }
            }
            catch (Exception exp)
            {
                BackupStatus = "Backup failed";
            }

            BackupBusy = false;
        }

        private async void Restore(object obj)
        {
            try
            {
                BackupBusy = true;
                BackupStatus = "Retrieving backup from OneDrive";

                PebbleConnector _pc = PebbleConnector.GetInstance();

                bool Result = await _pc.WatchItems.Restore();

                if (Result)
                {
                    BackupStatus = "Restoring watch items";

                    Result = await _pc.WatchItems.Load();
                }

                if (Result)
                {
                    BackupStatus = "Restore success";
                }
                else
                {
                    BackupStatus = "Restore failed";
                }
            }
            catch (Exception exp)
            {
                BackupStatus = "Restore failed";
            }

            BackupBusy = false;
        }


#endregion

#region Device association

        private PebbleDevice _AssociatedDevice;
        public String AssociatedDeviceName
        {
            get
            {
                if (_AssociatedDevice == null)
                {
                    _AssociatedDevice = PebbleDevice.LoadAssociatedDevice();
                }
                if (_AssociatedDevice != null)
                {
                    return _AssociatedDevice.Name;
                }
                return "";
            }
        }

        public String AssociatedDeviceFirmware
        {
            get
            {
                if (_AssociatedDevice == null)
                {
                    _AssociatedDevice = PebbleDevice.LoadAssociatedDevice();
                }
                if (_AssociatedDevice != null)
                {
                    return "Firmware " + _AssociatedDevice.Firmware;
                }
                return "";
            }
        }

        public bool IsDeviceAssociated
        {
            get
            {
                PebbleDevice AssociatedDevice = PebbleDevice.LoadAssociatedDevice();
                return AssociatedDevice != null;
            }
        }

        private void UndoAssociation(object obj)
        {
            PebbleDevice.RemoveAssociation();

            NotifyPropertyChanged("IsDeviceAssociated");
            NotifyPropertyChanged("AssociatedDeviceFirmware");
            NotifyPropertyChanged("AssociatedDeviceName");
        }

        public async void Associate(object obj)
        {
            PebbleConnector _pc = PebbleConnector.GetInstance();
            {
                PebbleDeviceName = await _pc.GetCandidatePebble();

                if (PebbleDeviceName != null)
                {
                    String Message = String.Format("Device {0} found. Do you want to associate it?", PebbleDeviceName.Name);

                    var messageDialog = new Windows.UI.Popups.MessageDialog(Message);
                    messageDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.PebbleAssociate)));
                    messageDialog.Commands.Add(new UICommand("No"));

                    await messageDialog.ShowAsync();
                }
            }
        }

        private async void PebbleAssociate(IUICommand command)
        {
            Log.Add("Associating " + PebbleDeviceName.Name);

            try
            {
                PebbleConnector _pc = PebbleConnector.GetInstance();
                if (await _pc.AssociatePebble(PebbleDeviceName))
                {
                    Log.Add("Success");

                    NotifyPropertyChanged("IsDeviceAssociated");
                    NotifyPropertyChanged("AssociatedDeviceFirmware");
                    NotifyPropertyChanged("AssociatedDeviceName");

                    var successDialog = new Windows.UI.Popups.MessageDialog(String.Format("Association {0} completed successfully.", PebbleDeviceName.Name));
                    successDialog.Commands.Add(new UICommand("Ok"));

                    await successDialog.ShowAsync();

                    return;
                }
                else
                {
                    Log.Add("Failed");
                }

            }
            catch (Exception exp)
            {
                Log.Add(String.Format("An error occurred while associating {0}: {1}", PebbleDeviceName, exp.Message));
            }

            var messageDialog = new Windows.UI.Popups.MessageDialog(String.Format("Association {0} failed.", PebbleDeviceName.Name));
            messageDialog.Commands.Add(new UICommand("Ok"));

            await messageDialog.ShowAsync();
        }



        #endregion

        #region Updates

        public void Update(object obj)
        {
            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();
            _pc.WatchItems.CheckUpdates();

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
        private PebbleDevice PebbleDeviceName;

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
