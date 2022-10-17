using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CShapr_Tcp_Server.Core.ThreadSystem
{
    public class ThreadBase : IDisposable
    {
        protected int delayTime;
        protected bool isThreadStop;
        protected Thread thread;

        /// <summary>
        /// 스레드 생성자
        /// </summary>
        /// <param name="delayTime">딜레이 타임</param>
        public ThreadBase()
        {
            isThreadStop = false;
            thread = new Thread(ThreadAction);
            thread.IsBackground = true;
        }
        /// <summary>
        /// 스레드 시작
        /// </summary>
        public void ThreadStart(int delayTime)
        {
            this.delayTime = delayTime;
            thread.Start();
        }
        /// <summary>
        /// 스레드 실제 동작 부분
        /// </summary>
        protected virtual void ThreadAction()
        {
            // 상속받은 클래스에서 재정의 필요
        }
        /// <summary>
        /// 스레드 삭제
        /// </summary>
        public void Dispose()
        {
            isThreadStop = true;
            Console.WriteLine(GetThreadName() + " thread stopped");
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 스레드 복사에 사용
        /// </summary>
        /// <returns>상속받은 객체</returns>
        public virtual ThreadBase Clone()
        {
            // 상속받은 클래스에서 재정의 필요
            return this;
        }
        /// <summary>
        /// 스레드 이름 정의 - 클래스와 같게 설정
        /// </summary>
        /// <returns>스레드 이름</returns>
        public virtual string GetThreadName()
        {
            return "ThreadBase";
        }
    }
}
