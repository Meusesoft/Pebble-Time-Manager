using System;
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
    public class vmWatchApp : INotifyPropertyChanged
    {
        #region Constructor

        public vmWatchApp()
        {
            Editable = true;
        }

        #endregion
        
        #region Fields

        private string _Name;
        private bool _Selected;
        private bool _Editable;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the watch app
        /// </summary>
        public String Name 
        { 
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private String _Developer;
        public String Developer
        {
            get
            {
                return _Developer;
            }
            set
            {
                _Developer = value;
                NotifyPropertyChanged("Developer");
            }
        }

        /// <summary>
        /// The Guid of the watch app
        /// </summary>
        public Guid Model;

        /// <summary>
        /// Selected
        /// </summary>
        public bool Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
                NotifyPropertyChanged("Selected");
            }
        }

        /// <summary>
        /// Editable
        /// </summary>
        public bool Editable
        {
            get
            {
                return _Editable;
            }
            set
            {
                _Editable = value;
                NotifyPropertyChanged("Editable");
            }
        }

        private bool _Active;
        /// <summary>
        /// The active watch face
        /// </summary>
        public bool Active
        {
            get
            {
                return _Active;
            }
            set
            {
                _Active = value;
                NotifyPropertyChanged("Active");
            }
        }

        /// <summary>
        /// The Guid of the watch app
        /// </summary>
        private Windows.UI.Xaml.Media.Imaging.BitmapImage _Image;
        public Windows.UI.Xaml.Media.Imaging.BitmapImage Image
        {

            get
            {
                return _Image;
            }
            set
            {
                _Image = value;
                NotifyPropertyChanged("Image");
            }
        }

        public String ImageFile { get; set; }

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
