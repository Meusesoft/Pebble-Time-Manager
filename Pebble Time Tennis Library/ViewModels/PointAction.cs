using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Media;


namespace Tennis_Statistics.ViewModels
{
    public class PointAction
    {
        public String Command { get; set; }
        public String Method { get; set; }
        public Color Color { get; set; }
        public ImageSource Icon { get; set; }

    }
}
