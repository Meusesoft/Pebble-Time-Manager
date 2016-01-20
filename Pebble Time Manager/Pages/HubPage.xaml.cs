
using Pebble_Time_Manager.ViewModels;
using System;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.Connector;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace Pebble_Time_Manager
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private Connector.TimeLineSynchronizer _TimeLineSynchronizer;
        private vmBinder _vmBinder;


        public HubPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            InitialiseStore();

            _vmBinder = vmBinder.GetInstance();

            _TimeLineSynchronizer = _vmBinder.TimeLineSynchronizer;

            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();
            _pc.WatchItems.WatchItemListChanged += _vmBinder.WatchFaces.WatchItemListChanged;
            _pc.WatchItems.WatchItemListChanged += _vmBinder.WatchApps.WatchItemListChanged;

            Hub.DataContext = _vmBinder;
            _vmBinder.Log.CollectionChanged += Log_CollectionChanged;

            _ConnectionToken = -1;
        }

        private void Log_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ListBox _lbLogs = (ListBox)FindChildControl<ListBox>(StatusHub, "lstLogs");
            int nItems = _lbLogs.Items.Count;
            if (nItems > 0)
            {
                _lbLogs.ScrollIntoView(_lbLogs.Items[nItems-1]);
            }
        }

        private async void InitialiseStore()
        {
            Helper.Purchases.getReference().Lock("pebble_notifications");

#if DEBUG
            try
            {
                StorageFile _resource = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/WindowsStoreProxy.xml"));
                await CurrentAppSimulator.ReloadSimulatorAsync(_resource);

                var licenseInformation = CurrentAppSimulator.LicenseInformation;
            }
            catch (Exception e)
            {

            }

            Helper.Purchases.getReference().Unlock("pebble_notifications");
            Helper.Purchases.getReference().Lock("pebble_sports");
            Helper.Purchases.getReference().ClearTryAvailable("pebble_sports");
            Helper.Purchases.getReference().Lock("pebble_tennis");
            Helper.Purchases.getReference().ClearTryAvailable("pebble_tennis");
#endif
        }

        private async void btnSynchronizeCalender_Click(object sender, RoutedEventArgs e)
        {
            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

            await _pc.StartBackgroundTask(PebbleConnector.Initiator.Synchronize);

            //await _TimeLineSynchronizer.Synchronize();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the new page
            Frame.Navigate(typeof(SettingsPage), null);
        }

        private async void btnWipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _TimeLineSynchronizer.Wipe();
            }
            catch (Exception)
            {

            }
        }

        private int _ConnectionToken;
        private async void btnConnect_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                if (await _pc.IsBackgroundTaskRunning())
                {
                    _vmBinder.Log.Add("Disconnecting...");

                    _pc.StopBackgroundTask(PebbleConnector.Initiator.Manual);
                }
                else
                {
                    _vmBinder.Log.Add("Connecting...");

                    await _pc.StartBackgroundTask(PebbleConnector.Initiator.Manual);
                }
            }
            catch (Exception exp)
            {
                _vmBinder.Log.Add("An exception occurred while connecting.");
                _vmBinder.Log.Add(exp.Message);
            }

                /*var devices = await DeviceInformation.FindAllAsync(
                               GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.DeviceInformation),
                               new string[] { "System.Devices.ContainerId" });

                foreach (var device in devices)
                    {
                        System.Diagnostics.Debug.WriteLine(device.Name);
                    }*/

                return;

            try
            {
                Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

                if (_ConnectionToken != -1)
                {
                    _pc.Disconnect(_ConnectionToken);
                    _ConnectionToken = -1;
                    _pc.Pebble.ItemSend -= Pebble_ItemSend;

                    _vmBinder.Log.Add("Disconnected");
                }
                else
                {
                    _vmBinder.Log.Clear();

                    _ConnectionToken = await _pc.Connect(_ConnectionToken);

                    if (_ConnectionToken != -1)
                    {
                        _vmBinder.WatchFaces.ActiveItem = _vmBinder.WatchFaces.Get(_pc.Pebble.CurrentWatchFace);
                        _pc.Pebble._protocol.StartRun();

                        _pc.Pebble.Log = _vmBinder.Log;
                        _vmBinder.Log.Clear();

                        _vmBinder.Log.Add("Connected");

                        _pc.Pebble.ItemSend += Pebble_ItemSend;

                        _vmBinder.Log.Add("Ready to process phone requests");
                    }
                    else
                    {
                        _vmBinder.Log.Add("No connection with Pebble Time.");
                        _vmBinder.Log.Add("Already connected or not paired?.");
                    }
                }
            }
            catch (Exception exp)
            {
                _vmBinder.Log.Add("An exception occurred while connecting.");
                _vmBinder.Log.Add(exp.Message);
            }
        }

        void Pebble_ItemSend(object sender, EventArgs e)
        {
            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

            _pc.Disconnect(_ConnectionToken);
            _ConnectionToken = -1;

            _pc.Pebble.ItemSend -= Pebble_ItemSend;

            _vmBinder.Log.Add("Disconnected");
        }

        private void btnStore_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the new page
            Frame.Navigate(typeof(WatchAppsStore), Hub.SectionsInView[0] == HubWatchFaces ? "faces" : "apps");
        }

        private void Hub_SectionsInViewChanged(object sender, SectionsInViewChangedEventArgs e)
        {
            UpdateButtons();
        }

        /// <summary>
        /// Enter edit mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            vmBinder _vmBinder = (vmBinder)Hub.DataContext;

            _vmBinder.WatchApps.Edit();
            _vmBinder.WatchFaces.Edit();

            ListView _lvWatchFaces = (ListView)FindChildControl<ListView>(HubWatchFaces, "lvWatchFaces");
            _lvWatchFaces.SelectionMode = ListViewSelectionMode.Multiple;
            _lvWatchFaces.IsItemClickEnabled = false;
            ListView _lvWatchApps = (ListView)FindChildControl<ListView>(HubWatchApps, "lvWatchApps");
            _lvWatchApps.SelectionMode = ListViewSelectionMode.Multiple;
            _lvWatchApps.IsItemClickEnabled = false;

            UpdateButtons();
        }

        private DependencyObject FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            int childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childNumber; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                FrameworkElement fe = child as FrameworkElement;
                // Not a framework element or is null
                if (fe == null) return null;

                if (child is T && fe.Name == ctrlName)
                {
                    // Found the control so return
                    return child;
                }
                else
                {
                    // Not found it - search children
                    DependencyObject nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel != null)
                        return nextLevel;
                }
            }
            return null;
        }

        /// <summary>
        /// Leave edit mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            vmBinder _vmBinder = (vmBinder)Hub.DataContext;

            _vmBinder.WatchApps.Save();
            _vmBinder.WatchFaces.Save();

            ListView _lvWatchFaces = (ListView)FindChildControl<ListView>(HubWatchFaces, "lvWatchFaces");
            _lvWatchFaces.SelectionMode = ListViewSelectionMode.None;
            _lvWatchFaces.IsItemClickEnabled = true;
            ListView _lvWatchApps = (ListView)FindChildControl<ListView>(HubWatchApps, "lvWatchApps");
            _lvWatchApps.SelectionMode = ListViewSelectionMode.None;
            _lvWatchApps.IsItemClickEnabled = true;

            UpdateButtons();
        }

        /// <summary>
        /// Update the visibility of appbarbuttons
        /// </summary>
        private void UpdateButtons()
        {
            vmBinder _vmBinder = (vmBinder)Hub.DataContext;

            bool EditMode = _vmBinder.WatchApps.EditMode;

            btnSynchronize.Visibility = (Hub.SectionsInView[0] == StatusHub && !EditMode ? Visibility.Visible : Visibility.Collapsed);
            btnClear.Visibility = (Hub.SectionsInView[0] == StatusHub && !EditMode ? Visibility.Visible : Visibility.Collapsed);
            btnEdit.Visibility = (Hub.SectionsInView[0] != StatusHub && !EditMode ? Visibility.Visible : Visibility.Collapsed);
            btnDelete.Visibility = (Hub.SectionsInView[0] != StatusHub && EditMode && (_vmBinder.WatchFaces.ItemsSelected || _vmBinder.WatchApps.ItemsSelected) ? Visibility.Visible : Visibility.Collapsed);
            btnSave.Visibility = (Hub.SectionsInView[0] != StatusHub && EditMode ? Visibility.Visible : Visibility.Collapsed);
            btnStore.Visibility = (!EditMode && (Hub.SectionsInView[0] == HubWatchApps || Hub.SectionsInView[0] == HubWatchFaces) ? Visibility.Visible : Visibility.Collapsed);
            btnSettings.Visibility = (!EditMode ? Visibility.Visible : Visibility.Collapsed);
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msgBox = new MessageDialog("Are you sure you want to delete the selected items?", "Confirmation");
            msgBox.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            msgBox.Commands.Add(new UICommand("No", new UICommandInvokedHandler(this.CommandInvokedHandler)));

            await msgBox.ShowAsync();

        }

        private async void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Yes")
            {
                await _vmBinder.WatchFaces.DeleteSelectedItems();
                await _vmBinder.WatchApps.DeleteSelectedItems();
            }
        }

        /// <summary>
        /// Resync the Pebble Time with the smart phone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnResync_Click(object sender, RoutedEventArgs e)
        {
            _vmBinder.Log.Add("Initiating resync...");

            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

            await _pc.StartBackgroundTask(PebbleConnector.Initiator.Reset);
        }

        /// <summary>
        /// Process selection change in watch faces during edit mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvWatchFaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vmBinder.WatchFaces.SelectItems(e.AddedItems);
            _vmBinder.WatchFaces.UnselectItems(e.RemovedItems);

            UpdateButtons();
        }

        /// <summary>
        /// An item was clicked, make the watch face active on the Pebble
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void lvWatchFaces_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                vmWatchFace SelectedItem = (vmWatchFace)e.ClickedItem;

                _vmBinder.Log.Add("Select item: " + SelectedItem.Name);

                await _vmBinder.WatchFaces.SetActiveWatchFace(SelectedItem);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("An exception occurred selecting watch item: " + exc.Message);
                _vmBinder.Log.Add("An exception occurred selecting watch item: " + exc.Message);
            }
        }

        /// <summary>
        /// Process selection change in watch faces during edit mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvWatchApps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            _vmBinder.WatchApps.SelectItems(e.AddedItems);
            _vmBinder.WatchApps.UnselectItems(e.RemovedItems);

            UpdateButtons();
        }

        /// <summary>
        /// An item was clicked, make the watch app active on the Pebble
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void lvWatchApps_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                vmWatchApp _app = (vmWatchApp)e.ClickedItem;

                switch (_app.Name)
                {
                    case "Pace":

                        // Navigate to the new page
                        Frame.Navigate(typeof(PaceApp), null);

                        break;

                    case "Tennis":

                        // Navigate to the new page
                        Frame.Navigate(typeof(TennisApp), null);

                        break;

                    default:

                        _vmBinder.Log.Add("Select item: " + _app.Name);

                        await PebbleConnector.GetInstance().Select(_app.Model, WatchItems.WatchItemType.WatchApp);

                        break;
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("An exception occurred launching watch item: " + exc.Message);
                _vmBinder.Log.Add("An exception occurred launching watch item: " + exc.Message);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _vmBinder.Log.Clear();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var bmp = Windows.Media.Playback.BackgroundMediaPlayer.Current;

            System.Diagnostics.Debug.WriteLine("State: player = " + (bmp.CurrentState == Windows.Media.Playback.MediaPlayerState.Playing));





        }
    }
}