using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using OpenTok;

namespace BasicVideoChatUWP
{
    /// <summary>
    /// Implementation of IDispatcher that uses UWP Main/UI thread to dispatch events.
    /// Available only for UWP applications.
    /// Delivery order is guaranteed.
    /// CPU-intensive or long-running tasks should not be executed in the handlers, as they would block the UI thread and affect performance or responsiveness.
    /// </summary>
    public class UWPDispatcher : IDispatcher
    {
        private readonly Control mainPage;
        private readonly CoreDispatcherPriority dispatchPriority;

        public UWPDispatcher(Control mainPage, CoreDispatcherPriority dispatchPriority = CoreDispatcherPriority.Normal)
        {
            this.mainPage = mainPage ?? throw new ArgumentNullException();
            this.dispatchPriority = dispatchPriority;
        }

        public async void DispatchEvent(object sender, EventHandler handler)
        {
            if (handler != null)
            {
                await mainPage.Dispatcher.RunAsync(dispatchPriority, new DispatchedHandler(() =>
                {
                    handler(sender, EventArgs.Empty);
                }));
            }
        }

        public async void DispatchEvent<T>(object sender, EventHandler<T> handler, T args) where T : EventArgs
        {
            if (handler != null)
            {
                await mainPage.Dispatcher.RunAsync(dispatchPriority, new DispatchedHandler(() =>
                {
                    handler(sender, args);
                }));
            }
        }
    }
}
