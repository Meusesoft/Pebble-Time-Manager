using Pebble_Time_Manager.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Pebble_Time_Manager.ViewModels;
using Windows.UI.Popups;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pebble_Time_Manager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Connector.TimeLineSynchronizer _TimeLineSynchronizer;

        public SettingsPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            vmBinder _vmBinder = vmBinder.GetInstance();
            _TimeLineSynchronizer = _vmBinder.TimeLineSynchronizer;
            //_vmBinder.NotificationsHandler.FatalError += NotificationsHandler_FatalError;

            DataContext = _vmBinder;

#if DEBUG

#else
                hubNotifications.Visibility = Visibility.Collapsed;
                hubApps.Visibility = Visibility.Collapsed;                
#endif
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Methods

        private async void btnClear_Click(object sender, RoutedEventArgs e)
        {
            await _TimeLineSynchronizer.Clear();
        }

        #endregion

        /*private async void btnPurchase_Click(object sender, RoutedEventArgs e)
        {
            await Pebble_Time_Manager.Helper.Purchases.getReference().Purchase("pebble_notifications");

            vmBinder _vmBinder = vmBinder.GetInstance();
            _vmBinder.NotificationsHandler.NotificationsPurchased = true;          
        }*/

        private void NotificationsHandler_FatalError(object sender, ExtendedEventArgs e)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog(e.Error, "Error");

            // Show the message dialog
            messageDialog.ShowAsync();
        }

        #region Backup and Restore

        private async void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog("A backup will be made on your OneDrive in the folder 'Backup'. If it does not exist it will be created.");
            messageDialog.Commands.Add(new UICommand("Continue", new UICommandInvokedHandler(this.InitiateBackup)));
            messageDialog.Commands.Add(new UICommand("Cancel"));

            await messageDialog.ShowAsync();
        }


        private void InitiateBackup(IUICommand command)
        {
            vmBinder.GetInstance().BackupCommand.Execute(null);
        }

        private async void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog("The last backup from your OneDrive will be restored. All current watch items on your device will be removed.");
            messageDialog.Commands.Add(new UICommand("Continue", new UICommandInvokedHandler(this.InitiateRestore)));
            messageDialog.Commands.Add(new UICommand("Cancel"));

            await messageDialog.ShowAsync();
        }


        private void InitiateRestore(IUICommand command)
        {
            vmBinder.GetInstance().RestoreCommand.Execute(null);
        }

        #endregion

        private void btnClearFiles_Click(object sender, RoutedEventArgs e)
        {
            LocalStorage.DeleteAll();
        }
    }
    }
