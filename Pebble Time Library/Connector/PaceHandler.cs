using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Pebble_Time_Manager;
using Pebble_Time_Manager.Common;
using System.Threading;
using Windows.UI.Core;

namespace Pebble_Time_Manager.Connector
{
    public struct Measurement
    {
        public BasicGeoposition Position;
        public double Distance;
        public DateTime Time;
        public TimeSpan Duration;
        public bool Pause;
    }

    public class PositionEventArgs : System.EventArgs
    {
        public BasicGeoposition Position  { get; set; }
    }

    public class PaceHandler
    {
        #region Constructor

        public PaceHandler()
        {
            Initialize();
        }

        #endregion

        #region Fields

        private Geolocator _geoLocater;
        private int _ConnectionToken;
        private List<Measurement> _measurements;
        private CancellationTokenSource _cancellationToken;
        public static PaceHandler _paceHandler;
        private Guid SportFace = new Guid(new byte[] { 0x4d, 0xab, 0x81, 0xa6, 0xd2, 0xfc, 0x45, 0x8a, 0x99, 0x2c, 0x7a, 0x1f, 0x3b, 0x96, 0xa9, 0x70 });
        private Connector.PebbleConnector _pc;
        private bool _isPositionUpdating;
        private DateTime _lastPositionUpdate;
        private Geoposition _lastPosition;

        #endregion

        #region Properties

        /// <summary>
        /// List of position measurements. A GPX file can be created from this.
        /// </summary>
        private List<Measurement> Measurements
        {
            get
            {
                if (_measurements == null)
                {
                    _measurements = new List<Measurement>();
                }
                return _measurements;
            }
        }

        /// <summary>
        /// The internal geolocator
        /// </summary>
        private Geolocator Geolocator
        {
            get
            {
                if (_geoLocater == null)
                {
                    _geoLocater = new Geolocator();
                    _geoLocater.DesiredAccuracy = PositionAccuracy.High;
                }

                return _geoLocater;
            }
        }

        /// <summary>
        /// CancellationToken to stop execution
        /// </summary>
        public CancellationTokenSource CancellationToken
        {
            get
            {
                if (_cancellationToken == null) _cancellationToken = new CancellationTokenSource();
                return _cancellationToken;

            }
        }

