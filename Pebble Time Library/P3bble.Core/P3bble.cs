using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using P3bble.Communication;
using P3bble.Constants;
using P3bble.Messages;
using P3bble.Types;
using Windows.Networking.Proximity;
using Windows.Storage;
using P3bble.PCL;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using Pebble_Time_Manager.WatchItems;
using Pebble_Time_Manager.Common;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;


#if NETFX_CORE && !WINDOWS_PHONE_APP
using Windows.Devices.Bluetooth.Rfcomm;
#endif

namespace P3bble
{
    /// <summary>
    /// Delegate to handle music control events
    /// </summary>
    /// <param name="action">The control action.</param>
    /// <remarks>Using this requires you set Pebble.IsMusicControlEnabled to true</remarks>
    public delegate void MusicControlReceivedHandler(MusicControlAction action);

    /// <summary>
    /// Delegate to handle installation progress
    /// </summary>
    /// <param name="percentComplete">The percent complete.</param>
    public delegate void InstallProgressHandler(int percentComplete);

    public class PebbleDevice
    {
        #region Properties

        public String Name { get; set; }
        public String ServiceId { get; set; }
        public String Firmware { get; set; }
        public String Board { get; set; }

        #endregion

        static public PebbleDevice LoadAssociatedDevice()
        {
            try
            {
                PebbleDevice Result = new PebbleDevice();

                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                if (localSettings.Values.ContainsKey("AssociatedDevice") && localSettings.Values.ContainsKey("AssociatedDeviceId"))
                {
                    Result.Name = (string)localSettings.Values["AssociatedDevice"];
                    Result.ServiceId = (string)localSettings.Values["AssociatedDeviceId"];

                    if (localSettings.Values.ContainsKey("AssociatedDeviceFirmware"))
                    {
                        Result.Firmware = (string)localSettings.Values["AssociatedDeviceFirmware"];
                    }
                    if (localSettings.Values.ContainsKey("AssociatedDeviceBoard"))
                    {
                        Result.Board = (string)localSettings.Values["AssociatedDeviceBoard"];
                    }
                    return Result;
                }
            }
            catch (Exception) { }

            return null;
        }

        static public void RemoveAssociation()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values.Remove("AssociatedDevice");
            localSettings.Values.Remove("AssociatedDeviceId");
            localSettings.Values.Remove("AssociatedDeviceFirmware");
            localSettings.Values.Remove("AssociatedDeviceBoard");
        }

