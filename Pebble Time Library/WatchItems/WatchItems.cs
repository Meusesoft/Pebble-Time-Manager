using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Collections.Specialized;
using Pebble_Time_Manager.Common;
using Pebble_Time_Library.Common;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net;
using Windows.Data.Json;
using Windows.Networking.BackgroundTransfer;
using System.Net.Http;
using System.Threading;

namespace Pebble_Time_Manager.WatchItems
{

    [KnownType(typeof(WatchApp))]
    [KnownType(typeof(WatchFace))]
    [CollectionDataContract]
    public class WatchItems : List<WatchItem>
    {

        #region Events

        //Event handler for item list change
        public delegate void ItemListEventHandler(object sender, NotifyCollectionChangedEventArgs e);

        public event ItemListEventHandler WatchItemListChanged;

        protected virtual void OnItemListChange(NotifyCollectionChangedEventArgs e)
        {
            if (WatchItemListChanged != null) WatchItemListChanged(this, e);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Add new watch item
        /// </summary>
        /// <param name="_newItem"></param>
        public async Task AddWatchItem(WatchItem _newItem)
        {
            NotifyCollectionChangedEventArgs ea;

            await DeleteWatchItem(_newItem);

            //Add the new watch face
            Add(_newItem);

            //Fire event
            ea = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _newItem);
            OnItemListChange(ea);

            await Save();
        }

        /// <summary>
        /// Delete new watch item from collection
        /// </summary>
        /// <param name="_newItem"></param>
        public async Task DeleteWatchItem(WatchItem _delItem)
        {
            NotifyCollectionChangedEventArgs ea;

            //If watchface with same ID exist, remove it
            var Items = this.Where(x => x.ID == _delItem.ID);

            foreach (var Item in Items)
            {
                ea = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _delItem);
                OnItemListChange(ea);
            }

            this.RemoveAll(x => x.ID == _delItem.ID);

            await Save();
        }

