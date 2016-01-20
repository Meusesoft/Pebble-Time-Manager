using System;
using System.Collections.Generic;
using System.Text;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;


namespace Tennis_Statistics.Helpers
{
    public class Speech
    {
        #region Constructor

        public Speech()
        {
            m_SpeechSyntesizer = new SpeechSynthesizer();

            SpeechRate = 100;
            SpeechVolume = 100;
        }

        #endregion

        #region Fields

        private SpeechSynthesizer m_SpeechSyntesizer;
        private MediaElement m_AssociatedMediaElement;
        private bool m_Speaking;

        #endregion

        #region Properties

        public MediaElement AssociatedMediaElement
        {
            get
            {
                return m_AssociatedMediaElement;
            }
            set
            {
                m_AssociatedMediaElement = value;
                m_AssociatedMediaElement.MediaEnded += AssociatedMediaElement_MediaEnded;
            }
        }

        public int SpeechRate { get; set; }

        public int SpeechVolume { get; set; }

        public bool Speaking
        {
            get
            {
                return m_Speaking;
            }
        }

        #endregion

        #region Methods

        public async Task SayAsync(String Message)
        {
            if (AssociatedMediaElement == null) throw new Exception("No media element is associated. Message can't be played.");

            m_Speaking = true;

            //Consturct the SSML
            String SSML = "<?xml version=\"1.0\"?><speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd\" xml:lang=\"en-US\">";
            SSML += "<prosody volume=\"" + SpeechVolume + "\" rate=\"" + SpeechRate + "\">";           
            SSML += Message + "</prosody></speak>";

            // Generate the audio stream from plain text.
            SpeechSynthesisStream stream = await m_SpeechSyntesizer.SynthesizeSsmlToStreamAsync(SSML);

            // Send the stream to the media object.
            AssociatedMediaElement.SetSource(stream, stream.ContentType);
            AssociatedMediaElement.Play();
        }

        void AssociatedMediaElement_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            m_Speaking = false;
        }

        #endregion
    }
}
