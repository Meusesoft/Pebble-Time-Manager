using System;
using System.Collections.ObjectModel;
using System.Linq;
using Tennis_Statistics.ViewModels;
using Windows.UI.Xaml.Data;

namespace Tennis_Statistics.Converters
{
    public class MultiResolutionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String a)
        {
            if (parameter == null) return 10;

            String ParameterValue = (String)parameter;
            int intParameter = int.Parse(ParameterValue);
            double Converted = ((double)intParameter / 1.2) * Tennis_Statistics.Helpers.ResolutionHelper.CurrentPixelsPerViewPixel;
            Converted = Math.Round(Converted);

            if (targetType == typeof(Double)) return Converted;
            if (targetType == typeof(Windows.UI.Xaml.GridLength))
            {
                Windows.UI.Xaml.GridLength Result = new Windows.UI.Xaml.GridLength(Converted);
                return Result;
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, String a)
        {
            // If necessary, here you can convert back. Check if which brush it is (if its one),
            // get its Color-value and return it.

            throw new NotImplementedException();
        }

    }
}
