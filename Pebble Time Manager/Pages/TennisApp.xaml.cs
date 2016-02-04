using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.Connector;
using Pebble_Time_Manager.ViewModels;
using System;
using System.Threading.Tasks;
using Tennis_Statistics.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pebble_Time_Manager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TennisApp : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private vmBinder _vmBinder;
        public TennisApp()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            ShowPage(NewMatchGrid);

            _vmBinder = vmBinder.GetInstance();

            DataContext = _vmBinder;

            Tennis_Statistics.Helpers.Purchases.getReference().Unlock(255);

            Initialize();
        }

        private void ShowPage(UIElement Page)
        {
            NewMatchGrid.Visibility = Page == NewMatchGrid ? Visibility.Visible : Visibility.Collapsed;
            //BottomCommandBar.Visibility = Page != NewMatchGrid ? Visibility.Visible : Visibility.Collapsed;
            MatchGrid.Visibility = Page == MatchGrid ? Visibility.Visible : Visibility.Collapsed;
            ProgressRing.Visibility = Page == ProgressRing ? Visibility.Visible : Visibility.Collapsed;
            PRelement.IsActive = Page == ProgressRing;
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
            if (_Timer!=null) _Timer.Stop();
            System.Diagnostics.Debug.WriteLine("Timer tennis app stopped");
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
            RegisterShare();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (navigationHelper.CanGoBack())
            {
                navigationHelper.GoBack();
            }
        }

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

                vmBinder _vmBinder = vmBinder.GetInstance();
                await _vmBinder.Tennis.vmMatch.PopulateDataRequest(request, false, true, true, true);
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region Fields

        private vmMatch CurrentMatch;
        private DispatcherTimer _Timer;

        #endregion

        #region Methods
        private async Task Initialize()
        {
            vmBinder _vmBinder = vmBinder.GetInstance();
            _vmBinder.vmNewMatch = await vmNewMatch.Load();

            _vmBinder.Tennis.vmMatch = new vmMatchState();
            _vmBinder.Tennis.vmMatch.IsExtendPossible = false;
            _vmBinder.Tennis.vmMatch.Paused = false;
            _vmBinder.Tennis.vmMatch.InProgress = false;
            _vmBinder.Tennis.vmMatch.Completed = false;

            String JSON = await Tennis_Statistics.Helpers.LocalStorage.Load("tennismatchstate.json");
            if (JSON.Length > 0 && (_vmBinder.Tennis.TryInUse || _vmBinder.Tennis.Purchased))
            {
                ReinitiateMatch();
            }
            else
            {
                await LocalStorage.Delete("tennismatchstate.json");
                await LocalStorage.Delete("tennis_pebble.xml");
            }

            RegisterShare();
        }

        private void ReinitiateMatch()
        {
            vmBinder _vmBinder = vmBinder.GetInstance();
            _vmBinder.Tennis.vmMatch = new vmMatchState();
            _vmBinder.Tennis.vmMatch.Paused = false;

            //_vmBinder.vmMatch.Start(_vmBinder.vmNewMatch);
            ApplicationData.Current.LocalSettings.Values[Constants.TennisState] = "1";
            ApplicationData.Current.LocalSettings.Values[Constants.BackgroundCommunicatieIsRunning] = false;

            ShowPage(ProgressRing);

            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();
        }

        /// <summary>
        /// Handler performede every second
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _Timer_Tick(object sender, object e)
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                //If new state is present; update window
                if (localSettings.Values.Keys.Contains(Constants.TennisState) &&
                    (string)localSettings.Values[Constants.TennisState] == "1")
                {
                    //Process the match state
                    String JSON = await Tennis_Statistics.Helpers.LocalStorage.Load("tennismatchstate.json");
                    if (JSON.Length > 0)
                    {
                        vmMatchState _newState = vmMatchState.Deserialize(JSON);
                        _newState.StatisticsCollection.Initialize();

                        vmBinder _vmBinder = vmBinder.GetInstance();
                        _vmBinder.Tennis.vmMatch.Fill(_newState);

                        if (localSettings.Values.Keys.Contains(Constants.BackgroundCommunicatieIsRunning))
                        {
                            if ((bool)localSettings.Values[Constants.BackgroundCommunicatieIsRunning])
                            {
                                _vmBinder.Tennis.vmMatch.Paused = !_vmBinder.Tennis.vmMatch.InProgress;
                            }
                            else
                            {
                                _vmBinder.Tennis.vmMatch.Paused = true;
                            }
                        }
                        else
                        {
                            _vmBinder.Tennis.vmMatch.Paused = true;
                        }


                        localSettings.Values.Remove(Constants.TennisState);

                        //Show the match page
                        ShowPage(MatchGrid);
                    }
                }

                //Check if error occurred
                if (localSettings.Values.Keys.Contains(Constants.BackgroundCommunicatieError) &&
                    (int)localSettings.Values[Constants.BackgroundCommunicatieError] == (int)BCState.ConnectionFailed)
                {
                    HandleConnectionFailed();

                    localSettings.Values[Constants.BackgroundCommunicatieError] = (int)BCState.OK;

                    vmBinder _vmBinder = vmBinder.GetInstance();
                    _vmBinder.Tennis.vmMatch.Paused = true;
                }

                //Check if background is running -> Paused = false
                if (localSettings.Values.Keys.Contains(Constants.BackgroundCommunicatieIsRunning) &&
                    (bool)localSettings.Values[Constants.BackgroundCommunicatieIsRunning])
                {
                    vmBinder _vmBinder = vmBinder.GetInstance();
                    _vmBinder.Tennis.vmMatch.Paused = false;
                }
                
                //If background task is running and ring is still visible; request for new state
                if (localSettings.Values.Keys.Contains(Constants.BackgroundCommunicatieIsRunning) &&
                    !(bool)localSettings.Values[Constants.BackgroundCommunicatieIsRunning] &&
                    ProgressRing.Visibility == Visibility.Visible)
                {
                    localSettings.Values[Constants.TennisState] = "2";
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine("_Timer_Tick exception: " + exp.Message);
            }
        }

        private async void HandleConnectionFailed()
        {
            MessageDialog msgBox = new MessageDialog("Connection failed with Pebble Time.", "Error");
            msgBox.Commands.Add(new UICommand("Ok"));

            await msgBox.ShowAsync();
        }

        #endregion

        #region Actions and Buttons

        private async void btnExtend_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msgBox = new MessageDialog("Are you sure you want to extend this match with additional sets?", "Confirmation");
            msgBox.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.ExtendCommandInvokedHandler)));
            msgBox.Commands.Add(new UICommand("No", new UICommandInvokedHandler(this.ExtendCommandInvokedHandler)));

            await msgBox.ShowAsync();
        }

        private async void ExtendCommandInvokedHandler(IUICommand command)
        {
            PebbleConnector _pc = PebbleConnector.GetInstance();

            if (command.Label == "Yes")
            {
                try
                {
                    await _pc.StartBackgroundTask(PebbleConnector.Initiator.Tennis);

                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values[Constants.BackgroundTennis] = true;
                    localSettings.Values[Constants.TennisCommand] = "extend";
                }
                catch (Exception exc)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(exc.Message, "Error");

                    // Show the message dialog
                    messageDialog.ShowAsync();
                }
            }
        }

        private async void btnPurchase_Click(object sender, RoutedEventArgs e)
        {
            await Pebble_Time_Manager.Helper.Purchases.getReference().Purchase("pebble_tennis");

            vmBinder _vmBinder = vmBinder.GetInstance();
            _vmBinder.Sport.Purchased = true;            
        }

        /// <summary>
        /// Handler for the event of closing the Flyout for Location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnContestantsFlyout_Click(object sender, RoutedEventArgs e)
        {
            btnContestants.Flyout.Hide();
        }

        /// <summary>
        /// Handler for the event of closing the Flyout for Location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFyCourtOk_Click(object sender, RoutedEventArgs e)
        {
            btnCourt.Flyout.Hide();
        }

        /// <summary>
        /// Handler for the event of closing the Flyout for log level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFyLogLevel_Click(object sender, RoutedEventArgs e)
        {
            btnLogLevel.Flyout.Hide();
        }

        /// <summary>
        /// Handler for the event of closing the Flyout for match settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFyMatch_Click(object sender, RoutedEventArgs e)
        {
            btnMatch.Flyout.Hide();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(MatchGrid);
            return;

            vmBinder _vmBinder = vmBinder.GetInstance();
            PebbleConnector _pc = PebbleConnector.GetInstance();
            //_vmBinder.vmNewMatch = new vmNewMatch();

            _vmBinder.Tennis.vmMatch = new vmMatchState();
            ApplicationData.Current.LocalSettings.Values.Remove(Constants.TennisState);
            //_vmBinder.vmMatch.Start(_vmBinder.vmNewMatch);

            ShowPage(ProgressRing);

            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();

            try
            {
                await _vmBinder.vmNewMatch.Save();

                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[Constants.BackgroundTennis] = true;

                await _pc.StartBackgroundTask(PebbleConnector.Initiator.Tennis);
            }
            catch (Exception exc)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(exc.Message, "Error");

                // Show the message dialog
                messageDialog.ShowAsync();
            }
        }

        private async void btnResume_Click(object sender, RoutedEventArgs e)
        {
            PebbleConnector _pc = PebbleConnector.GetInstance();

            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[Constants.BackgroundTennis] = true;

                await _pc.StartBackgroundTask(PebbleConnector.Initiator.Tennis);                
            }
            catch (Exception exc)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(exc.Message, "Error");

                // Show the message dialog
                messageDialog.ShowAsync();
            }
        }

        private void btnSwitch_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[Constants.TennisCommand] = "switch";
        }

        private async void btnStop_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msgBox = new MessageDialog("Are you sure you want to terminate this match?", "Confirmation");
            msgBox.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.StopCommandInvokedHandler)));
            msgBox.Commands.Add(new UICommand("No", new UICommandInvokedHandler(this.StopCommandInvokedHandler)));

            await msgBox.ShowAsync();
        }

        private void StopCommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Yes")
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[Constants.TennisCommand] = "stop";
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msgBox = new MessageDialog("Are you sure you want to delete this match?", "Confirmation");
            msgBox.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.DeleteCommandInvokedHandler)));
            msgBox.Commands.Add(new UICommand("No", new UICommandInvokedHandler(this.DeleteCommandInvokedHandler)));

            await msgBox.ShowAsync();
        }

        private async void DeleteCommandInvokedHandler(IUICommand command)
        {
            PebbleConnector _pc = PebbleConnector.GetInstance();

            if (command.Label == "Yes")
            {
                await Tennis_Statistics.Helpers.LocalStorage.Delete("tennis_pebble.xml");
                await Tennis_Statistics.Helpers.LocalStorage.Delete("tennismatchstate.json");

                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[Constants.BackgroundTennis] = false;

                _pc.StopBackgroundTask(PebbleConnector.Initiator.Tennis);

                //Show new match page
                ShowPage(NewMatchGrid);

                vmBinder _vmBinder = vmBinder.GetInstance();
                _vmBinder.Tennis.vmMatch.IsExtendPossible = false;
                _vmBinder.Tennis.vmMatch.Paused = false;
                _vmBinder.Tennis.vmMatch.InProgress = false;
                _vmBinder.Tennis.vmMatch.Completed = false;
                _vmBinder.Tennis.vmMatch.Notify();
            }
        }


        private async void btnSuspend_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msgBox = new MessageDialog("Are you sure you suspend this match?", "Confirmation");
            msgBox.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.SuspendCommandInvokedHandler)));
            msgBox.Commands.Add(new UICommand("No", new UICommandInvokedHandler(this.SuspendCommandInvokedHandler)));

            await msgBox.ShowAsync();
        }

        private async void SuspendCommandInvokedHandler(IUICommand command)
        {
            PebbleConnector _pc = PebbleConnector.GetInstance();

            if (command.Label == "Yes")
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values[Constants.TennisCommand] = "suspend";
            }
        }

        #endregion

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _vmBinder.Tennis.TennisVisible = false;    
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _vmBinder.Tennis.TennisVisible = true;
        }
    }
}
