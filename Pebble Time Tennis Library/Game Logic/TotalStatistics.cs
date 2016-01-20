using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    /// <summary>
    /// A statistical element of the user from all its matches
    /// </summary>
    [DataContract]
    public class TotalStatistic 
    {
        public TotalStatistic()
        { }

        [DataMember]
        public Statistics Item { get; set; }

        [DataMember]
        public String MatchID { get; set; }

        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public TennisMatch.MatchType MatchType { get; set; }
    }

    // A delegate type for hooking up change notifications.
    public delegate void UpdatedStatisticsEventHandler(object sender, EventArgs e);

    /// <summary>
    /// The collection of statistical element of the user from all its matches
    /// </summary>
    [DataContract]
    public class TotalStatistics
    {
        public TotalStatistics()
        {
            Items = new List<TotalStatistic>();
        }

        #region Properties

        [DataMember]
        public List<TotalStatistic> Items { get; set; }

        [DataMember]
        public String Player { get; set; }

        #endregion

        #region Events


        // Events that clients can use to be notified
        public event UpdatedStatisticsEventHandler UpdatedMatch;
        public event UpdatedStatisticsEventHandler UpdatedItem;

        // Invoke the changed match event
        protected virtual void OnUpdatedMatch(EventArgs e)
        {
            if (UpdatedMatch != null)
                UpdatedMatch(this, e);
        }

        //Invoke the changed item event.
        protected virtual void OnUpdatedItem(EventArgs e)
        {
            if (UpdatedItem != null)
                UpdatedItem(this, e);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Adds all statistics of the match to this collection
        /// </summary>
        /// <param name="match"></param>
        public void Add(TennisMatch match)
        {
            //get the contestant ID
            int ContestantID = GetContestantID(match);

            //clear the collection, in case the match was resumed
            Items.RemoveAll(x => x.MatchID == match.ID.ToString());

            //add the new values
            if (ContestantID != -1)
            {
                foreach (TennisStatistic Statistic in match.Statistics.Items)
                {
                    if (Statistic.Contestant == ContestantID) SetItem(Statistic.Item, Statistic.Value, match.ID.ToString(), match.Type);
                }
            }

            OnUpdatedMatch(EventArgs.Empty);
        }

        /// <summary>
        /// Adds all statistics of the match to this collection
        /// </summary>
        /// <param name="match"></param>
        public void Remove(TennisMatch match)
        {
            Remove(match.ID);
        }

        /// <summary>
        /// Adds all statistics of the match to this collection
        /// </summary>
        /// <param name="match"></param>
        public void Remove(System.Guid Guid)
        {
            Items.RemoveAll(x => x.MatchID == Guid.ToString());

            OnUpdatedMatch(EventArgs.Empty);
        }
        
        /// <summary>
        /// Return the contestant number of the player of this collection
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private int GetContestantID(TennisMatch match)
        {
            foreach (TennisPlayer Player in match.Contestant1.Players)
            {
                if (Player.ID == this.Player) return 1;
            }

            foreach (TennisPlayer Player in match.Contestant2.Players)
            {
                if (Player.ID == this.Player) return 2;
            }

            return -1;
        }

        /// <summary>
        /// Get the requested statistic from the collection
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public int GetItemTotal(Statistics Item, TennisMatch.MatchType MatchType)
        {
            int Result;

            Result = 0;

            foreach (TotalStatistic item in Items)
            {

                if (item.Item == Item && item.MatchType == MatchType)
                {
                    Result += item.Value;
                }
            }

            return Result;
        }
        
        /// <summary>
        /// Get the requested statistic from the collection
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public int GetItem(Statistics Item, String MatchID, TennisMatch.MatchType MatchType)
        {
            int Result;

            Result = 0;

            foreach (TotalStatistic item in Items)
            {

                if (item.Item == Item)
                {
                    Result += item.Value;
                }
            }

            return Result;
        }

        /// <summary>
        /// Increments the specified statistic with 1
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Contestant"></param>
        public void IncrementItem(Statistics Item, int Value, String MatchID, TennisMatch.MatchType MatchType)
        {
            ProcessIncrementItem(Item, Value, MatchID, MatchType);
        }

        /// <summary>
        /// Decrement the specified statistic with 1
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Contestant"></param>
        public void DecrementItem(Statistics Item, int Value, String MatchID, TennisMatch.MatchType MatchType)
        {
            ProcessIncrementItem(Item, -Value, MatchID, MatchType);
        }

        /// <summary>
        /// Increments the specified statitic for the contestant with the specified Delta value;
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Contestant"></param>
        /// <param name="Delta"></param>
        private void ProcessIncrementItem(Statistics Item, int Delta, String MatchID, TennisMatch.MatchType MatchType)
        {
            Boolean Found;

            Found = false;

            foreach (TotalStatistic item in Items)
            {

                if (item.Item == Item && item.MatchID == MatchID && item.MatchType == MatchType)
                {
                    item.Value += Delta;
                    Found = true;
                }
            }

            if (!Found)
            {
                TotalStatistic newItem = new TotalStatistic();
                newItem.Item = Item;
                newItem.Value = Delta;
                newItem.MatchID = MatchID;
                newItem.MatchType = MatchType;

                Items.Add(newItem);
            }

            OnUpdatedItem(EventArgs.Empty);
        }

        /// <summary>
        /// Increments the specified statitic for the contestant with the specified Delta value;
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Contestant"></param>
        /// <param name="Delta"></param>
        private void SetItem(Statistics Item, int Value, String MatchID, TennisMatch.MatchType MatchType)
        {
            Boolean Found;

            Found = false;

            foreach (TotalStatistic item in Items)
            {

                if (item.Item == Item && item.MatchID == MatchID)
                {
                    item.Value = Value;
                    Found = true;
                }
            }

            if (!Found)
            {
                TotalStatistic newItem = new TotalStatistic();
                newItem.Item = Item;
                newItem.Value = Value;
                newItem.MatchID = MatchID;
                newItem.MatchType = MatchType;

                Items.Add(newItem);
            }

            OnUpdatedItem(EventArgs.Empty);
        }

        #endregion
    }
}
