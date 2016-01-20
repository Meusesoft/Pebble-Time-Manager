using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.ComponentModel;
using Tennis_Statistics.Game_Logic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Windows.Devices.Geolocation;


namespace Tennis_Statistics.ViewModels
{
    [DataContract]
    public class vmNewMatch : INotifyPropertyChanged
    {
        #region Classes

            [DataContract]
            public class vmLocation : INotifyPropertyChanged
            {
                [DataMember]
                public String Name {get; set;}
                
                [DataMember]
                public Double Latitude { get; set; }
                
                [DataMember]
                public Double Longitude { get; set; }

                public Double Distance { get; set; }

                [DataMember]
                public String NameAndLocation { get; set; }
            
                public void SetNameAndLocation(Geoposition GeoPosition)
                {
                    Distance = vmNewMatch.CalculateDistance(GeoPosition.Coordinate.Point.Position.Latitude, 
                                                        GeoPosition.Coordinate.Point.Position.Longitude, 
                                                        Latitude, 
                                                        Longitude) / 1000;
                    NameAndLocation = String.Format("{0} ({1:N1} km)", Name, Distance );
                    NotifyPropertyChanged("NameAndLocation");
                }

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

        #endregion

        #region Constructor

        public vmNewMatch()
        {
            Player1 = new vmPlayer();
            Player1.Name = "Player 1";
            Player2 = new vmPlayer();
            Player2.Name = "Player 2";
            Player1Partner = new vmPlayer();
            Player1Partner.Name = "Partner";
            Player2Partner = new vmPlayer();
            Player2Partner.Name = "Partner";
            BestOutOf = 1;
            GamesPerSet = 6;
            LogLevel = 0;
            Type = 0;
            Surface = 0;
            TiebreakFinalSet = true;
            ShowAdvanced = false;
            Locations = new ObservableCollection<vmLocation>();

            // TimedOut on Windows Store App
            //GetPositionAsync();

            CreateMatchVariants();
        }

        #endregion

        #region Fields

        private Geolocator _geolocator = null;
        //private String m_Player1;
        //private String m_Player1Partner;
        //private String m_Player2;
        //private String m_Player2Partner;
        private int m_LogLevel;
        private int m_Type;
        private int m_BestOutOf;
        private int m_GamesPerSet;
        private int m_Surface;
        private Geoposition m_GeoPosition;
        private String m_Location;
        private vmLocation m_SelectedLocation;
        private ObservableCollection<vmLocation> m_Locations;
        private ObservableCollection<vmLocation> m_LocationsNearby;
        private TennisMatchVariant m_TennisMatchVariant;

        private const string _ConstUnknownLocation = "unknown location";


        #endregion

        #region Properties

        private ObservableCollection<String> m_Players;
        public ObservableCollection<String> Players
        {
            get
            {
                if (m_Players == null) m_Players = new ObservableCollection<string>();
                return m_Players;
            }
        }

        /*
        /// <summary>
        /// The name of player number 1
        /// </summary>
        [DataMember]
        public String Player1
        { 
            get
            {
                return m_Player1;
            }
            set
            {
                //if (value.GetType() == typeof(vmPlayer)) m_Player1 = ((vmPlayer)value).Name;
                m_Player1 = value;
                PreviousPlayers.Filter = value;
                NotifyPropertyChanged("Player1");
                NotifyPropertyChanged("Player1Description");
            }
        }*/

        private vmPlayer m_Player1;
        /// <summary>
        /// Player 1 of the tennis match
        /// </summary>
        [DataMember]
        public vmPlayer Player1
        {
            get
            {
                if (m_Player1 == null)
                {
                    m_Player1 = new vmPlayer();
                    m_Player1.Name = "Unknown";
                }
                return m_Player1;
            }
            set
            {
                m_Player1 = PreviousPlayers.MapPlayer(value);
                PreviousPlayers.Filter = m_Player1.Name;
                NotifyPropertyChanged("Player1");
                NotifyPropertyChanged("Player1Description");
            }
        }

