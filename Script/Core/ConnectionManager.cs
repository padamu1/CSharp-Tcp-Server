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
            clients = new Dictionary<TcpClient, WebSocketController>();
        }

        private void OnAcceptClient(IAsyncResult ar)
        {
            try
            {
                TcpClient client = tcpListener.EndAcceptTcpClient(ar); 
                Console.WriteLine("Client Connected");
                Client webSocketController = new Client(client);
                tcpListener.BeginAcceptTcpClient(OnAcceptClient, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void ServerStart()
        {
            tcpListener.Start();
            //비동기 Listening 시작
            tcpListener.BeginAcceptTcpClient(OnAcceptClient, null);
        }
        public void ServerStop()
        {
            try
            {
                tcpListener.Server.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                tcpListener.Server.Close();
                tcpListener.Stop();
            }
        }
    }
}