        /// <summary>
        /// Save the watch items to local storage
        /// </summary>
        public async Task Save()
        {
            try
            {
                String List = Common.Serializer.Serialize(this);
                await Common.LocalStorage.Save(List, "watchitems.xml", false);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("WatchItems.Save exception: {0}", e.Message));
            }
        }

        /// <summary>
        /// Get the WatchItem defined by its GUID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public WatchItem Get(Guid ID)
        {
            var Item = this.FirstOrDefault(x => x.ID == ID);

            return Item;
        }



        /// <summary>
        /// Load the watch items from local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Load()
        {
            try
            {
                WatchItems _watchItems;

                String List = await Common.LocalStorage.Load("watchitems.xml");

                if (List.Length == 0)
                {
                    //try backup
                    System.Diagnostics.Debug.WriteLine("Try backup watchitems.xml");
                    List = await Common.LocalStorage.Load("watchitems.bak");
                }

                if (List.Length > 0)
                {
                    //Make backup of xml storage file
                    Common.LocalStorage.Copy("watchtems.xml", "watchitems.bak");

                    _watchItems = (WatchItems)Common.Serializer.Deserialize(List, typeof(WatchItems));

                    //Check if Tennis app is available

                    //process all watch items
                    if (_watchItems != null)
                    {
                        this.Clear();

                        foreach (var item in _watchItems)
                        {
                            this.Add(item);
                        }
                    }
                    //else return false;
                }

#if WINDOWS_UWP
                if (this.Find(x => x.ID == Guid.Parse(Constants.TennisAppGuid)) == null)
                {
                    //WatchItem newitem = await WatchItem.Load("ms-appx:///Assets/Tennis.pbw");
                    //Add(newitem);      

                    WatchItem newitem = new WatchItem();
                    newitem.Name = "Tennis";
                    newitem.Developer = "Meusesoft";
                    newitem.ID = Guid.Parse(Constants.TennisAppGuid);
                    newitem.File = "ms-appx:///Assets/tennis_icon.png";
                    newitem.Flags = 8;
                    newitem.SDKVersionMajor = 5;
                    newitem.SDKVersionMinor = 72;
                    newitem.VersionMajor = 1;
                    newitem.VersionMinor = 0;
                    newitem.Type = WatchItemType.WatchApp;

                    this.Add(newitem);

                    await Save();
                }
#endif

#if WINDOWS_PHONE_APP
                WatchItem TennisApp = this.Find(x => x.ID == Guid.Parse(Constants.TennisAppGuid));
                if (TennisApp != null)
                {
                    this.Remove(TennisApp);
                }
#endif


                NotifyCollectionChangedEventArgs ea = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnItemListChange(ea);

                //Trigger event
                List<IWatchItem> _list = new List<IWatchItem>();
                foreach (var item in this)
                {
                    _list.Add(item);

                    NotifyCollectionChangedEventArgs eai = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
                    OnItemListChange(eai);
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("WatchItems.Load exception: {0}", e.Message));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Backup the watch items to OneDrive
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Backup()
        {
            try
            {
                string CompressedFile = "pebble_time_manager_backup.zip";

                // Retrieve files to compress
                IReadOnlyList<IStorageFile> filesToCompress = await LocalStorage.Files();

                // Created new file to store compressed files
                //This will create a file under the selected folder in the name   “Compressed.zip”

                Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile zipFile = await LocalFolder.CreateFileAsync(CompressedFile, CreationCollisionOption.ReplaceExisting);

                using (MemoryStream zipMemoryStream = new MemoryStream())
                {
                    // Create zip archive
                    using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create))
                    {
                        // For each file to compress...
                        foreach (StorageFile fileToCompress in filesToCompress)
                        {
                            if (fileToCompress.Name != CompressedFile)
                            {
                                //Read the contents of the file
                                byte[] buffer = WindowsRuntimeBufferExtensions.ToArray(await FileIO.ReadBufferAsync(fileToCompress));
                                // Create a zip archive entry
                                ZipArchiveEntry entry = zipArchive.CreateEntry(fileToCompress.Name);
                                // And write the contents to it
                                using (Stream entryStream = entry.Open())
                                {
                                    await entryStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                            }
                        }
                    }

                    using (IRandomAccessStream zipStream = await zipFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        // Write compressed data from memory to file
                        using (Stream outstream = zipStream.AsStreamForWrite())
                        {
                            byte[] buffer = zipMemoryStream.ToArray();
                            outstream.Write(buffer, 0, buffer.Length);
                            outstream.Flush();
                        }
                    }
                }

                //Backup to OneDrive
                await OneDrive.UploadFileAsync("Backup", CompressedFile);
                System.Diagnostics.Debug.WriteLine("Backup: " + CompressedFile);

                //Remove zip file
                await LocalStorage.Delete(CompressedFile);

            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine("Backup exception: " + exp.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Restore the watch items from OneDrive
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Restore()
        {
            try
            {
                string CompressedFile = "pebble_time_manager_backup.zip";

                //Get backup file
                await OneDrive.DownloadAsync("Backup", CompressedFile);

                Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Stream FileStream = await LocalFolder.OpenStreamForReadAsync(CompressedFile);

                using (ZipArchive zipArchive = new ZipArchive(FileStream, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    {
                        using (Stream entryStream = entry.Open())
                        {
                            byte[] buffer = new byte[entry.Length];
                            entryStream.Read(buffer, 0, buffer.Length);
                            // Create a file to store the contents 
                            StorageFile uncompressedFile = await LocalFolder.CreateFileAsync(entry.Name, CreationCollisionOption.ReplaceExisting);

                            // Store the contents 
                            using (IRandomAccessStream uncompressedFileStream =
                            await uncompressedFile.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                using (Stream outstream = uncompressedFileStream.AsStreamForWrite())
                                {
                                    outstream.Write(buffer, 0, buffer.Length);
                                    outstream.Flush();
                                }
                            }

                            System.Diagnostics.Debug.WriteLine("Restore exception: " + entry.Name);
                        }
                    }
                }

            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine("Restore exception: " + exp.Message);
                return false;
            }

            return true;
        }


        #endregion

        #region Download

        public enum DownloadState { Initiate, InProgress, Done, Error, Canceled};

        public class DownloadEventArgs : System.EventArgs
        {
            public DownloadEventArgs(String Status, DownloadState State)
            {
                this.Status = Status;
                this.State = State;
            }

            public DownloadEventArgs(String Status, DownloadState State, WatchItem WatchItem)
            {
                this.Status = Status;
                this.State = State;
                this.WatchItem = WatchItem;
            }

            public String Status{ get; set; }
            public DownloadState State { get; set; }
            public WatchItem WatchItem { get; set; }
        }

        public delegate void DownloadEventHandler(object sender, DownloadEventArgs e);
        public static event DownloadEventHandler OnDownloadEvent;
        public static CancellationTokenSource cts;
        private static WatchItem newWatchItem;

        public static async void Download(String PackageID)
        {
            try
            {
                cts = new CancellationTokenSource();

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

                System.Diagnostics.Debug.WriteLine($"{Type} {Title} {File}");

                //Initiate download
                HttpClient _hc = new HttpClient();

                HttpResponseMessage response = await _hc.GetAsync(File);
                response.EnsureSuccessStatusCode();

                Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                BackgroundDownloader downloader = new BackgroundDownloader();

                //Show download progress window
                OnDownloadEvent?.Invoke(PackageID, new DownloadEventArgs(Title, DownloadState.Initiate));

                //Download image
                downloader = new BackgroundDownloader();
                Windows.Storage.StorageFile destinationFile = await LocalFolder.CreateFileAsync(PackageID + ".gif", CreationCollisionOption.ReplaceExisting);
                DownloadOperation download = downloader.CreateDownload(new Uri(ListImage), destinationFile);

                await HandleDownloadImageAsync(download, true);

                //Download binary
                System.Diagnostics.Debug.WriteLine($"Download binary: {File}");
                destinationFile = await LocalFolder.CreateFileAsync(PackageID + ".zip", CreationCollisionOption.ReplaceExisting);
                download = downloader.CreateDownload(new Uri(File), destinationFile);

                //Create watchitem instance    
                newWatchItem = new WatchItem();
                newWatchItem.File = PackageID + ".zip";
                newWatchItem.ID = new Guid(Uuid);
                newWatchItem.Name = Title;

                await HandleDownloadAsync(download, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);

                OnDownloadEvent?.Invoke(null, new DownloadEventArgs(ex.Message, DownloadState.Error));
                cts = null;
            }
        }

        private static async Task HandleDownloadAsync(DownloadOperation download, bool start)
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

                if (response.StatusCode == 200)
                {
                    System.Diagnostics.Debug.WriteLine("Downloaded file: " + download.ResultFile.Name);

                    OnDownloadEvent?.Invoke(null, new DownloadEventArgs(download.ResultFile.Name, DownloadState.Done, newWatchItem));

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
                    WatchItem _newItem;
                    _newItem = await WatchItem.Load(download.ResultFile.Name);
                    await _pc.WatchItems.AddWatchItem((WatchItem)_newItem);

                    cts = null;
                }
                else
                {
                    OnDownloadEvent?.Invoke(null, new DownloadEventArgs("An error occurred while downloading", DownloadState.Error));
                    cts = null;
                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Canceled: " + download.Guid);
                OnDownloadEvent?.Invoke(null, new DownloadEventArgs(download.ResultFile.Name, DownloadState.Canceled));
                cts = null;
            }
        }

        private static async Task HandleDownloadImageAsync(DownloadOperation download, bool start)
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
                OnDownloadEvent?.Invoke(null, new DownloadEventArgs(download.ResultFile.Name, DownloadState.Canceled));
                cts = null;
            }
        }

        private static void DownloadImageProgress(DownloadOperation download)
        {

        }

        /// <summary>
        /// Update the progress windows
        /// </summary>
        /// <param name="download"></param>
        private static void DownloadProgress(DownloadOperation download)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Progress: {0}, Status: {1}", download.Guid, download.Progress.Status));

            double percent = 100;
            if (download.Progress.TotalBytesToReceive > 0)
            {
                percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
            }

            System.Diagnostics.Debug.WriteLine(String.Format(" - Transfered bytes: {0} of {1}, {2}%", download.Progress.BytesReceived, download.Progress.TotalBytesToReceive, percent));

            OnDownloadEvent?.Invoke(null, new DownloadEventArgs($"{(int)percent}", DownloadState.InProgress));

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


        #endregion
    }
}
