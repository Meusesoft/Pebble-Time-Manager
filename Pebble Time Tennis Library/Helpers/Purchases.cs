using System;
using Windows.ApplicationModel.Store;
using System.Collections.Generic;
using System.Text;

namespace Tennis_Statistics.Helpers
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
            }

            return _StaticReference;
        }

        /// <summary>
        /// Returns true if the requested feature is available
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static bool Available(String ID)
        {
            if (ID == "SHOTS") return Purchases.getReference().Available(1);
            if (ID == "SERVICE") return Purchases.getReference().Available(1);
            if (ID == "RETURN") return Purchases.getReference().Available(1);
            if (ID == "HASHTAG") return Purchases.getReference().Available(2);

            return true;
        }

        /// <summary>
        /// Returns true if the requested feature is available
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool Available(int ID)
        {
            return (AvailableItems & ID ) == ID;
        }

        /// <summary>
        /// Unlock the requested feature
        /// </summary>
        /// <param name="ID"></param>
        public void Unlock(int ID)
        {
            if (!Available(ID))
            {
                AvailableItems |= ID;
            }
        }

        /// <summary>
        /// Returns true if the purchase is successful
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static bool Purchase(int ID)
        {
            getReference().Unlock(ID);
            return true;
        }

        /// <summary>
        /// Returns the list of all purchaseable items
        /// </summary>
        /// <returns></returns>
        public static List<PurchaseableItem> PurchaseableItems()
        {
            List<PurchaseableItem> Items = new List<PurchaseableItem>();

            Items.Add(new PurchaseableItem { ID = 1, Name = "Advanced statistics", Description = "Unlock the advanced statistics tabs (service games, return games and shots).", Price = 1.99 });
            Items.Add(new PurchaseableItem { ID = 2, Name = "Remove hashtag", Description = "Don't add the #xxx hashtag when shareing to Twitter and Facebook.", Price = 0.99 });
            Items.Add(new PurchaseableItem { ID = 4, Name = "Multiple devices", Description = "Access your match statistics across multiple devices.", Price = 1.99 });
            Items.Add(new PurchaseableItem { ID = 255, Name = "All items", Description = "Unlock all features of this app.", Price = 3.99 });

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
        public int ID { get; set; }
        
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
        public double Price { get; set; }

        #endregion    
    }
}
