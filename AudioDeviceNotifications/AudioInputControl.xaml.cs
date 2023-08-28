using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using OpenTok;

namespace AudioDeviceNotifications
{
    public partial class AudioInputControl : UserControl
    {
        #region Events

        public delegate void AudioInputEnabledChangedHandler(bool isEnabled);

        public event AudioInputEnabledChangedHandler AudioInputEnabledChanged;

        #endregion

        #region Constructors

        public AudioInputControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Public

        public void Init(Window parent, MMAudioDevice audioDevice)
        {
            this.parent = parent;
            this.audioDevice = audioDevice;

            ReloadAudioInputComboBox();

            audioDevice.InputDeviceAdded += AudioInputDeviceAdded;
            audioDevice.InputDeviceRemoved += AudioInputDeviceRemoved;
            audioDevice.DefaultInputDeviceChanged += AudioDefaultInputDeviceChanged;
        }

        public void Close()
        {
            audioDevice.InputDeviceAdded -= AudioInputDeviceAdded;
            audioDevice.InputDeviceRemoved -= AudioInputDeviceRemoved;
            audioDevice.DefaultInputDeviceChanged -= AudioDefaultInputDeviceChanged;
        }

        #endregion

        #region Private

        private void AudioInputCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (AudioInputComboBox != null)
            {
                AudioInputComboBox.IsEnabled = true;
                OnAudioInputEnabledChanged();
            }
        }

        private void AudioInputCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AudioInputComboBox != null)
            {
                AudioInputComboBox.IsEnabled = false;
                OnAudioInputEnabledChanged();
            }
        }

        private void AudioInputComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AudioInputComboBox.SelectedItem != null)
            {
                AudioDevice.InputAudioDevice device = (AudioDevice.InputAudioDevice)AudioInputComboBox.SelectedItem;
                audioDevice.SetInputAudioDevice(device.Id);
            }
        }

        private void AudioInputDeviceAdded(object sender, AudioDevice.Notifications.InputAudioDeviceEventArgs e)
        {
            ReloadAudioInputComboBox();

            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(parent, string.Format("A new audio input device was detected: {0}\nWould you like to select this new device?", e.Device.Name), "New audio input device", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    AudioInputComboBox.SelectedItem = e.Device;
                }
            }));
        }

        private void AudioInputDeviceRemoved(object sender, AudioDevice.Notifications.InputAudioDeviceEventArgs e)
        {
            ReloadAudioInputComboBox();
        }

        private void AudioDefaultInputDeviceChanged(object sender, AudioDevice.Notifications.InputAudioDeviceEventArgs e)
        {
            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AudioInputComboBox.SelectedItem = e.Device;
            }));
        }

        private void ReloadAudioInputComboBox()
        {
            IList<AudioDevice.InputAudioDevice> availableAudioInputs = audioDevice.EnumerateInputAudioDevices();

            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (availableAudioInputs == null || availableAudioInputs.Count == 0)
                {
                    AudioInputComboBox.ItemsSource = null;
                }
                else
                {
                    AudioDevice.InputAudioDevice currentSelection = (AudioDevice.InputAudioDevice)AudioInputComboBox.SelectedItem;
                    AudioInputComboBox.ItemsSource = availableAudioInputs;
                    if (currentSelection != null)
                        AudioInputComboBox.SelectedItem = currentSelection;
                    if (AudioInputComboBox.SelectedItem == null)
                        AudioInputComboBox.SelectedItem = audioDevice.GetDefaultInputAudioDevice();
                    if (AudioInputComboBox.SelectedItem == null)
                        AudioInputComboBox.SelectedIndex = 0;
                }
                OnAudioInputEnabledChanged();
            }));
        }

        private void OnAudioInputEnabledChanged()
        {
            bool isAudioInputEnabled = AudioInputCheckBox.IsChecked.HasValue && AudioInputCheckBox.IsChecked.Value && AudioInputComboBox.SelectedItem != null;
            AudioInputEnabledChanged?.Invoke(isAudioInputEnabled);
        }

        private Window parent;
        private MMAudioDevice audioDevice;

        #endregion
    }
}
