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
    public class BooleanXOR : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String a)
        {
            Boolean Result = true;

            //If the value is null then presume the element should be collapsed
            if (value == null)
            {
                Result = true;
            }
            else
            {
                //Process value of type boolean
                if (value.GetType() == typeof(bool))
                {

                    bool boolValue = (bool)value;

                    Result = !boolValue;
                }
            }

            return Result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, String a)
        {
            // If necessary, here you can convert back. Check if which brush it is (if its one),
            // get its Color-value and return it.

            throw new NotImplementedException();
        }
    }
}
