using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    [DataContract(IsReference = true)]
    public class TennisContestant : IXmlSerializable
    {
        [DataMember]
        public List<TennisPlayer> Players { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public int SetsWon { get; set; }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public int ContestantNr { get; set; }

        /// <summary>
        /// True if this contestant contains the local player of this device
        /// </summary>
        public bool ContainsLocalPlayer
        {
            get
            {
                foreach (TennisPlayer Player in Players)
                {
                    if (Player.LocalPlayer == true) return true;
                }

                return false;
            }
        }

	    public TennisContestant() {

		    Players = new List<TennisPlayer>();
		    MaxPlayers = 2;
		    ID = 0;
	    }
	
	    public void Add(TennisPlayer newPlayer)
	    {		
		    if (Players.Count == MaxPlayers) throw new Exception("Maximum number of players exceeded.");
		
		    Players.Add(newPlayer);
	    }
	
	    public String getName()
	    {
		    String Result = "";
		
		    foreach (TennisPlayer Player in Players)
		    {
			    if (Result.Length > 0) Result += " / ";
			    Result+= Player.Name;
		    }
		
		    return Result;
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
        public void ReadXml(System.Xml.XmlReader reader)
        {
            String NodeName = reader.Name;

            String _ID = reader.GetAttribute("ID");
            String _SetsWon = reader.GetAttribute("SetsWon");
            String _MaxPlayers = reader.GetAttribute("MaxPlayers");
            String _ContestantNr = reader.GetAttribute("ContestantNr");

            ID = int.Parse(_ID);
            SetsWon = int.Parse(_SetsWon);
            MaxPlayers = int.Parse(_MaxPlayers);
            ContestantNr = int.Parse(_ContestantNr);

            //Continu to end of node
            while (reader.Read() && !(reader.Name == NodeName && reader.NodeType == System.Xml.XmlNodeType.EndElement))
            {
                switch (reader.Name)
                {
                    case "Player":

                        TennisPlayer newPlayer = new TennisPlayer();
                        newPlayer.ReadXml(reader);

                        Players.Add(newPlayer);

                        break;
                }
            }
        }

        /// <summary>
        /// Converts this match into its XML representation.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("ID", ID.ToString());
            writer.WriteAttributeString("SetsWon", SetsWon.ToString());
            writer.WriteAttributeString("MaxPlayers", MaxPlayers.ToString());
            writer.WriteAttributeString("ContestantNr", ContestantNr.ToString());

            //players
            writer.WriteStartElement("Players");

            foreach (var item in Players)
            {
                item.WriteXml(writer);    
            }

            writer.WriteEndElement();
        }

        #endregion

    }
}
