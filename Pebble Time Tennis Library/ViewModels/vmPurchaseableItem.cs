using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Tennis_Statistics.Helpers;

namespace Tennis_Statistics.ViewModels
{
    public class vmPurchaseableItem : INotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        /// The ID of this item
        /// </summary>
        public int ID
        {
            get
            {
                return Model.ID;
            }
        }
        
        /// <summary>
        /// The name / short description of this item
        /// </summary>
        public String Name { 
            get
            {
                return Model.Name;
            }
        }

        /// <summary>
        /// The long / description of this item
        /// </summary>
        public String Description
        {
            get
            {
                return Model.Description;
            }
        }

        /// <summary>
        /// True if this item is purchased
        /// </summary>
        public bool Purchased
        {
            get
            {
                return Purchases.getReference().Available(Model.ID);
            }
        }


        /// <summary>
        /// The price to purchase this item
        /// </summary>
        public double Price
        {
            get
            {
                return Model.Price;
            }
        }

        /// <summary>
        /// The instance of the item this viewmodel encapsulates 
        /// </summary>
        public PurchaseableItem Model
        {
            get
            {
                return m_Model;
            }
        }
        private PurchaseableItem m_Model;

        #endregion

        #region Commands

        RelayCommand m_cmdPurchase;
        /// <summary>
        /// The relaycommand for purchasing this item
        /// </summary>
        public RelayCommand cmdPurchase
        {
            get
            {
                if (m_cmdPurchase == null)
                    m_cmdPurchase = new RelayCommand(param => Purchase());

                return m_cmdPurchase;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the source instance for this viewmodel
        /// </summary>
        /// <param name="Instance"></param>
        public void SetSource(PurchaseableItem Instance)
        {
            m_Model = Instance;
        }


        /// <summary>
        /// Purchase the item this viewmodel represents
        /// </summary>
        public void Purchase()
        {
            if (!Purchased)
            {
                Purchases.Purchase(ID);
                NotifyPropertyChanged("Purchased");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
