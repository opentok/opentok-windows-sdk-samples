using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using OpenTok;

namespace FileAudioSource
{
    public partial class MainWindow : Window
    {
        private const string API_KEY = "47446341";
        private const string SESSION_ID = "2_MX40NzQ0NjM0MX5-MTY4NTM2NDk5MjU3MX5KV0xPR1ZRb1R2SG9hQzFCeElWSnBJUld-fn4";
        private const string TOKEN = "T1==cGFydG5lcl9pZD00NzQ0NjM0MSZzaWc9YjY4YmQyNDJkYzIxM2ZhZmQwN2Y4NjYzNTM3OThmNjEzZjkyOTEzYzpzZXNzaW9uX2lkPTJfTVg0ME56UTBOak0wTVg1LU1UWTROVE0yTkRrNU1qVTNNWDVLVjB4UFIxWlJiMVIyU0c5aFF6RkNlRWxXU25CSlVsZC1mbjQmY3JlYXRlX3RpbWU9MTY4NTM2NTA1NyZub25jZT0wLjgwNDUwMzIzODMyNTUwNzcmcm9sZT1wdWJsaXNoZXImZXhwaXJlX3RpbWU9MTY4Nzk1NzA1NiZpbml0aWFsX2xheW91dF9jbGFzc19saXN0PQ==";

        private Context context;
        private Session session;
        private Publisher publisher;
        private FileSourceAudioDevice fileSourceAudioDevice = new FileSourceAudioDevice();

        public MainWindow()
        {
            InitializeComponent();

            context = new Context(new WPFDispatcher());

            AudioDevice.SetCustomAudioDevice(context, fileSourceAudioDevice);
            fileSourceAudioDevice.AudioPropertiesChanged += FileSourceAudioDevice_AudioPropertiesChanged;

            // Uncomment following line to get debug logging
            // LogUtil.Instance.EnableLogging();

            publisher = new Publisher.Builder(context)
            {
                HasVideoTrack = false,
                HasAudioTrack = true
            }.Build();

            session = new Session.Builder(context, API_KEY, SESSION_ID).Build();
            session.Connected += Session_Connected;
            session.Disconnected += Session_Disconnected;
            session.Error += Session_Error;
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

        private void FileSourceAudioDevice_AudioPropertiesChanged(int numberOfChannels, int samplingRate)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (numberOfChannels == 0 || samplingRate == 0)
                    SelectedFileInfo.Text = "No selected file";
                else
                    SelectedFileInfo.Text = string.Format("Channels {0}, Sampling rate {1} samples/s", numberOfChannels, samplingRate);
            }));
        }

        private void SelectSourceFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wave files|*.wav";
            openFileDialog.FileOk += (s_, e_) =>
            {
                string sourceFileName = openFileDialog.FileName;
                fileSourceAudioDevice.SelectSourceFile(sourceFileName);
            };
            openFileDialog.ShowDialog();
        }
    }
}