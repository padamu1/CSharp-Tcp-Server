using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CShapr_Tcp_Server.Base
{
    public class Singleton<T>
    {
        private static readonly Lazy<T> instanceHolder = new Lazy<T>();
        public static T GetInstance()
        {
            return instanceHolder.Value;
        }
    }
}
