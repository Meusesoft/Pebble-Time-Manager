using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Tennis_Statistics.Game_Logic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Tennis_Statistics.ViewModels;
using System.IO;
using System.Text;

namespace Tennis_Statistics.ViewModels
{
    [DataContract]
    public class vmMatchState : INotifyPropertyChanged
    {
        private DispatcherTimer _Timer;

        #region Properties

        /// <summary>
        /// The model of the point currently being played
        /// </summary>
        [DataMember]
        public String CurrentPointDescription { get; set; }

        /// <summary>
        /// The name of player 1
        /// </summary>
        [DataMember]
        public String NamePlayer1 { get; set; }

        /// <summary>
        /// The name of player 2
        /// </summary>
        [DataMember]
        public String NamePlayer2 { get; set; }

        /// <summary>
        /// The current score of player 1
        /// </summary>
        [DataMember]
        public String ScorePlayer1 { get; set; }

        /// <summary>
        /// The current score of player 2
        /// </summary>
        [DataMember]
        public String ScorePlayer2 { get; set; }

        /// <summary>
        /// The status of the match in a string representation
        /// </summary>
        [DataMember]
        public String Status { get; set; }

        /// <summary>
        /// Description of match type in upper case characters
        /// </summary>
        [DataMember]
        public String MatchType { get; set; }

        /// <summary>
        /// String representation of the surface type
        /// </summary>
        [DataMember]
        public String Surface { get; set; }

        /// <summary>
        /// The duration of the current match
        /// </summary>
        [DataMember]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The current server (0 = player 1, 1 = player 2)
        /// </summary>
        [DataMember]
        public int Server { get; set; }

        /// <summary>
        /// The winner (0 = player 1, 2 = player 2)
        /// </summary>
        [DataMember]
        public int Winner { get; set; }

        private bool _InProgress;
        /// <summary>
        /// True if this match is in progress
        /// </summary>
        [DataMember]
        public bool InProgress
        {
            get
            {
                return _InProgress;
            }

            set
            {
                if (_InProgress != value)
                {
                    _InProgress = value;
                    NotifyPropertyChanged("InProgress");
                    NotifyPropertyChanged("Stoppable");
                }
            }
        }

        /// <summary>
        /// True if this match is in progress
        /// </summary>
        [DataMember]
        public bool Completed { get; set; }

        /// <summary>
        /// True if it is possible to extend the match with additional sets
        /// </summary>
        [DataMember]
        public bool IsExtendPossible { get; set; }

        /// <summary>
        /// True, if undo is possible
        /// </summary>
        [DataMember]
        public bool Undo { get; set; }

        /// <summary>
        /// True, if switching server is possible
        /// </summary>
        [DataMember]
        public bool Switch { get; set; }

        [DataMember]
        public string PrintableScore { get; set; }

        /// <summary>
        /// True, if match is paused
        /// </summary>
        private bool _Paused;
        [DataMember]
        public bool Paused
        {
            get
            {
                return _Paused;
            }

            set
            {
                if (Completed)
                {
                    value = false;
                }

                if (Paused != value)
                {
                    _Paused = value;
                    NotifyPropertyChanged("Paused");
                    NotifyPropertyChanged("Stoppable");
                }
            }
        }

        public bool Stoppable
        {
            get
            {
                return !Paused && InProgress;
            }
        }
        
        /// <summary>
        /// The scores of the current set
        /// </summary>
        [DataMember]
        public vmSetScore CurrentSetScore { get; set; }

        /// <summary>
        /// The total sets won by the players.
        /// </summary>
        [DataMember]
        public vmSetScore TotalSets { get; set; }

