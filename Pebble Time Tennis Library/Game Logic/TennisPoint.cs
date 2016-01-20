using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    [DataContract(IsReference=true)]
    public class TennisPoint
    {
        #region Enumerators

        public enum PointServe	{FirstServe, SecondServe};
        public enum PointResultType { Unknown, Winner, ForcedError, UnforcedError };
        public enum PointShot { Unknown, Forehand, Backhand, ApproachShot, GroundStroke, Volley, OverheadShot, Lob, Passing, Dropshot, Ace, Serve };
        public enum PointError { Unknown, DoubleFault, Long, Wide, Net, Other };
        public enum PointAce { Unknown, Wide, Line, Body };
        public enum PointType { Regular, GamePoint, BreakPoint, SetPoint, SetPointServer, MatchPoint, MatchPointServer }

        #endregion

        #region Constructor

        public TennisPoint()
	    {
		    Winner = 0;
		    PartOf = null;
            Type = new List<PointType>(1);
		    Type.Add(PointType.Regular);
		    ResultType = PointResultType.Unknown;
            Shot = new List<PointShot>();
            //Shot.Add(PointShot.Unknown);
		    Error = PointError.Unknown;
		    Ace = PointAce.Unknown;
		    Serve = PointServe.FirstServe;
	    }

        #endregion

        #region Fields

        [DataMember]
        public long ID { get; set; }

        [DataMember]
        public int Winner { get; set; }

        [DataMember]
        public int Server { get; set; }

        [DataMember]
        public TennisGame PartOf { get; set; }

        [DataMember]
        public PointServe Serve { get; set; }

        [DataMember]
        public PointResultType ResultType { get; set; }

        [DataMember]
        public List<PointShot> Shot { get; set; }

        [DataMember]
        public PointError Error { get; set; }

        [DataMember]
        public PointAce Ace { get; set; }

        [DataMember]
        public List<PointType> Type { get; set; }

        #endregion

        #region Commands
        /// <summary>
        /// Process the Ace command
        /// </summary>
        public void CommandAce()
        {
            ResultType = PointResultType.Winner;
            Shot.Add(PointShot.Ace);

            if (PartOf == null) throw new Exception("No reference to a game available. Unable to determine winner.");

            if (PartOf.Server == GetLocalPlayer()) 
            {
                CommandWin();
            }
            else
            {
                CommandLose();
            }
        }

        /// <summary>
        /// Process the Second serve command
        /// </summary>
        public void CommandSecondServe()
        {
            Serve = PointServe.SecondServe;
        }

        /// <summary>
        /// Process the Second serve command
        /// </summary>
        public void CommandDoubleFault()
        {
            ResultType = PointResultType.UnforcedError;
            Error = PointError.DoubleFault;
            Shot.Add(PointShot.Serve);

            if (PartOf == null) throw new Exception("No reference to a game available. Unable to determine winner.");

            if (PartOf.Server == GetLocalPlayer())
            {
                CommandLose();
            }
            else
            {
                CommandWin();
            }
        }

        /// <summary>
        /// Process the Win command
        /// </summary>
        public void CommandWin()
        {
            Winner = GetLocalPlayer();
        }

        /// <summary>
        /// Process the Lose command
        /// </summary>
        public void CommandLose()
        {
            Winner = 3 - GetLocalPlayer();
        }

        /// <summary>
        /// Proces the Winner command
        /// </summary>
        public void CommandWinner()
        {
            ResultType = PointResultType.Winner;
        }

        /// <summary>
        /// Process the Unforced error command
        /// </summary>
        public void CommandUnforcedError()
        {
            ResultType = PointResultType.UnforcedError;
        }

        /// <summary>
        /// Process the forced error command
        /// </summary>
        public void CommandForcedError()
        {
            ResultType = PointResultType.ForcedError;
        }

        /// <summary>
        /// Process the forehand command
        /// </summary>
        public void CommandForehand()
        {
            Shot.Add(PointShot.Forehand);
            Shot.Add(PointShot.GroundStroke);
        }

        /// <summary>
        /// Process the backhand command
        /// </summary>
        public void CommandBackhand()
        {
            Shot.Add(PointShot.Backhand);
            Shot.Add(PointShot.GroundStroke);
        }

        /// <summary>
        /// Process the dropshot command
        /// </summary>
        public void CommandDropshot()
        {
            Shot.Add(PointShot.Dropshot);
        }

        /// <summary>
        /// Process the lob command
        /// </summary>
        public void CommandLob()
        {
            Shot.Add(PointShot.Lob);
        }

        /// <summary>
        /// Process the volley command
        /// </summary>
        public void CommandVolley()
        {
            Shot.Add(PointShot.Volley);
        }

        /// <summary>
        /// Process the passing command
        /// </summary>
        public void CommandPassing()
        {
            Shot.Add(PointShot.Passing);
        }

        /// <summary>
        /// Process the smash command
        /// </summary>
        public void CommandSmash()
        {
            Shot.Add(PointShot.OverheadShot);
        }

        /// <summary>
        /// Process the body command (Ace - shot)
        /// </summary>
        public void CommandAceBody()
        {
            Ace = PointAce.Body;
        }

        /// <summary>
        /// Process the wide command (Ace - shot)
        /// </summary>
        public void CommandAceWide()
        {
            Ace = PointAce.Wide;
        }

        /// <summary>
        /// Process the line command (Ace - shot)
        /// </summary>
        public void CommandAceLine()
        {
            Ace = PointAce.Line;
        }

        /// <summary>
        /// Process the error - shot long command
        /// </summary>
        public void CommandErrorLong()
        {
            Error = PointError.Long;
        }

        /// <summary>
        /// Process the error - shot wide command
        /// </summary>
        public void CommandErrorWide()
        {
            Error = PointError.Long;
        }

        /// <summary>
        /// Process the error - net command
        /// </summary>
        public void CommandErrorNet()
        {
            Error = PointError.Net;
        }

        /// <summary>
        /// Process the error - other command
        /// </summary>
        public void CommandErrorOther()
        {
            Error = PointError.Other;
        }

        #endregion

        #region Methods

        public String GetShot()
        {
            if (Shot.Count == 0) return PointShot.Unknown.ToString();

            return Shot[0].ToString();
        }

        private int m_LocalPlayer = 0;
        public int GetLocalPlayer()
        {
            if (m_LocalPlayer == 0)
            {
                TennisGame Game = this.PartOf;
                TennisSet Set = Game.PartOf;
                TennisMatch Match = Set.PartOf;

                m_LocalPlayer = 1;
                //if (Match.Contestant1.ContainsLocalPlayer) m_LocalPlayer = 1;
                if (Match.Contestant2.ContainsLocalPlayer) m_LocalPlayer = 2;
            }

            return m_LocalPlayer;
        }

        #endregion

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
        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Name != "Point") throw new Exception("Unexpected node encountered, Point expected");

            String _ID = reader.GetAttribute("ID");
            String _Winner = reader.GetAttribute("Winner");
            String _Server = reader.GetAttribute("Server");
            String _Serve = reader.GetAttribute("Serve");
            String _ResultType = reader.GetAttribute("ResultType");
            String _Error = reader.GetAttribute("Error");
            String _Ace = reader.GetAttribute("Ace");

            ID = long.Parse(_ID);
            Winner = int.Parse(_Winner);
            Server = int.Parse(_Server);
            Serve = (PointServe)System.Enum.Parse(typeof(PointServe), _Serve);
            ResultType = (PointResultType)System.Enum.Parse(typeof(PointResultType), _ResultType);
            Error = (PointError)System.Enum.Parse(typeof(PointError), _Error);
            Ace = (PointAce)System.Enum.Parse(typeof(PointAce), _Ace);

            //Process Shots and Types
            while (!(reader.Name == "Point" && reader.NodeType == System.Xml.XmlNodeType.EndElement) && reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Shot":

                            while (reader.Name == "Shot")
                            {
                                String _Shot = reader.ReadElementContentAsString();
                                Shot.Add((PointShot)System.Enum.Parse(typeof(PointShot), _Shot));
                            }

                            break;


                        case "Type":

                            while (reader.Name == "Type")
                            {
                                String _Type = reader.ReadElementContentAsString();
                                Type.Add((PointType)System.Enum.Parse(typeof(PointType), _Type));
                            }

                            break;
                    }                    
                }
            }
        }

        /// <summary>
        /// Converts this match into its XML representation.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Point");

            //General
            writer.WriteAttributeString("ID", this.ID.ToString());
            writer.WriteAttributeString("Winner", this.Winner.ToString());
            writer.WriteAttributeString("Server", this.Server.ToString());
            writer.WriteAttributeString("Serve", this.Serve.ToString());
            writer.WriteAttributeString("ResultType", this.ResultType.ToString());
            writer.WriteAttributeString("Error", this.Error.ToString());
            writer.WriteAttributeString("Ace", this.Ace.ToString());

            //Shots
            writer.WriteStartElement("Shots");

            foreach(var item in Shot)
            {
                writer.WriteElementString("Shot", item.ToString());
            }
            
            writer.WriteEndElement();

            //Type
            writer.WriteStartElement("Types");

            foreach (var item in Type)
            {
                writer.WriteElementString("Type", item.ToString());
            }

            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        #endregion

    }
}
