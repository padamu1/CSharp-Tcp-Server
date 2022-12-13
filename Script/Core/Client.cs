using CShapr_Tcp_Server.Manager;
using CSharpTcpServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTcpServer.Core
{
    public class Client : WebSocketController
    {
        private string ip;
        public Client(TcpClient tcpClient) : base(tcpClient)
        {
            Socket c = tcpClient.Client;
            IPEndPoint ip_point = (IPEndPoint)c.RemoteEndPoint;
            string ip = ip_point.Address.ToString();
            ClientManager.GetInstance().Add(this);
        }
        public string GetIp()
        {
            return ip;
        }
        public override void Dispose()
        {
            base.Dispose();
            ClientManager.GetInstance().Remove(this);
        }
    }
}
