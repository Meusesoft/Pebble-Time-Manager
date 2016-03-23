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
using Windows.Storage;
using P3bble.Messages;
using Pebble_Time_Manager.WatchItems;
using Pebble_Time_Manager.Common;

namespace Pebble_Time_Manager.Connector
{
    public class TimeLineSynchronizer : INotifyPropertyChanged
    {
        #region Constructor

        public TimeLineSynchronizer()
        {           
            _Weather = new Weather.Weather();
            _Calender = new Calender.Calender();
            _ConnectionToken = -1;
            _pc = PebbleConnector.GetInstance();
            
            BackgroundTaskStatus();

            Initialize();
        }

        #endregion

        #region Fields

        private Weather.Weather _Weather;
        private Calender.Calender _Calender;

        private bool _reminders;
        private bool _fahrenheit;
        private bool _backgroundtask;
        private int _ConnectionToken;

        private Connector.PebbleConnector _pc;
        private ObservableCollection<String> _log;

        #endregion

        #region Properties

        /// <summary>
        /// Log of actions during synchronization
        /// </summary>
        public ObservableCollection<String> Log
        {
            get
            {
                if (_log == null) _log = new ObservableCollection<string>();

                _Weather.Log = _log;
                _Calender.Log = _log;

                return _log;
            }
        }

        /// <summary>
        /// When true, reminders will be activated on the Pebble Time
        /// </summary>
        public bool Reminders
        {
            get
            {
                return _reminders;
                
            }
            set
            {
                _reminders = value;
                _Calender.Reminders = value;

                //save setting
                var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                if (roamingSettings.Values.Keys.Contains("reminders"))
                {
                    roamingSettings.Values["reminders"] = Reminders;
                }
                else
                {
                    roamingSettings.Values.Add("reminders", Reminders);
                }

                NotifyPropertyChanged("Reminders");
            }
        }

        /// <summary>
        /// When true, weather pins will have fahrenheit temperatures
        /// </summary>
        public bool Fahrenheit
        {
            get
            {
                return _fahrenheit;
            }
            set
            {
                _fahrenheit = value;
                _Weather.Fahrenheit = value;

                //save setting
                var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                if (roamingSettings.Values.Keys.Contains("fahrenheit"))
                {
                    roamingSettings.Values["fahrenheit"] = Fahrenheit;
                }
                else
                {
                    roamingSettings.Values.Add("fahrenheit", Fahrenheit);
                }

                NotifyPropertyChanged("Fahrenheit");
            }
        }

        private int _BackgroundTaskFrequency;
        public int BackgroundTaskFrequency
        {
            get
            {
                return _BackgroundTaskFrequency;
            }
            set
            {
                _BackgroundTaskFrequency = value;

                int Frequency = 0;

                switch (value)
                {
                    case 1: Frequency = 15; break;
                    case 2: Frequency = 30; break;
                    case 3: Frequency = 60; break;
                    default: Frequency = 0; break;
                }

                BackgroundTaskActivate(Frequency);
                
                //save setting
                var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                if (roamingSettings.Values.Keys.Contains("backgroundtaskfrequency"))
                {
                    roamingSettings.Values["backgroundtaskfrequency"] = BackgroundTaskFrequency;
                }
                else
                {
                    roamingSettings.Values.Add("backgroundtaskfrequency", BackgroundTaskFrequency);
                }

                NotifyPropertyChanged("BackgroundTaskFrequency");
            }
        }

        /// <summary>
        /// The last time the timeline has been synchronized
        /// </summary>
        public DateTime LastSynchronization
        {
            get
            {
                try
                {
                    var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                    if (roamingSettings.Values.Keys.Contains("lastsynchronization"))
                    {
                        String LastSynchronization = (string)roamingSettings.Values["lastsynchronization"];
                        return DateTime.Parse(LastSynchronization);
                    }
                }
                catch (Exception) {}

                return DateTime.MinValue;
            }

            set
            {
                var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                if (roamingSettings.Values.Keys.Contains("lastsynchronization"))
                {
                    roamingSettings.Values["lastsynchronization"] = value.ToString();
                }
                else
                {
                    roamingSettings.Values.Add("lastsynchronization", value.ToString());
                }

                NotifyPropertyChanged("LastSynchronization");
            }
        }

