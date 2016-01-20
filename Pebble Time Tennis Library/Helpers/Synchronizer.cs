using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using System.Threading.Tasks;
using Tennis_Statistics.Game_Logic;
using Tennis_Statistics.ViewModels;

namespace Tennis_Statistics.Helpers
{
    // A delegate type for hooking up change notifications.
    public delegate void SynchronizedEventHandler(object sender, EventArgs e);

    public class Synchronizer
    {
        #region Properties

        public Tennis_Statistics.ViewModels.vmMatches vmMatches;

        //The last synchronization
        public DateTime LastSynchronization
        {
            get
            {
                //Retrieve last synchronization
                DateTime _LastSynchronization = DateTime.MinValue;
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("LastSynchronization"))
                {
                    try
                    {
                        _LastSynchronization = Convert.ToDateTime(ApplicationData.Current.LocalSettings.Values["LastSynchronization"]);
                    }
                    catch (Exception e)
                    {
                        _LastSynchronization = DateTime.MinValue;
                    }
                }

                return _LastSynchronization;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute the synchronization of files between the local and external storage
        /// </summary>
        /// <returns></returns>
        public async Task Execute()
        {
            await SyncFiles();

            //Fire the event that synchronization is complete
            OnSynchronizationComplete(EventArgs.Empty);
        }
            
        /// <summary>
        /// Synchronize the files between the OneDrive and the local files
        /// </summary>
        /// <returns></returns>
        public async Task<List<String>> SyncFiles()
        {
            List<String> Result = new List<string>(0);

            try
            {

                //Check if network is available
                bool isConnected = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                if (!isConnected) return Result;


                List<IDictionary<string, object>> Files = await OneDrive.FilesAsync("TennisData");
                IReadOnlyList<IStorageFile> LocalFiles = await LocalStorage.Files();
                List<Task> Queue = new List<Task>(0);


                if (Files != null) //When Files==null no network access
                {

                    //Retrieve last synchronization
                    DateTime LastSynchronization = DateTime.MinValue;
                    if (ApplicationData.Current.LocalSettings.Values.ContainsKey("LastSynchronization"))
                    {
                        try
                        {
                            LastSynchronization = Convert.ToDateTime(ApplicationData.Current.LocalSettings.Values["LastSynchronization"]);
                        }
                        catch (Exception e)
                        {
                            LastSynchronization = DateTime.MinValue;
                        }
                    }

                    //Download files from onedrive if not present on local storage
                    bool Download = false;

                    foreach (var File in Files)
                    {
                        DateTime parsedDateTime;
                        DateTime.TryParse(File["updated_time"].ToString(), null, System.Globalization.DateTimeStyles.None, out parsedDateTime);

                        if (File["type"].ToString() == "file")
                        {
                            Download = true;

                            //Check if file does not exist in the local files
                            foreach (IStorageFile LocalFile in LocalFiles)
                            {
                                if (File["name"].ToString() == LocalFile.Name)
                                {
                                    Download = false;

                                    //If it does, check if file is newer than last synchronization, download it
                                    var UpdatedTimeString = File["updated_time"];
                                    DateTime UpdatedTime = Convert.ToDateTime(UpdatedTimeString);

                                    if ((DateTime)UpdatedTime >= LastSynchronization)
                                    {
                                        //await DeleteFileAsync(LocalFile.Name);
                                        Download = true;
                                    }

                                    break;
                                }
                            }

                            //Download the file to the local files (if necessary)
                            if (Download)
                            {
                                Task newDownload = DownloadFileAsync(File);

                                Queue.Add(newDownload);
                                Result.Add(File["name"].ToString());
                            }
                        }
                    }

                    //Remove files from local storage if not available on external storage
                    bool Found = false;

                    foreach (IStorageFile LocalFile in LocalFiles)
                    {
                        Found = false;

                        foreach (var File in Files)
                        {
                            if (File["name"].ToString() == LocalFile.Name)
                            {
                                Found = true;
                                break;
                            }
                        }

                        if (!Found)
                        {
                            if (LocalFile.Name.StartsWith("match_"))
                            {
                                Task newDeletion = DeleteFileAsync(LocalFile.Name);

                                Queue.Add(newDeletion);
                            }
                        }
                    }

                    //Wait until all tasks in queue are finished
                    bool bBusy = true;

                    while (bBusy)
                    {
                        bBusy = false;

                        foreach (Task _task in Queue)
                        {
                            if (!_task.IsCompleted)
                            {
                                bBusy = true;
                            }
                        }

                        if (bBusy) await Task.Delay(100);
                    }

                    //Save setting of last synchronization
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["LastSynchronization"] = DateTime.Now.ToString();

                    LocalFiles = await LocalStorage.Files();

                    foreach (IStorageFile LocalFile in LocalFiles)
                    {
                        Result.Add(LocalFile.Name);
                    }
                }
            }
            catch
            {

            }

            return Result;
        }

        /// <summary>
        /// Download the file and add it as a match
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        private async Task DownloadFileAsync(IDictionary<string, object> File)
        {
            await OneDrive.DownloadAsync(File);

            vmMatch _vmMatch = new vmMatch();
            await _vmMatch.Load(File["name"].ToString());

            vmMatches.StoreMatch(_vmMatch);
        }

        /// <summary>
        /// Delete the file from local storage
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        private async Task DeleteFileAsync(String Filename)
        {
            String GUID = Filename;
            GUID = GUID.Substring(6, GUID.Length - 10); //remove match_ and .xml
            
            System.Guid _MatchID = new Guid(GUID);

            vmMatches.RemoveMatch(_MatchID);
        }
        #endregion

        #region Events

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event SynchronizedEventHandler SynchronizationComplete;

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnSynchronizationComplete(EventArgs e)
        {
            if (SynchronizationComplete != null)
                SynchronizationComplete(this, e);
        }

        #endregion

    }
}
