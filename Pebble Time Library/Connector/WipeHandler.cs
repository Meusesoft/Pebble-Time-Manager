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
using Pebble_Time_Manager.Connector;
using Pebble_Time_Manager.Weather;
using Pebble_Time_Manager.Calender;
using Windows.UI.Core;
using P3bble;

namespace Pebble_Time_Library.Connector
{
    public class WipeHandler
    {
        #region Constructor

        public WipeHandler(ObservableCollection<String> Log, TimeLineSynchronizer TimeLineSynchronizer)
        {
            _pc = PebbleConnector.GetInstance();

            _Log = Log;
            _TimeLineSynchronizer = TimeLineSynchronizer;
            _ConnectionToken = -1;
        }

        #endregion


        #region Fields

        private int _ConnectionToken;
        private PebbleConnector _pc;
        private ObservableCollection<String> _Log;
        public TimeLineSynchronizer _TimeLineSynchronizer;

        #endregion

        #region Properties

        public bool IsConnected
        {
            get
            {
                return _ConnectionToken != -1;
            }
        }

        #endregion


        /// <summary>
        /// Process the tennis message
        /// </summary>
        /// <param name="message"></param>
        private void AppMessageReceived(P3bbleMessage message)
        {
            if (message.Endpoint == P3bble.Constants.Endpoint.StandardV3)
            {
                StandardV3Message _SV3M = (StandardV3Message)message;

                if (MessageIdentifier == _SV3M.Identifier)  MessageReceived = true;
            }    
        }

        private bool MessageReceived = false;
        private int MessageIdentifier;

