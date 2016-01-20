using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Pebble_Time_Manager.Calender
{
    /// <summary>
    /// Used for caching the calenderitems send to Pebble
    /// </summary>

    [DataContract]
    public class CalenderItem
    {
        public String RoamingID { get; set; }
        public DateTime Time { get; set; }
        public String CalenderItemID { get; set; }
        public String ReminderID { get; set; }
    }
}
