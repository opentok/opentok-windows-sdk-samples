using OpenTok;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace CustomVideoRenderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string API_KEY = "47446341";
        private const string SESSION_ID = "2_MX40NzQ0NjM0MX5-MTY4MjQ5MTc1ODg2Mn54MXFJaUdmcVZwK0RtakdRRERxN25ydHZ-fn4";
        private const string TOKEN = "T1==cGFydG5lcl9pZD00NzQ0NjM0MSZzaWc9ZWE5NDAyMzVjMzU2ODdiMGU0NGRhN2MyY2JhMDlmYWVjNzM0Mjc3YzpzZXNzaW9uX2lkPTJfTVg0ME56UTBOak0wTVg1LU1UWTRNalE1TVRjMU9EZzJNbjU0TVhGSmFVZG1jVlp3SzBSdGFrZFJSRVJ4TjI1eWRIWi1mbjQmY3JlYXRlX3RpbWU9MTY4MjQ5MTc5MiZub25jZT0wLjAxNDE3NTk2NzM1MTIzNjk0JnJvbGU9cHVibGlzaGVyJmV4cGlyZV90aW1lPTE2ODUwODM3OTImaW5pdGlhbF9sYXlvdXRfY2xhc3NfbGlzdD0=";


        VideoCapturer Capturer;
        Session Session;
        Publisher Publisher;
        bool Disconnect = false;
        Dictionary<Stream, Subscriber> SubscriberByStream = new Dictionary<Stream, Subscriber>();

        public MainWindow()
        {
            InitializeComponent();

            // This shows how to enumarate the available capturer devices on the system to allow the user of the app
            // to select the desired camera. If a capturer is not provided in the publisher constructor the first available 
            // camera will be used.
            var devices = VideoCapturer.EnumerateDevices();
            if (devices.Count > 0)
            {
                var selectedDevice = devices[0];
                Trace.WriteLine("Using camera: " + devices[0].Name);
                Capturer = selectedDevice.CreateVideoCapturer(VideoCapturer.Resolution.High);
            }
            else
            {
                Trace.WriteLine("Warning: no cameras available, the publisher will be audio only.");
            }

            // We create the publisher here to show the preview when application starts
            // Please note that the PublisherVideo component is added in the xaml file
            Publisher = new Publisher.Builder(Context.Instance)
            {
                Renderer = PublisherVideo,
                Capturer = Capturer
            }.Build();

            if (API_KEY == "" || SESSION_ID == "" || TOKEN == "")
            {
                MessageBox.Show("Please fill out the API_KEY, SESSION_ID and TOKEN variables in the source code " +
                    "in order to connect to the session", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConnectDisconnectButton.IsEnabled = false;
            }
            else
            {
                Session = new Session.Builder(Context.Instance, API_KEY, SESSION_ID).Build();

                Session.Connected += Session_Connected;
                Session.Disconnected += Session_Disconnected;
                Session.Error += Session_Error;
                Session.StreamReceived += Session_StreamReceived;
                Session.StreamDropped += Session_StreamDropped;
            }

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var subscriber in SubscriberByStream.Values)
            {
                subscriber.Dispose();
            }
            Publisher?.Dispose();
            Capturer?.Dispose();
            Session?.Dispose();
        }

        private void Session_Connected(object sender, EventArgs e)
        {
            try
            {
                Session.Publish(Publisher);
            }
            catch (OpenTokException ex)
            {
                Trace.WriteLine("OpenTokException " + ex.ToString());
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            Trace.WriteLine("Session disconnected");
            SubscriberByStream.Clear();
            SubscriberGrid.Children.Clear();
        }

        private void Session_Error(object sender, Session.ErrorEventArgs e)
        {
            MessageBox.Show("Session error:" + e.ErrorCode, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void UpdateGridSize(int numberOfSubscribers)
        {
            int rows = Convert.ToInt32(Math.Round(Math.Sqrt(numberOfSubscribers)));
            int cols = rows == 0 ? 0 : Convert.ToInt32(Math.Ceiling(((double)numberOfSubscribers) / rows));
            SubscriberGrid.Columns = cols;
            SubscriberGrid.Rows = rows;
        }

        private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
        {
            Trace.WriteLine("Session stream received");

            SampleVideoRenderer renderer = new SampleVideoRenderer();
            renderer.EnableBlueFilter = PublisherVideo.EnableBlueFilter;
            renderer.EnableWatermark = PublisherVideo.EnableWatermark;

            SubscriberGrid.Children.Add(renderer);
            UpdateGridSize(SubscriberGrid.Children.Count);
            Subscriber subscriber = new Subscriber.Builder(Context.Instance, e.Stream)
            {
                Renderer = renderer
            }.Build();
            SubscriberByStream.Add(e.Stream, subscriber);

            try
            {
                Session.Subscribe(subscriber);
            }
            catch (OpenTokException ex)
            {
                Trace.WriteLine("OpenTokException " + ex.ToString());
            }
        }

        private void Session_StreamDropped(object sender, Session.StreamEventArgs e)
        {
            Trace.WriteLine("Session stream dropped");
            var subscriber = SubscriberByStream[e.Stream];
            if (subscriber != null)
            {
                SubscriberByStream.Remove(e.Stream);
                try
                {
                    Session.Unsubscribe(subscriber);
                }
                catch (OpenTokException ex)
                {
                    Trace.WriteLine("OpenTokException " + ex.ToString());
                }

                SubscriberGrid.Children.Remove((UIElement)subscriber.VideoRenderer);
                UpdateGridSize(SubscriberGrid.Children.Count);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Disconnect)
            {
                Trace.WriteLine("Disconnecting session");
                try
                {
                    Session.Unpublish(Publisher);
                    Session.Disconnect();
                }
                catch (OpenTokException ex)
                {
                    Trace.WriteLine("OpenTokException " + ex.ToString());
                }
            }
            else
            {
                Trace.WriteLine("Connecting session");
                try
                {
                    Session.Connect(TOKEN);
                }
                catch (OpenTokException ex)
                {
                    Trace.WriteLine("OpenTokException " + ex.ToString());
                }
            }
            Disconnect = !Disconnect;
            ConnectDisconnectButton.Content = Disconnect ? "Disconnect" : "Connect";
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            PublisherVideo.EnableBlueFilter = !PublisherVideo.EnableBlueFilter;
            foreach (var subscriber in SubscriberByStream.Values)
            {
                ((SampleVideoRenderer)subscriber.VideoRenderer).EnableBlueFilter = PublisherVideo.EnableBlueFilter;
            }
        }

        private void WatermarkButton_Click(object sender, RoutedEventArgs e)
        {
            PublisherVideo.EnableWatermark = !PublisherVideo.EnableWatermark;
            foreach (var subscriber in SubscriberByStream.Values)
            {
                ((SampleVideoRenderer)subscriber.VideoRenderer).EnableWatermark = PublisherVideo.EnableWatermark;
            }
        }
    }
}