        public double Distance { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan Pace { get; set; }

        public bool Miles{ get; set; }

        public bool IsRunning { get; private set; }

        public bool Paused { get; private set; }

        #endregion


        #region Methods

        /// <summary>
        /// Returns the global instance of the PebbleConnector class
        /// </summary>
        /// <returns></returns>
        public static PaceHandler GetInstance()
        {
            if (_paceHandler == null)
            {
                _paceHandler = new PaceHandler ();
            }

            return _paceHandler;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initialize()
        {
            _pc = Connector.PebbleConnector.GetInstance();

            if (_geoLocater == null)
            {
                _geoLocater = new Geolocator();
                _geoLocater.DesiredAccuracy = PositionAccuracy.High;
            }

            _ConnectionToken = -1;
        }

        /// <summary>
        /// Start measurements
        /// </summary>
        public async void Start()
        {
            //Clear previous measurements
            Measurements.Clear();
            Paused = false;

            _cancellationToken = new CancellationTokenSource();

            //Activate watch app
            if (_ConnectionToken == -1)
            {
                _ConnectionToken = await _pc.Connect(_ConnectionToken);

                if (_pc.IsConnected)
                {
                    await _pc.Launch(SportFace);
                }
            }

            if (_pc.Pebble != null) _pc.Pebble.MessageReceived += ProtocolMessageReceived;

            //Start thread
            /*var t = Task.Run(async () =>
            {
                await ContinuousMeasurements();
            });*/
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message"></param>
        private async void ProtocolMessageReceived(P3bble.P3bbleMessage message)
        {
            if (message.Endpoint == P3bble.Constants.Endpoint.ApplicationMessage)
            {
                if (message.GetType() == typeof(P3bble.Messages.AppMessage))
                {
                    P3bble.Messages.AppMessage _msg = (P3bble.Messages.AppMessage)message;
                    if (_msg.Command == P3bble.Messages.AppCommand.Push && _msg.AppUuid == SportFace)
                    {
                        if (_msg.Response.Last() == 0x02) Resume();
                        if (_msg.Response.Last() == 0x01) Pause();

                        System.Diagnostics.Debug.WriteLine("ProtocolMessageReceived: " + message.ToString());

                        List<byte> payload = new List<byte>();
                        payload.Add(0xff);

                        P3bble.Messages.AppMessage AckMessage = (P3bble.Messages.AppMessage)P3bble.P3bbleMessage.CreateMessage(P3bble.Constants.Endpoint.ApplicationMessage, payload);
                        AckMessage.TransactionId = _msg.TransactionId;

                        await _pc.Pebble.WriteMessageAsync(AckMessage);
                        await SendPebblePaceMessage();
                        await SendPebblePaceMessage();
                    }
                }
            }
        }

        /// <summary>
        /// Pause measurements
        /// </summary>
        public async Task Pause()
        {
            if (!Paused)
            {
                Paused = true;

                AddMeasurement();

                await SendPebblePaceMessage();

                Measurement _lastMeasurement = Measurements.Last();
                _lastMeasurement.Pause = true;
                Measurements[Measurements.Count - 1] = _lastMeasurement;
            }
        }

        /// <summary>
        /// Resume measurements
        /// </summary>
        public async Task Resume()
        {
            if (Paused)
            {
                AddMeasurement();

                await SendPebblePaceMessage();

                Paused = false;
            }
        }
        
        /// <summary>
        /// Stop measurements
        /// </summary>
        public async void Stop()
        {
            CancellationToken.Cancel();
        }

        /// <summary>
        /// The task for continuously monitoring the position; every 5 seconds
        /// </summary>
        /// <returns></returns>
        private async Task ContinuousMeasurements()
        {
            int i = 0;
            IsRunning = true;

            while (!CancellationToken.IsCancellationRequested)
            {
                //Write debug line
                //System.Diagnostics.Debug.WriteLine(String.Format("{0} - {1} - {2} - {3} - {4}", i, _newMeasurement.Position.Latitude, _newMeasurement.Position.Longitude, _newMeasurement.Position.Altitude, DateTime.Now));

                //Wait for 5 seconds
                System.Threading.Tasks.Task.Delay(1000).Wait();

                i++;
            }

            if (_ConnectionToken != -1)
            {
                _pc.Disconnect(_ConnectionToken);
                _ConnectionToken = -1;
            }

            IsRunning = false;
        }

        /// <summary>
        /// Update the Pace information
        /// </summary>
        /// <returns></returns>
        public async Task UpdatePosition()
        {
            //if (Paused) return;
            if (_isPositionUpdating) return;

            try
            {
                //Only update every 5 seconds
                if (_lastPositionUpdate != null)
                {
                    TimeSpan _difference = DateTime.Now - _lastPositionUpdate;
                    if (_difference.TotalSeconds < 5) return;
                }

                _isPositionUpdating = true;

                //Get current position
                _lastPositionUpdate = DateTime.Now;
                Geoposition _pos = await Geolocator.GetGeopositionAsync();
                _lastPosition = _pos;

                if (!Paused)
                {
                    Measurement _newMeasurement = AddMeasurement();

                    //Fire event
                    PositionEventArgs _arg = new PositionEventArgs();
                    _arg.Position = _newMeasurement.Position;
                    OnNewPosition(_arg);
                }

                //Send message to Pebble Time
                //SendPebblePaceMessage();
            }
            catch (Exception)
            {
            }

            _isPositionUpdating = false;
        }

        /// <summary>
        /// Add measurement to the collection
        /// </summary>
        /// <returns></returns>
        private Measurement AddMeasurement()
        {
            //Store measurement
            Measurement _newMeasurement = new Measurement();
            _newMeasurement.Time = DateTime.Now;
            _newMeasurement.Position = _lastPosition.Coordinate.Point.Position;
            _newMeasurement.Duration = new TimeSpan();

            if (Measurements.Count > 0)
            {
                double _deltaDistance = 0;

                if (Measurements.Count > 1)
                {
                    _deltaDistance = CalculateDistance(Measurements.Last().Position.Latitude,
                    Measurements.Last().Position.Longitude,
                    _newMeasurement.Position.Latitude,
                    _newMeasurement.Position.Longitude);
                }

                TimeSpan _deltaDuration = _newMeasurement.Time - Measurements.Last().Time;

                if (Measurements.Last().Pause)
                {
                    _deltaDistance = 0;
                    _deltaDuration = new TimeSpan(0);
                }

                _newMeasurement.Distance = Measurements.Last().Distance + _deltaDistance;
                _newMeasurement.Duration = Measurements.Last().Duration + _deltaDuration;
            }

            Measurements.Add(_newMeasurement);

            return _newMeasurement;
        }

        /// <summary>
        /// Send pace message to Pebble Time
        /// </summary>
        /// <returns></returns>
        public async Task SendPebblePaceMessage()
        {
            TimeSpan duration = new TimeSpan(0);
            TimeSpan pace = new TimeSpan(0);
            double distance = 0;

            //Calculate data
            if (Measurements.Count > 1)
            {
                //Calculate duration
                duration = Measurements.Last().Duration;

                //Get distance
                distance = Measurements.Last().Distance;
                if (Miles) distance *= 0.621371192;

                //Calculate pace
                int lastIndex = Measurements.Count - 1;
                int firstIndex = Math.Max(0, lastIndex - 3);

                double paceDistance = Measurements[lastIndex].Distance - Measurements[firstIndex].Distance;
                TimeSpan paceDuration = Measurements[lastIndex].Time - Measurements[firstIndex].Time;

                try
                {
                    double meters_per_second = (1000 * paceDistance) / paceDuration.TotalSeconds;
                    double seconds_per_kilometer = 1000 / meters_per_second;

                    pace = TimeSpan.FromSeconds(seconds_per_kilometer);
                }
                catch (Exception)
                {
                }
            }

            if (Measurements.Count > 0)
            {
                //Calculate duration
                duration = Measurements.Last().Duration + (DateTime.Now - Measurements.Last().Time);
            }

            //Write debug line
            System.Diagnostics.Debug.WriteLine(String.Format("Distance {0} - Duration {1} - Pace {2}", distance, duration.ToString(), pace.ToString()));

            Duration = duration;
            Distance = distance;
            Pace = pace;

            //Send message
            if (_pc.IsConnected)
            {
                await _pc.Pebble.SendSportMessage(duration, distance, pace, !Miles);
            }
        }

        /// <summary>
        /// Calculate the distance between to points
        /// </summary>
        /// <param name="prevLat"></param>
        /// <param name="prevLong"></param>
        /// <param name="currLat"></param>
        /// <param name="currLong"></param>
        /// <returns></returns>
        private double CalculateDistance(double prevLat, double prevLong, double currLat, double currLong)
        {
            /*System.Diagnostics.Debug.WriteLine(prevLat);
            System.Diagnostics.Debug.WriteLine(prevLong);
            System.Diagnostics.Debug.WriteLine(currLat);
            System.Diagnostics.Debug.WriteLine(currLong);*/

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

            //System.Diagnostics.Debug.WriteLine(distance);

            return distance;  // return results as meters
        }

        /// <summary>
        /// Create a GPX file
        /// </summary>
        public async Task CreateGPX()
        {
            //Construct the XML message
            String GPXContent;

            GPXContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + System.Environment.NewLine + "<gpx version=\"1.1\" creator=\"Meusesoft - Pebble Time Connect\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.topografix.com/GPX/1/1\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\">" + System.Environment.NewLine + "<trk>" + System.Environment.NewLine;
            if (Measurements.Count > 0)
            {
                GPXContent += String.Format("<time>{0}Z</time>", Measurements[0].Time.ToString("s")) + System.Environment.NewLine;
            }
            GPXContent += "<trkseg>" + System.Environment.NewLine;

            foreach (var measurement in Measurements)
            {
                if (!measurement.Pause)
                {
                    GPXContent += String.Format("<trkpt lon=\"{0:0.000000000}\" lat=\"{1:0.000000000}\"><ele>{2:0.0}</ele><time>{3}Z</time></trkpt>",
                        measurement.Position.Longitude,
                        measurement.Position.Latitude,
                        measurement.Position.Altitude,
                        measurement.Time.ToString("s")) + System.Environment.NewLine;
                }
                else
                {
                    GPXContent += "</trkseg>" + System.Environment.NewLine + "<trkseg>" + System.Environment.NewLine;
                }
            }

            GPXContent += "</trkseg>" + System.Environment.NewLine;
            GPXContent += "</trk>" + System.Environment.NewLine + "</gpx>" + System.Environment.NewLine;

            //Save the GPX file to local storage
            await LocalStorage.Save(GPXContent, Constants.PaceGPXFile, false);
        }

        #endregion

        #region Events

        //Event handler for new position event
        public delegate void NewPositionEventHandler(object sender, PositionEventArgs e);
        public event NewPositionEventHandler NewPosition;

        protected virtual void OnNewPosition(PositionEventArgs e)
        {
            if (NewPosition != null) NewPosition(this, e);
        }

        #endregion
    }
}
