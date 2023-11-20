using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTok
{
    public class FileSourceAudioDevice : IAudioDevice, IDisposable
    {
        #region Public

        public void SelectSourceFile(string file)
        {
            if (!File.Exists(file))
                throw new ArgumentException("File does not exist");
            if (!file.EndsWith(".wav"))
                throw new ArgumentException("Invalid file format");
            lock (audioDeviceLock)
            {
                sourceFile = file;
                AudioDevice.RestartInputAudioDevice();
            }
        }

        public delegate void AudioPropertiesChangedHandler(int numberOfChannels, int samplingRate);
        public event AudioPropertiesChangedHandler AudioPropertiesChanged;

        #endregion

        #region IAudioDevice

        public void InitAudio(AudioDevice.AudioBus audioBus)
        {
            lock (audioDeviceLock)
            {
                this.audioBus = audioBus;
            }
        }

        public void DestroyAudio()
        {
            lock (audioDeviceLock)
            {
                audioBus = null;
            }
        }

        public void InitAudioCapturer()
        {
            lock (audioDeviceLock)
            {
                if (IsAudioCapturerInitialized())
                    return;
                if (!string.IsNullOrWhiteSpace(sourceFile))
                {
                    sourceFileStream = File.OpenRead(sourceFile);
                    /* WAV file format header struct can be found at https://docs.fileformat.com/audio/wav/ */
                    byte[] wavHeader = new byte[40];
                    sourceFileStream.Read(wavHeader, 0, wavHeader.Length);
                    capturerSettings = new AudioDeviceSettings();
                    capturerSettings.NumChannels = wavHeader[22] | (wavHeader[23] << 8);
                    capturerSettings.SamplingRate = wavHeader[24] | (wavHeader[25] << 8) | (wavHeader[26] << 16) | (wavHeader[27] << 24);
                    AudioPropertiesChanged?.Invoke(capturerSettings.NumChannels, capturerSettings.SamplingRate);
                }
                else
                {
                    capturerSettings = new AudioDeviceSettings() { NumChannels = 2, SamplingRate = 48000 };
                    AudioPropertiesChanged?.Invoke(0, 0);
                }
                isAudioCapturerInitialized = true;
            }
        }

        public void DestroyAudioCapturer()
        {
            lock (audioDeviceLock)
            {
                StopAudioCapturer();
                if (!IsAudioCapturerInitialized())
                    return;
                if (sourceFileStream != null)
                {
                    sourceFileStream.Close();
                    sourceFileStream.Dispose();
                    sourceFileStream = null;
                }
                isAudioCapturerInitialized = false;
            }
        }

        public void StartAudioCapturer()
        {
            lock (audioDeviceLock)
            {
                if (IsAudioCapturerStarted())
                    return;
                if (!IsAudioCapturerInitialized())
                    throw new Exception("Audio capturer not initialized");

                isAudioCapturerStarted = true;

                _ = Task.Factory.StartNew(() =>
                {
                    int numberOfSamplesPer10ms = capturerSettings.NumChannels * capturerSettings.SamplingRate / 100;
                    byte[] buffer = new byte[numberOfSamplesPer10ms * 2];
                    unsafe
                    {
                        fixed (byte* bufferPtr = buffer)
                        {
                            DateTime nextBatchTime = DateTime.Now;
                            while (isAudioCapturerStarted)
                            {
                                try
                                {
                                    if (DateTime.Now >= nextBatchTime)
                                    {
                                        nextBatchTime += new TimeSpan(0, 0, 0, 0, 10);

                                        int bytesRead = buffer.Length;
                                        if (sourceFileStream != null)
                                            bytesRead = sourceFileStream.Read(buffer, 0, buffer.Length);
                                        audioBus.WriteCaptureData((IntPtr)bufferPtr, bytesRead / (2 * capturerSettings.NumChannels));
                                    }
                                    Thread.Sleep(1);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                });
            }
        }

        public void StopAudioCapturer()
        {
            lock (audioDeviceLock)
            {
                if (!IsAudioCapturerStarted())
                    return;
                isAudioCapturerStarted = false;
                Thread.Sleep(20); //Wait two whole cycles to make sure it's stopped
            }
        }

        public bool IsAudioCapturerInitialized()
        {
            lock (audioDeviceLock)
            {
                return isAudioCapturerInitialized;
            }
        }

        public bool IsAudioCapturerStarted()
        {
            lock (audioDeviceLock)
            {
                return isAudioCapturerStarted;
            }
        }

        public int GetEstimatedAudioCaptureDelay()
        {
            lock (audioDeviceLock)
            {
                return 0;
            }
        }

        public AudioDeviceSettings GetAudioCapturerSettings()
        {
            lock (audioDeviceLock)
            {
                return capturerSettings;
            }
        }

        public void InitAudioRenderer()
        {
            lock (audioDeviceLock)
            {
                isAudioRendererInitialized = true;
            }
        }

        public void DestroyAudioRenderer()
        {
            lock (audioDeviceLock)
            {
                StopAudioRenderer();
                isAudioRendererInitialized = false;
            }
        }

        public void StartAudioRenderer()
        {
            lock (audioDeviceLock)
            {
                isAudioRendererStarted = true;
            }
        }

        public void StopAudioRenderer()
        {
            lock (audioDeviceLock)
            {
                isAudioRendererStarted = false;
            }
        }

        public bool IsAudioRendererInitialized()
        {
            lock (audioDeviceLock)
            {
                return isAudioRendererInitialized;
            }
        }

        public bool IsAudioRendererStarted()
        {
            lock (audioDeviceLock)
            {
                return isAudioRendererStarted;
            }
        }

        public int GetEstimatedAudioRenderDelay()
        {
            return 0;
        }

        public AudioDeviceSettings GetAudioRendererSettings()
        {
            return new AudioDeviceSettings();
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            isDisposed = true;

            DestroyAudioCapturer();
            DestroyAudioRenderer();

            if (disposing)
            {
            }
        }

        ~FileSourceAudioDevice()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private

        private readonly object audioDeviceLock = new object();
        private AudioDevice.AudioBus audioBus;
        private bool isAudioCapturerInitialized;
        private bool isAudioRendererInitialized;
        private bool isAudioCapturerStarted;
        private bool isAudioRendererStarted;
        private string sourceFile;
        private FileStream sourceFileStream;
        private AudioDeviceSettings capturerSettings;
        private bool isDisposed;

        #endregion
    }
}
