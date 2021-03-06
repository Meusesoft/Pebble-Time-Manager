﻿using Jint;
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
    /*public class PebbleKitJS
    {
        public PebbleKitJS(IWatchItem ParentItem)
        {

        }

        public async Task Execute(String Javascript)
        {

        }
        public class URLEventArgs : EventArgs
        {
            public String URL;
            public IWatchItem WatchItem;
        }
        public void ShowConfiguration(IWatchItem item)
        {

        }

        public void WebViewClosed(String Data)
        {

        }

        public void Ready()
        {

        }

        public async Task AppMessage(Dictionary<int, object> Content)
        { }

        public delegate void OpenURLEventHandler(object sender, EventArgs e);
        public static event OpenURLEventHandler OpenURL;
    }*/

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
        public  Engine _JintEngine;
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
                var e = new Engine(c => c.AllowClr());
                e.SetValue("setTimeout", new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setTimeout__));
                e.SetValue("clearTimeout", new Action<int>(__clearTimeout__));
                //e.SetValue("setInterval", new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setInterval__));
                //e.SetValue("clearInterval", new Action<int>(__clearInterval__));

                LocalStorage _ls = new LocalStorage(_ParentItem);
                await _ls.Load();

                _JintEngine = new Engine()
                    .SetValue("log", new Action<object>(Debug))
                    .SetValue("localStorage", _ls)
                    .SetValue("console", new Console())
                    .SetValue("Pebble", _Pebble)
                    .SetValue("window", new Window())
                    .SetValue("navigator", new Navigator())
                    .SetValue("setTimeout", new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setTimeout__))
                    .SetValue("clearTimeout", new Action<int>(__clearTimeout__))
                    .SetValue("setInterval", new Func<Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>, double, int>(__setInterval__))
                    .SetValue("clearInterval", new Action<int>(__clearInterval__));

                _JintEngine.SetValue("XMLHttpRequest", TypeReference.CreateTypeReference(_JintEngine, typeof(XMLHttpRequest)));

                _Pebble.JintEngine = _JintEngine;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary> 
        /// https://developer.mozilla.org/en-US/docs/Web/API/Window.setTimeout 
        /// </summary> 
        /// <param name="callBackFunction"></param> 
        /// <param name="delay"></param> 
        /// <returns></returns> 
        private int __setTimeout__(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, double delay)
        {
            _timeoutID++;

            TimeOut _to = new TimeOut();
            _to.Delay = delay;
            _to.Function = callBackFunction;
            _to.ID = _timeoutID;

            TimeOutList.Add(_to);

            if (_dt == null)
            {
                _dt = new Windows.UI.Xaml.DispatcherTimer();
                _dt.Interval = TimeSpan.FromMilliseconds(100);
                _dt.Tick += _dt_Tick;
                _dt.Start();
            }

            return _timeoutID;

            // return this._eventQueue.Enqueue(new CallBackEvent(callBackFunction, delay, CallBackType.TimeOut)).Id; 
        }

        private void _dt_Tick(object sender, object e)
        {
            foreach (var item in TimeOutList)
            {
                item.Delay -= 100;
                if (item.Delay < 0)
                {
                    item.Delay = 1000000;

                    List<JsValue> parameters = new List<JsValue>();
                    JsValue r = item.Function.Invoke( // Call the callback function 
                        JsValue.Undefined,               // Pass this as undefined 
                        parameters.ToArray()             // Pass the parameter data 
                        );
                }
            }
        }

        private Windows.UI.Xaml.DispatcherTimer _dt;
        private int _timeoutID;
        internal class TimeOut
        {
            public int ID;
            public Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> Function;
            public double Delay;
        }

        private List<TimeOut> _TimeOutList;
        private List<TimeOut> TimeOutList
        {
            get
            {
                if (_TimeOutList == null) _TimeOutList = new List<TimeOut>();
                return _TimeOutList;
            }
        }


         /// <summary> 
         /// https://developer.mozilla.org/en-US/docs/Web/API/window.clearTimeout 
         /// </summary> 
         /// <param name="id"></param> 
        private void __clearTimeout__(int id)
        {
            // return this._eventQueue.Enqueue(new CallBackEvent(callBackFunction, delay, CallBackType.TimeOut)).Id; 
            TimeOut _to = TimeOutList.Find(x => x.ID == id);

            if (_to != null)
            {
                TimeOutList.Remove(_to);
            }
        }

        /// <summary> 
        /// https://developer.mozilla.org/en-US/docs/Web/API/Window.setTimeout 
        /// </summary> 
        /// <param name="callBackFunction"></param> 
        /// <param name="delay"></param> 
        /// <returns></returns> 
        private int __setInterval__(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, double delay)
        {
            return -1;

            // return this._eventQueue.Enqueue(new CallBackEvent(callBackFunction, delay, CallBackType.TimeOut)).Id; 
        }

        /// <summary> 
        /// https://developer.mozilla.org/en-US/docs/Web/API/window.clearTimeout 
        /// </summary> 
        /// <param name="id"></param> 
        private void __clearInterval__(int id)
        {
            // return this._eventQueue.Enqueue(new CallBackEvent(callBackFunction, delay, CallBackType.TimeOut)).Id; 
        }

        public async Task Execute(String Javascript)
        {
            try
            {
                if (Javascript == null) return;

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
                if (!_Pebble.EventListeners.ContainsKey("showConfiguration")) return;

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
                if (!_Pebble.EventListeners.ContainsKey("webviewclosed")) return;

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

        public void Ready()
        {
            try
            {
                if (!_Pebble.EventListeners.ContainsKey("ready")) return;

                var jsfunction = _Pebble.EventListeners["ready"];

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

        public async Task AppMessage(Dictionary<int, object> Content)
        {
            try
            {
                if (!_Pebble.EventListeners.ContainsKey("appmessage")) return;

                var jsfunction = _Pebble.EventListeners["appmessage"];

                System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = jsfunction as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                String JSON = "";

                foreach (var item in Content)
                {
                    var keys = from entry in _ParentItem.AppKeys
                               where entry.Value == item.Key 
                               select entry.Key;

                    if (keys.Count() == 1)
                    {
                        if (JSON.Length > 0) JSON += ", ";
                        JSON += String.Format("\"{0}\": ", keys.FirstOrDefault());

                        if (item.Value.GetType() == typeof(string))
                        {
                            JSON += String.Format("\"{0}\"", item.Value);
                        }
                        if (item.Value.GetType() == typeof(int))
                        {
                            JSON += String.Format("{0}", item.Value);
                        }
                        if (item.Value.GetType() == typeof(bool))
                        {
                            JSON += String.Format("{0}", (bool)item.Value ? 1 : 0);
                        }
                    }
                }

                Jint.Native.JsValue A = new JsValue(1);
                Jint.Native.JsValue[] B = new Jint.Native.JsValue[1];

                Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(_JintEngine);
                B[0] = _jsp.Parse(String.Format("{{\"payload\": {{ {0} }} }}", JSON));

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

            public string language
            {
                get
                {
                    return System.Globalization.CultureInfo.CurrentCulture.Name;
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
            public XMLHttpRequest()
            {
                System.Diagnostics.Debug.WriteLine("XMLHttpRequest created");
            }

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

            private string JSONEncode(String value)
            {
                String Result = value;
                Result = Result.Replace("\"", "\\\"");
                Result = Result.Replace("\n", "\\\n");
                Result = Result.Replace("\r", "\\\r");


                return Result;
            }

            public async void send(object data)
            {
                WebResponse _response;

                try
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("XMLHttpRequest.send data={0}", data == null ? "null" : data.ToString()));
                    _httpWebRequest.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586";


                    //_httpWebRequest.Accept = "text/html, application/json";

                   //Accept-Encoding: gzip, deflate
                   //Accept-Language: nl-NL, nl; q=0.5
                   //Cache-Control: no-cache
                   //Connection: Keep-Alive
                   //Host: nominatim.openstreetmap.org
                   //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586


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

                        System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue > _func = jsfunction as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                        String JSON = String.Format("{{\"responseText\":\"{0}\" }}", JSONEncode(Response));
                        Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(new Jint.Engine());
                        Jint.Native.JsValue _eValue = _jsp.Parse(JSON);
                        Jint.Native.JsValue A = _eValue; 
                        Jint.Native.JsValue[] B = new JsValue[1];
                        B[0] = new Jint.Native.JsValue(Response);

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

            public Engine JintEngine { get; set; }


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

            public async void sendAppMessage(ExpandoObject data, object functionAck, object functionNack)
            {
                await sendAppMessage(data);

                System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> _func = functionAck as System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

                Jint.Native.JsValue A = new JsValue(1);
                Jint.Native.JsValue[] B = new Jint.Native.JsValue[1];

                Jint.Native.Json.JsonParser _jsp = new Jint.Native.Json.JsonParser(JintEngine);
                String JSON = "null";
                Jint.Native.JsValue _eValue = _jsp.Parse(JSON);

                B[0] = _eValue;

                Jint.Native.JsValue C = _func.Invoke(A, B);
            }   
                
            public async Task sendAppMessage(ExpandoObject data)
            {
                PebbleConnector _pc = PebbleConnector.GetInstance();

                int newToken = await _pc.Connect(-1);

                try
                {
                    if (_pc.IsConnected)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("sendAppMessage(data={0})", data.ToString()));

                        P3bble.Messages.AppMessage _am = new P3bble.Messages.AppMessage(P3bble.Constants.Endpoint.ApplicationMessage);
                        uint iKey = 0;

                        _am.Content = new Dictionary<int, object>(data.Count());
                        _am.Command = P3bble.Messages.AppCommand.Push;
                        _am.AppUuid = ParentItem.ID;
                        _am.TransactionId = (byte)_pc.GetNextMessageIdentifier();

                        foreach (var element in data)
                        {
                            if (element.Value != null)
                            {
                                if (ParentItem.AppKeys.ContainsKey(element.Key)) {

                                    iKey = (uint)ParentItem.AppKeys[element.Key];

                                    Type VariableType = element.Value.GetType();
                                    System.Diagnostics.Debug.WriteLine(String.Format("  key: {0}-{3}, value: {1}, type: {2}", element.Key, element.Value, VariableType.ToString(), iKey));

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
                                }
                            }
                        }


                        //byte[] package = _am.ToBuffer();
                        //System.Diagnostics.Debug.WriteLine("<< PAYLOAD: " + BitConverter.ToString(package).Replace("-", ":"));

                        await _pc.Pebble._protocol.WriteMessage(_am);
                    }
                }
                catch (Exception exp)
                {
                    System.Diagnostics.Debug.WriteLine(exp.Message);
                }
                finally
                {
                    _pc.Disconnect(newToken);
                }
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

        private class Window
        {
            #region Methods

            public int setTimeout(object function, double milliSeconds)
            {
                return -1;
            }

            public void clearTimeout(int id)
            {

            }

            public int setInterval(object function, double milliSeconds)
            {
                return -1;
            }

            public void clearInterval(int id)
            {

            }
            #endregion
        }
        #endregion
    }
}
