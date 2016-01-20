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
    public class GetElementFromListConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, String a)
        {
            List<String> _Sets = (List<String>)value;
            int _Element = int.Parse((string)parameter);

            if (_Element >= _Sets.Count) return "";

            return _Sets[_Element];
        }

        public object ConvertBack(object value, Type targetType, object parameter, String a)
        {
            throw new NotImplementedException();
        }
    }
}
