using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    [DataContract(IsReference = true)]
    public class TennisMatch : IXmlSerializable
    {

        public enum MatchType { Singles = 0, Doubles = 1 }
        public enum LogLevelEnum { Points = 1, Errors = 2, Shots = 4, Placement = 8 };
        public enum MatchStatus { Created = 0, InProgress = 1, Completed = 2, Terminated = 3, Resigned = 4 }
        public enum Surface { Clay, Hard, ArtificialGrass, Grass, Carpet, Indoor }

        #region Constructors

        public TennisMatch()
        {
            LogLevel = LogLevelEnum.Shots;
            Points = new List<TennisPoint>();
            Sets = new List<TennisSet>();
            Statistics = new TennisStatistics(this);
            Duration = new TennisDuration();
            BestOutOf = 1;
            NumberGamesPerSet = 6;
            DeuceSuddenDeath = false;
            TieBreakAtSameScoreOf = 3;
            TieBreakFinalSet = true;
            TieBreakLength = 7;
            FinalSetIsTiebreak = false;
            FinalSetTieBreakLength = 10;
            Winner = 0;
            Status = MatchStatus.Created;
            ID = Guid.NewGuid();
            Contestant1 = new TennisContestant();
            Contestant1.ContestantNr = 1;
            Contestant2 = new TennisContestant();
            Contestant2.ContestantNr = 2;
            NextContestantToServe = Contestant2;
        }

        #endregion

        #region Properties

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public MatchType Type { get; set; }

        [DataMember]
        public Surface MatchSurface { get; set; }

        [DataMember]
        public LogLevelEnum LogLevel { get; set; }

        [DataMember]
        public MatchStatus Status { get; set; }

        [DataMember]
        public int Winner { get; set; }

        [DataMember]
        public TennisDuration Duration { get; set; }

        [DataMember]
        public String Location { get; set; }

        [DataMember]
        public int BestOutOf { get; set; }

        [DataMember]
        public Boolean DeuceSuddenDeath { get; set; }

        [DataMember]
        public int NumberGamesPerSet { get; set; }

        [DataMember]
        public int TieBreakAtSameScoreOf { get; set; }

        [DataMember]
        public int TieBreakLength { get; set; }

        [DataMember]
        public Boolean TieBreakFinalSet { get; set; }

        [DataMember]
        public Boolean FinalSetIsTiebreak { get; set; }

        [DataMember]
        public int FinalSetTieBreakLength { get; set; }

        [DataMember]
        public List<TennisSet> Sets { get; set; }

        [DataMember]
        public List<TennisPoint> Points { get; set; }

        [DataMember]
        public TennisContestant Contestant1 { get; set; }

        [DataMember]
        public TennisContestant Contestant2 { get; set; }

        [DataMember]
        public TennisContestant NextContestantToServe { get; set; }

        [DataMember]
        public TennisStatistics Statistics { get; set; }

        // [DataMember]
        // public TennisPlayer NextPlayerToServe { get; set; }

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

        #region INotifyPropertyChanging Members

        #endregion

        #region Methods

        /// <summary>
        /// Start this match
        /// </summary>
        public void Start()
        {
            if (Contestant1 == null || Contestant2 == null || Contestant1 == Contestant2) throw new Exception("A tennis match needs 2 contestants.");

            Duration.Start();

            Status = MatchStatus.InProgress;
        }

        /// <summary>
        /// Start a new set and a new game.
        /// </summary>
        public void StartNewSet()
        {
            if (!Duration.SessionInProgress) Start(); // Start this match

            TennisSet newSet = new TennisSet(this);
            newSet.Start();
            Sets.Add(newSet);

            if (NewSet != null) NewSet.Invoke(this, newSet);

            StartNewGame();
        }

        /// <summary>
        /// Pause the current match
        /// </summary>
        public void Pause()
        {
            if (Duration.SessionInProgress)
            {
                Duration.End();
                currentSet().Pause();
            }
        }

        /// <summary>
        /// Resume the current match
        /// </summary>
        public void Resume()
        {
            if (!Duration.SessionInProgress)
            {
                Duration.Start();
                currentSet().Resume();
            }
        }

        /// <summary>
        /// Get a list of default match variants; regular, fast4, tiebreaks
        /// </summary>
        /// <returns></returns>
        public static List<TennisMatchVariant> GetDefaultVariants()
        {
            return new List<TennisMatchVariant>() {
                new TennisMatchVariant {Description="Regular", DeuceSuddenDeath=false, FinalSetIsTiebreak=false, FinalSetTieBreakLength=7, NumberGamesPerSet=6, TieBreakAtSameScoreOf=6, TieBreakFinalSet=true, TieBreakLength=7},
                new TennisMatchVariant {Description="Fast4", DeuceSuddenDeath=true, FinalSetIsTiebreak=true, FinalSetTieBreakLength=10, NumberGamesPerSet=4, TieBreakAtSameScoreOf=3, TieBreakFinalSet=true, TieBreakLength=5},
                new TennisMatchVariant {Description="Tiebreaks", DeuceSuddenDeath=false, FinalSetIsTiebreak=false, FinalSetTieBreakLength=7, NumberGamesPerSet=0, TieBreakAtSameScoreOf=6, TieBreakFinalSet=true, TieBreakLength=7}
            };
        }

        /// <summary>
        /// Create a new match from the given variant
        /// </summary>
        /// <param name="_variant"></param>
        /// <returns></returns>
        public static TennisMatch CreateMatchFromVariant(TennisMatchVariant _variant)
        {
            TennisMatch _newMatch;

            _newMatch = new TennisMatch();
            _newMatch.DeuceSuddenDeath = _variant.DeuceSuddenDeath;
            _newMatch.FinalSetIsTiebreak = _variant.FinalSetIsTiebreak;
            _newMatch.FinalSetTieBreakLength = _variant.FinalSetTieBreakLength;
            _newMatch.NumberGamesPerSet = _variant.NumberGamesPerSet;
            _newMatch.TieBreakAtSameScoreOf = _variant.TieBreakAtSameScoreOf;
            _newMatch.TieBreakFinalSet = _variant.TieBreakFinalSet;
            _newMatch.TieBreakLength = _variant.TieBreakLength;

            return _newMatch;
        }

        /// <summary>
        /// Create a new match from the given variant name
        /// </summary>
        /// <param name="_variant"></param>
        /// <returns></returns>
        public static TennisMatch CreateMatchFromVariant(String _variant)
        {
            List<TennisMatchVariant> Variants = TennisMatch.GetDefaultVariants();

            var _selectedVariant = Variants.Where(x => x.Description.ToLower() == _variant.ToLower());

            if (_selectedVariant.Count() != 1) return null;

            return TennisMatch.CreateMatchFromVariant(_selectedVariant.First());
        }

        /// <summary>
        /// Start a new game, and if needed start a new set
        /// </summary>
        public void StartNewGame()
        {
            if (currentSet() == null)
            {
                StartNewSet(); //new set also starts new game
            }
            else
            {
                //Start new game
                currentSet().StartNewGame();
            }
        }

        /// <summary>
        /// Terminate the this match
        /// </summary>
        public void Terminate()
        {
            End(MatchStatus.Terminated);
        }

        /// <summary>
        /// Terminate the this match
        /// </summary>
        public void Resign(int ContestantToResign)
        {
            Winner = 3 - ContestantToResign;
            End(MatchStatus.Resigned);
        }

        /// <summary>
        /// Stop the this match
        /// </summary>
        public void End(MatchStatus newStatus)
        {
            Status = newStatus;
            Duration.End();

            TennisSet _set = currentSet();
            if (_set != null)
            {
                _set.End(Winner);
            }
        }

        /// <summary>
        /// Return the instance of the requested contestant number
        /// </summary>
        /// <param name="ContestantNr"></param>
        /// <returns></returns>
        public TennisContestant GetContestant(int ContestantNr)
        {
            if (ContestantNr == 1) return Contestant1;
            else return Contestant2;
        }

        /// <summary>
        /// Add the given point to this match
        /// </summary>
        /// <param name="newPoint"></param>
        public void Add(TennisPoint newPoint)
        {
            if (Winner != 0) return;
            if (currentSet() == null) StartNewSet();

            newPoint.PartOf = currentGame();
            Points.Add(newPoint);

            currentSet().Add(newPoint);

            //Update statistics
            Statistics.Add(newPoint);

            if (currentSet().Winner != 0)
            {
                //the set is finished. Is the match finished?
                int NumberOfSetsToWin = (BestOutOf / 2) + 1;

                GetContestant(currentSet().Winner).SetsWon++;

                if (GetContestant(currentSet().Winner).SetsWon == NumberOfSetsToWin)
                {
                    Winner = currentSet().Winner;
                    End(MatchStatus.Completed);
                }
                else
                {
                    StartNewSet();
                }
            }
        }

        /// <summary>
        /// Create a new point / rally instance
        /// </summary>
        /// <returns></returns>
        public TennisPoint CreateNewPoint()
        {
            TennisPoint newPoint = null;

            if (Winner == 0)
            {
                newPoint = new TennisPoint();
                newPoint.PartOf = currentGame();
                newPoint.Server = currentGame().Server;

                if (currentGame().BreakPoint()) newPoint.Type.Add(TennisPoint.PointType.BreakPoint);
                if (currentGame().GamePoint()) newPoint.Type.Add(TennisPoint.PointType.GamePoint);
                if (CheckSetPoint() != 0)
                {
                    newPoint.Type.Add(TennisPoint.PointType.SetPoint);
                    if (currentGame().GamePoint())
                    {
                        newPoint.Type.Add(TennisPoint.PointType.SetPointServer);
                    }
                }
                if (CheckMatchPoint() != 0)
                {
                    newPoint.Type.Add(TennisPoint.PointType.MatchPoint);
                    if (currentGame().GamePoint()) newPoint.Type.Add(TennisPoint.PointType.MatchPointServer);
                }
            }

            return newPoint;
        }

        /// <summary>
        /// Check if the set is on match point and if so return the number of the player who has it
        /// </summary>
        /// <returns></returns>
        private int CheckSetPoint()
        {
            TennisGame currentGame = this.currentGame();
            int ScoreServer = 0;
            int ScoreReceiver = 0;

            if (currentGame.Server == 1)
            {
                ScoreServer = currentSet().ScoreContestant1;
                ScoreReceiver = currentSet().ScoreContestant2;
            }
            else
            {
                ScoreReceiver = currentSet().ScoreContestant1;
                ScoreServer = currentSet().ScoreContestant2;
            }

            //Regular game
            if (currentGame.GetType() != typeof(TennisTiebreak))
            {
                if (ScoreServer >= NumberGamesPerSet - 1 && ScoreReceiver < ScoreServer && currentGame.GamePoint()) return currentGame.Server;
                if (ScoreReceiver >= NumberGamesPerSet - 1 && ScoreServer < ScoreReceiver && currentGame.BreakPoint()) return 3 - currentGame.Server;
            }
            else
            {
                //Tiebreak
                TennisTiebreak Tiebreak = (TennisTiebreak)currentGame;

                return Tiebreak.SetPoint();
            }

            return 0;
        }

        /// <summary>
        /// Check if the match is on match point and if so return the number of the player who has it
        /// </summary>
        /// <returns></returns>
        private int CheckMatchPoint()
        {
            int PlayerOnSetpoint = CheckSetPoint();
            if (PlayerOnSetpoint == 0) return 0;

            if ((Contestant1.SetsWon == ((BestOutOf + 1) / 2) - 1) && PlayerOnSetpoint == 1) return 1;
            if ((Contestant2.SetsWon == ((BestOutOf + 1) / 2) - 1) && PlayerOnSetpoint == 2) return 2;

            return 0;
        }

        /// <summary>
        /// Undo the last point
        /// </summary>
        public void Undo()
        {
            //If game is abnormally ended, reset the status
            if (Status == MatchStatus.Terminated || Status == MatchStatus.Resigned)
            {
                Winner = 0;
                Duration.Start(); // start a new session
                Status = MatchStatus.InProgress;
            }
            else
            {
                //If no points have been played, ignore
                if (Points.Count == 0) return;

                //Remove the last point
                TennisPoint point = Points.Last();
                Points.Remove(point);

                //Remove the points from the statistics
                Statistics.Remove(point);

                //If there was a winner, reset the status
                if (Winner != 0)
                {
                    Winner = 0;
                    Duration.Start();
                    Status = MatchStatus.InProgress;
                }

                while (!currentSet().Undo())
                {
                    Sets.Remove(currentSet());
                }
            }


            //Make sure the next server is properly set. It is unknown if the current game has changed
            //so we set this every time.
            NextContestantToServe = currentGame().Server == 1 ? Contestant1 : Contestant2;
        }

        /// <summary>
        /// Switch the server to start serving the match. Default is Contestant 1
        /// </summary>
        public void SwitchServer()
        {
            if (Points.Count > 0) throw new Exception("Can't switch server when the match has started.");

            TennisContestant newServer = GetNextServer();

            currentSet().StartServer = newServer.ContestantNr;
            currentGame().Server = newServer.ContestantNr;
            currentGame().Receiver = 3 - currentGame().Server;
            if (currentGame().GetType() == typeof(TennisTiebreak))
            {
                TennisTiebreak _tiebreak = (TennisTiebreak)currentGame();
                _tiebreak.SetStartServer(newServer.ContestantNr);
            }
        }

        /// <summary>
        /// Returns true if the match can be extended
        /// </summary>
        /// <returns></returns>
        public bool IsExtendPossible()
        {
            bool Result;

            Result = (Status == MatchStatus.Completed);
            Result = Result && BestOutOf < 5;

            return Result;
        }

        /// <summary>
        /// Extend the match with an additional set
        /// </summary>
        public void Extend()
        {
            if (IsExtendPossible())
            {
                Winner = 0;
                BestOutOf += 2;
                Status = MatchStatus.InProgress;
                Duration.Start();
                StartNewSet();
            }
            else
            {
                throw new Exception("This match cannot be extended. It contains the maximum amount of sets.");
            }
        }

        /// <summary>
        /// Returns the instance of the current set
        /// </summary>
        /// <returns></returns>
        public TennisSet currentSet()
        {
            if (Sets.Count == 0) return null;

            return Sets.Last();
        }

        /// <summary>
        /// Returns the instance of the current game
        /// </summary>
        /// <returns></returns>
        public TennisGame currentGame()
        {
            TennisSet currentSet = this.currentSet();

            if (currentSet == null) return null;

            return currentSet.currentGame();

        }

        /// <summary>
        /// Changes the server and returns its instance
        /// </summary>
        /// <returns></returns>
        public TennisContestant GetNextServer()
        {
            NextContestantToServe = ((NextContestantToServe == Contestant1) ? Contestant2 : Contestant1);

            return NextContestantToServe;
        }

        /// <summary>
        /// Returns the instance of the receiver
        /// </summary>
        /// <returns></returns>
        public TennisContestant GetReceiver()
        {
            return (NextContestantToServe == Contestant1) ? Contestant2 : Contestant1;
        }

        /// <summary>
        /// Constructs a printable score of all the sets
        /// </summary>
        /// <returns></returns>
        public String PrintableScore()
        {
            String _Score = "";

            foreach (TennisSet _set in Sets)
            {
                if (_Score.Length > 0) _Score += ", ";

                if (FinalSetIsTiebreak && Sets.Count == BestOutOf && _set == Sets.Last())
                {
                    _Score += String.Format("{0}-{1}", _set.Games.Last().Server == 1 ? _set.Games.Last().PointsServer : _set.Games.Last().PointsReceiver, _set.Games.Last().Server == 2 ? _set.Games.Last().PointsServer : _set.Games.Last().PointsReceiver);
                }
                else
                {
                    _Score += String.Format("{0}-{1}", _set.ScoreContestant1, _set.ScoreContestant2);
                    if (_set.Games.Last().GetType() == typeof(TennisTiebreak))
                    {
                        _Score += String.Format(" ({0})", Math.Min(_set.Games.Last().PointsServer, _set.Games.Last().PointsServer));
                    }
                }
            }

            return _Score;
        }

        #endregion

        #region Events

        // A delegate type for hooking up change notifications.
        public delegate void NewSetEventHandler(object sender, TennisSet _newSet);

        public event NewSetEventHandler NewSet;

        #endregion

        #region XML

        /// <summary>
        /// Return the XML schema.
        /// </summary>
        /// <returns>null</returns>
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates a match object from its XML representation.
        /// </summary>
        /// <param name="reader"></param>
        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            try
            {
                reader.MoveToContent();

                String _ID = reader.GetAttribute("ID");
                String _LogLevel = reader.GetAttribute("LogLevel");
                String _Surface = reader.GetAttribute("Surface");
                String _Type = reader.GetAttribute("Type");

                ID = System.Guid.Parse(_ID);
                LogLevel = (LogLevelEnum)System.Enum.Parse(typeof(LogLevelEnum), _LogLevel);
                Location = reader.GetAttribute("Location");
                MatchSurface = (Surface)System.Enum.Parse(typeof(Surface), _Surface);
                Type = (MatchType)System.Enum.Parse(typeof(MatchType), _Type);

                while (!reader.EOF)
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "Rules":
                                {
                                    String _BestOutOf = reader.GetAttribute("BestOutOf");
                                    String _DeuceSuddendDeath = reader.GetAttribute("DeuceSuddenDeath");
                                    String _FinalSetIsTiebreak = reader.GetAttribute("FinalSetIsTiebreak");
                                    String _FinalSetTiebreakLength = reader.GetAttribute("FinalSetTieBreakLength");
                                    String _NumberGamesPerSet = reader.GetAttribute("NumberGamesPerSet");
                                    String _TieBreakSameScoreOf = reader.GetAttribute("TieBreakAtSameScoreOf");
                                    String _TieBreakFinalSet = reader.GetAttribute("TieBreakFinalSet");
                                    String _TieBreakLength = reader.GetAttribute("TieBreakLength");

                                    BestOutOf = int.Parse(_BestOutOf);
                                    DeuceSuddenDeath = bool.Parse(_DeuceSuddendDeath);
                                    FinalSetIsTiebreak = bool.Parse(_FinalSetIsTiebreak);
                                    FinalSetTieBreakLength = int.Parse(_FinalSetTiebreakLength);
                                    NumberGamesPerSet = int.Parse(_NumberGamesPerSet);
                                    TieBreakAtSameScoreOf = int.Parse(_TieBreakSameScoreOf);
                                    TieBreakFinalSet = bool.Parse(_TieBreakFinalSet);
                                    TieBreakLength = int.Parse(_TieBreakLength);

                                    reader.Read();

                                    break;
                                }

                            case "Status":
                                {
                                    String _Status = reader.GetAttribute("Status");
                                    String _NextContestantToServe = reader.GetAttribute("NextContestantToServe");
                                    String _Winner = reader.GetAttribute("Winner");

                                    NextContestantToServe = (_NextContestantToServe == "1") ? Contestant1 : Contestant2;
                                    Winner = int.Parse(_Winner);
                                    Status = (MatchStatus)System.Enum.Parse(typeof(MatchStatus), _Status);

                                    reader.Read();

                                    break;
                                }

                            case "Duration":
                                {
                                    Duration.ReadXml(reader);

                                    break;
                                }

                            case "Contestant1":
                                {
                                    Contestant1.ReadXml(reader);

                                    break;
                                }

                            case "Contestant2":
                                {
                                    Contestant2.ReadXml(reader);

                                    break;
                                }

                            case "Statistics":
                                {
                                    Statistics.ReadXml(reader);

                                    break;
                                }

                            case "Sets":
                                {
                                    while (reader.Read() && !(reader.Name == "Sets" && reader.NodeType == System.Xml.XmlNodeType.EndElement))
                                    {
                                        if (reader.NodeType == System.Xml.XmlNodeType.Element)
                                        {
                                            if (reader.Name == "Set")
                                            {
                                                TennisSet _newSet = new TennisSet(this);
                                                _newSet.ReadXml(reader);

                                                Sets.Add(_newSet);
                                            }
                                        }
                                    }

                                    break;
                                }

                            default:

                                reader.Read();

                                break;

                        }
                    }
                    else reader.Read();
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Converts this match into its XML representation.
        /// </summary>
        /// <param name="writer"></param>
        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("ID", ID.ToString());
            writer.WriteAttributeString("Location", Location);
            writer.WriteAttributeString("LogLevel", LogLevel.ToString());
            writer.WriteAttributeString("Surface", MatchSurface.ToString());
            writer.WriteAttributeString("Type", Type.ToString());

            //Rules
            writer.WriteStartElement("Rules");
                writer.WriteAttributeString("BestOutOf", BestOutOf.ToString());
                writer.WriteAttributeString("DeuceSuddenDeath", DeuceSuddenDeath.ToString());
                writer.WriteAttributeString("FinalSetIsTiebreak", FinalSetIsTiebreak.ToString());
                writer.WriteAttributeString("FinalSetTieBreakLength", FinalSetTieBreakLength.ToString());
                writer.WriteAttributeString("NumberGamesPerSet", NumberGamesPerSet.ToString());
                writer.WriteAttributeString("TieBreakAtSameScoreOf", TieBreakAtSameScoreOf.ToString());
                writer.WriteAttributeString("TieBreakFinalSet", TieBreakFinalSet.ToString());
                writer.WriteAttributeString("TieBreakLength", TieBreakLength.ToString());
            writer.WriteEndElement();

            //Status
            writer.WriteStartElement("Status");
                writer.WriteAttributeString("NextContestantToServe", NextContestantToServe.ContestantNr.ToString());
                writer.WriteAttributeString("Status", Status.ToString());
                writer.WriteAttributeString("Winner", Winner.ToString());
            writer.WriteEndElement();

            //Contestants
            writer.WriteStartElement("Contestant1");
                Contestant1.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteStartElement("Contestant2");
                Contestant2.WriteXml(writer);
            writer.WriteEndElement();

            //Duration
            writer.WriteStartElement("Duration");
                Duration.WriteXml(writer);
            writer.WriteEndElement();

            //Statistics
            writer.WriteStartElement("Statistics");
                Statistics.WriteXml(writer);                
            writer.WriteEndElement();

            //Sets
            writer.WriteStartElement("Sets");
                foreach (var item in Sets)
                {
                    item.WriteXml(writer);
                }
            writer.WriteEndElement();

        }

        #endregion
    }
}
