using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Tennis_Statistics.Converters
{
    public class ItemToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String a)
        {
            List<object> _List = (List<object>)parameter;
            object _Element = value;

            if (_List.Contains(_Element))
            {
                return (_List.IndexOf(_Element)).ToString();
            }

            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, String a)
        {
            throw new NotImplementedException();
        }

    }
}
