using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Pebble_Time_Manager.Weather
{
    public class Weather
    {
        #region Fields

        public ObservableCollection<String> Log;
        private Connector.PebbleConnector _PebbleConnector;
        public bool Fahrenheit;

        #endregion

        #region Methods

        /// <summary>
        /// Synchronize the weather forecast every 12 hours
        /// </summary>
        /// <returns></returns>
        public async Task Synchronize()
        {
            try
            {
                //Get last synchronisation
                DateTime LastWeatherSynchronization = DateTime.MinValue;

                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.Keys.Contains("weathersynchronization"))
                {
                    try
                    {
                        LastWeatherSynchronization = DateTime.Parse((string)localSettings.Values["weathersynchronization"]);
                    }
                    catch(Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Weather:Synchronize:ParseDateTime: " + e.Message);
                    }
                }

                TimeSpan _ts = DateTime.Now - LastWeatherSynchronization;

                //Update the weather message if last synchronization was more than 6 hours ago
                if (_ts.TotalHours > 5)
                {
                    _PebbleConnector = Connector.PebbleConnector.GetInstance();

                    await WeatherSynhronize();
                }

                //set new synchronization time
                if (localSettings.Values.Keys.Contains("weathersynchronization"))
                {
                    localSettings.Values["weathersynchronization"] = DateTime.Now.ToString();
                }
                else
                {
                    localSettings.Values.Add("weathersynchronization", DateTime.Now.ToString());
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Weather:Synchronize: " + e.Message);
            }
        }

        /// <summary>
        /// Remove all messages from timeline and cached data
        /// </summary>
        /// <returns></returns>
        public async Task Clear()
        {
            _PebbleConnector = Connector.PebbleConnector.GetInstance();

            for (int i = 6; i >= 0; i--)
            {
                await RemoveWeatherMessage(i);
            }

            ClearCache();

            return;
        }

        /// <summary>
        /// Clear the cache of the weather synchronization
        /// </summary>
        public void ClearCache()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove("weathersynchronization");

        }

        #endregion

        #region Weather

        /// <summary>
        /// Process the 3 day weather forecast and send message to the Pebble TimeLine
        /// </summary>
        /// <returns></returns>
        public async Task WeatherSynhronize()
        {

            try
            {                
                //Get position
                Geolocator _geoLocator = new Geolocator();
                //_geoLocator.DesiredAccuracyInMeters = 50;

                Geoposition _geoPosition = await _geoLocator.GetGeopositionAsync(
                         maximumAge: TimeSpan.FromMinutes(30),
                         timeout: TimeSpan.FromSeconds(10));

                //Get Location
                MapLocation MapLocation = await GetLocationName(_geoPosition.Coordinate.Point);
                String Location = MapLocation.Address.Town;

                //Get language code
                String LanguageCode = System.Globalization.CultureInfo.CurrentUICulture.Name;

                //Get weather forecast                
                String URL = String.Format("https://api.weather.com/v2/geocode/{0}/{1}/aggregate.json?products=fcstdaily3&language={2}&units={3}&apiKey={4}",
                    _geoPosition.Coordinate.Point.Position.Latitude.ToString("0.00"),
                    _geoPosition.Coordinate.Point.Position.Longitude.ToString("0.00"),
                    LanguageCode,
                    Fahrenheit ? "a" : "m", // a for fahrenheit, m for celsius
                    "bbf04402eb962506451832d6a828b4d0");

                WebRequest _wr = HttpWebRequest.Create(URL);
                WebResponse _wresponse = await _wr.GetResponseAsync();
                Stream _stream = _wresponse.GetResponseStream();

                //Process weather forecast
                StreamReader _tr = new StreamReader(_stream);
                String JSON = _tr.ReadToEnd();

                JsonValue jsonValue = JsonValue.Parse(JSON);

                String Sunrise = jsonValue.GetObject()["fcstdaily3"].GetObject()["data"].GetObject()["forecasts"].GetArray()[0].GetObject()["sunrise"].GetString();
                JsonArray ForecastsArray = jsonValue.GetObject()["fcstdaily3"].GetObject()["data"].GetObject()["forecasts"].GetArray();

                int Forecasts = 6;
                JsonObject WeatherForecast;

                foreach (var Forecast in ForecastsArray)
                {
                    try
                    {
                        //Send sunrise to Pebble
                        if (Forecast.GetObject().ContainsKey("day"))
                        {
                            WeatherForecast = Forecast.GetObject()["day"].GetObject();

                            if (WeatherForecast != null)
                            {
                                await SendWeatherMessage(Forecasts, WeatherForecast, "Sunrise", Location, DateTime.Parse(Forecast.GetObject()["sunrise"].GetString()));
                            }

                            Forecasts--;
                        }

                        //Send sunset to Pebble
                        if (Forecast.GetObject().ContainsKey("night"))
                        {
                            WeatherForecast = Forecast.GetObject()["night"].GetObject();

                            if (WeatherForecast != null)
                            {
                                await SendWeatherMessage(Forecasts, WeatherForecast, "Sunset", Location, DateTime.Parse(Forecast.GetObject()["sunset"].GetString()));
                            }

                            Forecasts--;
                        }

                    }
                    catch (Exception e)
                    {
                        Log.Add("An error occurred while processing a forecast message.");
                    }

                    if (Forecasts < 1) break;
                }
            }
            catch (Exception e)
            {
                Log.Add("An error occurred while retrieving the weather forecast.");
            }
        }

        /// <summary>
        /// Construct and send a weather forecast message to the Pebble
        /// </summary>
        /// <param name="MessageNumber"></param>
        /// <param name="Forecast"></param>
        /// <param name="SunsetSunrise"></param>
        /// <param name="Location"></param>
        /// <param name="Time"></param>
        /// <returns></returns>
        private async Task SendWeatherMessage(int MessageNumber, JsonObject Forecast, String SunsetSunrise, String Location, DateTime Time)
        {
            try
            {
                double TempMin = 0;
                double TempMax = 0;
                IJsonValue jvTempMin;
                IJsonValue jvTempMax;

                //Send Sunrise to Pebble
                jvTempMin = Forecast.GetObject()["temp"];
                if (jvTempMin.ValueType != JsonValueType.Null) TempMin = jvTempMin.GetNumber();

                jvTempMax = Forecast.GetObject()["hi"];
                if (jvTempMax.ValueType != JsonValueType.Null) TempMax = jvTempMax.GetNumber();

                P3bble.Messages.TimeLineWeatherMessage _tlwm = new P3bble.Messages.TimeLineWeatherMessage(_PebbleConnector.GetNextMessageIdentifier(),
                    MessageNumber,
                    ConvertWeatherType2Icon((int)Forecast.GetObject()["icon_code"].GetNumber()),
                    (int)TempMin,
                    (int)TempMax,
                    SunsetSunrise,
                    Forecast.GetObject()["narrative"].GetString(),
                    Location,
                    Time);

                _tlwm.ToBuffer();
                await _PebbleConnector.Pebble.WriteTimeLineCalenderAsync(_tlwm);

                Log.Add(String.Format("{0}: {1}", SunsetSunrise, Time.ToString()));
            }
            catch (Exception e)
            {
                Log.Add(String.Format("An error occured with {0}: {1}", SunsetSunrise, Time.ToString()));
            }
        }

        /// <summary>
        /// Get the Pebble Icon representation of the weather type; rain, sun, clouds etc.
        /// </summary>
        /// <param name="WeatherType"></param>
        /// <returns></returns>
        private byte ConvertWeatherType2Icon(int WeatherIcon)
        {
            if (WeatherIconMap.ContainsKey(WeatherIcon)) return (byte)WeatherIconMap[WeatherIcon];

            return (byte)P3bble.Constants.Icons.rain_cloud_sun;
        }

        /// <summary>
        /// Static list of weatherchannel icon conversion to Pebble Icons
        /// </summary>
        private Dictionary<int, P3bble.Constants.Icons> WeatherIconMap = new Dictionary<int, P3bble.Constants.Icons>()
        {
            {0, P3bble.Constants.Icons.rain},
            {1, P3bble.Constants.Icons.rain},
            {2, P3bble.Constants.Icons.rain},
            {3, P3bble.Constants.Icons.rain},
            {4, P3bble.Constants.Icons.rain},
            {5, P3bble.Constants.Icons.snow},
            {6, P3bble.Constants.Icons.snow},
            {7, P3bble.Constants.Icons.snow},
            {8, P3bble.Constants.Icons.snow},
            {9, P3bble.Constants.Icons.light_rain},
            {10, P3bble.Constants.Icons.snow},
            {11, P3bble.Constants.Icons.light_rain},
            {12, P3bble.Constants.Icons.rain},
            {13, P3bble.Constants.Icons.snow},
            {14, P3bble.Constants.Icons.snow},
            {15, P3bble.Constants.Icons.heavy_snow},
            {16, P3bble.Constants.Icons.heavy_snow},
            {17, P3bble.Constants.Icons.heavy_snow},
            {18, P3bble.Constants.Icons.snow},
            {19, P3bble.Constants.Icons.clouds},
            {20, P3bble.Constants.Icons.light_rain},
            {21, P3bble.Constants.Icons.clouds},
            {22, P3bble.Constants.Icons.clouds},
            {23, P3bble.Constants.Icons.rain_cloud_sun},
            {24, P3bble.Constants.Icons.rain_cloud_sun},
            {25, P3bble.Constants.Icons.rain_cloud_sun},
            {26, P3bble.Constants.Icons.clouds},
            {27, P3bble.Constants.Icons.clouds},
            {28, P3bble.Constants.Icons.clouds},
            {29, P3bble.Constants.Icons.cloud_and_sun},
            {30, P3bble.Constants.Icons.cloud_and_sun},
            {31, P3bble.Constants.Icons.sun},
            {32, P3bble.Constants.Icons.sun},
            {33, P3bble.Constants.Icons.sun},
            {34, P3bble.Constants.Icons.sun},
            {35, P3bble.Constants.Icons.snow},
            {36, P3bble.Constants.Icons.sun},
            {37, P3bble.Constants.Icons.rain},
            {38, P3bble.Constants.Icons.rain},
            {39, P3bble.Constants.Icons.rain},
            {40, P3bble.Constants.Icons.rain},
            {41, P3bble.Constants.Icons.heavy_snow},
            {42, P3bble.Constants.Icons.heavy_snow},
            {43, P3bble.Constants.Icons.heavy_snow},
            {44, P3bble.Constants.Icons.rain_cloud_sun},
            {45, P3bble.Constants.Icons.light_rain},
            {46, P3bble.Constants.Icons.snow},        
            {47, P3bble.Constants.Icons.light_rain},
        };


        /// <summary>
        /// Retrieve the location name from the geo coordinates
        /// </summary>
        /// <param name="geoPoint"></param>
        /// <returns></returns>
        private async Task<MapLocation> GetLocationName(Geopoint geoPoint)
        {
            var result = await MapLocationFinder.FindLocationsAtAsync(geoPoint);

            if (result.Status == MapLocationFinderStatus.Success && result.Locations.Count > 0)
                return result.Locations[0];

            return null;
        }

        /// <summary>
        /// Removes a weather message from the time line
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private async Task<bool> RemoveWeatherMessage(int ID)
        {
            try
            {
                //Construct Message ID
                List<byte> MessageID = new List<byte>();
                MessageID.AddRange(new byte[]{ 0x61, 0xb2, 0x2b, 0xc8, 0x1e, 0x29, 0x46, 0x0d, 0xa2, 0x36, 0x3f, 0xe4, 0x09, 0xa4, 0x39 });
                MessageID.Add((byte)ID);
                Guid _ID = new Guid(MessageID.ToArray());


                //Send remove item command to Pebble
                P3bble.Messages.TimeLineCalenderRemoveMessage _tlcm = new P3bble.Messages.TimeLineCalenderRemoveMessage(_PebbleConnector.GetNextMessageIdentifier(), _ID);
                _tlcm.ToBuffer();
                await _PebbleConnector.Pebble.WriteTimeLineCalenderAsync(_tlcm);

                Log.Add("Remove weather forecast: " + ID.ToString());

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception:" + ID);
            }

            return true;
        }

        #endregion
    }
}
