using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Opentok;
using System.Threading;

namespace TestCSDKFromCSharp
{
    class Program
    {
        public const string API_KEY = "";
        public const string SESSION_ID = "";
        public const string TOKEN = "";

        public static AutoResetEvent waitHandle = new AutoResetEvent(false);
        public static int frames_received = 0;

        public static IntPtr subscriber = IntPtr.Zero;
        public static IntPtr session = IntPtr.Zero;
        public static IntPtr publisher = IntPtr.Zero;

        static void Main(string[] args)
        {

            Base.otc_init(IntPtr.Zero);
            //Base.otc_log_enable(1);

            var callbacks = new Opentok.Session.otc_session_cb();
            var subscriber_callbacks = new Opentok.Subscriber.otc_subscriber_cb();
            var publisher_callbacks = new Opentok.Publisher.otc_publisher_cb();

            callbacks.on_connected = (IntPtr session_cb, IntPtr userData) => {
                waitHandle.Set();
            };

            callbacks.on_disconnected = (IntPtr session_cb, IntPtr userData) => {
                waitHandle.Set();
            };

            callbacks.on_stream_received = (IntPtr session_cb, IntPtr user_data, IntPtr stream_cb) =>
            {
                subscriber = Opentok.Subscriber.otc_subscriber_new(stream_cb, ref subscriber_callbacks);

                // subscribe only to audio
                // Opentok.Subscriber.otc_subscriber_set_subscribe_to_video(subscriber, false);

                waitHandle.Set();
            };

            subscriber_callbacks.on_render_frame = (IntPtr subscriber_cb, IntPtr user_data, IntPtr frame) =>
            {
                frames_received++;
                if (frames_received == 14) waitHandle.Set();

            };

            publisher_callbacks.on_stream_created = (IntPtr publisher_cb, IntPtr userData, IntPtr stream) =>
            {
                waitHandle.Set();
            };

            // Connect to the session
            var session = Opentok.Session.otc_session_new(API_KEY, SESSION_ID, ref callbacks);
            Console.WriteLine("Connecting session");
            Opentok.Session.otc_session_connect(session, TOKEN);
            waitHandle.WaitOne();

            // Once we are connected, create a publisher and publish in the session
            // Use of IntPtr.Zero (NULL) as the second parameter means that we are going to
            // use the default video/audio capturer from webrtc
            publisher = Opentok.Publisher.otc_publisher_new(".NET 4.5", IntPtr.Zero, ref publisher_callbacks);

            // to send audio only
            // Opentok.Publisher.otc_publisher_set_publish_video(publisher, false);

            //Opentok.Session.otc_session_publish(session, publisher);
            //waitHandle.WaitOne();

            // Once we are publshing, we will wait until other stream is connected to the session
            Console.WriteLine("Connected, Waiting for stream");
            waitHandle.WaitOne();

            // When we have the stream, wait until the stream is connected
            // We will subscribe to this stream in the on_stream_received callback
            Console.WriteLine("Stream Received");
            Opentok.Session.otc_session_subscribe(session, subscriber);

            // Let's wait until 15 frames are received and we will quit the app
            Console.WriteLine("Wating for 15 frames");
            waitHandle.WaitOne();

            Opentok.Session.otc_session_disconnect(session);
            waitHandle.WaitOne();

            Opentok.Session.otc_session_delete(session);

            Opentok.Base.otc_destroy();

            Console.WriteLine("Success!!");
            Console.ReadKey();
        }
    }
}
