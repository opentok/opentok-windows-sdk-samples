using OpenTok;
using System;
using System.Drawing;
using System.Threading;

namespace ScreenSharing
{
    public class ScreenSharingCapturer : IVideoCapturer
    {
        Timer timer;
        IVideoFrameConsumer frameConsumer;
        const int WIDTH = 640;
        const int HEIGHT = 480;
        const int FPS = 30;        

        public void Init(IVideoFrameConsumer frameConsumer)
        {
            this.frameConsumer = frameConsumer;
        }

        public void Start()
        {
            timer = new Timer((Object stateInfo) =>
            {
                using (Bitmap bitmap = new Bitmap(WIDTH, HEIGHT, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap as Image))
                    {
                        graphics.CopyFromScreen(0, 0, 0, 0, new Size(WIDTH, HEIGHT), CopyPixelOperation.SourceCopy);
                    }
                    using (var frame = VideoFrame.CreateYuv420pFrameFromBitmap(bitmap))
                    {
                        frameConsumer.Consume(frame);
                    }
                }

            }, null, 0, 1000 / FPS);
        }

        public void Stop()
        {
            if (timer != null)
            {
                using (var timerDisposed = new ManualResetEvent(false))
                {
                    timer.Dispose(timerDisposed);
                    timerDisposed.WaitOne();
                }
            }
            timer = null;
        }

        public void Destroy()
        {
        }

        public VideoCaptureSettings GetCaptureSettings()
        {
            VideoCaptureSettings settings = new VideoCaptureSettings();
            settings.Width = WIDTH;
            settings.Height = HEIGHT;
            settings.Fps = FPS;
            settings.MirrorOnLocalRender = false;
            settings.PixelFormat = PixelFormat.FormatYuv420p;

            return settings;
        }
    }
}
