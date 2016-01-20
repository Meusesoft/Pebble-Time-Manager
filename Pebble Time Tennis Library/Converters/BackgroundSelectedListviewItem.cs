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
    public class TemplateSelectedListviewItem : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String a)
        {
            DataTemplateSelector Selector = (DataTemplateSelector)parameter;

            return Selector.SelectTemplate(value);

        }

        public object ConvertBack(object value, Type targetType, object parameter, String a)
        {
            throw new NotImplementedException();
        }

    }
}