        public void StoreAsAssociateDevice()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["AssociatedDevice"] = Name;
            localSettings.Values["AssociatedDeviceId"] = ServiceId;
            localSettings.Values["AssociatedDeviceFirmware"] = Firmware;
            localSettings.Values["AssociatedDeviceBoard"] = Board;
        }
    }

    /// <summary>
    /// Defines a connection to a Pebble watch
    /// </summary>
    public class P3bble : IP3bble, IDisposable
    {
        #region Constructor

        public P3bble()
        {
            Initialise();
        }

        private async Task Initialise()
        {
            await WatchItems.Load();
            LastError = "";
        }

        #endregion

        #region Fields

        // The underlying protocol handler...
        public Protocol _protocol;

        // Used to synchronise calls to the Pebble to make more natural for the API consumer...
        private ManualResetEventSlim _pendingMessageSignal;
        private P3bbleMessage _pendingMessage;
        private bool _phoneVersionReceived;

        //data structures


#if NETFX_CORE && !WINDOWS_PHONE_APP
        private RfcommDeviceService _deviceService;
#endif

        #endregion

        #region Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="P3bble"/> class.
        /// </summary>
        /// <param name="peerInformation">The peer device to connect to.</param>
        internal P3bble(PeerInformation peerInformation)
        {
            PeerInformation = peerInformation;
        }

        internal P3bble(BluetoothDevice _device)
        {
            BluetoothDevice = _device;
        }

#if NETFX_CORE && !WINDOWS_PHONE_APP
        internal P3bble(RfcommDeviceService deviceService)
        {
            _deviceService = deviceService;
        }
#endif


        /// <summary>
        /// Gets or sets a value indicating whether logging is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if logging enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsLoggingEnabled
        {
            get
            {
                return ServiceLocator.Logger.IsEnabled;
            }

            set
            {
                ServiceLocator.Logger.IsEnabled = value;
            }
        }

        /// <summary>
        /// Reference to Log collection
        /// </summary>
        public ObservableCollection<String> Log
        { get; set; }

        public delegate void PebbleMessageReceivedHandler(P3bbleMessage message);

        /// <summary>
        /// Handler to hook message handler at
        /// </summary>
        public PebbleMessageReceivedHandler MessageReceived { get; set; }

        /// <summary>
        /// Reference to watch items list
        /// </summary>
        public WatchItems WatchItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether music control is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if music control enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Turning this on tells the Pebble we are running Android</remarks>
        public static bool IsMusicControlEnabled { get; set; }

        /// <summary>
        /// Gets or sets the music control received handler.
        /// </summary>
        /// <value>
        /// The music control received handler.
        /// </value>
        public MusicControlReceivedHandler MusicControlReceived { get; set; }

        /// <summary>
        /// Gets or sets the install progress handler.
        /// </summary>
        /// <value>
        /// The install progress handler.
        /// </value>
        public InstallProgressHandler InstallProgress { get; set; }

        /// <summary>
        /// Gets a value indicating whether the Pebble is busy.
        /// </summary>
        /// <value>
        ///   <c>true</c> if busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets the display name for the device
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName
        {
            get
            {
                if (PeerInformation != null) return PeerInformation.DisplayName.Trim();
                if (BluetoothDevice != null) return BluetoothDevice.Name.Trim();
                return "";
            }
        }

        /// <summary>
        /// Gets the firmware version.
        /// </summary>
        /// <value>
        /// The firmware version.
        /// </value>
        public FirmwareVersion FirmwareVersion { get; private set; }

        /// <summary>
        /// Gets the recovery firmware version.
        /// </summary>
        /// <value>
        /// The recovery firmware version.
        /// </value>
        public FirmwareVersion RecoveryFirmwareVersion { get; private set; }

        public String Board { get; private set; }

        public bool IsUnfaithful { get; private set; }

        public PebbleDeviceType PebbleDeviceType
        {

            get
            {
                if (Board.ToLower().Contains("snowy")) return PebbleDeviceType.Basalt;
                if (Board.ToLower().Contains("bobby")) return PebbleDeviceType.Chalk;
                if (Board.ToLower().Contains("spauld")) return PebbleDeviceType.Chalk;
                return PebbleDeviceType.Aplite;
            }
        }

        /// <summary>
        /// Gets the underlying Bluetooth peer information.
        /// </summary>
        /// <value>
        /// The peer information.
        /// </value>
        public PeerInformation PeerInformation { get; private set; }

        public BluetoothDevice BluetoothDevice { get; private set; }

        /// <summary>
        /// Bundle being upload
        /// </summary>
        internal BundleUpload BundleUpload { get; private set; }

        /// <summary>
        /// The current watch face
        /// </summary>
        public Guid CurrentWatchFace { get; private set; }

        #endregion

        #region Events

        //Event handler for item send to watch
        public delegate void ItemSendEventHandler(object sender, EventArgs e);

        public event ItemSendEventHandler ItemSend;

        protected virtual void OnItemSend(EventArgs e)
        {
            if (ItemSend != null) ItemSend(this, e);
        }

        #endregion

        /// <summary>
        /// Detects any paired pebbles.
        /// </summary>
        /// <returns>A list of pebbles if some are found</returns>
        public static async Task<List<P3bble>> DetectPebbles()
        {
#if DEBUG
            // Turn on logging for debug builds by default
            ServiceLocator.Logger.IsEnabled = true;
#endif
            return await FindPebbles();
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns>A bool indicating if the connection was successful</returns>
        public async Task<bool> ConnectAsync()
        {

            // Check we're not already connected...
            if (this.IsConnected)
            {
                return true;
            }

            try
            {
                //#if NETFX_CORE && !WINDOWS_PHONE_APP
                //     this._protocol = await Protocol.CreateProtocolAsync(_deviceService);
                //#else
                //      this._protocol = await Protocol.CreateProtocolAsync(PeerInformation);
                this._protocol = await Protocol.CreateProtocolAsync(BluetoothDevice);
                //#endif
                if (this._protocol == null)
                {
                    throw new Exception("Could not initiate bluetooth protocol with Pebble Time.");
                }
                this._protocol.MessageReceived += this.ProtocolMessageReceived;

                P3bbleMessage _receivedMsg;

                //    WatchItems.Load();

                //phone version message
                // _receivedMsg = await this._protocol.ReceiveMessage(35);
                await this._protocol.ClearMessageBuffer();
                await this._protocol.WriteMessage(new PhoneVersionMessage(false));

                //pebble firmware version
                await this._protocol.WriteMessage(new VersionMessage());
                _receivedMsg = await this._protocol.ReceiveMessage(0);
                while (_receivedMsg == null || _receivedMsg.Endpoint != Endpoint.Version)
                {
                    _receivedMsg = await this._protocol.ReceiveMessage(0);
                }

                var VersionMessage = _receivedMsg as VersionMessage;
                this.FirmwareVersion = VersionMessage.Firmware;
                this.Board = VersionMessage.Board;
                this.IsUnfaithful = VersionMessage.IsUnfaithful;

                PebbleDevice _pd = PebbleDevice.LoadAssociatedDevice();
                if (_pd != null)
                {
                    _pd.Firmware = VersionMessage.Firmware.Version.ToString();
                    _pd.Board = VersionMessage.Board;
                    _pd.StoreAsAssociateDevice();
                }

                //factory setting message
                await this._protocol.WriteMessage(new FactorySettingMessage());
                _receivedMsg = await this._protocol.ReceiveMessage(0);

                //watch face message
                await this._protocol.WriteMessage(new WatchFaceMessage());
                while (_receivedMsg == null || _receivedMsg.Endpoint != Endpoint.WatchFaceSelect)
                {
                    _receivedMsg = await this._protocol.ReceiveMessage(0);
                }
                WatchFaceMessage wfm = (WatchFaceMessage)_receivedMsg;
                CurrentWatchFace = wfm.CurrentWatchFace;

                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["CurrentWatchFace"] = CurrentWatchFace;


                // await this._protocol.WriteMessage(new ResetMessage());

                //time message
                await this._protocol.WriteMessage(new TimeMessage(DateTime.Now));
                //_receivedMsg = await this._protocol.ReceiveMessage(0);

                this.IsConnected = true;
                // this._protocol.StartRun();

            }
            catch (Exception e)
            {
                LastError = e.Message;
                ServiceLocator.Logger.WriteLine("Error connecting to pebble " + e.Message);
                Debug.WriteLine("Can't connect to Pebble Time; is it already connected to another device?");
                this.IsConnected = false;
            }

            return this.IsConnected;
        }

        public String LastError;


        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            if (this._protocol != null)
            {
                this._protocol.StopRun();
                this._protocol.Dispose();
                this._protocol.MessageReceived = null;
                this._protocol = null;
                IsConnected = false;
            }

            if (!ServiceLocator.Logger.IsEnabled)
            {
                ServiceLocator.Logger.ClearUp();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Disconnect();
        }

        /// <summary>
        /// Pings the device.
        /// </summary>
        /// <returns>An async task to wait if required</returns>
        public Task PingAsync()
        {
            return this._protocol.WriteMessage(new PingMessage());
        }

        /// <summary>
        /// Resets the watch.
        /// </summary>
        /// <returns>An async task to wait if required</returns>
        public Task ResetAsync()
        {
            return this._protocol.WriteMessage(new ResetMessage());
        }

        /// <summary>
        /// Send pace message.
        /// </summary>
        /// <returns>An async task to wait if required</returns>
        public Task SendSportMessage(TimeSpan Duration, double Distance, TimeSpan Pace, bool MetricSystem)
        {
            return this._protocol.WriteMessage(new SportMessage(Duration, Distance, Pace, MetricSystem));
        }

        /// <summary>
        /// Send tennis score.
        /// </summary>
        /// <returns>An async task to wait if required</returns>
        public Task SendTennisMessage(String GameScore, String SetScore)
        {
            return this._protocol.WriteMessage(new TennisMessage(GameScore, SetScore));
        }
        public Task SendTennisMessage(String GameScore, String SetScore, String Status)
        {
            return this._protocol.WriteMessage(new TennisMessage(GameScore, SetScore, Status));
        }
        public Task SendTennisMessage(String GameScore, String SetScore, String Sets, String Status)
        {
            return this._protocol.WriteMessage(new TennisMessage(GameScore, SetScore, Sets, Status));
        }


        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <returns>An async task to wait that will return the current time</returns>
        public async Task<DateTime> GetTimeAsync()
        {
            var result = await this.SendMessageAndAwaitResponseAsync<TimeMessage>(new TimeMessage());
            if (result != null)
            {
                return result.Time;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Sets the time on the Pebble.
        /// </summary>
        /// <param name="newTime">The new time.</param>
        /// <returns>An async task to wait</returns>
        public Task SetTimeAsync(DateTime newTime)
        {
            return this._protocol.WriteMessage(new TimeMessage(newTime));
        }

        public DataWriter Writer()
        {
            return this._protocol._writer;
        }

        /// <summary>
        /// Gets the latest firmware version.
        /// </summary>
        /// <returns>An async task to wait that will result in firmware info</returns>
        public async Task<FirmwareResponse> GetLatestFirmwareVersionAsync()
        {
            string url = this.FirmwareVersion.GetFirmwareServerUrl(false);

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var serializer = new DataContractJsonSerializer(typeof(FirmwareResponse));
                    FirmwareResponse info = serializer.ReadObject(stream) as FirmwareResponse;
                    return info;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a list of the installed apps.
        /// </summary>
        /// <returns>
        /// An async task to wait that will result in a list of apps
        /// </returns>
        public async Task<InstalledApplications> GetInstalledAppsAsync()
        {
            System.Diagnostics.Debug.WriteLine("GetInstalledAppsAsync");
            var result = await this.SendMessageAndAwaitResponseAsync<AppManagerMessage>(new AppManagerMessage(AppManagerAction.ListApps));
            if (result != null)
            {
                return result.InstalledApplications;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Remove an installed application from the specified app-bank.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        public async Task<bool> RemoveAppAsync(InstalledApplication app)
        {
            ServiceLocator.Logger.WriteLine("RemoveAppAsync");
            var result = await this.SendMessageAndAwaitResponseAsync<AppManagerMessage>(new AppManagerMessage(AppManagerAction.RemoveApp, app.Id, app.Index));
            if (result != null)
            {
                return result.Result == AppManagerResult.AppRemoved;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        private int _messageidentifier;
        /// <summary>
        /// Get the next message identifier
        /// </summary>
        /// <returns></returns>
        public int GetNextMessageIdentifier()
        {
            _messageidentifier++;
            return _messageidentifier;
        }

        /// <summary>
        /// Add a watch item and send it to the Pebble (if connected)
        /// </summary>
        /// <param name="_newItem"></param>
        /// <returns></returns>
        public async Task<bool> AddWatchItemAsync(WatchItem _newItem)
        {
            try
            {
                ServiceLocator.Logger.WriteLine("AddWatchItemAsync");

                //Add to app
                await WatchItems.AddWatchItem(_newItem);

                //Add to watch
                WatchItemAddMessage _waa = new WatchItemAddMessage(GetNextMessageIdentifier(), _newItem);
                await _protocol.WriteMessage(_waa);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("AddWatchItemAsync exception: " + e.Message);
                ServiceLocator.Logger.WriteLine("AddWatchItemAsync exception: " + e.Message);
            }

            return false;
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
                ServiceLocator.Logger.WriteLine("DeleteWatchItemAsync");

                //Remove from watch
                if (IsConnected)
                {
                    WatchItemDeleteMessage _wda = new WatchItemDeleteMessage(GetNextMessageIdentifier(), _deleteItem);
                    await _protocol.WriteMessage(_wda);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("DeleteWatchItemAsync exception: " + e.Message);
                ServiceLocator.Logger.WriteLine("DeleteWatchItemAsync exception: " + e.Message);
            }

            return false;
        }

        /// <summary>
        /// Sets the now playing track.
        /// </summary>
        /// <param name="artist">The artist.</param>
        /// <param name="album">The album.</param>
        /// <param name="track">The track.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        /// <remarks>Using this method requires you set Pebble.IsMusicControlEnabled to true</remarks>
        public Task SetNowPlayingAsync(string artist, string album, string track)
        {
            return this._protocol.WriteMessage(new MusicMessage(artist, album, track));
        }

        /// <summary>
        /// Downloads an application or firmware bundle
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        public async Task<Bundle> DownloadBundleAsync(string uri)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var downloadStream = await response.Content.ReadAsStreamAsync();

                Guid fileGuid = Guid.NewGuid();

                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileGuid.ToString());

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    byte[] buffer = new byte[1024];
                    while (downloadStream.Read(buffer, 0, buffer.Length) > 0)
                    {
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }

                var bundle = new Bundle(fileGuid.ToString());
                await bundle.Initialise();
                return bundle;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Installs an application.
        /// </summary>
        /// <param name="bundle">The application.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        public async Task InstallAppAsync(Bundle bundle)
        {
            if (bundle.BundleType != BundleType.Application)
            {
                throw new ArgumentException("Only app bundles can be installed");
            }

            // Get list of installed apps...
            var installedApps = await this.GetInstalledAppsAsync();

            // Find the first free slot...
            uint firstFreeBank = 1;
            foreach (var installedApp in installedApps.ApplicationsInstalled)
            {
                if (installedApp.Index == firstFreeBank)
                {
                    firstFreeBank++;
                }

                if (firstFreeBank == installedApps.ApplicationBanks)
                {
                    throw new CannotInstallException("There are no memory slots free");
                }
            }

            double progress = 0;
            double totalBytes = bundle.Manifest.ApplicationManifest.Size + bundle.Manifest.Resources.Size;

            InstallProgressHandler handler = null;

            if (this.InstallProgress != null)
            {
                // Derive overall progress from the bytes sent for the part...
                handler = new InstallProgressHandler((partProgress) =>
                {
                    progress += partProgress;
                    int percentComplete = (int)(progress / totalBytes * 100);
                    System.Diagnostics.Debug.WriteLine("Installation " + percentComplete.ToString() + "% complete - " + progress.ToString() + " / " + totalBytes.ToString());
                    this.InstallProgress(percentComplete);
                });
            }

            System.Diagnostics.Debug.WriteLine(string.Format("Attempting to add app to bank {0} of {1}", firstFreeBank, firstFreeBank /*installedApps.ApplicationBanks*/));

            PutBytesMessage binMsg = new PutBytesMessage(PutBytesTransferType.Binary, bundle.BinaryContent, handler, firstFreeBank);

            try
            {
                var binResult = await this.SendMessageAndAwaitResponseAsync<PutBytesMessage>(binMsg, 60000);

                if (binResult == null || binResult.Errored)
                {
                    throw new CannotInstallException(string.Format("Failed to send binary {0}", bundle.Manifest.ApplicationManifest.Filename));
                }
            }
            catch (ProtocolException pex)
            {
                throw new CannotInstallException("Sorry, an internal error occurred: " + pex.Message);
            }

            if (bundle.HasResources)
            {
                PutBytesMessage resourcesMsg = new PutBytesMessage(PutBytesTransferType.Resources, bundle.Resources, handler, firstFreeBank);

                try
                {
                    var resourceResult = await this.SendMessageAndAwaitResponseAsync<PutBytesMessage>(resourcesMsg, 240000);

                    if (resourceResult == null || resourceResult.Errored)
                    {
                        throw new CannotInstallException(string.Format("Failed to send resources {0}", bundle.Manifest.Resources.Filename));
                    }
                }
                catch (ProtocolException pex)
                {
                    throw new CannotInstallException("Sorry, an internal error occurred: " + pex.Message);
                }
            }

            var appMsg = new AppMessage(Endpoint.AppManager) { Command = AppCommand.FinaliseInstall, AppIndex = firstFreeBank };
            await this.SendMessageAndAwaitResponseAsync<AppManagerMessage>(appMsg);

            await bundle.DeleteFromStorage();

            if (this.InstallProgress != null)
            {
                this.InstallProgress(100);
            }

            // Now launch the new app
            await this.LaunchAppAsync(bundle.Application.Uuid);
        }

        /// <summary>
        /// Launches an application.
        /// </summary>
        /// <param name="appUuid">The application UUID.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        public async Task<bool> LaunchAppAsync(Guid appUuid)
        {
            var msg = new AppMessage(Endpoint.Launcher)
            {
                AppUuid = appUuid,
                Command = AppCommand.Push
            };

            msg.AddTuple((uint)LauncherKeys.RunState, AppMessageTupleDataType.UInt, (byte)LauncherParams.Running);

            var result = await this.SendMessageAndAwaitResponseAsync<AppMessage>(msg, 0);

            if (result != null)
            {
                return result.Command == AppCommand.Ack;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Installs a firmware bundle.
        /// </summary>
        /// <param name="bundle">The firmware.</param>
        /// <param name="recovery">Whether to install recovery firmware.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        public async Task InstallFirmwareAsync(Bundle bundle, bool recovery)
        {
            if (bundle.BundleType != BundleType.Firmware)
            {
                throw new ArgumentException("Only firmware bundles can be installed");
            }

            double progress = 0;
            double totalBytes = bundle.Manifest.Firmware.Size + bundle.Manifest.Resources.Size;

            InstallProgressHandler handler = null;

            if (this.InstallProgress != null)
            {
                // Derive overall progress from the bytes sent for the part...
                handler = new InstallProgressHandler((partProgress) =>
                {
                    progress += partProgress;
                    int percentComplete = (int)(progress / totalBytes * 100);
                    ServiceLocator.Logger.WriteLine("Installation " + percentComplete.ToString() + "% complete - " + progress.ToString() + " / " + totalBytes.ToString());
                    this.InstallProgress(percentComplete);
                });
            }

            await this._protocol.WriteMessage(new SystemMessage(SystemCommand.FirmwareStart));

            if (bundle.HasResources)
            {
                PutBytesMessage resourcesMsg = new PutBytesMessage(PutBytesTransferType.SystemResources, bundle.Resources, handler);

                try
                {
                    var resourceResult = await this.SendMessageAndAwaitResponseAsync<PutBytesMessage>(resourcesMsg, 720000);

                    if (resourceResult == null || resourceResult.Errored)
                    {
                        throw new CannotInstallException(string.Format("Failed to send resources {0}", bundle.Manifest.Resources.Filename));
                    }
                }
                catch (ProtocolException pex)
                {
                    throw new CannotInstallException("Sorry, an internal error occurred: " + pex.Message);
                }
            }

            PutBytesMessage binMsg = new PutBytesMessage(recovery ? PutBytesTransferType.Recovery : PutBytesTransferType.Firmware, bundle.BinaryContent, handler);

            try
            {
                var binResult = await this.SendMessageAndAwaitResponseAsync<PutBytesMessage>(binMsg, 720000);

                if (binResult == null || binResult.Errored)
                {
                    throw new CannotInstallException(string.Format("Failed to send binary {0}", bundle.Manifest.Firmware.Filename));
                }
            }
            catch (ProtocolException pex)
            {
                throw new CannotInstallException("Sorry, an internal error occurred: " + pex.Message);
            }

            await this._protocol.WriteMessage(new SystemMessage(SystemCommand.FirmwareComplete));

            await bundle.DeleteFromStorage();

            if (this.InstallProgress != null)
            {
                this.InstallProgress(100);
            }
        }

        /// <summary>
        /// Installs an app or firmware bundle.
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        /// <remarks>
        /// Convenience method wrapping InstallAppAsync and InstallFirmwareAsync
        /// </remarks>
        public async Task InstallBundleAsync(Bundle bundle)
        {
            if (bundle != null)
            {
                switch (bundle.BundleType)
                {
                    case BundleType.Application:
                        await this.InstallAppAsync(bundle);
                        break;

                    case BundleType.Firmware:
                        await this.InstallFirmwareAsync(bundle, false);
                        break;

                    default:
                        throw new ArgumentException("Unknown bundle type");
                }
            }
            else
            {
                throw new ArgumentNullException("bundle", "Bundle must be supplied");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////
        // Demo methods that aren't much use without lower level OS support...
        //////////////////////////////////////////////////////////////////////////////////

        // Possibly useful for log message reading??
        ////public void BadPing()
        ////{
        ////    _protocol.WriteMessage(new PingMessage(new byte[7] { 1, 2, 3, 4, 5, 6, 7 }));
        ////}

        /// <summary>
        /// Sends a message to the Pebble
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
        public async Task<bool> WriteTimeLineCalenderAsync(P3bbleMessage _message)
        {
            await this._protocol.WriteMessage(_message);
            return true;
            /*if (await this._protocol.ReceiveAcknowledgement())
            { 
                return true;
            }
            else
            {
                throw new TimeoutException();
            }*/
        }

        /// <summary>
        /// Sends a message and await the acknowledgement to the Pebble
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
        public async Task<bool> WriteMessageAndReceiveAcknowledgementAsync(P3bbleMessage _message)
        {
            await this._protocol.WriteMessage(_message);
            if (await this._protocol.ReceiveAcknowledgement())
            {
                return true;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Sends a message to the Pebble
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
        public async Task<bool> WriteMessageAsync(P3bbleMessage _message)
        {
            await this._protocol.WriteMessage(_message);

            return true;
        }

        /// <summary>
        /// Request the current watch face
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
        public async Task<Guid> RequestWatchFaceMessageAsync(P3bbleMessage _message)
        {
            try
            {
                WatchFaceMessage receivedMessage;
                await this._protocol.WriteMessage(_message);

                receivedMessage = (WatchFaceMessage)await this._protocol.ReceiveMessage(0);

                return receivedMessage.CurrentWatchFace;
            }
            catch (Exception e)
            {
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Sends a watchface select message
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
        public async Task<bool> WriteWatchFaceSelectMessageAsync(P3bbleMessage _message)
        {
            await this._protocol.WriteMessage(_message);
            return true;
            /*if (await this._protocol.ReceiveAcknowledgement())
            {
                return true;
            }
            else
            {
                throw new TimeoutException();
            }*/
        }

        /// <summary>
        /// Sends an SMS notification.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        /// <remarks>
        /// Mainly for demoing capability
        /// </remarks>
        public Task SmsNotificationAsync(string Header, string Message)
        {
            return this._protocol.WriteMessage(new NotificationMessage(Pebble_Time_Manager.Connector.PebbleConnector.GetInstance().GetNextMessageIdentifier(),
                Guid.NewGuid(),
                DateTime.Now,
                Header,
                Message,
                Icons.bell));

            //return this._protocol.WriteMessage(new NotificationMessage(NotificationType.SMS, sender, message));
        }

        /// <summary>
        /// Starts a Phone call notification.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="number">The number.</param>
        /// <param name="cookie">The cookie.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        /// <remarks>
        /// Mainly for demoing capability
        /// </remarks>
        public Task PhoneCallAsync(string name, string number, byte[] cookie)
        {
            return this._protocol.WriteMessage(new PhoneControlMessage(PhoneControlType.IncomingCall, cookie, number, name));
        }

        /// <summary>
        /// Starts a Phone call Ring.
        /// </summary>
        /// <param name="cookie">The cookie.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        /// <remarks>
        /// Mainly for demoing capability
        /// </remarks>
        public Task RingAsync(byte[] cookie)
        {
            return this._protocol.WriteMessage(new PhoneControlMessage(PhoneControlType.Ring, cookie));
        }

        /// <summary>
        /// Indicate that a Phone call has started.
        /// </summary>
        /// <param name="cookie">The cookie.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        /// <remarks>
        /// Mainly for demoing capability
        /// </remarks>
        public Task StartCallAsync(byte[] cookie)
        {
            return this._protocol.WriteMessage(new PhoneControlMessage(PhoneControlType.Start, cookie));
        }

        /// <summary>
        /// Indicate that a Phone call has ended.
        /// </summary>
        /// <param name="cookie">The cookie.</param>
        /// <returns>
        /// An async task to wait
        /// </returns>
        /// <remarks>
        /// Mainly for demoing capability
        /// </remarks>
        public Task EndCallAsync(byte[] cookie)
        {
            return this._protocol.WriteMessage(new PhoneControlMessage(PhoneControlType.End, cookie));
        }

        //////////////////////////////////////////////////////////////////////////////////
        // Private methods below - e.g. handling discovery or incoming messages
        //////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Finds paired pebbles.
        /// </summary>
        /// <returns>A list of pebbles</returns>
        private static async Task<List<P3bble>> FindPebbles()
        {
            List<P3bble> result = new List<P3bble>();

            try
            {
#if NETFX_CORE && !WINDOWS_PHONE_APP

                var PebbleRfCommID = RfcommServiceId.FromUuid(new Guid("00000000-deca-fade-deca-deafdecacaff"));
                var PebbleDeviceService = RfcommDeviceService.GetDeviceSelector(PebbleRfCommID);
                var PebbleDevices = await DeviceInformation.FindAllAsync(PebbleDeviceService);


                //String BTSelector = BluetoothDevice.GetDeviceSelector();
                //if (Devices.Count <= 0) return;
                //var devices = await DeviceInformation.FindAllAsync(BTSelector);

                //                var selector = BluetoothDevice.GetDeviceSelector();
                //                var devices = await DeviceInformation.FindAllAsync(selector);
                string s = BluetoothDevice.GetDeviceSelector();

                foreach (var device in PebbleDevices)
                {
                    if (device.Name.ToLower().Contains("pebble"))
                    {
                        try
                        {
                            var _device = await BluetoothDevice.FromIdAsync(device.Id);
                            //var service = await RfcommDeviceService.FromIdAsync(device.Id);
                            //device.
                            result.Add(new P3bble(_device));
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e.Message);
                        }
                    }
                }

#else

                //PeerFinder.Start();
                PeerFinder.AlternateIdentities["Bluetooth:Paired"] = string.Empty;
                IReadOnlyList<PeerInformation> pairedDevices = await PeerFinder.FindAllPeersAsync();

                // Filter to only devices that are named Pebble - right now, that's the only way to
                // stop us getting headphones, etc. showing up...
                foreach (PeerInformation pi in pairedDevices)
                {
                    if (pi.DisplayName.ToLower().Contains("pebble")
                        || pi.DisplayName.ToLower().Contains("bc5f")
                        || pi.DisplayName.ToLower().Contains("7e3f"))
                    {
                        result.Add(new P3bble(pi));
                    }
                }

                if (pairedDevices.Count == 0)
                {
                    ServiceLocator.Logger.WriteLine("No paired devices were found.");
                }
#endif
            }
            catch (Exception ex)
            {
                // If Bluetooth is turned off, we will get an exception. We catch it to return a zero-count list.
                ServiceLocator.Logger.WriteLine("Exception looking for Pebbles: " + ex.ToString());
            }

            return result;
        }


        public static async Task<PebbleDevice> FindPebble()
        {
            return await FindPebble("");
        }


        public static async Task<PebbleDevice> FindPebble(String ServiceId)
        {
            List<P3bble> result = new List<P3bble>();

            try
            {
                //var PebbleRfCommID = RfcommServiceId.FromUuid(new Guid("00000000-deca-fade-deca-deafdecacaff"));
                //var PebbleDeviceService = RfcommDeviceService.GetDeviceSelector(PebbleRfCommID);
                //var PebbleDevices = await DeviceInformation.FindAllAsync(PebbleDeviceService);
                DeviceInformationCollection AllDevices;

                if (ServiceId.Length == 0)
                {
                    AllDevices = await DeviceInformation.FindAllAsync();
                }
                else
                {
                    string InterfaceClassGuid = ServiceId.Substring(ServiceId.Length - 38);
                    string Filter = String.Format("System.Devices.InterfaceClassGuid:=\"{0}\"", InterfaceClassGuid);
                    AllDevices = await DeviceInformation.FindAllAsync(Filter);
                }

                foreach (var device in AllDevices)
                {
                    if (device.Name.ToLower().Contains("pebble"))
                    {
                        var PebbleDevice = new PebbleDevice();
                        PebbleDevice.Name = device.Name;
                        PebbleDevice.ServiceId = device.Id;

                        return PebbleDevice;
                    }
                }
            }
            catch (Exception ex)
            {
                // If Bluetooth is turned off, we will get an exception. We catch it to return a zero-count list.
                ServiceLocator.Logger.WriteLine("Exception looking for Pebbles: " + ex.ToString());
            }

            return null;
        }

        /// <summary>
        /// Handles protocol messages
        /// </summary>
        /// <param name="message">The message.</param>
        private async void ProtocolMessageReceived(P3bbleMessage message)
        {
            ServiceLocator.Logger.WriteLine("ProtocolMessageReceived: " + message.Endpoint.ToString());

            switch (message.Endpoint)
            {
                case Endpoint.PhoneVersion:
                    // We need to tell the Pebble what we are...
                    await this._protocol.WriteMessage(new PhoneVersionMessage(IsMusicControlEnabled));
                    _phoneVersionReceived = true;
                    break;

                case Endpoint.Version:
                    // Store version info we got from the Pebble...
                    VersionMessage version = message as VersionMessage;
                    this.FirmwareVersion = version.Firmware;
                    this.RecoveryFirmwareVersion = version.RecoveryFirmware;
                    this.Board = version.Board;
                    this.IsUnfaithful = version.IsUnfaithful;
                    break;

                case Endpoint.Logs:
                    if (message as LogsMessage != null)
                    {
                        ServiceLocator.Logger.WriteLine("LOG: '" + (message as LogsMessage).Message + "'");
                    }

                    break;

                case Endpoint.MusicControl:
                    var musicMessage = message as MusicMessage;
                    if (this.MusicControlReceived != null && musicMessage != null && musicMessage.ControlAction != MusicControlAction.Unknown)
                    {
                        this.MusicControlReceived(musicMessage.ControlAction);
                    }

                    break;

                case Endpoint.AppManagerV3:

                    Messages.AppManagerMessageV3 apmm3 = (Messages.AppManagerMessageV3)message;

                    System.Diagnostics.Debug.WriteLine(">> RECEIVED MESSAGE FOR ENDPOINT AppManagerV3");
                    System.Diagnostics.Debug.WriteLine(">> AppManagerV3 - App requested: " + apmm3.App.ToString());

                    if (Log != null)
                    {
                        Log.Add(String.Format("Requested: " + apmm3.App.ToString()));
                    }

                    //Get the bundle and create bundle upload
                    var SelectedApp = WatchItems.FindAll(x => x.ID == apmm3.App);
                    if (SelectedApp.Count() > 0)
                    {
                        //Send the acknowledgement
                        AppManagerMessageV3 _apmv3 = new AppManagerMessageV3(APMV3Type.eAck);
                        await WriteMessageAsync(_apmv3);
                        System.Diagnostics.Debug.WriteLine("<< ACK Send");

                        IWatchItem _selectedApp = (IWatchItem)SelectedApp.First();

                        System.Diagnostics.Debug.WriteLine(String.Format("Requested: " + _selectedApp.Name));
                        if (Log != null)
                        {
                            Log.Add(String.Format("Requested: " + _selectedApp.Name));
                        }

                        //Get the app from the local storage
                        //StorageFile _resource = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/" + _selectedApp.File));
                        //await _resource.CopyAsync(ApplicationData.Current.LocalFolder, _selectedApp.File, NameCollisionOption.ReplaceExisting);

                        Bundle _bundle = await Types.Bundle.LoadFromLocalStorageAsync(_selectedApp.File, this.PebbleDeviceType);

                        BundleUpload = new BundleUpload(_bundle);
                        BundleUpload.TransactionId = apmm3.BundleId;
                        BundleUpload.Status = UploadStatus.eResource;

                        Messages.PutBytesMessageV3 _pbm3wft = new PutBytesMessageV3(PutBytesState.WaitForToken);
                        _pbm3wft.BundleUpload = BundleUpload;

                        await WriteMessageAsync(_pbm3wft);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("Unknown app requested"));
                        if (Log != null)
                        {
                            Log.Add(String.Format("Unknown app requested"));
                        }

                        //Send event
                        OnItemSend(EventArgs.Empty);
                    }

                    break;


                case Endpoint.PutBytes:

                    Messages.PutBytesMessage _pbm = (Messages.PutBytesMessage)message;

                    switch (_pbm.Result[0])
                    {
                        case 0x01:

                            if (BundleUpload.Status != UploadStatus.eNone)
                            {
                                if (!BundleUpload.EOD)
                                {
                                    BundleUpload.AddFileId((Messages.PutBytesMessage)_pbm);

                                    //Send file portion
                                    Messages.PutBytesMessageV3 _pbm3ip = new PutBytesMessageV3(PutBytesState.InProgress);
                                    _pbm3ip.BundleUpload = BundleUpload;

                                    BundleUpload.AddContentPortion(_pbm3ip);

                                    await WriteMessageAsync(_pbm3ip);
                                }
                                else
                                {
                                    //Send CRC
                                    Messages.PutBytesMessageV3 _pbm3crc = new PutBytesMessageV3(PutBytesState.Commit);
                                    _pbm3crc.BundleUpload = BundleUpload;

                                    await WriteMessageAsync(_pbm3crc);
                                }
                            }
                            else
                            {
                                //Send binary file
                                if (BundleUpload.BinaryId.Count == 0)
                                {
                                    BundleUpload.Status = UploadStatus.eBinary;
                                    Messages.PutBytesMessageV3 _pbm3wftb = new PutBytesMessageV3(PutBytesState.WaitForToken);
                                    _pbm3wftb.BundleUpload = BundleUpload;
                                    await WriteMessageAsync(_pbm3wftb);
                                }
                                else
                                {
                                    Messages.PutBytesMessageV3 _pbm3c = new PutBytesMessageV3(PutBytesState.Complete);
                                    _pbm3c.BundleUpload = BundleUpload;

                                    //Send event
                                    OnItemSend(EventArgs.Empty);
                                }
                            }

                            break;
                    }

                    break;

                default:

                    if (this.MessageReceived != null)
                    {
                        this.MessageReceived(message);
                    }

                    break;
            }

            // Check if we're waiting for a message...
            if (this._pendingMessageSignal != null && this._pendingMessage != null)
            {
                if (this._pendingMessage.Endpoint == message.Endpoint)
                {
                    ServiceLocator.Logger.WriteLine("ProtocolMessageReceived: we were waiting for this type of message");

                    // PutBytes messages are state machines, so need special treatment...
                    if (message.Endpoint == Endpoint.PutBytes)
                    {
                        var putMessage = this._pendingMessage as PutBytesMessage;
                        if (putMessage.HandleStateMessage(message as PutBytesMessage))
                        {
                            this._pendingMessageSignal.Set();
                        }
                        else
                        {
                            await this._protocol.WriteMessage(putMessage);
                        }
                    }
                    else
                    {
                        this._pendingMessage = message;
                        this._pendingMessageSignal.Set();
                    }
                }
                else
                {
                    // We've received a Log message when we were expecting something else,
                    // this means the protocol comms got messed up somehow, we should abort...
                    if (message.Endpoint == Endpoint.Logs)
                    {
                        this._pendingMessage = message;
                        this._pendingMessageSignal.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to the Pebble and awaits the response.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <param name="message">The message content.</param>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <returns>A message response</returns>
        /// <exception cref="System.InvalidOperationException">A message is being waited for already</exception>
        /// <remarks>Beware when debugging that setting a breakpoint in Protocol.Run or ProtocolMessageReceived will cause the ResetEvent to time out</remarks>
        private Task<T> SendMessageAndAwaitResponseAsync<T>(P3bbleMessage message, int millisecondsTimeout = 10000)
            where T : P3bbleMessage
        {
            if (this._pendingMessageSignal != null || this.IsBusy)
            {
                throw new InvalidOperationException("Already waiting for a message.");
            }

            return Task.Run<T>(async () =>
            {
                this.IsBusy = true;

                int startTicks = Environment.TickCount;
                this._pendingMessageSignal = new ManualResetEventSlim(false);
                this._pendingMessage = message;

                // Send the message...
                await this._protocol.WriteMessage(message);

                // Return directly if not waiting for the response
                if (millisecondsTimeout == 0) return message as T;

                // Wait for the response...
                this._pendingMessageSignal.Wait(millisecondsTimeout);

                T pendingMessage = null;

                if (this._pendingMessageSignal.IsSet)
                {
                    // Store any response will be null if timed out...
                    pendingMessage = this._pendingMessage as T;
                }

                // See if we have a protocol error
                LogsMessage logMessage = this._pendingMessage as LogsMessage;
                Type pendingMessageType = this._pendingMessage.GetType();

                // Clear the pending variables...
                this._pendingMessageSignal = null;
                this._pendingMessage = null;

                int timeTaken = Environment.TickCount - startTicks;

                this.IsBusy = false;

                if (pendingMessage != null)
                {
                    System.Diagnostics.Debug.WriteLine(pendingMessage.GetType().Name + " message received back in " + timeTaken.ToString() + "ms");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(message.GetType().Name + " message timed out in " + timeTaken.ToString() + "ms - type received was " + pendingMessageType.ToString());
                }

                return pendingMessage;
            });
        }
    }
}
