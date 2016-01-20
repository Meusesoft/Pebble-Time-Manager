using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Tennis_Statistics.Game_Logic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Linq;
using Tennis_Statistics.ViewModels;
using System.Threading.Tasks;
using System.Threading;

namespace Tennis_Statistics.ViewModels
{
    public class vmTotalStatistics : ObservableCollection<vmTotalStatistic>, INotifyPropertyChanged
    {
        public vmTotalStatistics()
        {
            Statistics = new TotalStatistics();
            SinglesVisibleMatchType = true;
        }

        #region Fields

        private TotalStatistics m_Statistics;

        #endregion

        #region Properties

        private TotalStatistics Statistics
        {
            get
            {
                return m_Statistics;        
            }
            set
            {
                m_Statistics = value;
                m_Statistics.UpdatedMatch += OnUpdatedMatch;
            }
        }

        private bool m_SinglesVisibleMatchType;
        public bool SinglesVisibleMatchType
        {
            get
            {
                return m_SinglesVisibleMatchType;
            }
            set
            {
                m_SinglesVisibleMatchType = value;
                m_DoubleVisibleMatchType = !value;

                foreach (vmTotalStatistic element in this)
                {
                    element.Visible = (element.MatchType == TennisMatch.MatchType.Singles);
                    element.NotifyVisibility();
                }

                NotifyPropertyChanged("SinglesVisibleMatchType");
                NotifyPropertyChanged("DoubleVisibleMatchType");
            }
        }

        private bool m_DoubleVisibleMatchType;
        public bool DoubleVisibleMatchType
        {
            get
            {
                return m_DoubleVisibleMatchType;
            }
            set
            {
                m_SinglesVisibleMatchType = !value;
                m_DoubleVisibleMatchType = value;

                foreach (vmTotalStatistic element in this)
                {
                    element.Visible = (element.MatchType == TennisMatch.MatchType.Doubles);
                    element.NotifyVisibility();
                }

                NotifyPropertyChanged("SinglesVisibleMatchType");
                NotifyPropertyChanged("DoubleVisibleMatchType");
            }
        }

        private TennisMatch.MatchType m_VisibleMatchType;
        /// <summary>
        /// The visible match type
        /// </summary>
        public TennisMatch.MatchType VisibleMatchType
        {
            get
            {
                return m_VisibleMatchType;
            }
            set
            {
                m_VisibleMatchType = value;

                foreach (vmTotalStatistic element in this)
                {
                    element.Visible = (element.MatchType == m_VisibleMatchType);
                    element.NotifyVisibility();
                }
            }
        }

        public System.Array PossibleMatchTypes
        {
            get
            {
                return Enum.GetValues(typeof(TennisMatch.MatchType));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add the match to the collection of total statistics
        /// </summary>
        /// <param name="Match"></param>
        public void Add(vmMatch Match)
        {
            Statistics.Add(Match.Match);
        }

        /// <summary>
        /// Remove the match from the collection of total statistics
        /// </summary>
        /// <param name="Match"></param>
        public void Remove(vmMatch Match)
        {
            Statistics.Remove(Match.Match);
        }

        /// <summary>
        /// Remove the match from the collection of total statistics
        /// </summary>
        /// <param name="Match"></param>
        public void Remove(System.Guid ID)
        {
            Statistics.Remove(ID);
        }
        //Process the changes made to the statistics collection
        void OnUpdatedMatch(object sender, System.EventArgs e)
        {
            if (this.Count == 0)
            {
                RebuildStatistics();
            }
            else
            {
                foreach (vmTotalStatistic Item in this)
                {
                    Item.Notify();
                }
            }
        }

        /// <summary>
        /// Rebuild the viewmodel from the source data
        /// </summary>
        public void RebuildStatistics()
        {
            Clear();

            var allValues = (vmTotalStatistic.StatisticElement[])Enum.GetValues(typeof(vmTotalStatistic.StatisticElement));

            vmTotalStatistic LastHeader = null;

            foreach (TennisMatch.MatchType matchType in Enum.GetValues(typeof(TennisMatch.MatchType)))
            {
                foreach (var element in allValues)
                {
                    vmTotalStatistic newElement = new vmTotalStatistic();
                    newElement.Statistic = element;
                    newElement.Source = m_Statistics;
                    newElement.MatchType = matchType;
                    newElement.Visible = true;

                    if (newElement.Header)
                    {
                        LastHeader = newElement;
                        // _StatisticElements.Add(newElement);
                        Add(LastHeader);
                    }
                    else
                    {
                        if (LastHeader != null)
                        {
                            LastHeader.Children.Add(newElement);
                        }
                    }
                }
            }

            foreach (vmTotalStatistic Item in this)
            {
                Item.Notify();
            }

            SinglesVisibleMatchType = SinglesVisibleMatchType;
        }
        

            /// <summary>
        /// (Re)load the previous state of the collection from local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Load()
        {
            try
            {
                String CompressedXML = await Helpers.LocalStorage.Load("totalstatistics.gz");

                TotalStatistics _TotalStatistics = (TotalStatistics)Helpers.Serializer.DecompressAndDeserialize(CompressedXML, typeof(TotalStatistics));

                Statistics = _TotalStatistics;
                RebuildStatistics();


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Save the state of the collection to local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Save()
        {
            try
            {
                String CompressedXML = Helpers.Serializer.SerializeAndCompress(m_Statistics);
                String Filename = "totalstatistics.gz";
                await Helpers.LocalStorage.Save(CompressedXML, Filename, false);

                return true;
            }
            catch (Exception)
            {
                //...
                return false;
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
