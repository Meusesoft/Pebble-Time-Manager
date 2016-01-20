using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using P3bble.Constants;
using P3bble.Helper;
using P3bble.Types;

namespace P3bble.Messages
{

    public class PutBytesMessageV3 : BaseMessage
    {
        #region Constructors

        public PutBytesMessageV3()
            : base(Endpoint.PutBytes)
        {
            Data = new List<byte>();
            State = PutBytesState.InProgress;
            AddSize = true;
        }

        public PutBytesMessageV3(PutBytesState State)
            : base(Endpoint.PutBytes)
        {
            Data = new List<byte>();
            this.State = State;
            AddSize = true;
        }

        public PutBytesMessageV3(PutBytesState State, List<byte> Id, List<byte> Data)
            : base(Endpoint.PutBytes)
        {
            this.Data = Data;
            this.State = State;
            AddSize = true;
        }
        
        #endregion

        #region Properties

        public List<byte> Data { get; set; }
        public PutBytesState State { get; set; }
        public BundleUpload BundleUpload { get; set; }
        public bool AddSize { get; set; }

        #endregion

        #region Methods

        protected override void AddContentToMessage(List<byte> payload)
        {
            switch (State)
            {
                //Send the size of the file to upload
                case PutBytesState.WaitForToken:

                    #region WaitForToken
                    //WriteMessage("00:0a:be:ef:01:00:00:19:38:84:00:00:00:66").Wait();

                    payload.Add((byte)State);
                
                    //Add the size of the file
                    uint Size = 0;

                    switch (BundleUpload.Status)
                    {
                        case UploadStatus.eResource:

                            Size = (uint)BundleUpload.Bundle.Resources.Length;

                            break;

                        case UploadStatus.eBinary:

                            Size = (uint)BundleUpload.Bundle.BinaryContent.Length;

                            break;
                    }

                    byte[] sizeBytes = BitConverter.GetBytes(Size);
                    if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(sizeBytes);
                        }

                    payload.AddRange(sizeBytes);

                    //Add upload type/resource/binary
                    switch (BundleUpload.Status)
                    {
                        case UploadStatus.eResource:

                            payload.Add(0x84);

                            break;

                        case UploadStatus.eBinary:

                            payload.Add(0x85);

                            break;
                    }

                    //Add transaction
                    List<byte> Transaction = new List<byte>();
                    Transaction.AddRange(BundleUpload.TransactionId);
                    Transaction.Reverse();
                    payload.AddRange(Transaction);

                    #endregion  

                    break;

                //Send a portion of data
                case PutBytesState.InProgress:

                    #region InProgress

                    List<byte> value = new List<byte>();

                    payload.Add((byte)State);

                    switch (BundleUpload.Status)
                    {
                        case UploadStatus.eResource:

                            payload.AddRange(BundleUpload.ResourceId);

                            break;

                        case UploadStatus.eBinary:

                            payload.AddRange(BundleUpload.BinaryId);

                            break;
                    }

                    if (AddSize)
                    {
                        AddWord2Payload(value, Data.Count);
                        value.Reverse();
                        payload.AddRange(value);
                    }

                    payload.AddRange(Data);

                    #endregion

                    break;

                case PutBytesState.Commit:

                    #region Commit

                    //Add state
                    payload.Add((byte)State);
                    
                    //Add Id 
                    byte[] Content = new byte[0];

                    switch (BundleUpload.Status)
                    {
                        case UploadStatus.eResource:

                            payload.AddRange(BundleUpload.ResourceId);
                            Content = BundleUpload.Bundle.Resources;

                            break;

                        case UploadStatus.eBinary:

                            payload.AddRange(BundleUpload.BinaryId);
                            Content = BundleUpload.Bundle.BinaryContent;

                            break;
                    }

                    //Add CRC
                    List<Byte> Buffer = new List<byte>();
                    Buffer.AddRange(Content);
                    uint crc = Helper.Util.Crc32(Buffer);
                    byte[] crcBytes = BitConverter.GetBytes(crc);
                    System.Diagnostics.Debug.WriteLine(string.Format("Sending CRC of {0:X}", crc));
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(crcBytes);
                    }

                    payload.AddRange(crcBytes);

                    //Move bundle to next file
                    BundleUpload.Progress = 0;
                    BundleUpload.Status = UploadStatus.eNone;
                        

                    #endregion

                    break;

                case PutBytesState.Complete:

                    #region Complete

                    //Add state
                    payload.Add((byte)State);

                    //Add Id
                    if (BundleUpload.ResourceId.Count > 0 ) 
                    {
                        payload.AddRange(BundleUpload.ResourceId);
                        BundleUpload.ResourceId.Clear();
                    }
                    if (BundleUpload.BinaryId.Count > 0)
                    {
                        payload.AddRange(BundleUpload.BinaryId);
                        BundleUpload.BinaryId.Clear();
                    }

                    #endregion

                    break;
            }
        }

        protected override void GetContentFromMessage(List<byte> payload)
        {





        }

        #endregion
    }
}
