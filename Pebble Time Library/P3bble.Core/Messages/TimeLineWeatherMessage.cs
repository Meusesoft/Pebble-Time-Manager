using System;
using System.Collections.Generic;
using P3bble.Constants;
using Windows.System.Profile;

namespace P3bble.Messages
{
    public class TimeLineWeatherMessage : BaseMessage
    {

        public TimeLineWeatherMessage()
            : base(Endpoint.StandardV3)
        {

        }

        public TimeLineWeatherMessage(int Transaction, int Number, int Icon, int TemperatureMin, int TemperatureMax, String SunState, String Description, String Location, DateTime Time)
            : base(Endpoint.StandardV3)
        {
            if (Number > 6 || Number < 0) throw new ArgumentOutOfRangeException("Number");
            
            this.Transaction = Transaction;
            this.Number = (byte)Number;
            this.SunState = SunState;
            this.TemperatureMin = TemperatureMin.ToString();
            this.TemperatureMax = TemperatureMax.ToString();
            this.Description = Description;
            this.Location = Location;
            this.Time = Time;
            this.Icon = (byte)Icon;
        }

        #region Properties

        public int Transaction { get; set; }

        public byte Number { get; set;}

        public String SunState { get; set; }

        public String TemperatureMin { get; set; }

        public String TemperatureMax { get; set; }
        
        public String Description { get; set; }

        public String Location { get; set; }

        public DateTime Time { get; set; }

        public byte Icon { get; set; }

        #endregion

        protected override void AddContentToMessage(List<byte> payload)
        {
            /*Part1 = "xx:xx:{1}:01:{0}:01:10:{2}"
              Part2 = "yy:yy:{2}:{3}:{4}:{8}:02:01:00:02:{5}"
              Part3 = "{10}:02:01:{6}:0b:{7}:04:04:00:15:00:00:80:{9}:00:09:01:01:06:00:52:65:6d:6f:76:65:01:02:01:01:0d:00:4d:75:74:65:20:43:61:6c:65:6e:64:61:72"

                0 = Transaction ID = 2 bytes
                1 = Endpoint = 2 bytes b1:db
                2 = Message identifier = 16 bytes
                3 = Host identifier = 16 bytes
                4 = Timestamp (seconds from 1970-1-1) = 4 bytes
                5 = Length total message = 2 bytes
                6 = Description 
                7 = Location
                8 = Duration meeting in minutes = 2 bytes 
                9 = Details
                10 = Number items = 03 (no content) / 04 (content)
                */

            List<byte> part1 = new List<byte>(0);
            List<byte> part2 = new List<byte>(0);
            List<byte> part3 = new List<byte>(0);

            //Add endpoint
           // AddInteger2Payload(part1, (Int32)Endpoint); 
            
            part1.Add(0x01);

            //Transaction ID
            AddInteger2Payload(part1, Transaction);

            part1.Add(0x01);
            part1.Add(0x10);

            //Message identifiers
            part1.AddRange(new byte[] { 0x61, 0xb2, 0x2b, 0xc8, 0x1e, 0x29, 0x46, 0x0d, 0xa2, 0x36, 0x3f, 0xe4, 0x09, 0xa4, 0x39 });
            part1.Add(Number);
            part2.AddRange(new byte[] { 0x61, 0xb2, 0x2b, 0xc8, 0x1e, 0x29, 0x46, 0x0d, 0xa2, 0x36, 0x3f, 0xe4, 0x09, 0xa4, 0x39 });
            part2.Add(Number);
            part2.AddRange(new byte[] { 0x61, 0xb2, 0x2b, 0xc8, 0x1e, 0x29, 0x46, 0x0d, 0xa2, 0x36, 0x3f, 0xe4, 0x09, 0xa4, 0x39, 0xff });

            //Time stamp
            DateTime NowUTC = new DateTimeOffset(Time).UtcDateTime;
            uint Seconds = (uint)(NowUTC - new DateTime(1970, 1, 1)).TotalSeconds;
            byte[] bTime = BitConverter.GetBytes(Seconds);
            part2.AddRange(bTime);


            part2.AddRange(new byte[] { 0x01, 0x00, 0x02, 0x01, 0x00, 0x06 });

            //Part 3
            part3.AddRange(new byte[] { 0x09, 0x02 });

            //Sun state
            part3.Add(0x01);
            AddString2Payload(part3, SunState);

            //Temperature
            part3.Add(0x02);
            AddInteger2Payload(part3, 5 + TemperatureMin.Length + TemperatureMax.Length);
            AddStringOnly2Payload(part3, TemperatureMin);
            part3.AddRange(new byte[] { 0xc2, 0xb0, 0x2f });
            AddStringOnly2Payload(part3, TemperatureMax);
            part3.AddRange(new byte[] { 0xc2, 0xb0 });

            //Description
            part3.Add(0x03);
            AddString2Payload(part3, Description);

            //Icon
            part3.AddRange(new byte[] { 0x04, 0x04, 0x00});
           // part3.Add(0x10); //sun
            //part3.Add(0x15); //rain
            //part3.Add(0x34); //heavy rain
            part3.Add(Icon);
            part3.AddRange(new byte[] { 0x00, 0x00, 0x80, 0x06, 0x04, 0x00, 0x34, 0x00, 0x00, 0x80 });
            
            //Location
            part3.AddRange(new byte[] { 0x0b });
            AddString2Payload(part3, Location);

            //Add actions and source weather
            part3.AddRange(new byte[] { 0x19, 0x0b, 0x00, 0x50, 0x6f, 0x77, 0x65, 0x72, 0x65, 0x64, 0x20, 0x62, 0x79, 0x3a, 0x1a, 0x13, 0x00, 0x54, 0x68, 0x65, 0x20, 0x57, 0x65, 0x61, 0x74, 0x68, 0x65, 0x72, 0x20, 0x43, 0x68, 0x61, 0x6e, 0x6e, 0x65, 0x6c });
        
            //Time stamp
            part3.AddRange(new byte[] { 0x0e, 0x04, 0x00 });
            NowUTC = new DateTimeOffset(DateTime.Now).UtcDateTime;
            Seconds = (uint)(NowUTC - new DateTime(1970, 1, 1)).TotalSeconds;
            bTime = BitConverter.GetBytes(Seconds);
            part3.AddRange(bTime);

             //Add actions and source weather
            part3.AddRange(new byte[] { 0x00, 0x09, 0x01, 0x01, 0x06, 0x00, 0x52, 0x65, 0x6d, 0x6f, 0x76, 0x65, 0x01, 0x02, 0x01, 0x01, 0x0c, 0x00, 0x4d, 0x75, 0x74, 0x65, 0x20, 0x57, 0x65, 0x61, 0x74, 0x68, 0x65, 0x72 });
                               

            //Construct the message
            AddInteger2Payload(payload, part3.Count - 2);
            payload.AddRange(part3);

            payload.InsertRange(0, part2);
            InsertInteger2Payload(payload, 0, payload.Count);

            payload.InsertRange(0, part1);

        }

        protected override void GetContentFromMessage(List<byte> payload)
        {
        
        }
    }
}
