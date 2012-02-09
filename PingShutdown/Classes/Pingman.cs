using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace PingShutdown
{
    class Pingman
    {
        /// <summary>
        /// method to validate an IP address using regular expressions.
        /// The pattern being used will validate an ip address with the 
        /// range of 1.0.0.0 to 255.255.255.255
        /// </summary>
        /// <param name="addr">Address to validate</param>
        /// <returns></returns>
        public static bool IsValidIP (string addr)
        {
            //create our match pattern
//            string pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.
//    ([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
            string pattern2 = @"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$";

            //create our Regular Expression object
            Regex check = new Regex(pattern2);

            //boolean variable to hold the status
            bool valid = false;
            //check to make sure an ip address was provided
            if (addr == "")
            {
                //no address provided so return false
                valid = false;
            }
            else
            {
                //address provided so use the IsMatch Method
                //of the Regular Expression object
                valid = check.IsMatch(addr, 0);
            }
            //return the results
            return valid;
        }


        public static PingReply ping (String host, int timeout, int buffersize)
        {
            Ping pingSender = new Ping();
            int timeToLive = 128;
            PingOptions options = new PingOptions(timeToLive, false);

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = ""; //aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            for (int i = 0; i < buffersize; i++)
            {
                data += "a";
            }
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            PingReply reply = pingSender.Send(host, timeout, buffer, options);
            return reply;
        }


        [DllImport("iphlpapi.dll")]
        public static extern int SendARP (int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);

        /// <summary>
        /// Requests the MAC address using Address Resolution Protocol
        /// </summary>
        /// <param name="IP">The IP.</param>
        /// <returns>the MAC address</returns>
        public static byte[] RequestMACAddress (string IP)
        {
            IPAddress addr = IPAddress.Parse(IP);
            int IPint = BitConverter.ToInt32(addr.GetAddressBytes(), 0);
            byte[] mac = new byte[6];
            int length = mac.Length;
            SendARP(IPint, 0, mac, ref length);
            return mac;
        }

        public static string RequestMACAddressString (string IP)
        {
            byte[] mac = RequestMACAddress(IP);
            string macAddress = BitConverter.ToString(mac, 0, mac.Length);
            return macAddress;
        }


        public static byte[] GetMacArray (string mac)
        {
            if (string.IsNullOrEmpty(mac)) throw new ArgumentNullException("mac");
            byte[] ret = new byte[6];
            try
            {
                string[] tmp = mac.Split(':', '-');
                if (tmp.Length != 6)
                {
                    tmp = mac.Split('.');
                    if (tmp.Length == 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            ret[i * 2] = byte.Parse(tmp[i].Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                            ret[i * 2 + 1] = byte.Parse(tmp[i].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        }
                    }
                    else
                        for (int i = 0; i < 12; i += 2)
                            ret[i / 2] = byte.Parse(mac.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                }
                else
                    for (int i = 0; i < 6; i++)
                        ret[i] = byte.Parse(tmp[i], System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                throw new ArgumentException("Argument doesn't have the correct format: " + mac, "mac");
            }
            return ret;
        }


        /// <summary>
        /// Sends a Wake-On-Lan packet to the specified MAC address.
        /// </summary>
        /// <param name="mac">Physical MAC address to send WOL packet to.</param>
        public static void WakeOnLan (byte[] mac)
        {
            // WOL packet is sent over UDP 255.255.255.0:40000.
            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 40000);

            // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
            byte[] packet = new byte[17 * 6];

            // Trailer of 6 times 0xFF.
                for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            // Body of magic packet contains 16 times the MAC address.
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];

            // Send WOL packet.
            client.Send(packet, packet.Length);
        }
    }
}
