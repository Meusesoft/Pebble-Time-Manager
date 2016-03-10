using Pebble_Time_Library.Javascript;
using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.Connector;
using Pebble_Time_Manager.ViewModels;
using Pebble_Time_Manager.WatchItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pebble_Time_Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WatchAppsPage : Page
    {
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private Connector.TimeLineSynchronizer _TimeLineSynchronizer;
        private vmBinder _vmBinder;
        private IWatchItem _WatchAppConfig;


        public WatchAppsPage()
        {
            this.InitializeComponent();

            _vmBinder = vmBinder.GetInstance();

            _TimeLineSynchronizer = _vmBinder.TimeLineSynchronizer;

            DataContext = _vmBinder;
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

            //UpdateButtons();
        }

        private void App_Activated(object sender, string e)
        {
            ConfigWebView.Visibility = Visibility.Collapsed;

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.PebbleWebViewClosed] = e;

            if (_WatchAppConfig != null)
            {
                //_WatchFaceConfig.WebViewClosed(e);
                localSettings.Values[Constants.PebbleWatchItem] = _WatchAppConfig.ID.ToString();
            }

            //Process the pebble webviewclosed in the background

            PebbleConnector _pc = PebbleConnector.GetInstance();
            _pc.StartBackgroundTask(PebbleConnector.Initiator.PebbleWebViewClosed);
        }

        private void PebbleJS_OpenURL(object sender, EventArgs e)
        {
            PebbleKitJS.URLEventArgs _uea = (PebbleKitJS.URLEventArgs)e;
            _WatchAppConfig = _uea.WatchItem;

            ConfigWebView.Navigate(new Uri(_uea.URL));
            ConfigWebView.Visibility = Visibility.Visible;
        }

        private void VmWatchFace_OnException(object sender, EventArgs e)
        {
            ErrorEventArgs eea = (ErrorEventArgs)e;

            MessageDialog msgBox = new MessageDialog(eea.Error, "Error");
            msgBox.Commands.Add(new UICommand("Ok"));

            msgBox.ShowAsync();
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _vmBinder.Commands.EditApps = true;
            _vmBinder.Commands.DeleteApps = false;

            vmWatchApp.OnOpenConfiguration += PebbleJS_OpenURL;
            vmWatchApp.OnException += VmWatchFace_OnException;
            App.Activated += App_Activated;

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _vmBinder.Commands.EditApps = false;
            _vmBinder.Commands.DeleteApps = false;
            _vmBinder.WatchApps.EditMode = false;

            vmWatchApp.OnOpenConfiguration -= PebbleJS_OpenURL;
            vmWatchApp.OnException -= VmWatchFace_OnException;
            App.Activated -= App_Activated;
        }
    }
}
