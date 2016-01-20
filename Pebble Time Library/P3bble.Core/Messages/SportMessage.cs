using System;
using System.Collections.Generic;
using System.Linq;
using P3bble.Constants;
using P3bble.PCL;

namespace P3bble.Messages
{
    internal class SportMessage : BaseMessage
    {
        #region Constructors

        public SportMessage()
            : base(Endpoint.ApplicationMessage)
        {
            MetricSystem = true;
        }

        public SportMessage(TimeSpan duration, double distance, TimeSpan pace, bool metricsystem)
            : base(Endpoint.ApplicationMessage)
        {
            Duration = duration;
            Distance = distance;
            Pace = pace;
            MetricSystem = metricsystem;
        }

        #endregion

        #region Properties

        public TimeSpan Duration { get; set; }
        public Double Distance { get; set; }
        public TimeSpan Pace { get; set; }
        public bool MetricSystem { get; set; }

        #endregion

        #region Methods

        protected override void AddContentToMessage(List<byte> payload)
        {
            String _Duration = Duration.ToString(@"mm\:ss");
            String _Distance = string.Format("{0:f2}", Distance);
            String _Pace = Pace.ToString(@"mm\:ss");

            payload.Add( 0x01 );
            payload.AddRange(new byte[] { 0xff, 0x4d, 0xab, 0x81, 0xa6, 0xd2, 0xfc, 0x45, 0x8a, 0x99, 0x2c, 0x7a, 0x1f, 0x3b, 0x96, 0xa9 }); //guid app
            payload.AddRange(new byte[] { 0x70, 0x05, 0x00, 0x00, 0x00, 0x00, 0x01 }); 

            AddInteger2Payload(payload, _Duration.Length + 1);
            AddStringOnly2Payload(payload, _Duration);

            payload.AddRange(new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x01 });

            AddInteger2Payload(payload, _Distance.Length + 1);
            AddStringOnly2Payload(payload, _Distance);

            payload.AddRange(new byte[] { 0x00, 0x02, 0x00, 0x00, 0x00, 0x01 });

            if (Pace.TotalMinutes > 45) _Pace = "-";
            if (Pace.TotalMilliseconds == 0) _Pace = "-";

            AddInteger2Payload(payload, _Pace.Length + 1);
            AddStringOnly2Payload(payload, _Pace);

            payload.AddRange(new byte[] { 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x01, 0x00 });
            payload.Add(MetricSystem ? (byte)0x01 : (byte)0x00);
            payload.AddRange(new byte[] { 0x05, 0x00, 0x00, 0x00, 0x02, 0x01, 0x00, 0x01 });

        }

        protected override void GetContentFromMessage(List<byte> payload)
        {

        }

        #endregion
    }
}
