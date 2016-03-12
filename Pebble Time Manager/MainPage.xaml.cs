using Pebble_Time_Manager.ViewModels;
using Pebble_Time_Manager.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.ApplicationModel.Store;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Pebble_Time_Manager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private vmBinder _vmBinder;

        public MainPage()
        {
            this.InitializeComponent();

            FrameLeft.Navigate(typeof(WatchFacesPage));

            if (IsMobile)
            {
                MySplitView.CompactPaneLength = 0;
                FrameRight.Visibility = Visibility.Collapsed;
                MainGrid.ColumnDefinitions.RemoveAt(1);
                btnConnect.Visibility = Visibility.Visible;
            }
            else
            {
                FrameRight.Navigate(typeof(ConnectPage));
                //FrameRight.Navigate(typeof(JavascriptPage));
                btnConnect.Visibility = Visibility.Collapsed;
            }

            _vmBinder = vmBinder.GetInstance();
            // _vmBinder.PageWatchApp = true;

            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();
            _pc.WatchItems.WatchItemListChanged += _vmBinder.WatchFaces.WatchItemListChanged;
            _pc.WatchItems.WatchItemListChanged += _vmBinder.WatchApps.WatchItemListChanged;
            _pc.WatchItems.Load();

            DataContext = _vmBinder;

            InitialiseStore();
        }

        private static bool? _IsMobile;
        public static bool IsMobile
        {
            get
            {
                if (_IsMobile.HasValue) return _IsMobile.Value;

                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                _IsMobile = (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile");
                return _IsMobile.Value;
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

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(_vmBinder.PageWatchApp);
        }

        private void btnPebbleStore_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(WatchAppsStore));
            MySplitView.IsPaneOpen = false;
        }

        private void btnPace_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(PaceApp));
            MySplitView.IsPaneOpen = false;
        }

        private void btnTennis_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(TennisApp));
            MySplitView.IsPaneOpen = false;
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(SettingsPage));
            MySplitView.IsPaneOpen = false;
        }

        private void btnFaces_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(WatchFacesPage));
            MySplitView.IsPaneOpen = false;
        }

        private void btnApps_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(WatchAppsPage));
            MySplitView.IsPaneOpen = false;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(ConnectPage));
            MySplitView.IsPaneOpen = false;
        }

        private async void btnResync_Click(object sender, RoutedEventArgs e)
        {
            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();
            Connector.TimeLineSynchronizer _tl = new Connector.TimeLineSynchronizer();
            _vmBinder.Log = _tl.Log;
            await _tl.Wipe();
        }
    }
}
