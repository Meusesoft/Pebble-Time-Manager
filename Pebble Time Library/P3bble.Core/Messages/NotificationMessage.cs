using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P3bble.Constants;

namespace P3bble.Messages
{

    internal class NotificationMessage : BaseMessage
    {

        public NotificationMessage(int Transaction, Guid ID, DateTime Time, String Header, String Details, Icons Icon)
            : base(Endpoint.StandardV3)
        {
            this.Transaction = Transaction;
            this.ID = ID;
            this.Time = Time; 
            this.Details = Details;
            this.Header = Header;
            this.Icon = Icon;
        }

        #region Properties

        public int Transaction { get; set; }

        public Guid ID { get; set;}

        public String Header { get; set; }

        public String Details { get; set; }

        public DateTime Time { get; set; }

        public Icons Icon { get; set; }

        #endregion

        protected override void AddContentToMessage(List<byte> payload)
        {
            /*Part1 = "xx:xx:{1}:01:{0}:04:10:{2}"
              Part2 = "yy:yy:{2}:{3}:{4}:00:00:01:01:00:04:{5}"
              Part3 = "03:00:01:{6}:03:{7}:04:04:00:01:00:00:80"

                0 = Transaction ID = 2 bytes
                1 = Endpoint = 2 bytes b1:db
                2 = Message identifier = 16 bytes
                3 = Host identifier = 16 bytes
                4 = Timestamp (seconds from 1970-1-1) = 4 bytes
                5 = Length total message = 2 bytes
                6 = Header 
                7 = Details
                */

            List<byte> part1 = new List<byte>(0);
            List<byte> part2 = new List<byte>(0);
            List<byte> part3 = new List<byte>(0);

            //Add endpoint
            part1.Add(0x01);

            //Transaction ID
            AddInteger2Payload(part1, Transaction);

            part1.Add(0x04);
            part1.Add(0x10);

            //Message identifier
            part1.AddRange(ID.ToByteArray());
            part2.AddRange(ID.ToByteArray());

            //Random identifier
            part2.AddRange(Guid.NewGuid().ToByteArray());

            //Time stamp
            DateTime NowUTC = new DateTimeOffset(Time).UtcDateTime;
            uint Seconds = (uint)(NowUTC - new DateTime(1970, 1, 1)).TotalSeconds;
            byte[] bTime = BitConverter.GetBytes(Seconds);
            part2.AddRange(bTime);

            part2.AddRange(new byte[] { 0x00, 0x00, 0x01, 0x01, 0x00, 0x04 });

            //Length total message

            //Number items
            part3.AddRange(new byte[] { 0x03, 0x00, 0x01 });

            //Description
            AddString2Payload(part3, Header);

            //Details
            if (Details.Length > 0)
            {
                part3.Add(0x03);
                AddString2Payload(part3, Details);
            }
            else
            {
                part3.Add(0x03);
                AddString2Payload(part3, "!");
            }
            
            //Icon
            part3.AddRange(new byte[] { 0x04, 0x04, 0x00 });
            part3.Add((byte)Icon);
            part3.AddRange(new byte[] { 0x00, 0x00, 0x80 });

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
