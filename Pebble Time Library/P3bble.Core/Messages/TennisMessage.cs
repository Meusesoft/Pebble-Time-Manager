using System;
using System.Collections.Generic;
using System.Linq;
using P3bble.Constants;
using P3bble.PCL;

namespace P3bble.Messages
{
    public class TennisMessage : AppMessage
    {
        #region Constructors

        public TennisMessage()
            : base(Endpoint.ApplicationMessage)
        {
            AppUuid = Guid.Parse(Pebble_Time_Manager.Common.Constants.TennisAppGuid);
        }

        public TennisMessage(String GameScore, String SetScore)
            : base(Endpoint.ApplicationMessage)
        {
            AppUuid = Guid.Parse(Pebble_Time_Manager.Common.Constants.TennisAppGuid);

            Content = new Dictionary<int, object>();
            Content.Add(1, GameScore);
            Content.Add(0, SetScore);
            Content.Add(3, "1");
            Content.Add(255, "1");
        }

        public TennisMessage(String GameScore, String SetScore, String Status)
            : base(Endpoint.ApplicationMessage)
        {
            AppUuid = Guid.Parse(Pebble_Time_Manager.Common.Constants.TennisAppGuid);

            Content = new Dictionary<int, object>();
            Content.Add(1, GameScore);
            Content.Add(0, SetScore);
            Content.Add(3, Status);
            Content.Add(255, "1");
        }

        public TennisMessage(String GameScore, String SetScore, String Sets, String Status)
            : base(Endpoint.ApplicationMessage)
        {
            AppUuid = Guid.Parse(Pebble_Time_Manager.Common.Constants.TennisAppGuid);

            Content = new Dictionary<int, object>();
            Content.Add(1, GameScore);
            Content.Add(0, SetScore);
            Content.Add(4, Sets);
            Content.Add(3, Status);
            Content.Add(255, "1");
        }

        #endregion

        #region Properties

        private String _SetScore { get; set; }
        private String _GameScore { get; set; }

        public String Action { get; set; }

        private String _State { get; set; }

        #endregion

        #region Methods

        protected override void AddContentToMessage(List<byte> payload)
        {
            payload.Add(0x01);
            payload.AddRange(new byte[] { 0x01, 0x50, 0x6b, 0xa5, 0x51, 0x7d, 0xf8, 0x41, 0xce, 0xa0, 0xff, 0x30, 0xd0, 0x3a, 0x88, 0xfa, 0x8d }); //guid app

            payload.AddRange(SetContentDictionary(Content));
        }


        #endregion
    }
}
