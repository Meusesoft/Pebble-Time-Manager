using System;
using System.Collections.Generic;
using P3bble.Constants;
using Windows.System.Profile;

namespace P3bble.Messages
{
    public class TimeLineCalenderReminderMessage : BaseMessage
    {

        public TimeLineCalenderReminderMessage()
            : base(Endpoint.StandardV3)
        {

        }

        public TimeLineCalenderReminderMessage(int Transaction, Guid ID, Guid BelongsTo, String Description, String Location, DateTime Time, TimeSpan Reminder, String Details)
            : base(Endpoint.StandardV3)
        {
            this.Transaction = Transaction;
            this.ID = ID;
            this.Description = Description;
            this.Location = Location;
            this.Time = Time;
            this.Details = Details;
            this.Reminder = Reminder;
            this.BelongsTo = BelongsTo;
        }

        #region Properties

        public int Transaction { get; set; }

        public Guid ID { get; set;}

        public String Description {get; set;}

        public String Location { get; set; }

        public DateTime Time { get; set; }

        public TimeSpan Reminder { get; set; }

        public int Duration { get; set; }

        public String Details { get; set; }

        public Guid BelongsTo { get; set; }
        
        #endregion



        protected override void AddContentToMessage(List<byte> payload)
        {
            //00:86:b1:db:01:5d:0a:03:10:6d:8e:3c:41:f4:01:4b:70:a2:d1:53:5a:9d:2a:3f:7a:6f:00:
            //6d:8e:3c:41:f4:01:4b:70:a2:d1:53:5a:9d:2a:3f:7a:4a:4a:65:2a:6b:df:4c:d0:8e:f7:00:72:5a:9f:2d:24:68:0e:9d:55:00:
            //00:03:01:00:03:41:00:03:03:01:07:00:54:65:73:74:69:6e:67:0b:03:00:41:62:63:04:04:00:03:00:00:80:00:04:01:01:07:
            //00:44:69:73:6d:69:73:73:01:0a:01:01:04:00:4d:6f:72:65:02:02:01:01:0d:00:4d:75:74:65:20:43:61:6c:65:6e:64:61:72


//00-83-B1-DB-01-03-00-03-10 1F-CF-3C-FD-64-87-F7-4D-98-82-56-DA-7B-8E-16-CD 6C-00 1F-CF-3C-FD-64-87-F7-4D-98-82-56-DA-7B-8E-16-CD 01-00-D1-3F-02-00-C1-D0-03-00-1E-DA-04-00-C5-41 E8-E8-9C-55 00-00-03-01-00-03 40-00 03-03-01-04-00-74-65-73-74-03-04-00-0D-0A-0D-0A       04-04-00-03-00-00-80-   04-01-01-07-00-44-69-73-6D-69-73-73-01-0A-01-01-04-00-4D-6F-72-65-02-02-01-01-0D-00-4D-75-74-65-20-43-61-6C-65-6E-64-61-72

//00:86:b1:db:01:98:7f:03:10 43:17:18:6d:33:a0:4f:34:8f:6b:77:f0:dd:30:fd:b6 6f:00 43:17:18:6d:33:a0:4f:34:8f:6b:77:f0:dd:30:fd:b6 eb:e9:0a:03:fa:bb:46:90:93:a7:18:98:e2:0c:ea:66 98:ed:9c:55 00:00:03:01:00:03 41:00 02:03:01:0d:00:52:65:6d:69:6e:64:65:72:20:74:65:73:74 04:04:00:03:00:00:80:00:04:01:01:07:00:44:69:73:6d:69:73:73:01:0a:01:01:04:00:4d:6f:72:65:02:02:01:01:0d:00:4d:75:74:65:20:43:61:6c:65:6e:64:61:72



            
            /*Part1 = "xx:xx:{1}:01:{0}:03:10:{2}"
              Part2 = "yy:yy:{2}:{3}:{4}:{8}:02:01:00:02:{5}"
              Part3 = "{10}:02:01:{6}:0b:{7}:04:04:00:15:00:00:80:{9}:00:09:01:01:06:00:52:65:6d:6f:76:65:01:02:01:01:0d:00:4d:75:74:65:20:43:61:6c:65:6e:64:61:72"
                                           ="04:04:00:03:00:00:80:00:04:01:01:07:00:44:69:73:6d:69:73:73:01:0a:01:01:04:00:4d:6f:72:65:02:02:01:01:0d:00:4d:75:74:65:20:43:61:6c:65:6e:64:61:72"
                0 = Transaction ID = 2 bytes
                1 = Text endpoint = 2 bytes b1:db
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

            part1.Add(0x03);
            part1.Add(0x10);

            //Message identifier
            part1.AddRange(ID.ToByteArray());
            part2.AddRange(ID.ToByteArray());

            //Belongs to identifier
            part2.AddRange(BelongsTo.ToByteArray());

            //Time stamp
            DateTime NowUTC = new DateTimeOffset(Time).UtcDateTime;
            NowUTC = NowUTC - Reminder;
            uint Seconds = (uint)(NowUTC - new DateTime(1970, 1, 1)).TotalSeconds;
            byte[] bTime = BitConverter.GetBytes(Seconds);
            part2.AddRange(bTime);


            part2.AddRange(new byte[] { 0x00, 0x00, 0x03, 0x01, 0x00, 0x03 });

            //Length total message

            //Number items
            byte _nItems = 0x01;
            if (Description.Length > 0) _nItems++;
            if (Location.Length > 0) _nItems++;
            if (Details.Length > 0) _nItems++; 
            part3.Add(_nItems);

            part3.AddRange(new byte[] { 0x03, 0x01 });

            //Description
            AddString2Payload(part3, Description);

            //Location
            if (Location.Length > 0)
            {
                part3.AddRange(new byte[] { 0x0b });
                AddString2Payload(part3, Location);
            }


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
            part3.AddRange(new byte[] { 0x04, 0x04, 0x00, 0x03, 0x00, 0x00, 0x80, 0x00 });
            part3.AddRange(new byte[] { 0x04, 0x01, 0x01, 0x07, 0x00, 0x44, 0x69, 0x73, 0x6d, 0x69, 0x73, 0x73, 0x01, 0x0a, 0x01, 0x01, 0x04, 0x00, 0x4d, 0x6f, 0x72, 0x65, 0x02, 0x02, 0x01, 0x01, 0x0d, 0x00, 0x4d, 0x75, 0x74, 0x65, 0x20, 0x43, 0x61, 0x6c, 0x65, 0x6e, 0x64, 0x61, 0x72 });
                       
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
