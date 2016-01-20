using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Pebble_Time_Manager.WatchItems
{
    [DataContract]
    [KnownType(typeof(WatchItem))]
    public class WatchApp : WatchItem
    {
        public WatchApp()
        {
            Type = WatchItemType.WatchApp;
        }
    }
}
