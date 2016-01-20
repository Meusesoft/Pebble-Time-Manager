using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Collections.Specialized;
using Pebble_Time_Manager.Common;

namespace Pebble_Time_Manager.WatchItems
{

    [KnownType(typeof(WatchApp))]
    [KnownType(typeof(WatchFace))]
    [CollectionDataContract]
    public class WatchItems : List<WatchItem>
    {

        #region Events

        //Event handler for item list change
        public delegate void ItemListEventHandler(object sender, NotifyCollectionChangedEventArgs e);

        public event ItemListEventHandler WatchItemListChanged;

        protected virtual void OnItemListChange(NotifyCollectionChangedEventArgs e)
        {
            if (WatchItemListChanged != null) WatchItemListChanged(this, e);
        }
        #endregion
      
        #region Methods
        
        /// <summary>
        /// Add new watch item
        /// </summary>
        /// <param name="_newItem"></param>
        public async Task AddWatchItem(WatchItem _newItem)
        {
            NotifyCollectionChangedEventArgs ea;

            await DeleteWatchItem(_newItem);
            
            //Add the new watch face
            Add(_newItem);

            //Fire event
            ea = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _newItem);
            OnItemListChange(ea);

            await Save();
        }

        /// <summary>
        /// Delete new watch item from collection
        /// </summary>
        /// <param name="_newItem"></param>
        public async Task DeleteWatchItem(WatchItem _delItem)
        {
            NotifyCollectionChangedEventArgs ea;

            //If watchface with same ID exist, remove it
            var Items = this.Where(x => x.ID == _delItem.ID);

            foreach (var Item in Items)
            {
                ea = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _delItem);
                OnItemListChange(ea);
            }

            this.RemoveAll(x => x.ID == _delItem.ID);

            await Save();
        }

        /// <summary>
        /// Save the watch items to local storage
        /// </summary>
        public async Task Save()
        {
            try
            {
                String List = Common.Serializer.Serialize(this);
                await Common.LocalStorage.Save(List, "watchitems.xml", false);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("WatchItems.Save exception: {0}", e.Message));
            }
        }

        /// <summary>
        /// Load the watch items from local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Load()
        {
//#if DEBUG
            if (this.Find(x => x.ID == Guid.Parse(Constants.TennisAppGuid)) == null)
            {
                /*WatchItem newitem = await WatchItem.Load("ms-appx:///Assets/Tennis.pbw");
                Add(newitem);  */             

                /*WatchItem newitem = new WatchItem();
                newitem.Name = "Tennis";
                newitem.Developer = "Meusesoft";
                newitem.ID = Guid.Parse(Constants.TennisAppGuid);
                newitem.File = "ms-appx:///Assets/tennis_icon.png";
                newitem.Flags = 8;
                newitem.SDKVersionMajor = 5;
                newitem.SDKVersionMinor = 72;
                newitem.VersionMajor = 1;
                newitem.VersionMinor = 0;
                newitem.Type = WatchItemType.WatchApp;

                Add(newitem);*/
            }
//#endif

            try
            {
                WatchItems _watchItems;

                String List = await Common.LocalStorage.Load("watchitems.xml");

                if (List.Length == 0)
                {
                    //try backup
                    System.Diagnostics.Debug.WriteLine("Try backup watchitems.xml");
                    List = await Common.LocalStorage.Load("watchitems.bak");
                }

                if (List.Length > 0)
                {
                    //Make backup of xml storage file
                    Common.LocalStorage.Copy("watchtems.xml", "watchitems.bak");

                    _watchItems = (WatchItems)Common.Serializer.Deserialize(List, typeof(WatchItems));

                    //Check if Tennis app is available

                    //process all watch items
                    if (_watchItems != null)
                    {
                        this.Clear();

                        foreach (var item in _watchItems)
                        {
                            this.Add(item);
                        }
                    }
                    //else return false;
                }

                if (this.Find(x => x.ID == Guid.Parse(Constants.TennisAppGuid)) == null)
                {
                    WatchItem newitem = new WatchItem();
                    newitem.Name = "Tennis";
                    newitem.Developer = "Meusesoft";
                    newitem.ID = Guid.Parse(Constants.TennisAppGuid);
                    newitem.File = "ms-appx:///Assets/tennis_icon.png";
                    newitem.Flags = 8;
                    newitem.SDKVersionMajor = 5;
                    newitem.SDKVersionMinor = 72;
                    newitem.VersionMajor = 1;
                    newitem.VersionMinor = 0;
                    newitem.Type = WatchItemType.WatchApp;

                    this.Add(newitem);

                    await Save();
                }

                NotifyCollectionChangedEventArgs ea = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnItemListChange(ea);

                //Trigger event
                List<IWatchItem> _list = new List<IWatchItem>();
                foreach (var item in this)
                {
                    _list.Add(item);

                    NotifyCollectionChangedEventArgs eai = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
                    OnItemListChange(eai);
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("WatchItems.Load exception: {0}", e.Message));
            }

            return false;
        }

        #endregion
    }
}
