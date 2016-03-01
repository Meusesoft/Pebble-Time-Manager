using P3bble;
using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.Connector;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Tennis_Statistics.ViewModels;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using System.Collections.Generic;
using P3bble.Messages;

namespace BackgroundTasks
{
    public sealed class BackgroundCommunication : IBackgroundTask
    {
        private class DelayDisconnect
        {
            public int Delay; //In milliseconds
            public PebbleConnector.Initiator Initiator; //Connection to be delayed
        }

        private Pebble_Time_Manager.Connector.PaceHandler _PaceHandler = null;
        private Tennis_Statistics.ViewModels.vmMatch TennisMatch = null;
        private PebbleConnector _pc;
        private Pebble_Time_Manager.Connector.TimeLineSynchronizer _tlsynchronizer = null;
        private ObservableCollection<String> Log;
        private List<DelayDisconnect> _DelayDisonnect = new List<DelayDisconnect>();
        private int Handler;
        private int ReconnectDelay;

        /// <summary>
        /// Main thread for communication with pebble on a background task.
        /// 
        /// Reading the state of the process is possible via the local settings BackgroundIsRunning. Synchronization via
        /// Mutex is not possible due to heavy use of await statements. The mutex will be abandoned and releasing gives
        /// an exception.
        /// </summary>
        /// <param name="taskInstance"></param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var def = taskInstance.GetDeferral();
            var localSettings = ApplicationData.Current.LocalSettings;

            Handler = -1;
            localSettings.Values[Constants.BackgroundCommunicatieError] = (int)BCState.OK;

            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Start BackgroundCommunication");

                    /*DeviceUseDetails details = (DeviceUseDetails)taskInstance.TriggerDetails;
                    System.Diagnostics.Debug.WriteLine("BackgroundCommunication Arguments: " + details.Arguments);
                    System.Diagnostics.Debug.WriteLine("BackgroundCommunication DeviceId: " + details.DeviceId);*/

                    //Connect
                    _pc = PebbleConnector.GetInstance();
                    Handler = await _pc.Connect(-1);

                    if (_pc.IsConnected)
                    {
                        AddToLog("Connection made with Pebble Time");

                        Log = new ObservableCollection<string>();
                        Log.CollectionChanged += Log_CollectionChanged;
                        _pc.Pebble.Log = Log;
                        _pc.StartReceivingMessages();
                        _pc.disconnectEventHandler += _pc_disconnectEventHandler;
                        _pc.Pebble._protocol.MessageReceived += AppMessageReceived;

                        bool Continue = true;

                        //initialise settings

                        while (Continue)
                        {
                            try
                            {
                                localSettings.Values[Constants.BackgroundCommunicatieIsRunning] = true;

                                if (_pc.IsConnected)
                                {
                                    await PaceHandler();

                                    await TennisHandler();

                                    await Wipe();

                                    await Synchronize();

                                    await Select();

                                    await Launch();

                                    await AddItem();
                                }
                                else
                                {
                                    await Reconnect();
                                }
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine(e.Message + e.StackTrace);
                            }

                            await ProcessDelay();

                            //Check if continue
                            Continue = ((int)localSettings.Values[Constants.BackgroundCommunicatieContinue] != 0);
                        }

                        await PaceHandlerCleanup();

                        localSettings.Values[Constants.BackgroundTennis] = false;
                        localSettings.Values[Constants.BackgroundPace] = false;

                        _pc.Pebble._protocol.MessageReceived -= AppMessageReceived;
                    }
                    else
                    {
                        AddToLog("Connection with Pebble Time Failed.");
                        localSettings.Values[Constants.BackgroundCommunicatieError] = (int)BCState.ConnectionFailed;

                        if (_pc.LastError.Length > 0)
                        {
                            AddToLog(_pc.LastError);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception BackgroundCommunication: " + e.Message);
                    localSettings.Values[Constants.BackgroundCommunicatieError] = (int)BCState.ExceptionOccurred;
                }
                finally
                {
                    localSettings.Values[Constants.BackgroundCommunicatieIsRunning] = false;
                    
                    //Disconnect
                    if (_pc.IsConnected)
                    {
                        _pc.Disconnect(Handler);

                        AddToLog("Disconnected from Pebble Time");
                    }
                }

                System.Diagnostics.Debug.WriteLine("End BackgroundCommunication");
            }

            def.Complete();
        }

