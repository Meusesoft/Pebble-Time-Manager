using System;
using System.Collections.Generic;
using P3bble.Constants;
using P3bble.Helper;
using P3bble.Messages;

namespace P3bble.Messages
{
    /// <summary>
    /// The base message
    /// </summary>
    public abstract class BaseMessage : P3bbleMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="P3bbleMessage"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        public BaseMessage(Endpoint endpoint) : 
            base(endpoint)
        {

        }

        /// <summary>
        /// Add String to the payload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        protected void AddString2Payload(List<byte> payload, String value)
        {
            String _value = RemoveSpecialChars(value);
            _value = _value.Substring(0, Math.Min(_value.Length, 1024));
            AddStringLength2Payload(payload, _value);
            byte[] _bytes = System.Text.Encoding.UTF8.GetBytes(_value);
            payload.AddRange(_bytes);
        }

        /// <summary>
        /// Add value to payload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        protected void AddInteger2Payload(List<byte> payload, int value)
        {
            byte[] len = BitConverter.GetBytes((Int16)value);
            payload.AddRange(len);
        }

        /// <summary>
        /// Add value to payload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        protected void AddWord2Payload(List<byte> payload, int value)
        {
            byte[] len = BitConverter.GetBytes((Int32)value);
            
            payload.AddRange(len);
        }
        
        /// <summary>
        /// Add value to payload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        protected void InsertInteger2Payload(List<byte> payload, int index, int value)
        {
            byte[] len = BitConverter.GetBytes((Int16)value);
            payload.InsertRange(index, len);
        }

        /// <summary>
        /// Add value to payload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        protected void InsertReverseInteger2Payload(List<byte> payload, int index, int value)
        {
            byte[] len = BitConverter.GetBytes((Int16)value);
            payload.Insert(index, len[0]);
            payload.Insert(index, len[1]);
        }

        /// <summary>
        /// Add string length to payload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        protected void AddStringLength2Payload(List<byte> payload, String value)
        {
            AddInteger2Payload(payload, value.Length);
        }

        /// <summary>
        /// Check if the given character is a special character
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected bool IsSpecialChar(char c)
        {
            if (c > 0x7F) return true;
            return false;
        }

        /// <summary>
        /// Remove the special characters from the string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected string RemoveSpecialChars(string s)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var cur in s)
            {
                if (!IsSpecialChar(cur))
                {
                    builder.Append(cur);
                }
            }
            return builder.ToString();
        }



        /// <summary>
        /// Add String to the payload
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        protected void AddStringOnly2Payload(List<byte> payload, String value)
        {
            String _value = RemoveSpecialChars(value);
            byte[] _bytes = System.Text.Encoding.UTF8.GetBytes(_value);
            payload.AddRange(_bytes);
        }

        public static string ConvertToHex(Int32 iValue)
        {

            Int32 iDuration = iValue;
            byte[] bytes = BitConverter.GetBytes(iValue);
            return bytes[0].ToString("X2") + ":" + bytes[1].ToString("X2");
        }

        public static string ConvertToHexReverse(Int32 iValue)
        {

            Int32 iDuration = iValue;
            byte[] bytes = BitConverter.GetBytes(iValue);
            return bytes[1].ToString("X2") + ":" + bytes[0].ToString("X2");
        }
    }
}
