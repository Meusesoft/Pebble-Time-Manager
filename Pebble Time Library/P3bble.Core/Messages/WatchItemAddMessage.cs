using System;
using System.Collections.Generic;
using System.Linq;
using P3bble.Constants;
using P3bble.PCL;
using Pebble_Time_Manager.WatchItems;

namespace P3bble.Messages
{
    public class WatchItemAddMessage : BaseMessage
    {
        #region Constructors

        public WatchItemAddMessage()
            : base(Endpoint.StandardV3)
        {        
        }

        public WatchItemAddMessage(int Transaction, WatchItem newWatchItem)
            : base(Endpoint.StandardV3)
        {
            this.Transaction = Transaction;
            this.newWatchItem = newWatchItem;
        }

        #endregion

        #region Properties

        public int Transaction { get; set; }

        public WatchItem newWatchItem { get; set; }

        #endregion

        /// <summary>
        /// Serialises a message into a byte representation ready for sending
        /// </summary>
        /// <returns>A byte array</returns>
        /*public override byte[] ToBuffer()
        {
            List<byte> buf = new List<byte>();

            this.AddContentToMessage(buf);

#if DEBUG
            String Message = "";
            foreach (byte _byte in buf)
            {
                Message += _byte.ToString("X2") + ":";
            }
#endif

            return buf.ToArray();
        }*/
        
        protected override void AddContentToMessage(List<byte> payload)
        {
       // String Message = "00:95:b1:db:01:06:63:02:10:55:15:15:f6:2c:fd:40:3e:b7:23:93:bb:33:ad:e6:b3:7e:00:55:15:15:f6:2c:fd:40:3e:b7:23:93:bb:33:ad:e6:b3:01:00:00:00:01:00:00:00:01:01:05:37:00:00:45:6e:69:67:6d:61:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00"            

            List<byte> part1 = new List<byte>(0);
            List<byte> part2 = new List<byte>(0);

            //Add message code
            part1.Add(0x01);

            //Transaction ID
            AddInteger2Payload(part1, Transaction);

            part1.Add(0x02);
            part1.Add(0x10);

            //Watch face identifier
            part1.AddRange(newWatchItem.ID.ToByteArray());
            
            //Watch face identifier
            part2.AddRange(newWatchItem.ID.ToByteArray());

            part2.AddRange(BitConverter.GetBytes(newWatchItem.Flags)); 
            part2.AddRange(BitConverter.GetBytes(newWatchItem.IconResourceID));
            part2.Add(newWatchItem.VersionMajor);
            part2.Add(newWatchItem.VersionMinor);
            part2.Add(newWatchItem.SDKVersionMajor);
            part2.Add(newWatchItem.SDKVersionMinor); 
            part2.AddRange(new byte[] { 0x00, 0x00 });

            //Watch face name
            byte[] _bytes = System.Text.Encoding.UTF8.GetBytes(newWatchItem.Name);
            part2.AddRange(_bytes);

            //Fill remainder with 0x00
            for (int i=0; i< 96 - newWatchItem.Name.Length; i++)
            {
                part2.Add(0x00);
            }

            //Construct the message
            AddInteger2Payload(payload, part2.Count);
            payload.AddRange(part2);

            payload.InsertRange(0, part1);
        }

        protected override void GetContentFromMessage(List<byte> payload)
        {

        }

    }
}
