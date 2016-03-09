using Pebble_Time_Manager.Connector;
using Pebble_Time_Manager.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Pebble_Time_Library.Javascript;
using Windows.UI.Xaml.Navigation;
using Pebble_Time_Manager.WatchItems;
using Pebble_Time_Manager.Common;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pebble_Time_Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WatchFacesPage : Page
    {
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private Connector.TimeLineSynchronizer _TimeLineSynchronizer;
        private vmBinder _vmBinder;
        private IWatchItem _WatchFaceConfig;

        public WatchFacesPage()
        {
            this.InitializeComponent();

            _vmBinder = vmBinder.GetInstance();

            _TimeLineSynchronizer = _vmBinder.TimeLineSynchronizer;

            DataContext = _vmBinder;
        }

        private void App_Activated(object sender, string e)
        {
            ConfigWebView.Visibility = Visibility.Collapsed;

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.PebbleWebViewClosed] = e;

            if (_WatchFaceConfig != null)
            {
                //_WatchFaceConfig.WebViewClosed(e);
                localSettings.Values[Constants.PebbleWatchItem] = _WatchFaceConfig.ID.ToString();
            }

            //Process the pebble webviewclosed in the background

            PebbleConnector _pc = PebbleConnector.GetInstance();
            _pc.StartBackgroundTask(PebbleConnector.Initiator.PebbleWebViewClosed);
        }

        private void PebbleJS_OpenURL(object sender, EventArgs e)
        {
            PebbleKitJS.URLEventArgs _uea = (PebbleKitJS.URLEventArgs)e;
            _WatchFaceConfig = _uea.WatchItem;

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
        /// Process selection change in watch faces during edit mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvWatchFaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vmBinder.WatchFaces.SelectItems(e.AddedItems);
            _vmBinder.WatchFaces.UnselectItems(e.RemovedItems);

            //UpdateButtons();
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _vmBinder.Commands.EditFaces = true;
            _vmBinder.Commands.DeleteFaces = false;

            //PebbleKitJS.OpenURL += PebbleJS_OpenURL;
            vmWatchFace.OnOpenConfiguration += PebbleJS_OpenURL;
            vmWatchFace.OnException += VmWatchFace_OnException;
            App.Activated += App_Activated;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _vmBinder.Commands.EditFaces = false;
            _vmBinder.Commands.DeleteFaces = false;
            _vmBinder.WatchFaces.EditMode = false;

            //PebbleKitJS.OpenURL -= PebbleJS_OpenURL;
            vmWatchFace.OnOpenConfiguration -= PebbleJS_OpenURL;
            vmWatchFace.OnException -= VmWatchFace_OnException;
            App.Activated -= App_Activated;
        }
    }
}
