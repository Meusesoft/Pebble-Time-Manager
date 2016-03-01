using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using System.Linq;
using System.Collections.Specialized;
using Pebble_Time_Manager.WatchItems;
using Pebble_Time_Manager.Common;

namespace Pebble_Time_Manager.ViewModels
{
    public class vmWatchApps : INotifyPropertyChanged
    {
        #region Constructors

        public vmWatchApps()
        {
            Initialize();
        }

        #endregion

        #region Fields

        private ObservableCollection<vmWatchApp> _WatchApps;

        #endregion

        #region Properties

        public ObservableCollection<vmWatchApp> WatchApps 
        {
            get
            {
                if (_WatchApps == null) _WatchApps = new ObservableCollection<vmWatchApp>();
                return _WatchApps;
            }
        }

        private bool _EditMode;
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
                    foreach (var element in WatchApps)
                    {
                        element.Selected = false;
                    }
                }

                _EditMode = value;

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
                foreach (var item in WatchApps)
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

            EditCommand = new RelayCommand(EditSwitch);
            DeleteCommand = new RelayCommand(Delete);

            AddSportApp();
        }

        private void AddSportApp()
        {
            /*vmWatchApp _WatchApp = new vmWatchApp();
            _WatchApp.Name = "Pace";
            _WatchApp.Developer = "Meusesoft";
            _WatchApp.ImageFile = "ms-appx:///Assets/sports_icon.png";
            byte[] SportsGuid = new byte[16] { 0x00, 0x3C, 0x86, 0x86, 0x31, 0xA1, 0x4F, 0x5F, 0x91, 0xF5, 0x01, 0x60, 0x0C, 0x9B, 0xDC, 0x59 };
            _WatchApp.Model = new Guid(SportsGuid);
            WatchApps.Add(_WatchApp);
            LoadImage(_WatchApp);*/

            /*_WatchApp = new vmWatchApp();
            _WatchApp.Name = "Tennis";
            _WatchApp.Developer = "Meusesoft";
            _WatchApp.ImageFile = "ms-appx:///Assets/tennis_icon.png";
            _WatchApp.Model = Guid.Parse(Constants.TennisAppGuid);
            WatchApps.Add(_WatchApp);
            LoadImage(_WatchApp);*/
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

                    WatchApps.Clear();
                    AddSportApp();
                
                break;


                //Add viewmodel WatchFace
                case NotifyCollectionChangedAction.Add:

                    foreach (IWatchItem item in e.NewItems)
                    {
                        if (item.Type == WatchItemType.WatchApp)
                        {
                            if (item.ID == Guid.Parse(Constants.TennisAppGuid)) return;

                            try
                            {
                                var vmExistingWatchFace = WatchApps.Single(x => x.Model == item.ID);
                                WatchApps.Remove(vmExistingWatchFace);
                            }
                            catch (Exception) { };

                            vmWatchApp _newWatchApp = new vmWatchApp();
                            _newWatchApp.Name = item.Name;
                            _newWatchApp.Model = item.ID;
                            _newWatchApp.Developer = item.Developer;
                            _newWatchApp.ImageFile = item.File.Replace(".zip", ".gif");
                            _newWatchApp.Configurable = item.Configurable;
                            _newWatchApp.Item = item;
                            
                            WatchApps.Add(_newWatchApp);
                            LoadImage(_newWatchApp);

                            System.Diagnostics.Debug.WriteLine("vmWatchApps add item: " + item.Name);
                        }
                    }

                    break;

                //Remove viewmodel WatchFace
                case NotifyCollectionChangedAction.Remove:

                    foreach (IWatchItem item in e.OldItems)
                    {
                        if (item.Type == WatchItemType.WatchApp)
                        {
                            vmWatchApp element = null;
                        
                            foreach (var WatchApp in WatchApps)
                            {
                                if (WatchApp.Model == item.ID)
                                {
                                    element = WatchApp;
                                    break;
                                }
                            }

                            try
                            {
                                if (element != null) WatchApps.Remove(element);
                            }
                            catch (Exception) { }

                            System.Diagnostics.Debug.WriteLine("vmWatchApps remove item: " + item.Name);

                        }
                    }

                    break;
                }

            //Sort
            var SortedApps = WatchApps.OrderBy(x => x.Name).ToList();
            WatchApps.Clear();
            
            foreach (var App in SortedApps)
            {
                WatchApps.Add(App);
            }

        }

        public async void LoadImage(vmWatchApp item)
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
                System.Diagnostics.Debug.WriteLine("vmWatchApps:LoadImage: " + e.Message);
            }
        }

        /// <summary>
        /// Select items
        /// </summary>
        /// <param name="Items"></param>
        public void SelectItems(IList<object> Items)
        {
            foreach (vmWatchApp item in Items)
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
            foreach (vmWatchApp item in Items)
            {
                item.Selected = false;
            }

            NotifyPropertyChanged("ItemsSelected");
        }

        /// <summary>
        /// Set the active watch face
        /// </summary>
        /// <param name="_item"></param>
        public async Task SetActiveWatchApp(vmWatchApp _item)
        {
            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                await _pc.Launch(_item.Model);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Execute the delete command
        /// </summary>
        /// <param name="obj"></param>
        private void Delete(object obj)
        {
            DeleteSelectedItems();
        }

        /// <summary>
        /// Delete selected items from phone and watch
        /// </summary>
        public async Task DeleteSelectedItems()
        {
            int _ConnectionToken = -1;

            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                _ConnectionToken = await _pc.Connect(_ConnectionToken);

                int i = 0;

                while (i < WatchApps.Count)
                {
                    var item = WatchApps[i];
                    i++;

                    if (item.Selected)
                    {
                        var selecteditems = _pc.WatchItems.Where(x => x.ID == item.Model);

                        if (selecteditems.Count() > 0)
                        {
                            WatchItems.WatchItem selecteditem = selecteditems.First();
                            await _pc.DeleteWatchItemAsync(selecteditem);
                            i--;
                        }
                    }

                    if (_pc.IsConnected)
                    {
                        _pc.Disconnect(_ConnectionToken);
                    }
                }
            }
            catch (Exception)
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                if (_pc.IsConnected) _pc.Disconnect(_ConnectionToken);
            }

            NotifyPropertyChanged("ItemsSelected");
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
