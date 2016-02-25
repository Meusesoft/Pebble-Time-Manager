using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pebble_Time_Manager.WatchItems
{
    public enum WatchItemType { Undefined, WatchFace, WatchApp};
    
    public interface IWatchItem
    {
        Guid ID { get; set; }

        String Name { get; set; }

        String Developer { get; set; }

        String File { get; set; }

        WatchItemType Type { get; set;}

        byte VersionMajor { get; set; }

        byte VersionMinor { get; set; }

        byte SDKVersionMajor { get; set; }

        byte SDKVersionMinor { get; set; }

        uint Flags { get; set; }

        uint IconResourceID { get; set; }

        bool Configurable { get; set; }

    }
}
