﻿using Pebble_Time_Library.Common;
using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.WatchItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;


namespace Pebble_Time_Manager.ViewModels
{
    public class vmWatchFaces : INotifyPropertyChanged
    {
        #region Constructors

        public vmWatchFaces()
        {
            Initialize();
        }

        #endregion

        #region Fields

        private ObservableCollection<vmWatchFace> _WatchFaces;
        private vmWatchFace _SelectedItem;
        private vmWatchFace _ActiveItem;
        private bool _EditMode;

        #endregion

        #region Properties

        public ObservableCollection<vmWatchFace> WatchFaces 
        {
            get
            {
                if (_WatchFaces==null) _WatchFaces = new ObservableCollection<vmWatchFace>();
                return _WatchFaces;
            }
        }

        /// <summary>
        /// The selected item
        /// </summary>
        public vmWatchFace SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                _SelectedItem = value;
                NotifyPropertyChanged("SelectedItem");
            }
        }

        /// <summary>
        /// The active watch face
        /// </summary>
        public vmWatchFace ActiveItem
        {
            get
            {
                return _ActiveItem;
            }
            set
            {
                if (_ActiveItem != value)
                {
                    if (_ActiveItem != null) _ActiveItem.Active = false;
                    _ActiveItem = value;
                    if (_ActiveItem != null) _ActiveItem.Active = true;
                }

                NotifyPropertyChanged("ActiveItem");
            }
        }

        /// <summary>
        /// The view is in edit mode; items can be deleted.
        /// </summary>
        public bool EditMode
        {
            get
            {
                return _EditMode;
            }
            set
            {
                if (_EditMode != value)
                {
                    //clear the selection
                    foreach (var element in WatchFaces)
                    {
                        element.Selected = false;
                    }
                }

                _EditMode = value;

                NotifyPropertyChanged("CurrentTemplate");
                NotifyPropertyChanged("EditMode");
            }
        }

        /// <summary>
        /// True if there are selected items
        /// </summary>
        public bool ItemsSelected
        {
            get
            {
                foreach (var item in WatchFaces)
                {
                    if (item.Selected) return true;
                }

                return false;
            }
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            EditMode = false;

            vmWatchFace _WatchFace = new vmWatchFace();
            _WatchFace.Name = "TicToc";
            _WatchFace.Developer = "Pebble";
            _WatchFace.ImageFile = "ms-appx:///Assets/tictoc_icon.png";
            byte[] TicTocGuid = new byte[16] { 0x8F, 0x3C, 0x86, 0x86, 0x31, 0xA1, 0x4F, 0x5F, 0x91, 0xF5, 0x01, 0x60, 0x0C, 0x9B, 0xDC, 0x59 };
            _WatchFace.Model = new Guid(TicTocGuid);
            WatchFaces.Add(_WatchFace);
            LoadImage(_WatchFace);

            EditCommand = new RelayCommand(EditSwitch);
            DeleteCommand = new RelayCommand(Delete);
            BackupCommand = new RelayCommand(Backup);
        }

        private void EditSwitch(object obj)
        {
            EditMode = !EditMode;
        }

        public void Edit()
        {
            EditMode = true;
        }

        public void Save()
        {
            EditMode = false;
        }

        /// <summary>
        /// Event handler for watch items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WatchItemListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                //Clear all watchfaces
                case NotifyCollectionChangedAction.Reset:

                    WatchFaces.Clear();

                    vmWatchFace _WatchFace = new vmWatchFace();
                    _WatchFace.Name = "TicToc";
                    _WatchFace.Developer = "Pebble";
                    _WatchFace.ImageFile = "ms-appx:///Assets/tictoc_icon.png";
                    byte[] TicTocGuid = new byte[16] { 0x8F, 0x3C, 0x86, 0x86, 0x31, 0xA1, 0x4F, 0x5F, 0x91, 0xF5, 0x01, 0x60, 0x0C, 0x9B, 0xDC, 0x59 };
                    _WatchFace.Model = new Guid(TicTocGuid);
                    _WatchFace.Editable = false;
                    WatchFaces.Add(_WatchFace);
                    LoadImage(_WatchFace);

                    System.Diagnostics.Debug.WriteLine("vmWatchFaces reset. ");

                    break;


                //Add viewmodel WatchFace
                case NotifyCollectionChangedAction.Add:

                    Guid CurrentWatchFace = Guid.Empty;
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    if (localSettings.Values.ContainsKey("CurrentWatchFace")) CurrentWatchFace = (Guid)localSettings.Values["CurrentWatchFace"];

                    foreach (IWatchItem item in e.NewItems)
                    {
                        if (item.Type == WatchItemType.WatchFace)
                        {
                            try
                            {
                                var vmExistingWatchFace = WatchFaces.Single(x => x.Model == item.ID);
                                WatchFaces.Remove(vmExistingWatchFace);
                            }
                            catch (Exception) { };
                                
                            vmWatchFace _newWatchFace = new vmWatchFace();
                            _newWatchFace.Name = item.Name;
                            _newWatchFace.Developer = item.Developer;
                            _newWatchFace.Model = item.ID;
                            _newWatchFace.Editable = true;
                            _newWatchFace.Item = item;
                            _newWatchFace.Active = (CurrentWatchFace == item.ID);
                            _newWatchFace.ImageFile = item.File.Replace(".zip", ".gif");
                            _newWatchFace.Configurable = item.Configurable;
                            WatchFaces.Add(_newWatchFace);
                            LoadImage(_newWatchFace);

                            if (_newWatchFace.Active) ActiveItem = _newWatchFace;

                            System.Diagnostics.Debug.WriteLine("vmWatchFaces add item: " + item.Name);
                        }
                    }

                    //Sort
                    var SortedFaces = WatchFaces.OrderBy(x => x.Name).ToList();
                    WatchFaces.Clear();

                    foreach (var App in SortedFaces)
                    {
                        WatchFaces.Add(App);
                    }

                    break;

                //Remove viewmodel WatchFace
                case NotifyCollectionChangedAction.Remove:

                    foreach (IWatchItem item in e.OldItems)
                    {
                        if (item.Type == WatchItemType.WatchFace)
                        {
                            vmWatchFace element = null;
                        
                            foreach (var WatchFace in WatchFaces)
                            {
                                if (WatchFace.Model == item.ID)
                                {
                                    element = WatchFace;
                                    break;
                                }
                            }

                            try
                            {
                                if (element != null) WatchFaces.Remove(element);
                            }
                            catch (Exception) { }
                        }

                        System.Diagnostics.Debug.WriteLine("vmWatchFaces remove item: " + item.Name);
                    }

                    break;
                }
        }

        public async void LoadImage(vmWatchFace item)
        {
            try
            {
                Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                item.Image = new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                if (item.ImageFile.Contains("ms-app"))
                {
                    item.Image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(item.ImageFile));
                }
                else
                {
                    Windows.Storage.StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(item.ImageFile);
                    await item.Image.SetSourceAsync(await file.OpenAsync(Windows.Storage.FileAccessMode.Read));
                }
                
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("vmWatchFaces:LoadImage: " + e.Message);
            }
        }

        /// <summary>
        /// Set the active watch face
        /// </summary>
        /// <param name="_item"></param>
        public async Task SetActiveWatchFace(vmWatchFace _item)
        {
            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                await _pc.Select(_item.Model, WatchItemType.WatchFace);

                ActiveItem = _item;
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Translate Guid to vmWatchFace item
        /// </summary>
        /// <param name="_item"></param>
        /// <returns></returns>
        public vmWatchFace Get(Guid _item)
        {
            var items = WatchFaces.Where(x => x.Model == _item);

            if (items.Count() == 0) return null;

            return items.First();
        }

        /// <summary>
        /// Select items
        /// </summary>
        /// <param name="Items"></param>
        public void SelectItems(IList<object> Items)
        {
            foreach (vmWatchFace item in Items)
            {
                item.Selected = true;
            }

            NotifyPropertyChanged("ItemsSelected");
        }

        /// <summary>
        /// Unselect items
        /// </summary>
        /// <param name="Items"></param>
        public void UnselectItems(IList<object> Items)
        {
            foreach (vmWatchFace item in Items)
            {
                item.Selected = false;
            }

            NotifyPropertyChanged("ItemsSelected");
        }

        private void Delete(object obj)
        {
            DeleteSelectedItems();
        }

        private async void Backup(object obj)
        {
            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();
            await _pc.WatchItems.Backup();
        }

        /// <summary>
        /// Delete selected items from phone and watch
        /// </summary>
        public async Task DeleteSelectedItems()
        {
            int _ConnectionToken = -1;

            try
            {
                //Collect all GUIDs of selected items
                List<Guid> GuidSelectedItems = new List<Guid>();

                var SelectedItems = WatchFaces.Where(x => x.Selected);

                foreach (var SelectedItem in SelectedItems)
                {
                    GuidSelectedItems.Add(SelectedItem.Model);
                }

                //Connect 
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                _ConnectionToken = await _pc.Connect(_ConnectionToken);

                //Remove all selected items
                foreach (var GuidSelectdItem in GuidSelectedItems)
                {
                    var selecteditem = _pc.WatchItems.Where(x => x.ID == GuidSelectdItem);
                    await _pc.DeleteWatchItemAsync(selecteditem.First());
                }

                //Disconnect
                if (_pc.IsConnected)
                {
                    _pc.Disconnect(_ConnectionToken);
                }
            }
            catch (Exception)
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                if (_pc.IsConnected) _pc.Disconnect(_ConnectionToken);
            }

            NotifyPropertyChanged("ItemsSelected");
        }

        /// <summary>
        /// Check for updates in the Pebble store
        /// </summary>
        public async Task CheckUpdates()
        {
            try
            {
                foreach (var WatchItem in WatchFaces)
                {
                    await WatchItem.CheckUpdate();
                    //if (WatchItem.Item!=null) WatchItem.Item.UpdateAvailable = true;
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine("CheckUpdates exception: " + exp.Message);
            }
        }

        #endregion

        #region Commands

        public RelayCommand EditCommand
        {
            get;
            private set;
        }

        public RelayCommand DeleteCommand
        {
            get;
            private set;
        }

        public RelayCommand BackupCommand
        {
            get;
            private set;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
