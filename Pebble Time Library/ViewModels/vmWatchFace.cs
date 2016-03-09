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
using Pebble_Time_Library.Javascript;
using P3bble.Types;
using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.Connector;

namespace Pebble_Time_Manager.ViewModels
{
    public class vmWatchFace : INotifyPropertyChanged
    {
        #region Constructor

        public vmWatchFace()
        {
            Editable = true;

            ConfigureCommand = new RelayCommand(Configure);
        }

        #endregion

        #region Fields

        private string _Name;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the watch face
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


        private bool _Selected;
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
                if (!Editable)
                {
                    _Selected = false;
                }
                else
                {
                    _Selected = value;
                }
                NotifyPropertyChanged("Selected");
            }
        }

        private bool _Editable;
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
        /// True if the watchface can be configured
        /// </summary>
        private bool _Configurable;
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
            }
        }

        /// <summary>
        /// The Guid of the watch face
        /// </summary>
        public Guid Model { get; set; }

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

        public String ImageFile { get; set; }

        public Pebble_Time_Manager.WatchItems.IWatchItem Item { get; set; }

        #endregion

        #region Methods

        public delegate void OpenConfigurationEventHandler(object sender, EventArgs e);
        public static event OpenConfigurationEventHandler OpenConfiguration;

        private DispatcherTimer _timer;
        private int _timerCycles;


        public async void Configure(object obj)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.PebbleWatchItem] = Item.ID.ToString();
            localSettings.Values[Constants.PebbleShowConfiguration] = "" ;

            PebbleConnector _pc = PebbleConnector.GetInstance();
            await _pc.StartBackgroundTask(PebbleConnector.Initiator.PebbleShowConfiguration);
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(250);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _timerCycles = 0;
        }

        private void _timer_Tick(object sender, object e)
        {
            _timerCycles++;

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            String URL = (String)localSettings.Values[Constants.PebbleShowConfiguration];
            if (URL.Length > 0)
            {
                PebbleKitJS.URLEventArgs _uea = new PebbleKitJS.URLEventArgs();
                _uea.URL = URL;
                _uea.WatchItem = Item;
                if (OpenConfiguration != null) OpenConfiguration(this, _uea);

                _timer.Stop();
            }

           // if (_timerCycles > 40) _timer.Stop();
        }

        #endregion

        #region Commands

        public RelayCommand ConfigureCommand
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
