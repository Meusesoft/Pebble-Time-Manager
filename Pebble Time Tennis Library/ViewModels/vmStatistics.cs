using System;
using System.Collections.Generic;
using System.Text;
using Tennis_Statistics.Game_Logic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Tennis_Statistics.ViewModels
{
    [DataContract]
    public class vmStatistics : INotifyPropertyChanged
    {
        #region Constructor

        public vmStatistics()
        {
            _StatisticElements = new List<vmStatistic>(0);
            Children = new ObservableCollection<vmStatistic>();            
        }

        public vmStatistics(String _Description, TennisStatistics _Source, vmStatisticsCollection _Owner)
        {
            _StatisticElements = new List<vmStatistic>(0);
            Children = new ObservableCollection<vmStatistic>();
            Description = _Description;
            Owner = _Owner;
            SetSource(_Source);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Internal list of all statistical elements
        /// </summary>
        private List<vmStatistic> _StatisticElements;
        private string m_Description;
        private bool m_IsSelected;
        private ObservableCollection<vmStatistic> m_Children;
        private string m_Duration;
        private TennisStatistics _Source;


        #endregion

        #region Properties

        [DataMember]
        public string Description
        {
            get
            {
                return m_Description;
            }
            set
            {
                m_Description = value.ToUpper();
            }
        }

        [DataMember]
        public bool IsSelected {
            get
            {
                return m_IsSelected;
            }
            set
            {
                if (m_IsSelected != value)
                {
                    m_IsSelected = value;
                    if (m_IsSelected) OnSelected(EventArgs.Empty);
                    NotifyPropertyChanged("IsSelected");
                }
            }        
        }

        /// <summary>
        /// Al list of related / children statistics. Used for a tree type hierarchy Header->Statistics
        /// </summary>
        [DataMember]
        public ObservableCollection<vmStatistic> Children
        {
            get
            {
                if (m_Children == null) m_Children = new ObservableCollection<vmStatistic>();
                return m_Children;
            }
            set
            {
                m_Children = value;
            }
        }

        [DataMember]
        public string Duration
        {
            get
            {
                if (_Source != null)
                {
                    int Minutes = _Source.GetItem(Statistics.TotalDuration, 0);

                    String Result = String.Format("{0}:{1:D2}", (int)(Minutes / 60), (int)(Minutes % 60));
                    
                    return Result;
                }
                else
                {
                    return m_Duration;
                }
                
                return "_:__";
            }
            set
            {
                m_Duration = value;
            }
        }

        public vmStatisticsCollection Owner { get; set; }

        #endregion

        #region Events

        public event SelectedEventHandler Selected;

        // Invoke the Selected event; called whenever list changes
        protected virtual void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        #endregion

        #region Methods

        public void SetSource(TennisStatistics Source)
        {
            _Source = Source;

            Children.Clear();

            var allValues = (vmStatistic.StatisticElement[])Enum.GetValues(typeof(vmStatistic.StatisticElement));

            vmStatistic LastHeader = null;

            TennisMatch.LogLevelEnum _LogLevel = Source.currentMatch.LogLevel;

            foreach (var element in allValues)
            {
                if (VisibleLogLevel(_LogLevel, element))
                {
                    vmStatistic newElement = new vmStatistic();
                    newElement.Statistic = element;
                    newElement.Source = Source;
                    newElement.Visible = true;
                    // _StatisticElements.Add(newElement);

                    if (newElement.Header)
                    {
                        LastHeader = newElement;
                        _StatisticElements.Add(newElement);
                        Children.Add(LastHeader);
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
        }

        /// <summary>
        /// Return true if element is visible in the current LogLevel
        /// </summary>
        /// <param name="Element"></param>
        /// <returns></returns>
        private bool VisibleLogLevel(TennisMatch.LogLevelEnum LogLevel, vmStatistic.StatisticElement Element)
        {
            switch (LogLevel)
            {
                case TennisMatch.LogLevelEnum.Placement:
                case TennisMatch.LogLevelEnum.Shots:

                    return true;

                case TennisMatch.LogLevelEnum.Errors:

                    switch (Element)
                    {
                        case vmStatistic.StatisticElement.ShotsHeader:
                        case vmStatistic.StatisticElement.ForehandWinner:
                        case vmStatistic.StatisticElement.ForehandForcedError:
                        case vmStatistic.StatisticElement.ForehandUnforcedError:
                        case vmStatistic.StatisticElement.BackhandWinner:
                        case vmStatistic.StatisticElement.BackhandForcedError:
                        case vmStatistic.StatisticElement.BackhandUnforcedError:
                        case vmStatistic.StatisticElement.VolleyWinner:
                        case vmStatistic.StatisticElement.VolleyForcedError:
                        case vmStatistic.StatisticElement.VolleyUnforcedError:

                            return false;                        
                        
                        default:

                            return true;
                    }

                case TennisMatch.LogLevelEnum.Points:

                    switch (Element)
                    {
                        case vmStatistic.StatisticElement.ShotsHeader:
                        case vmStatistic.StatisticElement.ForehandWinner:
                        case vmStatistic.StatisticElement.ForehandForcedError:
                        case vmStatistic.StatisticElement.ForehandUnforcedError:
                        case vmStatistic.StatisticElement.BackhandWinner:
                        case vmStatistic.StatisticElement.BackhandForcedError:
                        case vmStatistic.StatisticElement.BackhandUnforcedError:
                        case vmStatistic.StatisticElement.VolleyWinner:
                        case vmStatistic.StatisticElement.VolleyForcedError:
                        case vmStatistic.StatisticElement.VolleyUnforcedError:
                        case vmStatistic.StatisticElement.TotalWinners:
                        case vmStatistic.StatisticElement.TotalUnforcedErrors:
                        case vmStatistic.StatisticElement.TotalForcedErrors:

                            return false;                        
                        
                        default:

                            return true;
                    }

                  //  return true;
            }

            return false;
        }

         /// <summary>
        /// Send propertychanged notifications for all elements
        /// </summary>
        public void Notify()
        {
            foreach (vmStatistic element in this.Children)
            {
                element.Notify();
            }
            NotifyPropertyChanged("Duration");
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

    public class StatisticsItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate HeaderTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            vmStatistic element = (vmStatistic)item;

            if (element.Header) return HeaderTemplate;

            return ItemTemplate;
        }
    }
}
