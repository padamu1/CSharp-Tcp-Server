using CShapr_Tcp_Server.Core.ThreadSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CShapr_Tcp_Server.Core
{
    public class Worker : ThreadCore
    {
        private int workCount;
        public Worker(int delayTime):base(delayTime)
        {
            workCount = 0;
        }
        protected override void ThreadAction()
        {
            while (!isThreadStop)
            {
                Work();
                Console.Write(thread.Name);
                Thread.Sleep(delayTime);
            }
        }
        public void Work()
        {
            Console.WriteLine("Working..." + workCount++);
        }

    }
    public class ThreadManager
    {
        static readonly Lazy<ThreadManager> instanceHolder = new Lazy<ThreadManager>(() => new ThreadManager());
        public static ThreadManager GetInstance()
        {
            return instanceHolder.Value;
        }
        /// <summary>
        /// 스레드를 만드는 함수
        /// </summary>
        /// <param name="delayTime">딜레이 타임</param>
        public void MakeThread(int delayTime)
        {
            Worker worker = new Worker(delayTime);
            worker.ThreadStart();
            Task.Delay(5000).ContinueWith(t => { 
                RemoveWorker(worker);
                t.Dispose();
            });
        }
        /// <summary>
        /// 스레드를 삭제하는 함수
        /// </summary>
        /// <param name="thread"></param>
        public void RemoveWorker(Worker worker)
        {
            Console.WriteLine("Request Stop Work");
            worker.Dispose();
        }
    }
}
