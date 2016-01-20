using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.ComponentModel;
using System.Threading.Tasks;
using Tennis_Statistics.Helpers;
using Windows.UI.Xaml.Media;
using Microsoft.Live;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Tennis_Statistics.ViewModels
{

    public class vmSettings : INotifyPropertyChanged
    {
        private const string _DefaultScreenName = "Player";

        #region Constructor

        public vmSettings()
        {
            _MicrosoftAccount = MicrosoftAccount.GetInstance();
            _MicrosoftAccount.ConnectionChanged += MicrosoftAccount_ConnectionChanged;
            _MicrosoftAccount.ConnectionError += _MicrosoftAccount_ConnectionError;

        }

        #endregion

        #region Fields

        private MicrosoftAccount _MicrosoftAccount;

        #endregion

        #region Properties
        private Settings m_Settings;
        private Settings _Settings
        {
            get
            {
                if (m_Settings == null)
                {
                    m_Settings = Settings.GetInstance();
                }

                return m_Settings;
            }
        }

        /// <summary>
        /// Share the location
        /// </summary>
        public bool ShareLocation 
        { 
            get
            {
                object value = _Settings.Get("ShareLocation");
                if (value == null) return false;
                return (bool)value;
            }

            set
            {
                _Settings.Set("ShareLocation", value);
            }
        }

        /// <summary>
        /// Share the set scores
        /// </summary>
        public bool ShareSetScores
        {
            get
            {
                object value = _Settings.Get("ShareSetScores");
                if (value == null) return true;
                return (bool)value;
            }

            set
            {
                _Settings.Set("ShareSetScores", value);
            }
        }

        /// <summary>
        /// Share the duration of the match
        /// </summary>
        public bool ShareDuration
        {
            get
            {
                object value = _Settings.Get("ShareDuration");
                if (value == null) return true;
                return (bool)value;
            }

            set
            {
                _Settings.Set("ShareDuration", value);
            }
        }

        /// <summary>
        /// True if the background is black
        /// </summary>
        public bool BlackBackground
        {
            get
            {
                object value = _Settings.Get("Background");
                if (!(value is string)) return false;
                return ((string)value == "Black");
            }

            set
            {
                _Settings.Set("Background", "Black");
                NotifyPropertyChanged("BlackBackground");
                NotifyPropertyChanged("WhiteBackground");
                NotifyPropertyChanged("BlueBackground");
                NotifyPropertyChanged("DefaultBackground");
                NotifyPropertyChanged("UserDefinedBackground");
            }
        }

        /// <summary>
        /// True if the background is white
        /// </summary>
        public bool WhiteBackground
        {
            get
            {
                object value = _Settings.Get("Background");
                if (!(value is string)) return false;
                return ((string)value == "White");
            }

            set
            {
                _Settings.Set("Background", "White");
                NotifyPropertyChanged("BlackBackground");
                NotifyPropertyChanged("WhiteBackground");
                NotifyPropertyChanged("BlueBackground");
                NotifyPropertyChanged("DefaultBackground");
                NotifyPropertyChanged("UserDefinedBackground");
            }
        }

        /// <summary>
        /// True if the background is black
        /// </summary>
        public bool BlueBackground
        {
            get
            {
                object value = _Settings.Get("Background");
                if (!(value is string)) return false;
                return ((string)value == "Blue");
            }

            set
            {
                _Settings.Set("Background", "Blue");
                NotifyPropertyChanged("BlackBackground");
                NotifyPropertyChanged("WhiteBackground");
                NotifyPropertyChanged("BlueBackground"); 
                NotifyPropertyChanged("DefaultBackground");
                NotifyPropertyChanged("UserDefinedBackground");
            }
        }

        /// <summary>
        /// True if the background is the default
        /// </summary>
        public bool DefaultBackground
        {
            get
            {
                object value = _Settings.Get("Background");
                if (!(value is string)) return true;
                return ((string)value == "Default");
            }

            set
            {
                _Settings.Set("Background", "Default");
                NotifyPropertyChanged("BlackBackground");
                NotifyPropertyChanged("WhiteBackground");
                NotifyPropertyChanged("DefaultBackground");
                NotifyPropertyChanged("UserDefinedBackground");
            }
        }

        /// <summary>
        /// True if the background is user defined
        /// </summary>
        public bool UserDefinedBackground
        {
            get
            {
                object value = _Settings.Get("Background");
                if (!(value is string)) return false;
                return ((string)value == "UserDefined");
            }

            set
            {
                _Settings.Set("Background", "UserDefined");
                NotifyPropertyChanged("BlackBackground");
                NotifyPropertyChanged("BlueBackground");
                NotifyPropertyChanged("WhiteBackground");
                NotifyPropertyChanged("DefaultBackground");
                NotifyPropertyChanged("UserDefinedBackground");
            }
        }

        private List<BackgroundImage> m_BackgroundImages;
        public List<BackgroundImage> BackgroundImages
        {
            get
            {
                if (m_BackgroundImages == null)
                {
                    m_BackgroundImages = new List<BackgroundImage>(0);
                    m_BackgroundImages.Add(new BackgroundImage("Assets/australianopen.jpg"));
                    m_BackgroundImages.Add(new BackgroundImage("Assets/rolandgarros.jpg"));
                    m_BackgroundImages.Add(new BackgroundImage("Assets/wimbledon.jpg"));
                    m_BackgroundImages.Add(new BackgroundImage("Assets/usopen2.jpg"));
                    m_BackgroundImages.Add(new BackgroundImage("Assets/atptourfinals.jpg"));
                }

                return m_BackgroundImages;
            }
        }

        /// <summary>
        /// The status of the connection
        /// </summary>
        public bool ConnectedToMicrosoftAccount
        {
            get
            {
                return _MicrosoftAccount.Connected;
            }
        }

        private vmPlayer m_Player;
        public vmPlayer LocalPlayer
        {
            get
            {
                if (m_Player == null)
                {
                    m_Player = new vmPlayer();
                    m_Player.ConnectionError += m_Player_ConnectionError;
                    object value = _Settings.Get("UserID");
                    if (value is string) m_Player.ID = (string)value;
                    value = _Settings.Get("ScreenName");
                    if (value is string) m_Player.Name = (string)value;
                }
                return m_Player;
            }
            set
            {

            }
        }

        /// <summary>
        /// The profile picture bitmap
        /// </summary>
        public BitmapImage ProfilePicture
        {
            get
            {
                return LocalPlayer.ProfilePicture;      
            }
        }


        /// <summary>
        /// The ID of the user
        /// </summary>
        public String UserID
        {
            get
            {
                return _MicrosoftAccount.UserID;
            }
        }

        /// <summary>
        /// The name of the user
        /// </summary>
        public String Username
        {
            get
            {
                return _MicrosoftAccount.Username;
            }
        }

        /// <summary>
        /// The name of the user
        /// </summary>
        public String ScreenName
        {
            get
            {
                object value = _Settings.Get("ScreenName");
                if (!(value is string)) return _DefaultScreenName;
                return (string)value;
            }

            set
            {
                _Settings.Set("ScreenName", value);
                LocalPlayer.Name = value;
                NotifyPropertyChanged("ScreenName");
            }
        }
        
        /// <summary>
        /// The index of the image selected by the user for the background
        /// </summary>
        public int SelectedBackgroundIndex
        {             
            get
            {
                try
                {
                    object value = _Settings.Get("BackgroundImage");
                    if (!(value is string)) return 0;
                    string Index = (string)value;
                    return int.Parse(Index);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set
            {
                _Settings.Set("BackgroundImage", value.ToString());
            }
        }


        /// <summary>
        /// Placeholder class
        /// </summary>
        public class BackgroundImage
        {
            public BackgroundImage(String URI)
            {
                Image = URI;
            }

            public String Image { get; set; }
        }

        /// <summary>
        /// Description of the last error
        /// </summary>
        public string Error { get; set; }

        #endregion

        #region Commands

        RelayCommand m_cmdConnectMicrosoftAccount;
        /// <summary>
        /// Relay command for starting a new match
        /// </summary>
        public RelayCommand cmdConnectMicrosoftAccount
        {
            get
            {
                if (m_cmdConnectMicrosoftAccount == null)
                    m_cmdConnectMicrosoftAccount = new RelayCommand(param => _MicrosoftAccount.Connect());

                return m_cmdConnectMicrosoftAccount;
            }
        }

        RelayCommand m_cmdDisconnectMicrosoftAccount;
        /// <summary>
        /// Relay command for starting a new match
        /// </summary>
        public RelayCommand cmdDisconnectMicrosoftAccount
        {
            get
            {
                if (m_cmdDisconnectMicrosoftAccount == null)
                    m_cmdDisconnectMicrosoftAccount = new RelayCommand(param => _MicrosoftAccount.Disconnect());

                return m_cmdDisconnectMicrosoftAccount;
            }
        }
        
        #endregion
        
        #region Methods

        /// <summary>
        /// Save the settings to the device
        /// </summary>
        public void Save()
        {
            // _Settings.Save();
        }

        #endregion

        #region Event Error

        // A delegate type for hooking up change notifications.
        public delegate void ErrorEventHandler(object sender, ExtendedEventArgs e);

        /// <summary>
        /// The event client can use to be notified when the connection to the Microsoft account fails
        /// </summary>
        public event ErrorEventHandler Exception;

        // Invoke the ConnectionError event; 
        protected virtual void OnException(ExtendedEventArgs e)
        {
            if (Exception != null) Exception(this, e);
        }

        /// <summary>
        /// Propagate the error in the LocalPlayer instance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_Player_ConnectionError(object sender, ExtendedEventArgs e)
        {
            Error = e.Error;
            OnException(e);
        }

        /// <summary>
        /// Propagate the error in the MicrosoftAccount instance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _MicrosoftAccount_ConnectionError(object sender, ExtendedEventArgs e)
        {
            Error = e.Error;
            OnException(e);
        }

        #endregion

        #region EventHandlers 

        /// <summary>
        /// Event handler for change in connection status Microsoft Account
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MicrosoftAccount_ConnectionChanged(object sender)
        {
            //Load the profile picture
            LocalPlayer.ProfilePicture = null;
            LocalPlayer.ID = _MicrosoftAccount.UserID;

            //Update screen name
            if (ScreenName == _DefaultScreenName) ScreenName = Username;

            //Notify property changes
            NotifyPropertyChanged("ConnectedToMicrosoftAccount");
            NotifyPropertyChanged("ProfilePicture");
            NotifyPropertyChanged("UserID");
            NotifyPropertyChanged("UserName");
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
