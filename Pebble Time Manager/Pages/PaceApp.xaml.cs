using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pebble_Time_Manager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PaceApp : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Geolocator _geolocator;

        public PaceApp()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

           // ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            vmBinder _vmBinder = vmBinder.GetInstance();
            _vmBinder.Sport.FatalError += vmPace_FatalError;
            DataContext = _vmBinder;

            _geolocator = new Geolocator();
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
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            RegisterShare();

            await _geolocator.GetGeopositionAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        /*private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (navigationHelper.CanGoBack())
            {
                navigationHelper.GoBack();
            }
        }*/

        #endregion

        #region Share

        /// <summary>
        /// Register to the datarequested event of the datatransfermanager
        /// </summary>
        private void RegisterShare()
        {
            DataTransferManager _dtm = DataTransferManager.GetForCurrentView();
            _dtm.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.DataRequested);
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }
        
        /// <summary>
        /// Process the data request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            try
            {
                DataRequest request = e.Request;
                request.Data.Properties.Title = "Shared activity from Pebble Time Manager";
                request.Data.Properties.Description = "Attached is the activity its GPX file.";
                request.Data.SetText("Attached is the activity its GPX file.");

                IStorageFile GPXFile = await LocalStorage.GetFile(Constants.PaceGPXFile);
                List<IStorageFile> GPXFiles = new List<IStorageFile>();
                GPXFiles.Add(GPXFile);

                request.Data.SetStorageItems(GPXFiles);
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region Methods

        private async void btnPurchase_Click(object sender, RoutedEventArgs e)
        {
            await Pebble_Time_Manager.Helper.Purchases.getReference().Purchase("pebble_sports");

            vmBinder _vmBinder = vmBinder.GetInstance();
            _vmBinder.Sport.Purchased = true;
        }
        
        /// <summary>
        /// Respond to a fatal error by showing a messagedialog with the fatal message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void vmPace_FatalError(object sender, ExtendedEventArgs e)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog(e.Error, "Error");

            // Show the message dialog
            messageDialog.ShowAsync();
        }

        #endregion
    }
}
