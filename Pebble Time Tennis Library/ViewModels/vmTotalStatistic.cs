using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Tennis_Statistics.Game_Logic;

namespace Tennis_Statistics.ViewModels
{
    public class vmTotalStatistic : INotifyPropertyChanged
    {
        public enum StatisticElement
        {
            ServiceHeader, Aces, DoubleFaults, FirstServePercentage, FirstServePointsWon, SecondServePointsWon, BreakPointsSaved, ServiceGamesWon, ServicePointWon,
            ReturnHeader, FirstReturnWon, SecondReturnWon, BreakPointsWon, ReturnGamesWon, ReturnPointsWon,
            TotalHeader, TotalPointsWon
        }

        private Dictionary<StatisticElement, String> Descriptions = new Dictionary<StatisticElement, String>
        {
            {StatisticElement.ServiceHeader, "Service Record"},
            {StatisticElement.Aces, "Aces"},
            {StatisticElement.DoubleFaults, "Double Faults"},
            {StatisticElement.FirstServePercentage, "First Serve"},
            {StatisticElement.FirstServePointsWon, "First Serve Points Won"},
            {StatisticElement.SecondServePointsWon, "Second Serve Points Won"},
            {StatisticElement.BreakPointsSaved, "Break Points Won"},
            {StatisticElement.ServiceGamesWon, "Service Games Won"},
            {StatisticElement.ServicePointWon, "Service Points Won"},

            {StatisticElement.ReturnHeader, "Return Record"},
            {StatisticElement.FirstReturnWon, "First Serve Return Points Won"},
            {StatisticElement.SecondReturnWon, "Second Serve Return Points Won"},
            {StatisticElement.BreakPointsWon, "Break Points Converted"},
            {StatisticElement.ReturnGamesWon, "Return Games Won"},
            {StatisticElement.ReturnPointsWon, "Return Points Won"},

            {StatisticElement.TotalHeader, "Totals"},
            {StatisticElement.TotalPointsWon, "Total Points Won"},

        };

        #region Properties

        /// <summary>
        /// The string presentation of this statistics for contestant 1
        /// </summary>
        public String Value
        {
            get
            {
                return CalculateValue();
            }
        }

        /// <summary>
        /// The description of this statistic
        /// </summary>
        public String Description
        {
            get
            {
                if (Descriptions.ContainsKey(Statistic)) return Descriptions[Statistic].ToUpper();
                return "Unknown";
            }
        }

        /// <summary>
        /// The statistical element this instance contains and represents
        /// </summary>
        public StatisticElement Statistic { get; set; }

        /// <summary>
        /// The match type
        /// </summary>
        public TennisMatch.MatchType MatchType { get; set; }

        /// <summary>
        /// The source of the statistics
        /// </summary>
        public TotalStatistics Source { get; set; }

        /// <summary>
        /// True if this element is a header
        /// </summary>
        public bool Header
        {
            get
            {
                switch (Statistic)
                {
                    case StatisticElement.ServiceHeader:
                    case StatisticElement.ReturnHeader:
                    case StatisticElement.TotalHeader:

                        return true;

                    default:

                        return false;
                }
            }
        }

        /// <summary>
        /// True if this statistic is visible, and false if it is hidden / collapsed
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Al list of related / children statistics. Used for a tree type hierarchy Header->Statistics
        /// </summary>
        private List<vmTotalStatistic> m_Children;
        public List<vmTotalStatistic> Children
        {
            get
            {
                if (m_Children == null) m_Children = new List<vmTotalStatistic>(0);
                return m_Children;
            }
        }

        #endregion

        #region Methods

        private String CalculateValue()
        {

            string Result = "";

            switch (Statistic)
            {
                case StatisticElement.Aces:

                    Result = String.Format("{0}", Source.GetItemTotal(Statistics.Aces, MatchType));

                    break;

                case StatisticElement.DoubleFaults:

                    Result = String.Format("{0}", Source.GetItemTotal(Statistics.DoubleFaults, MatchType));

                    break;

                case StatisticElement.FirstServePercentage:

                    Result = BuildPercentageNotation(Statistics.FirstServicesPlayed, Statistics.TotalServicePoints);

                    break;

                case StatisticElement.FirstServePointsWon:

                    Result = BuildPercentageNotationLong(Statistics.FirstServicesWon, Statistics.FirstServicesPlayed);

                    break;

                case StatisticElement.SecondServePointsWon:

                    Result = BuildPercentageNotationLong(Statistics.SecondServicesWon, Statistics.SecondServicesPlayed);

                    break;

                case StatisticElement.BreakPointsSaved:

                    Result = BuildPercentageNotationLong(Statistics.ServiceBreakPointsWon, Statistics.ServiceBreakPointsPlayed);

                    break;

                case StatisticElement.ServiceGamesWon:

                    Result = BuildPercentageNotationLong(Statistics.ServiceGamesWon, Statistics.ServiceGamesPlayed);

                    break;

                case StatisticElement.FirstReturnWon:

                    Result = BuildPercentageNotationLong(Statistics.FirstReturnWon, Statistics.FirstReturnPlayed);

                    break;

                case StatisticElement.SecondReturnWon:

                    Result = BuildPercentageNotationLong(Statistics.SecondReturnWon, Statistics.SecondReturnPlayed);

                    break;

                case StatisticElement.BreakPointsWon:

                    Result = BuildPercentageNotationLong(Statistics.ReturnBreakPointsWon, Statistics.ReturnBreakPointsPlayed);

                    break;

                case StatisticElement.ReturnGamesWon:

                    Result = BuildPercentageNotationLong(Statistics.ReturnGamesWon, Statistics.ReturnGamesPlayed);

                    break;

                case StatisticElement.ServicePointWon:

                    Result = BuildPercentageNotationLong(Statistics.TotalServicePointsWon, Statistics.TotalServicePoints);

                    break;

                case StatisticElement.ReturnPointsWon:

                    Result = BuildPercentageNotationLong(Statistics.TotalReturnPointsWon, Statistics.TotalReturnPoints);

                    break;

                case StatisticElement.TotalPointsWon:

                    Result = BuildPercentageNotationLong(Statistics.TotalPointsWon, Statistics.TotalPoints);

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
        }

        private String BuildPercentageNotation(Statistics Dividend, Statistics Diviser)
        {
            int iDividend = Source.GetItemTotal(Dividend, MatchType);
            int iDiviser = Source.GetItemTotal(Diviser, MatchType);

            if (iDiviser == 0) return "0%";

            return String.Format("{0:0%}", (float)iDividend / (float)iDiviser);
        }

        private String BuildPercentageNotationLong(Statistics Dividend, Statistics Diviser)
        {
            int iDividend = Source.GetItemTotal(Dividend, MatchType);
            int iDiviser = Source.GetItemTotal(Diviser, MatchType);
            if (iDiviser == 0) return "0/0 (0%)";

            return String.Format("{0}/{1} ({2:0%})", iDividend, iDiviser, (float)iDividend / (float)iDiviser);
        }

        public void Notify()
        {
            NotifyPropertyChanged("Value");

            foreach (vmTotalStatistic Element in Children)
            {
                Element.Notify();
            }
        }

        public void NotifyVisibility()
        {
            NotifyPropertyChanged("Visible");

            foreach (vmTotalStatistic Element in Children)
            {
                Element.NotifyVisibility();
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
