using System;
using System.Windows;
using System.Collections.Concurrent;
using OpenTok;

namespace OpenTokWPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public const string API_KEY = "";
        public const string SESSION_ID = "";
        public const string TOKEN = "";

        VideoCapturer capturer;
        Session session;
        Publisher publisher;
        bool Disconnect = false;

        ConcurrentDictionary<Stream, Subscriber> subscriberByStream = new ConcurrentDictionary<Stream, Subscriber>();


        public MainWindow()
        {
            InitializeComponent();

            var devices = VideoCapturer.EnumerateDevices();
            var selectedDevice = devices[0];
            capturer = selectedDevice.CreateVideoCapturer(VideoCapturer.Resolution.High);
            publisher = new Publisher(Context.Instance, renderer: publisherVideo, capturer: capturer);

            //var screenSharing = new ScreenSharingCapturer();
            //publisher = new Publisher(Context.Instance, renderer: publisherVideo, capturer: screenSharing);

            session = new Session(Context.Instance, API_KEY, SESSION_ID);

            session.Connected += Session_Connected;
            session.Disconnected += Session_Disconnected;
            session.Error += Session_Error;
            session.ConnectionCreated += Session_ConnectionCreated;
            session.StreamReceived += Session_StreamReceived;
            session.StreamDropped += Session_StreamDropped;

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var subscriber in subscriberByStream.Values)
            {
                subscriber.Dispose();
            }
            publisher?.Dispose();
            capturer?.Dispose();
            session?.Dispose();
        }

        private void Session_ConnectionCreated(object sender, Session.ConnectionEventArgs e)
        {
            Console.WriteLine("Session connection created:" + e.Connection.Id);            
        }

        private void Session_Error(object sender, Session.ErrorEventArgs e)
        {
            Console.WriteLine("Session error:" + e.ErrorCode);
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Session disconnected");
            subscriberByStream.Clear();

            subscriberGrid.Children.Clear();
        }

        private void Session_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Session connected connection id:" + session.Connection.Id);
            try
            {
                session.Publish(publisher);
            }
            catch (OpenTokException ex)
            {
                Console.WriteLine("OpenTokException " + ex.ToString());
            }
            
        }

        private void UpdateGridSize(int numberOfSubscribers)
        {
            int rows = Convert.ToInt32(Math.Round(Math.Sqrt(numberOfSubscribers)));
            int cols = rows == 0 ? 0 : Convert.ToInt32(Math.Ceiling(((double)numberOfSubscribers) / rows));
            subscriberGrid.Columns = cols;
            subscriberGrid.Rows = rows;
        }

        private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
        {
            Console.WriteLine("Session stream received");
            VideoRenderer renderer = new VideoRenderer();
            subscriberGrid.Children.Add(renderer);
            UpdateGridSize(subscriberGrid.Children.Count);
            Subscriber subscriber = new Subscriber(Context.Instance, e.Stream, renderer);
            subscriberByStream.TryAdd(e.Stream, subscriber);

            try
            {
                session.Subscribe(subscriber);
            }
            catch (OpenTokException ex)
            {
                Console.WriteLine("OpenTokException " + ex.ToString());
            }
        }

        private void Session_StreamDropped(object sender, Session.StreamEventArgs e)
        {
            Console.WriteLine("Session stream dropped");
            var subscriber = subscriberByStream[e.Stream];
            if (subscriber != null)
            {
                Subscriber outsubs;
                subscriberByStream.TryRemove(e.Stream, out outsubs);

                try
                {
                    session.Unsubscribe(subscriber);
                }
                catch (OpenTokException ex)
                {
                    Console.WriteLine("OpenTokException " + ex.ToString());
                }

                subscriberGrid.Children.Remove((UIElement)subscriber.VideoRenderer);
                UpdateGridSize(subscriberGrid.Children.Count);
            }

        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Disconnect)
            {
                Console.WriteLine("Disconnecting session");

                try
                {
                    session.Unpublish(publisher);
                    session.Disconnect();
                }
                catch (OpenTokException ex)
                {
                    Console.WriteLine("OpenTokException " + ex.ToString());
                }
            }
            else
            {
                Console.WriteLine("Connecting session");
                try
                {
                    session.Connect(TOKEN);
                }
                catch (OpenTokException ex)
                {
                    Console.WriteLine("OpenTokException " + ex.ToString());
                }
            }
            Disconnect = !Disconnect;
            button.Content = Disconnect ? "Disconnect" : "Connect";
        }
    }
}