        private async Task WaitForMessage(int Identifier)
        {
            MessageIdentifier = Identifier;

            MessageReceived = false;

            while (!MessageReceived)
            {
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Reset the watch / timeline
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Wipe()
        {
            try
            {
                if (await _pc.IsBackgroundTaskRunning())
                {
                    _Log.Add("Stop current activities");

                    _pc.StopBackgroundTask(PebbleConnector.Initiator.Manual);

                    await _pc.WaitBackgroundTaskStopped(5);
                }

                await Connect();

                String Message;

                //Get current watch face ID
                //WatchFaceMessage _wfm = new WatchFaceMessage();
                Guid CurrentWatchFace = _pc.Pebble.CurrentWatchFace; //await _pc.Pebble.RequestWatchFaceMessageAsync(_wfm);

                _Log.Add(String.Format("Current watch face: {0}", CurrentWatchFace));
                System.Diagnostics.Debug.WriteLine(String.Format("Current watch face: {0}", CurrentWatchFace));

                _pc.StartReceivingMessages();
                _pc.Pebble._protocol.MessageReceived += AppMessageReceived;

                //Set the watch face
                byte[] TicTocByte = new byte[16] { 0x8F, 0x3C, 0x86, 0x86, 0x31, 0xA1, 0x4F, 0x5F, 0x91, 0xF5, 0x01, 0x60, 0x0C, 0x9B, 0xDC, 0x59 };
                Guid TicTocGuid = new Guid(TicTocByte);

                if (CurrentWatchFace != Guid.Empty && CurrentWatchFace != TicTocGuid)
                {

                    WatchFaceSelectMessage _wsm = new WatchFaceSelectMessage(CurrentWatchFace, TicTocGuid);
                    await _pc.Pebble.WriteMessageAsync(_wsm);

                    System.Diagnostics.Debug.WriteLine(String.Format("TicToc Watch fase set as current"));
                }

                StandardV3Message _SV3M;

                for (int i = 1; i <= 9; i++)
                {
                    _SV3M = new StandardV3Message(_pc.Pebble.GetNextMessageIdentifier(), (byte)i);
                    await _pc.Pebble._protocol.WriteMessage(_SV3M);
                    await WaitForMessage(_SV3M.Identifier);
                }


                /*Message = "00:04:b1:db:05:d0:11:01";
                await _pc.WriteMessage(Message);
                await WaitForMessage("d0:11");

                Message = "00:04:b1:db:05:65:54:02";
                await _pc.WriteMessage(Message);
                await WaitForMessage("65:54");

                Message = "00:04:b1:db:05:8c:b5:03";
                await _pc.WriteMessage(Message);
                await WaitForMessage("8c:b5");

                Message = "00:04:b1:db:05:8c:b6:04";
                await _pc.WriteMessage(Message);
                await WaitForMessage("8c:b6");

                Message = "00:04:b1:db:05:8c:b7:05";
                await _pc.WriteMessage(Message);
                await WaitForMessage("8c:b7");

                Message = "00:04:b1:db:05:8c:b8:06";
                await _pc.WriteMessage(Message);
                await WaitForMessage("8c:b8");

                Message = "00:04:b1:db:05:8c:b9:07";
                await _pc.WriteMessage(Message);
                await WaitForMessage("8c:b9");

                Message = "00:04:b1:db:05:8c:ba:08";
                await _pc.WriteMessage(Message);
                await WaitForMessage("8c:ba");

                Message = "00:04:b1:db:05:8c:b0:09";
                await _pc.WriteMessage(Message);
                await WaitForMessage("8c:b0");*/

                _Log.Add("Pebble Time wiped.");

                System.Diagnostics.Debug.WriteLine("Pebble Time wiped.");

                foreach (var item in _pc.Pebble.WatchItems)
                {
                    WatchItemAddMessage _waam = new WatchItemAddMessage(_pc.Pebble.GetNextMessageIdentifier(), item);
                    await _pc.Pebble._protocol.WriteMessage(_waam);
                    await WaitForMessage(_waam.Transaction);

                    switch (item.Type)
                    {
                        case WatchItemType.WatchFace:

                            _Log.Add(String.Format("Watch face {0} added.", item.Name));
                            System.Diagnostics.Debug.WriteLine(String.Format("Watch face {0} added.", item.Name));

                            break;

                        case WatchItemType.WatchApp:

                            _Log.Add(String.Format("Watch app {0} added.", item.Name));
                            System.Diagnostics.Debug.WriteLine(String.Format("Watch app {0} added.", item.Name));

                            break;
                    }
                }

                _Log.Add("Watch items restored");
                System.Diagnostics.Debug.WriteLine("Watch items restored");

                //Clear caches
                Weather.ClearCache();
                await Calender.ClearCache();

                _Log.Add("Cache cleared on phone");
                System.Diagnostics.Debug.WriteLine("Cache cleared on phone");

                //Update timeline
                await _TimeLineSynchronizer.Synchronize();

                //Set the watch face
                if (CurrentWatchFace != Guid.Empty && CurrentWatchFace != TicTocGuid)
                {

                    WatchFaceSelectMessage _wsm = new WatchFaceSelectMessage(TicTocGuid, CurrentWatchFace);
                    await _pc.Pebble.WriteMessageAsync(_wsm);

                    _Log.Add(String.Format("Selected watch face: {0}", CurrentWatchFace));
                    System.Diagnostics.Debug.WriteLine(String.Format("Selected watch face: {0}", CurrentWatchFace));

                    _pc.Pebble.ItemSend += Pebble_ItemSend;
                }
                else
                {
                    Disconnect();
                }

                return true;

            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Wipe exception: {0}", exp.Message));
                Disconnect();
            }

            return false;
        }

        async void Pebble_ItemSend(object sender, EventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Disconnect();

                _pc.Pebble.ItemSend -= Pebble_ItemSend;

                _Log.Add("Disconnected");
            });
        }

        /// <summary>
        /// Connect to a pebble
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            bool bNewConnection = false; 

            bNewConnection = !_pc.IsConnected;

            //Connect to the watch
            _ConnectionToken = await _pc.Connect(_ConnectionToken);

            if (!_pc.IsConnected)
            {
                _Log.Add("No connection with Pebble Time.");
                _Log.Add("Already connected or not paired?.");
                throw new Exception("No connection with Pebble Time");
            }

            if (bNewConnection) _Log.Add("Connected");
        }
        
        /// <summary>
        /// Disconnect from the Pebble
        /// </summary>
        public void Disconnect()
        {
            //Disconnect
            _pc.Disconnect(_ConnectionToken);

            _ConnectionToken = -1;

            //if (!_pc.IsConnected) _Log.Add("Disconnected");
        }
    }
}
