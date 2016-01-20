using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Web.Syndication;
using Windows.Media;
using Windows.UI.Xaml.Media;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using Tennis_Statistics.Helpers;

namespace Tennis_Statistics.ViewModels
{
    public class vmMatches : INotifyPropertyChanged
    {
        public class vmInterestItem
        {
            public string Id {get; set;}
            public string Title {get; set;}
            public string Link {get; set;}
            public string Description {get; set;}
        }


        #region Constructor

        public vmMatches()
        {
            CreateVMNewMatch();
            //vmNewMatch = new vmNewMatch();

            StoredMatches.CollectionChanged += StoredMatches_CollectionChanged;

            _Settings = Settings.GetInstance();
            _Settings.SettingChanged += SettingChanged;
        }

        #endregion

        #region Fields

        private vmStoredMatches m_StoredMatches;

        private RelayCommand m_cmdNewMatch;
        private RelayCommand m_cmdLoadMore;

        private Settings _Settings;

        private Synchronizer _Synchronizer;

        #endregion

        #region Properties

        /// <summary>
        /// The (last) active match 
        /// </summary>
        public vmMatch CurrentMatch { get; set; }

        /// <summary>
        /// View model for creating a new match
        /// </summary>
        public vmNewMatch vmNewMatch { get; set; }

        /// <summary>
        /// The list of stored matches
        /// </summary>
        public vmStoredMatches StoredMatches
        {
            get
            {
                if (m_StoredMatches == null) m_StoredMatches = new vmStoredMatches();

                return m_StoredMatches;
            }
        }

        /// <summary>
        /// The statistics of the local player
        /// </summary>
        private vmTotalStatistics m_TotalStatistics;
        public vmTotalStatistics TotalStatistics
        {
            get
            {
                if (m_TotalStatistics == null) m_TotalStatistics = new vmTotalStatistics();

                return m_TotalStatistics;
            }
        }
        
        /// <summary>
        /// The last match in this collection
        /// </summary>
        private vmStoredMatch m_LastMatch;
        public vmStoredMatch LastMatch
        {
            get
            {
                return m_LastMatch;
            }
            set
            {
                if (m_LastMatch != null) m_LastMatch.IsLastMatch = false;
                m_LastMatch = value;
                if (m_LastMatch != null) m_LastMatch.IsLastMatch = true;
                NotifyPropertyChanged("LastMatch");
            }
        }

        /// <summary>
        /// A list of news items 
        /// </summary>
        private ObservableCollection<vmInterestItem> m_InterestsFeed;
        public ObservableCollection<vmInterestItem> InterestsFeed
        {
            get
            {
                if (m_InterestsFeed == null) m_InterestsFeed = new ObservableCollection<vmInterestItem>();

                return m_InterestsFeed;
            }
        }

        /// <summary>
        /// The current background color or image
        /// </summary>
        public Brush Background
        {
            get
            {
                //Retrieve the setting
                return _Settings.Background();
            }
        }

        /// <summary>
        /// Instance of the synchronizer to synchronize match data between local and external (onedrive) storage
        /// </summary>
        private Synchronizer Synchronizer
        {
            get
            {
                if (_Synchronizer == null)
                {
                    _Synchronizer = new Synchronizer();
                    _Synchronizer.vmMatches = this;
                    _Synchronizer.SynchronizationComplete += _Synchronizer_SynchronizationComplete;
                }

                return _Synchronizer;
            }
        }

