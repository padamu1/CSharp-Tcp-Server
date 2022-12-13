using CShapr_Tcp_Server.Base;
using CSharpTcpServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CShapr_Tcp_Server.Manager
{
    public class ClientManager : Singleton<ClientManager>
    {
        private Dictionary<string, Client> clients = new Dictionary<string, Client>();
        public void Add(Client client)
        {
            if(clients.ContainsKey(client.GetIp()))
            {
                clients[client.GetIp()].Dispose();
            }
            clients.Add(client.GetIp(), client);
        }
        public void Remove(Client client)
        {
            if (clients.ContainsKey(client.GetIp()))
            {
                clients.Remove(client.GetIp());
            }
        }
    }
}
