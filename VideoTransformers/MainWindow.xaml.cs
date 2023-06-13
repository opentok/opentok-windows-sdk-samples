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
    /*
    public class ScreenCapturer : IVideoCapturer
    {
        private readonly int width;
        private readonly int height;
        private readonly int fps;
        private System.Threading.Timer timer;
        private IVideoFrameConsumer frameConsumer;

        public ScreenCapturer(VideoCapturer.Resolution resolution, VideoCapturer.FrameRate frameRate)
        {
            if (resolution == VideoCapturer.Resolution.Low)
            {
                width = 320;
                height = 240;
            }
            else if (resolution == VideoCapturer.Resolution.Medium)
            {
                width = 640;
                height = 460;
            }
            else if (resolution == VideoCapturer.Resolution.High)
            {
                width = 1280;
                height = 720;
            }
            else if (resolution == VideoCapturer.Resolution.High1080p)
            {
                width = 1920;
                height = 1080;
            }
            else
            {
                throw new ArgumentException(nameof(resolution));
            }

            if (frameRate == VideoCapturer.FrameRate.Fps30)
            {
                fps = 30;
            }
            else if (frameRate == VideoCapturer.FrameRate.Fps15)
            {
                fps = 15;
            }
            else if (frameRate == VideoCapturer.FrameRate.Fps7)
            {
                fps = 7;
            }
            else if (frameRate == VideoCapturer.FrameRate.Fps1)
            {
                fps = 1;
            }
            else
            {
                throw new ArgumentException(nameof(frameRate));
            }
        }

        public void Destroy()
        {
        }

        public void Init(IVideoFrameConsumer frameConsumer)
        {
            this.frameConsumer = frameConsumer;
        }

        public void SetVideoContentHint(VideoContentHint contentHint)
        {
            frameConsumer.SetVideoContentHint(contentHint);
        }

        public VideoContentHint GetVideoContentHint()
        {
            return frameConsumer.GetVideoContentHint();
        }

        public void Start()
        {
            Rectangle screenBounds = Screen.GetBounds(System.Drawing.Point.Empty);
            timer = new System.Threading.Timer((object stateInfo) =>
            {
                using (Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    bitmap.SetResolution(width, height);
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.CopyFromScreen(0, 0, 0, 0, screenBounds.Size, CopyPixelOperation.SourceCopy);
                    }
                    using (VideoFrame frame = VideoFrame.CreateYuv420pFrameFromBitmap(bitmap))
                    {
                        frameConsumer.Consume(frame);
                    }
                }

            }, null, 0, 1000 / fps);
        }

        public void Stop()
        {
            if (timer != null)
            {
                using (ManualResetEvent timerDisposed = new ManualResetEvent(false))
                {
                    _ = timer.Dispose(timerDisposed);
                    _ = timerDisposed.WaitOne();
                }
            }
            timer = null;
        }

        public VideoCaptureSettings GetCaptureSettings()
        {
            VideoCaptureSettings settings = new VideoCaptureSettings
            {
                Width = width,
                Height = height,
                Fps = fps,
                MirrorOnLocalRender = false,
                PixelFormat = OpenTok.PixelFormat.FormatYuv420p
            };

            return settings;
        }
    }
    */
    public partial class MainWindow : Window
    {
        private const string API_KEY = "47521351";
        private const string SESSION_ID = "1_MX40NzUyMTM1MX5-MTY4NjY3NDE0ODk1NH5sUFdKVkczcFNsblkzY25XRVpQUXVkbzd-fn4";
        private const string TOKEN = "T1==cGFydG5lcl9pZD00NzUyMTM1MSZzaWc9YTExMzZlNzQ1ZGVmMGNiZmYwMmI2NDQ3YzQxNzAwZjk4MDVkMDcxYjpzZXNzaW9uX2lkPTFfTVg0ME56VXlNVE0xTVg1LU1UWTROalkzTkRFME9EazFOSDVzVUZkS1ZrY3pjRk5zYmxrelkyNVhSVnBRVVhWa2J6ZC1mbjQmY3JlYXRlX3RpbWU9MTY4NjY3NDE1NSZub25jZT0wLjk4ODI2MTkxMzE0MTQ0OTImcm9sZT1wdWJsaXNoZXImZXhwaXJlX3RpbWU9MTY4OTI2NjE1NCZpbml0aWFsX2xheW91dF9jbGFzc19saXN0PQ==";

        private Context context;
        private Session Session;
        private Publisher Publisher;

        public MainWindow()
        {
            InitializeComponent();

            context = new Context(new WPFDispatcher());

            // Uncomment following line to get debug logging
            // LogUtil.Instance.EnableLogging();

            IList<VideoCapturer.VideoDevice> capturerDevices = VideoCapturer.EnumerateDevices();
            if (capturerDevices == null || capturerDevices.Count == 0)
                throw new Exception("No video capture devices detected");
            //VideoCapturer.Resolution resolution = VideoCapturer.Resolution.High;
            //VideoCapturer.FrameRate frameRate = VideoCapturer.FrameRate.Fps30;

            //IVideoCapturer videoCapturer = videoCapturer = new ScreenCapturer(resolution, frameRate);

            //Publisher = new Publisher.Builder(Context.Instance)
            //{
            //    Renderer = PublisherVideo,
            //    Capturer = videoCapturer
            //}.Build();


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

            //VideoTransformer backgroundBlur = new VideoTransformer("BackgroundBlur", "{\"radius\":\"High\"}");
            ICustomVideoTransformer customBorder = new borderTransformer();
            VideoTransformer border = new VideoTransformer("border", customBorder);

            List<VideoTransformer> transformers = new List<VideoTransformer>
            {
                //backgroundBlur,
                border
            };

            // List of video transformers
            Publisher.VideoTransformers = transformers;

            // Clear Video Transformers
            // publisher.VideoTransformers.Clear();
            // List<VideoTransformer> transformers = new List<VideoTransformer> { };
            // Publisher.VideoTransformers = transformers;

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

        public class borderTransformer : ICustomVideoTransformer
        {
            private Bitmap CreateBitmapFromYuv420pFrame(VideoFrame frame)
            {
                // Get the Y, U, and V planes from the YUV420p frame
                IntPtr[] planes = new IntPtr[3];
                planes[0] = frame.GetPlane(0); // Y plane
                planes[1] = frame.GetPlane(1); // U plane
                planes[2] = frame.GetPlane(2); // V plane

                // Get the strides for the Y, U, and V planes
                int[] strides = new int[3];
                strides[0] = frame.GetPlaneStride(0); // Y plane stride
                strides[1] = frame.GetPlaneStride(1); // U plane stride
                strides[2] = frame.GetPlaneStride(2); // V plane stride

                // Create a new Bitmap with the same width and height as the video frame
                Bitmap bitmap = new Bitmap(frame.Width, frame.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // Lock the bitmap's data
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

                // Copy the YUV420p data to the bitmap's data
                for (int i = 0; i < 3; i++)
                {
                    IntPtr plane = planes[i];
                    int stride = strides[i];
                    int planeHeight = frame.GetPlaneHeight(i);
                    int planeSize = frame.GetPlaneSize(i);

                    unsafe
                    {
                        byte* planePtr = (byte*)plane.ToPointer();
                        byte* destinationPtr = (byte*)bitmapData.Scan0 + i * bitmapData.Stride;

                        for (int y = 0; y < planeHeight; y++)
                        {
                            // Copy a row of data from the plane to the bitmap
                            for (int x = 0; x < stride; x++)
                            {
                                byte* planePixel = planePtr + y * stride + x;
                                byte* destinationPixel = destinationPtr + y * bitmapData.Stride + x;

                                *destinationPixel = *planePixel;
                            }
                        }
                    }
                }

                // Unlock the bitmap's data
                bitmap.UnlockBits(bitmapData);

                return bitmap;
            }

            public Bitmap ResizeImage(Bitmap image, int width, int height)
            {
                float scaleX = width / (float)image.Width;
                float scaleY = height / (float)image.Height;

                // Create a new Bitmap with the desired width and height
                Bitmap resizedImage = new Bitmap(width, height);

                // Create a Graphics object from the resized image
                using (Graphics graphics = Graphics.FromImage(resizedImage))
                {
                    // Set the interpolation mode to high quality
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // Apply the scaling transformation to the Graphics object
                    graphics.ScaleTransform(scaleX, scaleY);

                    // Draw the original image onto the resized image
                    graphics.DrawImage(image, 0, 0);
                }

                return resizedImage;
            }


            public void Transform(VideoFrame frame)
            {
                // Get the Y, U, and V planes from the YUV420p frame
                IntPtr[] planes = new IntPtr[3];
                planes[0] = frame.GetPlane(0); // Y plane
                planes[1] = frame.GetPlane(1); // U plane
                planes[2] = frame.GetPlane(2); // V plane

                // Get the strides for the Y, U, and V planes
                int[] strides = new int[3];
                strides[0] = frame.GetPlaneStride(0); // Y plane stride
                strides[1] = frame.GetPlaneStride(1); // U plane stride
                strides[2] = frame.GetPlaneStride(2); // V plane stride

                // Convert the VideoFrame to a YUV420p frame
                VideoFrame yuvFrame = VideoFrame.CreateYuv420pFrameFromBuffer(frame.PixelFormat, frame.Width, frame.Height, planes, strides);

                // Create a new Bitmap from the YUV420p frame
                Bitmap videoFrameBitmap = CreateBitmapFromYuv420pFrame(yuvFrame);

                // Load the image
                Bitmap image = new Bitmap("Resources/Vonage_Logo.png");

                // Calculate the desired size of the image
                int desiredWidth = frame.Width / 8; // Adjust this value as needed
                int desiredHeight = (int)(image.Height * ((float)desiredWidth / image.Width));

                // Resize the image to the desired size
                image = ResizeImage(image, desiredWidth, desiredHeight);

                // Calculate the position of the image
                int imageX = frame.Width * 1 / 5; // X-coordinate of the image
                int imageY = frame.Height * 1/5; // Y-coordinate of the image

                // Create a Graphics object from the video frame bitmap
                Graphics graphics = Graphics.FromImage(videoFrameBitmap);

                // Draw the image onto the video frame bitmap
                graphics.DrawImage(image, imageX, imageY);

                // Dispose of the Graphics object and the image
                graphics.Dispose();
                image.Dispose();

                // Perform further operations with the transformed video frame...
            }
        }
    }
}
