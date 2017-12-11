ScreenSharing
=============

This project shows how to use OpenTok Windows SDK to publish a stream that uses
the content of the screen as the video source for an OpenTok publisher.

*Important:* To use this application, follow the instructions in the
[Quick Start](../README.md#quick-start) section of the main README file
for this repository.

Application Notes
-----------------
  * This application uses Microsoft DirectX graphics APIs, which are not supported on
    Windows 7. This sample code works on Windows 8+.

ScreenSharingCapturer.cs
------------------------

This is the core class of the sample application. It captures the contents of the
screen and uses the frames as the video source for an OpenTok Publisher object.

To be able to provide frames to the OpenTok SDK you need to implement the
`IVideoCapturer` interface. This is also known as building your own video Capturer.

The app returns the capture settings in the implementation of the
`IVideoCapturer.GetCaptureSettings()` method:

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

The application implements the `Init(frameConsumer)`, `Start()`, `Stop()`, and `Destroy()` methods
defined by the `IVideoCapturer` interface of the OpenTok Windows SDK. The OpenTok SDK manages the
capturer lifecycle by calling these methods when the Publisher initializes the video capturer,
when it starts requesting frames, when it stops capturing frames, and when the capturer is
destroyed.

Note that the `Init` method contains a `frameConsumer` parameter. This object is defined by the
`IVideoFrameConsumer` interface of the OpenTok Windows SDK. The app saves that parameter value and
uses it to provide a frame to the custom video capturer for the Publisher.

Whenever a frame is ready, the app calls the `Consume(frame)` method of the `frameConsumer`
object (passing in the frame object):

```csharp
using (var frame = VideoFrame.CreateYuv420pFrameFromBuffer(PixelFormat.FormatArgb32,
    width, height,
    planes, strides)) // planes and strides the actual frame data
{
    frameConsumer.Consume(frame);
}
```

#### Capturing the screen

This sample uses SharpDX (a DirectX C# wrapper) to capture the screen contents. To create
the video, the app uses a timer, which is called 15 times a second. In each tick, the timer
captures the screen and provides the frame to custom video capturer by calling
`frameConsumer.Consume(frame)`.

MainWindow.xaml.cs
------------------

To use the capturer, pass it in as the `capturer` parameter of the `Publisher()` constructor:

```csharp
Capturer = new ScreenSharingCapturer();

Publisher = new Publisher(Context.Instance, 
  renderer: PublisherVideo,
  capturer: Capturer,
  hasAudioTrack: false)
```
