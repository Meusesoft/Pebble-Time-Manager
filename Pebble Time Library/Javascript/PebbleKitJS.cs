using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Pebble_Time_Manager.Connector;
using Pebble_Time_Manager.ViewModels;
using Pebble_Time_Manager.WatchItems;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Pebble_Time_Manager.Common;
using System.Runtime.Serialization;

namespace Pebble_Time_Library.Javascript
{
    public class PebbleKitJS
    {
        #region Constructors

        public PebbleKitJS(IWatchItem ParentItem)
        {
            _ParentItem = ParentItem;
            _Pebble = new Pebble(_ParentItem);

        }

        #endregion

        #region Fields

        private Pebble _Pebble;
        private Engine _JintEngine;
        private String[] _JavascriptLines;
        private IWatchItem _ParentItem;

        #endregion

        #region Properties

        public Engine JintEngine
        {
            get
            {
                return _JintEngine;
            }
        }


        #endregion

        #region Methods

        private async Task Initialise()
        {

            try
            {
                LocalStorage _ls = new LocalStorage(_ParentItem);
                await _ls.Load();

                _JintEngine = new Engine()
                    .SetValue("log", new Action<object>(Debug))
                    .SetValue("localStorage", _ls)
                    .SetValue("console", new Console())
                    .SetValue("Pebble", _Pebble)
                    .SetValue("navigator", new Navigator())
                ;

                _JintEngine.SetValue("XMLHttpRequest", TypeReference.CreateTypeReference(_JintEngine, typeof(XMLHttpRequest)));

            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public async Task Execute(String Javascript)
        {
            try
            {
                if (_JintEngine == null) await Initialise();

                _JavascriptLines = Javascript.Split("\n\r".ToCharArray());

                _JintEngine.Execute(Javascript);
            }
            catch (Jint.Runtime.JavaScriptException exp)
            {
                String Exception = String.Format("{0}" + Environment.NewLine + "Line: {1}" + Environment.NewLine + "Source: {2}",
                    exp.Message,
                    exp.LineNumber,
                    _JavascriptLines[exp.LineNumber - 1]);

                throw new System.Exception(Exception);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public void ShowConfiguration(IWatchItem item)
        {
            try
            {
                var jsfunction = _Pebble.EventListeners["showConfiguration"];

                System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = jsfunction as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                Jint.Native.JsValue A = new JsValue(1);
                Jint.Native.JsValue[] B = new Jint.Native.JsValue[1];

                Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(_JintEngine);
                B[0] = _jsp.Parse("{}");

                Jint.Native.JsValue C = _func.Invoke(A, B);
            }
            catch (Jint.Runtime.JavaScriptException exp)
            {
                String Exception = String.Format("{0}" + Environment.NewLine + "Line: {1}" + Environment.NewLine + "Source: {2}",
                    exp.Message,
                    exp.LineNumber,
                    _JavascriptLines[exp.LineNumber - 1]);

                throw new System.Exception(Exception);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public void WebViewClosed(String Data)
        {
            try
            {
                var jsfunction = _Pebble.EventListeners["webviewclosed"];

                String Argument = Data;

                String[] Result = Data.Split("#".ToCharArray());
                if (Result.Count() > 1)
                {
                    Argument = Result[1];

                    System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = jsfunction as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                    Jint.Native.JsValue A = new JsValue(1);
                    Jint.Native.JsValue[] B = new Jint.Native.JsValue[1];

                    Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(_JintEngine);
                    String JSON = String.Format("{{\"response\":\"{0}\"}}", Uri.EscapeUriString(Uri.UnescapeDataString(Argument)));
                    Jint.Native.JsValue _eValue = _jsp.Parse(JSON);

                    B[0] = _eValue;

                    Jint.Native.JsValue C = _func.Invoke(A, B);
                }
            }
            catch (Jint.Runtime.JavaScriptException exp)
            {
                String Exception = String.Format("{0}" + Environment.NewLine + "Line: {1}" + Environment.NewLine + "Source: {2}",
                    exp.Message,
                    exp.LineNumber,
                    _JavascriptLines[exp.LineNumber - 1]);

                throw new System.Exception(Exception);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        private void Debug(object text)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("log: {0}", text.ToString()));
        }

        #endregion

        #region Events

        public delegate void OpenURLEventHandler(object sender, EventArgs e);
        public static event OpenURLEventHandler OpenURL;

        public class URLEventArgs : EventArgs
        {
            public String URL;
            public IWatchItem WatchItem;
        }

        #endregion

        #region private classes

        private class Console
        {
            public void log(String value)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("log: {0}", value));
            }
            public void warn(String value)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("warn: {0}", value));
            }
        }

        [DataContract]
        private class LocalStorage
        {
            public LocalStorage()
            {
            }

            public LocalStorage(IWatchItem ParentItem)
            {
                StorageContainer = new Dictionary<Guid, Dictionary<string, string>>();
                _ParentItem = ParentItem;
            }

            private IWatchItem _ParentItem;

            [DataMember]
            public Dictionary<Guid, Dictionary<String, String>> StorageContainer { get; set; }


            public object getItem(String item)
            {
                String value = "null";

                /*if (ParentItem.StoredItems == null)
                {
                    ParentItem.StoredItems = new Dictionary<string, string>();
                }

                if (ParentItem.StoredItems.ContainsKey(item))
                {
                    return ParentItem.StoredItems[item];
                }*/

                if (StorageContainer.ContainsKey(_ParentItem.ID))
                {
                    Dictionary<String, String> ItemStorageContainer = StorageContainer[_ParentItem.ID];

                    if (ItemStorageContainer.ContainsKey(item))
                    {
                        value = ItemStorageContainer[item];
                    }
                }

                System.Diagnostics.Debug.WriteLine(String.Format("LocalStorage.getItem: {0}={1}", item, value));

                return value;
            }

            public async Task<object> setItem(String item, string value)
            {
                /*if (ParentItem.StoredItems == null)
                {
                    ParentItem.StoredItems = new Dictionary<string, string>();
                }

                if (ParentItem.StoredItems.ContainsKey(item))
                {
                    ParentItem.StoredItems[item] = value;
                }
                else
                {
                    ParentItem.StoredItems.Add(item, value);
                }*/

                if (!StorageContainer.ContainsKey(_ParentItem.ID))
                {
                    StorageContainer.Add(_ParentItem.ID, new Dictionary<string, string>());
                }

                if (StorageContainer.ContainsKey(_ParentItem.ID))
                {
                    Dictionary<String, String> ItemStorageContainer = StorageContainer[_ParentItem.ID];

                    if (ItemStorageContainer.ContainsKey(item))
                    {
                        ItemStorageContainer[item] = value;
                    }
                    else
                    {
                        ItemStorageContainer.Add(item, value);
                    }

                    await Save();
                }

                System.Diagnostics.Debug.WriteLine(String.Format("LocalStorage.setItem: {0}<={1}", item, value));

                return value;
            }

            public async Task Save()
            {
                try
                {
                    String List = Serializer.Serialize(this);
                    await Pebble_Time_Manager.Common.LocalStorage.Save(List, "pebblejs_storage.xml", false);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("PebbleJS.LocalStorage.Save exception: {0}", e.Message));
                }
            }

            public async Task Load()
            {
                try
                {
                    LocalStorage _temp;

                    String List = await Pebble_Time_Manager.Common.LocalStorage.Load("pebblejs_storage.xml");

                    if (List.Length == 0)
                    {
                        //try backup
                        System.Diagnostics.Debug.WriteLine("Try backup pebblejs_storage.xml");
                        List = await Pebble_Time_Manager.Common.LocalStorage.Load("pebblejs_storage.bak");
                    }

                    if (List.Length > 0)
                    {
                        //Make backup of xml storage file
                        Pebble_Time_Manager.Common.LocalStorage.Copy("watchtems.xml", "watchitems.bak");

                        //Deserialize
                        _temp = (LocalStorage)Pebble_Time_Manager.Common.Serializer.Deserialize(List, typeof(LocalStorage));
                        StorageContainer = _temp.StorageContainer;
                    }

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("PebbleJS.LocalStorage.Load exception: {0}", e.Message));
                }
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
                        if (_geoLocator == null) _geoLocator = new Geolocator();
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
            public Pebble(IWatchItem _ParentItem)
            {
                ParentItem = _ParentItem;
            }

            private IWatchItem ParentItem;


            public void addEventListener(String Event, object function)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("addEventListener(Event = {0}, Function = {1})", Event, function.ToString()));

                if (EventListeners.ContainsKey(Event)) EventListeners.Remove(Event);
                EventListeners.Add(Event, function);
            }

            public void openURL(String URL)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("openURL(URL={0})", URL));

                //Fire URL event
                URLEventArgs _uea = new URLEventArgs();
                _uea.URL = URL;
                _uea.WatchItem = ParentItem;
                if (PebbleKitJS.OpenURL != null) PebbleKitJS.OpenURL(this, _uea);
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
        #endregion
    }
}
