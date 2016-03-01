using System;
using System.Collections.Generic;
using System.Linq;
using P3bble.Constants;
using P3bble.PCL;

namespace P3bble.Messages
{
    public class WatchFaceSelectMessage : BaseMessage
    {
        private byte[] _cookie;

        public Guid currentWatchFace;
        public Guid newWatchFace;

        public WatchFaceSelectMessage(Guid CurrentWatchFace, Guid NewWatchFace)
            : base(Endpoint.WatchFaceSelect)
        {
            currentWatchFace = CurrentWatchFace;
            newWatchFace = NewWatchFace;
        }

        /// <summary>
        /// Serialises a message into a byte representation ready for sending
        /// </summary>
        /// <returns>A byte array</returns>
        public override byte[] ToBuffer()
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
        }
        
        protected override void AddContentToMessage(List<byte> payload)
        {
            if (currentWatchFace != null)
            {
                payload.AddRange(new byte[] { 0x00, 0x11, 0x00, 0x34, 0x02 });
                payload.AddRange(currentWatchFace.ToByteArray());
            }
            payload.AddRange(new byte[]{ 0x00, 0x11, 0x00, 0x34, 0x01 });
            payload.AddRange(newWatchFace.ToByteArray());

        }

        protected override void GetContentFromMessage(List<byte> payload)
        {

        }
    }
}
