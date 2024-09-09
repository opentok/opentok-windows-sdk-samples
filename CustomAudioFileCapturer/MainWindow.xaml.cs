using System;
using System.Diagnostics;
using System.Windows;
using OpenTok;

namespace CustomAudioFileCapturer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string API_KEY = "";
        private const string SESSION_ID = "";
        private const string TOKEN = "";

        private Context context;
        private Session session;
        private Publisher publisher;
        private CustomCapturerAudioDevice customCapturerAudioDevice = new CustomCapturerAudioDevice();

        public MainWindow()
        {
            InitializeComponent();

            context = new Context(new WPFDispatcher());

            AudioDevice.SetCustomAudioDevice(context, customCapturerAudioDevice);
            customCapturerAudioDevice.StatusChanged += FileRecordingAudioDevice_StatusChanged;

            // Uncomment following line to get debug logging
            //Logger.Enable();

            publisher = new Publisher.Builder(context)
            {
                HasVideoTrack = false,
                HasAudioTrack = true
            }.Build();

            session = new Session.Builder(context, API_KEY, SESSION_ID).Build();
            session.Connected += Session_Connected;
            session.Disconnected += Session_Disconnected;
            session.Error += Session_Error;
            session.StreamReceived += Session_StreamReceived;
            session.Connect(TOKEN);

            StopRecordingButton.IsEnabled = false;
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
            Subscriber subscriber = new Subscriber.Builder(context, e.Stream).Build();
            session.Subscribe(subscriber);
        }

        private void StartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            customCapturerAudioDevice.StartRecording();
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            customCapturerAudioDevice.StopRecording();
        }

        private void FileRecordingAudioDevice_StatusChanged(bool isRecording)
        {
            StartRecordingButton.IsEnabled = !isRecording;
            StopRecordingButton.IsEnabled = isRecording;
        }
    }
}
