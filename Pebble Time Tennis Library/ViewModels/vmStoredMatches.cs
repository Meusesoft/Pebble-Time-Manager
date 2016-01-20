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
    public class vmStoredMatches : ObservableCollection<vmStoredMatch>, INotifyPropertyChanged
    {
        #region Properties

        private bool m_CanLoadMoreStoredMatches;
        /// <summary>
        /// True if more stored matches can be loaded
        /// </summary>
        public bool CanLoadMoreStoredMatches 
        { 
            get
            {
                return m_CanLoadMoreStoredMatches;
            }
            set
            {
                m_CanLoadMoreStoredMatches = value;
                NotifyPropertyChanged("CanLoadMoreStoredMatches");
            }
       }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Match"></param>
        /// <returns></returns>
        public async Task<bool> AddOrUpdate(vmMatch Match)
        {
            vmStoredMatch StoredMatch = new vmStoredMatch(Match.Match);

            //Remove an existing match with the same ID
            await RemoveAndDelete(Match);

            //Add the new or updated match
            this.Insert(0, StoredMatch);

            //Reorder the collection by start time
            this.OrderByDescending(x => x.StartTime);

            //It worked, return true;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Match"></param>
        /// <returns></returns>
        public async Task<bool> Update(vmMatch Match)
        {
            vmStoredMatch StoredMatch = new vmStoredMatch(Match.Match);

            //Remove an existing match with the same ID
            Remove(Match);

            //Add the new or updated match
            this.Insert(0, StoredMatch);

            //Reorder the collection by start time
            this.OrderByDescending(x => x.StartTime);

            //It worked, return true;
            return true;
        }

        /// <summary>
        /// Remove a match from the collection with the same ID as the given match
        /// </summary>
        /// <param name="Match"></param>
        public void Remove(vmMatch Match)
        {
            Remove(Match.Match.ID);
        }

        /// <summary>
        /// Remove a match from the collection with the same ID as the given match
        /// </summary>
        /// <param name="Match"></param>
        public void Remove(Guid MatchID)
        {
            //Check if GUID already exists
            //If so, remove them
            var itemsToRemove = this.Where(i => i.ID == MatchID).ToList();

            foreach (var item in itemsToRemove)
            {
                Remove(item);
            }
        }

        /// <summary>
        /// Remove a match from the collection with the same ID as the given match
        /// </summary>
        /// <param name="Match"></param>
        public async Task RemoveAndDelete(vmMatch Match)
        {
            await RemoveAndDelete(Match.Match.ID);
        }
        
        /// <summary>
        /// Remove a match from the collection with the same ID as the given match
        /// </summary>
        /// <param name="Match"></param>
        public async Task RemoveAndDelete(Guid MatchID)
        {
            //Check if GUID already exists
            //If so, remove them
            var itemsToRemove = this.Where(i => i.ID == MatchID).ToList();

            foreach (var item in itemsToRemove)
            {
                Remove(item);

                String Filename = String.Format("match_{0}.xml", MatchID.ToString());
                await Helpers.LocalStorage.Delete(Filename);
            }
        }
        
        /// <summary>
        /// (Re)load the previous state of the collection from local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Load()
        {
            try
            {
                String CompressedXML = await Helpers.LocalStorage.Load("matches.gz");
                int Tally = 0;

                vmStoredMatches newStoredMatches = (vmStoredMatches)Helpers.Serializer.DecompressAndDeserialize(CompressedXML, typeof(vmStoredMatches));

                foreach(vmStoredMatch element in newStoredMatches)
                {
                    if (this.Where(x => x.ID == element.ID).Count() == 0)
                    {
                        Add(element);
                        Tally++;
                    }
                    if (Tally > 10) break;
                }

                CanLoadMoreStoredMatches = (newStoredMatches.Count > this.Count);

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
                String CompressedXML = Helpers.Serializer.SerializeAndCompress(this);
                String Filename = "matches.gz";
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
