using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Storage;


namespace Pebble_Time_Manager.Helper
{
    public class Purchases
    {
        #region Constructor

        public Purchases()
        {
    
        }

        #endregion

        #region Fields

        public static Purchases _StaticReference;
        int AvailableItems;

        #endregion

        #region Methods

        /// <summary>
        /// Get the reference to the instance of the purchases class 
        /// </summary>
        /// <returns></returns>
        public static Purchases getReference()
        {
            if (Purchases._StaticReference == null)
            {
                Purchases._StaticReference = new Purchases();
                 _StaticReference.Initialise().Wait();
            }

            return _StaticReference;
        }

        private async Task Initialise()
        {
            try
            {
#if DEBUG
               // StorageFile _resource = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/WindowsStoreProxy.xml"));
               // await CurrentAppSimulator.ReloadSimulatorAsync(_resource);

                var licenseInformation = CurrentAppSimulator.LicenseInformation;
#else
                var licenseInformation = CurrentApp.LicenseInformation;
#endif
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Purchases.Initialise" + e.Message);
            }
        }

        /// <summary>
        /// Returns true if the requested feature is available
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool Available(String ID)
        {
           // return true;
            
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.Keys.Contains(ID))
                {
                    bool Result = ((bool)localSettings.Values[ID]);
                    return Result;
                }
                else
                {
#if DEBUG
                    ProductLicense license = CurrentAppSimulator.LicenseInformation.ProductLicenses[ID];
#else
                    ProductLicense license = CurrentApp.LicenseInformation.ProductLicenses[ID];
#endif
                    if (license.IsActive) Unlock(ID);

                    return license.IsActive;
                }           
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Purchases: " + e.Message);
            }

            return false;
        }

        /// <summary>
        /// Unlock the requested feature
        /// </summary>
        /// <param name="ID"></param>
        public void Unlock(string ID)
        {
            System.Diagnostics.Debug.WriteLine("Unlock: " + ID);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.Keys.Contains(ID))
            {
                localSettings.Values[ID] = true;
            }
            else
            {
                localSettings.Values.Add(ID, true);
            }
        }

        /// <summary>
        /// Unlock the requested feature
        /// </summary>
        /// <param name="ID"></param>
        public void Lock(string ID)
        {
            System.Diagnostics.Debug.WriteLine("Lock: " + ID);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.Keys.Contains(ID))
            {
                localSettings.Values[ID] = false;
            }
        }

        /// <summary>
        /// Unlock the requested feature
        /// </summary>
        /// <param name="ID"></param>
        public void TryUse(string ID)
        {
            String IDTry = "Try" + ID;
            System.Diagnostics.Debug.WriteLine("Try: " + IDTry);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.Keys.Contains(IDTry))
            {
                int i = (int)localSettings.Values[IDTry];
                i--;
                i = Math.Max(i, 0);
                localSettings.Values[IDTry] = i;
            }
            else
            {
                localSettings.Values[IDTry] = 3;
            }
        }

        /// <summary>
        /// Unlock the requested feature
        /// </summary>
        /// <param name="ID"></param>
        public int TryAvailable(string ID)
        {
            String IDTry = "Try" + ID;
            System.Diagnostics.Debug.WriteLine("TryAvailable: " + IDTry);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.Keys.Contains(IDTry))
            {
                return (int)localSettings.Values[IDTry];
            }
            else
            {
                return 3;
            }
        }

        /// <summary>
        /// Unlock the requested feature
        /// </summary>
        /// <param name="ID"></param>
        public void ClearTryAvailable(string ID)
        {
            String IDTry = "Try" + ID;
            System.Diagnostics.Debug.WriteLine("TryAvailable: " + IDTry);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[IDTry] = 3;
        }

        /// <summary>
        /// Returns true if the purchase is successful
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task<bool> Purchase(string ID)
        {
            try
            {
            #if DEBUG
                ProductLicense license = CurrentAppSimulator.LicenseInformation.ProductLicenses[ID];

                if (!license.IsActive)
                {
                    PurchaseResults Results = await CurrentAppSimulator.RequestProductPurchaseAsync(ID);

                    if (Results.Status == ProductPurchaseStatus.Succeeded)
                    {
                        Unlock(ID);        
                    }
                }
            #else
                ProductLicense license = CurrentApp.LicenseInformation.ProductLicenses[ID];

                if (!license.IsActive)
                {
                    PurchaseResults Results = await CurrentApp.RequestProductPurchaseAsync(ID);

                    if (Results.Status == ProductPurchaseStatus.Succeeded)
                    {
                        Unlock(ID);
                    }
                }
            #endif
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Purchases: " + e.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the list of all purchaseable items
        /// </summary>
        /// <returns></returns>
        public async Task<List<PurchaseableItem>> PurchaseableItems()
        {
            // First, retrieve the list of some products by their IDs.
        #if DEBUG
            ListingInformation listings = await CurrentAppSimulator.LoadListingInformationByProductIdsAsync(new string[] { "pebble_notifications" });
        #else
            ListingInformation listings = await CurrentApp.LoadListingInformationByProductIdsAsync(new string[] { "pebble_notifications" });
        #endif

            List<PurchaseableItem> Items = new List<PurchaseableItem>();

            foreach (var item in listings.ProductListings.Values)
            {
                Items.Add(new PurchaseableItem { ID = item.ProductId , Name = item.Name, Description = item.Description, Price = item.FormattedPrice });
            }

            return Items;
        }

        #endregion
    }

    public class PurchaseableItem
    {
        #region Properties

        /// <summary>
        /// The unique identifier
        /// </summary>
        public string ID { get; set; }
        
        /// <summary>
        /// The name / short description of this item
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The long / description of this item
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// The price to purchase this item
        /// </summary>
        public string Price { get; set; }

        #endregion    
    }
}
