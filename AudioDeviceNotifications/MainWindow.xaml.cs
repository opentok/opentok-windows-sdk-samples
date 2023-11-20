using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using OpenTok;

namespace BasicVideoChat
{
    public partial class MainWindow : Window
    {
        private const string API_KEY = "";
        private const string SESSION_ID = "";
        private const string TOKEN = "";

        private readonly IDispatcher dispatcher = new WPFDispatcher();
        private readonly Context context;
        private readonly MMAudioDevice audioDevice;
        private Session session;
        private Publisher publisher;

        public MainWindow()
        {
            InitializeComponent();

            // Uncomment following line to get debug logging
            Logger.Enable(Logger.Level.Debug);

            context = new Context(dispatcher);

            audioDevice = new MMAudioDevice(dispatcher);
            AudioDevice.SetCustomAudioDevice(context, audioDevice);
            AudioInput.AudioInputEnabledChanged += AudioInput_AudioInputEnabledChanged;
            AudioOutput.Init(this, audioDevice);
            AudioInput.Init(this, audioDevice);

            IList<VideoCapturer.VideoDevice> capturerDevices = VideoCapturer.EnumerateDevices();
            if (capturerDevices == null || capturerDevices.Count == 0)
                throw new Exception("No video capture devices detected");

            publisher = new Publisher.Builder(context)
            {
                Capturer = capturerDevices[0].CreateVideoCapturer(VideoCapturer.Resolution.High, VideoCapturer.FrameRate.Fps30),
                Renderer = PublisherVideo
            }.Build();

            session = new Session.Builder(context, API_KEY, SESSION_ID).Build();
            session.Connected += Session_Connected;
            session.Disconnected += Session_Disconnected;
            session.Error += Session_Error;
            session.StreamReceived += Session_StreamReceived;
            session.Connect(TOKEN);
        }

        private void Session_Connected(object sender, System.EventArgs e)
        {
            session.Publish(publisher);
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
            session.Subscribe(subscriber);
        }

        private void AudioInput_AudioInputEnabledChanged(bool isEnabled)
        {
            if (publisher != null)
                publisher.PublishAudio = isEnabled;
        }
    }
}
