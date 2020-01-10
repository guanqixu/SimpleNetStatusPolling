using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleNetStatusPolling
{
    class NetworkHelper
    {


        public static bool IsOnline(string ipAddress, int port, int timeout)
        {
            TcpClient client = new TcpClient();
            var ipe = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            var task = client.ConnectAsync(IPAddress.Parse(ipAddress), port);
            task.Wait(timeout);

            var result = false;
            result = client.Connected;
            client.Close();

            return result;
        }

        public static bool IsOnline(IPEndPoint ipe, int timeout)
        {
            TcpClient client = new TcpClient();
            var task = client.ConnectAsync(ipe.Address, ipe.Port);
            task.Wait(timeout);

            var result = false;
            result = client.Connected;
            client.Close();

            return result;
        }

    }
}
