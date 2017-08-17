using OpenTok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomRendererSample
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CustomRendererSample"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CustomRendererSample;assembly=CustomRendererSample"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:VideoRenderer/>
    ///
    /// </summary>
    public class VideoRenderer : Control, IVideoRenderer
    {
        private int width = -1;
        private int height = -1;
        private WriteableBitmap videoBitmap;

        public bool EnableBlueFilter;

        static VideoRenderer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VideoRenderer), new FrameworkPropertyMetadata(typeof(VideoRenderer)));
        }

        public VideoRenderer()
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
                    if (frame.Width != width || frame.Height != height)
                    {
                        width = frame.Width;
                        height = frame.Height;
                        videoBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);

                        if (Background is ImageBrush)
                        {
                            ImageBrush b = (ImageBrush)Background;
                            b.ImageSource = videoBitmap;
                        }
                        else
                        {
                            throw new Exception("Please use an ImageBrush as background in the VideoRenderer control");
                        }
                    }

                    if (videoBitmap != null)
                    {
                        videoBitmap.Lock();
                        {
                            IntPtr[] buffer = { videoBitmap.BackBuffer };
                            int[] stride = { videoBitmap.BackBufferStride };
                            frame.ConvertInPlace(OpenTok.PixelFormat.FormatArgb32, buffer, stride);

                            if (EnableBlueFilter)
                            {
                                // This is a very slow filter just for demonstration purposes
                                IntPtr p = videoBitmap.BackBuffer;
                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < width; x++, p += 4)
                                    {
                                        Marshal.WriteInt32(p, Marshal.ReadInt32(p) & 0xff);
                                    }
                                    p += stride[0] - width * 4;
                                }
                            }
                        }
                        videoBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                        videoBitmap.Unlock();
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
