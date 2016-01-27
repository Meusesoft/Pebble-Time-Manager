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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Pebble_Time_Manager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private vmBinder _vmBinder;
        private Frame _rootFrame;


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
                btnConnect.Visibility = Visibility.Collapsed;
            }


            _vmBinder = vmBinder.GetInstance();
            _vmBinder.PageWatchApp = true;

            DataContext = _vmBinder;
        }

        public static bool IsMobile
        {
            get
            {
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                return (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile");
            }
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
        }

        private void btnPace_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(PaceApp));
        }

        private void btnTennis_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(TennisApp));
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(SettingsPage));
        }

        private void btnFaces_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(WatchFacesPage));
        }

        private void btnApps_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(WatchAppsPage));
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            FrameLeft.Navigate(typeof(ConnectPage));
        }
    }
}
