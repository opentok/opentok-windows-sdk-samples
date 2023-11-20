using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using OpenTok;

namespace AudioDeviceNotifications
{
    public partial class AudioOutputControl : UserControl
    {
        #region Constructors

        public AudioOutputControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Public

        public void Init(Window parent, MMAudioDevice audioDevice)
        {
            this.parent = parent;
            this.audioDevice = audioDevice;

            ReloadAudioOutputComboBox();

            audioDevice.OutputDeviceAdded += AudioOutputDeviceAdded;
            audioDevice.OutputDeviceRemoved += AudioOutputDeviceRemoved;
            audioDevice.DefaultOutputDeviceChanged += AudioDefaultOutputDeviceChanged;
        }

        public void Close()
        {
            audioDevice.OutputDeviceAdded -= AudioOutputDeviceAdded;
            audioDevice.OutputDeviceRemoved -= AudioOutputDeviceRemoved;
            audioDevice.DefaultOutputDeviceChanged -= AudioDefaultOutputDeviceChanged;
        }

        #endregion

        #region Private

        private void AudioOutputComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AudioOutputComboBox.SelectedItem != null)
            {
                AudioDevice.OutputAudioDevice device = (AudioDevice.OutputAudioDevice)AudioOutputComboBox.SelectedItem;
                audioDevice.SetOutputAudioDevice(device.Id);
            }
        }

        private void AudioOutputDeviceAdded(object sender, AudioDevice.Notifications.OutputAudioDeviceEventArgs e)
        {
            ReloadAudioOutputComboBox();

            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(parent, string.Format("A new audio output device was detected: {0}\nWould you like to select this new device?", e.Device.Name), "New audio output device", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    AudioOutputComboBox.SelectedItem = e.Device;
                }
            }));
        }

        private void AudioOutputDeviceRemoved(object sender, AudioDevice.Notifications.OutputAudioDeviceEventArgs e)
        {
            ReloadAudioOutputComboBox();
        }

        private void AudioDefaultOutputDeviceChanged(object sender, AudioDevice.Notifications.OutputAudioDeviceEventArgs e)
        {
            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AudioOutputComboBox.SelectedItem = e.Device;
            }));
        }

        private void ReloadAudioOutputComboBox()
        {
            IList<AudioDevice.OutputAudioDevice> availableAudioOutputs = audioDevice.EnumerateOutputAudioDevices();

            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (availableAudioOutputs == null || availableAudioOutputs.Count == 0)
                {
                    AudioOutputComboBox.ItemsSource = null;
                }
                else
                {
                    AudioDevice.OutputAudioDevice currentSelection = (AudioDevice.OutputAudioDevice)AudioOutputComboBox.SelectedItem;
                    AudioOutputComboBox.ItemsSource = availableAudioOutputs;
                    if (currentSelection != null)
                        AudioOutputComboBox.SelectedItem = currentSelection;
                    if (AudioOutputComboBox.SelectedItem == null)
                        AudioOutputComboBox.SelectedItem = audioDevice.GetDefaultOutputAudioDevice();
                    if (AudioOutputComboBox.SelectedItem == null)
                        AudioOutputComboBox.SelectedIndex = 0;
                }
            }));
        }

        private Window parent;
        private MMAudioDevice audioDevice;

        #endregion
    }
}
