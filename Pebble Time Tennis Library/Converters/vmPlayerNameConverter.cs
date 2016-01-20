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
using Tennis_Statistics.ViewModels;

namespace Tennis_Statistics.Converters
{
    public class vmPlayerNameConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            String Result = "";
            if (value == null) return "null";

            if (value.GetType() == typeof(vmPlayer))
            {
                Result = ((vmPlayer)value).Name;
            }

            return Result;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            vmPlayer newPlayer = new vmPlayer();
            newPlayer.Name = value.ToString();

            return newPlayer;
        }
    }
}
