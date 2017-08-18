Custom Video Renderer
==================================

This project uses the custom video renderer features in the OpenTok Windows SDK.
By the end of a code review, you should have a basic understanding of the
internals of the video render API.

*Important:* To use this application, follow the instructions in the
[Quick Start](../README.md#quick-start) section of the main README file
for this repository.

VideoRenderer.cs
----------------------

For our example we create a new class Called VideoRenderer that will implement the `IVideoRenderer` interface.

That interface constains just one method, `RenderFrame`. This method will be called everytime a new frame is ready to be drawed.

In order to draw the frames, the VideoRenderer class is also a WPF Control, and will use its inherited `background` property to fill it with the contents of the frame. In order to do so, we use a `WriteableBitmap` whcih will be updated in every frame using this code:

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
  frame.ConvertInPlace(OpenTok.PixelFormat.FormatArgb32,buffer,
    stride);

  videoBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
  videoBitmap.Unlock();

  // ...
```

Being a subclass of a WPF control, allow us to directly place it in the xaml file:

```xaml
<local:VideoRenderer
  x:Name="PublisherVideo"
  HorizontalAlignment="Right" VerticalAlignment="Bottom"
  Width="184" Height="114"
  Margin="0,0,10.429,10.143"
  BorderBrush="#FF5B1919" BorderThickness="1" >
  <local:VideoRenderer.Effect>
    <DropShadowEffect Opacity="0.6"/>
  </local:VideoRenderer.Effect>
  <local:VideoRenderer.Background>
    <ImageBrush Stretch="UniformToFill">
    </ImageBrush>
  </local:VideoRenderer.Background>
</local:VideoRenderer>
```

MainWindow.xaml.cs
----------------------------

In order to use the new renderer, we need to tell the `Publisher` to use it. We can do it with this code:

```csharp
Publisher = new Publisher(Context.Instance,
  renderer: PublisherVideo,
  capturer: Capturer);
```
