using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using OpenTok;

namespace BasicVideoChat
{
    public partial class MainWindow : Window
    {
        private const string API_KEY = "47797141";
        private const string SESSION_ID = "2_MX40Nzc5NzE0MX5-MTcxOTkxMzg2NTc0OH52NWpQQzdNQUdQVEc4VDNhUmdlZ1loOXV-fn4";
        private const string TOKEN = "T1==cGFydG5lcl9pZD00Nzc5NzE0MSZzZGtfdmVyc2lvbj0mc2lnPWQ2MDkzMmZlYzI4MDRlNTk1NmI4ZDgxZWUzNzNmM2E0MWUyMzJiOGE6c2Vzc2lvbl9pZD0yX01YNDBOemM1TnpFME1YNS1NVGN4T1RreE16ZzJOVGMwT0g1Mk5XcFFRemROUVVkUVZFYzRWRE5oVW1kbFoxbG9PWFYtZm40JmNyZWF0ZV90aW1lPTE3MTk5MTM4NjQmZXhwaXJlX3RpbWU9MTcyMjUwNTg2NCZyb2xlPW1vZGVyYXRvciZub25jZT0yYzk2ZDI5YS01ODQwLTQxZTQtYmY2YS00MjE1ZjIyZmQ2OGY=";

        private Context context;
        private Session Session;
        private Publisher Publisher;

        public MainWindow()
        {
            InitializeComponent();

            context = new Context(new WPFDispatcher());

            // Uncomment following line to get debug logging
            // Logger.Enable();

            IList<VideoCapturer.VideoDevice> capturerDevices = VideoCapturer.EnumerateDevices();
            if (capturerDevices == null || capturerDevices.Count == 0)
                throw new Exception("No video capture devices detected");

            Publisher = new Publisher.Builder(context)
            {
                Capturer = capturerDevices[0].CreateVideoCapturer(VideoCapturer.Resolution.High, VideoCapturer.FrameRate.Fps30),
                Renderer = PublisherVideo
            }.Build();

            Session = new Session.Builder(context, API_KEY, SESSION_ID).Build();
            Session.Connected += Session_Connected;
            Session.Disconnected += Session_Disconnected;
            Session.Error += Session_Error;
            Session.StreamReceived += Session_StreamReceived;
            Session.Connect(TOKEN);
        }

        private void Session_Connected(object sender, System.EventArgs e)
        {
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
    }
}
