using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using OpenTok;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BasicVideoChatUWP
{
    public sealed partial class UWPVideoRenderer : UserControl, IVideoRenderer
    {
        private int width = -1;
        private int height = -1;
        private WriteableBitmap videoBitmap;
        private byte[] pixelBytes;
        private GCHandle pinnedPixelBytes;

        public UWPVideoRenderer()
        {
            InitializeComponent();
        }

        public async void RenderFrame(OpenTok.VideoFrame frame)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(async () =>
            {
                try
                {
                    if (frame.Width != width || frame.Height != height)
                    {
                        width = frame.Width;
                        height = frame.Height;

                        videoBitmap = new WriteableBitmap(width, height);
                        ((ImageBrush)MainGrid.Background).ImageSource = videoBitmap;

                        if (pinnedPixelBytes.IsAllocated)
                        {
                            pinnedPixelBytes.Free();
                        }
                        pixelBytes = new byte[width * height * 4]; //Based on PixelFormat.FormatArgb32 
                        pinnedPixelBytes = GCHandle.Alloc(pixelBytes, GCHandleType.Pinned);
                    }

                    if (videoBitmap != null)
                    {
                        IntPtr[] buffer = { pinnedPixelBytes.AddrOfPinnedObject() };
                        int[] stride = { 4 * width }; //Based on PixelFormat.FormatArgb32 
                        frame.ConvertInPlace(PixelFormat.FormatArgb32, buffer, stride);

                        using (System.IO.Stream videoBitmapStream = videoBitmap.PixelBuffer.AsStream())
                        {
                            await videoBitmapStream.WriteAsync(pixelBytes, 0, pixelBytes.Length);
                        }

                        videoBitmap.Invalidate();
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