        /// <summary>
        /// When true, reminders will be activated on the Pebble Time
        /// </summary>
       /* public bool BackgroundTask
        {
            get
            {
                return _backgroundtask;
            }
            set
            {
                if (_backgroundtask != value)
                {
                    _backgroundtask = value;

                    BackgroundTaskActivate(value);

                    NotifyPropertyChanged("BackgroundTask");
                }
            }
        }*/

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the class
        /// </summary>
        private void Initialize()
        {
            //Read settings
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values.Keys.Contains("reminders"))
            {
                Reminders = (bool)roamingSettings.Values["reminders"];
            }
            if (roamingSettings.Values.Keys.Contains("fahrenheit"))
            {
                Fahrenheit = (bool)roamingSettings.Values["fahrenheit"];
            }
            else
            {
                Fahrenheit = !System.Globalization.RegionInfo.CurrentRegion.IsMetric;
            }
            if (roamingSettings.Values.Keys.Contains("backgroundtaskfrequency"))
            {
                _BackgroundTaskFrequency = (int)roamingSettings.Values["backgroundtaskfrequency"];
            }

            //Clear the log
            Log.Clear();
        }

        /// <summary>
        /// Synchronize the elements of the time line
        /// </summary>
        public async Task Synchronize()
        {
            bool ConnectionExists = true;

            try
            {
                //Connect
                if (_ConnectionToken == -1)
                {
                    ConnectionExists = false;
                    await Connect();
                }

                if (_ConnectionToken != -1)
                {
                    //Push weather timeline
                    _Weather.Fahrenheit = Fahrenheit;
                    await _Weather.Synchronize();

                    //Push calender events
                    await _Calender.Synchronize();

#if DEBUG
                    //notify synchronisztion
                    //await _pc.Pebble.SmsNotificationAsync("Synchronization", "Done");
#endif
                    LastSynchronization = DateTime.Now;

                    if (!ConnectionExists) Disconnect();
                }

            }
            catch (Exception e)
            {
                if (_pc.IsConnected)
                {
                    _pc.Pebble.SmsNotificationAsync("Synchronization", "Error: " + e.Message).Wait();

                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Reset the watch / timeline
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Clear()
        {
            try
            {
                await Connect();

                await _Weather.Clear();

                await _Calender.Clear();

                Disconnect();

                return true;

            }
            catch (Exception)
            {
                if (_pc.IsConnected) Disconnect();
            }

            return false;
        }

        enum UploadStatus { eNone, eResource, eBinary, eDone, eStop}
        private P3bble.Types.Bundle _bundle;
        private List<byte> FirstResourceID = new List<byte>(); 
        private List<byte> ResourceID = new List<byte>();
        private int Step = 1;
        int pointer = 0;
        private UploadStatus eUploadStatus = UploadStatus.eNone;
        byte Request;

        /// <summary>
        /// Message received
        /// </summary>
        /// <param name="_Message"></param>
        /*public async void MessageReceived (P3bble.P3bbleMessage _Message)
        {
            if (_Message.Endpoint == P3bble.Constants.Endpoint.AppManagerV3)
            {
               /*
                System.Diagnostics.Debug.WriteLine("<< Initiate send");

                eUploadStatus = UploadStatus.eResource;
                pointer = 0;
                Step = 1;
                */
           /* }

            if (_Message.Endpoint == P3bble.Constants.Endpoint.PutBytes && eUploadStatus == UploadStatus.eResource)
            {
                P3bble.Messages.PutBytesMessage _pmb = (P3bble.Messages.PutBytesMessage)_Message;

                P3bble.Messages.PutBytesMessageV3 _pmbV3 = new P3bble.Messages.PutBytesMessageV3();

                if (pointer < _bundle.Resources.Count())
                {


                    if (ResourceID.Count == 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            ResourceID.Add(_pmb.Result[i + 1]);
                        }
                        FirstResourceID.AddRange(ResourceID);
                    }

                    _pmbV3.Id.AddRange(ResourceID);

                    int j = 0;
                    int pointer_previous = pointer;

                    while (j < 2000 && pointer < _bundle.Resources.Count())
                    {
                        _pmbV3.Data.Add(_bundle.Resources[pointer]);
                        pointer++;
                        j++;
                    }

                    await _pc.Pebble._protocol.WriteMessage(_pmbV3);

                    System.Diagnostics.Debug.WriteLine(String.Format("<< Bytes send {0} - {1}", pointer_previous, pointer));
                }
                else
                {

                    switch (Step)
                    {
                        /*case 0:

                            System.Diagnostics.Debug.WriteLine("Step 0");

                            P3bble.Messages.PutBytesMessageV3 _pmbV32 = new P3bble.Messages.PutBytesMessageV3();
                            _pmbV32.Id.AddRange(ResourceID);                    
                    
                            String Message = "00:00:15:1d:03:0d:1a:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:0f:00:f8:01:00:3f:00:e0:07:00:fc:00:80:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:1f:00:f0:03:00:7e:00:c0:ff:03:f8:7f:00:ff:0f:e0:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:01:00:00:00:15:1d:03:0d:1a:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:0f:00:f8:01:00:3f:00:e0:07:00:fc:00:80:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:07:f0:ff:00:fe:1f:c0:ff:03:f8:7f:00:ff:0f:e0:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:01:00:00:00:15:1d:01:0d:19:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:0f:e0:ff:01:fc:3f:80:1f:00:f0:03:00:7e:00:c0:0f:00:f8:01:00:3f:00:e0:07:00:fc:00:80:1f:00:f0:03:00:7e:00:c0:0f:00:f8:01:00:3f:00:e0:07:00:fc:00:80:1f:00:f0:03:00:7e:00:c0:0f:00:f8:01:00:00:00:15:1d:03:0d:1a:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:0f:e0:ff:01:fc:3f:80:ff:07:f0:ff:00:fe:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:07:f0:ff:00:fe:1f:c0:ff:03:f8:7f:00:ff:0f:e0:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:01:00:00:00:15:1d:03:0d:1a:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:0f:e0:ff:01:fc:3f:80:ff:07:f0:ff:00:fe:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:1f:00:f0:03:00:7e:00:c0:0f:00:f8:01:00:3f:00:e0:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:01:00:00:00:1d:06:03:24:23:3f:f0:83:ff:07:7e:f0:ff:c0:0f:fe:1f:f8:c1:ff:03:3f:f8:7f:e0:07:3f:00:00";
                            String[] StringBytes = Message.Split(":".ToCharArray());
                            foreach (String Byte in StringBytes) _pmbV32.Data.Add(byte.Parse(Byte, System.Globalization.NumberStyles.HexNumber));

                            await _pc.Pebble._protocol.WriteMessage(_pmbV32);

                            Step = 1;

                            break;*/

                        /*case 1:

                            System.Diagnostics.Debug.WriteLine("Step 1");

                            List<Byte> Buffer = new List<byte>();
                            Buffer.AddRange(_bundle.Resources.ToList());

                            uint crc = P3bble.Helper.Util.Crc32(Buffer);
                            byte[] crcBytes = BitConverter.GetBytes(crc);
                            System.Diagnostics.Debug.WriteLine(string.Format("Sending CRC of {0:X}", crc));

                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(crcBytes);
                            }

                            String Message3 = "5e:9f:eb:cd";

                            P3bble.Messages.PutBytesMessageV3 _pmbV33 = new P3bble.Messages.PutBytesMessageV3();
                            _pmbV33.Id.AddRange(ResourceID);
                            _pmbV33.Status = 0x03;
                            _pmbV33.AddSize = false;
                            String[] StringBytes2 = Message3.Split(":".ToCharArray());

                            foreach (String Byte in StringBytes2) _pmbV33.Data.Add(byte.Parse(Byte, System.Globalization.NumberStyles.HexNumber));

                            await _pc.Pebble._protocol.WriteMessage(_pmbV33);

                            Step = 3;

                            break;

                        /*case 2:

                                System.Diagnostics.Debug.WriteLine("Step 2");
                            
                                P3bble.Messages.PutBytesMessageV3 _pmbV4 = new P3bble.Messages.PutBytesMessageV3();

                                _pmbV4.Status = 0x05;
                                _pmbV4.AddSize = false;

                                _pmbV4.Id.AddRange(ResourceID);


                                await _pc.Pebble._protocol.WriteMessage(_pmbV4);

                                Step = 3;

                                System.Diagnostics.Debug.WriteLine("<< Resource complete send");

                            break;*/

                        /*case 3:

                            System.Diagnostics.Debug.WriteLine("Step 3");
                            
                            eUploadStatus = UploadStatus.eBinary;
                            ResourceID.Clear();
                            System.Diagnostics.Debug.WriteLine("<< Resource upload done");
                            pointer = 0;
                            Step = 1;

                            byte[] RequestByte = new byte[1];
                            RequestByte[0] = Request;
                            String StringRequestByte = BitConverter.ToString(RequestByte);
                
                            WriteMessage("00:0a:be:ef:01:00:00:08:f4:85:00:00:00:" + StringRequestByte).Wait();

                            System.Diagnostics.Debug.WriteLine("<< Initiate send");

                            return;

                            break;

                    }
                }
            }

            if (_Message.Endpoint == P3bble.Constants.Endpoint.PutBytes && eUploadStatus == UploadStatus.eDone)
            {
                /*P3bble.Messages.PutBytesMessage _pmb = (P3bble.Messages.PutBytesMessage)_Message;

                P3bble.Messages.PutBytesMessageV3 _pmbV4 = new P3bble.Messages.PutBytesMessageV3();

                _pmbV4.Status = 0x05;
                _pmbV4.AddSize = false;

                if (Step == 1) _pmbV4.Id.AddRange(FirstResourceID);
                else _pmbV4.Id.AddRange(ResourceID);

                Step++;

                await _pc.Pebble._protocol.WriteMessage(_pmbV4);

                if (Step == 3)
                {
                    System.Diagnostics.Debug.WriteLine("<< Done send");

                    eUploadStatus = UploadStatus.eStop;
                }*/

            /*}


            if (_Message.Endpoint == P3bble.Constants.Endpoint.PutBytes && eUploadStatus == UploadStatus.eBinary)
            {
                P3bble.Messages.PutBytesMessage _pmb = (P3bble.Messages.PutBytesMessage)_Message;

                P3bble.Messages.PutBytesMessageV3 _pmbV3 = new P3bble.Messages.PutBytesMessageV3();

                if (pointer < _bundle.BinaryContent.Count())
                {
                    if (ResourceID.Count == 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            ResourceID.Add(_pmb.Result[i + 1]);
                        }
                    }

                    _pmbV3.Id.AddRange(ResourceID);

                    int j = 0;
                    int pointer_previous = pointer;

                    while (j < 2000 && pointer < _bundle.BinaryContent.Count())
                    {
                        _pmbV3.Data.Add(_bundle.BinaryContent[pointer]);
                        pointer++;
                        j++;
                    }

                    await _pc.Pebble._protocol.WriteMessage(_pmbV3);

                    System.Diagnostics.Debug.WriteLine(String.Format("<< Bytes send {0} - {1}", pointer_previous, pointer));
                }
                else
                {
                    switch (Step)
                    {
                        case 1:

                            System.Diagnostics.Debug.WriteLine("Step 1");

                            List<Byte> Buffer = new List<byte>();
                            Buffer.AddRange(_bundle.BinaryContent.ToList());

                            uint crc = P3bble.Helper.Util.Crc32(Buffer);
                            byte[] crcBytes = BitConverter.GetBytes(crc);
                            System.Diagnostics.Debug.WriteLine(string.Format("Sending CRC of {0:X}", crc));

                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(crcBytes);
                            }

                            String Message3 = "a9:d7:72:9e";

                            P3bble.Messages.PutBytesMessageV3 _pmbV33 = new P3bble.Messages.PutBytesMessageV3();
                            _pmbV33.Id.AddRange(ResourceID);
                            _pmbV33.Status = 0x03;
                            _pmbV33.AddSize = false;
                            String[] StringBytes2 = Message3.Split(":".ToCharArray());

                            foreach (String Byte in StringBytes2) _pmbV33.Data.Add(byte.Parse(Byte, System.Globalization.NumberStyles.HexNumber));

                            await _pc.Pebble._protocol.WriteMessage(_pmbV33);

                            Step = 2;
                        
                        break;

                        case 2:

                            System.Diagnostics.Debug.WriteLine("Step 2");

                            P3bble.Messages.PutBytesMessageV3 _pmbV4 = new P3bble.Messages.PutBytesMessageV3();

                            _pmbV4.Status = 0x05;
                            _pmbV4.AddSize = false;

                            _pmbV4.Id.AddRange(FirstResourceID);


                            await _pc.Pebble._protocol.WriteMessage(_pmbV4);

                            Step = 3;

                            System.Diagnostics.Debug.WriteLine("<< Resource complete send");

                        break;
                        
                        case 3:

                                System.Diagnostics.Debug.WriteLine("Step 3");
                            
                                P3bble.Messages.PutBytesMessageV3 _pmbV5 = new P3bble.Messages.PutBytesMessageV3();

                                _pmbV5.Status = 0x05;
                                _pmbV5.AddSize = false;

                                _pmbV5.Id.AddRange(ResourceID);


                                await _pc.Pebble._protocol.WriteMessage(_pmbV5);

                                Step = 4;

                                System.Diagnostics.Debug.WriteLine("<< Binary complete send");
                        break;

                        case 4:

                            System.Diagnostics.Debug.WriteLine("Step 4");
                                
                            eUploadStatus = UploadStatus.eDone;
                            ResourceID.Clear();
                            System.Diagnostics.Debug.WriteLine("<< Binary upload done");

                        break;


                    }
                }
            }
        }*/


        void Pebble_ItemSend(object sender, EventArgs e)
        {
            Disconnect();
            
            _pc.Pebble.ItemSend -= Pebble_ItemSend;

            Log.Add("Disconnected");
        }

        /// <summary>
        /// Activate or deactive the background task
        /// </summary>
        /// <param name="Frequency">0 = disabled, 1 = 15 minutes, 2=30 minutes, 3=60 minutes</param>
        /// <returns>true if activated</returns>
        private async Task<bool> BackgroundTaskActivate(int Frequency)
        {
            try
            {
                BackgroundAccessStatus _status = await BackgroundExecutionManager.RequestAccessAsync();

                if (_status == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
                {
                    var PebbleTimeTaskName = "Pebble Time Background Synchronization";

                    //Remove the current registration
                    foreach (var task in BackgroundTaskRegistration.AllTasks)
                    {
                        if (task.Value.Name == PebbleTimeTaskName)
                        {
                            task.Value.Unregister(true);
                        }
                    }

                    if (Frequency == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("BackgroundTaskStatus: Deactivated");

                        return false;
                    }

                    //Add a new registration
                    var builder = new BackgroundTaskBuilder();

                    TimeTrigger _timeTrigger = new TimeTrigger((uint)Frequency, false);

                    builder.Name = PebbleTimeTaskName;
                    //builder.TaskEntryPoint = "PebbleTimePhoneBackground.PebbleTimeBackgroundTask";
                    //builder.TaskEntryPoint = "BackgroundTaskPhone.PebbleTimeBackgroundTask";
                    builder.TaskEntryPoint = "BackgroundTasks.BackgroundSynchronizer";

                    builder.SetTrigger(_timeTrigger);

                    BackgroundTaskRegistration _task = builder.Register();

                    System.Diagnostics.Debug.WriteLine(String.Format("BackgroundTaskStatus: Activated ({0} minutes)", Frequency));

                    return true;
                }
            }
            catch (Exception e) 
            {
                System.Diagnostics.Debug.WriteLine("ActivateBackgroundTask: " + e.Message);
            }

            return false;
        }

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
                    var PebbleTimeTaskName = "Pebble Time Background Task";

                    //Check the current registration
                    foreach (var task in BackgroundTaskRegistration.AllTasks)
                    {
                        if (task.Value.Name == PebbleTimeTaskName)
                        {
                            _backgroundtask = true;
                            return;
                        }
                    }

                    _backgroundtask = false;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BackgroundTaskStatus: " + e.Message);
            }

            _backgroundtask = false;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Connect to a pebble
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            bool bNewConnection = false;

            //clear log
            Log.Clear();

            bNewConnection = !_pc.IsConnected;

            //Connect to the watch
            _ConnectionToken = await _pc.Connect(_ConnectionToken);

            if (!_pc.IsConnected)
            {
                Log.Add("No connection with Pebble Time.");
                Log.Add("Already connected or not paired?.");
                throw new Exception("No connection with Pebble Time");
            }

            if (bNewConnection) Log.Add("Connected");
        }

        /// <summary>
        /// Disconnect from the Pebble
        /// </summary>
        private void Disconnect()
        {
            //Disconnect
            _pc.Disconnect(_ConnectionToken);

            _ConnectionToken = -1;

            if (!_pc.IsConnected) Log.Add("Disconnected");
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
