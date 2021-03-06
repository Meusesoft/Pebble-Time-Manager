﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using P3bble.Helper;
using SharpCompress.Archive;
using SharpCompress.Archive.Zip;
using Windows.Storage;
using Windows.Data.Json;

namespace P3bble.Types
{
    /// <summary>
    /// The bundle type
    /// </summary>
    public enum BundleType
    {
        /// <summary>
        /// Application bundle
        /// </summary>
        Application,

        /// <summary>
        /// Firmware bundle
        /// </summary>
        Firmware
    }

    public enum PebbleDeviceType
    {
        Original,
        Aplite,
        Basalt,
        Chalk        
    }

    /// <summary>
    /// Represents an app this.Bundle
    /// <remarks>STRUCT_DEFINITION in pebble.py</remarks>
    /// </summary>
    public class Bundle
    {
        private string _path;
        private IArchive _bundle;

        /// <summary>
        /// Create a new Pebblethis.Bundle from a .pwb file and parse its metadata.
        /// </summary>
        /// <param name="path">The relative or full path to the file.</param>
        internal Bundle(string path)
        {
            this._path = path;
            this.PebbleDeviceType = PebbleDeviceType.Basalt;
        }

        /// <summary>
        /// Gets the type of the bundle.
        /// </summary>
        /// <value>
        /// The type of the bundle.
        /// </value>
        public BundleType BundleType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the bundle has resources.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the bundle has resources.
        /// </value>
        public bool HasResources { get; private set; }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public string Filename
        {
            get
            {
                return Path.GetFileName(this.FullPath);
            }
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>
        /// The full path.
        /// </value>
        public string FullPath { get; private set; }

        /// <summary>
        /// Gets the application details.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public ApplicationMetadata Application { get; private set; }

        public BundleAppinfo AppInfo { get; private set; }

        internal byte[] BinaryContent { get; private set; }

        internal byte[] Resources { get; private set; }

        internal BundleManifest Manifest { get; private set; }

        public String Javascript { get; private set; }

        public PebbleDeviceType PebbleDeviceType {get; set;}

        /// <summary>
        /// Loads a bundle from ApplicationData.Current.LocalFolder.
        /// </summary>
        /// <param name="name">The name of the file in ApplicationData.Current.LocalFolder.</param>
        /// <returns>A bundle</returns>
        public static async Task<Bundle> LoadFromLocalStorageAsync(string name)
        {
            try
            {
                var bundle = new Bundle(name);
                await bundle.Initialise();
                return bundle;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("LoadFromLocalStorageAsync: {0}", e.Message));
            }

            return null;
        }

        public static async Task<Bundle> LoadFromLocalStorageAsync(string name, PebbleDeviceType PebbleDeviceType)
        {
            try
            {
                var bundle = new Bundle(name);
                bundle.PebbleDeviceType = PebbleDeviceType;
                await bundle.Initialise();
                return bundle;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("LoadFromLocalStorageAsync: {0}", e.Message));
            }

            return null;
        }

