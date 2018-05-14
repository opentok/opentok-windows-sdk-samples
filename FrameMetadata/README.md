FrameMetadata
=====================

This app shows how to add metadata to video frames in a published stream and how to read
the metadata in a subscriber to the stream.

This project uses a custom video capturers to provide video frames for the publisher.
And it uses a custom video renderer to render video frames for the publisher and subscriber.
When creating each video frame, the video capturer adds frame metadata (a timestamp). And
when rendering each video frame, the video renderer reads the frame metadata and logs it to
the console.

*Important:* To use this application, follow the instructions in the
[Quick Start](../README.md#quick-start) section of the main README file
for this repository.

SampleVideoCapturer.cs
----------------------

The SampleVideoCapturer class implements the `IVideoCapturer` interface defined in the OpenTok
Windows SDK. 

The `Init()` method of the SampleVideoCapturer (inherited from the `IVideoCapturer`
interface) is called when the video capturer is initialized. An object that implements
the `IVideoFrameConsumer` interface (defined by the OpenTok Windows SDK) is passed into
the method:

```csharp
public void Init(IVideoFrameConsumer frameConsumer)
{
    this.frameConsumer = frameConsumer;
}
```

The application calls the `Start()` method of the video capturer after the capturer is created.
(This method is inherited from the `IVideoCapturer` interface.) In its implementation of the method, the SampleVideoCapturer instantiates a Timer:

```csharp
public void Start()
{
    timer = new Timer(1000);
    timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
    timer.Enabled = true;
}
```

The `OnTimedEvent()` method is called at each interval of the timer. It creates an instance of
the VideoFrame class (also defined by the OpenTok Windows SDK), which represents the video frame
to be used by the video capturer:

```csharp
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
```

In this sample application, the video capturer renders a green rectangle to be used as the
bitmap source for each video frame.

When composing the frame, the application sets the `Metadata` property of the VideoFrame object
to a DateTime timestamp.


SampleVideoRenderer.cs
----------------------

The SampleVideoRenderer class implements the `IVideoRenderer` interface defined in the OpenTok
Windows SDK. That interface contains one method: `RenderFrame(VideoFrame)`. This method is called
when a new frame is ready to be drawn.

This sample application simply logs the frame metadata (if there is any) to the console:

```csharp
public void RenderFrame(VideoFrame frame)
{
    if (frame.Metadata != null)
    {
        Trace.WriteLine("Video frame metadata: " + Encoding.ASCII.GetString(frame.Metadata));
    }
    // ...
}
```

To draw the frames, the SampleVideoRenderer class is a WPF Control, and it uses the inherited
`Background` property to fill the control with the contents of the frame. The SampleVideoRenderer
class creates a `WriteableBitmap` object, which is updated with every frame (when the `RenderFrame`
method is called). The `frame` object passed into the `RenderFrame(VideoFrame)` method is an
instance of the VideoFrame class, defined by the OpenTok Windows SDK. The
`ConvertInPlace(destinationFormat, planes, strides)` method of this object copies the video frame
to the `BackBuffer` property of the WriteableBitmap object:

```csharp
public void RenderFrame(VideoFrame frame)
{
  // This code has been simplified for the sake of clarity.
  // Please refer to the actual class to get the whole sample.
  // ...
  videoBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
  ImageBrush b = (ImageBrush)Background;
  b.ImageSource = videoBitmap;
  // ...
  IntPtr[] buffer = { videoBitmap.BackBuffer };
  int[] stride = { videoBitmap.BackBufferStride };
  frame.ConvertInPlace(OpenTok.PixelFormat.FormatArgb32, buffer, stride);

  videoBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
  videoBitmap.Unlock();

  // ...
```

Because SampleVideoRenderer extends the WPF Control class, we can reference it in
the MainWindow.xaml file:

```xaml
<local:SampleVideoRenderer
  x:Name="PublisherVideo"
  HorizontalAlignment="Right" VerticalAlignment="Bottom"
  Width="184" Height="114"
  Margin="0,0,10.429,10.143"
  BorderBrush="#FF5B1919" BorderThickness="1" >
  <local:SampleVideoRenderer.Effect>
    <DropShadowEffect Opacity="0.6"/>
  </local:SampleVideoRenderer.Effect>
  <local:SampleVideoRenderer.Background>
    <ImageBrush Stretch="UniformToFill">
    </ImageBrush>
  </local:SampleVideoRenderer.Background>
</local:SampleVideoRenderer>
```

MainWindow.xaml.cs
------------------

In order to use the custom video renderer for the publisher, we pass it in as
the `renderer` parameter of the `Publisher()` constructor:

```csharp
PublisherVideo.StreamSourceType = "Publisher";
Publisher = new Publisher(Context.Instance,
  renderer: PublisherVideo,
  capturer: Capturer);
```

The `SampleVideoRenderer` class includes a public `StreamSourceType`, which we set to
identify the renderer as belonging to a Publisher or Subscriber.

In order to use the custom video renderer for the subscriber, we create a new instance
of the SampleVideoRenderer class and pass it in as the `renderer` parameter of the
`Subscriber()` constructors:

```csharp
SampleVideoRenderer renderer = new SampleVideoRenderer();
renderer.StreamSourceType = "Subscriber";
Subscriber subscriber = new Subscriber(Context.Instance, e.Stream, renderer);
```

To use the custom video capturer for the publisher, we create a new instance of the
SampleVideoCapturer class, and pass it in as the `capturer` parameter of the `Publisher()`
constructor:

```csharp
Capturer = new SampleVideoCapturer();
Publisher = new Publisher(Context.Instance,
  renderer: PublisherVideo,
  capturer: Capturer);
```
