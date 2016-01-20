using System;
using System.Collections.Generic;
using System.Linq;
using P3bble.Constants;
using P3bble.PCL;

namespace P3bble.Messages
{
    internal class FactorySettingMessage : P3bbleMessage
    {
        private byte[] _cookie;

        public FactorySettingMessage()
            : base(Endpoint.FactorySetting)
        {

        }

        protected override void AddContentToMessage(List<byte> payload)
        {
            byte[] message = { 0x00, 0x09, 0x6d, 0x66, 0x67, 0x5f, 0x63, 0x6f, 0x6c, 0x6f, 0x72 };

            byte[] msg = new byte[0];
            //msg = msg.Concat(prefix).Concat(session).Concat(remote).ToArray();
            msg = msg.Concat(message).ToArray();

            payload.AddRange(msg);
     
        }

        protected override void GetContentFromMessage(List<byte> payload)
        {

        }
    }
}