        private vmPlayer m_Player1Partner;
        /// <summary>
        /// The name of the partner of player number 2
        /// </summary>
        [DataMember]
        public vmPlayer Player1Partner
        {
            get
            {
                if (m_Player1Partner == null)
                {
                    m_Player1Partner = new vmPlayer();
                    m_Player1Partner.Name = "Unknown";
                }
                return m_Player1Partner;
            }
            set
            {
                m_Player1Partner = PreviousPlayers.MapPlayer(value);
                PreviousPlayers.Filter = m_Player1Partner.Name;
                NotifyPropertyChanged("Player1Partner");
                NotifyPropertyChanged("Player1Description");
            }
        }

        private vmPlayer m_Player2;
        /// <summary>
        /// Player 1 of the tennis match
        /// </summary>
        [DataMember]
        public vmPlayer Player2
        {
            get
            {
                if (m_Player2 == null)
                {
                    m_Player2 = new vmPlayer();
                    m_Player2.Name = "Unknown";
                }
                return m_Player2;
            }
            set
            {
                m_Player2 = PreviousPlayers.MapPlayer(value);
                PreviousPlayers.Filter = m_Player2.Name;
                NotifyPropertyChanged("Player2");
                NotifyPropertyChanged("Player2Description");
            }
        }

        private vmPlayer m_Player2Partner;
        /// <summary>
        /// The name of the partner of player number 2
        /// </summary>
        [DataMember]
        public vmPlayer Player2Partner
        {
            get
            {
                if (m_Player2Partner == null)
                {
                    m_Player2Partner = new vmPlayer();
                    m_Player2Partner.Name = "Unknown";
                }
                return m_Player2Partner;
            }
            set
            {
                m_Player2Partner = PreviousPlayers.MapPlayer(value);
                PreviousPlayers.Filter = m_Player2Partner.Name;                

                NotifyPropertyChanged("Player2Partner");
                NotifyPropertyChanged("Player2Description");
            }
        }

        /// <summary>
        /// Presentable name for constestant 1
        /// </summary>
        public String Player1Description
        {
            get
            {
                String Description = Player1.Name;
                if (Type == 1)
                {
                    Description += " / ";
                    Description += Player1Partner.Name;
                }

                return Description;
            }
        }

        /// <summary>
        /// Presentable name for constestant 2
        /// </summary>
        public String Player2Description
        {
            get
            {
                String Description = Player2.Name;
                if (Type == 1)
                {
                    Description += " / ";
                    Description += Player2Partner.Name;
                }

                return Description;
            }
        }

        /// <summary>
        /// The level of logging requested
        /// </summary>
        [DataMember]
        public int LogLevel
        {
            get
            {
                return m_LogLevel;
            }
            set
            {
                m_LogLevel = value;
                NotifyPropertyChanged("LogLevel");
                NotifyPropertyChanged("LogLevelDescription");
            }
        }

        /// <summary>
        /// String representation of the loglevel
        /// </summary>
        public String LogLevelDescription
        {
            get
            {
                switch (LogLevel)
                {
                    case 0: return "Points";
                    case 1: return "Errors";
                    case 2: return "Shots";
                    case 3: return "Placement";
                }

                return "";
            }
        }
        
        /// <summary>
        /// The type of game; singles or doubles
        /// </summary>
        [DataMember]
        public int Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
                NotifyPropertyChanged("Type");
                NotifyPropertyChanged("Doubles");
                NotifyPropertyChanged("MatchDescription");
                NotifyPropertyChanged("Player1Description");
                NotifyPropertyChanged("Player2Description");
            }
        }

        /// <summary>
        /// True, if match type is Doubles
        /// </summary>
        public Boolean Doubles
        {
            get
            {
                return m_Type == 1;
            }
        }

        /// <summary>
        /// The number of sets to play, 0, 1 (= best of 3), 2 (= best of 5)
        /// </summary>
        [DataMember]
        public int BestOutOf
        {
            get
            {
                return m_BestOutOf;
            }
            set
            {
                m_BestOutOf = value;
                NotifyPropertyChanged("BestOutOf");
                NotifyPropertyChanged("MatchDescription");
            }
        }

        /// <summary>
        /// String representation of the properties LogLevel and Type
        /// </summary>
        public string MatchDescription
        {
            get
            {
                String Result;

                Result = String.Format("{0} | Best of {1} | {2}", Type == 0 ? "Singles" : "Doubles", ((BestOutOf * 2) + 1).ToString(), MatchVariants[SelectedMatchIndex].Description);


                /*if (GamesPerSet != 6)
                {
                    Result += String.Format(" | {0} Games per set", GamesPerSet);
                }*/

                return Result;
            }
        }

