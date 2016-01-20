using System;
using Windows.UI.Xaml.Data;

namespace Tennis_Statistics.Converters
{
    public class ContestantNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            
            String ContestantNameLong = (String)value;
            String ContestantNameShort = ContestantNameLong;

            //If the name is longer than 10 characters. Split it and take the parts.
            if (ContestantNameLong.Length > 10)
            {
                ContestantNameShort = "";

                String[] Parts = ContestantNameLong.Split(" ".ToCharArray());
                int CharactersPerParts = 10 / Parts.Length;

                foreach (String Part in Parts)
                {
                    ContestantNameShort += Part.Substring(0, Math.Min(CharactersPerParts, Part.Length));
                }
            }

            return ContestantNameShort;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            string language)
        {
            throw new NotImplementedException();
        }
    }
}
