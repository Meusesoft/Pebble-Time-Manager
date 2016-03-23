using Pebble_Time_Manager.Connector;
using Pebble_Time_Manager.ViewModels;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pebble_Time_Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConnectPage : Page
    {
        private vmBinder _vmBinder;
        public ConnectPage()
        {
            this.InitializeComponent();

            _vmBinder = vmBinder.GetInstance();

            DataContext = _vmBinder;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _vmBinder.Log.Clear();
        }

        private async void btnSynchronizeCalender_Click(object sender, RoutedEventArgs e)
        {
            Connector.PebbleConnector _pc = Connector.PebbleConnector.GetInstance();

            await _pc.StartBackgroundTask(PebbleConnector.Initiator.Synchronize);

            //await _TimeLineSynchronizer.Synchronize();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _vmBinder.Commands.ClearLog = true;
            _vmBinder.Commands.Synchronize = true;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _vmBinder.Commands.ClearLog = false;
            _vmBinder.Commands.Synchronize = false;
        }
    }
}
