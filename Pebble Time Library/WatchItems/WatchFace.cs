using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Pebble_Time_Manager.WatchItems
{
    /// <summary>
    /// Used for caching the watch face
    /// </summary>
    [DataContract]
    [KnownType(typeof(WatchItem))]
    public class WatchFace : WatchItem
    {
        public WatchFace()
        {
            Type = WatchItemType.WatchFace;
            VersionMajor = 1;
            VersionMinor = 0;
            VersionMajor = 5;
            VersionMinor = 0;
        }

        //1 = WatchFace (appinfo.json) watchapp - watchface:true
        //8 = Configurable (appinfo.json) capabilities - configurable
        //8 = Contains javascript

        public byte Flags { get; set; }
    }
}
