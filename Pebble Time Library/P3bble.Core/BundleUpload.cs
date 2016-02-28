using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P3bble.Types;

namespace P3bble
{
    public enum UploadStatus { eNone, eResource, eBinary, eFinalise, eFinished }

    public class BundleUpload
    {
        #region Constructor

        public BundleUpload(Bundle _Bundle)
        {
            Bundle = _Bundle;

            TransactionId = new List<byte>();
            ResourceId = new List<byte>();
            BinaryId = new List<byte>();

            Status = UploadStatus.eNone;

            Progress = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Bundle being uploaded
        /// </summary>
        public Bundle Bundle { get; set; }

        /// <summary>
        /// Upload status
        /// </summary>
        public UploadStatus Status { get; set; }

        /// <summary>
        /// Progress of current file being uploaded
        /// </summary>
        public long Progress { get; set; }
        
        /// <summary>
        /// Bundle upload ID
        /// </summary>
        public List<byte> TransactionId { get; set; }
        
        /// <summary>
        /// Resource upload ID
        /// </summary>
        public List<byte> ResourceId { get; set; }

        /// <summary>
        /// Binary upload ID
        /// </summary>
        public List<byte> BinaryId { get; set; }

        /// <summary>
        /// End of data; true if the current file is uploaded.
        /// </summary>
        public bool EOD
        {
            get
            {
                bool Result = false;
                
                switch (Status)
                {
                    case UploadStatus.eResource:

                        Result = (Progress == Bundle.Resources.Length);

                        break;

                    case UploadStatus.eBinary:

                        Result = (Progress == Bundle.BinaryContent.Length);

                        break;
                }

                return Result;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add the id to the bundle
        /// </summary>
        /// <param name="Id"></param>
        public void AddFileId(Messages.PutBytesMessage message)
        {
            List<byte> Id = new List<byte>();
            
            for (int i = 0; i < 4; i++)
            {
                Id.Add(message.Result[i + 1]);
            }

            switch (Status)
            {
                case UploadStatus.eResource:
         
                    if (ResourceId.Count == 0) ResourceId = Id;

                    break;

                case UploadStatus.eBinary:

                    if (BinaryId.Count == 0) BinaryId = Id;

                    break;
            }
        }

        /// <summary>
        /// Add a portion of the content to the message
        /// </summary>
        /// <param name="message"></param>
        public void AddContentPortion(Messages.PutBytesMessageV3 message)
        {
            byte[] Content = new byte[0];
            
            switch (Status)
            {
                case UploadStatus.eResource:

                    Content = Bundle.Resources;

                    break;

                case UploadStatus.eBinary:

                    Content = Bundle.BinaryContent;

                    break;
            }

            //Add data to the message
            int j = 0;
            long ProgressStart = Progress;

            while (j < 2000 && Progress < Content.Count())
            {
                message.Data.Add(Content[Progress]);
                Progress++;
                j++;
            }

            System.Diagnostics.Debug.WriteLine(String.Format("<< Bytes send {0} - {1}", ProgressStart, Progress));
        }

        #endregion
    }
}
