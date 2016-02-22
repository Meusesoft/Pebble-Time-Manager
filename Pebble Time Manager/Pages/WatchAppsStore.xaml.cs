using Pebble_Time_Manager.Common;
using Pebble_Time_Manager.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pebble_Time_Manager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WatchAppsStore : Page
    {
        private NavigationHelper navigationHelper;
        private vmBinder _vmBinder;

        public WatchAppsStore()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            _vmBinder = vmBinder.GetInstance();
            DataContext = _vmBinder;
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
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            /*if (e.Parameter.GetType() == typeof(String))
            {
                String _parameter = (String)e.Parameter;
                if (_parameter == "faces")
                {
                    btnFace_Click(null, null);
                }
                else
                {
                    btnApps_Click(null, null);
                }
            }*/

            wbView.LongRunningScriptDetected += WbView_LongRunningScriptDetected;
            wbView.ScriptNotify += WbView_ScriptNotify;
            grDownload.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        #endregion


        private void WbView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("WbView_ScriptNotify");
        }

        private void WbView_LongRunningScriptDetected(WebView sender, WebViewLongRunningScriptDetectedEventArgs args)
        {
            //args.StopPageScriptExecution = true;
            System.Diagnostics.Debug.WriteLine("WbView_LongRunningScriptDetected");
        }

        private void wbView_LoadCompleted(object sender, NavigationEventArgs e)
        {

            //Download button visible if app or watchface page is visible
            btnDownload.Visibility = wbView.BaseUri.AbsolutePath.Contains("en_US/application/") ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;

            //Back button visible if page history isn't happy
            btnBack.Visibility = (wbView.CanGoBack) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            wbView.GoBack();
        }

        private void wbView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wbView_NavigationCompleted");
        }

        private void wbView_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wbView_ContentLoading");

            _vmBinder.Store.CheckDownloadableItem(args.Uri);
            LastLoadedUrl = args.Uri.AbsoluteUri;
        }

        private void wbView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wbView_DOMContentLoaded");

            /* String Result = await wbView.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName(\"add-button\").removeAttribute(\"ng-click\");" });
            try
            {
                String Result = await wbView.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName(\"add-button\")).removeAttribute(\"ng-click\");" });
                System.Diagnostics.Debug.WriteLine(Result);
            }
            catch (Exception) { }*/
        }

        private void wbView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("wbView_ScriptNotify");
        }

        private void wbView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("wbView_Tapped");
        }

        private void wbView_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("wbView_Loaded");
        }

        private String LastLoadedUrl;

        private async void wbView_FrameContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wbView_FrameContentLoading");
            System.Diagnostics.Debug.WriteLine(args.Uri.ToString());

            LastLoadedUrl = args.Uri.ToString();

            /*if (args.Uri.ToString().Contains("en_US/application/"))
            {
                btnDownload.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                btnDownload.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            if (args.Uri.ToString().Contains("watchfaces"))
            {
                btnApps.Visibility = Visibility.Visible;
                btnFace.Visibility = Visibility.Collapsed;
            }
            if (args.Uri.ToString().Contains("watchapps"))
            {
                btnApps.Visibility = Visibility.Collapsed;
                btnFace.Visibility = Visibility.Visible;
            }

            try
            {
                   String Result = await wbView.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName(\"add-button\")).removeAttribute(\"ng-click\");" });
                    System.Diagnostics.Debug.WriteLine(Result);
            }
            catch (Exception) { }*/
        }

        private void wbView_FrameDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wbView_FrameDOMContentLoaded");
        }

        private void wbView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wbView_FrameNavigationCompleted");
        }

        private void wbView_FrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wbView_FrameNavigationStarting");
        }

        private void wbView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wbView_NavigationStarting");
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnCancel.Visibility = Visibility.Collapsed;
                btnStop.Visibility = Visibility.Visible;

                cts = new CancellationTokenSource();

                /*string html = await wbView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
                System.Diagnostics.Debug.WriteLine(html);*/
                String currentURL = LastLoadedUrl;
                char[] delimiterChars = { '/' };
                String[] parts = currentURL.Split(delimiterChars);

                //Construct URL
                String PackageID = parts.Last();
                if (PackageID.Length > 24) PackageID = PackageID.Substring(0, 24);
                String URL = String.Format("https://api2.getpebble.com/v2/apps/id/{0}?image_ratio=1&platform=all&hardware=basalt", PackageID);

                //Start webrequest for JSON
                WebRequest _wr = HttpWebRequest.Create(URL);
                WebResponse _wresponse = await _wr.GetResponseAsync();
                Stream _stream = _wresponse.GetResponseStream();

                //Read the JSON
                StreamReader _tr = new StreamReader(_stream);
                String JSON = _tr.ReadToEnd();

                JsonValue jsonValue = JsonValue.Parse(JSON);

                String Title = jsonValue.GetObject()["data"].GetArray()[0].GetObject()["title"].GetString();
                String Type = jsonValue.GetObject()["data"].GetArray()[0].GetObject()["type"].GetString();
                String File = jsonValue.GetObject()["data"].GetArray()[0].GetObject()["latest_release"].GetObject()["pbw_file"].GetString();
                String Uuid = jsonValue.GetObject()["data"].GetArray()[0].GetObject()["uuid"].GetString();
                String ListImage = jsonValue.GetObject()["data"].GetArray()[0].GetObject()["list_image"].GetObject()["80x80"].GetString();

                System.Diagnostics.Debug.WriteLine(String.Format("{0} {1} {2}", Type, Title, File));

                //Initiate download
                HttpClient _hc = new HttpClient();

                HttpResponseMessage response = await _hc.GetAsync(File);
                response.EnsureSuccessStatusCode();

                Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                BackgroundDownloader downloader = new BackgroundDownloader();

                //Show download progress window
                grDownload.Visibility = Visibility.Visible;
                pbDownload.Value = 0;
                txtDownload.Text = String.Format("Downloading {0}", Title);

                //Download image
                downloader = new BackgroundDownloader();
                Windows.Storage.StorageFile destinationFile = await LocalFolder.CreateFileAsync(PackageID + ".gif", CreationCollisionOption.ReplaceExisting);
                DownloadOperation download = downloader.CreateDownload(new Uri(ListImage), destinationFile);

                await HandleDownloadImageAsync(download, true);

                //Download binary
                System.Diagnostics.Debug.WriteLine(String.Format("Download binary: {0}", File));
                destinationFile = await LocalFolder.CreateFileAsync(PackageID + ".zip", CreationCollisionOption.ReplaceExisting);
                download = downloader.CreateDownload(new Uri(File), destinationFile);

                //Create watchitem instance    
                newWatchItem = new WatchItems.WatchItem();
                newWatchItem.File = PackageID + ".zip";
                newWatchItem.ID = new Guid(Uuid);
                newWatchItem.Name = Title;

                await HandleDownloadAsync(download, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);

                MessageDialog msgBox = new MessageDialog("An error occurred while downloading the requested item.", "Error");
                await msgBox.ShowAsync();
            }
        }

        private CancellationTokenSource cts;
        private WatchItems.WatchItem newWatchItem;

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Running: " + download.Guid);

                // Store the download so we can pause/resume.
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token, progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();

                System.Diagnostics.Debug.WriteLine(String.Format("Completed: {0}, Status Code: {1}", download.Guid, response.StatusCode));
                txtDownload.Text = String.Format("Download completed - {0}", newWatchItem.Name);
                // grDownload.Visibility = Visibility.Collapsed;

                if (response.StatusCode == 200)
                {
                    System.Diagnostics.Debug.WriteLine("Downloaded file: " + download.ResultFile.Name);

                    txtDownload.Text = String.Format("Download completed");
                    btnCancel.Visibility = Visibility.Visible;
                    btnStop.Visibility = Visibility.Collapsed;

                    //Set the name of the file in a localsetting
                    var localSettings = ApplicationData.Current.LocalSettings;
                    localSettings.Values[Constants.BackgroundCommunicatieDownloadedItem] = download.ResultFile.Name;

                    //Start background task to sed new item to pebble
                    Pebble_Time_Manager.Connector.PebbleConnector _pc = Pebble_Time_Manager.Connector.PebbleConnector.GetInstance();
                    try
                    {
                        await _pc.StartBackgroundTask(Connector.PebbleConnector.Initiator.AddItem);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }

                    //Add new item to viewmodel
                    WatchItems.WatchItem _newItem;
                    _newItem = await WatchItems.WatchItem.Load(download.ResultFile.Name);
                    await _pc.WatchItems.AddWatchItem((WatchItems.WatchItem)_newItem);
                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Canceled: " + download.Guid);
            }
        }

        private async Task HandleDownloadImageAsync(DownloadOperation download, bool start)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Running image: " + download.Guid);

                // Store the download so we can pause/resume.
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadImageProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token, progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();

                System.Diagnostics.Debug.WriteLine(String.Format("Completed image: {0}, Status Code: {1}", download.Guid, response.StatusCode));
                // grDownload.Visibility = Visibility.Collapsed;

                if (response.StatusCode == 200)
                {
                    //image loaded
                    System.Diagnostics.Debug.WriteLine("Downloaded file: " + download.ResultFile.Name);

                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Canceled: " + download.Guid);                
            }
        }

        private void DownloadImageProgress(DownloadOperation download)
        {

        }

            /// <summary>
        /// Update the progress windows
        /// </summary>
        /// <param name="download"></param>
        private void DownloadProgress(DownloadOperation download)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Progress: {0}, Status: {1}", download.Guid,
                download.Progress.Status));

            double percent = 100;
            if (download.Progress.TotalBytesToReceive > 0)
            {
                percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
            }

            System.Diagnostics.Debug.WriteLine(String.Format(" - Transfered bytes: {0} of {1}, {2}%",
                download.Progress.BytesReceived, download.Progress.TotalBytesToReceive, percent));

            pbDownload.Value = (int)percent;

            if (download.Progress.HasRestarted)
            {
                System.Diagnostics.Debug.WriteLine(" - Download restarted");
            }

            if (download.Progress.HasResponseChanged)
            {
                // We've received new response headers from the server.
                System.Diagnostics.Debug.WriteLine(" - Response updated; Header count: " + download.GetResponseInformation().Headers.Count);

                // If you want to stream the response data this is a good time to start.
                // download.GetResultStreamAt(0);
            }
        }

        /// <summary>
        /// Cancel the download and/or close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            grDownload.Visibility = Visibility.Collapsed;

            btnCancel.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }
            }

            btnStop.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Visible;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            vmBinder _vm = vmBinder.GetInstance();

            _vm.Commands.SearchStore = true;
            _vm.Commands.FaceStore = true;
            _vm.Commands.AppStore = true;
            _vm.Store.DownloadAvailable = false;

            _vm.Store.StartDownload += Store_StartDownload;
        }

        private void Store_StartDownload(object sender, EventArgs e)
        {
            btnDownload_Click(sender, null);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            vmBinder _vm = vmBinder.GetInstance();

            _vm.Commands.SearchStore = false;
            _vm.Commands.FaceStore = false;
            _vm.Commands.AppStore = false;
            _vm.Store.DownloadAvailable = false;
        }
    }
}
