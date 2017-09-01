using OpenTok;
using System;
using System.Windows;

namespace BasicVideoChat
{
    public partial class MainWindow : Window
    {
        public const string API_KEY = "472032"; 
        public const string SESSION_ID = "2_MX40NzIwMzJ-fjE1MDQyMjY5Njg0Njd-YUxKZXNNUm11eVA3Vms0SU94cW40dFFPfn4"; 
        public const string TOKEN = "T1==cGFydG5lcl9pZD00NzIwMzImc2RrX3ZlcnNpb249ZGVidWdnZXImc2lnPThiZWZlOGI3MTQ5OGQwZmY3OWZlNzdhODVhZDU0ZjJiMjIyZWEwMTI6c2Vzc2lvbl9pZD0yX01YNDBOekl3TXpKLWZqRTFNRFF5TWpZNU5qZzBOamQtWVV4S1pYTk5VbTExZVZBM1ZtczBTVTk0Y1c0MGRGRlBmbjQmY3JlYXRlX3RpbWU9MTUwNDIyNjk2OCZyb2xlPW1vZGVyYXRvciZub25jZT0xNTA0MjI2OTY4LjQ5Mzc2MzQ3MTc5MTcmZXhwaXJlX3RpbWU9MTUwNjgxODk2OA==";
         
        Session Session;
        VideoCapturer Capturer;
        Publisher Publisher;

        public MainWindow()
        {
            InitializeComponent();

            var devices = VideoCapturer.EnumerateDevices();

            if (devices.Count > 0)
            {
                var selectedDevice = devices[0];
                Capturer = selectedDevice.CreateVideoCapturer(VideoCapturer.Resolution.High);
            }
            else
            {
                Console.WriteLine("Warning: no cameras available, the publisher will be audio only.");
            }

            Publisher = new Publisher(Context.Instance, renderer: PublisherVideo, capturer: Capturer);

            Session = new Session(Context.Instance, API_KEY, SESSION_ID);
            Session.Connected += Session_Connected;
            Session.Disconnected += Session_Disconnected;
            Session.Error += Session_Error;
            Session.StreamReceived += Session_StreamReceived;

            Session.Connect(TOKEN);
        }

        private void Session_Connected(object sender, System.EventArgs e)
        {
            try
            {
                Session.Publish(Publisher);
            }
            catch (OpenTokException ex)
            {
                Console.WriteLine("Session.Publish() exception " + ex.ToString());
            }
        }
        private void Session_Disconnected(object sender, System.EventArgs e)
        {
            Console.WriteLine("Session disconnected.");
        }

        private void Session_Error(object sender, Session.ErrorEventArgs e)
        {
            Console.WriteLine("Session error:" + e.ErrorCode);
        }

        private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
        {
            Subscriber subscriber = new Subscriber(Context.Instance, e.Stream, SubscriberVideo);
            Session.Subscribe(subscriber);
        }

    }
}
