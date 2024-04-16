using System;
using System.Net;
using System.Net.Sockets;

namespace Apotea.Design.ActorModel.Services.ServicesDefault
{
    internal class NetworkScanner
    {
        public static int GetPort()
        {
            var port = 0;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                var localEp = new IPEndPoint(IPAddress.Any, 0);
                socket.Bind(localEp);
                localEp = (IPEndPoint)socket.LocalEndPoint!;
                port = localEp.Port;
            }
            finally
            {
                socket.Close();
            }
            return port;
        }

        public static bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    client.EndConnect(result);
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
