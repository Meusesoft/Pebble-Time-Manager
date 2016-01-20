using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.ApplicationModel.Background;
using Windows.Phone.Notification.Management;
using Windows.ApplicationModel.Email;
using Pebble_Time_Manager.ViewModels;

namespace Pebble_Time_Manager.Connector
{
    public class NotificationsHandler : INotifyPropertyChanged
    {
        #region Constructors

        public NotificationsHandler()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.Keys.Contains("backgroundtasknotifications"))
            {
                _BackgroundTaskEnabled = (bool)localSettings.Values["backgroundtasknotifications"];
            }

            Apps = new vmApps();
            //Apps.LoadStoredItems();

            try
            {
                GetAppsFromPhone();

                _NotificationAlarm = GetBooleanProperty("NotificationAlarm", false);
                _NotificationEmail = GetBooleanProperty("NotificationEmail", true);
                _NotificationToast = GetBooleanProperty("NotificationToast", true);
                _NotificationReminder = GetBooleanProperty("NotificationReminder", false);
                _NotificationPhone = GetBooleanProperty("NotificationPhone", true);
                _NotificationBatterySaver = GetBooleanProperty("NotificationBatterySaver", true);
                _NotificationMedia = GetBooleanProperty("NotificationMedia", false);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("NotificationsHandler: " + e.Message);
            }

