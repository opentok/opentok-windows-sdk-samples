using OpenTok;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace ClientObservability
{
    public partial class MainWindow : Window
    {
        private const string API_KEY = "47797141";
        private const string SESSION_ID = "2_MX40Nzc5NzE0MX5-MTc1ODcyMTU3NTEyN35MdDhBSXBseUdRSEh6a3B3TGdNRVRySGV-fn4";
        private const string TOKEN = "T1==cGFydG5lcl9pZD00Nzc5NzE0MSZzZGtfdmVyc2lvbj0mc2lnPTZkNjM0ZDE2NDg4MzM4MGMyMWUxZWU0ODk2MWMzZmU3ZTRiNzg0NDQ6c2Vzc2lvbl9pZD0yX01YNDBOemM1TnpFME1YNS1NVGMxT0RjeU1UVTNOVEV5TjM1TWREaEJTWEJzZVVkUlNFaDZhM0IzVEdkTlJWUnlTR1YtZm40JmNyZWF0ZV90aW1lPTE3NTg3MjE1NzQmZXhwaXJlX3RpbWU9MTc2MTMxMzU3NCZyb2xlPW1vZGVyYXRvciZub25jZT1kMTU5M2UxNC04ZWU4LTQ3NmMtOTg1NC1iMjJlMjBmODVjZWE=";

        private Context context;
        private Session session;
        private Publisher publisher;
        private Subscriber subscriber;
        private Task rtcStatsThread;
        private bool rtcStatsThreadIsRunning = true;

        public MainWindow()
        {
            InitializeComponent();

            context = new Context(new WPFDispatcher());

            // Uncomment following line to get debug logging
            // Logger.Enable();

            IList<VideoCapturer.VideoDevice> capturerDevices = VideoCapturer.EnumerateDevices();
            if (capturerDevices == null || capturerDevices.Count == 0)
                throw new Exception("No video capture devices detected");

            publisher = new Publisher.Builder(context)
            {
                Capturer = capturerDevices[0].CreateVideoCapturer(VideoCapturer.Resolution.High, VideoCapturer.FrameRate.Fps30),
                Renderer = PublisherVideo
            }.Build();
            publisher.AudioStatsUpdated += PublisherAudioStatsUpdated;
            publisher.VideoStatsUpdated += PublisherVideoStatsUpdated;
            publisher.RtcStatsReport += PublisherRtcStatsReport;

            session = new Session.Builder(context, API_KEY, SESSION_ID).Build();
            session.Connected += Session_Connected;
            session.Disconnected += Session_Disconnected;
            session.Error += Session_Error;
            session.StreamReceived += Session_StreamReceived;
            session.Connect(TOKEN);

            /* 
             * Both audio and video stats callbacks are invoked automatically once subscribed,
             * but due to the weight of RTC stats, the callback will not be invoked unless specifically requested.
             * For that purpose we can use a dedicated thread that will request RTC stats every second.
             */
            rtcStatsThread = Task.Run(async () =>
            {
                while (rtcStatsThreadIsRunning)
                {
                    await Task.Delay(1000);
                    publisher?.GetRtcStatsReport();
                    subscriber?.GetRtcStatsReport();
                }                
            });

            Unloaded += MainWindowUnloaded;
        }

        private void MainWindowUnloaded(object sender, RoutedEventArgs e)
        {
            rtcStatsThreadIsRunning = false;
        }

        private void Session_Connected(object sender, System.EventArgs e)
        {
            session.Publish(publisher);
        }

        private void Session_Disconnected(object sender, System.EventArgs e)
        {
            Trace.WriteLine("Session disconnected.");
        }

        private void Session_Error(object sender, Session.ErrorEventArgs e)
        {
            Trace.WriteLine("Session error:" + e.ErrorCode);
        }

        private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
        {
            subscriber = new Subscriber.Builder(context, e.Stream)
            {
                Renderer = SubscriberVideo
            }.Build();
            subscriber.AudioStatsUpdated += SubscriberAudioStatsUpdated;
            subscriber.VideoStatsUpdated += SubscriberVideoStatsUpdated;
            subscriber.RtcStatsReport += SubscriberRtcStatsReport;
            session.Subscribe(subscriber);
        }

        private void PublisherAudioStatsUpdated(object sender, Publisher.AudioNetworkStatsEventArgs e)
        {
            if (e.Stats.Length == 0)
                return;
            Trace.WriteLine("Publisher audio stats: BytesSent=" + e.Stats[0].BytesSent + ", PacketsLost=" + e.Stats[0].PacketsLost);
        }

        private void PublisherVideoStatsUpdated(object sender, Publisher.VideoNetworkStatsEventArgs e)
        {
            if (e.Stats.Length == 0)
                return;
            Trace.WriteLine("Publisher video stats: BytesSent=" + e.Stats[0].BytesSent + ", PacketsLost=" + e.Stats[0].PacketsLost);
        }

        private void PublisherRtcStatsReport(object sender, Publisher.RtcStatsReportArgs e)
        {
            if (e.stats.Length == 0)
                return;
            Trace.WriteLine("Publisher audio stats: Full JSON stats:\n" + e.stats[0].JsonArrayOfReports);
        }

        private void SubscriberAudioStatsUpdated(object sender, Subscriber.AudioNetworkStatsEventArgs e)
        {
            Trace.WriteLine("Subscriber audio stats: BytesReceived=" + e.BytesReceived + ", PacketsLost=" + e.PacketsLost);
            if (e.SenderStats != null)
                Trace.WriteLine("Sender side audio stats: MaxBitrate=" + e.SenderStats.MaxBitrate + ", CurrentBitrate=" + e.SenderStats.CurrentBitrate);
        }

        private void SubscriberVideoStatsUpdated(object sender, Subscriber.VideoNetworkStatsEventArgs e)
        {
            Trace.WriteLine("Subscriber video stats: BytesReceived=" + e.BytesReceived + ", PacketsLost=" + e.PacketsLost);
            if (e.SenderStats != null)
                Trace.WriteLine("Sender side video stats: MaxBitrate=" + e.SenderStats.MaxBitrate + ", CurrentBitrate=" + e.SenderStats.CurrentBitrate);
        }

        private void SubscriberRtcStatsReport(object sender, Subscriber.RtcStatsReportArgs e)
        {
            Trace.WriteLine("Publisher audio stats: Full JSON stats:\n" + e.JsonArrayOfReports);
        }
    }
}
