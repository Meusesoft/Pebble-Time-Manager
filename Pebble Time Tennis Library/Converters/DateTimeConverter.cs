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
using Windows.Globalization.DateTimeFormatting;
using Windows.Globalization;

namespace Tennis_Statistics.Converters
{


    public class DateTimeConverter : IValueConverter
    {
            public object Convert(object value, Type targetType, object parameter, string language)
            {
                if (value == null)
                    return null;

                if (value is TimeSpan) return TimeSpanFormat((TimeSpan)value, parameter);

                if (value is DateTime)
                {
                    String _parameter = "default";
                    DateTime _value = (DateTime)value;

                    if (parameter is string)
                    {
                        _parameter = parameter as string;
                    }

                    switch (_parameter)
                    {
                        case "day":

                            return _value.Day;

                        case "month":

                            return DateTimeMonth(_value).Substring(1, 3);

                        default:
                     
                            return DateTimeFormat((DateTime)value);
                    }
                }

                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter,
                string language)
            {
                throw new NotImplementedException();
            }

            private String TimeSpanFormat(TimeSpan value, object parameter)
            {
                String Result = "";
                if (parameter != null)
                {
                    if (parameter is string) Result = (string)parameter;
                }

                return Result + String.Format("{0}:{1:D2}", ((value.Days * 24) + value.Hours), value.Minutes);
            }

            private String DateTimeFormat(DateTime value)
            {
                var homeLanguages = Windows.System.UserProfile.GlobalizationPreferences.Languages;
                var homeRegion = Windows.System.UserProfile.GlobalizationPreferences.HomeGeographicRegion;

                var dateTimeFormatter = new DateTimeFormatter(YearFormat.Full,
                    MonthFormat.Full,
                    DayFormat.Default,
                    DayOfWeekFormat.None,
                    HourFormat.None,
                    MinuteFormat.None,
                    SecondFormat.None,
                    homeLanguages,
                    homeRegion,
                    CalendarIdentifiers.Gregorian,
                    ClockIdentifiers.TwentyFourHour);

                return dateTimeFormatter.Format(value).ToUpper();
            }

            private String DateTimeMonth(DateTime value)
            {
                var homeLanguages = Windows.System.UserProfile.GlobalizationPreferences.Languages;
                var homeRegion = Windows.System.UserProfile.GlobalizationPreferences.HomeGeographicRegion;

                var dateTimeFormatter = new DateTimeFormatter(YearFormat.None,
                    MonthFormat.Full,
                    DayFormat.None,
                    DayOfWeekFormat.None,
                    HourFormat.None,
                    MinuteFormat.None,
                    SecondFormat.None,
                    homeLanguages,
                    homeRegion,
                    CalendarIdentifiers.Gregorian,
                    ClockIdentifiers.TwentyFourHour);

                String Result = dateTimeFormatter.Format(value).ToUpper();

                return Result;
            }
    }
}
