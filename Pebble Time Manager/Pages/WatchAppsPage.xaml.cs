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

        public WatchAppsPage()
        {
            this.InitializeComponent();

            _vmBinder = vmBinder.GetInstance();

            _TimeLineSynchronizer = _vmBinder.TimeLineSynchronizer;

            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();
            _pc.WatchItems.WatchItemListChanged += _vmBinder.WatchFaces.WatchItemListChanged;
            _pc.WatchItems.WatchItemListChanged += _vmBinder.WatchApps.WatchItemListChanged;

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

    }
}
