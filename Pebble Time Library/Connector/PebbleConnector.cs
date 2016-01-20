using P3bble.Messages;
using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.WatchItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Streams;

namespace Pebble_Time_Manager.Connector
{
    public class PebbleConnector
    {
        #region Constructor

        public PebbleConnector()
        {
            _connections = new List<int>();
            _ConnectionToken = -1;
        }

        #endregion

        #region Fields

        private static PebbleConnector _PebbleConnector;
        private DataReader _reader;
        private DataWriter _writer;
        private StreamSocket _socket;
        private P3bble.P3bble _pebble;
        private Pebble_Time_Manager.WatchItems.WatchItems _WatchItems;
        private List<int> _connections;

        #endregion

        #region Properties

        /// <summary>
        /// Reference to the internal P3bble instance
        /// </summary>
        public P3bble.P3bble Pebble
        {
            get
            {
                return _pebble;
            }
        }

        /// <summary>
        /// Reference to the internal P3bble instance
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_pebble == null) return false;
                return _pebble.IsConnected;
            }
        }

        /// <summary>
        /// List of watch items
        /// </summary>
        public Pebble_Time_Manager.WatchItems.WatchItems WatchItems
        {
            get
            {
                if (_WatchItems == null)
                {
                    _WatchItems = new Pebble_Time_Manager.WatchItems.WatchItems();
                    _WatchItems.Load();
                }

                return _WatchItems;
            }
        }

        /// <summary>
        /// The last error message
        /// </summary>
        public string LastError { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Connect to the pebble time watch
        /// </summary>
        /// <returns></returns>
        public async Task<int> Connect(int Token)
        {
            LastError = "";
            if (Token != -1 && _connections.Contains(Token) && IsConnected) return Token;

            int newToken = CreateConnectionToken();

            try
            {
                if (!IsConnected)
                {
                    //await RegisterBackgroundBluetoothTask();

                    //return false;
                    P3bble.P3bble.IsMusicControlEnabled = true;
#if DEBUG
                    P3bble.P3bble.IsLoggingEnabled = true;
#else
                    P3bble.P3bble.IsLoggingEnabled = false;
#endif
                    List<P3bble.P3bble> pebbles = await P3bble.P3bble.DetectPebbles();

                    if (pebbles.Count >= 1)
                    {
                        _pebble = pebbles[0];
                        _pebble.WatchItems = this.WatchItems;
                        bool Result = await _pebble.ConnectAsync();

                        if (Result)
                        {
                            return newToken;
                        }

                        LastError = "Connect to Pebble Time failed; is it already connected?";
                        // _pebble._protocol.StartRun();
                    }
                    else
                    {
                        //No Pebble found
                        LastError = "No Pebble Time detected; is one paired?";
                    }
                }
                else
                {
                    return newToken;
                }

            }
            catch (Exception)
            {

            }

            ReleaseConnectionToken(newToken);

            return -1;
        }

        /// <summary>
        /// Start processing received messages
        /// </summary>
        /// <returns></returns>
        public bool StartReceivingMessages()
        {
            if (IsConnected && _pebble._protocol!=null)
            {
                _pebble._protocol.StartRun();
                _pebble._protocol.disconnectEventHandler += _protocol_disconnectEventHandler;
                return true;
            }

            return false;
        }

        private void _protocol_disconnectEventHandler(object sender, EventArgs e)
        {
            OnDisconnect(e);
        }

        #region Events

        //Event handler for item list change
        public delegate void DisconnectEventHandler(object sender, EventArgs e);

        public event DisconnectEventHandler disconnectEventHandler;

        protected virtual void OnDisconnect(EventArgs e)
        {
            if (disconnectEventHandler != null) disconnectEventHandler(this, e);
        }

        #endregion

        /// <summary>
        /// Stop processing received message
        /// </summary>
        /// <returns></returns>
        public bool StopReceivingMessages()
        {
            if (IsConnected && _pebble._protocol != null)
            {
                _pebble._protocol.StopRun();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Create a connection token
        /// </summary>
        /// <returns></returns>
        private int CreateConnectionToken()
        {
            int newToken = 1;

            if (_connections.Count > 0)
            {
                int _max = _connections.Max();
                return _max + 1;
            }

            _connections.Add(newToken);

            return newToken;
        }

        /// <summary>
        /// Release the connection token
        /// </summary>
        /// <param name="Token"></param>
        private void ReleaseConnectionToken(int Token)
        {
            _connections.Remove(Token);
        }

        /// <summary>
        /// Delete a watch item and send it to the Pebble (if connected)
        /// </summary>
        /// <param name="_deleteItem"></param>
        /// <returns></returns>
        public async Task<bool> DeleteWatchItemAsync(WatchItem _deleteItem)
        {
            try
            {
                //Remove from app
                await WatchItems.DeleteWatchItem(_deleteItem);

                //Remove from storage
                await LocalStorage.Delete(_deleteItem.File);

                //Remove from watch
                if (IsConnected)
                {
                    await Pebble.DeleteWatchItemAsync(_deleteItem);
                }

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("DeleteWatchItemAsync exception: " + e.Message);
            }

            return false;
        }

        /// <summary>
        /// Disconnect the connection with the pebble time watch
        /// </summary>
        /// <returns></returns>
        public bool Disconnect(int Token)
        {
            ReleaseConnectionToken(Token);

            if (_connections.Count == 0)
            {
                try
                {
                    if (_pebble != null) _pebble.Disconnect();
                    if (this._writer != null) this._writer.FlushAsync().AsTask().Wait();
                    if (this._socket != null) this._socket.Dispose();
                    if (this._writer != null) this._writer.Dispose();
                    if (this._reader != null) this._reader.Dispose();
                }
                catch
                {
                }

                System.Diagnostics.Debug.WriteLine("Disconnected");
            }
            return true;
        }

        private int _ConnectionToken;

        /// <summary>
        /// Select watch face/app
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task Select(Guid ID, WatchItemType _type)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            switch (_type)
            {
                case WatchItemType.WatchFace:

                    localSettings.Values[Constants.BackgroundCommunicatieSelectItem] = ID.ToString();

                    await StartBackgroundTask(Initiator.Select);

                    break;

                case WatchItemType.WatchApp:

                    localSettings.Values[Constants.BackgroundCommunicatieLaunchItem] = ID.ToString();

                    await StartBackgroundTask(Initiator.Launch);

                    break;
            }
        }

        public async Task SelectFace(Guid ID)
        { 
            //Connect to the watch
            try
            {
                //_ConnectionToken = await Connect(_ConnectionToken);

                if (!IsConnected)
                {
                    throw new Exception("No connection with Pebble Time");
                }

                Guid CurrentWatchFace = Pebble.CurrentWatchFace;
                //Get current ID
                /*WatchFaceMessage _wfm = new WatchFaceMessage();
                Guid CurrentWatchFace = await Pebble.RequestWatchFaceMessageAsync(_wfm);
                */
                if (CurrentWatchFace != Guid.Empty)
                {
                    //Set new ID
                    WatchFaceSelectMessage _wfsm = new WatchFaceSelectMessage(CurrentWatchFace, ID);
                    await Pebble.WriteWatchFaceSelectMessageAsync(_wfsm);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Select WatchFace: " + e.Message);
            }

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["CurrentWatchFace"] = ID;

            //Pebble._protocol.StartRun();
            //SetDisconnectTimer(60, _ConnectionToken);
            //Pebble.ItemSend += Pebble_ItemSend;

            //if (_pc.IsConnected) _pc.Disconnect();
        }

        public async Task Launch(Guid ID)
        {
            //Connect to the watch
            try
            {
                //_ConnectionToken = await Connect(_ConnectionToken);

                if (!IsConnected)
                {
                    throw new Exception("No connection with Pebble Time");
                }

                Guid CurrentWatchFace = Pebble.CurrentWatchFace;
                //Get current ID
                /*WatchFaceMessage _wfm = new WatchFaceMessage();
                Guid CurrentWatchFace = await Pebble.RequestWatchFaceMessageAsync(_wfm);
                */

                //Pebble._protocol.StartRun();
                //SetDisconnectTimer(60, _ConnectionToken);
                //Pebble.ItemSend += Pebble_ItemSend;

                if (CurrentWatchFace != Guid.Empty)
                {
                    //Set new ID
                    await Pebble.LaunchAppAsync(ID);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Select WatchApp: " + e.Message);
            }

            //var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            //localSettings.Values["CurrentWatchFace"] = ID;

            //if (_pc.IsConnected) _pc.Disconnect();
        }

        void Pebble_ItemSend(object sender, EventArgs e)
        {
            Disconnect(_ConnectionToken);

            Pebble.ItemSend -= Pebble_ItemSend;

            System.Diagnostics.Debug.WriteLine("Disconnected");
        }

        private bool _TimerRunning;
        private int Cycles;

        /// <summary>
        /// Set the time out duration after which the connection will be disconnected
        /// </summary>
        /// <param name="Seconds"></param>
        public async void SetDisconnectTimer(int Seconds, int ConnectionToken)
        {
            if (_TimerRunning)
            {
                Cycles = Math.Max(Seconds * 10, Cycles);
                return;
            }

            Cycles = Seconds * 10;
            _TimerRunning = true;

            while (IsConnected && Cycles > 0)
            {
                await Task.Delay(100);

                Cycles--;
            }
            _TimerRunning = false;

            Disconnect(ConnectionToken);

            System.Diagnostics.Debug.WriteLine(String.Format("Disconnect after {0} seconds", Seconds - (Cycles / 10)));
        }


        /// <summary>
        /// Get the next message identifier
        /// </summary>
        /// <returns></returns>
        public int GetNextMessageIdentifier()
        {
            return _pebble.GetNextMessageIdentifier();
        }

        /// <summary>
        /// Returns the global instance of the PebbleConnector class
        /// </summary>
        /// <returns></returns>
        public static PebbleConnector GetInstance()
        {
            if (_PebbleConnector == null)
            {
                _PebbleConnector = new PebbleConnector();
            }

            return _PebbleConnector;
        }


        #endregion

        #region BackgroundCommunication
        
        /// <summary>
        /// Returns true if background communication is running
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsBackgroundTaskRunning()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.BackgroundCommunicatieIsRunning] = false;

            await System.Threading.Tasks.Task.Delay(750);

            bool Result = (bool)localSettings.Values[Constants.BackgroundCommunicatieIsRunning];

            return Result;

            /*try
            {
                Mutex _mutex;
                Result = Mutex.TryOpenExisting(Constants.BackgroundCommunicatieMutex, out _mutex);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("IsBackgroundTaskRunning exception: " + e.Message);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("BackgroundCommunication running: " + Result);
            }

            return Result;*/
        }

        public enum Initiator
        {
            Manual = 0x0001,
            Select = 0x0002,
            Reset = 0x0004,
            Synchronize = 0x0008,
            Launch = 0x0010,
            Delay = 0x0020,
            AddItem = 0x0040,
            Pace = 0x0100,
            Tennis = 0x0200,
            Reserved1 = 0x0400,
            Reserved2 = 0x0800
        }

        /// <summary>
        /// Start background task
        /// </summary>
        public async Task StartBackgroundTask(Initiator InitiatedBy)
        {
            PebbleConnector.SetBackgroundTaskRunningStatus(InitiatedBy);

            if (!await IsBackgroundTaskRunning())
            {
                // Only register the task if it's not registered yet
                var result = await BackgroundExecutionManager.RequestAccessAsync();

                if (result != BackgroundAccessStatus.Denied)
                {
                    //Unregister background task
                    foreach (var task in BackgroundTaskRegistration.AllTasks)
                    {
                        if (task.Value.Name == Constants.BackgroundCommunicationTaskName)
                        {
                            task.Value.Unregister(true);
                        }
                    }

                    EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();

                    if (deviceInfo.SystemProductName == "Virtual")
                    {
                        //Start on emulator
                        var Trigger = new TimeTrigger(15, true);

                        BackgroundTaskBuilder _builder = new BackgroundTaskBuilder
                        {
                            Name = Constants.BackgroundCommunicationTaskName,
                            TaskEntryPoint = Constants.BackgroundCommunicationTaskEntry,
                        };
                        _builder.SetTrigger(Trigger);
                        _builder.Register();

                        return;
                    }

                    //Start on real device
                    var _Trigger = new DeviceUseTrigger();

                    BackgroundTaskBuilder builder = new BackgroundTaskBuilder
                    {
                        Name = Constants.BackgroundCommunicationTaskName,
                        TaskEntryPoint = Constants.BackgroundCommunicationTaskEntry,
                    };
                    builder.SetTrigger(_Trigger);
                    var _registration = builder.Register();

                    try
                    {
                        var device = (await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(new Guid(Constants.PebbleGuid))))).FirstOrDefault(y => y.Name.Contains("Pebble"));

                        if (device == null) throw new OperationCanceledException("Is bluetooth enabled and the Pebble Time paired?");

                        DeviceTriggerResult x = await _Trigger.RequestAsync(device.Id);

                        System.Diagnostics.Debug.WriteLine("DeviceTriggerResult: " + x.ToString());
                    }
                    catch (Exception exc)
                    {
                        if (exc.GetType() == typeof(System.OperationCanceledException)) throw exc;
                        if (exc.GetType() != typeof(System.InvalidOperationException))
                        {
                            throw new Exception("Background communication task can't be started: " + exc.Message);
                        }
                    }
                }
            }
        }

        public void StopBackgroundTask(Initiator InitiatedBy)
        {
            PebbleConnector.ClearBackgroundTaskRunningStatus(InitiatedBy);

        }

        public static void SetBackgroundTaskRunningStatus(Initiator InitiatedBy)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = localSettings.Values[Constants.BackgroundCommunicatieContinue];
            if (value != null && value.GetType() == typeof(int))
            {
                int newValue = (int)value;
                newValue |= (int)InitiatedBy;
                localSettings.Values[Constants.BackgroundCommunicatieContinue] = newValue;
            }
            else
            {
                localSettings.Values[Constants.BackgroundCommunicatieContinue] = (int)InitiatedBy;
            }
        }

        /// <summary>
        /// Stop the background task
        /// </summary>
        public static void ClearBackgroundTaskRunningStatus(Initiator InitiatedBy)
        {
            //Stop background
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            int value = (int)localSettings.Values[Constants.BackgroundCommunicatieContinue];
            value &= ~(int)InitiatedBy;

            if (InitiatedBy == Initiator.Manual) value = 0;

            localSettings.Values[Constants.BackgroundCommunicatieContinue] = value;
        }

        public static bool IsBackgroundTaskRunningStatusSet(Initiator InitiatedBy)
        {
            //Stop background
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            int value = (int)localSettings.Values[Constants.BackgroundCommunicatieContinue];

            value &= (int)InitiatedBy;

            return (value != 0);
       }

        #endregion

    }
}
