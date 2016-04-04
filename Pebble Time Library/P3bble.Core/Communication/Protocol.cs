using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using P3bble.Constants;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using P3bble.PCL;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.Devices.Bluetooth;

namespace P3bble.Communication
{
    /// <summary>
    /// Encapsulates comms with the Pebble
    /// </summary>
    public class Protocol : IDisposable
    {
        private readonly Mutex _mutex = new Mutex();
        private StreamSocket _socket;
        public DataWriter _writer;
        private DataReader _reader;
        private object _lock;
        private bool _isRunning;

        private Protocol(StreamSocket socket)
        {
            this._socket = socket;
            this._writer = new DataWriter(this._socket.OutputStream);
            this._reader = new DataReader(this._socket.InputStream);
            this._reader.InputStreamOptions = InputStreamOptions.Partial;

            this._lock = new object();
#if WINDOWS_PHONE
            this._isRunning = true;
            System.Threading.ThreadPool.QueueUserWorkItem(this.Run);
#else
            /*this._isRunning = true;
            this.Run(null);*/
#endif
        }

        public delegate void MessageReceivedHandler(P3bbleMessage message);
        public MessageReceivedHandler MessageReceived { get; set; }

        /// <summary>
        /// Creates the protocol - encapsulates the socket creation
        /// </summary>
        /// <param name="peer">The peer.</param>
        /// <returns>A protocol object</returns>
        public static async Task<Protocol> CreateProtocolAsync(PeerInformation peer)
        {
            //#if WINDOWS_PHONE || WINDOWS_PHONE_APP
            // {00001101-0000-1000-8000-00805f9b34fb} specifies we want a Serial Port - see http://developer.nokia.com/Community/Wiki/Bluetooth_Services_for_Windows_Phone
            // {00000000-deca-fade-deca-deafdecacaff} Fix ServiceID for WP8.1 Update 2

            try
            {
                StreamSocket socket = new StreamSocket();
                await socket.ConnectAsync(peer.HostName, Guid.Parse(Pebble_Time_Manager.Common.Constants.PebbleGuid).ToString("B"));

                return new Protocol(socket);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

            }
            // await socket.ConnectAsync(peer.HostName, Guid.Parse("0000180a-0000-1000-8000-00805f9b34fb").ToString("B"));
            return null;



            //await socket.ConnectAsync(peer.HostName, Guid.Parse("00001101-0000-1000-8000-00805f9b34fb").ToString("B"));


//#endif

            //throw new NotImplementedException();
        }

        public static async Task<Protocol> CreateProtocolAsync(BluetoothDevice _device)
        {
            try
            {
                StreamSocket socket = new StreamSocket();

                var Services = _device.RfcommServices;
                if (Services.Count > 0)
                {
                    await socket.ConnectAsync(_device.HostName, Services[0].ServiceId.Uuid.ToString("B"));

                    return new Protocol(socket);
                }

                //await socket.ConnectAsync(_device.HostName, Guid.Parse(Pebble_Time_Manager.Common.Constants.PebbleGuid).ToString("B"));

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            return null;
        }

#if NETFX_CORE  && !WINDOWS_PHONE_APP
        public static async Task<Protocol> CreateProtocolAsync(Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService peer)
        {
            StreamSocket socket = new StreamSocket();
            await socket.ConnectAsync(peer.ConnectionHostName, new Guid(0x00001101, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB).ToString("B"), SocketProtectionLevel.PlainSocket);
            return new Protocol(socket);
        }
#endif

        public void StartRun()
        {
            if (this._isRunning) return;

            this._isRunning = true;

            this.Run(null);
        }

        public void StopRun()
        {
            this._isRunning = false;
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
        /// Sends a message to the Pebble.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>An async task to wait</returns>
        public Task WriteMessage(P3bbleMessage message)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    this._mutex.WaitOne();

                    byte[] package = message.ToBuffer();

                    ServiceLocator.Logger.WriteLine("<< SEND MESSAGE FOR ENDPOINT " + ((Endpoint)message.Endpoint).ToString() + " (" + ((int)message.Endpoint).ToString() + ")");
                    ServiceLocator.Logger.WriteLine("<< PAYLOAD: " + BitConverter.ToString(package).Replace("-", ":"));

                    System.Diagnostics.Debug.WriteLine("<< SEND MESSAGE FOR ENDPOINT " + ((Endpoint)message.Endpoint).ToString() + " (" + ((int)message.Endpoint).ToString() + ")");
                    System.Diagnostics.Debug.WriteLine("<< PAYLOAD: " + BitConverter.ToString(package).Replace("-",":"));

                    this._writer.WriteBytes(package);
                    this._writer.StoreAsync().AsTask().Wait();

                    this._mutex.ReleaseMutex();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("<< EXCEPTION: " + e.Message);
                }
            });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _isRunning = false;
            
