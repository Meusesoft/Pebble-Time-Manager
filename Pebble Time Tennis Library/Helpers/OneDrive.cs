using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Tennis_Statistics.Helpers
{
    public class OneDrive
    {
        #region Fields

        public static LiveConnectClient liveClient;
        
        #endregion

        #region Methods

        /// <summary>
        /// Connect the the live client
        /// </summary>
        /// <returns></returns>
        public static async Task<string> ConnectAsync()
        {
            try
            {
                if (liveClient == null)
                {

                    var authClient = new LiveAuthClient();

                    //  ask for both read and write access to the OneDrive
                    LiveLoginResult result = await authClient.LoginAsync(new string[] { "wl.skydrive", "wl.skydrive_update" });

                    //  if login successful 
                    if (result.Status == LiveConnectSessionStatus.Connected)
                    {
                        //  create a OneDrive client
                        liveClient = new LiveConnectClient(result.Session);
                    }
                }

                return "Success";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        
        /// <summary>
        /// Upload file to OneDrive
        /// </summary>
        /// <param name="Foldername"></param>
        /// <param name="Filename"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public static async Task<int> CreateFileAsync(String Foldername, String Filename, String Content)
        {
            try
            {
                //  create a local file
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(Filename, CreationCollisionOption.ReplaceExisting);

                //  copy content to local file
                System.Text.UTF8Encoding UTF8encoding = new System.Text.UTF8Encoding();
                byte[] ContentBytes = UTF8encoding.GetBytes(Content);

                using (Stream fileStream = await file.OpenStreamForWriteAsync())
                {
                    await fileStream.WriteAsync(ContentBytes, 0, ContentBytes.Length);
                    await fileStream.FlushAsync();
                }

                int result = await UploadFileAsync(Foldername, Filename);

                // delete the local file
                await file.DeleteAsync();

                return result;
            }
            catch (Exception e)
            {
                return e.Message.Length;
            }
        }

        /// <summary>
        /// Upload a file to OneDrive
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public static async Task<int> UploadFileAsync(String Foldername, String Filename)
        {
            try
            {
                //  create OneDrive auth client
                var authClient = new LiveAuthClient();

                //  ask for both read and write access to the OneDrive
                LiveLoginResult result = await authClient.LoginAsync(new string[] { "wl.skydrive", "wl.skydrive_update" });

                //  if login successful 
                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    //  create a OneDrive client
                    liveClient = new LiveConnectClient(result.Session);

                    //  create a local file
                    StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(Filename);

                    //  create a folder
                    string folderID = await GetFolderIDAsync(Foldername, true);

                    if (string.IsNullOrEmpty(folderID))
                    {
                        //  return error
                        return 0;
                    }

                    //  upload local file to OneDrive
                    await liveClient.BackgroundUploadAsync(folderID, file.Name, file, OverwriteOption.Overwrite);

                    return 1;
                }
            }
            catch (Exception e)
            {
                return e.Message.Length;
            }

            //  return error
            return 0;
        }

        /// <summary>
        /// Get the ID of the requested folder
        /// </summary>
        /// <param name="Foldername"></param>
        /// <param name="Create"></param>
        /// <returns></returns>
        public static async Task<string> GetFolderIDAsync(string Foldername, bool Create)
        {
            try
            {
                string queryString = "me/skydrive/files?filter=folders";

                //  get all folders
                LiveOperationResult loResults;
                System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();

                try
                {
                    //Wait for 5 seconds. 
                    var awaitableTask = liveClient.GetAsync(queryString, cts.Token);
                    int i = 50;
                    while (awaitableTask.Status == TaskStatus.Running || awaitableTask.Status == TaskStatus.WaitingForActivation)
                    {
                        await Task.Delay(100);
                        i--;

                        if (i == 0)
                        {
                            cts.Cancel();
                            return String.Empty;
                        }
                    }                    

                    loResults = awaitableTask.Result;
                }
                catch (Exception e)
                {
                    throw new OperationCanceledException(e.Message);
                }

                //LiveOperationResult loResults = await liveClient.GetAsync(queryString);
                dynamic folders = loResults.Result;

                foreach (dynamic folder in folders.data)
                {
                    if (string.Compare(folder.name, Foldername, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //  found our folder
                        return folder.id;
                    }
                }

                //  folder not found
                if (Create)
                { 
                    //  create folder
                    Dictionary<string, object> folderDetails = new Dictionary<string, object>();
                    folderDetails.Add("name", Foldername);
                    loResults = await liveClient.PostAsync("me/skydrive", folderDetails);
                    folders = loResults.Result;

                    // return folder id
                    return folders.id;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return String.Empty;
        }

        /// <summary>
        /// Download a file from onedrive
        /// </summary>
        /// <returns></returns>
        public static async Task<int> DownloadAsync(string Foldername, string Filename)
        {
            try
            {
                string fileID = string.Empty;

                //  get folder ID
                string folderID = await GetFolderIDAsync(Foldername, false);

                if (string.IsNullOrEmpty(folderID))
                {
                    return 0; // doesnt exists
                }

                //  get list of files in this folder
                LiveOperationResult loResults = await liveClient.GetAsync(folderID + "/files");
                List<object> folder = loResults.Result["data"] as List<object>;

                //  search for our file 
                foreach (object fileDetails in folder)
                {
                    IDictionary<string, object> file = fileDetails as IDictionary<string, object>;
                    if (string.Compare(file["name"].ToString(), Filename, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //  found our file
                        fileID = file["id"].ToString();
                        
                        return await DownloadAsync(file); 
                    }
                }

                //No file downloaded
                return 0;            

            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// Download a file from onedrive
        /// </summary>
        /// <returns></returns>
        public static async Task<int> DownloadAsync(IDictionary<string, object> File)
        {
            try
            {
                //  create local file
                StorageFile localFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(File["name"].ToString(), CreationCollisionOption.ReplaceExisting);

                //  download file from OneDrive
                await liveClient.BackgroundDownloadAsync(File["id"].ToString() + "/content", localFile);

                return 1;
            }
            catch (Exception e)
            {
                return e.Message.Length;
            }
            return 0;
        }

        /// <summary>
        /// Download a file from onedrive
        /// </summary>
        /// <returns></returns>
        public static async Task<int> DeleteAsync(string Foldername, string Filename)
        {
            try
            {
                string fileID = string.Empty;

                //  get folder ID
                string folderID = await GetFolderIDAsync(Foldername, false);

                if (string.IsNullOrEmpty(folderID))
                {
                    return 0; // doesnt exists
                }

                //  get list of files in this folder
                LiveOperationResult loResults = await liveClient.GetAsync(folderID + "/files");
                List<object> folder = loResults.Result["data"] as List<object>;

                //  search for our file 
                foreach (object fileDetails in folder)
                {
                    IDictionary<string, object> file = fileDetails as IDictionary<string, object>;
                    if (string.Compare(file["name"].ToString(), Filename, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //  found our file
                        fileID = file["id"].ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(fileID))
                {
                    //  file doesnt exists
                    return 0;
                }

                //  download file from OneDrive
                await liveClient.DeleteAsync(fileID);

                return 1;
            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// Get the list of files in the requested folder
        /// </summary>
        /// <returns></returns>
        public static async Task<List<IDictionary<string, object>>> FilesAsync(string Foldername)
        {
            try
            {
                List<IDictionary<string, object>> Result = null;

                if (await ConnectAsync() == "Success")
                {

                    Result = new List<IDictionary<string, object>>();

                    string fileID = string.Empty;

                    //  get folder ID
                    string folderID = await GetFolderIDAsync(Foldername, false);

                    if (string.IsNullOrEmpty(folderID))
                    {
                        return null; // doesnt exists 
                    }

                    //  get list of files in this folder
                    LiveOperationResult loResults = await liveClient.GetAsync(folderID + "/files");
                    List<object> folder = loResults.Result["data"] as List<object>;

                    //  search for our file 
                    foreach (object fileDetails in folder)
                    {
                        IDictionary<string, object> file = fileDetails as IDictionary<string, object>;
                        Result.Add(file);
                    }
                }

                return Result;
            }
            catch
            {
            }
            return null;
        }
        #endregion
    }
}
