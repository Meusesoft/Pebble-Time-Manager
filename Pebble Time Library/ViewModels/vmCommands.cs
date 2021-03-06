﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace Pebble_Time_Manager.ViewModels
{
    public class vmCommands : INotifyPropertyChanged
    {

        #region Properties

        private bool _ClearLog;
        public bool ClearLog
        {
            get
            {
                return _ClearLog;
            }
            set
            {
                _ClearLog = value;
                NotifyPropertyChanged("ClearLog");
            }
        }

        private bool _Synchronize;
        public bool Synchronize
        {
            get
            {
                return _Synchronize;
            }
            set
            {
                _Synchronize = value;
                NotifyPropertyChanged("Synchronize");
            }
        }

        private bool _EditFaces;
        public bool EditFaces
        {
            get
            {
                return _EditFaces;
            }
            set
            {
                _EditFaces = value;
                NotifyPropertyChanged("EditFaces");
            }
        }

        private bool _DeleteFaces;
        public bool DeleteFaces
        {
            get
            {
                return _DeleteFaces;
            }
            set
            {
                _DeleteFaces = value;
                NotifyPropertyChanged("DeleteFaces");
            }
        }

        private bool _EditApps;
        public bool EditApps
        {
            get
            {
                return _EditApps;
            }
            set
            {
                _EditApps = value;
                NotifyPropertyChanged("EditApps");
            }
        }

        private bool _DeleteApps;
        public bool DeleteApps
        {
            get
            {
                return _DeleteApps;
            }
            set
            {
                _DeleteApps = value;
                NotifyPropertyChanged("DeleteApps");
            }
        }


        private bool _SearchStore;
        public bool SearchStore
        {
            get
            {
                return _SearchStore;
            }
            set
            {
                _SearchStore = value;
                NotifyPropertyChanged("SearchStore");
            }
        }

        private bool _FaceStore;
        public bool FaceStore
        {
            get
            {
                return _FaceStore;
            }
            set
            {
                _FaceStore = value;
                NotifyPropertyChanged("FaceStore");
            }
        }

        private bool _AppStore;
        public bool AppStore
        {
            get
            {
                return _AppStore;
            }
            set
            {
                _AppStore = value;
                NotifyPropertyChanged("AppStore");
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
