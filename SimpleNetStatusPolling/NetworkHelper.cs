using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNetStatusPolling
{
    /// <summary>
    /// 网络助手
    /// </summary>
    class NetworkHelper
    {
        /// <summary>
        /// 指定ip及端口是否在线
        /// </summary>
        /// <param name="ipAddress">网络地址</param>
        /// <param name="port">网络端口</param>
        /// <param name="timeout">尝试网络连接的超时时间</param>
        /// <returns></returns>
        public static bool IsOnline(string ipAddress, int port, int timeout)
        {
            TcpClient client = new TcpClient();
            var ar = client.BeginConnect(ipAddress, port, null, null);
            ar.AsyncWaitHandle.WaitOne(timeout);
            var result = client.Connected;
            client.Close();
            return result;
        }

        /// <summary>
        /// 指定网络终结点是否在线
        /// </summary>
        /// <param name="ipe">网络终结点</param>
        /// <param name="timeout">尝试网络连接的超时时间</param>
        /// <returns></returns>
        public static bool IsOnline(IPEndPoint ipe, int timeout)
        {
            return IsOnline(ipe.Address.ToString(), ipe.Port, timeout);
        }

    }
}