        /// <summary>
        /// Deletes the bundle from storage.
        /// </summary>
        /// <returns>An async task to await</returns>
        public async Task DeleteFromStorage()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(this._path);
            if (file != null)
            {
                await file.DeleteAsync();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.BundleType == BundleType.Application)
            {
                string format = "{0} containing watch app {1}";
                return string.Format(format, this.Filename, this.Application);
            }
            else
            {
                // This is pretty ugly, but will do for now.
                string format = "{0} containing fw version {1} for hw rev {2}";
                return string.Format(format, this.Filename, this.Manifest.Resources.Version, this.Manifest.Firmware.HardwareRevision);
            }
        }

        internal async Task Initialise()
        {
            PebbleDeviceType BundlePebbleDeviceType = PebbleDeviceType;

            //Get the app from the local storage

            if (this._path.Contains("ms-appx:"))
            {
                StorageFile _resource = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/Tennis.pbw"));
                await _resource.CopyAsync(ApplicationData.Current.LocalFolder, "temp.zip", NameCollisionOption.ReplaceExisting);

                this._path = "temp.zip";
            }

            //StorageFile _resource2 = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/tennis.pbw"));
            // await _resource2.CopyAsync(ApplicationData.Current.LocalFolder, "temp.zip", NameCollisionOption.ReplaceExisting);

            // this._path = "temp.zip";

            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(this._path);
            if (file == null)
            {
                throw new FileNotFoundException("the file could not be found in the isolated storage");
            }

            this.FullPath = file.Path;
            this._bundle = ZipArchive.Open(await file.OpenStreamForReadAsync());

            bool Basalt = true;

            //Load appinfo
            var appinfoEntry = this._bundle.Entries.Where(e => string.Compare(e.FilePath, "appinfo.json", StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (appinfoEntry != null)
            {
                using (Stream jsonstream = appinfoEntry.OpenEntryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof(BundleAppinfo));
                    AppInfo = serializer.ReadObject(jsonstream) as BundleAppinfo;
                }

                using (Stream jsonstream = appinfoEntry.OpenEntryStream())
                {
                    using (StreamReader reader = new StreamReader(jsonstream))
                    {
                        string contents = reader.ReadToEnd();
                        JsonObject A = JsonObject.Parse(contents);

                        foreach (var item in A)
                        {
                            if (item.Key == "appKeys")
                            {
                                AppInfo.AppKeys = new System.Collections.Generic.Dictionary<string, int>();

                                JsonObject B = JsonObject.Parse(item.Value.Stringify());

                                foreach (var item2 in B)
                                {
                                    AppInfo.AppKeys.Add(item2.Key, Convert.ToInt32(item2.Value.GetNumber()));
                                }
                            }
                        }
                    }
                }
            }

            //Load manifest
            String ManifestPath = "";

            switch (PebbleDeviceType)
            {
                case PebbleDeviceType.Aplite: ManifestPath = "aplite/manifest.json"; break;
                case PebbleDeviceType.Basalt: ManifestPath = "basalt/manifest.json"; break;
                case PebbleDeviceType.Chalk:  ManifestPath = "chalk/manifest.json";  break;
            }

            var manifestEntry = this._bundle.Entries.Where(e => string.Compare(e.FilePath, ManifestPath, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();

            if (manifestEntry == null)
            {
                manifestEntry = this._bundle.Entries.Where(e => string.Compare(e.FilePath, "manifest.json", StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                BundlePebbleDeviceType = PebbleDeviceType.Original;
                if (manifestEntry == null)
                {
                    throw new ArgumentException("manifest.json not found in archive - not a Pebble Bundle.");
                }
            }

            using (Stream jsonstream = manifestEntry.OpenEntryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(BundleManifest));
                this.Manifest = serializer.ReadObject(jsonstream) as BundleManifest;
            }

            //Load binary content
            String BinaryPath = "";

            if (this.Manifest.Type == "firmware")
            {
                this.BundleType = BundleType.Firmware;
                this.BinaryContent = await this.ReadFileToArray(this.Manifest.Firmware.Filename, this.Manifest.Firmware.Size);
            }
            else
            {
                switch (BundlePebbleDeviceType)
                {
                    case PebbleDeviceType.Aplite: BinaryPath = "aplite/"; break;
                    case PebbleDeviceType.Basalt: BinaryPath = "basalt/"; break;
                    case PebbleDeviceType.Chalk: BinaryPath = "chalk/"; break;
                }

                this.BundleType = BundleType.Application;
                this.BinaryContent = await this.ReadFileToArray(BinaryPath + this.Manifest.ApplicationManifest.Filename, this.Manifest.ApplicationManifest.Size);

                // Convert first part to app manifest
#if NETFX_CORE  && !WINDOWS_PHONE_APP
                byte[] buffer = new byte[Marshal.SizeOf<ApplicationMetadata>()];
#else
                byte[] buffer = new byte[Marshal.SizeOf(typeof(ApplicationMetadata))];
#endif
                Array.Copy(this.BinaryContent, 0, buffer, 0, buffer.Length);
                this.Application = buffer.AsStruct<ApplicationMetadata>();
            }

            //Load resources
            this.HasResources = this.Manifest.Resources.Size != 0;
            if (this.HasResources)
            {
                this.Resources = await this.ReadFileToArray(BinaryPath + this.Manifest.Resources.Filename, this.Manifest.Resources.Size);
            }

            //Load javascript
            var javascriptEntry = this._bundle.Entries.Where(e => string.Compare(e.FilePath, "pebble-js-app.js", StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (javascriptEntry != null)
            {
                using (Stream jsonstream = javascriptEntry.OpenEntryStream())
                {
                    using (StreamReader reader = new StreamReader(jsonstream))
                    {
                        Javascript = reader.ReadToEnd();
                    }
                }
            }
        }

        private async Task<byte[]> ReadFileToArray(string file, int size)
        {
           var entry = this._bundle.Entries.Where(e => string.Compare(e.FilePath, file, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();

            if (entry == null)
            {
                string format = "App file {0} not found in archive";
                throw new ArgumentException(string.Format(format, file));
            }

            using (Stream stream = entry.OpenEntryStream())
            {
                byte[] result = new byte[size];
                await stream.ReadAsync(result, 0, result.Length);
                return result;
            }

            return null;
        }
    }
}
