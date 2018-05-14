using OpenTok;
using System;
using System.Text;
using System.Timers;
using System.Drawing;

namespace FrameMetadata
{
    public class SampleVideoCapturer : IVideoCapturer
    {
        private IVideoFrameConsumer frameConsumer;
        private Timer timer;
        private int WIDTH = 320;
        private int HEIGHT = 240;

        public void Destroy()
        {
            timer.Dispose();
        }

        public VideoCaptureSettings GetCaptureSettings()
        {
            VideoCaptureSettings videoCaptureSettings = new VideoCaptureSettings();
            videoCaptureSettings.Fps = 1;
            return videoCaptureSettings;
        }

        public void Init(IVideoFrameConsumer frameConsumer)
        {
            this.frameConsumer = frameConsumer;
        }

        public void Start()
        {
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Bitmap bitmap = new Bitmap(WIDTH, HEIGHT);
            Graphics gfx = Graphics.FromImage(bitmap);
            SolidBrush brush = new SolidBrush(Color.FromArgb(255, 0, 255, 0));
            gfx.FillRectangle(brush, 0, 0, WIDTH, HEIGHT);
            VideoFrame frame = VideoFrame.CreateYuv420pFrameFromBitmap(bitmap);
            frame.Metadata = Encoding.ASCII.GetBytes(DateTime.Now.ToString("MM/dd/yyy hh:mm:ss.fff"));
            frameConsumer.Consume(frame);
            frame.Dispose();
        }

        public void Stop()
        {
            timer.Stop();
        }
    }
}
