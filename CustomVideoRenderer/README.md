CustomVideoRenderer
=====================

This project uses the custom video renderer features in the OpenTok Windows SDK.
By the end of a code review, you should have a basic understanding of the
internals of the OpenTok video renderer API.

Most applications work fine with the default renderer (VideoRenderer) included with the OpenTok
Windows SDK. However, you can define a custom video renderer to add custom effects. For example,
when the user clicks a button, this sample application adds a blue filter to the rendered video.

*Important:* To use this application, follow the instructions in the
[Quick Start](../README.md#quick-start) section of the main README file
for this repository.

SampleVideoRenderer.cs
----------------------

The SampleVideoRenderer class implements the `IVideoRenderer` interface defined in the OpenTok
Windows SDK. That interface contains one method: `RenderFrame(VideoFrame)`. This method is called
when a new frame is ready to be drawn.

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
  // This code has been simplified for the sake of clarity
  // Please refer to the actual class to get the whole sample
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

Because SampleVideoRenderer extends the WPF Control class, we can reference it in the MainWindow.xaml
file:

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

In order to use the new renderer, we pass it in as the `renderer` parameter of the
`Publisher()` constructor:

```csharp
Publisher = new Publisher(Context.Instance,
  renderer: PublisherVideo,
  capturer: Capturer);
```
