using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.Text;
using System.IO;
using Windows.Storage;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Tennis_Statistics.Helpers
{
    public class Serializer
    {

        /// <summary>
        /// Convert this instance of an object into a serialized DataContract XML string
        /// </summary>
        /// <returns></returns>
        public static string Serialize(object _instance)
        {
            try
            {
                DataContractSerializerSettings settings = new DataContractSerializerSettings();
                settings.PreserveObjectReferences = true;
                DataContractSerializer writer = new DataContractSerializer(_instance.GetType(), settings);

                System.IO.StringWriter output = new System.IO.StringWriter();
                MemoryStream Stream = new MemoryStream();

                writer.WriteObject(Stream, _instance);

                StreamReader reader = new StreamReader(Stream);
                Stream.Position = 0;

                string text = reader.ReadToEnd();

                return text;
            }
            catch (Exception e)
            {
                return e.InnerException.ToString();
            }
        }

        /// <summary>
        /// Convert this instance of an object into a serialized XML string
        /// </summary>
        /// <returns></returns>
        public static string XMLSerialize(object _instance)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(_instance.GetType());

                System.IO.StringWriter output = new System.IO.StringWriter();

                writer.Serialize(output, _instance);

                return output.ToString();
            }
            catch (Exception e)
            {
                return e.InnerException.ToString();
            }
        }

        /// <summary>
        /// Deserialize the XML string into an instance of the requested type, if possible.
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="_Type"></param>
        /// <returns></returns>
        public static object Deserialize(String XML, Type _Type)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(_Type);
                
                MemoryStream input = new MemoryStream();

                var stringBytes = System.Text.Encoding.UTF8.GetBytes(XML);
                input.Write(stringBytes, 0, stringBytes.Length);
                input.Position = 0;
                
                object _object = serializer.ReadObject(input);

                return _object;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Deserialize the XML string into an instance of the requested type, if possible.
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="_Type"></param>
        /// <returns></returns>
        public static object XMLDeserialize(String XML, Type _Type)
        {
            try
            {
                if (XML.Length == 0) return null;

                XmlSerializer reader = new XmlSerializer(_Type);
                System.IO.StringReader input = new StringReader(XML);
                object _object = reader.Deserialize(input);

                return _object;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Serialize the instance and compress the result
        /// </summary>
        /// <param name="_instance"></param>
        /// <returns></returns>
        public static string SerializeAndCompress(object _instance)
        {
            return Compress(Serialize(_instance));
        }

        /// <summary>
        /// Decompress the string and deserialize its contents in the requested class type
        /// </summary>
        /// <param name="CompressedXML"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        public static object DecompressAndDeserialize(string CompressedXML, Type _type)
        {
            return Deserialize(Decompress(CompressedXML), _type);
        }

        #region Compression and decompression
        
        /// <summary>
        /// Compress the string as a GZIP stream
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Compress(string text)
        {
          //  return text;
            
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        /// <summary>
        /// Decompress the gzip compressed string
        /// </summary>
        /// <param name="compressedText"></param>
        /// <returns></returns>
        public static string Decompress(string compressedText)
        {
            //return compressedText;
            
            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }
        }

        #endregion
    }
}
