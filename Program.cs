using CShapr_Tcp_Server.Core.ThreadSystem;
using CSharpTcpServer.Core;
using System.Collections;
using System.Net;
using System.Net.Sockets;
public class ServerStateManager : ThreadBase
{
    private NetworkStarter _networkStarter;
    public ServerStateManager(NetworkStarter networkStarter)
    {
        thread.Start();
        this._networkStarter = networkStarter;
    }
    protected override void ThreadAction()
    {
        base.ThreadAction();
        while (true)
        {
            Console.Write("Server start\n");
            Console.WriteLine("서버를 종료하시려면 \"Stop\"를 입력해주세요.");
            switch (Console.ReadLine())
            {
                case "Stop":
                    _networkStarter.ServerStop();
                    break;
                case "Start":
                    _networkStarter.ServerStart();
                    break;
                default:
                    break;
            }
        }
    }
}
public class Program
{
    private static NetworkStarter? _networkStarter;
    public static void Main()
    {
        _networkStarter = new NetworkStarter();
        ServerStateManager serverStateManager = new ServerStateManager(_networkStarter);
        while (true)
        {

        }
    }
}