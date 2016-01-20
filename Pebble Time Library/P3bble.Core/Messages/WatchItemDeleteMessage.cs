using System;
using System.Collections.Generic;
using System.Linq;
using P3bble.Constants;
using P3bble.PCL;
using Pebble_Time_Manager.WatchItems;

namespace P3bble.Messages
{
    public class WatchItemDeleteMessage : BaseMessage
    {
        #region Constructors

        public WatchItemDeleteMessage()
            : base(Endpoint.StandardV3)
        {        
        }

        public WatchItemDeleteMessage(int Transaction, WatchItem newWatchItem)
            : base(Endpoint.StandardV3)
        {
            this.WatchItem = newWatchItem;
            this.Transaction = Transaction;
        }

        #endregion

        #region Properties

        public int Transaction { get; set; }

        public WatchItem WatchItem { get; set; }

        #endregion

        #region Methods

        protected override void AddContentToMessage(List<byte> payload)
        {
            //00:15:b1:db:04:0b:93:02:10:1f:0b:07:01:cc:8f:47:ec:86:e7:71:81:39:7f:9a:25
                
            List<byte> part1 = new List<byte>(0);
            List<byte> part2 = new List<byte>(0);

            //Add message code
            part1.Add(0x04);

            //Transaction ID
            AddInteger2Payload(part1, Transaction);

            part1.Add(0x02);
            part1.Add(0x10);

            //Watch face identifier
            part1.AddRange(WatchItem.ID.ToByteArray());

            payload.InsertRange(0, part1);

        }

        protected override void GetContentFromMessage(List<byte> payload)
        {

        }

        #endregion
    }
}
