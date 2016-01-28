**TESTING THE SAMPLE APP:**
* Import the project in Visual Studio by clicking on ``BasicSubscriber.sln`` file.
* Include the files opentok_dyn.dll, pthreadVC2.dll in ``opentok-windows-sdk-samples\c#\TestCSDKFromCSharp\bin\Debug`` folder.

**UNDERSTANDING THE CODE:**

BasicSubscriber is a basic example of how to use Opentok on C#.

1) Initializing an Opentok session and connecting to session:

``var callbacks = new Opentok.Session.otc_session_cb();
var session = Opentok.Session.otc_session_new(API_KEY, SESSION_ID, ref callbacks);
Opentok.Session.otc_session_connect(session, TOKEN);``

The Session constructor instantiates a new Session object.
* The first parameter is your OpenTok API key see the [Developer Dashboard](https://dashboard.tokbox.com/projects).
* The second parameter is the session ID for the OpenTok session your app connects to. You can generate a session ID from the
  [Developer Dashboard](https://dashboard.tokbox.com/projects) or from a [server-side library](http://www.tokbox.com/opentok/docs/concepts/server_side_libraries.html).
* The third parameter is session callback handler.                                                 
   ``var callbacks = new Opentok.Session.otc_session_cb();``

The otc_session_connect() method connects to the OpenTok session.
`` Opentok.Session.otc_session_connect(session, TOKEN);``
* The first parameter is the session object.
* The second parameter is the token.
* The TOKEN constant is the token string for the client connecting to the session. See [Token Creation Overview](http://tokbox.com/opentok/tutorials/create-token/) for details. You can generate
  a token from the [Developer Dashboard](https://dashboard.tokbox.com/projects).    

When the app connects to the OpenTok session, on_connected method is called back.
``callbacks.on_connected = (IntPtr session_cb, IntPtr userData) =>   {                
         waitHandle.Set();                                                                                                              
   };``

An app must create a Session object and connect to the session it before the app can publish or subscribe to streams in the session.
When another client's stream is added to a session, the on_stream_received() method of the Session object is called:
``callbacks.on_stream_received = (IntPtr session_cb, IntPtr user_data, IntPtr stream_cb) => {   
       subscriber = Opentok.Subscriber.otc_subscriber_new(stream_cb, ref subscriber_callbacks);                                                                                          }``

**Knowing when you have disconnected from the session:**
When the app disconnects from the session, the on_disconnected method of the Session interface is called:
``callbacks.on_disconnected = (IntPtr session_cb, IntPtr userData) =>   {                
         waitHandle.Set();   
         };``

To subscribe to the stream call the function otc_session_subscribe of Session interface:
``Opentok.Session.otc_session_subscribe(session, subscriber);``
* The first parameter is the session object.
* The second parameter is the subscriber object.

To disconnect from session call the function otc_session_disconnect of Session interface:
``Opentok.Session.otc_session_disconnect(session);``

**Publishing an Audio stream to session:**
App publishes to stream using Publisher constructor.
``publisher = Opentok.Publisher.otc_publisher_new(".NET 4.5", IntPtr.Zero, ref publisher_callbacks);``
* The second parameter indicates that we are publishing both audio and video.
* The third parameter is publisher callback handler.

Now we publish to the session using otc_session_publish() to the OpenTok session :
``Opentok.Session.otc_session_publish(session, publisher);``
* The first parameter we are passing session object.
* The second parameter we are passing publisher object.

When publisher stream is created and starts streaming on_stream_created() method:
``publisher_callbacks.on_stream_created = (IntPtr publisher_cb, IntPtr userData, IntPtr stream) =>
        {
                waitHandle.Set();
         };``

**Subscribing to an Audio stream session:**
Method on_stream_received() of Session interface initializes subscriber object for stream.
``subscriber = Opentok.Subscriber.otc_subscriber_new(stream_cb, ref subscriber_callbacks);``
* The first parameter is the stream that is being subscribed to.
* The second parameter is subscriber callback handler.

When another client's stream frame is received, the on_render_frame() method of the Subscriber is called:
 ``subscriber_callbacks.on_render_frame = (IntPtr subscriber_cb, IntPtr user_data, IntPtr frame) =>
 {
    frames_received++;
    if (frames_received == 14) waitHandle.Set();
 };``
