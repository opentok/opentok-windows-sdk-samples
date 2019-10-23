using OpenTok;
using System;
using System.Diagnostics;
using System.Windows;

namespace BasicVideoChat
{
    public partial class MainWindow : Window
    {
        public const string API_KEY = "";
        public const string SESSION_ID = ""; 
        public const string TOKEN = "";

        Session Session;
        Publisher Publisher;

        

        public MainWindow()
        {
            InitializeComponent();

            // Uncomment following line to get debug logging
            // LogUtil.Instance.EnableLogging();

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
            Trace.WriteLine("Session disconnected.");
        }

        private void Session_Error(object sender, Session.ErrorEventArgs e)
        {
            Trace.WriteLine("Session error:" + e.ErrorCode);
        }

        private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
        {
            Subscriber subscriber = new Subscriber(Context.Instance, e.Stream, SubscriberVideo);
            Session.Subscribe(subscriber);
        }
    }
}
