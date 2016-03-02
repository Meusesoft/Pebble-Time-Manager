using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P3bble.Constants;

namespace P3bble.Messages
{
    /// <summary>
    /// Application command
    /// </summary>
    public enum AppCommand : byte
    {
        /// <summary>
        /// The push response
        /// </summary>
        Push = 1,

        /// <summary>
        /// The request response
        /// </summary>
        Request = 2,

        /// <summary>
        /// The finalise install command
        /// </summary>
        FinaliseInstall = 3,

        /// <summary>
        /// The ack response
        /// </summary>
        Ack = 0xff,

        /// <summary>
        /// The nack response
        /// </summary>
        Nack = 0x7f
    }

    /// <summary>
    /// Application Message Data Types
    /// </summary>
    public enum AppMessageTupleDataType : byte
    {
        /// <summary>
        /// A byte array
        /// </summary>
        ByteArray = 0,

        /// <summary>
        /// A string
        /// </summary>
        String = 1,

        /// <summary>
        /// An unsigned integer
        /// </summary>
        UInt = 2,

        /// <summary>
        /// An integer
        /// </summary>
        Int = 3
    }

    /// <summary>
    /// Launcher keys
    /// </summary>
    internal enum LauncherKeys : byte
    {
        /// <summary>
        /// The run state key
        /// </summary>
        RunState = 1
    }

    /// <summary>
    /// Application launch param
    /// </summary>
    internal enum LauncherParams : byte
    {
        /// <summary>
        /// The not running state
        /// </summary>
        NotRunning = 0,

        /// <summary>
        /// The running state
        /// </summary>
        Running = 1
    }

    public class AppMessage : P3bbleMessage
    {
        internal const byte RunState = 1;

        private List<byte[]> _tuples = new List<byte[]>();
        
        public AppMessage()
            : this(Endpoint.ApplicationMessage)
        {
        }

        public AppMessage(Endpoint messageType)
            : base(messageType)
        {
        }

        public Guid AppUuid { get; set; }

        public AppCommand Command { get; set; }

        public uint AppIndex { get; set; }

        public byte TransactionId { get; set; }

        public Dictionary<int, object> Content { get; set; }

        /// <summary>
        /// Gets or sets the remaining response.
        /// </summary>
        /// <value>
        /// The remaining response.
        /// </value>
        /// <remarks>Not expecting this to get used</remarks>
        private byte[] RemainingResponse { get; set; }

        public List<byte> Response { get; set; }

        public void AddTuple(uint key, AppMessageTupleDataType dataType, byte data)
        {
            this.AddTuple(key, dataType, new byte[] { data });
        }
        
        public void AddTuple(uint key, AppMessageTupleDataType dataType, byte[] data)
        {
            List<byte> result = new List<byte>();

            byte[] keyBytes = BitConverter.GetBytes(key);
            byte[] lengthBytes = BitConverter.GetBytes((short)data.Length);
            
            result.AddRange(keyBytes);
            result.Add((byte)dataType);
            result.AddRange(lengthBytes);
            result.AddRange(data);

            this._tuples.Add(result.ToArray());
        }

        protected override void AddContentToMessage(List<byte> payload)
        {
            // Add the command
            payload.Add((byte)this.Command);

            if (this.Command == AppCommand.FinaliseInstall)
            {
                byte[] indexBytes = BitConverter.GetBytes(this.AppIndex);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(indexBytes);
                }

                payload.AddRange(indexBytes);
            }
            if (this.Command == AppCommand.Ack)
            {
                payload.Add(TransactionId);
            }
            else
            {
                List<byte> data = new List<byte>();

                // Add a transaction id:
                data.Add(TransactionId);

                // Add the app id:
                data.AddRange(this.AppUuid.ToByteArray());

                if (Content == null)
                {
                    // Add the actual data to send - first the count...
                    data.Add((byte)(this._tuples.Count * 4));

                    // Now the tuples...
                    foreach (var tuple in this._tuples)
                    {
                        data.AddRange(tuple);
                    }
                }
                else
                {
                    // Add the data from the content dictionary
                    data.AddRange(SetContentDictionary(Content));
                }

                payload.AddRange(data);
            }
        }

