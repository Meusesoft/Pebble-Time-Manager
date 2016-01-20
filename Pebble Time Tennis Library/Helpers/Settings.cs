using System;
using System.Collections.Generic;
using System.Text;
using Tennis_Statistics.Helpers;
using System.Threading.Tasks;
using Windows.Media;
using Windows.UI.Xaml.Media;

namespace Tennis_Statistics.Helpers
{
    public class Settings
    {
        #region Constructor

        public Settings()
        {
        }

        #endregion

        #region Fields

        #endregion

        #region Methods

        /*public async void Save()
        {
            String XML = Serializer.XMLSerialize(this);
            await LocalStorage.Save(XML, "settings.xml", false);
        }*/

        /// <summary>
        /// Set or update the setting
        /// </summary>
        /// <param name="Setting"></param>
        /// <param name="Value"></param>
        public void Set(String Setting, object Value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            if (localSettings.Values.ContainsKey(Setting))
            {
                localSettings.Values[Setting] = Value;
            }
            else
            {
                localSettings.Values.Add(Setting, Value);
            }

            //Fire event
            if (SettingChanged != null) SettingChanged(this, Setting);
        }

        /// <summary>
        /// Get the value of the requested setting
        /// </summary>
        /// <param name="Setting"></param>
        /// <returns></returns>
        public object Get(String Setting)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            if (localSettings.Values.ContainsKey(Setting))
            {
                return localSettings.Values[Setting];
            }
            else
            {
                return null;
            }            
        }

        /// <summary>
        /// Get the value of the requested setting
        /// </summary>
        /// <param name="Setting"></param>
        /// <returns></returns>
        public bool GetBoolean(String Setting, bool Default)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            if (localSettings.Values.ContainsKey(Setting))
            {
                object value = localSettings.Values[Setting];
                if (value is bool)
                {
                    return (bool)value;
                }

                return Default;
            }
            else
            {
                return Default;
            }
        }
        
        /// <summary>
        /// The current background color or image
        /// </summary>
        public Brush Background()
        {
            Windows.UI.Xaml.Media.Imaging.BitmapImage image;
            ImageBrush _Brush;

            //Retrieve the setting
            object Value = Get("Background");
            String Background = "Default";
            if (Value is String) Background = (String)Value;

            switch (Background)
            {
                case "Black":

                    return new SolidColorBrush(Windows.UI.Color.FromArgb(16, 255, 255, 255));

                case "White":

                    return new SolidColorBrush(Windows.UI.Color.FromArgb(192, 255, 255, 255));

                case "Blue":

                    return new SolidColorBrush(Windows.UI.Color.FromArgb(192, 0, 148, 255));

                case "UserDefined":

                    try
                    {
                        Value = Get("BackgroundImage");
                        String BackgroundImageIndex = (String)Value;
                        int Index = Int16.Parse(BackgroundImageIndex);

                        switch (Index)
                        {
                            case 0: image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/australianopen.jpg")); break;
                            case 1: image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/rolandgarros.jpg")); break;
                            case 2: image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/wimbledon.jpg")); break;
                            case 3: image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/usopen2.jpg")); break;
                            default:
                            case 4: image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/atptourfinals.jpg")); break;
                        }

                        _Brush = new ImageBrush();
                        _Brush.Opacity = 0.50;
                        _Brush.Stretch = Stretch.UniformToFill;
                        _Brush.ImageSource = image;

                        return _Brush;
                    }
                    catch (Exception)
                    {
                        return new SolidColorBrush(Windows.UI.Color.FromArgb(16, 255, 255, 255));
                    }

                    break;

                case "Default":
                default:


                    switch (DateTime.Now.Month)
                    {
                        case 1:
                        case 2:
                            image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/australianopen.jpg"));
                            break;
                        case 3:
                        case 4:
                        case 5:
                            image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/rolandgarros.jpg"));
                            break;
                        case 6:
                        case 7:
                            image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/wimbledon.jpg"));
                            break;
                        case 8:
                        case 9:
                            image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/usopen2.jpg"));
                            break;
                        default:
                            image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri("ms-appx:///Assets/atptourfinals.jpg"));
                            break;
                    }

                    _Brush = new ImageBrush();
                    _Brush.Opacity = 0.50;
                    _Brush.Stretch = Stretch.UniformToFill;
                    _Brush.ImageSource = image;


                    return _Brush;
            }
        }

        #endregion

        #region Events

        public delegate void ChangedEventHandler(object sender, String Setting);

        public event ChangedEventHandler SettingChanged;

        #endregion

        #region Static elements

        private static Settings _SettingsInstance;

        /// <summary>
        /// Returns the global instance of the Settings class
        /// </summary>
        /// <returns></returns>
        public static Settings GetInstance()
        {
            if (_SettingsInstance == null) _SettingsInstance = new Settings();

            return _SettingsInstance;
        }
        
        #endregion

        #region Inner classes



        #endregion
    }
}
