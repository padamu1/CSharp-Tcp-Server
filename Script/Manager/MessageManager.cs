using CSharpTcpServer.Core;
using CSharpTcpServer.Core.Util;

namespace CSharpTcpServer.Manager
{
    public class MessageManager
    {
        public static void ProcessData(WebSocketController wsController, EventData dataFormat)                      // 클라이언트로부터 들어온 정보를 처리하기 위한 함수
        {
            byte eventCode = dataFormat.eventCode;
            Dictionary<byte, object> param = new Dictionary<byte, object>();

            // 데이터가 널이 아니라면 넣어줌
            if (dataFormat.data != null)
            {
                param = dataFormat.data;
            }
            switch (eventCode)
            {
                case (byte)0:   // eventCode 자리
                    break;
            }
        }
    }
}
