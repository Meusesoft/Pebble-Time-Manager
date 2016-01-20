using System;
using System.ComponentModel;
using Tennis_Statistics.Game_Logic;
using System.Runtime.Serialization;

namespace Tennis_Statistics.ViewModels
{
    [DataContract]
    public class vmSetScore : INotifyPropertyChanged
    {
        #region Constructors

        public vmSetScore()
        {
        }

        public vmSetScore(TennisSet _set)
        {
            Initialize(this, _set);
        }

        public vmSetScore(vmSetScore _setScore)
        {
            Initialize(_setScore);
        }

        #endregion

        #region Fields

        private String m_Score1;
        private String m_Score1Tiebreak;
        private String m_Score2;
        private String m_Score2Tiebreak;

        #endregion

        #region Properties

        /// <summary>
        /// The score of contestant 1
        /// </summary>
        [DataMember]
        public String Score1
        {
            get
            {
                return m_Score1;
            }
            set
            {
                m_Score1 = value;
                NotifyPropertyChanged("Score1");
            }
        }

        /// <summary>
        /// If this set ended in a tiebreak, this is the score of contestant 1 in that tiebreak
        /// </summary>
        [DataMember]
        public String Score1Tiebreak
        {
            get
            {
                return m_Score1Tiebreak;
            }
            set
            {
                m_Score1Tiebreak = value;
                NotifyPropertyChanged("Score1Tiebreak");
            }
        }

        /// <summary>
        /// The score of contestant 2
        /// </summary>
        [DataMember]
        public String Score2
        {
            get
            {
                return m_Score2;
            }
            set
            {
                m_Score2 = value;
                NotifyPropertyChanged("Score2");
            }
        }

        /// <summary>
        /// If this set ended in a tiebreak, this is the score of contestant 2 in that tiebreak
        /// </summary>
        [DataMember]
        public String Score2Tiebreak
        {
            get
            {
                return m_Score2Tiebreak;
            }
            set
            {
                m_Score2Tiebreak = value;
                NotifyPropertyChanged("Score2Tiebreak");
            }
        }

        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public bool InProgress { get; set; }

        [DataMember]
        public bool IsTiebreak { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize an instance with the contents of a tennisset
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="set"></param>
        public static void Initialize(vmSetScore instance, TennisSet set)
        {
            instance.Score1 = set.ScoreContestant1.ToString();
            instance.Score2 = set.ScoreContestant2.ToString();

            instance.Index = set.PartOf.Sets.IndexOf(set) + 1;
            instance.InProgress = (set.Winner == 0);

            if (set.Games.Count > 1)
            {
                TennisGame LastGame = set.Games[set.Games.Count - 1];
                if (LastGame.GetType() == typeof(TennisTiebreak))
                {
                    instance.IsTiebreak = true;

                    if (!instance.InProgress)
                    {
                        instance.Score1Tiebreak = LastGame.getScoreContestant(1);
                        instance.Score2Tiebreak = LastGame.getScoreContestant(2);
                    }
                }
            } 
        }

        /// <summary>
        /// Make a copy of the vmSetScore instance
        /// </summary>
        /// <param name="instance"></param>
        private void Initialize(vmSetScore instance)
        {
            if (instance != null)
            {
                Score1 = instance.Score1;
                Score2 = instance.Score2;
                Score1Tiebreak = instance.Score1Tiebreak;
                Score2Tiebreak = instance.Score2Tiebreak;
                IsTiebreak = instance.IsTiebreak;
                InProgress = instance.InProgress;
            }
        }

        /// <summary>
        /// Notify values possibly changed
        /// </summary>
        public void Notify()
        {
            NotifyPropertyChanged("Score1");
            NotifyPropertyChanged("Score2");
            NotifyPropertyChanged("Score1Tiebreak");
            NotifyPropertyChanged("Score2Tiebreak");
            NotifyPropertyChanged("IsTiebreak");
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
