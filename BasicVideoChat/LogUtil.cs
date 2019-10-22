using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BasicVideoChat
{
    class LogUtil
    {
        private static readonly Lazy<LogUtil> lazy = new Lazy<LogUtil>
            (() => new LogUtil());

        public static LogUtil Instance { get { return lazy.Value; } }

        public void EnableLogging()
        {
            otc_logger_func X = (string message) =>
            {
                Console.WriteLine(message);
            };
            otc_log_enable(0x7FFFFFFF);
            otc_log_set_logger_func(X);
        }

        // Static interfaces
        [DllImport("opentok", EntryPoint = "otc_log_set_logger_func", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void otc_log_set_logger_func(otc_logger_func logger);

        [DllImport("opentok", EntryPoint = "otc_log_enable", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void otc_log_enable(int level);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void otc_logger_func(string message);
    }
}
