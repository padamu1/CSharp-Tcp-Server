using CShapr_Tcp_Server.Core.ThreadSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CShapr_Tcp_Server.Core
{
    /// <summary>
    /// 스레드 테스트용 클래스
    /// </summary>
    public class Worker : ThreadBase
    {
        private int workCount=0;
        public Worker():base()
        {
            // 생성자
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
        public override ThreadBase Clone()
        {
            Worker worker = new Worker();
            worker.workCount = workCount;
            worker.delayTime = delayTime;
            return worker;
        }
        public override string GetThreadName()
        {
            return "Worker";
        }

    }
    public class ThreadManager
    {
        static readonly Lazy<ThreadManager> instanceHolder = new Lazy<ThreadManager>(() => new ThreadManager());
        public static ThreadManager GetInstance()
        {
            return instanceHolder.Value;
        }

        private Dictionary<string, ThreadBase> threadPoolDic; // 다양한 스레드를 담기 위한 풀 딕셔너리
        public ThreadManager()
        {
            threadPoolDic = new Dictionary<string, ThreadBase>();
        }
        /// <summary>
        /// 사용할 스레드를 풀에 등록
        /// </summary>
        /// <param name="threadName"></param>
        /// <param name="thread"></param>
        public void RegistThread(string threadName,ThreadBase thread)
        {
            if(!threadPoolDic.ContainsKey(threadName))
            {
                threadPoolDic.Add(threadName, thread);
                Console.WriteLine(threadName + " registered successfully");
            }
            else
            {
                Console.WriteLine(threadName + " already registered");
            }
        }
        /// <summary>
        /// 스레드 풀안에 있는 객체를 가져와 딜레이 타임을 적용해 동작하는 스레드를 만드는 함수
        /// </summary>
        /// <param name="threadName"></param>
        /// <param name="delayTime"></param>
        public void MakeThread(string threadName,int delayTime)
        {
            if(threadPoolDic.ContainsKey(threadName))
            {
                ThreadBase tb = threadPoolDic[threadName].Clone();
                tb.ThreadStart(delayTime);
                Console.WriteLine(threadName + " thread start");
            }
        }
        /// <summary>
        /// 등록된 스레드를 삭제하는 함수
        /// </summary>
        /// <param name="thread"></param>
        public void RemoveWorker(ThreadBase tb)
        {
            Console.WriteLine(tb.GetThreadName() + " thread will stop");
            tb.Dispose();
        }
    }
}
