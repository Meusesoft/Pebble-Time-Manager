using System;
using System.Collections.Generic;
using P3bble.Constants;
using Windows.System.Profile;

namespace P3bble.Messages
{
    public class TimeLineCalenderReminderRemoveMessage : BaseMessage
    {

        public TimeLineCalenderReminderRemoveMessage()
            : base(Endpoint.StandardV3)
        {

        }

        public TimeLineCalenderReminderRemoveMessage(int Transaction, Guid ID)
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
            //Message = "00:15:b1:db:04:56:1b:03:10:6d:8e:3c:41:f4:01:4b:70:a2:d1:53:5a:9d:2a:3f:7a";

            //Add message code
            payload.Add(0x04);

            //Transaction ID
            AddInteger2Payload(payload, Transaction);

            payload.Add(0x03);
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
