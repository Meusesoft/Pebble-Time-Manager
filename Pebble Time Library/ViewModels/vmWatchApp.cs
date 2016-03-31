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
using Pebble_Time_Manager.Connector;
using Pebble_Time_Manager.Common;
using Pebble_Time_Library.Javascript;

namespace Pebble_Time_Manager.ViewModels
{
    public class vmWatchApp : INotifyPropertyChanged
    {
        #region Constructor

        public vmWatchApp()
        {
            Editable = true;

            ConfigureCommand = new RelayCommand(Configure);
            UpdateCommand = new RelayCommand(Update);
        }

        #endregion

        #region Fields

        private string _Name;
        private bool _Selected;
        private bool _Editable;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the watch app
        /// </summary>
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

        private String _Developer;
        public String Developer
        {
            get
            {
                return _Developer;
            }
            set
            {
                _Developer = value;
                NotifyPropertyChanged("Developer");
            }
        }

        /// <summary>
        /// The Guid of the watch app
        /// </summary>
        public Guid Model;

        /// <summary>
        /// Selected
        /// </summary>
        public bool Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
                NotifyPropertyChanged("Selected");
            }
        }

        /// <summary>
        /// Editable
        /// </summary>
        public bool Editable
        {
            get
            {
                return _Editable;
            }
            set
            {
                _Editable = value;
                NotifyPropertyChanged("Editable");
            }
        }

        private bool _Active;
        /// <summary>
        /// The active watch face
        /// </summary>
        public bool Active
        {
            get
            {
                return _Active;
            }
            set
            {
                _Active = value;
                NotifyPropertyChanged("Active");
            }
        }

        /// <summary>
        /// The Guid of the watch app
        /// </summary>
        private Windows.UI.Xaml.Media.Imaging.BitmapImage _Image;
        public Windows.UI.Xaml.Media.Imaging.BitmapImage Image
        {

            get
            {
                return _Image;
            }
            set
            {
                _Image = value;
                NotifyPropertyChanged("Image");
            }
        }

        private bool _Configurable;
        /// <summary>
        /// True if the watchface can be configured
        /// </summary>
        public bool Configurable
        {
            get
            {
                return _Configurable;
            }
            set
            {
                _Configurable = value;
                NotifyPropertyChanged("Configurable");
                NotifyPropertyChanged("ShowConfigureOption");
            }
        }

        public bool ShowConfigureOption
        {
            get
            {
                return _Configurable && !_Configuring;
            }
        }

        private bool _Configuring;
        /// <summary>
        /// True if configure webpage is being started
        /// </summary>
        public bool Configuring
        {
            get
            {
                return _Configuring && Configurable;
            }
            set
            {
                _Configuring = value;
                NotifyPropertyChanged("Configuring");
                NotifyPropertyChanged("ShowConfigureOption");
            }
        }

        /// <summary>
        /// True if an update is available in the Pebble store
        /// </summary>
        public bool UpdateAvailable
        {
            get
            {
                if (Item == null) return false;
                return Item.UpdateAvailable;
            }
        }

        private bool _Updating;
        /// <summary>
        /// Update is in progress
        /// </summary>
        public bool Updating
        {
            get
            {
                return _Updating;
            }
            set
            {
                _Updating = value;
                NotifyPropertyChanged("Updating");
            }

        }

        public String ImageFile { get; set; }

        public Pebble_Time_Manager.WatchItems.IWatchItem Item { get; set; }

        #endregion

        #region Events

        public delegate void OpenConfigurationEventHandler(object sender, EventArgs e);
        public static event OpenConfigurationEventHandler OnOpenConfiguration;

        public delegate void ExceptionEventHandler(object sender, EventArgs e);
        public static event ExceptionEventHandler OnException;

        #endregion

        #region Methods

        private DispatcherTimer _timer;
        private int _timerCycles;

        public async void Configure(object obj)
        {
            Configuring = true;

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.PebbleWatchItem] = Item.ID.ToString();
            localSettings.Values[Constants.PebbleShowConfiguration] = "";

            PebbleConnector _pc = PebbleConnector.GetInstance();
            await _pc.StartBackgroundTask(PebbleConnector.Initiator.PebbleShowConfiguration);
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _timerCycles = 0;
        }

        private void _timer_Tick(object sender, object e)
        {
            _timerCycles++;

            //Check for result
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            String URL = (String)localSettings.Values[Constants.PebbleShowConfiguration];
            if (URL.Length > 0)
            {
                PebbleKitJS.URLEventArgs _uea = new PebbleKitJS.URLEventArgs();
                _uea.URL = URL;
                _uea.WatchItem = Item;
                if (OnOpenConfiguration != null) OnOpenConfiguration(this, _uea);

                Configuring = false;
                _timer.Stop();
            }

            //Check if error occurred
            if (localSettings.Values.Keys.Contains(Constants.BackgroundCommunicatieError) &&
                (int)localSettings.Values[Constants.BackgroundCommunicatieError] == (int)BCState.ConnectionFailed)
            {
                localSettings.Values[Constants.BackgroundCommunicatieError] = (int)BCState.OK;

                ErrorEventArgs ea = new ErrorEventArgs();
                ea.Error = "Connection failed with Pebble Time.";

                if (OnException != null) OnException(this, ea);

                Configuring = false;
                _timer.Stop();
            }

            //Check for time out
            if (_timerCycles > 40)
            {
                Configuring = false;
                _timer.Stop();

                ErrorEventArgs ea = new ErrorEventArgs();
                ea.Error = "Time out occurred while opening the settings window.";

                if (OnException != null) OnException(this, ea);
            }
        }

        public async Task CheckUpdate()
        {
            if (Item != null)
            {
                await Item.CheckUpdate();
                NotifyPropertyChanged("UpdateAvailable");
            }
        }

        #endregion

        #region Commands

        public RelayCommand ConfigureCommand
        {
            get;
            private set;
        }

        public RelayCommand UpdateCommand
        {
            get;
            private set;
        }

        #endregion

        #region Update

        private void Update(object obj)
        {
            try
            {
                String PackageID = Item.File.Replace(".zip", "");

                //Initiate download
                WatchItems.WatchItems.OnDownloadEvent += WatchItems_OnDownloadEvent;
                WatchItems.WatchItems.Download(PackageID);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Process the download events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WatchItems_OnDownloadEvent(object sender, WatchItems.WatchItems.DownloadEventArgs e)
        {
            switch (e.State)
            {
                case WatchItems.WatchItems.DownloadState.Initiate:

                    Updating = true;

                    break;

                case WatchItems.WatchItems.DownloadState.InProgress:

                    break;

                case WatchItems.WatchItems.DownloadState.Done:

                    Updating = false;
                    Item.UpdateAvailable = false;

                    NotifyPropertyChanged("UpdateAvailable");

                    //Set the name of the file in a localsetting
                    var localSettings = ApplicationData.Current.LocalSettings;
                    localSettings.Values[Constants.BackgroundCommunicatieDownloadedItem] = e.Status;

                    //Start background task to sed new item to pebble
                    Pebble_Time_Manager.Connector.PebbleConnector _pc = Pebble_Time_Manager.Connector.PebbleConnector.GetInstance();
                    try
                    {
                        await _pc.StartBackgroundTask(Connector.PebbleConnector.Initiator.AddItem);
                    }
                    catch (Exception exp)
                    {
                        System.Diagnostics.Debug.WriteLine(exp.Message);
                    }

                    //Add new item to viewmodel
                    WatchItems.WatchItem _newItem;
                    _newItem = await WatchItems.WatchItem.Load(e.Status);
                    await _pc.WatchItems.AddWatchItem((WatchItems.WatchItem)_newItem);

                    Item = _newItem;

                    break;

                case WatchItems.WatchItems.DownloadState.Error:

                    Updating = false;

                    break;

                case WatchItems.WatchItems.DownloadState.Canceled:

                    break;
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


    }
}
