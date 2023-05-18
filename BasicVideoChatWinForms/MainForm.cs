using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTok;

using System.Threading.Tasks;

namespace BasicVideoChatWinForms
{
    public partial class MainForm : Form
    {
        private const string API_KEY = "47446341";
        private const string SESSION_ID = "2_MX40NzQ0NjM0MX5-MTY4MjQ5MTc1ODg2Mn54MXFJaUdmcVZwK0RtakdRRERxN25ydHZ-fn4";
        private const string TOKEN = "T1==cGFydG5lcl9pZD00NzQ0NjM0MSZzaWc9ZWE5NDAyMzVjMzU2ODdiMGU0NGRhN2MyY2JhMDlmYWVjNzM0Mjc3YzpzZXNzaW9uX2lkPTJfTVg0ME56UTBOak0wTVg1LU1UWTRNalE1TVRjMU9EZzJNbjU0TVhGSmFVZG1jVlp3SzBSdGFrZFJSRVJ4TjI1eWRIWi1mbjQmY3JlYXRlX3RpbWU9MTY4MjQ5MTc5MiZub25jZT0wLjAxNDE3NTk2NzM1MTIzNjk0JnJvbGU9cHVibGlzaGVyJmV4cGlyZV90aW1lPTE2ODUwODM3OTImaW5pdGlhbF9sYXlvdXRfY2xhc3NfbGlzdD0=";

        public MainForm()
        {
            InitializeComponent();

            context = new Context(new WinFormsDispatcher(this));            

            IList<VideoCapturer.VideoDevice> capturerDevices = VideoCapturer.EnumerateDevices();
            if (capturerDevices == null || capturerDevices.Count == 0)
                throw new Exception("No video capture devices detected");

            Publisher = new Publisher.Builder(context)
            {
                Capturer = capturerDevices[0].CreateVideoCapturer(VideoCapturer.Resolution.High, VideoCapturer.FrameRate.Fps30),
                Renderer = PublisherVideo
            }.Build();

            IList<AudioDevice.InputAudioDevice> availableMics = AudioDevice.EnumerateInputAudioDevices();
            if (availableMics == null || availableMics.Count == 0)
                throw new Exception("No audio capture devices detected");
            AudioDevice.SetInputAudioDevice(availableMics[0]);

            Session = new Session.Builder(context, API_KEY, SESSION_ID).Build();
            Session.Connected += Session_Connected;
            Session.Disconnected += Session_Disconnected;
            Session.Error += Session_Error;
            Session.StreamReceived += Session_StreamReceived;

            Session.Connect(TOKEN);         
        }

        #region Private

        private Context context;
        private Session Session;
        private Publisher Publisher;

        private void Session_Connected(object sender, System.EventArgs e)
        {
            Trace.WriteLine("Session connected.");
            Session.Publish(Publisher);
        }

        private void Session_Disconnected(object sender, System.EventArgs e)
        {
            Trace.WriteLine("Session disconnected.");
        }

        private void Session_Error(object sender, Session.ErrorEventArgs e)
        {
            Trace.WriteLine("Session error:" + e.ErrorCode);
        }

        private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
        {
            Subscriber subscriber = new Subscriber.Builder(context, e.Stream)
            {
                Renderer = SubscriberVideo
            }.Build();
            Session.Subscribe(subscriber);
        }

        #endregion
    }
}
