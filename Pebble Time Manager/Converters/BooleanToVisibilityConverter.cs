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

namespace Pebble_Time_Manager.Converters
{
    class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String a)
        {
            Boolean Result = true;
            
            //If the value is null then presume the element should be collapsed
            if (value == null)
            {
                Result = false;
            }
            else
            {
                //Process value of type boolean
                if (value.GetType() == typeof(bool))
                {

                    bool boolValue = (bool)value;

                    Result = boolValue;
                }

                //Process value of type string
                if (value.GetType() == typeof(string))
                {

                    string strValue = (string)value;

                    Result = strValue.Length > 0;
                }

                //Process value of type integer
                if (value.GetType() == typeof(int))
                {

                    int intValue = (int)value;

                    Result = (intValue > 0);
                }
            }

            //Invert if parameter is "XOR"
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(String))
                {
                    String ParameterValue = (String)parameter;

                    if (ParameterValue.ToLower() == "xor") Result = !Result;
                }
            }

            //Convert boolean to visibility
            return Result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, String a)
        {
            // If necessary, here you can convert back. Check if which brush it is (if its one),
            // get its Color-value and return it.

            throw new NotImplementedException();
        }
    }
}
