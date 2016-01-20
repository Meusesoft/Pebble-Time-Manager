using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.Graphics.Display;

namespace Tennis_Statistics.Helpers
{
    public enum Resolutions { WVGA, WXGA, HD };

    public class ResolutionHelper
    {
        public  static Double CurrentPixelsPerViewPixel
        {
            get
            {
                var DI = DisplayInformation.GetForCurrentView();

                switch (DI.ResolutionScale)
                {
                    case ResolutionScale.Scale100Percent: return 1.0;
                    case ResolutionScale.Scale120Percent: return 1.2;
                    case ResolutionScale.Scale140Percent: return 1.4;
                    case ResolutionScale.Scale160Percent: return 1.6;
                    case ResolutionScale.Scale180Percent: return 1.8;
                    case ResolutionScale.Scale225Percent: return 2.25;
                    default: return 1.0;
                }

                return DI.RawDpiX;
               // return DI.RawPixelsPerViewPixel;
                }

        }
        
        public  static bool IsWvga
        {
            get
            {
                return CurrentPixelsPerViewPixel == 1.2;
            }
        }

        public  static bool IsWxga
        {
            get
            {
                return CurrentPixelsPerViewPixel == 2.0;
            }
        }

        public  static bool IsHD
        {
            get
            {
                return (CurrentPixelsPerViewPixel == 2.2 | CurrentPixelsPerViewPixel == 1.8);
            }
        }

        public static Resolutions CurrentResolution
        {
            get
            {
                if (IsWvga) return Resolutions.WVGA;
                else if (IsWxga) return Resolutions.WXGA;
                else if (IsHD) return Resolutions.HD;
                else throw new InvalidOperationException("Unknown resolution");
            }
        }
    }
}
