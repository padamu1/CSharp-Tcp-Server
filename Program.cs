using CSharpTcpServer.Core;
using System.Collections;
using System.Net;
using System.Net.Sockets;
public class Program
{
    public static void Main()
    {
        NetworkStarter networkStarter = new NetworkStarter();
        bool isServerStop = false;
        while (!isServerStop)
        {
            Console.Write("Server start\n");
            Console.WriteLine("서버를 종료하시려면 \"quit\"를 입력해주세요.");
            switch (Console.ReadLine())
            {
                case "quit":
                    isServerStop = true;
                    break;
                default:
                    break;
            }
        }
    }
}