        [DataMember]
        public vmStatisticsCollection StatisticsCollection { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Fill properties from the current match
        /// </summary>
        /// <param name="_CurrentMatch"></param>
        public void Fill(vmMatch _CurrentMatch)
        {
            CurrentPointDescription = _CurrentMatch.CurrentPointDescription;
            NamePlayer1 = _CurrentMatch.NamePlayer1;
            NamePlayer2 = _CurrentMatch.NamePlayer2;
            ScorePlayer1 = _CurrentMatch.ScorePlayer1;
            ScorePlayer2 = _CurrentMatch.ScorePlayer2;
            PrintableScore = _CurrentMatch.Match.PrintableScore();
            Status = _CurrentMatch.Status;
            MatchType = _CurrentMatch.MatchType;
            Surface = _CurrentMatch.Surface;
            Duration = _CurrentMatch.Duration;
            Server = _CurrentMatch.Server;
            Winner = _CurrentMatch.Winner;
            InProgress = _CurrentMatch.InProgress;
            Completed = _CurrentMatch.Completed;
            IsExtendPossible = _CurrentMatch.IsExtendPossible;
            Undo = _CurrentMatch.Undo;
            Switch = _CurrentMatch.Switch;
            TotalSets = new vmSetScore(_CurrentMatch.TotalSets);
            CurrentSetScore = new vmSetScore(_CurrentMatch.CurrentSetScore);
            StatisticsCollection = _CurrentMatch.StatisticsCollection;

            Notify();
        }

        /// <summary>
        /// Fill properties from the current match
        /// </summary>
        /// <param name="_CurrentMatch"></param>
        public void Fill(vmMatchState _CurrentMatch)
        {
            CurrentPointDescription = _CurrentMatch.CurrentPointDescription;
            NamePlayer1 = _CurrentMatch.NamePlayer1;
            NamePlayer2 = _CurrentMatch.NamePlayer2;
            ScorePlayer1 = _CurrentMatch.ScorePlayer1;
            ScorePlayer2 = _CurrentMatch.ScorePlayer2;
            PrintableScore = _CurrentMatch.PrintableScore;
            Status = _CurrentMatch.Status;
            MatchType = _CurrentMatch.MatchType;
            Surface = _CurrentMatch.Surface;
            Duration = _CurrentMatch.Duration;
            Server = _CurrentMatch.Server;
            Winner = _CurrentMatch.Winner;
            InProgress = _CurrentMatch.InProgress;
            Completed = _CurrentMatch.Completed;
            IsExtendPossible = _CurrentMatch.IsExtendPossible;
            Undo = _CurrentMatch.Undo;
            Switch = _CurrentMatch.Switch;
            TotalSets = new vmSetScore(_CurrentMatch.TotalSets);
            CurrentSetScore = new vmSetScore(_CurrentMatch.CurrentSetScore);

            if (StatisticsCollection == null)
            {
                StatisticsCollection = _CurrentMatch.StatisticsCollection;
            }
            else
            {
                StatisticsCollection.Update(_CurrentMatch.StatisticsCollection);
            }

            Notify();
        }

        public void Notify()
        {
            NotifyPropertyChanged("CurrentPointDescription");
            NotifyPropertyChanged("NamePlayer1");
            NotifyPropertyChanged("NamePlayer2");
            NotifyPropertyChanged("ScorePlayer1");
            NotifyPropertyChanged("ScorePlayer2");
            NotifyPropertyChanged("Server");
            NotifyPropertyChanged("Status");
            NotifyPropertyChanged("MatchType");
            NotifyPropertyChanged("Surface");
            NotifyPropertyChanged("Duration");
            NotifyPropertyChanged("Winner");
            NotifyPropertyChanged("InProgress");
            NotifyPropertyChanged("Completed");
            NotifyPropertyChanged("IsExtendPossible");
            NotifyPropertyChanged("Undo");
            NotifyPropertyChanged("Switch");
            NotifyPropertyChanged("TotalSets");
            NotifyPropertyChanged("CurrentSetScore");
            NotifyPropertyChanged("StatisticsCollection");
            //NotifyPropertyChanged("Stoppable");
            TotalSets.Notify();
            CurrentSetScore.Notify();
            StatisticsCollection.Notify();

            if (NewState != null) NewState(this, EventArgs.Empty);
        }

        /// <summary>
        /// Deserialize JSON string to vmMatchState
        /// </summary>
        /// <param name="JSON"></param>
        /// <returns></returns>
        public static vmMatchState Deserialize(String JSON)
        {
            try
            {
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(JSON));

                //DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(vmMatchState));
                //vmMatchState _State = (vmMatchState)js.ReadObject(stream);

                DataContractSerializer xml = new DataContractSerializer(typeof(vmMatchState));
                vmMatchState _State = (vmMatchState)xml.ReadObject(stream);


                System.Diagnostics.Debug.WriteLine(_State);

                return _State;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("vmMatchState:Deserialize: {0}", e.Message));
            }

            return new vmMatchState();
        }

