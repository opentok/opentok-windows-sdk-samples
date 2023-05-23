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
        public bool EnableWatermark;
        public bool AutoResize;

        public WriteableBitmap Watermark
        {
            get; private set;
        }
        public float RelativeSize
        {
            get; set;
        }

        static SampleVideoRenderer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SampleVideoRenderer), new FrameworkPropertyMetadata(typeof(SampleVideoRenderer)));
        }

        public SampleVideoRenderer()
        {
            var brush = new ImageBrush();
            brush.Stretch = Stretch.UniformToFill;
            Background = brush;

            Watermark = CreateFromResource("Data/vonage-logo-white-913x200.png");
        }

        public void RenderFrame(VideoFrame frame)
        {
            // WritableBitmap has to be accessed from a STA thread
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    WriteableBitmap watermark = Watermark;
                    Rect position = new Rect(10, 10, Watermark.PixelWidth, Watermark.PixelHeight);

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

                        if (AutoResize)
                        {
                            int w = Watermark.PixelWidth;
                            int h = Watermark.PixelHeight;

                            float ratio = (float)w / h;
                            if (ratio > 1)
                            {
                                w = (int)(FrameWidth * RelativeSize);
                                h = (int)(w / ratio);
                            }
                            else
                            {
                                h = (int)(FrameHeight * RelativeSize);
                                w = (int)(h * ratio);
                            }

                            watermark = ResizeWritableBitmap(Watermark, w, h);
                            position = new Rect(10, 10, w, h);
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

                            if (EnableWatermark)
                            {
                                VideoBitmap.Blit(position, watermark, new Rect(0, 0, position.Width, position.Height));
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

        private WriteableBitmap CreateFromResource(string resource)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(resource, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return new WriteableBitmap(bi);
        }

        private static WriteableBitmap ResizeWritableBitmap(WriteableBitmap wBitmap, int reqWidth, int reqHeight)
        {
            int Stride = wBitmap.PixelWidth * ((wBitmap.Format.BitsPerPixel + 7) / 8);
            int NumPixels = Stride * wBitmap.PixelHeight;
            ushort[] ArrayOfPixels = new ushort[NumPixels];


            wBitmap.CopyPixels(ArrayOfPixels, Stride, 0);

            int OriWidth = (int)wBitmap.PixelWidth;
            int OriHeight = (int)wBitmap.PixelHeight;

            double nXFactor = (double)OriWidth / (double)reqWidth;
            double nYFactor = (double)OriHeight / (double)reqHeight;

            double fraction_x, fraction_y, one_minus_x, one_minus_y;
            int ceil_x, ceil_y, floor_x, floor_y;

            ushort pix1, pix2, pix3, pix4;
            int nStride = reqWidth * ((wBitmap.Format.BitsPerPixel + 7) / 8);
            int nNumPixels = reqWidth * reqHeight;
            ushort[] newArrayOfPixels = new ushort[nNumPixels];

            for (int y = 0; y < reqHeight; y++)
            {
                for (int x = 0; x < reqWidth; x++)
                {
                    // Setup
                    floor_x = (int)Math.Floor(x * nXFactor);
                    floor_y = (int)Math.Floor(y * nYFactor);

                    ceil_x = floor_x + 1;
                    if (ceil_x >= OriWidth) ceil_x = floor_x;

                    ceil_y = floor_y + 1;
                    if (ceil_y >= OriHeight) ceil_y = floor_y;

                    fraction_x = x * nXFactor - floor_x;
                    fraction_y = y * nYFactor - floor_y;

                    one_minus_x = 1.0 - fraction_x;
                    one_minus_y = 1.0 - fraction_y;

                    pix1 = ArrayOfPixels[floor_x + floor_y * OriWidth];
                    pix2 = ArrayOfPixels[ceil_x + floor_y * OriWidth];
                    pix3 = ArrayOfPixels[floor_x + ceil_y * OriWidth];
                    pix4 = ArrayOfPixels[ceil_x + ceil_y * OriWidth];

                    ushort g1 = (ushort)(one_minus_x * pix1 + fraction_x * pix2);
                    ushort g2 = (ushort)(one_minus_x * pix3 + fraction_x * pix4);
                    ushort g = (ushort)(one_minus_y * (double)(g1) + fraction_y * (double)(g2));
                    newArrayOfPixels[y * reqWidth + x] = g;
                }
            }

            WriteableBitmap newWBitmap = new WriteableBitmap(reqWidth, reqHeight, 96, 96, PixelFormats.Gray16, null);
            Int32Rect Imagerect = new Int32Rect(0, 0, reqWidth, reqHeight);
            int newStride = reqWidth * ((PixelFormats.Gray16.BitsPerPixel + 7) / 8);
            newWBitmap.WritePixels(Imagerect, newArrayOfPixels, newStride, 0);
            return newWBitmap;
        }
    }
}
