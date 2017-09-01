SimpleMultiParty
================

SimpleMultiParty builds on the BasicVideoChat sample application, by adding features for
multi-party calls. See the [Basic tutorial at the OpenTok developer
center](https://tokbox.com/developer/tutorials/windows/basic-video-chat/) for a description
of how to connect to an OpenTok session and how to publish and subscribe to streams in a session.

*Important:* To use this application, follow the instructions in the
[Quick Start](../README.md#quick-start) section of the main README file for this repository.

MainWindow.xaml
---------------

The Grid element in the MainWindow.xaml includes a `SubscriberGrid` element, which is a
`UniformGrid` used to display the video renderers for the subscribers for each stream
in the OpenTok session.

```xml
<Grid>
  <UniformGrid x:Name="SubscriberGrid" Rows="1" Columns="0">
  </UniformGrid>
  <OpenTok:VideoRenderer x:Name="PublisherVideo" HorizontalAlignment="Right" Height="114" VerticalAlignment="Bottom" Width="184" Margin="0,0,10.429,10.143" BorderBrush="#FF5B1919" BorderThickness="1" >
    <OpenTok:VideoRenderer.Effect>
      <DropShadowEffect Opacity="0.6"/>
    </OpenTok:VideoRenderer.Effect>
    <OpenTok:VideoRenderer.Background>
      <ImageBrush Stretch="UniformToFill">
      </ImageBrush>
    </OpenTok:VideoRenderer.Background>
  </OpenTok:VideoRenderer>
  <Grid HorizontalAlignment="Left" Height="100" VerticalAlignment="Top" Width="100">
    <Button x:Name="ConnectDisconnectButton" Content="Connect" HorizontalAlignment="Left" Margin="10,10,0,70" Width="80" Click="Connect_Click"/>
  </Grid>
</Grid>
```

MainWindow.xaml.cs
------------------

When another client publishes a stream, the `Session.StreamReceived` event is sent, and the
`Session_StreamReceived` delegate method is called. It creates a new VideoRenderer for the
subscriber and adds that element to the `SubscriberGrid` (defined in the MainWindow.xaml file).
It then instantiates a Subscriber object and subscribes to the stream:

```csharp
private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
{
    Console.WriteLine("Session stream received");

    VideoRenderer renderer = new VideoRenderer();
    SubscriberGrid.Children.Add(renderer);
    UpdateGridSize(SubscriberGrid.Children.Count);
    Subscriber subscriber = new Subscriber(Context.Instance, e.Stream, renderer);
    SubscriberByStream.Add(e.Stream, subscriber);

    try
    {
        Session.Subscribe(subscriber);
    }
    catch (OpenTokException ex)
    {
        Console.WriteLine("OpenTokException " + ex.ToString());
    }
}
```

The `SubscriberByStream` object is a dictionary that maps Streams to Subscribers.

When a client stops publishing a stream, the `Session.StreamDropped` event is sent, and the
`Session_StreamDropped` delegate method is called. It looks up the Subscriber corresponding to
the Stream passed into the method. It then removes that Subscriber from the `SubscriberByStream`
dictionary, calls the `Session.Unsubscribe()` method and removes the subscriber's video renderer
from the `SubscriberGrid`:

```csharp
private void Session_StreamDropped(object sender, Session.StreamEventArgs e)
{
    Console.WriteLine("Session stream dropped");
    var subscriber = SubscriberByStream[e.Stream];
    if (subscriber != null)
    {
        SubscriberByStream.Remove(e.Stream);
        try
        {
            Session.Unsubscribe(subscriber);
        }
        catch (OpenTokException ex)
        {
            Console.WriteLine("OpenTokException " + ex.ToString());
        }

        SubscriberGrid.Children.Remove((UIElement)subscriber.VideoRenderer);
        UpdateGridSize(SubscriberGrid.Children.Count);
    }
}
```

The app clears the `SubscriberGrid` when it disconnects from the OpenTok session:

private void Session_Disconnected(object sender, EventArgs e)
{
    Console.WriteLine("Session disconnected");
    SubscriberByStream.Clear();
    SubscriberGrid.Children.Clear();
}
