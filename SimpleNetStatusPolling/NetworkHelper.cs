using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNetStatusPolling
{
    class NetworkHelper
    {
        public static bool IsOnline(string ipAddress, int port, int timeout)
        {
            TcpClient client = new TcpClient();
            var ar = client.BeginConnect(ipAddress, port, null, null);
            ar.AsyncWaitHandle.WaitOne(timeout);
            var result = client.Connected;
            client.Close();
            return result;
        }

        public static bool IsOnline(IPEndPoint ipe, int timeout)
        {
            return IsOnline(ipe.Address.ToString(), ipe.Port, timeout);
        }

    }
}
