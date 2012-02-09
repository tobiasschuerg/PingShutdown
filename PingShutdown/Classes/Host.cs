using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Net.Sockets;

namespace PingShutdown
{
    class Host
    {
        public int id;
        public string hostIP = "";
        public string hostname = "";
        public long pingtime { get; set; }
        private DateTime lastTimeOnline = DateTime.Now;
        public TimeSpan offline;
        public bool online {get; set;}

        public Host (string host, int id)
        {
            online = false;
            if (host == "")
            {
                throw new Exception("no empty host!");
            } else if (Pingman.IsValidIP(host)) {
                this.hostIP = host;
            } else { 
                this.hostname = host;
            }
            this.id = id;
        }


        internal bool isSet ()
        {
            if (hostIP != null || hostname != null)
            {
                return true;
            }else {
                return false;
            }
        }

        public void validateHost ()
        {
            if (this.hostname == "")
            {
                this.hostname = "[identifying host]";
            }
            Thread ghn = new Thread(new ThreadStart(identifyHost));
            ghn.Start();
        }

        private void identifyHost ()
        {
            try
            {
                IPHostEntry hostinfo;
                hostinfo = Dns.GetHostEntry(this.getWhatIsSet());
                if (this.hostIP == "")
                {
                    foreach (IPAddress curAdd in hostinfo.AddressList) {
                        if (Pingman.IsValidIP(curAdd.ToString()))
                        {
                            this.hostIP = curAdd.ToString();
                        }
                    }
                } else {
                    this.hostname = hostinfo.HostName;
                }
                
                // this.hostIP = "";
                //foreach (IPAddress curAdd in hostinfo.AddressList) {
                //    this.hostIP += curAdd.ToString() + "/n";
                //}
                //MessageBox.Show(hostinfo.AddressList.Length + " <-länge | IP -> /n" + this.hostIP);
            }
            catch (SocketException e)
            {
                string error = "";
                error += "SocketException caught!!! /n";
                error +="Source : " + e.Source + "/n";
                error +="Message : " + e.Message;
                MessageBox.Show(error);
            }
            catch (ArgumentNullException e)
            {
                string error = "";
                error += "ArgumentNullException caught!!!";
                error += "Source : " + e.Source + "/n";
                error += "Message : " + e.Message;
                MessageBox.Show(error);
            }
            catch (Exception e)
            {
                string error = "";
                error += "Exception caught!!!";
                error += "Source : " + e.Source + "/n";
                error += "Message : " + e.Message;
                MessageBox.Show(error);
            }
        }


        public string getWhatIsSet ()
        {
            if (hostIP != "")
            {
                return hostIP;
            }
            else if (hostname != "")
            {
                return hostname;
            }
            else
            {
                //this section should not be reached
                throw new Exception("neither ip nor host is set!");
            }
        }

        public void ping (int timeout, int buffersize)
        {
            PingReply pr = null;    
            try                       
            {
                pr = Pingman.ping(this.getWhatIsSet(), timeout, buffersize);
            }
            catch (PingException)
            {
                pr = null;
            }

             if (pr != null)
             {
                 if (pr.Status == IPStatus.Success)
                 {
                     online = true;
                     lastTimeOnline = DateTime.Now;
                     if (this.hostname == "" || this.hostIP == "")
                     {
                         this.validateHost();
                     }
                     this.pingtime = pr.RoundtripTime;  
                 }
                 else if (pr.Status == IPStatus.TimedOut 
                     || pr.Status == IPStatus.TtlExpired
                     || pr.Status == IPStatus.DestinationHostUnreachable)
                 {
                     online = false;
                     offline = DateTime.Now - this.lastTimeOnline;
                 } else {
                     online = false;
                     // throw new Exception("Pingstatus: \n" + pr.Status + "\n :-(");
                     // todo loggen ??
                 }
             }
        }

        public String toString ()
        {
            return hostname + " (" + hostIP + ")";
        }
    }
}
