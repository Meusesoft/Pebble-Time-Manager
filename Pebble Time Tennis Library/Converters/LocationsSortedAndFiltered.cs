using System;
using System.Collections.ObjectModel;
using System.Linq;
using Tennis_Statistics.ViewModels;
using Windows.UI.Xaml.Data;

namespace Tennis_Statistics.Converters
{
    public class LocationsSortedAndFiltered : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String a)
        {
            if (value == null) return null;

            ObservableCollection<vmNewMatch.vmLocation> Source = (ObservableCollection<vmNewMatch.vmLocation>)value;

            var SortedList = from l in Source where l.Distance < 20 orderby l.Distance select l;

            return SortedList;

        }

        public object ConvertBack(object value, Type targetType, object parameter, String a)
        {
            // If necessary, here you can convert back. Check if which brush it is (if its one),
            // get its Color-value and return it.

            throw new NotImplementedException();
        }

    }
}
