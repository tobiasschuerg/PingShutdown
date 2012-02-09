using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PingShutdown
{

    class Shutdown
    {
        /// <summary>
        /// Windows restart
        /// </summary>
        public static void Restart ()
        {
            StartShutDown("-f -r -t 5");
        }

        /// <summary>
        /// Log off.
        /// </summary>
        public static void LogOff ()
        {
            StartShutDown("-l");
        }

        /// <summary>
        ///  Shutting Down Windows 
        /// </summary>
        public static void Shut ()
        {
            StartShutDown("-f -s -t 5");
        }

        private static void StartShutDown (string param)
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.FileName = "cmd";
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Arguments = "/C shutdown " + param;
            Process.Start(proc);
        }

        /// <summary>
        ///  Shutting Down Windows 
        /// </summary>
        public static void shutdown (string parameter)
        {
            StartShutDown(parameter);
        }

    }
}
