using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using P3bble.Types;

namespace Pebble_Time_Manager.WatchItems
{
    [DataContract]
    public class WatchItem : IWatchItem
    {
        #region Constructors

        public WatchItem()
        {
            Type = WatchItemType.Undefined;
        }

        #endregion

        #region Properties

        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public String Developer { get; set; }
        [DataMember]
        public String File { get; set; }
        [DataMember]
        public WatchItemType Type { get; set; }
        [DataMember]
        public byte VersionMajor { get; set; }
        [DataMember]
        public byte VersionMinor { get; set; }
        [DataMember]
        public byte SDKVersionMajor { get; set; }
        [DataMember]
        public byte SDKVersionMinor { get; set; }
        [DataMember]
        public uint Flags { get; set; }
        [DataMember]
        public uint IconResourceID { get; set; }
        #endregion
        [DataMember]
        public bool Configurable { get; set; }

        #region Methods

        /// <summary>
        /// Create and initialise a watchitem instance which corresponds with the pbw
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public static async Task<WatchItem> Load(String Filename)
        {
            Bundle newBundle = await Bundle.LoadFromLocalStorageAsync(Filename);

            return Load(newBundle);
        }

        /// <summary>
        /// Create and initialise a watchitem instance which corresponds with the pbw
        /// </summary>
        /// <param name="_Bundle"></param>
        /// <returns></returns>
        public static WatchItem Load(P3bble.Types.Bundle _Bundle)
        {
            WatchItem _newItem = new WatchItem();

            _newItem.ID = _Bundle.Application.Uuid;
            _newItem.Name = _Bundle.Application.AppName;
            _newItem.Developer = _Bundle.Application.CompanyName;
            _newItem.File = _Bundle.Filename;
            _newItem.VersionMajor = _Bundle.Application.AppMajorVersion;
            _newItem.VersionMinor = _Bundle.Application.AppMinorVersion;
            _newItem.SDKVersionMajor = _Bundle.Application.SdkMajorVersion;
            _newItem.SDKVersionMinor = _Bundle.Application.SdkMinorVersion;
            _newItem.Flags = (byte)_Bundle.Application.Flags;
            _newItem.IconResourceID = (byte)_Bundle.Application.IconResourceID;
            _newItem.Configurable = false;

            try
            {
                _newItem.Configurable = _Bundle.AppInfo.Capabilities.Contains("configurable");
            }
            catch (Exception) { }

            

            _newItem.Type = WatchItemType.WatchApp;
            if ((_newItem.Flags & 1) == 1) _newItem.Type = WatchItemType.WatchFace;

            return _newItem;
        }

        #endregion

    }
}
