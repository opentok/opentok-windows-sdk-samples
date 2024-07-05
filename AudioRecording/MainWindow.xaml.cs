using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using OpenTok;

namespace AudioRecording
{
    public partial class MainWindow : Window
    {
        private const string API_KEY = "";
        private const string SESSION_ID = "";
        private const string TOKEN = "";
        private const string OUTPUT_FILE_PATH = "C:\\temp\\audio.wav";

        private Context context;
        private Session session;
        private Publisher publisher;
        private Subscriber subscriber;
        private bool isRecording;
        private FileStream outputFileStream;
        private short numberOfChannels;
        private int samplingRate;
        private short bytesPerSample;

        public MainWindow()
        {
            InitializeComponent();
            RecordingButtons.IsEnabled = false;

            context = new Context(new WPFDispatcher());

            // Uncomment following line to get debug logging
            // Logger.Enable();

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
            subscriber = new Subscriber.Builder(context, e.Stream)
            {
                Renderer = SubscriberVideo
            }.Build();            
            session.Subscribe(subscriber);
            RecordingButtons.IsEnabled = true;
        }

        private void Subscriber_AudioData(object sender, Subscriber.AudioDataEventArgs e)
        {
            if (!isRecording)
                return;
            if (outputFileStream == null)
            {
                outputFileStream = File.OpenWrite(OUTPUT_FILE_PATH);
                outputFileStream.Write(new byte[44], 0, 44); /* Make room for the wave file header that will be written the app closes */

                numberOfChannels = (short)e.NumberOfChannels;
                samplingRate = e.SampleRate;
                bytesPerSample = (short)(e.BitsPerSample / 8);
            }
            
            outputFileStream.Write(e.SampleBuffer, 0, e.NumberOfSamples * e.NumberOfChannels * bytesPerSample);
            outputFileStream.Flush();
        }

        private void StartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            string directory = Path.GetDirectoryName(OUTPUT_FILE_PATH);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            if (File.Exists(OUTPUT_FILE_PATH))
                File.Delete(OUTPUT_FILE_PATH);

            isRecording = true;
            subscriber.AudioData += Subscriber_AudioData;

            StartRecordingButton.Visibility = Visibility.Collapsed;
            StopRecordingButton.Visibility = Visibility.Visible;
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            StartRecordingButton.Visibility = Visibility.Visible;
            StopRecordingButton.Visibility = Visibility.Collapsed;

            isRecording = false;
            subscriber.AudioData -= Subscriber_AudioData;

            if (outputFileStream == null)
                return;
            outputFileStream.Flush();
            outputFileStream.Close();

            /* Write wave file header */
            long fileLength = new FileInfo(OUTPUT_FILE_PATH).Length;
            outputFileStream = File.OpenWrite(OUTPUT_FILE_PATH);
            outputFileStream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            outputFileStream.Write(BitConverter.GetBytes(fileLength), 0, 4);
            outputFileStream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
            outputFileStream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            outputFileStream.Write(BitConverter.GetBytes(16), 0, 4); /* Length of the following fmt section */
            outputFileStream.Write(BitConverter.GetBytes(1), 0, 2); /* 1=PCM format */
            outputFileStream.Write(BitConverter.GetBytes(numberOfChannels), 0, 2);
            outputFileStream.Write(BitConverter.GetBytes(samplingRate), 0, 4);
            outputFileStream.Write(BitConverter.GetBytes(samplingRate * bytesPerSample * numberOfChannels), 0, 4);
            outputFileStream.Write(BitConverter.GetBytes(bytesPerSample * numberOfChannels), 0, 2);
            outputFileStream.Write(BitConverter.GetBytes(bytesPerSample * 8), 0, 2);
            outputFileStream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            outputFileStream.Write(BitConverter.GetBytes(fileLength - 44), 0, 4);
            outputFileStream.Flush();
            outputFileStream.Close();
            outputFileStream = null;
        }
    }
}
