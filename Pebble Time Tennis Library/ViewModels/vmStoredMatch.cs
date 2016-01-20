using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Tennis_Statistics.Game_Logic;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Tennis_Statistics.ViewModels
{
    [DataContract]
    public class vmStoredMatch : INotifyPropertyChanged
    {
        #region Constructors

        public vmStoredMatch() { }

        public vmStoredMatch(TennisMatch Match)
        {
            GetData(Match);
        }

        #endregion

        #region Properties

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public TimeSpan Duration { get; set; }

        [DataMember]
        public TennisMatch.MatchStatus Status { get; set; }

        [DataMember]
        public int Winner { get; set; }

        [DataMember]
        public String Contestant1 { get; set; }

        [DataMember]
        public String Contestant2 { get; set; }

        [DataMember]
        public String Surface { get; set; }

        [DataMember]
        public String Location { get; set; }

        private List<vmSetScore> m_Sets = new List<vmSetScore>(1);

        [DataMember]
        public List<vmSetScore> Sets
        {
            get
            {
                return m_Sets;
            }
            set
            {
                m_Sets = value;
            }
        }
        
        [DataMember]
        public String MatchType { get; set; }

        private bool m_isLastMatch;
        public bool IsLastMatch
        {
            get
            {
                return m_isLastMatch;
            }
            set
            {
                m_isLastMatch = value;
                NotifyPropertyChanged("IsLastMatch");
            }
        }


        public String SetsString
        {
            get
            {
                String Result="";

                foreach (vmSetScore Set in this.Sets)
                {
                    Result += String.Format("{0}-{1} ", Set.Score1, Set.Score2);
                }
                return Result;
            }
        }

        public String CombinedStatus
        {
            get
            {
                return String.Format("{0} | {1} | {2}", StatusRepresentation, MatchType, SetsString);
            }
        }

        public String CombinedSurfaceAndLocation
        {
            get
            {
                return String.Format("{0} | {1}", Surface.ToUpper(), Location.ToUpper());
            }
        }

        public String StatusRepresentation
        {
            get
            {
                if (Winner != 0)
                {
                    return String.Format("Winner: {0}", Winner == 1 ? Contestant1 : Contestant2);
                }
                
                //Set the status
                switch (this.Status)
                {
                    //case TennisMatch.MatchStatus.Completed:

                    //    return String.Format("Winner: {0}", Winner == 1 ? Contestant1 : Contestant2);

                    case TennisMatch.MatchStatus.Resigned:

                       return String.Format("Resigned by {0}", Winner == 1 ? Contestant2 : Contestant1);

                    case TennisMatch.MatchStatus.Terminated:

                        return "Terminated";

                    default:

                        return "In progress";
                }
            }
        }

        #endregion

        #region Methods

        public void GetData(TennisMatch Match)
        {
            ID = Match.ID;
            StartTime = Match.Duration.FirstSession;
            Duration = Match.Duration.Duration;
            Winner = Match.Winner;
            Status = Match.Status;
            Contestant1 = Match.Contestant1.getName();
            Contestant2 = Match.Contestant2.getName();
            MatchType = Match.Type.ToString() + " | best of " + Match.BestOutOf.ToString();
            MatchType = MatchType.ToUpper();
            Location = Match.Location;
            Surface = Match.MatchSurface == TennisMatch.Surface.ArtificialGrass ? "Artificial grass" :  Match.MatchSurface.ToString();

            Sets.Clear();

            foreach(TennisSet Set in Match.Sets)
            {
                vmSetScore _sSet = new vmSetScore(Set);
                Sets.Add(_sSet);
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

    }
}
