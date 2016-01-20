using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Tennis_Statistics.Game_Logic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Tennis_Statistics.ViewModels
{
    // A delegate type for hooking up selected change notifications.
    public delegate void SelectedEventHandler(object sender, EventArgs e);

    [DataContract]
    public class vmStatisticsCollection : INotifyPropertyChanged
    {
        #region Constructors

        public vmStatisticsCollection()
        {
            Children = new ObservableCollection<vmStatistics>();
        }

        #endregion

        #region Fields

        private ObservableCollection<vmStatistics> m_Children;
        private vmGroup m_SelectedGroup;

        #endregion

        #region Properties

        private vmStatistics m_Selected;
        public vmStatistics Selected {
            get
            {
                if (m_Selected == null) Selected = this.Children[0];
                return m_Selected;
            } 
            set
            {
                if (m_Selected != value)
                {
                    if (m_Selected != null) m_Selected.IsSelected = false;
                    m_Selected = value;
                    m_Selected.IsSelected = true;
                    NotifyPropertyChanged("Selected");
                    NotifyPropertyChanged("VisibleElements");
                }
            }
        }

        [DataContract]
        public class vmGroup : INotifyPropertyChanged
        {
            [DataMember]
            public vmStatistic.StatisticElement Type { get; set; }
            private string m_Description;

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

            private bool m_IsSelected;
            [DataMember]
            public bool IsSelected
            {
                get
                {
                    return m_IsSelected;
                }
                set
                {
                    if (m_IsSelected != value)
                    {
                        m_IsSelected = value;
                        if (m_IsSelected) OnSelected(EventArgs.Empty); //Fire event when selected
                        NotifyPropertyChanged("IsSelected");
                    }
                }
            }

            public bool m_IsAvailable;
            public bool m_IsAvailableChecked;
            /// <summary>
            /// True if this item is available within the users license
            /// </summary>
            public bool IsAvailable
            {
                get
                {
                    if (!m_IsAvailableChecked)
                    {
                        m_IsAvailableChecked = true;
                        m_IsAvailable = Helpers.Purchases.Available(this.Description);
                    }

                    return m_IsAvailable;
                }
            }

            #region Events

            public event SelectedEventHandler Selected;

            // Invoke the Selected event; called whenever list changes
            protected virtual void OnSelected(EventArgs e) 
            {
                if (Selected != null)
                    Selected(this, e);
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

        public vmGroup SelectedGroup
        {
            get
            {
                return m_SelectedGroup;
            }
            set
            {
                if (m_SelectedGroup != null) m_SelectedGroup.IsSelected = false;
                m_SelectedGroup = value;
                m_SelectedGroup.IsSelected = true;
                NotifyPropertyChanged("SelectedGroup");
                NotifyPropertyChanged("VisibleElements");
            }
        }

        [DataMember]
        public List<vmGroup> Groups
        {
            get
            {
                List<vmGroup> _Result = new List<vmGroup>(1);

                foreach(vmStatistic Element in Selected.Children)
                {
                    if (Element.Header)
                    {
                        vmGroup newGroup = new vmGroup();
                        newGroup.IsSelected = false;
                        newGroup.Description = Element.Description;
                        newGroup.Type = Element.Statistic;
                        newGroup.Selected += newGroup_Selected;

                        if (_Result.Count == 0)
                        {
                            newGroup.IsSelected = true;
                        }

                        _Result.Add(newGroup);
                    }
                }

                return _Result;
            }
        }

        void newGroup_Selected(object sender, EventArgs e)
        {
            SelectedGroup = (vmGroup)sender;
        }

        public List<vmStatistic> VisibleElements
        {
            get
            {
                if (Selected!=null)
                {
                    foreach(vmStatistic Element in Selected.Children)
                    {
                        if (Element.Statistic == SelectedGroup.Type) return Element.Children;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Al list of related / children statistics. Used for a tree type hierarchy Header->Statistics
        /// </summary>
        [DataMember]
        public ObservableCollection<vmStatistics> Children
        {
            get
            {
                if (m_Children == null)
                {
                    m_Children = new ObservableCollection<vmStatistics>();
                    m_Children.CollectionChanged += M_Children_CollectionChanged;
                }

                return m_Children;
            }
            set
            {
                m_Children = value;
                m_Children.CollectionChanged += M_Children_CollectionChanged;
            }
        }

        private void M_Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    vmStatistics _newItem = (vmStatistics)item;
                    _newItem.Selected += _newItem_Selected;
                }
            }
        }

        private void _newItem_Selected(object sender, EventArgs e)
        {
            Selected = (vmStatistics)sender;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            foreach (var item in Children)
            {
                item.Selected += Item_Selected;
            }
        }

        private void Item_Selected(object sender, EventArgs e)
        {
            Selected = (vmStatistics)sender;
        }

        /// <summary>
        /// Update the statistics
        /// </summary>
        /// <param name="_vmSC"></param>
        public void Update(vmStatisticsCollection _vmSC)
        {
            Children.Clear();

            foreach (var Child in _vmSC.Children)
            {
                Children.Add(Child);

                if (Child.Description == Selected.Description) Selected = Child;
            }

            Notify();
            NotifyPropertyChanged("VisibleElements");
        }

        /// <summary>
        /// Set the match source of this collection
        /// </summary>
        /// <param name="Match"></param>
        public void SetSource(TennisMatch Match)
        {
            Children.Clear();

            if (Match != null)
            {
                vmStatistics newStatistics = new vmStatistics("Match", Match.Statistics, this);
                newStatistics.Selected += newStatistics_Selected;

                Children.Add(newStatistics);

                Match.NewSet += Match_NewSet;

                int i = 1;

                foreach (TennisSet Set in Match.Sets)
                {
                    newStatistics = new vmStatistics(String.Format("Set {0}", i), Set.Statistics, this);
                    newStatistics.Selected += newStatistics_Selected;

                    Children.Add(newStatistics);

                    i++;
                }

                Selected = this.Children[0];
            }
        }

        void newStatistics_Selected(object sender, EventArgs e)
        {
            Selected = (vmStatistics)sender;
        }

        /// <summary>
        /// Add a new set to the collection (event handler)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="_newSet"></param>
        void Match_NewSet(object sender, TennisSet _newSet)
        {
            vmStatistics newStatistics = new vmStatistics(String.Format("Set {0}", this.Children.Count), _newSet.Statistics, this);
            newStatistics.Selected += newStatistics_Selected;

            Children.Add(newStatistics);
        }

        /// <summary>
        /// Send property change notifications
        /// </summary>
        public void Notify()
        {
            if (Selected!=null)
            {
                Selected.Notify();
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

    public class StatisticsSelectedItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate SelectedItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            bool bItem = (bool)item;

            return bItem ? SelectedItemTemplate : ItemTemplate;
        }
    }

}
