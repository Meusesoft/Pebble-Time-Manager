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
using Tennis_Statistics.ViewModels;

namespace Tennis_Statistics.Helpers
{
    public class MicrosoftAccount
    {
        #region Properties

        /// <summary>
        /// The status of the connection
        /// </summary>
        public bool Connected
        {
            get
            {
                object value = _Settings.Get("ConnectedToMicrosoftAccount");
                if (!(value is bool)) return false;
                return (bool)value;
            }

            set
            {
                _Settings.Set("ConnectedToMicrosoftAccount", value);
            }
        }

        /// <summary>
        /// The ID of the user
        /// </summary>
        public String UserID
        {
            get
            {
                object value = _Settings.Get("UserID");
                if (!(value is string)) return "";
                return (string)value;
            }

            set
            {
                _Settings.Set("UserID", value);

            }
        }

        /// <summary>
        /// The name of the user
        /// </summary>
        public String Username
        {
            get
            {
                object value = _Settings.Get("Username");
                if (!(value is string)) return "";
                return (string)value;
            }

            set
            {
                _Settings.Set("Username", value);
            }
        }

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


        #endregion

        #region Methods

        /// <summary>
        /// Connect the tennis statistics profile to the user its Microsoft Account
        /// </summary>
        public async void Connect()
        {
            try
            {
                var authClient = new LiveAuthClient();
                LiveLoginResult result = await authClient.LoginAsync(new string[] { "wl.signin" });

                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    //Create connection
                    var connectClient = new LiveConnectClient(result.Session);

                    //Get the user data
                    var meResult = await connectClient.GetAsync("me");
                    IDictionary<String, Object> meData = meResult.Result;
                    UserID = meData["id"].ToString();
                    Username = meData["name"].ToString();

                    //Connection succeeded
                    Connected = true;
                    OnConnectionChanged();                   
                }
            }
            catch (Exception e)
            {
                ExtendedEventArgs eaa = new ExtendedEventArgs();
                eaa.Error = "An error occurred while connecting to your Microsoft account. Please try again later.";
                OnConnectionError(eaa);
            }
        }

        /// <summary>
        /// Disconnect the profile from the Microsoft Account
        /// </summary>
        public void Disconnect()
        {
            Connected = false;
            OnConnectionChanged();
        }

        #endregion

        #region Event ConnectionError

        // A delegate type for hooking up change notifications.
        public delegate void ConnectionErrorEventHandler(object sender, ExtendedEventArgs e);

        /// <summary>
        /// The event client can use to be notified when the connection to the Microsoft account fails
        /// </summary>
        public event ConnectionErrorEventHandler ConnectionError;

        // Invoke the ConnectionError event; 
        protected virtual void OnConnectionError(ExtendedEventArgs e)
        {
            if (ConnectionError != null)
                ConnectionError(this, e);
        }

        #endregion

        #region Event ConnectionChanged

        // A delegate type for hooking up change notifications.
        public delegate void ConnectionChangedEventHandler(object sender);

        /// <summary>
        /// The event client can use to be notified when the connection to the Microsoft account fails
        /// </summary>
        public event ConnectionChangedEventHandler ConnectionChanged;

        // Invoke the ConnectionError event; 
        protected virtual void OnConnectionChanged()
        {
            if (ConnectionChanged != null) ConnectionChanged(this);
        }

        #endregion

        #region Static elements

        private static MicrosoftAccount _MicrosoftAccountInstance;

        /// <summary>
        /// Returns the global instance of the Settings class
        /// </summary>
        /// <returns></returns>
        public static MicrosoftAccount GetInstance()
        {
            if (_MicrosoftAccountInstance == null) _MicrosoftAccountInstance = new MicrosoftAccount();

            return _MicrosoftAccountInstance;
        }

        #endregion

    }
}
