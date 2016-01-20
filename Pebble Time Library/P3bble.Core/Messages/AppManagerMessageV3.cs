using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using P3bble.Constants;
using P3bble.Helper;
using P3bble.Types;

namespace P3bble.Messages
{
    public enum APMV3Type { eNone, eAck }

    public class AppManagerMessageV3 : P3bbleMessage
    {
        #region Constructor

        public AppManagerMessageV3()
            : base(Endpoint.AppManagerV3)
        {
            BundleId = new List<byte>();
            Type = APMV3Type.eNone;
        }

        public AppManagerMessageV3(APMV3Type _type)
            : base(Endpoint.AppManagerV3)
        {
            BundleId = new List<byte>();
            Type = _type;
        }

        #endregion

        #region Properties

        public Guid App { get; set; }
        public List<byte> BundleId { get; set; }
        public APMV3Type Type { get; set; }

        #endregion

        protected override void AddContentToMessage(List<byte> payload)
        {
            switch (Type)
            {

                case APMV3Type.eAck:

                    payload.AddRange(new byte[] { 0x01, 0x01 });

                    break;
            }
        }

        protected override void GetContentFromMessage(List<byte> payload)
        {
            AppManagerAction messageType = (AppManagerAction)payload[0];

            byte[] data = payload.ToArray();

            //Get the app/watch guid
            byte[] AppGuid = new byte[16];
            for (int i=0; i<16; i++)
            {
                AppGuid[i] = payload[i + 1];
            }

            App = new Guid(AppGuid);

            //Get the bundle id
            BundleId.Add(payload[17]);
            BundleId.Add(payload[18]);
            BundleId.Add(payload[19]);
            BundleId.Add(payload[20]);
        }
    }
}
