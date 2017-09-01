using OpenTok;
using System;
using System.Windows;

namespace BasicVideoChat
{
    public partial class MainWindow : Window
    {
        public const string API_KEY = "472032"; 
        public const string SESSION_ID = "2_MX40NzIwMzJ-fjE1MDQyOTIzMjMzMjh-M0w0NkVuSDlTN2JQdUEwUWV1akJJZGx2fn4"; 
        public const string TOKEN = "T1==cGFydG5lcl9pZD00NzIwMzImc2RrX3ZlcnNpb249ZGVidWdnZXImc2lnPWVhMjdhYTlhMzEyNzY0NTE2MmQ2MDc0ZTM5MWQzNWU2MjcxMGM3Yzk6c2Vzc2lvbl9pZD0yX01YNDBOekl3TXpKLWZqRTFNRFF5T1RJek1qTXpNamgtTTB3ME5rVnVTRGxUTjJKUWRVRXdVV1YxYWtKSlpHeDJmbjQmY3JlYXRlX3RpbWU9MTUwNDI5MjMyMyZyb2xlPW1vZGVyYXRvciZub25jZT0xNTA0MjkyMzIzLjM1NDE5MjYzMjE1OTMmZXhwaXJlX3RpbWU9MTUwNjg4NDMyMyZjb25uZWN0aW9uX2RhdGE9SmVmZg==";
         
        Session Session;
        Publisher Publisher;

        public MainWindow()
        {
            InitializeComponent();

            Publisher = new Publisher(Context.Instance, renderer: PublisherVideo);

            Session = new Session(Context.Instance, API_KEY, SESSION_ID);
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
