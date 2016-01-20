using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    
    [DataContract(IsReference = true)]
    [KnownType(typeof(TennisTiebreak))]
    public class TennisGame : IXmlSerializable
    {
        public enum ScoreEnum{s0, s15, s30, s40, sAdvantage};
        public enum StateEnum{sNone, sProgress, sFinished};

        [DataMember]
        public List<TennisPoint> Points { get; set; }

        [DataMember]
        public int Server { get; set; }
        
        [DataMember]
        public int Receiver { get; set; }
        
        [DataMember]
        public int Winner { get; set; }
        
        [DataMember]
        public ScoreEnum ScoreServer { get; set; }
        
        [DataMember]
        public ScoreEnum ScoreReceiver { get; set; }
        
        [DataMember]
        public StateEnum State { get; set; }
        
        [DataMember]
        public DateTime StartTime { get; set; }
        
        [DataMember]
        public DateTime EndTime { get; set; }
        
        [DataMember]
        public int PointsServer { get; set; }
        
        [DataMember]
        public int PointsReceiver { get; set; }
        
        [DataMember]
        public int PointsPerGame { get; set; }
        
        [DataMember]
        public long ID { get; set; }

        [DataMember]
        public TennisSet PartOf { get; set; }

	    public TennisGame()
	    {
		    Points = new List<TennisPoint>(0);
		    State = StateEnum.sNone;
		    ScoreServer = ScoreEnum.s0;
		    ScoreReceiver = ScoreEnum.s0;
		    PointsPerGame = 4;
		    PointsServer = 0;
		    PointsReceiver = 0;
		    Winner = 0;
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
        }

        public void Start() 
	    {
		    State = StateEnum.sProgress;
            StartTime = DateTime.Now;
		
		    if (Server==0 || Receiver==0)
		    {
			    throw new Exception("A tennis game needs two players.");
		    }
		
	    }

        public void End(int winner)
	    {
		    State = StateEnum.sFinished;
		    EndTime = DateTime.Now;
		    Winner = winner;
	    }

        public void setPoints(List<TennisPoint> points)
	    {
		    Points = points;
	    }

        public virtual String getScoreContestant(int Contestant)
	    {
		    if (Server == Contestant) return ParseScore(ScoreServer);
		    else return ParseScore(ScoreReceiver);
	    }

        public TennisPoint Undo()
	    {
		    if (Points.Count == 0) return null; // can't undo.

            TennisPoint PointUndone = Points.Last();
            Points.Remove(PointUndone);
		    State = StateEnum.sProgress;
		    Winner = 0;
		
		    RebuildScore();

            return PointUndone;
	    }
	
	    private void RebuildScore()
	    {
		    PointsServer = 0;
		    PointsReceiver = 0;
		
		    foreach (TennisPoint point in Points) {
			
			    if (point.Winner == Server) PointsServer++;
			    else PointsReceiver++;
		    }

		    ScoreServer = ConvertScore(PointsServer);
		    ScoreReceiver = ConvertScore(PointsReceiver);
	    }
	
	    public ScoreEnum ConvertScore(int Value)
	    {
		    switch (Value)
		    {
			    case 0: return ScoreEnum.s0;
			    case 1: return ScoreEnum.s15;
			    case 2: return ScoreEnum.s30;
			    case 3: return ScoreEnum.s40;
		    }
		
		    if (Value % 1 == 1) return ScoreEnum.s40;
		    else return ScoreEnum.sAdvantage;
	    }

        public Boolean Add(TennisPoint newPoint) 
	    {
		    if (State != StateEnum.sProgress)
		    {
			    throw new Exception("This game is not in progress");
		    }
		
		    Points.Add(newPoint);
		
		    if (newPoint.Winner == Server) 
			    {
			    PointsServer++;
			    if (PointsServer >= PointsPerGame && PointsServer > PointsReceiver + 1) End(Server);
                if (PointsServer >= PointsPerGame && this.PartOf.PartOf.DeuceSuddenDeath) End(Server);
			    }
		    else 
			    {
			    PointsReceiver++;
			    if (PointsReceiver >= PointsPerGame && PointsReceiver > PointsServer + 1) End(Receiver);
                if (PointsReceiver >= PointsPerGame && this.PartOf.PartOf.DeuceSuddenDeath) End(Receiver);
                }
		
		    if (PointsServer == PointsPerGame && PointsReceiver == PointsPerGame) //Return to deuce.
			    {
			    PointsServer--;
			    PointsReceiver--;
			    }

		
		    ScoreServer = ConvertScore(PointsServer);
		    ScoreReceiver = ConvertScore(PointsReceiver);

		    return true;		
	    }


        public String Score()
	    {
		    if (State == StateEnum.sFinished) return "Game";
		
		    String Result;
		
		    Result = ParseScore(ScoreServer) + "-" + ParseScore(ScoreReceiver);
		
		    return Result;	
	    }

        public String ScoreNaturalLanguage(String SentenceFormat)
        {
            if (State == StateEnum.sFinished) return "This game is won by " + (Winner == Server ? "the server" : "the receiver");
            if (SentenceFormat.Length == 0) SentenceFormat = "The score is {0}";

            String Result;
            String _ScoreServer = ParseScore(ScoreServer);
            String _ScoreReceiver = ParseScore(ScoreReceiver);

            if (_ScoreServer == "0" && _ScoreReceiver != "0") _ScoreServer = "love";
            if (_ScoreReceiver == "0" && _ScoreServer != "0") _ScoreReceiver = "love";

            Result = _ScoreServer + " " + _ScoreReceiver;

            if (ScoreServer == ScoreEnum.sAdvantage) Result = "advantage server";
            if (ScoreReceiver == ScoreEnum.sAdvantage) Result = "advantage receiver";
            if (ScoreServer == ScoreReceiver && ScoreServer == ScoreEnum.s40) Result = "deuce";


            if (BreakPoint()) Result += ". breakpoint";

            return String.Format(SentenceFormat, Result);
        }

        public String ParseScore(ScoreEnum score)
	    {
		    switch (score)
		    {
			    case ScoreEnum.s0: return "0";
                case ScoreEnum.s15: return "15";
                case ScoreEnum.s30: return "30";
                case ScoreEnum.s40: return "40";
                case ScoreEnum.sAdvantage: return "A";
		    }
		
		    return "";
	    }
	
	    public virtual Boolean BreakPoint()
	    {
		    return ((PointsReceiver >= PointsPerGame - 1 && PointsReceiver > PointsServer) || (this.PartOf.PartOf.DeuceSuddenDeath && PointsReceiver == PointsPerGame - 1));
	    }
	
	    public virtual Boolean GamePoint()
	    {
            return ((PointsServer >= PointsPerGame - 1 && PointsServer > PointsReceiver) || (this.PartOf.PartOf.DeuceSuddenDeath && PointsServer == PointsPerGame - 1)); 
	    }	
	
	    public TimeSpan GameDuration()
	    {
            TimeSpan ElapsedTime;
		
		    switch (State)
		    {
		    case StateEnum.sFinished:
			
                ElapsedTime = EndTime - StartTime;
			
			    break;

            case StateEnum.sProgress:

			    DateTime currentDate = new DateTime();

                ElapsedTime = currentDate - StartTime;
			
			    break;
			
		    default:

                ElapsedTime = new TimeSpan(0);
			
			    break;
		    }

            return ElapsedTime;
	    }

        #region XML

        /// <summary>
        /// Return the XML schema.
        /// </summary>
        /// <returns>null</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates a match object from its XML representation.
        /// </summary>
        /// <param name="reader"></param>
        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Name != "Game") throw new Exception("Unexpected node encountered, Game expected");

            ReadXmlBase(reader, "Game");

        }
         
        public void ReadXmlBase(System.Xml.XmlReader reader, String Node)
        {            
            String _ID = reader.GetAttribute("ID");
            String _Winner = reader.GetAttribute("Winner");
            String _Server = reader.GetAttribute("Server");
            String _Receiver = reader.GetAttribute("Receiver");
            String _PointsServer = reader.GetAttribute("PointsServer");
            String _PointsReceiver = reader.GetAttribute("PointsReceiver");
            String _ScoreServer = reader.GetAttribute("ScoreServer");
            String _ScoreReceiver = reader.GetAttribute("ScoreReceiver");
            String _State = reader.GetAttribute("State");
            String _StartTime = reader.GetAttribute("StartTime");
            String _EndTime = reader.GetAttribute("EndTime");
            String _PointsPerGame = reader.GetAttribute("PointsPerGame");

            ID = long.Parse(_ID);
            Winner = int.Parse(_Winner);
            Server = int.Parse(_Server);
            Receiver = int.Parse(_Receiver);
            PointsServer = int.Parse(_PointsServer);
            PointsReceiver = int.Parse(_PointsReceiver);
            ScoreServer = (ScoreEnum)System.Enum.Parse(typeof(ScoreEnum), _ScoreServer);
            ScoreReceiver = (ScoreEnum)System.Enum.Parse(typeof(ScoreEnum), _ScoreReceiver);
            State = (StateEnum)System.Enum.Parse(typeof(StateEnum), _State);
            StartTime = DateTime.Parse(_StartTime);
            if (_EndTime != null) EndTime = DateTime.Parse(_EndTime);
            PointsPerGame = int.Parse(_PointsPerGame);

            //Process points
            while (reader.Read() && !(reader.Name == Node && reader.NodeType == System.Xml.XmlNodeType.EndElement))
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (reader.Name == "Point")
                    {
                        TennisPoint _newPoint = new TennisPoint();
                        _newPoint.PartOf = this;
                        _newPoint.ReadXml(reader);

                        Points.Add(_newPoint);
                    }
                }
            }
        }

        /// <summary>
        /// Converts this match into its XML representation.
        /// </summary>
        /// <param name="writer"></param>
        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Game");

            WriteXmlBase(writer);
        
            writer.WriteEndElement();
        }

        public virtual void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            //General
            writer.WriteAttributeString("ID", this.ID.ToString());
            writer.WriteAttributeString("Winner", this.Winner.ToString());
            writer.WriteAttributeString("Server", this.Server.ToString());
            writer.WriteAttributeString("Receiver", this.Receiver.ToString());
            writer.WriteAttributeString("PointsServer", this.PointsServer.ToString());
            writer.WriteAttributeString("PointsReceiver", this.PointsReceiver.ToString());
            writer.WriteAttributeString("ScoreServer", this.ScoreServer.ToString());
            writer.WriteAttributeString("ScoreReceiver", this.ScoreReceiver.ToString());
            writer.WriteAttributeString("State", this.State.ToString());
            writer.WriteAttributeString("StartTime", this.StartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff"));
            writer.WriteAttributeString("EndTime", this.StartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff"));
            writer.WriteAttributeString("PointsPerGame", this.PointsPerGame.ToString());

            //Points
            writer.WriteStartElement("Points");

            foreach (var item in Points)
            {
                item.WriteXml(writer);
            }

            writer.WriteEndElement();
        }

        #endregion

    }
}