        protected override void GetContentFromMessage(List<byte> payload)
        {
            try
            {
                if (payload.Count > 0)
                {
                    switch (payload[0])
                    {
                        case (byte)AppCommand.Push:

                            this.Command = (AppCommand)payload[0];

                            this.RemainingResponse = new byte[payload.Count - 1];
                            Response = new List<byte>();

                            //Get message index
                            TransactionId = payload[1];

                            //Get AppUuid
                            byte[] GuidApp = new byte[16];
                            payload.CopyTo(2, GuidApp, 0, 16);

                            AppUuid = new Guid(GuidApp);

                            //Get response
                            Response = payload.GetRange(18, payload.Count - 18);

                            //Process the response
                            Content = GetContentDictionary(Response);

                            break;

                        case (byte)AppCommand.Request:
                        case (byte)AppCommand.Ack:
                        case (byte)AppCommand.Nack:

                            this.Command = (AppCommand)payload[0];
                            break;

                        default:
                            break;
                    }

                    if (payload.Count > 1)
                    {
                        this.RemainingResponse = new byte[payload.Count - 1];
                        payload.CopyTo(1, this.RemainingResponse, 0, payload.Count - 1);

                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("AppMessage::GetContentFromMessage: {0}", e.Message));
            }
        }

        /// <summary>
        /// Process the payload and convert to the content dictionary
        /// </summary>
        protected Dictionary<int, object> GetContentDictionary(List<byte> Payload)
        {
            //Process content
            Dictionary<int, object> Result = new Dictionary<int, object>();

            int Elements = Payload[0];
            Payload.RemoveRange(0, 1);

            for (int i = 0; i < Elements; i++)
            {
                //get key
                byte[] part = Payload.GetRange(0, 4).ToArray();
                int Key = BitConverter.ToInt32(part, 0);
                Payload.RemoveRange(0, 4);

                //get type 
                byte type = Payload[0];

                switch (type)
                {
                    case 0x03: //int

                        byte[] biLength = Payload.GetRange(1, 2).ToArray();
                        int iLength = BitConverter.ToInt16(biLength, 0);

                        byte[] bValue = Payload.GetRange(3, iLength).ToArray();
                        int iValue = BitConverter.ToInt32(bValue, 0);

                        Payload.RemoveRange(0, iLength + 3);

                        Result.Add(Key, iValue);

                        break;


                    case 0x01: //string

                        byte[] bsLength = Payload.GetRange(1, 2).ToArray();
                        int sLength = BitConverter.ToInt16(bsLength, 0);

                        byte[] sContent = Payload.GetRange(3, sLength).ToArray();
                        string sValue = System.Text.Encoding.UTF8.GetString(sContent, 0, sContent.Length - 1);

                        Payload.RemoveRange(0, sLength + 3);

                        Result.Add(Key, sValue);

                        break;
                }
            }

            return Result;
        }

        protected List<byte> SetContentDictionary(Dictionary<int, object> Content)
        {
            List<byte> Result = new List<byte>();

            //Add the number of elements
            Result.Add((byte)Content.Count);

            //Add all the elements to the payload
            for (int i=0; i<Content.Count; i++)
            {
                byte[] bKeys = BitConverter.GetBytes(Content.Keys.ElementAt(i));
                Result.AddRange(bKeys);

                object value = Content.Values.ElementAt(i);

                switch (value.GetType().ToString())
                {
                    case "System.Int32":

                        Result.Add(0x03);

                        Int16 iLength = 4;
                        byte[] biValueLength = BitConverter.GetBytes(iLength);

                        Result.AddRange(biValueLength);

                        byte[] biValue = BitConverter.GetBytes((int)value);
                        Result.AddRange(biValue);

                        break;

                    case "System.String":

                        Result.Add(0x01);

                        string sValue = (string)value;
                        Int16 sLength = (short)(sValue.Length + 1);
                        byte[] bsValueLength = BitConverter.GetBytes(sLength);

                        Result.AddRange(bsValueLength);

                        byte[] bsValue = System.Text.Encoding.UTF8.GetBytes(sValue);
                        Result.AddRange(bsValue);
                        Result.Add(0x00);

                        break;

                    default:

                        System.Diagnostics.Debug.WriteLine("SetContentDictionary: Type not implemented.");
                        throw new Exception("SetContentDictionary: Type not implemented.");

                        break;
                }
            }

            return Result;
        }
    }
}
