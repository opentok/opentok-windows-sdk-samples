Screen Sharing
=========================

This project shows how to use OpenTok Windows SDK to publish a stream that uses
the content of the screen as the video source.

*Important:* To use this application, follow the instructions in the
[Quick Start](../README.md#quick-start) section of the main README file
for this repository.

ScreenSharingCapturer.cs
---------------------------------

This is the core class of the sample, this class will capture the contents of the
screen and will send the frames to the OpenTok SDK to publish them.

To be able to provide frames to the OpenTok SDK you need to implement the
`IVideoCapturer` interface. This is also known as building your own video Capturer.

To implement this interface you need to provide 5 methods:

```csharp
void Destroy();
void Init();
void Start(IVideoFrameConsumer frameConsumer);
void Stop();
VideoCaptureSettings GetCaptureSettings();
```

OpenTok SDK will manage the capturer lifecycle by calling `Init`, `Start`, `Stop`
and `Destroy` methods. First you need to return your capture settings in the
`GetCaptureSettings` method. Note also that the `Start` method contains a parameter
called `frameConsumer`. You need to save that parameter inside your class, since
you will use it to provide a frame to the SDK.

In the ScreenSharing capturer sample, we return this settings:

```csharp
public VideoCaptureSettings GetCaptureSettings()
{
    VideoCaptureSettings settings = new VideoCaptureSettings();
    settings.Width = width; // DesktopBounds.Right;
    settings.Height = height; // DesktopBounds.Bottom;
    settings.Fps = FPS; // 16
    settings.MirrorOnLocalRender = false;
    settings.PixelFormat = PixelFormat.FormatYuv420p;
    return settings;
}
```

Whenever we have a frame ready, we provide it to the SDK by calling:

```csharp
using (var frame = VideoFrame.CreateYuv420pFrameFromBuffer(PixelFormat.FormatArgb32,
    width, height,
    planes, strides)) // Being planes and strides the actual frame data
{
    frameConsumer.Consume(frame);
}
```

#### Capturing the screen

In this sample we use SharpDX (A DirectX c# wrapper) to capture the screen contents.
To create the video, we use a timer which is scheduled to be called 15 times a second. In each tick, the timer will capture the screen and will provide the frame to the SDK by using the capturer interface.

MainWindow.xaml.cs
--------------------------

In order to use the capturer, we need to tell the Publisher to use it. We do that with this code:

```csharp
Capturer = new ScreenSharingCapturer();

Publisher = new Publisher(Context.Instance, 
  renderer: PublisherVideo,
  capturer: Capturer,
  hasAudioTrack: false)
```
