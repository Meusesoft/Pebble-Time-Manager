using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Xml;

namespace Tennis_Statistics.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region NotifyPropertChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;


        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        protected void NotifyCollectionChanges(NotifyCollectionChangedAction Action, object Element)

        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(Action, Element));
            }        
        }

        #endregion

    }
}
