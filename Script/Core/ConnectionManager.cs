using System.Net;
using System.Net.Sockets;

namespace CSharpTcpServer.Core
{
    public class ConnectionManager
    {
        private readonly TcpListener tcpListener;
        public Dictionary<TcpClient, WebSocketController> clients;
        public ConnectionManager(int port)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            clients = new Dictionary<TcpClient,WebSocketController>();
            tcpListener.Start();
            //비동기 Listening 시작
            tcpListener.BeginAcceptTcpClient(OnAcceptClient, null);
        }

        private void OnAcceptClient(IAsyncResult ar)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(ar);
            Console.WriteLine("Client Connected");
            WebSocketController webSocketController = new WebSocketController(client);
            tcpListener.BeginAcceptTcpClient(OnAcceptClient, null);
        }
    }
}
