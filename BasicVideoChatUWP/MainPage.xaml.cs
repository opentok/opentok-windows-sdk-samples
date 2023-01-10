using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Serialization;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OpenTok;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BasicVideoChatUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static string API_KEY = "47446341";
        private static string SESSION_ID = "2_MX40NzQ0NjM0MX5-MTY3MzI1MzQ1NzkxMX41RWg0RVNoWXU0S3ZIOWVJV0NxVnBWcHN-fn4";
        private static string TOKEN = "T1==cGFydG5lcl9pZD00NzQ0NjM0MSZzaWc9MzJmMjdhMGRmNDk2MjliZWJlM2FmNDU5YTcwNzQyNzc0ZGVkNDI5MjpzZXNzaW9uX2lkPTJfTVg0ME56UTBOak0wTVg1LU1UWTNNekkxTXpRMU56a3hNWDQxUldnMFJWTm9XWFUwUzNaSU9XVkpWME54Vm5CV2NITi1mbjQmY3JlYXRlX3RpbWU9MTY3MzI1MzQ5NSZub25jZT0wLjU3NTg1NzIyMzAxMTgwODImcm9sZT1wdWJsaXNoZXImZXhwaXJlX3RpbWU9MTY3NTg0NTQ5NCZpbml0aWFsX2xheW91dF9jbGFzc19saXN0PQ==";

        private const VideoCapturer.Resolution DEFAULT_RESOLUTION = VideoCapturer.Resolution.High;
        private const VideoCapturer.FrameRate DEFAULT_FRAME_RATE = VideoCapturer.FrameRate.Fps30;

        private readonly ConcurrentDictionary<Stream, Subscriber> subscriberByStream = new ConcurrentDictionary<Stream, Subscriber>();
        private readonly Context context;
        private Session session;
        private Publisher publisher;
        private bool isConnected = false;

        public MainPage()
        {
            InitializeComponent();

            context = new Context(new UWPDispatcher(this));

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(API_KEY) || string.IsNullOrWhiteSpace(SESSION_ID))
            {
                throw new Exception("ApiKey, SessionId and Token parameters must be provided inside .config file");
            }
            session = new Session.Builder(context, API_KEY, SESSION_ID).Build();

            session.Connected += Session_Connected;
            session.Disconnected += Session_Disconnected;
            session.Error += Session_Error;
            session.ConnectionCreated += Session_ConnectionCreated;
            session.StreamReceived += Session_StreamReceived;
            session.StreamDropped += Session_StreamDropped;

            InitAsync();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (Subscriber subscriber in subscriberByStream.Values)
            {
                subscriber.Dispose();
            }
            if (publisher != null)
            {
                publisher.VideoCapturer.Destroy();
                publisher.Dispose();
                session?.Dispose();
            }
        }

        private async void InitAsync()
        {
            _ = await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures-app"));
            MessageDialog requestPermissionDialog = new MessageDialog("Before proceeding, verify Cam and Mic permissions are granted to the application and then click OK to continue");
            UICommand okCommand = new UICommand("OK");
            requestPermissionDialog.Commands.Add(okCommand);
            requestPermissionDialog.DefaultCommandIndex = 0;
            _ = await requestPermissionDialog.ShowAsync();

            ConfigureAudioDevice();
            RebuildPublisher();
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

        private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
        {
            Console.WriteLine("Session stream received");
            Subscriber subscriber = new Subscriber.Builder(context, e.Stream)
            {
                //Renderer = renderer
                Renderer = subscriberVideo
            }.Build();
            _ = subscriberByStream.TryAdd(e.Stream, subscriber);

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
            Subscriber subscriber = subscriberByStream[e.Stream];
            if (subscriber != null)
            {
                _ = subscriberByStream.TryRemove(e.Stream, out _);

                try
                {
                    session.Unsubscribe(subscriber);
                }
                catch (OpenTokException ex)
                {
                    Console.WriteLine("OpenTokException " + ex.ToString());
                }
            }

        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected)
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
                    if (string.IsNullOrWhiteSpace(TOKEN))
                    {
                        throw new Exception("ApiKey, SessionId and Token parameters must be provided inside .config file");
                    }
                    session.Connect(TOKEN);
                }
                catch (OpenTokException ex)
                {
                    Console.WriteLine("OpenTokException " + ex.ToString());
                }
            }
            isConnected = !isConnected;
            ConnectButton.Content = isConnected ? "Disconnect" : "Connect";
        }

        private void RebuildPublisher()
        {
            if (publisher != null)
            {
                publisher.Dispose();
            }

            try
            {
                IList<VideoCapturer.VideoDevice> availableCameras = VideoCapturer.EnumerateDevices();
                if (availableCameras == null || availableCameras.Count == 0)
                {
                    throw new Exception("No cameras detected");
                }

                VideoCapturer.VideoDevice currentCamera = availableCameras[0];
                VideoCapturer capturer = currentCamera.CreateVideoCapturer(DEFAULT_RESOLUTION, DEFAULT_FRAME_RATE);
                publisher = new Publisher.Builder(context)
                {
                    Renderer = publisherVideo,
                    Capturer = capturer
                }.Build();
            }
            catch (OpenTokException ex)
            {
                Console.WriteLine("OpenTokException " + ex.ToString());
            }

            if (isConnected)
            {
                session.Publish(publisher);
            }
        }

        private void ConfigureAudioDevice()
        {
            IList<AudioDevice.InputAudioDevice> availableMics = AudioDevice.EnumerateInputAudioDevices();
            if (availableMics == null || availableMics.Count == 0)
            {
                Console.WriteLine("No mics detected");
                return;
            }
            AudioDevice.SetInputAudioDevice(availableMics[0]);
        }
    }
}