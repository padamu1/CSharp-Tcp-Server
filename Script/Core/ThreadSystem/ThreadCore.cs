using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CShapr_Tcp_Server.Core.ThreadSystem
{
    public class ThreadCore : IDisposable
    {
        protected int delayTime;
        protected bool isThreadStop;
        protected Thread thread;

        /// <summary>
        /// 딜레이 타임을 넣고 스레드 생성
        /// </summary>
        /// <param name="delayTime">딜레이 타임</param>
        public ThreadCore(int delayTime)
        {
            this.delayTime = delayTime;
            isThreadStop = false;
            thread = new Thread(ThreadAction);
        }

        /// <summary>
        /// 스레드 시작
        /// </summary>
        public void ThreadStart()
        {
            thread.Start();
        }
        /// <summary>
        /// 스레드 실제 동작 부분
        /// </summary>
        protected virtual void ThreadAction()
        {
        }
        /// <summary>
        /// 스레드 삭제
        /// </summary>
        public void Dispose()
        {
            isThreadStop = true;
            Console.WriteLine("ThreadStop");
            GC.SuppressFinalize(this);
            Console.WriteLine("Thread Kill");
        }
    }
}
