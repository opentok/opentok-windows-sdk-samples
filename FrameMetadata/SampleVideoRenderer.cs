using OpenTok;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FrameMetadata
{
    public class SampleVideoRenderer : Control, IVideoRenderer
    {
        private int FrameWidth = -1;
        private int FrameHeight = -1;
        private WriteableBitmap VideoBitmap;

        public String StreamSourceType; // Used to identify the renderer as being a Publisher or a Subscriber renderer 

        static SampleVideoRenderer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SampleVideoRenderer), new FrameworkPropertyMetadata(typeof(SampleVideoRenderer)));
        }

        public SampleVideoRenderer()
        {
            var brush = new ImageBrush();
            brush.Stretch = Stretch.UniformToFill;
            Background = brush;
        }

        public void RenderFrame(VideoFrame frame)
        {
            if (frame.Metadata != null)
            {
                Trace.WriteLine(StreamSourceType + " video frame metadata: " + Encoding.ASCII.GetString(frame.Metadata));
            }
            // WritableBitmap has to be accessed from a STA thread
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (frame.Width != FrameWidth || frame.Height != FrameHeight)
                    {
                        FrameWidth = frame.Width;
                        FrameHeight = frame.Height;
                        VideoBitmap = new WriteableBitmap(FrameWidth, FrameHeight, 96, 96, PixelFormats.Bgr32, null);

                        if (Background is ImageBrush)
                        {
                            ImageBrush b = (ImageBrush)Background;
                            b.ImageSource = VideoBitmap;
                        }
                        else
                        {
                            throw new Exception("Please use an ImageBrush as background in the SampleVideoRenderer control");
                        }
                    }

                    if (VideoBitmap != null)
                    {
                        VideoBitmap.Lock();
                        {
                            IntPtr[] buffer = { VideoBitmap.BackBuffer };
                            int[] stride = { VideoBitmap.BackBufferStride };
                            frame.ConvertInPlace(OpenTok.PixelFormat.FormatArgb32, buffer, stride);
                        }
                        VideoBitmap.AddDirtyRect(new Int32Rect(0, 0, FrameWidth, FrameHeight));
                        VideoBitmap.Unlock();
                    }
                }
                finally
                {
                    frame.Dispose();
                }
            }));
        }
    }
}