        private void _pc_disconnectEventHandler(object sender, EventArgs e)
        {
            //Disconnected from Pebble
            //PebbleConnector.ClearBackgroundTaskRunningStatus(PebbleConnector.Initiator.Manual);

            _pc.Disconnect(Handler);

            AddToLog("Connection lost with Pebble Time. Retrying in 10 seconds.");

            ReconnectDelay = 10000;
        }

        /// <summary>
        /// Process the tennis message
        /// </summary>
        /// <param name="message"></param>
        private void AppMessageReceived(P3bbleMessage message)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            switch (message.Endpoint)
            {
                case P3bble.Constants.Endpoint.WatchFaceSelect:

                    WatchFaceMessage _watchFaceMessage = (WatchFaceMessage)message;

                    System.Diagnostics.Debug.WriteLine("WatchFaceMessage received: " + _watchFaceMessage.CurrentWatchFace.ToString());

                    break;

                case P3bble.Constants.Endpoint.ApplicationMessage:

                    P3bble.Messages.AppMessage _appMessage = (P3bble.Messages.AppMessage)message;

                    if (_appMessage.AppUuid != Guid.Parse("51a56b50-f87d-ce41-a0ff-30d03a88fa8d"))
                    {
                        System.Diagnostics.Debug.WriteLine("AppMessage received: " + _appMessage.AppUuid.ToString());
                    }

                    break;
            }
        }

        private async Task Reconnect()
        {
            ReconnectDelay -= 500;

            if (ReconnectDelay <= 0)
            {
                Handler = await _pc.Connect(-1);

                if (_pc.IsConnected)
                {
                    AddToLog("Reconnected with Pebble Time");

                    _pc.StartReceivingMessages();
                }
                else
                {
                    AddToLog("Reconnection failed with Pebble Time. Retrying in 5 minutes");

                    ReconnectDelay = 300000;
#if DEBUG
                    AddToLog("Debug mode. Retrying in 5 seconds");

                    ReconnectDelay = 300;
#endif
                }
            }
        }

        private async Task ProcessDelay()
        {
            //Wait a second
            await System.Threading.Tasks.Task.Delay(500);

            //Process delays
            if (_DelayDisonnect.Count > 0)
            {
                foreach (DelayDisconnect item in _DelayDisonnect)
                {
                    item.Delay -= 500;
                    if (item.Delay <= 0) PebbleConnector.ClearBackgroundTaskRunningStatus(item.Initiator);
                }

                _DelayDisonnect.RemoveAll(x => x.Delay <= 0);

                if (_DelayDisonnect.Count == 0) PebbleConnector.ClearBackgroundTaskRunningStatus(PebbleConnector.Initiator.Delay);
            }
        }

        private void AddProcessDelay(int milliSeconds, PebbleConnector.Initiator Initator)
        {
            DelayDisconnect _new = new DelayDisconnect();
            _new.Delay = milliSeconds;
            _new.Initiator = Initator;

            _DelayDisonnect.Add(_new);

            PebbleConnector.ClearBackgroundTaskRunningStatus(Initator);
            PebbleConnector.SetBackgroundTaskRunningStatus(PebbleConnector.Initiator.Delay);
        }


        #region Pace

        /// <summary>
        /// Pace handler
        /// </summary>
        /// <returns></returns>
        private async Task PaceHandler()
        {
            //initialise settings
            var localSettings = ApplicationData.Current.LocalSettings;
            var roamingSettings = ApplicationData.Current.RoamingSettings;

            if (localSettings.Values.Keys.Contains(Constants.BackgroundPace) &&
                (bool)localSettings.Values[Constants.BackgroundPace])
            {
                if (!PebbleConnector.IsBackgroundTaskRunningStatusSet(PebbleConnector.Initiator.Pace))
                {
                    await PaceHandlerCleanup();
                }
                else
                { 
                    if (_PaceHandler == null)
                    {

                        localSettings.Values[Constants.PaceSwitchPaused] = false;
                        localSettings.Values[Constants.PaceGPX] = false;
                        localSettings.Values[Constants.Miles] = !System.Globalization.RegionInfo.CurrentRegion.IsMetric;
                        if (roamingSettings.Values.Keys.Contains(Constants.Miles))
                            localSettings.Values[Constants.Miles] = roamingSettings.Values[Constants.Miles];
                        localSettings.Values[Constants.BackgroundCommunicatieIsRunning] = true;

                        //initialise pace handler
                        _PaceHandler = new Pebble_Time_Manager.Connector.PaceHandler();
                        _PaceHandler.Miles = (bool)localSettings.Values[Constants.Miles];
                        _PaceHandler.Start();
                        await _PaceHandler.UpdatePosition();
                    }

                    //Update pace
                    _PaceHandler.UpdatePosition();

                    if (!_PaceHandler.Paused)
                    {
                        //Send Pebble Message
                        await _PaceHandler.SendPebblePaceMessage();

                        localSettings.Values[Constants.PaceDistance] = string.Format("{0:f2}", _PaceHandler.Distance);
                        localSettings.Values[Constants.PaceDuration] = _PaceHandler.Duration.ToString(@"m\:ss");
                        localSettings.Values[Constants.PacePace] = _PaceHandler.Pace.ToString(@"m\:ss");
                        if (_PaceHandler.Pace.TotalMinutes > 45) localSettings.Values[Constants.PacePace] = "-";
                        if (_PaceHandler.Pace.TotalMilliseconds == 0) localSettings.Values[Constants.PacePace] = "-";

                    }

                    localSettings.Values[Constants.PacePaused] = _PaceHandler.Paused;

                    //Check the switch pause/resume
                    if (localSettings.Values.Keys.Contains(Constants.PaceSwitchPaused))
                    {
                        bool SwitchPaused = (bool)localSettings.Values[Constants.PaceSwitchPaused];
                        localSettings.Values[Constants.PaceSwitchPaused] = false;
                        if (SwitchPaused)
                        {
                            if (_PaceHandler.Paused)
                            {
                                await _PaceHandler.Resume();
                            }
                            else
                            {
                                await _PaceHandler.Pause();
                            }
                        }
                    }
                }
            }
        }

        private async Task PaceHandlerCleanup()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.Keys.Contains(Constants.BackgroundPace) &&
                (bool)localSettings.Values[Constants.BackgroundPace] &&
                    _PaceHandler != null)
            {
                    localSettings.Values[Constants.BackgroundPace] = false;

                    _PaceHandler.Stop();

                    System.Diagnostics.Debug.WriteLine("Create GPX file");

                    await _PaceHandler.CreateGPX();

                    //reset local settings
                    localSettings.Values[Constants.PaceGPX] = true;
            }
        }

        #endregion

        #region Tennis

        /// <summary>
        /// Tennis handler
        /// </summary>
        private async Task TennisHandler()
        {
            if (!PebbleConnector.IsBackgroundTaskRunningStatusSet(PebbleConnector.Initiator.Tennis)) return;

            //initialise settings
            var localSettings = ApplicationData.Current.LocalSettings;
            var roamingSettings = ApplicationData.Current.RoamingSettings;

            if (localSettings.Values.Keys.Contains(Constants.BackgroundTennis) &&
                    (bool)localSettings.Values[Constants.BackgroundTennis])
            {
                //Initiate tennis match
                if (TennisMatch == null)
                {
                    TennisMatch = new vmMatch();
                    await TennisMatch.Load("tennis_pebble.xml");

                    if (TennisMatch.Match == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Initialize Tennis Match in background.");
                        vmNewMatch _vmNewMatch = await vmNewMatch.Load();

                        TennisMatch = new vmMatch();
                        TennisMatch.Start(_vmNewMatch);
                    }
                    else
                    {
                        TennisMatch.NewPoint();
                    }

                    vmMatchState _State = new vmMatchState();
                    _State.Fill(TennisMatch);
                    await Tennis_Statistics.Helpers.LocalStorage.Save(_State.Serialize(), "tennismatchstate.json", false);
                    localSettings.Values[Constants.TennisState] = "1";

                    System.Diagnostics.Debug.WriteLine("Tennis Match state stored");

                    _pc.Pebble._protocol.MessageReceived += this.TennisMessageReceived;

                    Guid TennisWatchApp = Guid.Parse(Constants.TennisAppGuid);
                    await _pc.Launch(TennisWatchApp);

                    await SendPebbleTennisScore(_State, TennisMatch);


                    // await SendPebbleTennisScore(_State);
                }
            }

            //A new state requested
            if (localSettings.Values.Keys.Contains(Constants.TennisState) &&
                (string)localSettings.Values[Constants.TennisState] == "2")
            {
                await SaveMatchState();
            }

            //Check for and process command
            if (localSettings.Values.Keys.Contains(Constants.TennisCommand)
                && TennisMatch != null)
            {
                String Command = (String)localSettings.Values[Constants.TennisCommand];
                System.Diagnostics.Debug.WriteLine("Processing tennis command: " + Command);

                switch (Command)
                {
                    case "switch":

                        TennisMatch.cmdSwitchServer.Execute(null);

                        AddToLog("Tennis: switch command");

                        break;

                    case "stop":

                        TennisMatch.Terminate();

                        PebbleConnector.ClearBackgroundTaskRunningStatus(PebbleConnector.Initiator.Tennis);

                        AddToLog("Tennis: stop command");

                        break;

                    case "suspend":

                        TennisMatch.Pause();

                        PebbleConnector.ClearBackgroundTaskRunningStatus(PebbleConnector.Initiator.Tennis);

                        AddToLog("Tennis: suspend command");

                        break;

                    case "extend":

                        TennisMatch.cmdExtendMatch.Execute(null);

                        AddToLog("Tennis: extend command");

                        break;
                }

                localSettings.Values.Remove(Constants.TennisCommand);

                await SaveMatchState();
            }
        }

        /// <summary>
        /// Process the tennis message
        /// </summary>
        /// <param name="message"></param>
        private async void TennisMessageReceived(P3bbleMessage message)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (message.Endpoint == P3bble.Constants.Endpoint.ApplicationMessage)
            {
                P3bble.Messages.AppMessage _tennisMessage = (P3bble.Messages.AppMessage)message;

                if (_tennisMessage.AppUuid == Guid.Parse("51a56b50-f87d-ce41-a0ff-30d03a88fa8d"))
                {
                    System.Diagnostics.Debug.WriteLine("TennisMessage received");

                    P3bble.Messages.AppMessage AckMessage = new P3bble.Messages.AppMessage();
                    AckMessage.Command = P3bble.Messages.AppCommand.Ack;
                    AckMessage.TransactionId = _tennisMessage.TransactionId;
                    await _pc.Pebble._protocol.WriteMessage(AckMessage);
                    System.Diagnostics.Debug.WriteLine("TennisMessage Acknowledged");

                    if (TennisMatch.Paused)
                    {
                        System.Diagnostics.Debug.WriteLine("Match suspended; message ignored.");
                        return;
                    }

                    if (_tennisMessage.Content.ContainsKey(1))
                    {
                        int ActionCode = (int)_tennisMessage.Content[1];

                        System.Diagnostics.Debug.WriteLine(String.Format("Tennis action: {0}", ActionCode));

                        switch (ActionCode)
                        {
                            case 0:

                                TennisMatch.ProcessAction("CommandLose");

                                break;

                            case 1:

                                TennisMatch.ProcessAction("CommandWin");

                                break;

                            case 2:

                                TennisMatch.ProcessAction("CommandSecondServe");

                                System.Diagnostics.Debug.WriteLine("CommandSecondServe");

                                break;

                            case 3:

                                TennisMatch.ProcessAction("CommandAce");

                                break;

                            case 5:

                                TennisMatch.ProcessAction("CommandDoubleFault");

                                System.Diagnostics.Debug.WriteLine("CommandDoubleFault");

                                break;

                            case 4:

                                TennisMatch.ProcessAction("CommandUndo");

                                break;

                            case 16:

                                TennisMatch.ProcessAction("CommandExtend");

                                break;

                            case 128:

                                string log = (string)_tennisMessage.Content[2];
                                System.Diagnostics.Debug.WriteLine(log);

                                break;

                            case 254:

                                //request new state
                                System.Diagnostics.Debug.WriteLine("Tennis: new state requested");

                                break;
                        }

                    await SaveMatchState();
                    }
                }
            }
        }

        private async Task SaveMatchState()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            vmMatchState _State = new vmMatchState();

            await TennisMatch.Save("tennis_pebble.xml");

            _State.Fill(TennisMatch);
            await Tennis_Statistics.Helpers.LocalStorage.Save(_State.Serialize(), "tennismatchstate.json", false);

            localSettings.Values[Constants.TennisState] = "1";

            await SendPebbleTennisScore(_State, TennisMatch);
        }

        /// <summary>
        /// Send Tennis score to Pebble Time
        /// </summary>
        /// <param name="_State"></param>
        /// <returns></returns>
        private async Task SendPebbleTennisScore(vmMatchState _State, vmMatch _Match)
        {
            String GameScore;
            String SetScore;
            String Sets = "";
            String Status = "1";

            if (_State.Server == 0 || _State.CurrentSetScore.IsTiebreak)
            {
                GameScore = String.Format("{0} - {1}", _State.ScorePlayer1, _State.ScorePlayer2);
            }
            else
            {
                GameScore = String.Format("{0} - {1}", _State.ScorePlayer2, _State.ScorePlayer1);
            }

            SetScore = String.Format("S:{0}-{1} G:{2}-{3}",
                    _State.TotalSets.Score1, _State.TotalSets.Score2,
                    _State.CurrentSetScore.Score1, _State.CurrentSetScore.Score2);

            if (_State.Winner == 1) GameScore = "WON";
            if (_State.Winner == 2) GameScore = "LOST";
            if (_State.Winner != 0)
            {
                Status = "2";
                Sets = _Match.Match.PrintableScore().Replace(", ", ";") + ";";
                SetScore = "Completed";
            }

            //Send message
            if (_pc.IsConnected)
            {
                await _pc.Pebble.SendTennisMessage(GameScore, SetScore, Sets, Status);
            }

            //Write debug line
            System.Diagnostics.Debug.WriteLine(String.Format("Tennis score sent: {0}, {1}, {2}, {3}", GameScore, SetScore, Sets, Status));
        }

        #endregion

        #region Wipe, Synchronize and Select app
        /// <summary>
        /// Wipe the Pebble Time
        /// </summary>
        private async Task Wipe()
        {
            if (PebbleConnector.IsBackgroundTaskRunningStatusSet(PebbleConnector.Initiator.Reset))
            {
                _tlsynchronizer = new TimeLineSynchronizer();
                _tlsynchronizer.Log.CollectionChanged += Log_CollectionChanged;
                await _tlsynchronizer.Wipe();

                AddProcessDelay(30000, PebbleConnector.Initiator.Reset); 
            }
        }

        private async Task Synchronize()
        {
            if (PebbleConnector.IsBackgroundTaskRunningStatusSet(PebbleConnector.Initiator.Synchronize))
            {
                _tlsynchronizer = new TimeLineSynchronizer();
                _tlsynchronizer.Log.CollectionChanged += Log_CollectionChanged;
                await _tlsynchronizer.Synchronize();

                PebbleConnector.ClearBackgroundTaskRunningStatus(PebbleConnector.Initiator.Synchronize);
            }
        }

        private async Task Select()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (PebbleConnector.IsBackgroundTaskRunningStatusSet(PebbleConnector.Initiator.Select))
            {
                String item = (string)localSettings.Values[Constants.BackgroundCommunicatieSelectItem];
                Guid itemGuid = Guid.Parse(item);

                await _pc.SelectFace(itemGuid);

                AddToLog("Request send to select item: " + item);

                AddProcessDelay(30000, PebbleConnector.Initiator.Select);
            }
        }

        private async Task Launch()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (PebbleConnector.IsBackgroundTaskRunningStatusSet(PebbleConnector.Initiator.Launch))
            {
                String item = (string)localSettings.Values[Constants.BackgroundCommunicatieLaunchItem];
                Guid itemGuid = Guid.Parse(item);

                await _pc.Launch(itemGuid);

                AddToLog("Request send to launch app: " + item);

                AddProcessDelay(30000, PebbleConnector.Initiator.Launch);
            }
        }

        private async Task AddItem()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (PebbleConnector.IsBackgroundTaskRunningStatusSet(PebbleConnector.Initiator.AddItem))
            {
                String Filename = (String)localSettings.Values[Constants.BackgroundCommunicatieDownloadedItem];

                var _newItem = await Pebble_Time_Manager.WatchItems.WatchItem.Load(Filename);

                await _pc.Pebble.AddWatchItemAsync(_newItem);

                AddToLog("New app send: " + _newItem.Name);

                PebbleConnector.ClearBackgroundTaskRunningStatus(PebbleConnector.Initiator.AddItem);
            }
        }

        #endregion

        #region Logging

        /// <summary>
        /// Add message to system log
        /// </summary>
        /// <param name="Message"></param>
        public void AddToLog(String Message)
        {
            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                String Log = "";

                if (localSettings.Values.ContainsKey(Constants.BackgroundCommunicatieLog))
                {
                    Log = (String)localSettings.Values[Constants.BackgroundCommunicatieLog];
                }

                Log += Message + Environment.NewLine;

                if (Log.Length > 512)
                {
                    Log = "";

                    string[] lines = Log.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    int i = lines.Length;
                    i--;

                    do
                    {
                        Log += lines[i];
                        Log += Environment.NewLine;
                        i--;
                    }
                    while (Log.Length < 512 && i>0);
                }

                localSettings.Values[Constants.BackgroundCommunicatieLog] = Log;
            }
            catch (Exception e)
            {
                ApplicationData.Current.LocalSettings.Values[Constants.BackgroundCommunicatieLog] = e.Message;
            }
        }

        private void Log_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (string Item in e.NewItems)
                {
                    AddToLog(Item);
                }
            }
        }

        #endregion
    }
}
