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
    }
}
