using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTok;

namespace CustomAudioFileCapturer
{
    public class CustomCapturerAudioDevice : IAudioDevice, IDisposable
    {
        #region Public

        public void StartRecording()
        {
            string directory = Path.GetDirectoryName(OUTPUT_FILE_PATH);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            if (File.Exists(OUTPUT_FILE_PATH))
                File.Delete(OUTPUT_FILE_PATH);

            outputFileStream = File.OpenWrite(OUTPUT_FILE_PATH);
            outputFileStream.Write(new byte[44], 0, 44); /* Write empty wav header that we'll fill later */
            isRecording = true;

            StatusChanged?.Invoke(true);
        }

        public void StopRecording()
        {
            isRecording = false;
            Thread.Sleep(20); /* Wait in case there is any pending write to the file */

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
            outputFileStream.Write(BitConverter.GetBytes(NUMBER_OF_CHANNELS), 0, 2);
            outputFileStream.Write(BitConverter.GetBytes(SAMPLING_RATE), 0, 4);
            outputFileStream.Write(BitConverter.GetBytes(SAMPLING_RATE * BYTES_PER_SAMPLE * NUMBER_OF_CHANNELS), 0, 4);
            outputFileStream.Write(BitConverter.GetBytes(BYTES_PER_SAMPLE * NUMBER_OF_CHANNELS), 0, 2);
            outputFileStream.Write(BitConverter.GetBytes(BYTES_PER_SAMPLE * 8), 0, 2);
            outputFileStream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            outputFileStream.Write(BitConverter.GetBytes(fileLength - 44), 0, 4);
            outputFileStream.Flush();
            outputFileStream.Close();
            outputFileStream = null;

            StatusChanged?.Invoke(false);
        }

        public delegate void StatusChangedHandler(bool isRecording);
        public event StatusChangedHandler StatusChanged;

        #endregion

        #region IAudioDevice

        /* Capturer subsystem is empty and only provided to satisfy IAudioDevice interface */

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
                isAudioCapturerInitialized = true;
            }
        }

        public void DestroyAudioCapturer()
        {
            lock (audioDeviceLock)
            {
                StopAudioCapturer();
                isAudioCapturerInitialized = false;
            }
        }

        public void StartAudioCapturer()
        {
            lock (audioDeviceLock)
            {
                isAudioCapturerStarted = true;
            }
        }

        public void StopAudioCapturer()
        {
            lock (audioDeviceLock)
            {
                isAudioCapturerStarted = false;
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
                return new AudioDeviceSettings();
            }
        }

        public void InitAudioRenderer()
        {
            lock (audioDeviceLock)
            {
                if (IsAudioRendererInitialized())
                    return;
                rendererSettings = new AudioDeviceSettings()
                {
                    NumChannels = NUMBER_OF_CHANNELS,
                    SamplingRate = SAMPLING_RATE
                };
                isAudioRendererInitialized = true;
            }
        }

        public void DestroyAudioRenderer()
        {
            lock (audioDeviceLock)
            {
                StopAudioRenderer();
                if (!IsAudioRendererInitialized())
                    return;
                isAudioRendererInitialized = false;
            }
        }

        public void StartAudioRenderer()
        {
            lock (audioDeviceLock)
            {
                if (IsAudioRendererStarted())
                    return;
                if (!IsAudioRendererInitialized())
                    throw new Exception("Audio renderer not initialized");

                isAudioRendererStarted = true;

                _ = Task.Factory.StartNew(() =>
                {
                    int numberOfSamplesPer10ms = rendererSettings.SamplingRate / 100;
                    byte[] buffer = new byte[numberOfSamplesPer10ms * rendererSettings.NumChannels * BYTES_PER_SAMPLE];
                    unsafe
                    {
                        fixed (byte* bufferPtr = buffer)
                        {
                            DateTime nextBatchTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, 10);
                            while (isAudioRendererStarted)
                            {
                                try
                                {
                                    DateTime now = DateTime.Now;
                                    if (now >= nextBatchTime)
                                    {
                                        nextBatchTime += new TimeSpan(0, 0, 0, 0, 10);
                                        int samples = audioBus.ReadRenderData((IntPtr)bufferPtr, numberOfSamplesPer10ms);
                                        if (isRecording)
                                        {
                                            outputFileStream.Write(buffer, 0, numberOfSamplesPer10ms * rendererSettings.NumChannels * BYTES_PER_SAMPLE);
                                            outputFileStream.Flush();
                                        }
                                    }
                                    TimeSpan sleepTime = nextBatchTime > now ? nextBatchTime - now : TimeSpan.Zero;
                                    Thread.Sleep(sleepTime);
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

        public void StopAudioRenderer()
        {
            lock (audioDeviceLock)
            {
                StopRecording();
                if (!IsAudioRendererStarted())
                    return;
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
            return rendererSettings;
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

        ~CustomCapturerAudioDevice()
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

        private const string OUTPUT_FILE_PATH = "C:\\temp\\audio.wav";
        private const short NUMBER_OF_CHANNELS = 2;
        private const int SAMPLING_RATE = 48000;
        private const short BYTES_PER_SAMPLE = 2;

        private readonly object audioDeviceLock = new object();
        private AudioDevice.AudioBus audioBus;
        private bool isAudioCapturerInitialized;
        private bool isAudioRendererInitialized;
        private bool isAudioCapturerStarted;
        private bool isAudioRendererStarted;
        private bool isRecording;
        private FileStream outputFileStream;
        private AudioDeviceSettings rendererSettings = new AudioDeviceSettings();
        private bool isDisposed;

        #endregion
    }
}
