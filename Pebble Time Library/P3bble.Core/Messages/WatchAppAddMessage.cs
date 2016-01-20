using System;
using System.Collections.Generic;
using System.Linq;
using P3bble.Constants;
using P3bble.PCL;
using Pebble_Time_Manager.WatchItems;

namespace P3bble.Messages
{
    public class WatchAppAddMessage : BaseMessage
    {
        #region Constructors

        public WatchAppAddMessage()
            : base(Endpoint.StandardV3)
        {        
        }

        public WatchAppAddMessage(int Transaction, WatchApp newWatchApp)
            : base(Endpoint.StandardV3)
        {
            this.Transaction = Transaction;
            this.newWatchApp = newWatchApp;
        }

        #endregion

        #region Properties

        public int Transaction { get; set; }

        public WatchApp newWatchApp { get; set; }

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
            part1.AddRange(newWatchApp.ID.ToByteArray());
            
            //Watch face identifier
            part2.AddRange(newWatchApp.ID.ToByteArray());

            part2.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x05 });
            byte bPos = 18;//(byte)(18 + newWatchApp.Position);
            part2.Add(bPos);
            part2.AddRange(new byte[] { 0x00, 0x00 });

            //Watch face name
            byte[] _bytes = System.Text.Encoding.UTF8.GetBytes(newWatchApp.Name);
            part2.AddRange(_bytes);

            //Fill remainder with 0x00
            for (int i = 0; i < 96 - newWatchApp.Name.Length; i++)
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
