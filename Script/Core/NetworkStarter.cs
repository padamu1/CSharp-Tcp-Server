namespace CSharpTcpServer.Core
{
    public class NetworkStarter
    {
        public ConnectionManager ConnectionManager { get; set; }
        public NetworkStarter()
        {
            ConnectionManager = new ConnectionManager(3000);
        }
    }
}
