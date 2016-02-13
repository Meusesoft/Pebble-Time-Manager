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
using Windows.Media.SpeechSynthesis;
using System.Runtime.Serialization.Json;

namespace Tennis_Statistics.ViewModels
{
    public class vmMatch : INotifyPropertyChanged
    {

        public vmMatch()
        {
            CurrentPoint = new vmPoint();
            StatisticsCollection = new vmStatisticsCollection();
            StoreMatch = true;
        }

        #region Properties
        /// <summary>
        /// The model of the point currently being played
        /// </summary>
        public vmPoint CurrentPoint { get; set; }

        /// <summary>
        /// The model of the point currently being played
        /// </summary>
        public String CurrentPointDescription
        {
            get
            {
                if (CurrentPoint == null) return "";

                return CurrentPoint.Description;
            }
        }

        
        /// <summary>
        /// List of presentable statistics
        /// </summary>
        public vmStatisticsCollection StatisticsCollection { get; set; }

        /// <summary>
        /// The model of the current match
        /// </summary>
        private TennisMatch _Match;
        public TennisMatch Match
        {
            get
            {
                return _Match;
            }
            set
            {
                _Match = value;

                if (CurrentPoint != null && value!=null)
                {
                    CurrentPoint.LogLevel = value.LogLevel;
                }
            }
        }
        
        /// <summary>
        /// The name of player 1
        /// </summary>
        public String NamePlayer1
        {
            get
            {
                return Match.Contestant1.getName();
            }

        }

        /// <summary>
        /// The name of player 2
        /// </summary>
        public String NamePlayer2
        {
            get
            {
                return Match.Contestant2.getName();
            }

        }

        /// <summary>
        /// The current score of player 1
        /// </summary>
        public String ScorePlayer1
        {
            get
            {
                if (Match.Status == TennisMatch.MatchStatus.Completed) return "";
                
                String Score;

                if (Match.currentGame().GetType() == typeof(TennisTiebreak))
                { 
                    TennisTiebreak currentTiebreak = (TennisTiebreak)Match.currentGame();
                    if (currentTiebreak.GetStartServer() == 1)
                    {
                        Score = currentTiebreak.PointsServer.ToString();
                    }
                    else
                    {
                        Score = currentTiebreak.PointsReceiver.ToString();
                    }
                }
                else
                {

                    if (Match.currentGame().Server == 1)
                    {
                        Score = Match.currentGame().ParseScore(Match.currentGame().ScoreServer);
                    }
                    else
                    {
                        Score = Match.currentGame().ParseScore(Match.currentGame().ScoreReceiver);
                    }
                }

                return Score;
            }
        }

        /// <summary>
        /// The current score of player 2
        /// </summary>
        public String ScorePlayer2
        {
            get
            {
                if (Match.Status == TennisMatch.MatchStatus.Completed) return "";

                String Score;

                if (Match.currentGame().GetType() == typeof(TennisTiebreak))
                {
                    TennisTiebreak currentTiebreak = (TennisTiebreak)Match.currentGame();
                    if (currentTiebreak.GetStartServer() == 2)
                    {
                        Score = currentTiebreak.PointsServer.ToString();
                    }
                    else
                    {
                        Score = currentTiebreak.PointsReceiver.ToString();
                    }
                }
                else
                {
                    if (Match.currentGame().Server == 2)
                    {
                        Score = Match.currentGame().ParseScore(Match.currentGame().ScoreServer);
                    }
                    else
                    {
                        Score = Match.currentGame().ParseScore(Match.currentGame().ScoreReceiver);
                    }
                }

                return Score;
            }
        }

        /// <summary>
        /// The status of the match in a string representation
        /// </summary>
        public String Status
        {
            get
            {
                if (Match.Winner != 0)
                {
                    return String.Format("Winner: {0}", Match.Winner == 1 ? NamePlayer1 : NamePlayer2).ToUpper();
                }
                
                switch (Match.Status)
                {
                    //case TennisMatch.MatchStatus.Completed: return String.Format("Winner: {0}", Match.Winner == 1 ? NamePlayer1 : NamePlayer2).ToUpper();

                    case TennisMatch.MatchStatus.Terminated: return "Terminated".ToUpper();

                    case TennisMatch.MatchStatus.Resigned: return String.Format("Resigned by {0}", Match.Winner == 1 ? NamePlayer2 : NamePlayer1).ToUpper();

                    default: return "In progress".ToUpper();
                }                
            }
        }

