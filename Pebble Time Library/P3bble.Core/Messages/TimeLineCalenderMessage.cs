using System;
using System.Collections.Generic;
using P3bble.Constants;
using Windows.System.Profile;

namespace P3bble.Messages
{
    public class TimeLineCalenderMessage : BaseMessage
    {

        public TimeLineCalenderMessage()
            : base(Endpoint.StandardV3)
        {

        }

        public TimeLineCalenderMessage(int Transaction, Guid ID, String Description, String Location, DateTime Time, int Duration, String Details)
            : base(Endpoint.StandardV3)
        {
            this.Transaction = Transaction;
            this.ID = ID;
            this.Description = Description;
            this.Location = Location;
            this.Time = Time;
            this.Duration = Duration;
            this.Details = Details;
            Icon = 0x15;
        }

        #region Properties

        public int Transaction { get; set; }

        public Guid ID { get; set;}

        public String Description {get; set;}

        public String Location { get; set; }

        public DateTime Time { get; set; }

        public int Duration { get; set; }

        public String Details { get; set; }

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

            //Message identifier
            part1.AddRange(ID.ToByteArray());
            part2.AddRange(ID.ToByteArray());

            //Host identifier
            HardwareToken myToken = HardwareIdentification.GetPackageSpecificToken(null);
            Windows.Storage.Streams.IBuffer hardwareId = myToken.Id;
            byte[] hwIDBytes = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(hardwareId);
            byte[] truncArray = new byte[16];
            Array.Copy(hwIDBytes, truncArray, truncArray.Length);
            part2.AddRange(truncArray);

            //Time stamp
            DateTime NowUTC = new DateTimeOffset(Time).UtcDateTime;
            uint Seconds = (uint)(NowUTC - new DateTime(1970, 1, 1)).TotalSeconds;
            byte[] bTime = BitConverter.GetBytes(Seconds);
            part2.AddRange(bTime);

            //Duration
            AddInteger2Payload(part2, Duration);

            part2.AddRange(new byte[] { 0x02, 0x01, 0x00, 0x02 });

            //Length total message

            //Number items
            byte _nItems = 0x01;
            if (Description.Length > 0) _nItems++;
            if (Location.Length > 0) _nItems++;
            if (Details.Length > 0) _nItems++; 
            part3.Add(_nItems);

            part3.AddRange(new byte[] { 0x02, 0x01 });

            //Description
            AddString2Payload(part3, Description);

            //Location
            if (Location.Length > 0)
            {
                part3.AddRange(new byte[] { 0x0b });
                AddString2Payload(part3, Location);
            }

            //Icon
            part3.AddRange(new byte[] { 0x04, 0x04, 0x00 });
            part3.Add((byte)Icons.calender);
            part3.AddRange(new byte[] { 0x00, 0x00, 0x80 });

            //Details
            if (Details.Length > 0)
            {
                part3.Add(0x03);
                AddString2Payload(part3, Details);
            }
           /* else
            {
                part3.Add(0x00);
            }*/

            //Add actions
            part3.AddRange(new byte[] { 0x00, 0x09, 0x01, 0x01, 0x06, 0x00, 0x52, 0x65, 0x6d, 0x6f, 0x76, 0x65, 0x01, 0x02, 0x01, 0x01, 0x0d, 0x00, 0x4d, 0x75, 0x74, 0x65, 0x20, 0x43, 0x61, 0x6c, 0x65, 0x6e, 0x64, 0x61, 0x72 });

                       
            //Construct the message
            AddInteger2Payload(payload, part3.Count - 2);
            payload.AddRange(part3);

            payload.InsertRange(0, part2);
            InsertInteger2Payload(payload, 0, payload.Count);

            payload.InsertRange(0, part1);
            //InsertReverseInteger2Payload(payload, 0, payload.Count);



        }

        protected override void GetContentFromMessage(List<byte> payload)
        {
        
        }



    }
}
