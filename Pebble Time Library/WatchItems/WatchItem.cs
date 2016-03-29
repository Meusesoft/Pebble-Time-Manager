using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using P3bble.Types;
using Pebble_Time_Library.Javascript;

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
        [DataMember]
        public bool Configurable { get; set; }
        [DataMember]
        public List<String> Platforms { get; set; }
        [DataMember]
        public Dictionary<String, int> AppKeys { get; set; }
        [DataMember]
        public Dictionary<String, String> StoredItems { get; set; }

        public bool UpdateAvailable { get; set; }

        #endregion

        #region Fields

        private PebbleKitJS _PebbleKitJS;

        #endregion

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

            try
            {
                _newItem.Platforms = new List<string>();
                _newItem.Platforms.AddRange(_Bundle.AppInfo.TargetPlatforms);
            }
            catch (Exception) { }

            try
            {
                _newItem.AppKeys = new Dictionary<string, int>();

                foreach (var item in _Bundle.AppInfo.AppKeys)
                {
                    _newItem.AppKeys.Add(item.Key, item.Value);
                }
            }
            catch (Exception) { }

            _newItem.Type = WatchItemType.WatchApp;
            if ((_newItem.Flags & 1) == 1) _newItem.Type = WatchItemType.WatchFace;

            return _newItem;
        }

        #endregion

        #region Javascript

        private async Task LoadJavascript()
        {
            if (_PebbleKitJS == null)
            {
                Bundle _Bundle = await Bundle.LoadFromLocalStorageAsync(File);

                _PebbleKitJS = new PebbleKitJS(this);
                
                await _PebbleKitJS.Execute(_Bundle.Javascript);
            }
        }

        public async Task ShowConfiguration()
        {
            try
            {
                await LoadJavascript();

                _PebbleKitJS.ShowConfiguration(this);
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
        }

        public async Task WebViewClosed(string Data)
        {
            try
            {
                await LoadJavascript();

                _PebbleKitJS.WebViewClosed(Data);
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
        }

        public async Task Ready()
        {
            try
            {
                await LoadJavascript();

                _PebbleKitJS.Ready();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);

                throw exp;
            }
        }
        public async Task AppMessage(Dictionary<int, object> Content)
        {
            try
            {
                await LoadJavascript();

                _PebbleKitJS.AppMessage(Content);
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);

                throw exp;
            }
        }

        #endregion

    }
}
