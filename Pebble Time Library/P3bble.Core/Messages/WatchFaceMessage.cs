using System;
using System.Collections.Generic;
using System.Linq;
using P3bble.Constants;
using P3bble.PCL;

namespace P3bble.Messages
{
    internal class WatchFaceMessage : BaseMessage
    {
        public Guid CurrentWatchFace;

        public WatchFaceMessage()
            : base(Endpoint.WatchFaceSelect)
        {

        }

        protected override void AddContentToMessage(List<byte> payload)
        {
            payload.Add( 0x03 );     
        }

        protected override void GetContentFromMessage(List<byte> payload)
        {
            byte[] _content = payload.ToArray();
            _content = _content.Skip(1).ToArray();

            CurrentWatchFace = new Guid(_content);
        }
    }
}