        /// <summary>
        /// The current position of the device
        /// </summary>
        public Geoposition GeoPosition
        {
            get
            {
                if (m_GeoPosition == null)
                {
                    return null;
                }
                return m_GeoPosition;
            }
            set
            {
                m_GeoPosition = value;

                UpdateNearbyLocations();

                NotifyPropertyChanged("GeoPosition");
                NotifyPropertyChanged("Location");
            }
        }

        /// <summary>
        /// The current location of the device
        /// </summary>
        [DataMember]
        public String Location
        {
            get
            {
                if (m_Location == null)
                {
                    return _ConstUnknownLocation;
                }

                return m_Location;
                }
            set
            {
                m_Location = value;
                NotifyPropertyChanged("Location");
            }
        }

        /// <summary>
        /// The currently selected location
        /// </summary>
        public vmLocation SelectedLocation
        {
            get
            {
                return m_SelectedLocation;        
            }
            set
            {
                m_SelectedLocation = value;
                NotifyPropertyChanged("SelectedLocation");

                if (m_SelectedLocation != null)
                {
                    Location = m_SelectedLocation.Name;
                }
            }
        }

        /// <summary>
        /// The list of known locations
        /// </summary>
        [DataMember]
        public ObservableCollection<vmLocation> Locations
        {
            get
            {
                if (m_Locations == null) m_Locations = new ObservableCollection<vmLocation>();
                return m_Locations;
            }
            set
            {
                m_Locations = value;
            }
        }

        /// <summary>
        /// The list of locations nearby, it is refreshed when the geoposition is refreshed
        /// </summary>
        public ObservableCollection<vmLocation> LocationsNearby
        {
            get
            {
                if (m_LocationsNearby == null) m_LocationsNearby = new ObservableCollection<vmLocation>();
                return m_LocationsNearby;
            }
            set
            {
                m_LocationsNearby = value;
            }
        }
        
        /// <summary>
        /// The number of games per set (0-6, 0 = only a tiebreak)
        /// </summary>
        [DataMember]
        public int GamesPerSet 
        {             
            get
            {
                return m_GamesPerSet;
            }
            set
            {
                m_GamesPerSet = value;
                NotifyPropertyChanged("GamesPerSet");
                NotifyPropertyChanged("MatchDescription");
            }
        }
        
        /// <summary>
        /// If true, a tiebreak is played in the final set
        /// </summary>
        [DataMember]
        public Boolean TiebreakFinalSet { get; set; }

        /// <summary>
        /// List of available match variants 
        /// </summary>
        [DataMember]
        public List<TennisMatchVariant> MatchVariants {get; set; }


        /// <summary>
        /// The selected match variant (a set of rules)
        /// </summary>
        [DataMember]
        private int m_SelectedMatchIndex;
        public int SelectedMatchIndex
        {
            get
            {
                return m_SelectedMatchIndex;
            }
            set
            {
                m_SelectedMatchIndex = value;

                NotifyPropertyChanged("SelectedMatchIndex");
                NotifyPropertyChanged("MatchDescription");
            }
        }
        

        /// <summary>
        /// If true, advanced settings are visible
        /// </summary>
        public Boolean ShowAdvanced { get; set; }

        /// <summary>
        /// The type of surface the match is played upon
        /// </summary>
        [DataMember]
        public int Surface
        {
            get
            {
                return m_Surface;
            }
            set
            {
                m_Surface = value;
                NotifyPropertyChanged("SurfaceRepresentation");
            }
        }

        
        
        /// <summary>
        /// String representation of the surface type
        /// </summary>
        public String SurfaceRepresentation
        {
            get
            {
                switch (Surface)
                {
                    case 0:
                       return TennisMatch.Surface.Clay.ToString();
                    case 1:
                        return "Artificial grass";// TennisMatch.Surface.ArtificialGrass;
                    case 2:
                        return TennisMatch.Surface.Hard.ToString();
                    case 3:
                        return TennisMatch.Surface.Indoor.ToString();
                    case 4:
                        return TennisMatch.Surface.Grass.ToString();
                }

                return "Unknown";
            }
        }

