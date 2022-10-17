using CShapr_Tcp_Server.Core;
using CShapr_Tcp_Server.Core.ThreadSystem;

namespace CSharpTcpServer.Core
{
    public class NetworkStarter : ThreadBase
    {
        private ConnectionManager? m_connectionManager;
        public NetworkStarter()
        {
            // 스레드 매니저 생성
            ThreadManager.GetInstance();
            m_connectionManager = new ConnectionManager(3000);
            thread.Start();
        }
        protected override void ThreadAction()
        {
            base.ThreadAction();
            while(true)
            {

            }
        }
        public void ServerStop()
        {
            m_connectionManager?.ServerStop();
        }
        public void ServerStart()
        {
            m_connectionManager?.ServerStart();
        }

    }
}
