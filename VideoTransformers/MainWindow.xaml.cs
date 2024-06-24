using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows;
using OpenTok;
using System.Windows.Forms;

namespace VideoTransformers
{
    public partial class MainWindow : Window
    {
        private const string API_KEY = "";
        private const string SESSION_ID = "";
        private const string TOKEN = "";

        private Context context;
        private Session Session;
        private Publisher Publisher;

        public MainWindow()
        {
            InitializeComponent();

            context = new Context(new WPFDispatcher());

            // Uncomment following line to get debug logging
            // Logger.Enable();

            IList<VideoCapturer.VideoDevice> capturerDevices = VideoCapturer.EnumerateDevices();
            if (capturerDevices == null || capturerDevices.Count == 0)
                throw new Exception("No video capture devices detected");

            Publisher = new Publisher.Builder(context)
            {
                Capturer = capturerDevices[0].CreateVideoCapturer(VideoCapturer.Resolution.High, VideoCapturer.FrameRate.Fps30),
                Renderer = PublisherVideo
            }.Build();

            IList<AudioDevice.InputAudioDevice> availableMics = AudioDevice.EnumerateInputAudioDevices();
            if (availableMics == null || availableMics.Count == 0)
                throw new Exception("No audio capture devices detected");
            AudioDevice.SetInputAudioDevice(availableMics[0]);

            Session = new Session.Builder(context, API_KEY, SESSION_ID).Build();
            Session.Connected += Session_Connected;
            Session.Disconnected += Session_Disconnected;
            Session.Error += Session_Error;
            Session.StreamReceived += Session_StreamReceived;
            Session.Connect(TOKEN);
        }

        private void Session_Connected(object sender, System.EventArgs e)
        {
            Session.Publish(Publisher);

            /* Vonage video background blur transformer */
            VideoTransformer backgroundBlur = new VideoTransformer("BackgroundBlur", "{\"radius\":\"High\"}");

            /* Custom video transformer */
            ICustomVideoTransformer customLogo = new LogoTransformer();
            VideoTransformer logo = new VideoTransformer("logo", customLogo);

            List<VideoTransformer> videoTransformers = new List<VideoTransformer>
            {
                backgroundBlur,
                logo
            };

            // List of video transformers
            Publisher.VideoTransformers = videoTransformers;

            /* Vonage audio noise suppression transformer */
            AudioTransformer audioTransformer = new AudioTransformer("NoiseSuppression", "");

            List<AudioTransformer> audioTransformers = new List<AudioTransformer>
            {
                audioTransformer
            };

            // List of audio transformers
            Publisher.AudioTransformers = audioTransformers;
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
            Session.Subscribe(subscriber);
        }

        public class LogoTransformer : ICustomVideoTransformer
        {
            public Bitmap ResizeImage(Bitmap image, int width, int height)
            {
                Bitmap resizedImage = new Bitmap(width, height);

                using (Graphics graphics = Graphics.FromImage(resizedImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(image, 0, 0, width, height);
                }

                return resizedImage;
            }

            public void Transform(VideoFrame frame)
            {
                // Obtain the Y plane of the video frame
                IntPtr yPlane = frame.GetPlane(0);

                // Get the dimensions of the video frame
                int videoWidth = frame.Width;
                int videoHeight = frame.Height;

                // Get the dimensions of the logo
                Bitmap image = new Bitmap("Resources/Vonage_Logo.png");

                // Calculate the desired size of the image
                int desiredWidth = videoWidth / 10; // Adjust this value as needed
                int desiredHeight = (int)(image.Height * ((float)desiredWidth / image.Width));

                // Resize the image to the desired size
                Bitmap newimage = ResizeImage(image, desiredWidth, desiredHeight);

                int logoWidth = newimage.Width;
                int logoHeight = newimage.Height;

                // Location of the image (top right corner of video)
                int logoPositionX = videoWidth * 5/6 - logoWidth; // Adjust this as needed for the desired position
                int logoPositionY = videoHeight * 1/7 - logoHeight; // Adjust this as needed for the desired position

                // Overlay the logo on the video frame
                for (int y = 0; y < logoHeight; y++)
                {
                    for (int x = 0; x < logoWidth; x++)
                    {
                        int frameX = logoPositionX + x;
                        int frameY = logoPositionY + y;

                        if (frameX >= 0 && frameX < videoWidth && frameY >= 0 && frameY < videoHeight)
                        {
                            int frameOffset = frameY * videoWidth + frameX;

                            // Get the logo pixel color
                            Color logoColor = newimage.GetPixel(x, y);

                            // Extract the color channels (ARGB)
                            int logoAlpha = logoColor.A;
                            int logoRed = logoColor.R;

                            // Overlay the logo pixel on the video frame
                            int framePixel = Marshal.ReadByte(yPlane, frameOffset) & 0xFF;

                            // Calculate the blended pixel value
                            int blendedPixel = ((logoAlpha * logoRed + (255 - logoAlpha) * framePixel) / 255) & 0xFF;

                            // Set the blended pixel value in the video frame
                            Marshal.WriteByte(yPlane, frameOffset, (byte)blendedPixel);
                        }
                    }
                }
            }
        }
    }
}
