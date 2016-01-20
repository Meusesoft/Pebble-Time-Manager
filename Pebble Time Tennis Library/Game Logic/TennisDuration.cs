using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    public class TennisDuration : List<DateTime>, IXmlSerializable
    {
        #region Properties

        /// <summary>
        /// True if a session is in progress
        /// </summary>
        public bool SessionInProgress
        {
            get
            {
                return (this.Count % 2) != 0;
            }
        }

        /// <summary>
        /// Return the total duration of all sessions
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                TimeSpan Total = TimeSpan.Zero;
                DateTime LastStart = DateTime.MinValue;
                Enumerator _Enumerator = this.GetEnumerator();

                if (this.Count > 0)
                {
                    do
                    {
                        if (LastStart == DateTime.MinValue)
                        {
                            LastStart = _Enumerator.Current;
                        }
                        else
                        {
                            Total += _Enumerator.Current - LastStart;
                            LastStart = DateTime.MinValue;
                        }
                    } while (_Enumerator.MoveNext());

                    if (LastStart != DateTime.MinValue)
                    {
                        Total += DateTime.Now - LastStart;
                    }
                }

                return Total;
            }
        }

        /// <summary>
        /// The start date of the first session 
        /// </summary>
        public DateTime FirstSession
        {
            get
            {
                if (Count == 0) return DateTime.MinValue;

                return this[0];
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start a session
        /// </summary>
        public void Start()
        {
            if (!SessionInProgress)
            {
                Add(DateTime.Now);
            }
            else
            {
                throw new Exception("Can't start session. Session is in progress.");
            }
        }

        /// <summary>
        /// End a session
        /// </summary>
        public void End()
        {
            if (SessionInProgress)
            {
                Add(DateTime.Now);
            }
            else
            {
                throw new Exception("Can't end session. No session is in progress.");
            }
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
            if (reader.Name != "Duration") throw new Exception("Unexpected node encountered, Duration expected");

            while (!(reader.Name == "Duration" && reader.NodeType == System.Xml.XmlNodeType.EndElement) && reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    while (reader.Name == "TimeStamp")
                    {
                        String _TimeStamp = reader.ReadElementContentAsString();
                        
                        Add(DateTime.Parse(_TimeStamp));
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
            foreach (var item in this)
            {
                writer.WriteElementString("TimeStamp", item.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff"));
            }
        }

        #endregion
    }
}