            _ConnectionToken = -1;
        }

        #endregion

        #region Fields

        private bool _BackgroundTaskEnabled;
        private Connector.PebbleConnector _pc;
        private bool _NotificationAlarm;
        private bool _NotificationEmail;
        private bool _NotificationToast;
        private bool _NotificationReminder;
        private bool _NotificationPhone;
        private bool _NotificationMedia;
        private bool _NotificationCortana; 
        private bool _NotificationBatterySaver;
        private int _ConnectionToken;

        #endregion

        #region Properties

        public vmApps Apps { get; set; }

        /// <summary>
        /// True if background task is enabled
        /// </summary>
        public bool BackgroundTaskEnabled
        {
            get
            {
                return _BackgroundTaskEnabled;
            }
            set
            {
                if (_BackgroundTaskEnabled != value)
                {
                    _BackgroundTaskEnabled = value;

                    BackgroundTaskActivate(_BackgroundTaskEnabled);

                    //save setting
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    if (localSettings.Values.Keys.Contains("backgroundtasknotifications"))
                    {
                        localSettings.Values["backgroundtasknotifications"] = BackgroundTaskEnabled;
                    }
                    else
                    {
                        localSettings.Values.Add("backgroundtasknotifications", BackgroundTaskEnabled);
                    }

                    NotifyPropertyChanged("BackgroundTaskEnabled");
                    NotifyPropertyChanged("NotificationAppsEnabled");
                }
            }
        }

        /// <summary>
        /// The last time notifications have been received
        /// </summary>
        public DateTime LastNotifications
        {
            get
            {
                try
                {
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    if (localSettings.Values.Keys.Contains("lastnotifications"))
                    {
                        String LastSynchronization = (string)localSettings.Values["lastnotifications"];
                        return DateTime.Parse(LastSynchronization);
                    }
                }
                catch (Exception) { }

                return DateTime.MinValue;
            }

            set
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.Keys.Contains("lastnotifications"))
                {
                    localSettings.Values["lastnotifications"] = value.ToString();
                }
                else
                {
                    localSettings.Values.Add("lastnotifications", value.ToString());
                }

                NotifyPropertyChanged("LastNotifications");
            }
        }

        /// <summary>
        /// True if notifications has been purchased
        /// </summary>
        public bool NotificationsPurchased
        {
            get
            {              
                return Pebble_Time_Manager.Helper.Purchases.getReference().Available("pebble_notifications"); 
            }
            set
            {
                NotifyPropertyChanged("NotificationsPurchased");
            }
        }

        /// <summary>
        /// Notification alarm on/off
        /// </summary>
        public bool NotificationAlarm
        {
            get
            {
                return _NotificationAlarm;
            }
            set
            {
                _NotificationAlarm = value;
                SetNotificationTypes();
                SetBooleanProperty("NotificationAlarm", value);
            }
        }

        /// <summary>
        /// Notification Cortana
        /// </summary>
        public bool NotificationCortana
        {
            get
            {
                return _NotificationCortana;
            }
            set
            {
                _NotificationCortana = value;
                SetNotificationTypes();
                SetBooleanProperty("NotificationCortana", value);
            }
        }

        /// <summary>
        /// Notification battery saver on/off
        /// </summary>
        public bool NotificationBatterySaver
        {
            get
            {
                return _NotificationBatterySaver;
            }
            set
            {
                _NotificationBatterySaver = value;
                SetNotificationTypes();
                SetBooleanProperty("NotificationBatterySaver", value);
            }
        }

        /// <summary>
        /// Notification e-mail on/off
        /// </summary>
        public bool NotificationEmail
        {
            get
            {
                return _NotificationEmail;
            }
            set
            {
                _NotificationEmail = value;
                SetNotificationTypes();
                SetBooleanProperty("NotificationEmail", value);
            }
        }

        /// <summary>
        /// Notification phone on/off
        /// </summary>
        public bool NotificationPhone
        {
            get
            {
                return _NotificationPhone;
            }
            set
            {
                _NotificationPhone = value;
                SetNotificationTypes();
                SetBooleanProperty("NotificationPhone", value);
            }
        }

        /// <summary>
        /// Notification toast on/off
        /// </summary>
        public bool NotificationToast
        {
            get
            {
                return _NotificationToast;
            }
            set
            {
                _NotificationToast = value;
                SetNotificationTypes();
                SetBooleanProperty("NotificationToast", value);
                NotifyPropertyChanged("NotificationAppsEnabled");
            }
        }

        /// <summary>
        /// Notification reminder on/off
        /// </summary>
        public bool NotificationReminder
        {
            get
            {
                return _NotificationReminder;
            }
            set
            {
                _NotificationReminder = value;
                SetNotificationTypes();
                SetBooleanProperty("NotificationReminder", value);
            }
        }

        /// <summary>
        /// Notification reminder on/off
        /// </summary>
        public bool NotificationMedia
        {
            get
            {
                return _NotificationMedia;
            }
            set
            {
                _NotificationMedia = value;
                SetNotificationTypes();
                SetBooleanProperty("NotificationMedia", value);
            }
        }

        /// <summary>
        /// Notification reminder on/off
        /// </summary>
        public bool NotificationAppsEnabled
        {
            get
            {
                return _NotificationToast && _BackgroundTaskEnabled;
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Process the notification trigger
        /// </summary>
        /// <param name="nextTriggerDetails"></param>
        /// <returns></returns>
        public async Task ProcessNotifications(IAccessoryNotificationTriggerDetails nextTriggerDetails)
        {
            System.Diagnostics.Debug.WriteLine(nextTriggerDetails.AppDisplayName);
            byte[] Cookie = new byte[4];

            bool KeepConnectionAlive = false;
            bool Talking = false;

            await Connect();

            while (nextTriggerDetails != null || KeepConnectionAlive)
            {
                if (nextTriggerDetails != null)
                {
                    try
                    {
                        AccessoryNotificationType accessoryNotificationType = nextTriggerDetails.AccessoryNotificationType;

                        System.Diagnostics.Debug.WriteLine("Notification: " + accessoryNotificationType.ToString());

                        switch (accessoryNotificationType)
                        {
                            case AccessoryNotificationType.Email:

                                await SendEmailMessage(nextTriggerDetails);

                                break;

                            case AccessoryNotificationType.Toast:

                                await ProcessToastMessage(nextTriggerDetails);
                                break;

                            case AccessoryNotificationType.BatterySaver:

                                await SendBatterySaverMessage(nextTriggerDetails);
                                break;

                            case AccessoryNotificationType.Alarm:

                                await SendAlarmMessage((AlarmNotificationTriggerDetails)nextTriggerDetails);
                                break;

                            case AccessoryNotificationType.CortanaTile:

                                await SendCortanaMessage((CortanaTileNotificationTriggerDetails)nextTriggerDetails);
                                break;

                            case AccessoryNotificationType.Phone:

                                PhoneNotificationTriggerDetails entd = (PhoneNotificationTriggerDetails)nextTriggerDetails;

                                if (entd.PhoneNotificationType != PhoneNotificationType.LineChanged)
                                {

                                    System.Diagnostics.Debug.WriteLine("Phone - " + entd.CallDetails.State.ToString());

                                    switch (entd.CallDetails.State)
                                    {
                                        case PhoneCallState.Ringing:

                                            Cookie = BitConverter.GetBytes(_pc.GetNextMessageIdentifier());

                                            await _pc.Pebble.PhoneCallAsync(entd.CallDetails.ContactName, entd.CallDetails.PhoneNumber, Cookie);

                                            KeepConnectionAlive = true;
                                            // _pc.Pebble._protocol.StartRun();

                                            break;

                                        case PhoneCallState.Ended:

                                            await _pc.Pebble.EndCallAsync(Cookie);

                                            KeepConnectionAlive = false;

                                            break;

                                        case PhoneCallState.Talking:

                                            if (!Talking)
                                            {
                                                await _pc.Pebble.StartCallAsync(Cookie);

                                                Talking = true;
                                            }

                                            break;

                                        default:

                                            KeepConnectionAlive = false;

                                            break;
                                    }
                                }

                                break;

                            default:

                                break;
                                }

                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("ProcessNotifications Exception: " + e.Message);
                    }

                    /*if (KeepConnectionAlive)
                    {
                        Task.Delay(500).Wait();

                        P3bble.P3bbleMessage msg = await _pc.Pebble._protocol.ReceiveMessage(0);

                        if (msg != null)
                        {
                            String abc = msg.Endpoint.ToString();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ReceiveMessage: none");
                        }
                    }*/

                    AccessoryManager.ProcessTriggerDetails(nextTriggerDetails);
                }

                nextTriggerDetails = AccessoryManager.GetNextTriggerDetails();
            }

            if (!KeepConnectionAlive) Disconnect();

            LastNotifications = DateTime.Now;
        }

        /// <summary>
        /// Send an e-mail message to the Pebble
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal async Task SendEmailMessage(IAccessoryNotificationTriggerDetails obj)
        {
            try
            {
                EmailNotificationTriggerDetails _entd = obj as EmailNotificationTriggerDetails;

                String From = _entd.SenderName;
                String Subject = _entd.EmailMessage.Subject;
                String Body = _entd.EmailMessage.Body;

                if (From.Length > 255) From = From.Substring(0, 255);
                if (Subject.Length > 255) Subject = Subject.Substring(0, 255);
                if (Body.Length > 2048) Body = Body.Substring(0, 2048);

                String Details = Subject + Environment.NewLine + Environment.NewLine + Body;

                //Send Message
                await SendToastMessageToPebble(From, Details, P3bble.Constants.Icons.mail);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Process the toast message
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal async Task ProcessToastMessage(IAccessoryNotificationTriggerDetails obj)
        {
            ToastNotificationTriggerDetails _tntd = obj as ToastNotificationTriggerDetails;

            try
            {
                switch (_tntd.AppDisplayName.ToLower())
                {
                    case "messaging":
                    case "whatsapp":

                        await SendTextOrChatMessage(_tntd);
                        break;

                    case "twitter":

                        await SendToastMessage(_tntd, P3bble.Constants.Icons.twitter);
                        break;
                    
                    case "facebook":

                        await SendToastMessage(_tntd, P3bble.Constants.Icons.facebook);
                        break;

                    case "skype":

                        await SendToastMessage(_tntd, P3bble.Constants.Icons.skype);
                        break;
                    
                    default:

                        await SendToastMessage(_tntd, P3bble.Constants.Icons.bell);
                        break;
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Send a text or chat message to the pebble. The first text line is the person who send the message.
        /// </summary>
        /// <param name="_tntd"></param>
        /// <returns></returns>
        internal async Task SendTextOrChatMessage(ToastNotificationTriggerDetails _tntd)
        {
            try
            {
                String Header = _tntd.AppDisplayName;
                String Details = _tntd.Text1 + ":" + Environment.NewLine + _tntd.Text2 + Environment.NewLine + _tntd.Text3 + Environment.NewLine + _tntd.Text4;

                await SendToastMessageToPebble(Header, Details, P3bble.Constants.Icons.text_balloon);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Send an alarm
        /// </summary>
        /// <param name="_tntd"></param>
        /// <returns></returns>
        internal async Task SendAlarmMessage(AlarmNotificationTriggerDetails _tntd)
        {
            try
            {
                String Header = _tntd.AppDisplayName;
                String Details = String.Format("{0}, {1}{2}{3}",
                    _tntd.Title,
                    _tntd.ReminderState.ToString(),
                    Environment.NewLine,
                    _tntd.Timestamp);

                await SendToastMessageToPebble(Header, Details, P3bble.Constants.Icons.bell);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Send an alarm
        /// </summary>
        /// <param name="_tntd"></param>
        /// <returns></returns>
        internal async Task SendCortanaMessage(CortanaTileNotificationTriggerDetails _ctntd)
        {
            try
            {
                String Header = _ctntd.AppDisplayName;

                string content = _ctntd.Content;
                string emphasizedText = _ctntd.EmphasizedText;
                string largeContent1 = _ctntd.LargeContent1;
                string largeContent2 = _ctntd.LargeContent2;
                string str8 = (largeContent1 != "" ? largeContent1 : largeContent2);
                string nonWrappedSmallContent1 = _ctntd.NonWrappedSmallContent1;
                string nonWrappedSmallContent2 = _ctntd.NonWrappedSmallContent2;
                string nonWrappedSmallContent3 = _ctntd.NonWrappedSmallContent3;
                string nonWrappedSmallContent4 = _ctntd.NonWrappedSmallContent4;
                string[] strArray = new String[] { nonWrappedSmallContent1, " ", nonWrappedSmallContent2, " ", nonWrappedSmallContent3, " ", nonWrappedSmallContent4 };
                string str9 = String.Concat(strArray);
                string source = _ctntd.Source;
                string[] strArray1 = new String[] { content, " ", emphasizedText, str8, str9, source };
                String Details = String.Concat(strArray1);

                await SendToastMessageToPebble(Header, Details, P3bble.Constants.Icons.text_balloon);
            }
            catch (Exception) { }
        }
        
        /// <summary>
        /// Send the battery saver state
        /// </summary>
        /// <param name="_tntd"></param>
        /// <returns></returns>
        internal async Task SendBatterySaverMessage(IAccessoryNotificationTriggerDetails _tntd)
        {
            try
            {
                String Header = "Battery saver";
                String Details = AccessoryManager.BatterySaverState ? "Enabled" : "Disabled";

                await SendToastMessageToPebble(Header, Details, P3bble.Constants.Icons.lightning_bolt);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Process the general toast message
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal async Task SendToastMessage(ToastNotificationTriggerDetails _tntd, P3bble.Constants.Icons Icon)
        {
            try
            {
                String Header = _tntd.AppDisplayName;
                String Details = "";
                if (_tntd.Text1.Length > 0) Details += _tntd.Text1 + Environment.NewLine;
                if (_tntd.Text2.Length > 0) Details += _tntd.Text2 + Environment.NewLine;
                if (_tntd.Text3.Length > 0) Details += _tntd.Text3 + Environment.NewLine;
                if (_tntd.Text4.Length > 0) Details += _tntd.Text4;

                await SendToastMessageToPebble(Header, Details, Icon);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Send the toast message to the Pebble
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="Details"></param>
        /// <param name="Icon"></param>
        /// <returns></returns>
        internal async Task SendToastMessageToPebble(String Header, String Details, P3bble.Constants.Icons Icon)
        {
            try
            {
                //Send Message
                P3bble.Messages.NotificationMessage _nm = new P3bble.Messages.NotificationMessage(_pc.GetNextMessageIdentifier(),
                        Guid.NewGuid(),
                        DateTime.Now,
                        Header,
                        Details,
                        Icon);

                await _pc.Pebble.WriteMessageAndReceiveAcknowledgementAsync(_nm);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Activate or deactive the background task for notification handling
        /// </summary>
        /// <param name="Activate"></param>
        /// <returns>true if activated</returns>
        private async Task<bool> BackgroundTaskActivate(bool Activate)
        {
            try
            {
                BackgroundAccessStatus _status = await BackgroundExecutionManager.RequestAccessAsync();

                if (_status == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
                {
                    var PebbleTimeTaskName = "Pebble Time Background Notifications";

                    //Remove the current registration
                    foreach (var task in BackgroundTaskRegistration.AllTasks)
                    {
                        if (task.Value.Name == PebbleTimeTaskName)
                        {
                            task.Value.Unregister(true);
                        }
                    }

                    if (!Activate)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("BackgroundTask Notifications Status: Deactivated"));

                        return true;
                    }

                    String Result = AccessoryManager.RegisterAccessoryApp();

                    if (await GetAppsFromPhone())
                    {

                        foreach (var item in Apps)
                        {
                            AccessoryManager.EnableNotificationsForApplication(item.ID);
                        }

                        var backgroundTaskBuilder = new BackgroundTaskBuilder();

                        backgroundTaskBuilder.Name = PebbleTimeTaskName;
                        backgroundTaskBuilder.TaskEntryPoint = "BackgroundTasks.BackgroundNotifications";

                        DeviceManufacturerNotificationTrigger deviceManufacturerNotificationTrigger = new DeviceManufacturerNotificationTrigger(String.Concat("Microsoft.AccessoryManagement.Notification:", Result), false);
                        backgroundTaskBuilder.SetTrigger(deviceManufacturerNotificationTrigger);
                        BackgroundTaskRegistration backgroundTaskRegistration1 = backgroundTaskBuilder.Register();

                        /*WindowsRuntimeMarshal.AddEventHandler<BackgroundTaskCompletedEventHandler>(new Func<BackgroundTaskCompletedEventHandler,
                            EventRegistrationToken>(backgroundTaskRegistration1, BackgroundTaskRegistration.add_Completed),
                            new Action<EventRegistrationToken>(backgroundTaskRegistration1,
                                BackgroundTaskRegistration.remove_Completed),
                                new BackgroundTaskCompletedEventHandler(this.registration_Completed));*/

                        SetNotificationTypes();

                        // PhoneAppsList();

                        System.Diagnostics.Debug.WriteLine(String.Format("BackgroundTask Notifications Status: Activated"));

                        return true;
                    }
                    else
                    {
                        ExtendedEventArgs eaa = new ExtendedEventArgs();
                        eaa.Error = String.Format("Notifications access denied.");
                        OnFatalError(eaa);

                        _BackgroundTaskEnabled = false;
                        NotifyPropertyChanged("BackgroundTaskEnabled");
                        NotifyPropertyChanged("NotificationAppsEnabled");
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BackgroundTask Notifications Exception: " + e.Message);

                ExtendedEventArgs eaa = new ExtendedEventArgs();
                eaa.Error = String.Format("A fatal error occurred: {0}", e.Message);
                #if DEBUG
                eaa.Error = String.Format("A fatal error occurred. {1}:{0}", e.Message, e.Source);
                #endif
                OnFatalError(eaa);
            }

            return false;
        }

        /// <summary>
        /// Enable the notification types
        /// </summary>
        private void SetNotificationTypes()
        {
            AccessoryManager.DisableAllAccessoryNotificationTypes();

            int NT = 0;

            if (NotificationAlarm) NT += (int)AccessoryNotificationType.Alarm;  
            if (NotificationEmail) NT += (int)AccessoryNotificationType.Email;  
            if (NotificationMedia) NT += (int)AccessoryNotificationType.Media;  
            if (NotificationPhone) NT += (int)AccessoryNotificationType.Phone;  
            if (NotificationBatterySaver) NT += (int)AccessoryNotificationType.BatterySaver;  
            if (NotificationToast) NT += (int)AccessoryNotificationType.Toast;              
            if (NotificationReminder) NT += (int)AccessoryNotificationType.Reminder;
            if (NotificationCortana) NT += (int)AccessoryNotificationType.CortanaTile;
  
            System.Diagnostics.Debug.WriteLine("SetNotificationTypes:" + NT);
                
            AccessoryManager.EnableAccessoryNotificationTypes(NT);
        }

        /// <summary>
        /// Fill the collection with phone apps
        /// </summary>
        public async Task<bool> GetAppsFromPhone()
        {
            try
            {
                IReadOnlyDictionary<String, AppNotificationInfo> apps = AccessoryManager.GetApps();

                List<vmApp> _temp = new List<vmApp>();

                foreach (String key in apps.Keys)
                {
                    String id = apps[key].Id;
                    String name = apps[key].Name;

                    vmApp _newApp = new vmApp() { ID = id, Name = name/*, AppIconStream = AccessoryManager.GetAppIcon(id)*/};

                    _temp.Add(_newApp);  

                   // _newApp.PopulateImageData();
                }

                var _sortedapps = from app in _temp orderby app.Selected descending, app.Name select app;

                foreach(vmApp item in _sortedapps)
                {
                    Apps.Add(item);
                }

                // await Apps.SaveItems();

                return true;
            }
            catch (Exception) { }

            return false;    
        }


        /*private void PhoneAppsList()
        {
            IReadOnlyDictionary<String, AppNotificationInfo> apps = AccessoryManager.GetApps();
            foreach (String key in apps.Keys)
            {
                String id = apps[key].Id;
                try
                {
                    AccessoryManager.EnableNotificationsForApplication(id);
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                }
            }
        }*/


        /// <summary>
        /// Retrieve the status of the background synchronization task
        /// </summary>
        /// <returns>Sets the property BackgroundTask</returns>
        private async void BackgroundTaskStatus()
        {
            try
            {
                BackgroundAccessStatus _status = await BackgroundExecutionManager.RequestAccessAsync();

                if (_status == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
                {
                    var PebbleTimeTaskName = "Pebble Time Background Notifications";

                    //Check the current registration
                    foreach (var task in BackgroundTaskRegistration.AllTasks)
                    {
                        if (task.Value.Name == PebbleTimeTaskName)
                        {
                            _BackgroundTaskEnabled = true;
                            return;
                        }
                    }

                    _BackgroundTaskEnabled = false;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BackgroundTaskStatus: " + e.Message);
            }

            _BackgroundTaskEnabled = false;
        }

        /// <summary>
        /// Set the property value, save it to the settings and notify property changed
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        private void SetBooleanProperty(String Name, bool value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.Keys.Contains(Name))
            {
                localSettings.Values[Name] = value.ToString();
            }
            else
            {
                localSettings.Values.Add(Name, value.ToString());
            }

            NotifyPropertyChanged(Name);
        }

        /// <summary>
        /// Set the property value, save it to the settings and notify property changed
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        private bool GetBooleanProperty(String Name, bool Default)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.Keys.Contains(Name))
                {
                    return bool.Parse(localSettings.Values[Name].ToString());
                }
                else
                {
                    return Default;
                }
            }
            catch (Exception)
            {
                return Default;
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

        #region Helper methods

        /// <summary>
        /// Connect to a pebble
        /// </summary>
        /// <returns></returns>
        private async Task Connect()
        {
            if (_ConnectionToken !=-1 )
            {
                throw new Exception("Notification connection still active.");
            }

            //Connect to the watch
            _pc = Connector.PebbleConnector.GetInstance();

            _ConnectionToken = await _pc.Connect(_ConnectionToken);

            if (!_pc.IsConnected)
            {
                throw new Exception("No connection with Pebble Time");
            }
        }

        /// <summary>
        /// Disconnect from the Pebble
        /// </summary>
        private void Disconnect()
        {
            _pc.Disconnect(_ConnectionToken);

            _ConnectionToken = -1;
        }

        /// <summary>
        /// Write a message direct to the Pebble. Message is described as a string (00:01:02 etc)
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        private async Task WriteMessage(String Message)
        {
            String[] StringBytes = Message.Split(":".ToCharArray());
            Byte[] Bytes = new Byte[StringBytes.Count()];
            int index = 0;

            foreach (String Byte in StringBytes)
            {
                Bytes[index] = byte.Parse(Byte, System.Globalization.NumberStyles.HexNumber);
                index++;
            }

            _pc.Pebble.Writer().WriteBytes(Bytes);
            await _pc.Pebble.Writer().StoreAsync().AsTask();
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
}
