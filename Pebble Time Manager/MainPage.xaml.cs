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
using Pebble_Time_Manager.Connector;
using Windows.UI.Popups;
using P3bble;
using Windows.UI.ViewManagement;

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
            FrameRight.Navigate(typeof(ConnectPage));

            /*if (IsMobile)
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
            }*/

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

                if (_IsMobile.Value)
                {
                    _IsMobile = (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch);
                }

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

        private PebbleDevice PebbleDeviceName;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PebbleConnector _pc = PebbleConnector.GetInstance();
            if (!await _pc.IsPebbleAssociated())
            {
                MessageDialog msgBox = new MessageDialog("No Pebble device has been associated with Pebble Time Manager. Do you want to search and associate a device now?" + System.Environment.NewLine + System.Environment.NewLine + "This will take a couple of seconds to complete.");
                msgBox.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.AssociateInvokedHandler)));
                msgBox.Commands.Add(new UICommand("No", new UICommandInvokedHandler(this.AssociateInvokedHandler)));

                await msgBox.ShowAsync();
            }
        }

        private async void AssociateInvokedHandler(IUICommand command)
        {
            if (command.Label == "Yes")
            {
                _vmBinder.Associate(null);
            }
        }

        private void ScreenSize_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "Small")
            {
                FrameRight.Content = null;
            }
            if (e.NewState.Name == "Wide")
            {
                FrameRight.Navigate(typeof(ConnectPage));
            }
        }
    }
}
