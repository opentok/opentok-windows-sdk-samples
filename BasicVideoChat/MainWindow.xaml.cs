using OpenTok;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

        [DllImport("opentok", EntryPoint = "otc_log_set_logger_func", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void otc_log_set_logger_func(otc_logger_func logger);

        [DllImport("opentok", EntryPoint = "otc_log_enable", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void otc_log_enable(int level);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void otc_logger_func(string message);

        void EnableLogging()
        {
            otc_logger_func X = (string message) =>
            {
                Console.WriteLine(message);
            };
            otc_log_enable(0x7FFFFFFF);
            otc_log_set_logger_func(X);
        }

        public MainWindow()
        {
            InitializeComponent();

            EnableLogging();


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
