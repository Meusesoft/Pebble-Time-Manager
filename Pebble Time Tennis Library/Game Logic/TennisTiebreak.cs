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
    public class TennisTiebreak : TennisGame
    {
      [DataMember]
      public int startServer { get; set; }
      
      [DataMember]
      public int startReceiver { get; set; }
	
	    public TennisTiebreak() {

		    PointsPerGame = 7;
	    }

        public new void Start() 
	    {
		    base.Start();
		
		    startServer = Server;
		    startReceiver = Receiver;
	    }

        public new Boolean Undo()
	    {
		    if (Points.Count == 0) return false; // can't undo
		
		    Points.Remove(Points.Last());
		    State = StateEnum.sProgress;
		    EndTime = DateTime.Now;
		    Winner = 0;
		
		    RebuildScore();
		
		    return true;
	    }

        public Boolean Add(TennisPoint newPoint) 
	    {
		    if (State != StateEnum.sProgress)
		    {
			    throw new Exception("This game is not in progress");
		    }
		
		    Points.Add(newPoint);
		
		    RebuildScore();
		
		    if ((PointsServer >= PointsPerGame && PointsReceiver < PointsServer-1) ||
                (PointsReceiver >= PointsPerGame && PointsServer < PointsReceiver - 1))
		    {
			    End(PointsServer>PointsReceiver ? startServer : startReceiver);
		    }
		
		    return true;		
	    }
	
	    private void RebuildScore()
	    {
		    PointsServer = 0;
		    PointsReceiver = 0;
		
		    Server = startServer;
		    Receiver = startReceiver;

		    foreach (TennisPoint point in Points) {
			
			    if (point.Winner == startServer) PointsServer++;
			    else PointsReceiver++;			
		    }
		
		    //Server serves points       1,4,5,8,9,12,13
		    //Receivers servers points   2,3,6,7,10,11
		    int CalcServer = (Points.Count / 2) + Points.Count % 2; //who serves the next point
            if (CalcServer % 2 == 1) 
			    {
                    Server = startReceiver;
                    Receiver = startServer;
			    }
	    }

        public new String Score()
	    {
		    if (State == StateEnum.sFinished) return "Game";
		
		    String Result;
		
		    Result = PointsServer + "";
		    if (startServer == Server) Result += "*";
		    Result += "-" + PointsReceiver;
		    if (startServer != Server) Result += "*";
		
		    return Result;	
	    }

        public String ScoreNaturalLanguage()
        {
            if (State == StateEnum.sFinished) return "This game is won by " + (Winner == Server ? "the server" : "the receiver");

            return String.Format("the score is {0} {1}", PointsServer, PointsReceiver);
        }

        public void SetStartServer(int newServer)
	    {
		    startServer = newServer;
		    startReceiver = 3 - newServer;
            Server = startServer;
            Receiver = startReceiver;
	    }

        public int GetStartServer()
	    {
		    return startServer;
	    }

        public override String getScoreContestant(int Contestant)
	    {
		    if (Contestant == startServer) return PointsServer + "";
		    else return PointsReceiver + "";
	    }

        public override Boolean BreakPoint()
	    {
		    //Break points don't count in tie break.
		    return false;
	    }
	
	    public override Boolean GamePoint()
	    {
		    //Determine if the server has a setpoint		
		    if (startServer == Server)
		    {
			    return (PointsServer >= PointsPerGame - 1 && PointsServer > PointsReceiver);		
		    }
		    else
		    {
			    return (PointsReceiver >= PointsPerGame - 1 && PointsReceiver > PointsServer);		
		    }
	    }	
		
	    public int SetPoint()
	    {
		    Boolean setpoint = (Math.Max(PointsServer, PointsReceiver) >= PointsPerGame - 1 && PointsServer != PointsReceiver);
		
		    if (setpoint)
		    {
			    if (PointsServer > PointsReceiver) return startServer;
			    else return startReceiver;
		    }

		    return 0;
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
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Name != "Tiebreak") throw new Exception("Unexpected node encountered, Tiebreak expected");

            String _StartServer = reader.GetAttribute("StartServer");
            String _StartReceiver = reader.GetAttribute("StartReceiver");
            startServer = int.Parse(_StartServer);
            startReceiver = int.Parse(_StartReceiver);

            ReadXmlBase(reader, "Tiebreak");

        }

        /// <summary>
        /// Converts this match into its XML representation.
        /// </summary>
        /// <param name="writer"></param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Tiebreak");

            writer.WriteAttributeString("StartServer", this.startServer.ToString());
            writer.WriteAttributeString("StartReceiver", this.startReceiver.ToString());

            WriteXmlBase(writer);

            writer.WriteEndElement();
        }

        #endregion

    }
}