            if (this._writer != null)
            {
                this._writer.Dispose();
                this._writer = null;
            }

            if (this._reader != null)
            {
                this._reader.Dispose();
                this._reader = null;
            }

            if (this._socket != null)
            {
                this._socket.Dispose();
                this._socket = null;
            }

            if (this._mutex != null)
            {
                this._mutex.Dispose();
            }
        }

#if NETFX_CORE  && !WINDOWS_PHONE_APP
        private void Run(object host)
        {
            Task.Factory.StartNew(
                async () =>
            {
#else
        private async void Run(object host)
        {
#endif
            //return;

            while (this._isRunning)
            {
                await ReceiveAndProcessMessage();
                
                await Task.Delay(100);
#if NETFX_CORE  && !WINDOWS_PHONE_APP
                    await Task.Delay(100);
#endif
            
            }
#if NETFX_CORE  && !WINDOWS_PHONE_APP
            },
            TaskCreationOptions.LongRunning);
#endif
        }

        public async Task ReceiveAndProcessMessage()
        {
            P3bbleMessage msg;

            msg = await ReceiveMessage(0);

            if (msg != null)
            {
                if (this.MessageReceived != null)
                {
                    this.MessageReceived(msg);
                }
            }
        }       


        public async Task<bool> ReceiveAcknowledgement()
        {
            var readMutex = new AsyncLock();
            bool Result = false;

            try
            {
                //while (!Result)
                {
                    await this._reader.LoadAsync(7);

                    using (await readMutex.LockAsync())
                    {
                        // this._mutex.WaitOne();

                        if (this._reader.UnconsumedBufferLength == 7)
                        {
                            byte[] payloadlength = new byte[2];
                            byte[] endpoint = new byte[2];
                            byte[] messageid = new byte[2];
                            byte[] result = new byte[1];


                            IBuffer buffer = this._reader.ReadBuffer(this._reader.UnconsumedBufferLength);

                            using (var dr = DataReader.FromBuffer(buffer))
                            {
                                dr.ReadBytes(payloadlength);
                                dr.ReadBytes(endpoint);
                                dr.ReadBytes(messageid);
                                dr.ReadBytes(result);
                            }

                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(payloadlength);
                                Array.Reverse(endpoint);
                            }

                            int MessageId = BitConverter.ToUInt16(messageid, 0);
                            int EndpointId = BitConverter.ToUInt16(endpoint, 0);
                            int PayloadLength = BitConverter.ToUInt16(payloadlength, 0);
                            Result = (result[0] == 0x01);

                            System.Diagnostics.Debug.WriteLine(">> RECEIVED ACKNOWLEDGMENT: " + (Result).ToString() + " (" + MessageId.ToString() + ")");
                            System.Diagnostics.Debug.WriteLine(">>                ENDPOINT: " + (EndpointId).ToString() + " - LENGTH: " + PayloadLength.ToString());
                        }
                        else
                        {

                            System.Diagnostics.Debug.WriteLine(">> RECEIVED UNKNOWN MESSAGE");

                            await ClearMessageBuffer();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(">> RECEIVED ACKNOWLEDGMENT EXCEPTION:" + e.Message);
            
                ClearMessageBuffer().Wait();
            }

            return Result;
        }

        /// <summary>
        /// Clear the message buffer. 
        /// </summary>
        /// <returns></returns>
        public async Task ClearMessageBuffer()
        {
            var readMutex = new AsyncLock();
            
            try
                {
                    if (this._reader.UnconsumedBufferLength == 0)  await this._reader.LoadAsync(1024);

                    using (await readMutex.LockAsync())
                    {
                        uint payloadLength = 1024;

                        byte[] payload = new byte[1024];
                        System.Diagnostics.Debug.WriteLine(">> UNCONSUMED BUFFER LENGTH: " + this._reader.UnconsumedBufferLength);
                        if (this._reader.UnconsumedBufferLength != payloadLength) payload = new byte[this._reader.UnconsumedBufferLength];
                        this._reader.ReadBytes(payload);

                        System.Diagnostics.Debug.WriteLine(">> PAYLOAD: " + BitConverter.ToString(payload));
                    }
                }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("ClearMessageBuffer: " + e.StackTrace);
            }
        }

        private int NoMessageCount;
        /// <summary>
        /// Receive message from Pebble
        /// </summary>
        /// <param name="MinMessageSize"></param>
        /// <returns></returns>
        public async Task<P3bbleMessage> ReceiveMessage(uint MinMessageSize)
        {
            var readMutex = new AsyncLock();
            
            try
                {
                    await this._reader.LoadAsync(4);

                    System.Diagnostics.Debug.WriteLine("[message available]");
                    using (await readMutex.LockAsync())
                    {
                       // this._mutex.WaitOne();

                        System.Diagnostics.Debug.WriteLine("[message unlocked]");
                        uint payloadLength;
                        uint endpoint;

                        if (this._reader.UnconsumedBufferLength > 0)
                        {
                            NoMessageCount = 0;


                            IBuffer buffer = this._reader.ReadBuffer(this._reader.UnconsumedBufferLength);

                            this.GetLengthAndEndpoint(buffer, out payloadLength, out endpoint);

                            System.Diagnostics.Debug.WriteLine(">> RECEIVED MESSAGE FOR ENDPOINT: " + ((Endpoint)endpoint).ToString() + " (" + endpoint.ToString() + ") - " + payloadLength.ToString() + " bytes");

                            if (endpoint > 0 && payloadLength > 0 && payloadLength >= MinMessageSize)
                            {
                                payloadLength = Math.Max(payloadLength, MinMessageSize);
                                byte[] payload = new byte[payloadLength];
                                await this._reader.LoadAsync(payloadLength);
                                System.Diagnostics.Debug.WriteLine(">> UNCONSUMED BUFFER LENGTH: " + this._reader.UnconsumedBufferLength);
                                if (this._reader.UnconsumedBufferLength != payloadLength) payload = new byte[this._reader.UnconsumedBufferLength];
                                this._reader.ReadBytes(payload);

                                P3bbleMessage msg = this.ReadMessage(payload, endpoint);

                                System.Diagnostics.Debug.WriteLine(">> RECEIVED MESSAGE FOR ENDPOINT: " + ((Endpoint)endpoint).ToString() + " (" + endpoint.ToString() + ") - " + payloadLength.ToString() + " bytes");
                                System.Diagnostics.Debug.WriteLine(">> PAYLOAD: " + BitConverter.ToString(payload));

                                return msg;
                            }
                            else
                            {   
                                ServiceLocator.Logger.WriteLine(">> RECEIVED MESSAGE WITH BAD ENDPOINT OR LENGTH: " + endpoint.ToString() + ", " + payloadLength.ToString());
                                System.Diagnostics.Debug.WriteLine(">> RECEIVED MESSAGE WITH BAD ENDPOINT OR LENGTH: " + endpoint.ToString() + ", " + payloadLength.ToString());

                                await ClearMessageBuffer();
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(">> NO MESSAGE");

                            NoMessageCount++;

                            if (NoMessageCount > 25)
                            {
                                //Assume bluetooth disconnect
                                StopRun();
                                OnDisconnect(EventArgs.Empty);
                            }
                        }

                       // this._mutex.ReleaseMutex();
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("ReceiveMessage: " + e.Message);

                    if (e.HResult == -2147023901 || e.HResult == -2147014843)
                    {
                        StopRun();
                        OnDisconnect(EventArgs.Empty);
                    }



                }

            return null;

        }

        /// <summary>
        /// Retrieve the lenght and endpoint from the Pebble Time Message
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="payloadLength"></param>
        /// <param name="endpoint"></param>
        private void GetLengthAndEndpoint(IBuffer buffer, out uint payloadLength, out uint endpoint)
        {
            if (buffer.Length != 4)
            {
                payloadLength = 0;
                endpoint = 0;
                return;
            }

            byte[] payloadSize = new byte[2];
            byte[] endpo = new byte[2];

            using (var dr = DataReader.FromBuffer(buffer))
            {
                dr.ReadBytes(payloadSize);
                dr.ReadBytes(endpo);
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(payloadSize);
                Array.Reverse(endpo);
            }

            payloadLength = BitConverter.ToUInt16(payloadSize, 0);
            endpoint = BitConverter.ToUInt16(endpo, 0);
        }

        private P3bbleMessage ReadMessage(byte[] payloadContent, uint endpoint)
        {
            List<byte> lstBytes = payloadContent.ToList();
            byte[] array = lstBytes.ToArray();
            ServiceLocator.Logger.WriteLine(">> PAYLOAD: " + BitConverter.ToString(array));
            return P3bbleMessage.CreateMessage((Endpoint)endpoint, lstBytes);
        }

        private IBuffer GetBufferFromByteArray(byte[] package)
        {
            using (DataWriter dw = new DataWriter())
            {
                dw.WriteBytes(package);
                return dw.DetachBuffer();
            }
        }
    }
}
