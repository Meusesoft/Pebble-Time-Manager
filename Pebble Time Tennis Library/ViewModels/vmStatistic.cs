using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Tennis_Statistics.Game_Logic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Tennis_Statistics.ViewModels
{
    [DataContract]
    public class vmStatistic : INotifyPropertyChanged
    {
        public enum StatisticElement
        {
            [EnumMember] Unknown,
            [EnumMember] TotalHeader,
            [EnumMember] TotalAces,
            [EnumMember] TotalDoubleFaults,
            [EnumMember] TotalServicePoints,
            [EnumMember] TotalReturnPoints,
            [EnumMember] TotalWinners,
            [EnumMember] TotalForcedErrors,
            [EnumMember] TotalUnforcedErrors,
            [EnumMember] TotalPoints,
            [EnumMember] ServiceHeader,
            [EnumMember] FirstServicePercentage,
            [EnumMember] FirstServiceWon,
            [EnumMember] SecondServiceWon,
            [EnumMember] BreakpointsSaved,
            [EnumMember] ServiceGamesPercentage,
            [EnumMember] ReturnHeader,
            [EnumMember] FirstReturnWon,
            [EnumMember] SecondReturnWon,
            [EnumMember] BreakpointsWon,
            [EnumMember] ReturnGamesPercentage,
            [EnumMember] ShotsHeader,
            [EnumMember] ForehandWinner,
            [EnumMember] ForehandForcedError,
            [EnumMember] ForehandUnforcedError,
            [EnumMember] BackhandWinner,
            [EnumMember] BackhandForcedError,
            [EnumMember] BackhandUnforcedError,
            [EnumMember] VolleyWinner,
            [EnumMember] VolleyForcedError,
            [EnumMember] VolleyUnforcedError
        }

        private Dictionary<StatisticElement, String> m_Descriptions;
        private Dictionary<StatisticElement, String> Descriptions
        {
            get
            {
                if (m_Descriptions == null)
                {
                    m_Descriptions = new Dictionary<StatisticElement, String>
                        {
                            {StatisticElement.TotalHeader, "Totals"},
                            {StatisticElement.TotalAces, "Aces"},
                            {StatisticElement.TotalDoubleFaults, "Double Faults"},
                            {StatisticElement.TotalWinners, "Winners"},
                            {StatisticElement.TotalUnforcedErrors, "Unforced errors"},
                            {StatisticElement.TotalForcedErrors, "Forced errors"},
                            {StatisticElement.TotalServicePoints, "Service points"},
                            {StatisticElement.TotalReturnPoints, "Return points"},
                            {StatisticElement.TotalPoints, "Points"},
                            {StatisticElement.ServiceHeader, "Service"},
                            {StatisticElement.FirstServicePercentage, "First service"},
                            {StatisticElement.FirstServiceWon, "First service won"},
                            {StatisticElement.SecondServiceWon, "Second service won"},
                            {StatisticElement.BreakpointsSaved, "Break points saved"},
                            {StatisticElement.ServiceGamesPercentage, "Service games"},
                            {StatisticElement.ReturnHeader, "Return"},
                            {StatisticElement.FirstReturnWon, "First return won"},
                            {StatisticElement.SecondReturnWon, "Second return won"},
                            {StatisticElement.BreakpointsWon, "Break points won"},
                            {StatisticElement.ReturnGamesPercentage, "Return games"},
                            {StatisticElement.ShotsHeader, "Shots"},
                            {StatisticElement.ForehandWinner, "Forehand winner"},
                            {StatisticElement.ForehandUnforcedError, "FH unforced error"},
                            {StatisticElement.ForehandForcedError, "FH forced error"},
                            {StatisticElement.BackhandWinner, "Backhand winner"},
                            {StatisticElement.BackhandUnforcedError, "BH unforced error"},
                            {StatisticElement.BackhandForcedError, "BH forced error"},
                            {StatisticElement.VolleyWinner, "Volley winner"},
                            {StatisticElement.VolleyUnforcedError, "V unforced error"},
                            {StatisticElement.VolleyForcedError, "V forced error"},
                        };
                }

                return m_Descriptions;
            }
        }





        #region Properties
        private String _Data;
        /// <summary>
        /// The string presentation of this statistics for contestant 1
        /// </summary>
        private String m_Value1;
        [DataMember]
        public String Value1
        {
            get
            {
                if (Source == null) return m_Value1;
                return CalculateValue(1);
            }
            set
            {
                m_Value1 = value;
            }
        }

        /// <summary>
        /// The string presentation of this statistics for contestant 2
        /// </summary>
        private String m_Value2;
        [DataMember]
        public String Value2
        {
            get
            {
                if (Source == null) return m_Value2;
                return CalculateValue(2);
            }
            set
            {
                m_Value2 = value;
            }
        }

        /// <summary>
        /// The description of this statistic
        /// </summary>
        private String m_Description;
        [DataMember]
        public String Description
        {
            get
            {
                if (Statistic == StatisticElement.Unknown) return m_Description;
                if (Descriptions.ContainsKey(Statistic)) return Descriptions[Statistic].ToUpper();
                return "Unknown";
            }
            set
            {
                m_Description = value;
            }
        }

        /// <summary>
        /// The statistical element this instance contains and represents
        /// </summary>
        [DataMember]
        public StatisticElement Statistic { get; set; }

        /// <summary>
        /// The source of the statistics
        /// </summary>
        public TennisStatistics Source { get; set; }

        /// <summary>
        /// True if this element is a header
        /// </summary>
        private bool m_Header;
        [DataMember]
        public bool Header
        {
            get
            {
                if (Statistic == StatisticElement.Unknown) return m_Header;
                switch (Statistic)
                {
                    case StatisticElement.ServiceHeader:
                    case StatisticElement.ReturnHeader:
                    case StatisticElement.TotalHeader:
                    case StatisticElement.ShotsHeader:

                        return true;

                    default:

                        return false;
                }
            }
            set
            {
                m_Header = value;
            }
        }

        /// <summary>
        /// True if this statistic is visible, and false if it is hidden / collapsed
        /// </summary>
        [DataMember]
        public bool Visible { get; set; }

        /// <summary>
        /// Al list of related / children statistics. Used for a tree type hierarchy Header->Statistics
        /// </summary>
        private List<vmStatistic> m_Children;
        [DataMember]
        public List<vmStatistic> Children
        {
            get
            {
                if (m_Children == null) m_Children = new List<vmStatistic>(0);
                return m_Children;
            }
            set
            {
                m_Children = value;
            }
        }

        #endregion

        #region Methods

        private String CalculateValue(int Contestant)
        {

            string Result = "";

            switch (Statistic)
            {
                case StatisticElement.TotalAces:

                    Result = String.Format("{0}", Source.GetItem(Statistics.Aces, Contestant));

                    break;

                case StatisticElement.TotalDoubleFaults:

                    Result = String.Format("{0}", Source.GetItem(Statistics.DoubleFaults, Contestant));

                    break;

                case StatisticElement.TotalWinners:

                    Result = String.Format("{0}", Source.GetItem(Statistics.PointWinners, Contestant));

                    break;

                case StatisticElement.TotalUnforcedErrors:

                    Result = String.Format("{0}", Source.GetItem(Statistics.PointUnforcedErrors, Contestant));

                    break;

                case StatisticElement.TotalForcedErrors:

                    Result = String.Format("{0}", Source.GetItem(Statistics.PointForcedErrors, Contestant));

                    break;

                case StatisticElement.FirstServicePercentage:

                    Result = BuildPercentageNotation(Contestant, Statistics.FirstServicesPlayed, Statistics.TotalServicePoints);

                    break;

                case StatisticElement.FirstServiceWon:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.FirstServicesWon, Statistics.FirstServicesPlayed);

                    break;

                case StatisticElement.SecondServiceWon:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.SecondServicesWon, Statistics.SecondServicesPlayed);

                    break;

                case StatisticElement.BreakpointsSaved:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.ServiceBreakPointsWon, Statistics.ServiceBreakPointsPlayed);

                    break;

                case StatisticElement.ServiceGamesPercentage:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.ServiceGamesWon, Statistics.ServiceGamesPlayed);

                    break;

                case StatisticElement.FirstReturnWon:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.FirstReturnWon, Statistics.FirstReturnPlayed);

                    break;

                case StatisticElement.SecondReturnWon:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.SecondReturnWon, Statistics.SecondReturnPlayed);

                    break;

                case StatisticElement.BreakpointsWon:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.ReturnBreakPointsWon, Statistics.ReturnBreakPointsPlayed);

                    break;

                case StatisticElement.ReturnGamesPercentage:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.ReturnGamesWon, Statistics.ReturnGamesPlayed);

                    break;

                case StatisticElement.TotalServicePoints:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.TotalServicePointsWon, Statistics.TotalServicePoints);

                    break;

                case StatisticElement.TotalReturnPoints:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.TotalReturnPointsWon, Statistics.TotalReturnPoints);

                    break;

                case StatisticElement.TotalPoints:

                    Result = BuildPercentageNotationLong(Contestant, Statistics.TotalPointsWon, Statistics.TotalPoints);

                    break;

                case StatisticElement.ForehandWinner:

                    Result = String.Format("{0}", Source.GetItem(Statistics.ForehandWinner, Contestant));

                    break;

                case StatisticElement.ForehandForcedError:

                    Result = String.Format("{0}", Source.GetItem(Statistics.ForehandForcedError, Contestant));

                    break;

                case StatisticElement.ForehandUnforcedError:

                    Result = String.Format("{0}", Source.GetItem(Statistics.ForehandUnforcedError, Contestant));

                    break;

                case StatisticElement.BackhandWinner:

                    Result = String.Format("{0}", Source.GetItem(Statistics.BackhandWinner, Contestant));

                    break;

                case StatisticElement.BackhandForcedError:

                    Result = String.Format("{0}", Source.GetItem(Statistics.BackhandForcedError, Contestant));

                    break;

                case StatisticElement.BackhandUnforcedError:

                    Result = String.Format("{0}", Source.GetItem(Statistics.BackhandUnforcedError, Contestant));

                    break;

                case StatisticElement.VolleyWinner:

                    Result = String.Format("{0}", Source.GetItem(Statistics.VolleyWinner, Contestant));

                    break;

                case StatisticElement.VolleyForcedError:

                    Result = String.Format("{0}", Source.GetItem(Statistics.VolleyForcedError, Contestant));

                    break;

                case StatisticElement.VolleyUnforcedError:

                    Result = String.Format("{0}", Source.GetItem(Statistics.VolleyUnforcedError, Contestant));

                    break;

                default:

                    Result = "";

                    break;
            }

            return Result;
        }

        /// <summary>
        /// Switch the visibility of the items
        /// </summary>
        public void SwitchVisibility()
        {
            Visible = !Visible;
            NotifyPropertyChanged("Visible");

            /*if (Header)
            {

                foreach (vmStatistic item in Children)
                {
                    item.SwitchVisibility();
                }
            }
            else
            {
                Visible = !Visible;
                NotifyPropertyChanged("Visible");
            }*/
        }

        private String BuildPercentageNotation(int Contestant, Statistics Dividend, Statistics Diviser)
        {
            int iDividend = Source.GetItem(Dividend, Contestant);
            int iDiviser = Source.GetItem(Diviser, Contestant);

            if (iDiviser == 0) return "0%";

            return String.Format("{0:0%}", (float)iDividend / (float)iDiviser);
        }

        private String BuildPercentageNotationLong(int Contestant, Statistics Dividend, Statistics Diviser)
        {
            int iDividend = Source.GetItem(Dividend, Contestant);
            int iDiviser = Source.GetItem(Diviser, Contestant);
            if (iDiviser == 0) return "0/0 (0%)";

            return String.Format("{0}/{1} ({2:0%})", iDividend, iDiviser, (float)iDividend / (float)iDiviser);
        }

        public void Notify()
        {
            NotifyPropertyChanged("Value1");
            NotifyPropertyChanged("Value2");

            foreach (vmStatistic Element in Children)
            {
                Element.Notify();
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
