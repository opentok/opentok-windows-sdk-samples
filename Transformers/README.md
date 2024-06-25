Transformers
==============

Transformers is a simple application meant to get a new developer started using the
OpenTok Windows SDK Transformers API. For a full description, see the [Basic tutorial at the OpenTok developer
center](https://tokbox.com/developer/tutorials/windows/basic-video-chat/).

*Important:* To use this application, follow the instructions in the
[Quick Start](../README.md#quick-start) section of the main README file for this repository.

<p class="topic-summary">
You can use pre-built transformers in the Vonage Media Processor library or create your own custom video transformer to apply to published video/audio.
</p>

You can use the
<a href="/developer/sdks/windows/reference/class_open_tok_1_1_publisher.html#a316a588accfef236c10cbd6bb139247d"><code>Publisher.VideoTransformers</code></a>
properties to apply video transformers to a stream.

<div class="important">
  <p>
  <b>Important:</b>
  </p>
  <p>
  <ul>
    <li>The video transformer API is a beta feature.</li>
    <li>NVIDIA GPUs are recommended for optimal performance.</li>
  </ul>
  </p>
</div>

For video, you can apply the background blur video transformer included in the Vonage Media Library.

You can also create your own custom video transformers.

## Applying a video transformer from the Vonage Media Library

Use the <a href="/developer/sdks/windows/reference/class_open_tok_1_1_video_transformer.html#a3f29fa4e726cfcb9c6c78affc8da8639"><code>VideoTransformer(string  name, string  properties)</code></a>
method to create a video transformer that uses a named transformer from the Vonage Media Library.

Currently, only one transformer is supported: background blur. Set the `name` parameter to `"BackgroundBlur"`.
Set the `properties` parameter to a JSON string defining properties for the transformer.
For the background blur transformer, this JSON includes one property -- `radius` -- which can be set
to `"High"`, `"Low"`, or `"None"`.

```csharp
VideoTransformer backgroundBlur = new VideoTransformer("BackgroundBlur", "{\"radius\":\"High\"}");
List<VideoTransformer> transformers = new ArrayList<VideoTransformer> 
{
  backgroundBlur
};
publisher.VideoTransformers = transformers;
```

## Creating a custom video transformer

Create a class that implements the <a href="/developer/sdks/windows/reference/interface_open_tok_1_1_i_custom_video_transformer.html"><code>ICustomVideoTransformer</code></a> 
interface. Implement the `ICustomVideoTransformer.Transform()` method, applying a transformation to the `VideoFrame` object passed into the method.
The `ICustomVideoTransformer.Transform` method is triggered for each video frame:

```csharp
public class MyCustomTransformer : IVideoTransformer
{
    public void Transform(VideoFrame frame)
    {
        // transformer implementation
    }
}
```

In this sample, to display one of the infinite transformations that can be applied to video frames, a logo is being added to the bottom right corner of the video.

```csharp
public void Transform(VideoFrame frame)
{
    // Obtain the Y plane of the video frame
    IntPtr yPlane = frame.GetPlane(0);

    // Get the dimensions of the video frame
    int videoWidth = frame.Width;
    int videoHeight = frame.Height;

    // Get the dimensions of the logo
    Bitmap image = new Bitmap("Resources/Vonage_Logo.png");

    // Calculate the desired size of the image
    int desiredWidth = videoWidth / 10; // Adjust this value as needed
    int desiredHeight = (int)(image.Height * ((float)desiredWidth / image.Width));

    // Resize the image to the desired size
    Bitmap newimage = ResizeImage(image, desiredWidth, desiredHeight);

    int logoWidth = newimage.Width;
    int logoHeight = newimage.Height;

    // Location of the image (top right corner of video)
    int logoPositionX = videoWidth * 5/6 - logoWidth; // Adjust this as needed for the desired position
    int logoPositionY = videoHeight * 1/7 - logoHeight; // Adjust this as needed for the desired position

    // Overlay the logo on the video frame
    for (int y = 0; y < logoHeight; y++)
    {
        for (int x = 0; x < logoWidth; x++)
        {
            int frameX = logoPositionX + x;
            int frameY = logoPositionY + y;

            if (frameX >= 0 && frameX < videoWidth && frameY >= 0 && frameY < videoHeight)
            {
                int frameOffset = frameY * videoWidth + frameX;

                // Get the logo pixel color
                Color logoColor = newimage.GetPixel(x, y);

                // Extract the color channels (ARGB)
                int logoAlpha = logoColor.A;
                int logoRed = logoColor.R;

                // Overlay the logo pixel on the video frame
                int framePixel = Marshal.ReadByte(yPlane, frameOffset) & 0xFF;

                // Calculate the blended pixel value
                int blendedPixel = ((logoAlpha * logoRed + (255 - logoAlpha) * framePixel) / 255) & 0xFF;

                // Set the blended pixel value in the video frame
                Marshal.WriteByte(yPlane, frameOffset, (byte)blendedPixel);
            }
        }
    }
}
```

Then set the `PublisherKit.VideoTransformers` property to an array that includes the object that implements the
OTCustomVideoTransformer interface:

```csharp
MyCustomTransformer myCustomTransformer = new();
List<VideoTransformer> transformers = new ArrayList<VideoTransformer> 
{
  myCustomTransformer
};
publisher.VideoTransformers = transformers;
```

You can combine the Vonage Media library transformer (see the previous section) with custom transformers or apply
multiple custom transformers by adding multiple VideoTransformer objects to the ArrayList used
for the `PublisherKit.VideoTransformers` property.


## Clearing video transformers for a publisher

To clear video transformers for a publisher, set the `Publisher.VideoTransformers` property to an empty array.

```csharp
publisher.VideoTransformers =  new ArrayList<VideoTransformer> {};
```
