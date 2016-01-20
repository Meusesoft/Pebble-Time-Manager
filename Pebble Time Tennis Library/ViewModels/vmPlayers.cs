using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Tennis_Statistics.Helpers;

namespace Tennis_Statistics.ViewModels
{
    [DataContract]
    public class vmPlayers
    {

        #region Properties

        private List<vmPlayer> m_Players;
        /// <summary>
        /// The list containing all players
        /// </summary>
        [DataMember]
        public List<vmPlayer> Players
        {
            get
            {
                if (m_Players == null) m_Players = new List<vmPlayer>(0);
                return m_Players;
            }
            set
            {
                m_Players = value;
            }
        }

        private ObservableCollection<vmPlayer> m_FilteredPlayers;
        /// <summary>
        /// A collection of players with match the filter
        /// </summary>
        public ObservableCollection<vmPlayer> FilteredPlayers
        {
            get
            {
                if (m_FilteredPlayers == null) m_FilteredPlayers = new ObservableCollection<vmPlayer>();
                return m_FilteredPlayers;
            }
        }

        private String m_Filter;
        /// <summary>
        /// The filter used on the collection of players
        /// </summary>
        public String Filter
        {
            get
            {
                return m_Filter;
            }
            set
            {
                m_Filter = value;
                FilterCollection();
            }
        }
        
        #endregion

        #region Methods

        public void AddOrUpdateLocalPlayer()
        {
            Settings _Settings = Settings.GetInstance();

            vmPlayer LocalPlayer = new vmPlayer();

            object value = _Settings.Get("UserID");

            if (!String.IsNullOrEmpty((string)value))
            {
                if (value is string) LocalPlayer.ID = (string)value;

                value = _Settings.Get("ScreenName");
                if ((value is string)) LocalPlayer.Name = (string)value;

                LocalPlayer.LocalPlayer = true;

                var LocalPlayers = Players.Where(p => p.ID == LocalPlayer.ID).ToList();

                //Add a new local player
                if (LocalPlayers.Count() == 0)
                {
                    AddPlayer(LocalPlayer);
                }

                //Update the existing player
                if (LocalPlayers.Count() > 0)
                {
                    LocalPlayers.First().Name = LocalPlayer.Name;
                    LocalPlayers.First().LocalPlayer = true;
                }
            }

            //Set other players to false if they have a localplayer-flag set
            var NonLocalPlayers = Players.Where(p => p.ID != LocalPlayer.ID && p.LocalPlayer == true).ToList();

            foreach (vmPlayer Player in NonLocalPlayers)
            {
                Player.LocalPlayer = false;
            }            
        }

        public void AddPlayer(vmPlayer _Player)
        {
            AddPlayer(_Player.Name, _Player.ID, _Player.ProfileImage);
        }            
            
        public void AddPlayer(String Name, String ID, String ProfileImage)
        {
            var MatchingPlayers = Players.Where(p => p.Name == Name);

            if (MatchingPlayers.Count() == 0)
            {

                vmPlayer newPlayer = new vmPlayer();
                newPlayer.ID = ID;
                newPlayer.Name = Name;
                newPlayer.ProfileImage = ProfileImage;
                newPlayer.LastMatch = DateTime.Now;

                Players.Add(newPlayer);
            }
            else
            {
                foreach (vmPlayer MatchingPlayer in MatchingPlayers)
                {
                    MatchingPlayer.LastMatch = DateTime.Now;
                }
            }

            FilterCollection();
        }        

        /// <summary>
        /// Return the player who's name is the same as the given vmPlayer instance
        /// </summary>
        /// <param name="_value"></param>
        /// <returns></returns>
        public vmPlayer MapPlayer(vmPlayer _value)
        {
            var Candidates = Players.Where(p => p.Name == _value.Name).ToList();
            if (Candidates.Count == 1) return Candidates.First();
            return _value;
        }
        
        /// <summary>
        /// Filter the collection to match the filter and populate the observablecollection
        /// </summary>
        private void FilterCollection()
        {
            FilteredPlayers.Clear();
            if (string.IsNullOrEmpty(Filter)) return;

            var _filteredPlayers = Players.Where(p => p.Name.StartsWith(Filter) && p.Name != Filter).OrderByDescending(p => p.LastMatch);
            if (_filteredPlayers.Count() == 0) return;

            foreach (vmPlayer Player in _filteredPlayers)
            {
              FilteredPlayers.Add(Player);
            }
        }

        #endregion
    }
}
