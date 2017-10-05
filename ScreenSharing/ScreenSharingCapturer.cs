using OpenTok;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Threading;

namespace ScreenSharing
{
    public class ScreenSharingCapturer : IVideoCapturer
    {
        const int FPS = 15;
        int width;
        int height;
        Timer timer;
        IVideoFrameConsumer frameConsumer;

        Texture2D screenTexture;
        OutputDuplication duplicatedOutput;

        public void Init(IVideoFrameConsumer frameConsumer)
        {
            this.frameConsumer = frameConsumer;
        }

        public void Start()
        {
            const int numAdapter = 0;

            // Change the output number to select a different desktop
            const int numOutput = 0;

            var factory = new Factory1();
            var adapter = factory.GetAdapter1(numAdapter);
            var device = new SharpDX.Direct3D11.Device(adapter);

            var output = adapter.GetOutput(numOutput);
            var output1 = output.QueryInterface<Output1>();

            // When you have a multimonitor setup, the coordinates might be a little bit strange
            // depending on how you've setup the environment.
            // In any case Right - Left should give the width, and Bottom - Top the height.
            var desktopBounds = output.Description.DesktopBounds;
            width = desktopBounds.Right - desktopBounds.Left;
            height = desktopBounds.Bottom - desktopBounds.Top;

            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            screenTexture = new Texture2D(device, textureDesc);
            duplicatedOutput = output1.DuplicateOutput(device);

            timer = new Timer((Object stateInfo) =>
            {
                try
                {
                    SharpDX.DXGI.Resource screenResource;
                    OutputDuplicateFrameInformation duplicateFrameInformation;

                    duplicatedOutput.AcquireNextFrame(1000 / FPS, out duplicateFrameInformation, out screenResource);

                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                        device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                    screenResource.Dispose();
                    duplicatedOutput.ReleaseFrame();

                    var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read,
                                                                           SharpDX.Direct3D11.MapFlags.None);

                    IntPtr[] planes = { mapSource.DataPointer };
                    int[] strides = { mapSource.RowPitch };
                    using (var frame = VideoFrame.CreateYuv420pFrameFromBuffer(PixelFormat.FormatArgb32, width, height,
                                                                               planes, strides))
                    {
                        frameConsumer.Consume(frame);
                    }

                    device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                }
                catch (SharpDXException) { }
            }, null, 0, 1000 / FPS);

            output1.Dispose();
            output.Dispose();
            adapter.Dispose();
            factory.Dispose();
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
          duplicatedOutput.Dispose();
          screenTexture.Dispose();
        }

    public VideoCaptureSettings GetCaptureSettings()
        {
            VideoCaptureSettings settings = new VideoCaptureSettings();
            settings.Width = width;
            settings.Height = height;
            settings.Fps = FPS;
            settings.MirrorOnLocalRender = false;
            settings.PixelFormat = PixelFormat.FormatYuv420p;
            return settings;
        }
    }
}