        private vmPlayers m_PreviousPlayers;
        /// <summary>
        /// List of previous players for autocomplete usages
        /// </summary>
        [DataMember]
        public vmPlayers PreviousPlayers
        {
            get
            {
                if (m_PreviousPlayers == null) m_PreviousPlayers = new vmPlayers();
                return m_PreviousPlayers;
            }
            set
            {
                m_PreviousPlayers = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Translate the index to the enum LogLevelEnum
        /// </summary>
        /// <returns></returns>
        public TennisMatch.LogLevelEnum GetLogLevel()
        {
            TennisMatch.LogLevelEnum _LogLevel;

            switch (LogLevel)
            {
                case 0: _LogLevel = TennisMatch.LogLevelEnum.Points; break;
                case 1: _LogLevel = TennisMatch.LogLevelEnum.Errors; break;
                case 2: _LogLevel = TennisMatch.LogLevelEnum.Shots; break;
                case 3: _LogLevel = TennisMatch.LogLevelEnum.Placement; break;
                default: _LogLevel = TennisMatch.LogLevelEnum.Points; break;
            }

            return _LogLevel;
        }

        /// <summary>
        /// Translate the index to the enum Surface
        /// </summary>
        /// <returns></returns>
        public TennisMatch.Surface GetSurface()
        {
            TennisMatch.Surface _Surface;

            switch (m_Surface)
            {
                case 0: _Surface = TennisMatch.Surface.Clay; break;
                case 1: _Surface = TennisMatch.Surface.ArtificialGrass; break;
                case 2: _Surface = TennisMatch.Surface.Hard; break;
                case 3: _Surface = TennisMatch.Surface.Indoor; break;
                case 4: _Surface = TennisMatch.Surface.Grass; break;
                default: _Surface = TennisMatch.Surface.Clay; break;
            }

            return _Surface;
        }

        /// <summary>
        /// Translate the index to the number of sets
        /// </summary>
        /// <returns></returns>
        public int GetBestOutOf()
        {
            int _BestOutOf;

            _BestOutOf = BestOutOf * 2 + 1;

            return _BestOutOf;
        }

        /// <summary>
        /// Translate the index to the MatchType enum
        /// </summary>
        /// <returns></returns>
        public TennisMatch.MatchType GetType()
        {
            TennisMatch.MatchType _Type;

            switch (Type)
            {
                case 0: _Type = TennisMatch.MatchType.Singles; break;
                case 1: _Type = TennisMatch.MatchType.Doubles; break;

                default: _Type = TennisMatch.MatchType.Singles; break;
            }

            return _Type;
        }

        /// <summary>
        /// Store the selected location to the collection of locations.
        /// Do not store if the name and the position < 3 km of a location already present
        /// </summary>
        public void StoreLocation()
        {
            if (this.Location == _ConstUnknownLocation) return;
            if (GeoPosition == null) return;

            foreach (vmLocation Location in Locations)
            {
                if (Location.Name == this.Location)
                {
                    double Distance = (CalculateDistance(Location.Latitude,
                                            Location.Longitude,
                                            GeoPosition.Coordinate.Point.Position.Latitude,
                                            GeoPosition.Coordinate.Point.Position.Longitude));

                    if (Distance < 2000) //A location with the samen name within two kilometers is considered the same
                    {
                        return;
                    }
                }
            }

            vmLocation newLocation = new vmLocation();
            newLocation.Name = this.Location;
            newLocation.Latitude = GeoPosition.Coordinate.Point.Position.Latitude;
            newLocation.Longitude = GeoPosition.Coordinate.Point.Position.Longitude;
            newLocation.SetNameAndLocation(GeoPosition);
            Locations.Add(newLocation);

            UpdateNearbyLocations();
        }

        /// <summary>
        /// Add the current players to the list of Players
        /// </summary>
        public void StorePlayers()
        {
            
            PreviousPlayers.AddPlayer(Player1);
            PreviousPlayers.AddPlayer(Player2);

            if (Type!=0)
            {
                PreviousPlayers.AddPlayer(Player1Partner);
                PreviousPlayers.AddPlayer(Player2Partner);
            }
        }

        /// <summary>
        /// Switch the show advanced option
        /// </summary>
        public void SwitchAdvanced()
        {
            ShowAdvanced = !ShowAdvanced;
            NotifyPropertyChanged("ShowAdvanced");
        }

        /// <summary>
        /// Calculate the distance between to locations
        /// </summary>
        /// <param name="prevLat"></param>
        /// <param name="prevLong"></param>
        /// <param name="currLat"></param>
        /// <param name="currLong"></param>
        /// <returns></returns>
        public static double CalculateDistance(double prevLat, double prevLong, double currLat, double currLong)
        {
            const double degreesToRadians = (Math.PI / 180.0);
            const double earthRadius = 6371; // kilometers

            // convert latitude and longitude values to radians
            var prevRadLat = prevLat * degreesToRadians;
            var prevRadLong = prevLong * degreesToRadians;
            var currRadLat = currLat * degreesToRadians;
            var currRadLong = currLong * degreesToRadians;

            // calculate radian delta between each position.
            var radDeltaLat = currRadLat - prevRadLat;
            var radDeltaLong = currRadLong - prevRadLong;

            // calculate distance
            var expr1 = (Math.Sin(radDeltaLat / 2.0) *
                         Math.Sin(radDeltaLat / 2.0)) +

                        (Math.Cos(prevRadLat) *
                         Math.Cos(currRadLat) *
                         Math.Sin(radDeltaLong / 2.0) *
                         Math.Sin(radDeltaLong / 2.0));

            var expr2 = 2.0 * Math.Atan2(Math.Sqrt(expr1),
                                         Math.Sqrt(1 - expr1));

            var distance = (earthRadius * expr2);
            return distance * 1000;  // return results as meters
        }

        /// <summary>
        /// Get the position of the device (async)
        /// </summary>
        public async Task GetPositionAsync()
        {
            try
            {
                if (_geolocator == null) _geolocator = new Geolocator();
                Geoposition pos = await _geolocator.GetGeopositionAsync();

                GeoPosition = pos;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Rebuild the list of nearby locations
        /// </summary>
        public void UpdateNearbyLocations()
        {
            bool Added;
            LocationsNearby.Clear();
            
            foreach (vmLocation Location in Locations)
            {
                Location.SetNameAndLocation(m_GeoPosition);
                if (Location.Distance < 10)
                {
                    int index = 0;

                    //Insert the location
                    Added = false;
                    while (index < LocationsNearby.Count)
                    {
                        if (LocationsNearby[index].Distance > Location.Distance)
                        {
                            LocationsNearby.Insert(index, Location);
                            Added = true;
                            break;
                        }
                        index++;
                    }

                    if (!Added) LocationsNearby.Add(Location);

                    //if (LocationsNearby.Count > 2) LocationsNearby.RemoveAt(3);
                }
            }

            //Set the location 
            if (LocationsNearby.Count > 0)
            {
                if (LocationsNearby[0].Distance < 1) Location = LocationsNearby[0].Name;
            }
        }

        /// <summary>
        /// Fill the collection with default match variants
        /// </summary>
        private void CreateMatchVariants()
        {
            MatchVariants = TennisMatch.GetDefaultVariants();

            SelectedMatchIndex = 0;
        }

        /// <summary>
        /// Save the current settings
        /// </summary>
        /// <returns></returns>
        public async Task Save()
        {
            //Save the settings to the local storage, do not wait for it (no await)
            try
            {
                String StoreSettingsNewMatch = Helpers.Serializer.SerializeAndCompress(this);
                await Helpers.LocalStorage.Save(StoreSettingsNewMatch, "newmatchsettings.gz", false);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Load the settings from local storage
        /// </summary>
        /// <returns></returns>
        public static async Task<vmNewMatch> Load()
        {
            vmNewMatch _vmNewMatch = new vmNewMatch();

            try
            {
                //Load the last settings from local storage and deserialize
                String StoredSettingsNewMatch = await Helpers.LocalStorage.Load("newmatchsettings.gz");
                if (StoredSettingsNewMatch.Length > 0)
                {
                    _vmNewMatch = (vmNewMatch)Helpers.Serializer.DecompressAndDeserialize(StoredSettingsNewMatch, typeof(vmNewMatch));
                    if (_vmNewMatch == null) _vmNewMatch = new vmNewMatch();
                }
            }
            catch (Exception e)
            {
                //An error occurred, create a brand new one
                _vmNewMatch = new vmNewMatch();
            }

            //await _vmNewMatch.GetPositionAsync();
            //_vmNewMatch.PreviousPlayers.AddOrUpdateLocalPlayer();

            return _vmNewMatch;
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
