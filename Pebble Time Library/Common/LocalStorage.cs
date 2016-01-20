using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Pebble_Time_Manager.Common
{
    public class LocalStorage
    {
        /// <summary>
        /// Store the content to the local storage of this device
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="Filename"></param>
        public static async Task Save(String Content, String Filename, bool GenerateUniqueName)
        {
            try
            {
                Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                Windows.Storage.StorageFile File = await LocalFolder.CreateFileAsync(Filename, GenerateUniqueName ? Windows.Storage.CreationCollisionOption.GenerateUniqueName : CreationCollisionOption.ReplaceExisting);

                await Windows.Storage.FileIO.WriteTextAsync(File, Content);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);            
            }
        }

        /// <summary>
        /// Store the contents of the stream to the local storage of this device
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="Filename"></param>
        /// <param name="GenerateUniqueName"></param>
        /// <returns></returns>
        public static async Task Save(System.IO.Stream _Stream, String Filename, bool GenerateUniqueName)
        {
            try
            {
                Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                Windows.Storage.StorageFile File = await LocalFolder.CreateFileAsync(Filename, GenerateUniqueName ? Windows.Storage.CreationCollisionOption.GenerateUniqueName : CreationCollisionOption.ReplaceExisting);


            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }


        /// <summary>
        /// Load the contents of the requested file
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public static async Task<String> Load(String Filename)
        {
            Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile File = await LocalFolder.GetFileAsync(Filename);

                String Content = await FileIO.ReadTextAsync(File);

                return Content;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return "";
            }
        }

        /// <summary>
        /// Get the IStorageFile reference for the requested file
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public static async Task<IStorageFile> GetFile(String Filename)
        {
            Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile File = await LocalFolder.GetFileAsync(Filename);

                return File;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Copy file to destination
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Destination"></param>
        /// <returns></returns>
        public static async Task<IStorageFile> Copy(String Source, String Destination)
        {
            Windows.Storage.IStorageFile File = await GetFile(Source);

            if (File == null) return null;

            Windows.Storage.IStorageFile CopiedFile = await File.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder, Destination, NameCollisionOption.ReplaceExisting);

            return CopiedFile;
        }

        /// <summary>
        /// Store the content to the local storage of this device
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="Filename"></param>
        public static async Task Delete(String Filename)
        {
            try
            {
                Windows.Storage.IStorageFile File = await GetFile(Filename);
                if (File != null) await File.DeleteAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Store the content to the local storage of this device
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="Filename"></param>
        public static async Task DeleteAll()
        {
            try
            {
                Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                IReadOnlyList<StorageFile> LocalFiles = await LocalFolder.GetFilesAsync();

                foreach(StorageFile File in LocalFiles)
                {
                    await File.DeleteAsync();
                 }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Return the list of files in the local folder
        /// </summary>
        /// <returns></returns>
        public static async Task<IReadOnlyList<IStorageFile>> Files()
        {
            Windows.Storage.StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {
                IReadOnlyList<IStorageFile> Files = await LocalFolder.GetFilesAsync();

                return Files;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return null;
            }
        }

    }
}
