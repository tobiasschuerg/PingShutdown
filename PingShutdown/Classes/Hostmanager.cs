using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PingShutdown
{
    class Hostmanager
    {
        public int pingtimeout{
            set;
            get;
        }
        public int pingBufferSize
        {
            set;
            get;
        }
        private Host[] hosts = new Host[32];
        public int hostcount = 0;
        private int hostID = 0;

        //todo: Threading
        public Thread refreshWorker = null; //change name
        public int sleeptime
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="buffersize"></param>
        public Hostmanager (int timeout, int buffersize)
        {
            this.pingtimeout = timeout;
            this.pingBufferSize = buffersize;
            // Thread fürs refreshen erstellen
            this.sleeptime = 5000;
            refreshWorker = new Thread(this.refreshThread);
            refreshWorker.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public bool addHost (Host host)
        {
            if (host == null)
            {
                throw new Exception("empty Host submitted");
            }

            bool success = true;
            foreach (Host h in hosts)   //check if host is already in list
            {
                if (h != null && h.hostIP == host.hostIP)
                {
                    success = false;
                }
            }
            if (success)
            {
                success = false;
                for (int i = 0; i < hosts.Length; i++)   //find a free host entry
                {
                    if (hosts[i] == null)
                    {
                        host.id = hostID;
                        hosts[i] = host;
                        success = true;
                        hostID++;
                        hostcount++;
                        break;
                    }
                    else if (i == hosts.Length)
                    {
                        throw new Exception("Limit reached");
                    }
                }
            }
            return success;
        }

        public void removeHost (int id)
        {
            for (int i = 0; i < hosts.Length; i++)
            {
                if (hosts[i] != null && hosts[i].id == id)
                {
                    hosts[i] = null;
                }
            }
            hostcount--;
        }


        public void refreshHosts ()
        {
            foreach (Host h in hosts)
            {
                if (h != null)
                {
                    h.ping(pingtimeout, pingBufferSize);
                }
            }
        }

        internal Host[] getHosts ()
        {
            Host[] hostarray = new Host[hostcount];
            int index = 0;
            foreach (Host host in hosts)
            {
                if (host == null)
                {
                    // unused Host
                }
                else
                {
                    hostarray[index] = host;
                    index++;
                }
            }
            return hostarray;
        }

        internal Host getHost (int id)
        {
            Host h = null;
            foreach (Host host in hosts)
            {
                if (host != null && host.id == id)
                {
                    h = host;
                    break;
                }
            }
        return h;
        }

        private void refreshThread() {
            while (true)
            {
                refreshHosts();
                Thread.Sleep(sleeptime);
            }
        }      
    }
}
