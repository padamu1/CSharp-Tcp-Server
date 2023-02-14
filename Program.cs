using CShapr_Tcp_Server.Core.ThreadSystem;
using CShapr_Tcp_Server.Manager;
using CSharpTcpServer.Core;
using System.Collections;
using System.Net;
using System.Net.Sockets;
public class ApplicationManager 
{
    public static void Manage()
    {
        Console.WriteLine("When you wan server Stop - input : \"Stop\", Start - input : \"Start\", End - input : \"Quit\"");
        switch (Console.ReadLine())
        {
            case "Stop":
                NetworkStarter.GetInstance().ServerStop();
                Manage();
                break;
            case "Start":
                NetworkStarter.GetInstance().ServerStart();
                Manage();
                break;
            case "Quit":
                return;
            default:
                break;
        }
    }
}
public class Program : ApplicationManager
{
    private static NetworkStarter? _networkStarter;
    public static void Main()
    {
        ClientManager.GetInstance();
        Manage();
    }
}