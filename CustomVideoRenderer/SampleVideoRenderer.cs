using OpenTok;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CustomVideoRenderer
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CustomVideoRenderer"
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CustomVideoRenderer;assembly=SampleVideoRenderer"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right-click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:SampleVideoRenderer/>
    ///
    /// </summary>
    public class SampleVideoRenderer : Control, IVideoRenderer
    {
        private int FrameWidth = -1;
        private int FrameHeight = -1;
        private WriteableBitmap VideoBitmap;

        public bool EnableBlueFilter;

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

                            if (EnableBlueFilter)
                            {
                                // This is a very slow filter just for demonstration purposes
                                IntPtr p = VideoBitmap.BackBuffer;
                                for (int y = 0; y < FrameHeight; y++)
                                {
                                    for (int x = 0; x < FrameWidth; x++, p += 4)
                                    {
                                        Marshal.WriteInt32(p, Marshal.ReadInt32(p) & 0xff);
                                    }
                                    p += stride[0] - FrameWidth * 4;
                                }
                            }
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
