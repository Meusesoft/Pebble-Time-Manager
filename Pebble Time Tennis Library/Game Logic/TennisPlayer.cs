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
    public class TennisPlayer : IXmlSerializable
    {
        [DataMember]
        public String ID { get; set; }

        [DataMember]
        public String Name {get; set;}

        [DataMember]
        public bool LocalPlayer { get; set; }

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
            if (reader.Name != "Player") throw new Exception("Unexpected node encountered, Player expected");

            String _ID = reader.GetAttribute("ID");
            String _Name = reader.GetAttribute("Name");
            String _LocalPlayer = reader.GetAttribute("LocalPlayer");

            if (_ID != null) ID = _ID;
            Name = _Name;
            LocalPlayer = bool.Parse(_LocalPlayer);
        }

        /// <summary>
        /// Converts this match into its XML representation.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Player");
            
            if (ID != null) writer.WriteAttributeString("ID", ID.ToString());
            writer.WriteAttributeString("Name", Name.ToString());
            writer.WriteAttributeString("LocalPlayer", LocalPlayer.ToString());

            writer.WriteEndElement();

        }

        #endregion

    }
}