        /// <summary>
        /// Serialize the vmMatchState to JSON
        /// </summary>
        /// <returns></returns>
        public String Serialize()
        {
            String Result = "";

            try
            {
                MemoryStream stream = new MemoryStream();

                //DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(vmMatchState));
                //js.WriteObject(stream, this);

                DataContractSerializer xml = new DataContractSerializer(typeof(vmMatchState));
                xml.WriteObject(stream, this);

                stream.Position = 0;
                Result = new StreamReader(stream).ReadToEnd();

                System.Diagnostics.Debug.WriteLine(Result);

                /*js = new DataContractJsonSerializer(typeof(vmStatisticsCollection));
                stream = new MemoryStream();
                js.WriteObject(stream, this.StatisticsCollection);
                stream.Position = 0;
                System.Diagnostics.Debug.WriteLine(new StreamReader(stream).ReadToEnd());*/
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("vmMatchState:Serialize: {0}", e.Message));

            }

            return Result;
        }


        public bool AddAttachmentToDataRequest { get; set; }
        /// <summary>
        /// Populate the datarequest for sharing. The match status will be published
        /// </summary>
        /// <param name="_Request"></param>

        public async void PopulateDataRequest(DataRequest _Request)
        {
            Tennis_Statistics.Helpers.Settings _Settings = Tennis_Statistics.Helpers.Settings.GetInstance();

            bool ShareLocation = _Settings.GetBoolean("ShareLocation", false);
            bool ShareSetScores = _Settings.GetBoolean("ShareSetScores", true);
            bool ShareDuration = _Settings.GetBoolean("ShareDuration", true);

            await PopulateDataRequest(_Request, ShareLocation, ShareSetScores, ShareDuration, false);
        }

        public async Task PopulateDataRequest(DataRequest _Request, bool ShareLocation, bool ShareSetScores, bool ShareDuration, bool AddAttachment)
        {
            try
            {
                if (this.Completed)
                {
                    String Location = "";
                    String SetScores = "";
                    String Duration = "";

                    if (ShareLocation) Location = String.Format(" at {0}", Location);

                    if (ShareDuration)
                    {
                        TimeSpan tsDuration = this.Duration;
                        Duration = String.Format("The match lasted {0}:{1:D2}. ", ((tsDuration.Days * 24) + tsDuration.Hours), tsDuration.Minutes);
                    }

                    if (ShareSetScores)
                    {
                        int SetsPlayer1 = int.Parse(TotalSets.Score1);
                        int SetsPlayer2 = int.Parse(TotalSets.Score2);

                        if (SetsPlayer1 + SetsPlayer2 <= 1)
                        { 
                            SetScores = String.Format("The score is {0}. ", PrintableScore);
                        }
                        else
                        {
                            SetScores = String.Format("The sets are {0}. ", PrintableScore);
                        }
                    }

                    String Message;
                    String Title;
                    if (Winner != 0/*Match.Status == TennisMatch.MatchStatus.Completed*/)
                    {

                        Title = String.Format("{0} won the tennis match against {1}.",
                                            Winner == 1 ? NamePlayer1 : NamePlayer2,
                                            Winner == 1 ? NamePlayer2 : NamePlayer1
                                            );
                        Message = String.Format("{0}{1}",
                                            SetScores,
                                            Duration
                                            );
                    }
                    else
                    {
                        Title = String.Format("The tennis match between {0} and {1} was stopped.",
                                            NamePlayer1,
                                            NamePlayer2);
                        Message = String.Format("{0}{1}",
                                            SetScores,
                                            Duration
                                            );
                    }

                    if (!Helpers.Purchases.Available("HASHTAG"))
                    {
                        Message += "Recorded with #PebbleTimeManager for Windows Phone.";
                    }

                    _Request.Data.Properties.Title = Title;
                    _Request.Data.SetText(Message);

                    if (AddAttachment || AddAttachmentToDataRequest)
                    {
                        DataRequestDeferral deferral = _Request.GetDeferral();

                        IStorageFile Attachment = await Tennis_Statistics.Helpers.LocalStorage.GetFile("tennis_pebble.xml");

                        List<IStorageItem> storageItems = new List<IStorageItem>();
                        storageItems.Add(Attachment);
                        _Request.Data.SetStorageItems(storageItems);

                        deferral.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                _Request.Data.Properties.Title = "Error";
                _Request.Data.SetText("An error occurred: " + e.InnerException);
            }
        }


        #endregion

        #region Events

        public delegate void NewStateEventHandler(object sender, EventArgs e);
        public event NewStateEventHandler NewState;

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