        private bool _IsSynchronizing;
        /// <summary>
        /// True if synchronizing is working
        /// </summary>
        public bool IsSynchronizing
        {
            get
            {
                return _IsSynchronizing;
            }
            set
            {
                _IsSynchronizing = value;
                NotifyPropertyChanged("IsSynchronizing");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create the vmNewMatch instance.
        /// </summary>
        public async Task CreateVMNewMatch()
        {
            vmNewMatch = new vmNewMatch();

            try
            {
                //Load the last settings from local storage and deserialize
                String StoredSettingsNewMatch = await Helpers.LocalStorage.Load("newmatchsettings.gz");
                if (StoredSettingsNewMatch.Length > 0)
                {
                    vmNewMatch = (vmNewMatch)Helpers.Serializer.DecompressAndDeserialize(StoredSettingsNewMatch, typeof(vmNewMatch));
                    if (vmNewMatch == null) vmNewMatch = new vmNewMatch();
                }
                else
                {
                    vmNewMatch = new vmNewMatch();
                }

            }
            catch (Exception e)
            {
                //An error occurred, create a brand new one
                vmNewMatch = new vmNewMatch();
            }

            await vmNewMatch.GetPositionAsync();
            vmNewMatch.PreviousPlayers.AddOrUpdateLocalPlayer();
        }

        /// <summary>
        /// Create a new match and start it
        /// </summary>
        public async Task NewMatch()
        {
            //Save the settings to the local storage, do not wait for it (no await)
            try
            {
                String StoreSettingsNewMatch = Helpers.Serializer.SerializeAndCompress(vmNewMatch);
                await Helpers.LocalStorage.Save(StoreSettingsNewMatch, "newmatchsettings.gz", false);
            }
            catch (Exception)
            {

            }

            //Start the new match
            CurrentMatch = new vmMatch();
            CurrentMatch.Start(vmNewMatch);
        }

        /// <summary>
        /// Resume the match identified by the given Guid.
        /// </summary>
        /// <param name="MatchId"></param>
        public async Task ResumeMatch(Guid MatchId)
        {
            try
            {
                vmMatch resumeMatch = new vmMatch();
                await resumeMatch.Load(MatchId);

                CurrentMatch = resumeMatch;
                resumeMatch.CurrentPoint.LogLevel = resumeMatch.Match.LogLevel;

                TotalStatistics.Remove(CurrentMatch);
                
                resumeMatch.NewPoint();
            }
            catch (Exception)
            {
                throw new Exception("An unexpected error occured while resuming match.");
            }
        }

        /// <summary>
        /// Terminate the current match / no winners
        /// </summary>
        public void TerminateMatch()
        {
            if (CurrentMatch != null)
            {
                CurrentMatch.Terminate();
            }
        }

        /// <summary>
        /// Store the current match to the local datastore
        /// </summary>
        public async void StoreMatch()
        {
            if (CurrentMatch != null)
            {
                await StoreMatch(CurrentMatch);

                await CurrentMatch.Save();

                await CurrentMatch.SaveToOneDrive();
            }
        }

        /// <summary>
        /// Store the current match to the local datastore
        /// </summary>
        public async Task StoreMatch(vmMatch _vmMatch)
        {
            //Store the match
            await StoredMatches.Update(_vmMatch);
            
            //Save the stored matches
            await StoredMatches.Save();

            //Update and save total statistics
            TotalStatistics.Add(_vmMatch);
            await TotalStatistics.Save();

            //Fire event
            OnMatchStored(EventArgs.Empty);
        }
        
        // An event that clients can use to be notified whenever a new match is stored
        public event EventHandler MatchStored;

        // Invoke the MatchStored event; calledwhenever a new match is stored
        public virtual void OnMatchStored(EventArgs e)
        {
            if (MatchStored != null) MatchStored(this, e);
        }

        /// <summary>
        /// Remove the current match from the stored matches in the local datastore
        /// </summary>
        public async void RemoveMatch()
        {
            if (CurrentMatch != null)
            {
                RemoveMatch(CurrentMatch.Match.ID);

                CurrentMatch.RemoveFromOneDrive();
            }
        }

        /// <summary>
        /// Remove the current match from the stored matches in the local datastore
        /// </summary>
        public async void RemoveMatch(Guid ID)
        {
            await StoredMatches.RemoveAndDelete(ID);
            StoredMatches.Save();

            TotalStatistics.Remove(ID);
            TotalStatistics.Save();
        
            if (CurrentMatch != null)
            {
                if (CurrentMatch.Match != null)
                {
                    if (CurrentMatch.Match.ID == ID) CurrentMatch = null;
                }
            }

            //Fire event
            OnMatchStored(EventArgs.Empty);
        }

        /// <summary>
        /// Load the stored matches from the local storage
        /// </summary>
        public async void LoadStoredMatches()
        {
            await StoredMatches.Load();
        }

        /// <summary>
        /// Load the stored matches from the local storage
        /// </summary>
        public async void LoadStatistics()
        {
            await TotalStatistics.Load();
        }

        /// <summary>
        /// Load the RSS feed from ATPWorldTour.com
        /// </summary>
        /// <returns></returns>
        public async Task GetTennisNews() 
        { 
            //Retrieve the RSS            
            SyndicationClient client = new SyndicationClient();
            Uri feedUri = new Uri("http://feeds2.feedburner.com/Tennis-AtpWorldTourHeadlineNews"); 
            var feed = await client.RetrieveFeedAsync(feedUri); 
            
            //Process the RSS items            
            InterestsFeed.Clear();
            
            foreach (SyndicationItem item in feed.Items) 
            { 
                string data = string.Empty; 
                if (feed.SourceFormat == SyndicationFormat.Atom10) 
                { 
                    data = item.Content.Text; 
                } 
                else if (feed.SourceFormat == SyndicationFormat.Rss20) 
                { 
                    data = item.Summary.Text; 
                } 
 
                vmInterestItem newItem = new vmInterestItem();

                newItem.Id = item.Id;
                newItem.Title = item.Title.Text;
                newItem.Link = item.Links[0].Uri.ToString();
                newItem.Description = data.Split(new string[] {"<br>"}, StringSplitOptions.None)[0];
 
                InterestsFeed.Add(newItem);

                if (InterestsFeed.Count > 5) return;
            } 
        }

        /// <summary>
        /// Start the synchronization between local and external storage
        /// </summary>
        public void SynchronizeMatchDataNow()
        {
            if (!IsSynchronizing)
            {
                Synchronizer.Execute();
                IsSynchronizing = true;
            }
        }

        /// <summary>
        /// Synchronize the match data; if connect to a microsoft account and the last sync was more than 5 minutes ago
        /// </summary>
        public void SynchronizeMatchData()
        {
            object value = _Settings.Get("ConnectedToMicrosoftAccount");
            if (!(value is bool)) return;

            if ((bool)value)
            {
                TimeSpan _TimeSpan = DateTime.Now - Synchronizer.LastSynchronization;
                if (_TimeSpan.TotalMinutes >= 5)
                {
                    SynchronizeMatchDataNow();
                }
            }
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Handler for collection changes of stored matches. After the collection has change the 'collection' last match will contain the
        /// last match added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StoredMatches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (StoredMatches.Count > 0)
            {
                if (LastMatch != StoredMatches[0])
                {
                    LastMatch = StoredMatches[0];
                }
            }
            else
            {
                LastMatch = null;
            }
        }

        /// <summary>
        /// Handler for the update of a setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Setting"></param>
        private void SettingChanged(object sender, string Setting)
        {
            if (Setting == "Background") NotifyPropertyChanged("Background");
            if (Setting == "BackgroundImage") NotifyPropertyChanged("Background");
            if (Setting == "UserID" || Setting == "ScreenName" || Setting == "ConnectedToMicrosoftAccount")
            {
                vmNewMatch.PreviousPlayers.AddOrUpdateLocalPlayer();
            }
            if (Setting == "ScreenName")
            {
                object ScreenName = _Settings.Get("ScreenName");
                if (ScreenName.GetType() == typeof(String))
                {
                    if (vmNewMatch.Player1.Name != ScreenName && vmNewMatch.Player2.Name != ScreenName)
                    {
                        vmNewMatch.Player1.Name = (String)ScreenName;
                    }
                }
            }
        }

        /// <summary>
        /// Handler for the completion of the synchronization between local and external storage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _Synchronizer_SynchronizationComplete(object sender, EventArgs e)
        {
            IsSynchronizing = false;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Relay command for starting a new match
        /// </summary>
        public RelayCommand cmdNewMatch
        {
            get
            {
                if (m_cmdNewMatch == null)
                    m_cmdNewMatch = new RelayCommand(param => NewMatch());

                return m_cmdNewMatch;
            }
        }

        /// <summary>
        /// Relay command for loading more stored matches
        /// </summary>
        public RelayCommand cmdLoadMore
        {
            get
            {
                if (m_cmdLoadMore == null)
                    m_cmdLoadMore = new RelayCommand(param => LoadStoredMatches());

                return m_cmdLoadMore;
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
