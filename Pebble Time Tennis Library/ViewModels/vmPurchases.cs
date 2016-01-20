using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Tennis_Statistics.Helpers;
using Windows.UI.Xaml.Media;

namespace Tennis_Statistics.ViewModels
{
    public class vmPurchases : ObservableCollection<vmPurchaseableItem>
    {
        #region Constructor

        public vmPurchases()
        {
            List<PurchaseableItem> Items = Purchases.PurchaseableItems();
            vmPurchaseableItem vmItem;
            foreach (var Item in Items)
            {
                vmItem = new vmPurchaseableItem();
                vmItem.SetSource(Item);
                Add(vmItem);
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<vmPurchaseableItem> PurchaseableItems
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// The current background color or image
        /// </summary>
        public Brush Background
        {
            get
            {
                //Retrieve the setting
                return Tennis_Statistics.Helpers.Settings.GetInstance().Background();
            }
        }

        #endregion


    }
}
