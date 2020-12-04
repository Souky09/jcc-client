using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;

namespace SharedClasses.Persistence
{
    public static class PingHelper
    {
        public static PingReply Ping(string ipAddress)
        {
            IPAddress ip;
            IPAddress.TryParse(ipAddress, out ip);
            //var success = "Error";
            Ping pinger = null;
            PingReply reply = null;
            if (ip != null)
            {
                try
                {
                    pinger = new Ping();
                    reply = pinger.Send(ip);
                    //if (reply.Status == IPStatus.Success)
                    //    success = "success";
                }
                catch (PingException)
                {
                    // Discard PingExceptions and return false;
                }

            }
            
            return  reply;
        }


    }
}