        /// <summary>
        /// Description of match type in upper case characters
        /// </summary>
        public String MatchType
        {
            get
            {
                String MatchType = "";
                    
                MatchType += String.Format("{0} | best of {1}", Match.Type.ToString(), Match.BestOutOf.ToString());
                MatchType = MatchType.ToUpper();

                return MatchType;
            }
        }

       
        /// <summary>
        /// String representation of the surface type
        /// </summary>
        public String Surface
        {
            get
            {
                switch (Match.MatchSurface)
                {
                    case TennisMatch.Surface.ArtificialGrass:
                        return "Artifical Grass";

                    default:
                        return Match.MatchSurface.ToString();
                }
            }
        }

        /// <summary>
        /// The duration of the current match
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return Match.Duration.Duration;
            }
        }
        
        /// <summary>
        /// The current server (0 = player 1, 1 = player 2)
        /// </summary>
        public int Server
        {
            get
            {
                if (Match.currentGame().Server == 1) return 0;
                return 2;
            }
        }

        /// <summary>
        /// The winner (0 = player 1, 2 = player 2)
        /// </summary>
        public int Winner
        {
            get
            {
                return Match.Winner;
            }
        }

        /// <summary>
        /// True if this match is in progress
        /// </summary>
        public bool InProgress
        {
            get
            {
                return Match.Status == TennisMatch.MatchStatus.InProgress;
            }
        }

        /// <summary>
        /// True if this match is in progress
        /// </summary>
        public bool Completed
        {
            get
            {
                return Match.Status == TennisMatch.MatchStatus.Completed ||
                    Match.Status == TennisMatch.MatchStatus.Terminated ||
                    Match.Status == TennisMatch.MatchStatus.Resigned;
            }
        }

        /// <summary>
        /// True if it is possible to extend the match with additional sets
        /// </summary>
        public bool IsExtendPossible
        {
            get
            {
                return Match.IsExtendPossible();
            }
        }

        /// <summary>
        /// True, if undo is possible
        /// </summary>
        public bool Undo
        {
            get
            {
                return Match.Points.Count > 0 || (CurrentPoint != null && CurrentPoint.Point.Winner == 0 && CurrentPoint.Point.Serve == TennisPoint.PointServe.SecondServe);;
            }
        }

        /// <summary>
        /// True, if switching server is possible
        /// </summary>
        public bool Switch
        {
            get
            {
                return !Undo && Match.Status == TennisMatch.MatchStatus.InProgress;
            }
        }

        /// <summary>
        /// The scores of the current set
        /// </summary>
        private vmSetScore m_CurrentSetScore;
        public vmSetScore CurrentSetScore
        {
            get
            {
                if (m_CurrentSetScore == null)
                {
                    m_CurrentSetScore = new vmSetScore(Match.currentSet());
                }
                else
                {
                    vmSetScore.Initialize(m_CurrentSetScore, Match.currentSet());
                }

                return m_CurrentSetScore;
            }
        }

        /// <summary>
        /// The total sets won by the players.
        /// </summary>
        private vmSetScore m_TotalSets;
        public vmSetScore TotalSets
        {
            get
            {
                int SetsWon1 = 0, SetsWon2 = 0;

                if (m_TotalSets == null)
                {
                    m_TotalSets = new vmSetScore(Match.currentSet());
                }

                foreach (TennisSet Set in Match.Sets)
                {
                    if (Set.Winner==1) SetsWon1++;
                    if (Set.Winner==2) SetsWon2++;
                }

                m_TotalSets.Score1 = SetsWon1.ToString();
                m_TotalSets.Score2 = SetsWon2.ToString();

                return m_TotalSets;
            }
        }

        /// <summary>
        /// True if this match should be stored
        /// </summary>
        public bool StoreMatch { get; set; }

        /// <summary>
        /// The current background color or image
        /// </summary>
        public Brush Background
        {
            get
            {
                //Retrieve the setting
                return Tennis_Statistics.Helpers.Settings.GetInstance().Background();
            }
        }

        /// <summary>
        /// True if the game is paused
        /// </summary>
        public bool Paused
        {
            get
            {
                return !Match.Duration.SessionInProgress;
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

        #region Methods

        /// <summary>
        /// Start the match
        /// </summary>
        public void Start(vmNewMatch newMatch)
        {
            if (Match == null)
            {
                Match = new TennisMatch();
            }

            //Set the properties of the new match
            Match.Type = newMatch.GetType();
            Match.NumberGamesPerSet = newMatch.GamesPerSet;
            Match.BestOutOf = newMatch.GetBestOutOf();
            Match.TieBreakFinalSet = newMatch.TiebreakFinalSet;
            Match.LogLevel = newMatch.GetLogLevel();
            Match.Location = newMatch.Location;
            Match.MatchSurface = newMatch.GetSurface();

            TennisMatchVariant _variant = newMatch.MatchVariants[newMatch.SelectedMatchIndex];
            Match.DeuceSuddenDeath = _variant.DeuceSuddenDeath;
            Match.FinalSetIsTiebreak = _variant.FinalSetIsTiebreak;
            Match.FinalSetTieBreakLength = _variant.FinalSetTieBreakLength;
            Match.NumberGamesPerSet = _variant.NumberGamesPerSet;
            Match.TieBreakAtSameScoreOf = _variant.TieBreakAtSameScoreOf;
            Match.TieBreakFinalSet = _variant.TieBreakFinalSet;
            Match.TieBreakLength = _variant.TieBreakLength;

            StatisticsCollection.SetSource(Match);

            //Add the players
            Tennis_Statistics.Game_Logic.TennisPlayer Player1 = new Game_Logic.TennisPlayer();
            Player1.Name = newMatch.Player1.Name;
            Player1.ID = newMatch.Player1.ID;
            Player1.LocalPlayer = newMatch.Player1.LocalPlayer;
            Tennis_Statistics.Game_Logic.TennisPlayer Player2 = new Game_Logic.TennisPlayer();
            Player2.Name = newMatch.Player2.Name;
            Player2.ID = newMatch.Player2.ID;
            Player2.LocalPlayer = newMatch.Player2.LocalPlayer;
            
            Match.Contestant1.Players.Add(Player1);
            Match.Contestant2.Players.Add(Player2);

            //Add partners
            if (Match.Type == TennisMatch.MatchType.Doubles)
            {
                Tennis_Statistics.Game_Logic.TennisPlayer Partner1 = new Game_Logic.TennisPlayer();
                Partner1.Name = newMatch.Player1Partner.Name;
                Partner1.ID = newMatch.Player2Partner.ID;
                Partner1.LocalPlayer = newMatch.Player1Partner.LocalPlayer;
                Tennis_Statistics.Game_Logic.TennisPlayer Partner2 = new Game_Logic.TennisPlayer();
                Partner2.Name = newMatch.Player2Partner.Name;
                Partner2.ID = newMatch.Player2Partner.ID;
                Partner2.LocalPlayer = newMatch.Player2Partner.LocalPlayer;

                Match.Contestant1.Players.Add(Partner1);
                Match.Contestant2.Players.Add(Partner2);
            }

            CurrentPoint.LogLevel = Match.LogLevel;

            //Start the new set
            Match.StartNewSet();

            NewPoint();
        }

        /// <summary>
        /// Terminate the current match
        /// </summary>
        public void Terminate()
        {
            ProcessAction("CommandTerminate");
        }

        /// <summary>
        /// Pause the current match (stop the timer, no status changes)
        /// </summary>
        public void Pause()
        {
            if (Match.Status == TennisMatch.MatchStatus.InProgress) Match.Pause();
        }

        /// <summary>
        /// Resume the current match (resume timer)
        /// </summary>
        public void Resume()
        {
            if (Match.Status == TennisMatch.MatchStatus.InProgress) Match.Resume();
        }

        /// <summary>
        /// Terminate the current match
        /// </summary>
        public void StopWithWinner(int Winner)
        {
            Match.Winner = Winner;
            ProcessAction("CommandEnd");
        }

        /// <summary>
        /// Terminate the current match
        /// </summary>
        public void Resign(int Resignee)
        {
            Match.Winner = 3 - Resignee;
            ProcessAction("CommandResign");
        }
        
        /// <summary>
        /// Start a new point
        /// </summary>
        public void NewPoint()
        {
            if (Match.Status == TennisMatch.MatchStatus.InProgress)
            {
                TennisPoint newPoint = Match.CreateNewPoint();

                if (!Match.Duration.SessionInProgress) Match.Resume();

                if (CurrentPoint == null)
                {
                    CurrentPoint = new vmPoint();
                    CurrentPoint.LogLevel = this.Match.LogLevel;
                }
                CurrentPoint.Point = newPoint;

                NotifyPropertyChanged("CurrentPoint");
                NotifyPropertyChanged("CurrentPointDescription");
                NotifyPropertyChanged("Server");
                NotifyPropertyChanged("InProgress");
            }
            else
            {
                CurrentPoint = null;
                NotifyPropertyChanged("Status"); //something happend that terminated the match (status != InProgess)
                NotifyPropertyChanged("InProgress");
                NotifyPropertyChanged("CurrentPointDescription");
                NotifyPropertyChanged("CurrentPoint");
            }
        }

        /// <summary>
        /// Process the action/choice of the user
        /// </summary>
        /// <param name="Method"></param>
        public void ProcessAction(string CommandMethod)
        {
            try
            {
                switch (CommandMethod)
                {
                    case "CommandTerminate":

                        Match.Terminate();

                        break;

                    case "CommandEnd":

                        Match.End(TennisMatch.MatchStatus.Terminated);

                        break;

                    case "CommandResign":

                        Match.End(TennisMatch.MatchStatus.Resigned);

                        break;

                    case "CommandUndo":

                        if (CurrentPoint == null || CurrentPoint.Point.Winner == 0 && CurrentPoint.Point.Serve != TennisPoint.PointServe.SecondServe)
                        {
                            //Undo the last played point if the current point is not present, or if it has not been played yet.
                            Match.Undo();
                        }

                        NewPoint();

                        break;

                    case "CommandSwitchServer":

                        Match.SwitchServer();

                        NewPoint();

                        break;

                    case "CommandExtend":

                        Match.Extend();

                        NewPoint();

                        break;

                    default:

                        CurrentPoint.ProcessAction(CommandMethod);

                        if (CurrentPoint.PossibleActions.Count <= 1)
                        {
                            //no choices to make; start a new point
                            Match.Add(CurrentPoint.Point);

                            NewPoint();
                        }
                        break;
                }

                NotifyPropertyChanged("CurrentPoint");
                NotifyPropertyChanged("Duration");
                NotifyPropertyChanged("IsExtendPossible");
                NotifyPropertyChanged("SetScores");
                NotifyPropertyChanged("ScorePlayer1");
                NotifyPropertyChanged("ScorePlayer2");
                NotifyPropertyChanged("Server");
                NotifyPropertyChanged("InProgress");
                NotifyPropertyChanged("Winner");
                NotifyPropertyChanged("Completed");
                NotifyPropertyChanged("Undo");
                NotifyPropertyChanged("Sets");
                NotifyPropertyChanged("Switch");
                NotifyPropertyChanged("MatchType");
                NotifyPropertyChanged("Status");

                CurrentSetScore.Notify();
                TotalSets.Notify();
                StatisticsCollection.Notify();
            }
            catch (Exception e)
            {
                ExtendedEventArgs eaa = new ExtendedEventArgs();
                eaa.Error = "A fatal error occurred. Please try again or stop this match to preserve the statistics.";
                #if DEBUG
                eaa.Error = String.Format("A fatal error occurred. {1}:{0}", e.Message, e.Source); 
                #endif
                OnFatalError(eaa);
            }
        }

        /// <summary>
        /// Save the current match to local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Save()
        {
            String Filename = String.Format("match_{0}.xml", Match.ID.ToString());

            return await Save(Filename);
        }

        /// <summary>
        /// Save the current match to local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Save(String Filename)
        {
            try
            {
                String XML = Helpers.Serializer.XMLSerialize(this.Match);
                await Helpers.LocalStorage.Save(XML, Filename, false);

                return true;
            }
            catch (Exception)
            {
                //...
                return false;
            }
        }
        
        /// <summary>
                 /// Save the current match to OneDrive
                 /// </summary>
                 /// <returns></returns>
        public async Task<bool> SaveToOneDrive()
        {
            Helpers.MicrosoftAccount _Account = Helpers.MicrosoftAccount.GetInstance();
            if (!_Account.Connected) return true;
            
            try
            {
                String XML = Helpers.Serializer.XMLSerialize(this.Match);
                String Filename = String.Format("match_{0}.xml", Match.ID.ToString());
                await Helpers.OneDrive.UploadFileAsync("TennisData", Filename);

                return true;
            }
            catch (Exception e)
            {
                //...
                return false;
            }
        }

        /// <summary>
        /// Create view containing the current state of the match (score and statistics only)
        /// </summary>
        /// <returns></returns>
        public String CreateJSONView()
        {   
            String Result;

            Result = "";

            



            return Result;
        }

        /// <summary>
        /// Save the current match to OneDrive
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RemoveFromOneDrive()
        {
            try
            {
                String Filename = String.Format("match_{0}.xml", Match.ID.ToString());
                await Helpers.OneDrive.DeleteAsync("TennisData", Filename);

                return true;
            }
            catch (Exception e)
            {
                //...
                return false;
            }
        }

        
        /// <summary>
        /// Load the requested file/match
        /// </summary>
        /// <returns></returns>
        public async Task Load(Guid MatchId)
        {
            //Load the data
            String Filename = String.Format("match_{0}.xml", MatchId.ToString());

            await Load(Filename);
        }

        /// <summary>
        /// Load the requested file/match
        /// </summary>
        /// <returns></returns>
        public async Task Load(String Filename)
        {
            try
            {
                //Load the data
                String XML = await Helpers.LocalStorage.Load(Filename);
                //CompressedXML = "";
                //Deserialize
                TennisMatch StoredMatch = (TennisMatch)Helpers.Serializer.XMLDeserialize(XML, typeof(TennisMatch));

                //Restore references

                //Set it as the current match
                Match = StoredMatch;
                StatisticsCollection.SetSource(_Match);
            }
            catch (Exception)
            {
                ExtendedEventArgs eea = new ExtendedEventArgs();
                eea.Error = "An error occured while loading the stored match";
                OnFatalError(eea);
                //throw new Exception();
            }
        }

        public bool AddAttachmentToDataRequest { get; set; }
        /// <summary>
        /// Populate the datarequest for sharing. The match status will be published
        /// </summary>
        /// <param name="_Request"></param>
        public async void PopulateDataRequest(DataRequest _Request)
        {
            try
            {
                Tennis_Statistics.Helpers.Settings _Settings = Tennis_Statistics.Helpers.Settings.GetInstance();

                if (Match.Status != TennisMatch.MatchStatus.InProgress)
                {
                    bool ShareLocation = _Settings.GetBoolean("ShareLocation", false);
                    bool ShareSetScores = _Settings.GetBoolean("ShareSetScores", true);
                    bool ShareDuration = _Settings.GetBoolean("ShareDuration", true);

                    String Location = "";
                    String SetScores = "";
                    String Duration = "";

                    if (ShareLocation) Location = String.Format(" at {0}", Match.Location);
                    if (ShareDuration)
                    {
                        TimeSpan tsDuration = Match.Duration.Duration;
                        Duration = String.Format("The match lasted {0}:{1:D2}. ", ((tsDuration.Days * 24) + tsDuration.Hours), tsDuration.Minutes);
                    }

                    if (ShareSetScores)
                    {
                        if (Match.Sets.Count == 1)
                        {
                            SetScores = String.Format("The score is {0}. ", Match.PrintableScore());
                        }
                        else
                        {
                            SetScores = String.Format("The sets are {0}. ", Match.PrintableScore());
                        }
                    }

                    String LocalPlayer = Match.Contestant1.getName();
                    if (Match.Contestant2.ContainsLocalPlayer) LocalPlayer = Match.Contestant2.getName();

                    String Title = String.Format("{0} completed a tennis match.",
                                        LocalPlayer);

                    String Message;
                    if (Match.Winner!=0/*Match.Status == TennisMatch.MatchStatus.Completed*/)
                    {
                        Message = String.Format("{0} won the tennis match against {1}{2}. {3}{4}",
                                            Match.GetContestant(Match.Winner).getName(),
                                            Match.GetContestant(3 - Match.Winner).getName(),
                                            Location,
                                            SetScores,
                                            Duration
                                            );
                    }
                    else
                    {
                        String OtherPlayer = LocalPlayer == Match.Contestant1.getName() ? Match.Contestant2.getName() : Match.Contestant1.getName();
                        if (ShareLocation) Location = "It was played" + Location + ". ";

                        Message = String.Format("The match against {0} was terminated. {1}{2}{3}",
                                            OtherPlayer,
                                            Location,
                                            SetScores,
                                            Duration);
                    }

                    if (!Helpers.Purchases.Available("HASHTAG"))
                    {
                        Message += "Recorded with #TennisStatistics for Windows Phone.";
                    }

                    _Request.Data.Properties.Title = Title;
                    _Request.Data.SetText(Message);

                    if (AddAttachmentToDataRequest)
                    {
                        DataRequestDeferral deferral = _Request.GetDeferral();

                        await Save("export.xml");

                        IStorageFile Attachment = await Tennis_Statistics.Helpers.LocalStorage.GetFile("export.xml");

                        List<IStorageItem> storageItems = new List<IStorageItem>();
                        storageItems.Add(Attachment);
                        _Request.Data.SetStorageItems(storageItems);

                        deferral.Complete();
                    }


                    //_Request.Data.SetWebLink(new Uri("http://www.nu.nl"));
                    //_Request.Data.Properties.Description = "HTML!";

                    // string htmlFormat = HtmlFormatHelper.CreateHtmlFormat(Message);
                    //_Request.Data.SetHtmlFormat(htmlFormat);
                }
            }
            catch (Exception e)
            {
                _Request.Data.Properties.Title = "Error";
                _Request.Data.SetText("An error occurred: " + e.InnerException);
            }
        }

        #endregion

        #region Commands

        RelayCommand m_cmdUndo;

        public RelayCommand cmdUndo
        {
            get
            {
                if (m_cmdUndo == null)
                    m_cmdUndo = new RelayCommand(param => ProcessAction("CommandUndo"));

                return m_cmdUndo;
            }
        }

        RelayCommand m_cmdSwitchServer;

        public RelayCommand cmdSwitchServer
        {
            get
            {
                if (m_cmdSwitchServer == null)
                    m_cmdSwitchServer = new RelayCommand(param => ProcessAction("CommandSwitchServer"));

                return m_cmdSwitchServer;
            }
        }

        RelayCommand m_cmdExtendMatch;

        public RelayCommand cmdExtendMatch
        {
            get
            {
                if (m_cmdExtendMatch == null)
                    m_cmdExtendMatch = new RelayCommand(param => ProcessAction("CommandExtend"));

                return m_cmdExtendMatch;
            }
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

    public class SetTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SetInProgressTemplate { get; set; }
        public DataTemplate SetFinishedHeaderTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            vmSetScore _object = (vmSetScore)item;

            if (_object.InProgress) return SetInProgressTemplate;

            return SetFinishedHeaderTemplate;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject abc)
        {
            return SelectTemplateCore(item);
        }


    }

}
