using System;
using System.Collections.Generic;
using System.Text;

namespace PingShutdown
{
    class FormTools
    {
        public static string TimeSpanToString(TimeSpan span){
            int seconds = span.Seconds;
            int minutes = span.Minutes;
            int hours = span.Hours;
            int days = span.Days;
            string uptime = "";
            if (days > 0)
            {
                uptime += days + "d ";
            }
            if (hours > 0)
            {
                uptime += hours + "h ";
            }
            if (minutes > 0)
            {
                uptime += minutes + "m ";
            }
            if (seconds > 0)
            {
                uptime += seconds + "s ";
            }
            return uptime;
        }
    }
}
