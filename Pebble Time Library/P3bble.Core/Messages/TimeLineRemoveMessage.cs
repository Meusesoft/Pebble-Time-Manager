using System;
using System.Collections.Generic;
using P3bble.Constants;
using Windows.System.Profile;

namespace P3bble.Messages
{
    public class TimeLineCalenderRemoveMessage : BaseMessage
    {

        public TimeLineCalenderRemoveMessage()
            : base(Endpoint.StandardV3)
        {

        }

        public TimeLineCalenderRemoveMessage(int Transaction, Guid ID)
            : base(Endpoint.StandardV3)
        {
            this.Transaction = Transaction;
            this.ID = ID;
        }

        #region Properties

        public int Transaction { get; set; }

        public Guid ID { get; set; }

        #endregion

        protected override void AddContentToMessage(List<byte> payload)
        {
            //Message = "00:15:b1:db:04:65:89:01:10:1e:3b:0d:e3:33:b1:44:ad:9b:7c:24:02:0d:4b:4c:99";

            //Add message code
            payload.Add(0x04);

            //Transaction ID
            AddInteger2Payload(payload, Transaction);

            payload.Add(0x01);
            payload.Add(0x10);

            //Message identifier
            payload.AddRange(ID.ToByteArray());

        }

        protected override void GetContentFromMessage(List<byte> payload)
        {

        }

        /// <summary>
        /// Add value to payload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        private void AddInteger2Payload(List<byte> payload, int value)
        {
            byte[] len = BitConverter.GetBytes((Int16)value);
            payload.AddRange(len);
        }
    }
}
