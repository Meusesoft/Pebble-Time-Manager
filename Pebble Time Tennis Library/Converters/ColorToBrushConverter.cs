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
    public class ColorToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, String a)
        {
            // Convert the color to a solid brush
            if (value is Color)
            {
                return new SolidColorBrush((Color)value);
            }

            return new SolidColorBrush(Color.FromArgb(255,0,255,0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, String a)
        {
            // If necessary, here you can convert back. Check if which brush it is (if its one),
            // get its Color-value and return it.

            throw new NotImplementedException();
        }
    }
}
