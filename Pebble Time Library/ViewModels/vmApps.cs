using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;



namespace Pebble_Time_Manager.ViewModels
{
    public class vmApps : ObservableCollection<vmApp>, INotifyPropertyChanged
    {
        #region Constructors

        public vmApps()
        {
            Initialize();
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Methods

        private void Initialize()
        {

          
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Load the selected apps from local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LoadSelectedItems()
        {
            try
            {
                ObservableCollection<vmApp> _vmApps;

                String XMLList = await Common.LocalStorage.Load("vmapps.xml");
                if (XMLList.Length > 0)
                {
                    _vmApps = (ObservableCollection<vmApp>)Common.Serializer.XMLDeserialize(XMLList, typeof(ObservableCollection<vmApp>));

                    if (_vmApps != null)
                    {
                        foreach (var item in this.Where(x => x.Selected == true))
                        {
                            item.Selected = false;
                        }

                        foreach (var item in _vmApps.Where(x => x.Selected == true))
                        {
                            var app = this.Where(x => x.ID == item.ID).First();

                            if (app != null) app.Selected = true;
                        }

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("vmApps.LoadSelectedItems exception: {0}", e.Message));
            }

            return false;
        }

        /// <summary>
        /// Load the stored apps from local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LoadStoredItems()
        {
            try
            {
                ObservableCollection<vmApp> _vmApps;

                String List = await Common.LocalStorage.Load("vmapps.xml");
                if (List.Length > 0)
                {
                    _vmApps = (ObservableCollection<vmApp>)Common.Serializer.Deserialize(List, typeof(ObservableCollection<vmApp>));

                    if (_vmApps != null)
                    {
                        this.Clear();

                        foreach (var item in _vmApps)
                        {
                            this.Add(item);
                        }

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("vmApps.LoadStoredItems exception: {0}", e.Message));
            }

            return false;
        }

        /// <summary>
        /// Save the selected items to local storage
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveItems()
        {
            try
            {
                String List = Common.Serializer.Serialize(this);
                await Common.LocalStorage.Save(List, "vmapps.xml", false);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("vmApps.SaveItems exception: {0}", e.Message));
            }

            return false;
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
