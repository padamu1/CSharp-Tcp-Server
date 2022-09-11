using CShapr_Tcp_Server.Core;

namespace CSharpTcpServer.Core
{
    public class NetworkStarter
    {
        public ConnectionManager ConnectionManager { get; set; }
        public NetworkStarter()
        {
            // 스레드 매니저 생성
            ThreadManager.GetInstance();
            Thread ee = new Thread(MakeNewThreadTest);
            ee.Start();

            ConnectionManager = new ConnectionManager(3000);
        }
        public void MakeNewThreadTest()
        {
            while(true)
            {
                ThreadManager.GetInstance().MakeThread(1000);
                Thread.Sleep(3000);
            }
        }

    }
}
