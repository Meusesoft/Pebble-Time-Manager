using Jint;
using Jint.Runtime.Interop;
using Jint.Native;
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
using System.Net;
using Windows.Devices.Geolocation;
using System.Dynamic;
using Pebble_Time_Manager.Connector;

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

        private String[] JavascriptFile;

        private async void LoadText()
        {
            string filepath = @"Assets\javascript.txt";
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await folder.GetFileAsync(filepath); // error here
            var Lines = await Windows.Storage.FileIO.ReadTextAsync(file);

            txtScript.Text = Lines;

            JavascriptFile = Lines.Split((char)13);
        }

        private Pebble _Pebble = new Pebble();
        private Engine _JintEngine;

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                _Pebble.webView = webConfig;

                _JintEngine = new Engine()
                    .SetValue("log", new Action<object>(Debug))
                    .SetValue("localStorage", new LocalStorage())
                    .SetValue("console", new Console())
                    .SetValue("Pebble", _Pebble)
                    .SetValue("navigator", new Navigator())
                ;

                _JintEngine.SetValue("XMLHttpRequest", TypeReference.CreateTypeReference(_JintEngine, typeof(XMLHttpRequest)));

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
            public object getItem(String item)
            {
                return "null";
            }

            public object setItem(String item, string value)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("LocalStorage.setItem: {0}", value));
                return "null";
            }
        }

        private class Navigator
        {
            private GeoLocation _geolocation;
            public GeoLocation geolocation
            {
                get
                {
                    if (_geolocation == null) _geolocation = new GeoLocation();
                    return _geolocation;
                }
            }

            internal class GeoLocation
            {
                private object _funcSuccess;
                private object _funcError;

                private Geolocator _geoLocator;
                public Geolocator geoLocator
                {
                    get
                    {
                        if (_geoLocator== null) _geoLocator = new Geolocator();
                        return _geoLocator;
                    }
                }

                public async void getCurrentPosition(object funcSuccess, object funcError, object options)
                {
                    System.Diagnostics.Debug.WriteLine("GeoLocation.getCurrentPosition");

                    _funcSuccess = funcSuccess; 
                    _funcError = funcError;

                    try
                    {
                        geoLocator.DesiredAccuracy = PositionAccuracy.Default;
                        Geoposition _pos = await geoLocator.GetGeopositionAsync();

                        ExecuteSuccess(_pos);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception" + e.Message);

                        ExecuteError(e.HResult, e.Message);
                    }
                }

                private void ExecuteSuccess(Geoposition _pos)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("GeoLocation.ExecuteSuccess");

                        System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = _funcSuccess as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;
                        Jint.Native.Function.ScriptFunctionInstance JintScript = _func.Target as Jint.Native.Function.ScriptFunctionInstance;
                        Jint.Engine _JintEngine = JintScript.Engine;

                        JsValue A = new JsValue(1);
                        JsValue[] B = new JsValue[1];

                        Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(_JintEngine);
                        String FormatString = "{{\"coords\":{{\"latitude\":{0}, \"longitude\":{1}}}}}";
                        String Coordinates = String.Format(FormatString, _pos.Coordinate.Latitude, _pos.Coordinate.Longitude);
                        B[0] = _jsp.Parse(Coordinates);

                        _func.Invoke(A, B);
                    }
                    catch (Jint.Runtime.JavaScriptException exc)
                    {
                        System.Diagnostics.Debug.WriteLine("JavaScriptException" + exc.Message + " " + exc.LineNumber);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception" + e.Message);

                        ExecuteError(e.HResult, e.Message);
                    }
                }

                private void ExecuteError(int errcode, string errmessage)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("GeoLocation.ExecuteError");

                        System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = _funcError as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;
                        Jint.Native.Function.ScriptFunctionInstance JintScript = _func.Target as Jint.Native.Function.ScriptFunctionInstance;
                        Jint.Engine _JintEngine = JintScript.Engine;

                        JsValue A = new JsValue(1);
                        JsValue[] B = new Jint.Native.JsValue[1];

                        Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(_JintEngine);
                        String FormatString = "{{\"code\":{0}, \"message\":\"{1}\"}}";
                        String Error = String.Format(FormatString, errcode, errmessage);
                        B[0] = _jsp.Parse(Error);

                        _func.Invoke(A, B);
                    }
                    catch (Exception exp)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception" + exp.Message);
                    }
                }
            }
        }

        private class XMLHttpRequest
        {
            #region Properties

            private HttpWebRequest _httpWebRequest;
            private HttpWebRequest httpWebRequest
            {
                get
                {
                    return _httpWebRequest;
                }
            }

            private object _onload;
            public object onload
            {
                get
                {
                    return _onload;
                }
                set
                {
                    _onload = value;

                    System.Diagnostics.Debug.WriteLine(String.Format("XMLHttpRequest.onload set={0}", _onload));
                }
            }

            private int _status = 0;
            public int status
            {
                get
                {
                    return _status;
                }
            }

            private int _readyState;
            public int readyState
            {
                get
                {
                    return _readyState;
                }
            }

            private string _responseText;
            public string responseText
            {
                get
                {
                    return _responseText;
                }
            }

            #endregion

            #region Methods

            public void open(string method, string url)
            {
                open(method, url, true);
            }

            public void open(string method, string url, bool async)
            {
                open(method, url, async, "", "");
            }

            public void open(string method, string url, bool async = true, string user = "", string password = "")
            {
                System.Diagnostics.Debug.WriteLine(String.Format("XMLHttpRequest.open method={0}, url={1}, async={2}", method, url, async));

                _httpWebRequest = WebRequest.CreateHttp(url);
                _httpWebRequest.Method = method;

            }

            public void send()
            {
                send(null);
            }

            public async void send(object data)
            {
                WebResponse _response;

                try
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("XMLHttpRequest.send data={0}", data == null ? "null" : data.ToString()));
                    _httpWebRequest.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586";
                    

                    //_httpWebRequest.Accept = "text/html, application/json";
                    /*
                   Accept-Encoding: gzip, deflate
                   Accept-Language: nl-NL, nl; q=0.5
                   Cache-Control: no-cache
                   Connection: Keep-Alive
                   Host: nominatim.openstreetmap.org
                   User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586
                   */

                    _response = await _httpWebRequest.GetResponseAsync();
                    
                    Stream _stream = _response.GetResponseStream();
                    StreamReader _tr = new StreamReader(_stream);
                    String Response = _tr.ReadToEnd();

                    System.Diagnostics.Debug.WriteLine(String.Format("XMLHttpRequest.send response={0}", Response));

                    _readyState = 4;
                    _status = 200;
                    _responseText = Response;

                    if (onload != null)
                    {

                        var jsfunction = onload;

                        System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = jsfunction as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                        Jint.Native.JsValue A = new JsValue(1);
                        Jint.Native.JsValue[] B = new Jint.Native.JsValue[1];
                        Jint.Native.JsValue C = _func.Invoke(A, B);
                    }
                }
                catch (Jint.Runtime.JavaScriptException exc)
                {
                    System.Diagnostics.Debug.WriteLine("JavaScriptException " + exc.Message + exc.LineNumber);

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception " + e.Message);
                }
               finally
                {
                    //_response.Close;
                }
            }

            #endregion



        }



    private class Pebble
        {

            public WebView webView { get; set; }

            public void addEventListener(String Event, object function)
            { 
                System.Diagnostics.Debug.WriteLine(String.Format("addEventListener(Event = {0}, Function = {1})", Event, function.ToString()));

                if (EventListeners.ContainsKey(Event)) EventListeners.Remove(Event);
                EventListeners.Add(Event, function);
            }

            public void openURL(String URL)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("openURL(URL={0})",URL));

                if (webView!=null) webView.Navigate(new Uri(URL));
            }

            public string getAccountToken()
            {
                return "account";
            }

            public void sendAppMessage(ExpandoObject data)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("sendAppMessage(data={0})", data.ToString()));

                P3bble.Messages.AppMessage _am = new P3bble.Messages.AppMessage(P3bble.Constants.Endpoint.ApplicationMessage);
                uint iKey = 0;

                _am.Content = new Dictionary<int, object>(data.Count());

                foreach (var element in data)
                {
                    Type VariableType = element.Value.GetType();
                    System.Diagnostics.Debug.WriteLine(String.Format("  key: {0}, value: {1}, type: {2}", element.Key, element.Value, VariableType.ToString()));

                    if (VariableType == typeof(String))
                    {
                        String Value = (String)element.Value;
                        //_am.AddTuple(iKey, P3bble.Messages.AppMessageTupleDataType.String, System.Text.Encoding.UTF8.GetBytes(Value));
                        _am.Content.Add((int)iKey, Value);
                    }

                    if (VariableType == typeof(Double))
                    {
                        double dValue = (double)element.Value;
                        int iValue = (int)dValue;
                        byte[] bytes = BitConverter.GetBytes(iValue);
                        //_am.AddTuple(iKey, P3bble.Messages.AppMessageTupleDataType.Int, bytes);
                        _am.Content.Add((int)iKey, iValue);
                    }

                    iKey++;
                }

                PebbleConnector _pc = PebbleConnector.GetInstance();

                byte[] package = _am.ToBuffer();
                System.Diagnostics.Debug.WriteLine("<< PAYLOAD: " + BitConverter.ToString(package).Replace("-", ":"));
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

                Jint.Native.JsValue A = new JsValue(1);
                Jint.Native.JsValue[] B = new Jint.Native.JsValue[1];

                Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(_JintEngine);
                Jint.Native.JsValue _eValue = _jsp.Parse("{\"payload\":{\"request_weather\":\"true\"}}");

                B[0] = _eValue;
               // B[1] = new Jint.Native.JsValue(1);

               // _JintEngine.SetValue("eValue", _eValue);

                _JintEngine.Step += _JintEngine_Step;
               

                Jint.Native.JsValue C = _func.Invoke(A, B);
            }
            catch (Jint.Runtime.JavaScriptException exp)
            {
                String Exception = String.Format("{0}" + Environment.NewLine + "Line: {1}" + Environment.NewLine + "Source: {2}", 
                    exp.Message, 
                    exp.LineNumber,
                    JavascriptFile[exp.LineNumber - 1]);

                txtResult.Text =  Exception;
            }
        }

        private Jint.Runtime.Debugger.StepMode _JintEngine_Step(object sender, Jint.Runtime.Debugger.DebugInformation e)
        {
            System.Diagnostics.Debug.WriteLine(e.CurrentStatement.ToString());

            return Jint.Runtime.Debugger.StepMode.Into;
        }


        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var jsfunction = _Pebble.EventListeners["showConfiguration"];

                System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = jsfunction as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                Jint.Native.JsValue A = new JsValue(1);
                Jint.Native.JsValue[] B = new Jint.Native.JsValue[1];

                Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(_JintEngine);
                Jint.Native.JsValue _eValue = _jsp.Parse("{}");

                B[0] = _eValue;

                _JintEngine.Step += _JintEngine_Step;

                App.Activated += App_Activated;


                Jint.Native.JsValue C = _func.Invoke(A, B);
            }
            catch (Jint.Runtime.JavaScriptException exp)
            {
                String Exception = String.Format("{0}" + Environment.NewLine + "Line: {1}" + Environment.NewLine + "Source: {2}",
                    exp.Message,
                    exp.LineNumber,
                    JavascriptFile[exp.LineNumber - 1]);

                txtResult.Text = Exception;
            }
        }


        private void App_Activated(object sender, string e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(e);

                webConfig.NavigateToString("_blank");

                App.Activated -= App_Activated;

                var jsfunction = _Pebble.EventListeners["webviewclosed"];

                String Argument = e;

                String[] Result = e.Split("#".ToCharArray());
                if (Result.Count() > 1)
                {
                    Argument = Result[1];
                }

                System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = jsfunction as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                Jint.Native.JsValue A = new JsValue(1);
                Jint.Native.JsValue[] B = new Jint.Native.JsValue[1];

                Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(_JintEngine);
                String JSON = String.Format("{{\"response\":\"{0}\"}}", Uri.EscapeUriString(Uri.UnescapeDataString(Argument)));
                Jint.Native.JsValue _eValue = _jsp.Parse(JSON);

                B[0] = _eValue;

                Jint.Native.JsValue C = _func.Invoke(A, B);
            }
            catch (Jint.Runtime.JavaScriptException exp)
            {
                String Exception = String.Format("{0}" + Environment.NewLine + "Line: {1}" + Environment.NewLine + "Source: {2}",
                    exp.Message,
                    exp.LineNumber,
                    JavascriptFile[exp.LineNumber - 1]);

                txtResult.Text = Exception;
            }
            catch (Exception exp)
            {
                txtResult.Text = exp.Message;
            }
        }
    }
}
