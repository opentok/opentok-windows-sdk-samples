using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using OpenTok;

namespace BasicVideoChat
{
    public partial class MainWindow : Window
    {
        private const string API_KEY = "";
        private const string SESSION_ID = "";
        private const string TOKEN = "";

        private Context context;
        private Session session;
        private Publisher publisher;
        private AudioDevice.Notifications audioDeviceNotifications;

        public MainWindow()
        {
            InitializeComponent();

            context = new Context(new WPFDispatcher());

            // Uncomment following line to get debug logging
            // LogUtil.Instance.EnableLogging();

            IList<VideoCapturer.VideoDevice> capturerDevices = VideoCapturer.EnumerateDevices();
            if (capturerDevices == null || capturerDevices.Count == 0)
                throw new Exception("No video capture devices detected");

            publisher = new Publisher.Builder(context)
            {
                Capturer = capturerDevices[0].CreateVideoCapturer(VideoCapturer.Resolution.High, VideoCapturer.FrameRate.Fps30),
                Renderer = PublisherVideo
            }.Build();

            ReloadAudioInputComboBox();
            ReloadAudioOutputComboBox();

            // Detection of added devices can sometimes take a few seconds. This is a consequence of windows OS detection system
            audioDeviceNotifications = new AudioDevice.Notifications(new WPFDispatcher());
            audioDeviceNotifications.InputDeviceAdded += AudioInputDeviceAdded;
            audioDeviceNotifications.OutputDeviceAdded += AudioOutputDeviceAdded;
            audioDeviceNotifications.InputDeviceRemoved += AudioInputDeviceRemoved;
            audioDeviceNotifications.OutputDeviceRemoved += AudioOutputDeviceRemoved;
            audioDeviceNotifications.DefaultInputDeviceChanged += AudioDefaultInputDeviceChanged;
            audioDeviceNotifications.DefaultOutputDeviceChanged += AudioDefaultOutputDeviceChanged;

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
            Subscriber subscriber = new Subscriber.Builder(context, e.Stream)
            {
                Renderer = SubscriberVideo
            }.Build();
            session.Subscribe(subscriber);
        }

        private void ReloadAudioInputComboBox()
        {
            IList<AudioDevice.InputAudioDevice> availableAudioInputs = AudioDevice.EnumerateInputAudioDevices();
            if (availableAudioInputs == null || availableAudioInputs.Count == 0)
            {
                Console.WriteLine("No audio inputs detected");
                AudioInputComboBox.ItemsSource = null;
            }
            else
            {
                AudioDevice.InputAudioDevice currentSelection = (AudioDevice.InputAudioDevice)AudioInputComboBox.SelectedItem;
                AudioInputComboBox.ItemsSource = availableAudioInputs;
                if (currentSelection != null)
                    AudioInputComboBox.SelectedItem = currentSelection;
                if (AudioInputComboBox.SelectedItem == null)
                    AudioInputComboBox.SelectedItem = AudioDevice.GetDefaultInputAudioDevice();
                if (AudioInputComboBox.SelectedItem == null)
                    AudioInputComboBox.SelectedIndex = 0;
            }
        }

        private void ReloadAudioOutputComboBox()
        {
            IList<AudioDevice.OutputAudioDevice> availableAudioOutputs = AudioDevice.EnumerateOutputAudioDevices();
            if (availableAudioOutputs == null || availableAudioOutputs.Count == 0)
            {
                Console.WriteLine("No audio outputs detected");
                AudioOutputComboBox.ItemsSource = null;
            }
            else
            {
                AudioDevice.OutputAudioDevice currentSelection = (AudioDevice.OutputAudioDevice)AudioOutputComboBox.SelectedItem;
                AudioOutputComboBox.ItemsSource = availableAudioOutputs;
                if (currentSelection != null)
                    AudioOutputComboBox.SelectedItem = currentSelection;
                if (AudioOutputComboBox.SelectedItem == null)
                    AudioOutputComboBox.SelectedItem = AudioDevice.GetDefaultOutputAudioDevice();
                if (AudioOutputComboBox.SelectedItem == null)
                    AudioOutputComboBox.SelectedIndex = 0;
            }
        }

        private void AudioOutputComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AudioOutputComboBox.SelectedItem != null)
                AudioDevice.SetOutputAudioDevice((AudioDevice.OutputAudioDevice)AudioOutputComboBox.SelectedItem);
        }

        private void AudioInputComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AudioInputComboBox.SelectedItem != null)
                AudioDevice.SetInputAudioDevice((AudioDevice.InputAudioDevice)AudioInputComboBox.SelectedItem);
        }
        
        private void AudioInputDeviceAdded(object sender, AudioDevice.Notifications.InputAudioDeviceEventArgs e)
        {
            Console.WriteLine("Audio input device added: " + e.Device);
            ReloadAudioInputComboBox();
            MessageBoxResult messageBoxResult = MessageBox.Show(this, string.Format("A new audio input device was detected: {0}\nWould you like to select this new device?", e.Device.Name), "New audio input device", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
                AudioInputComboBox.SelectedItem = e.Device;
        }

        private void AudioOutputDeviceAdded(object sender, AudioDevice.Notifications.OutputAudioDeviceEventArgs e)
        {
            Console.WriteLine("Audio output device added: " + e.Device);
            ReloadAudioOutputComboBox();
            MessageBoxResult messageBoxResult = MessageBox.Show(this, string.Format("A new audio output device was detected: {0}\nWould you like to select this new device?", e.Device.Name), "New audio output device", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
                AudioOutputComboBox.SelectedItem = e.Device;
        }

        private void AudioInputDeviceRemoved(object sender, AudioDevice.Notifications.InputAudioDeviceEventArgs e)
        {
            Console.WriteLine("Audio input device removed: " + e.Device);
            ReloadAudioInputComboBox();
        }

        private void AudioOutputDeviceRemoved(object sender, AudioDevice.Notifications.OutputAudioDeviceEventArgs e)
        {
            Console.WriteLine("Audio output device removed: " + e.Device);
            ReloadAudioOutputComboBox();
        }

        private void AudioDefaultInputDeviceChanged(object sender, AudioDevice.Notifications.InputAudioDeviceEventArgs e)
        {
            Console.WriteLine("Default audio input device changed: " + e.Device);
            AudioInputComboBox.SelectedItem = e.Device;
        }

        private void AudioDefaultOutputDeviceChanged(object sender, AudioDevice.Notifications.OutputAudioDeviceEventArgs e)
        {
            Console.WriteLine("Default audio output device changed: " + e.Device);
            AudioOutputComboBox.SelectedItem = e.Device;
        }
    }
}
