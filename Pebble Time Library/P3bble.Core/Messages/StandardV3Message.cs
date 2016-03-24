using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P3bble.Constants;

namespace P3bble.Messages
{
    /// <summary>
    /// The System Message
    /// <remarks>
    /// These messages are used to signal important events/state-changes to the watch firmware.
    /// </remarks>
    /// </summary>
    internal class StandardV3Message : BaseMessage
    {
        public StandardV3Message()
            : base(Endpoint.StandardV3)
        {
        }

        public StandardV3Message(int Transaction, byte Result)
            : base(Endpoint.StandardV3)
        {
            Identifier = Transaction;
            this.Result = Result;
        }

        /// <summary>
        /// Gets the system command.
        /// </summary>
        /// <value>
        /// The system command.
        /// </value>
        public int Identifier { get; private set; }

        public byte Result { get; private set; }

        protected override void AddContentToMessage(List<byte> payload)
        {
            //00:04:b1: db: 05            
            payload.Add(0x05);

            AddInteger2Payload(payload, Identifier);

            payload.Add(Result);
        }

        protected override void GetContentFromMessage(List<byte> payload)
        {
            if (payload.Count >= 3)
            {
                byte[] biLength = payload.GetRange(0, 2).ToArray();
                this.Identifier = BitConverter.ToInt16(biLength, 0);

                this.Result = payload[2];
            }
        }
    }
}
