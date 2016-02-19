using Jint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pebble_Time_Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JavascriptPage : Page
    {
        public JavascriptPage()
        {
            this.InitializeComponent();

            LoadText();
        }

        private async void LoadText()
        {
            string filepath = @"Assets\javascript.txt";
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await folder.GetFileAsync(filepath); // error here
            var Lines = await Windows.Storage.FileIO.ReadTextAsync(file);

            txtScript.Text = Lines;


        }

        private Pebble _Pebble = new Pebble();
        private Engine _JintEngine;

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                _JintEngine = new Engine()
                    .SetValue("log", new Action<object>(Debug))
                    .SetValue("localStorage", new LocalStorage())
                    .SetValue("console", new Console())
                    .SetValue("Pebble", _Pebble);
                ;

                _JintEngine.Execute(txtScript.Text);
            }
            catch (Exception exp)
            {
                txtResult.Text = exp.Message;
            }
        }

        private void Debug(object text)
        {
            txtResult.Text = text.ToString();
        }

        public class Console
        {
            public void log(String value)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("log: {0}", value));
            }
        }

        private class LocalStorage
        {
            public object getItem(String obj)
            {
                return "{}";
            }
        }

        private class Pebble
        {
            public void addEventListener(String Event, object function)
            { 
                System.Diagnostics.Debug.WriteLine(String.Format("addEventListener(Event = {0}, Function = {1})", Event, function.ToString()));

                EventListeners.Add(Event, function);
            }

            public void openURL(String URL)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("openURL(URL={0})",URL));
            }

            public void sendAppMessage(object data)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("sendAppMessage(data={0})", data.ToString()));
            }

            private Dictionary<String, object> _EventListeners;

            public Dictionary<String, object> EventListeners
            {
                get
                {
                    if (_EventListeners == null) _EventListeners = new Dictionary<string, object>();
                    return _EventListeners;
                }
            }
        }

        private void btnAppMesssage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var jsfunction = _Pebble.EventListeners["appmessage"];

                System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = jsfunction as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                AppMessage AM = new AppMessage();
                AM.payload.request_weather = true;

                Jint.Native.JsValue A = Jint.Native.JsValue.FromObject(_JintEngine, AM);
                Jint.Native.JsValue[] B = new Jint.Native.JsValue[2];
                B[0] = new Jint.Native.JsValue(0);
                B[1] = new Jint.Native.JsValue(1);

                _JintEngine.SetValue("e", AM);

                Jint.Native.JsValue C = _func.Invoke(A, B);
            }
            catch (Exception exp)
            {
                txtResult.Text = exp.Message;
            }

        }

        private class AppMessage
        {
            public struct A { public bool request_weather; };

            public A payload;
        }
    }
}
