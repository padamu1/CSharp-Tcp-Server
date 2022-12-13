using CSharpTcpServer.Core.Util;
using CSharpTcpServer.Manager;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
namespace CSharpTcpServer.Core
{
    public enum WEB_SOCKET_STATE
    {
        None = 0,
        Connecting = 1,
        Open = 2,
        CloseSent = 3,
        CloseReceived = 4,
        Closed = 5,
        Aborted = 6
    }

    public enum PAYLOAD_DATA_TYPE
    {
        Unknown = -1,
        Continuation = 0,
        Text = 1,
        Binary = 2,
        ConnectionClose = 8,
        Ping = 9,
        Pong = 10
    }
    public class WebSocketController
    {
        public WEB_SOCKET_STATE State { get; protected set; } = WEB_SOCKET_STATE.None;
        protected readonly TcpClient targetClient;
        protected readonly NetworkStream messageStream;
        protected readonly byte[] dataBuffer = new byte[1024];

        public WebSocketController(TcpClient tcpClient)
        {
            State = WEB_SOCKET_STATE.Connecting;  //완전한 WebSocket 연결이 아니므로 연결 중 표시
            targetClient = tcpClient;
            messageStream = targetClient.GetStream();
            messageStream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnReadData, null);
        }
        /// <summary>
        /// 클라이언트에 데이터를 전달하기 위한 함수
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="param"></param>
        public void SendPacket(EventData eventData)
        {
            SendData(ByteUtillity.ObjectToByte(eventData));
        }

        protected void OnReadData(IAsyncResult ar)
        {
            int size = messageStream.EndRead(ar);

            byte[] httpRequestRaw = new byte[7];    //HTTP request method는 7자리를 넘지 않는다.
                                                    //GET만 확인하면 되므로 new byte[3]해도 상관없음
            Array.Copy(dataBuffer, httpRequestRaw, httpRequestRaw.Length);
            string httpRequest = Encoding.UTF8.GetString(httpRequestRaw);

            //GET 요청인지 여부 확인
            if (Regex.IsMatch(httpRequest, "^GET", RegexOptions.IgnoreCase))
            {
                if (State == WEB_SOCKET_STATE.Open) // 이미 연결 중인 상태일 경우 다시 연결 요청에 대한 응답을 할 이유가 없으므로 dispose
                {
                    Dispose();
                    return;
                }
                HandshakeToClient(size);        // 연결 요청에 대한 응답
                State = WEB_SOCKET_STATE.Open;    // 응답이 성공하여 연결 중으로 상태 전환
            }
            else
            {
                if (size == 0) // 비어있는 데이터는 없으므로 dispose
                {
                    Dispose();
                    return;
                }
                // 메시지 수신에 대한 처리, 반환 값은 연결 종료 여부
                if (ProcessClientRequest(size) == false) { return; }
            }
            //데이터 수신 재시작
            messageStream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnReadData, null);
        }

        protected void HandshakeToClient(int dataSize)
        {
            string raw = Encoding.UTF8.GetString(dataBuffer);

            string swk = Regex.Match(raw, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
            string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

            // HTTP/1.1은 연속된 CR, LF를 라인의 끝을 의미하는 마커로 정의
            byte[] response = Encoding.UTF8.GetBytes(
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Connection: Upgrade\r\n" +
                "Upgrade: websocket\r\n" +
                "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

            //요청 승인 응답 전송
            messageStream.Write(response, 0, response.Length);
        }
        protected bool ProcessClientRequest(int dataSize)
        {
            bool fin = (dataBuffer[0] & 0b10000000) != 0;   // 혹시 false일 경우 다음 데이터와 이어주는 처리를 해야 함
            bool mask = (dataBuffer[1] & 0b10000000) != 0;  // 클라이언트에서 받는 경우 무조건 true
            PAYLOAD_DATA_TYPE opcode = (PAYLOAD_DATA_TYPE)(dataBuffer[0] & 0b00001111); // enum으로 변환

            int msglen = dataBuffer[1] - 128; // Mask bit가 무조건 1라는 가정하에 수행
            int offset = 2;     //데이터 시작점
            if (msglen == 126)  //길이 126 이상의 경우
            {
                msglen = BitConverter.ToInt16(new byte[] { dataBuffer[3], dataBuffer[2] });
                offset = 4;
            }
            else if (msglen == 127)
            {
                // 이 부분은 구현 안 함. 나중에 필요한 경우 구현
                Console.WriteLine("Error: over int16 size");
                return true;
            }

            if (mask)
            {
                byte[] decoded = new byte[msglen];
                //마스킹 키 획득
                byte[] masks = new byte[4] { dataBuffer[offset], dataBuffer[offset + 1], dataBuffer[offset + 2], dataBuffer[offset + 3] };
                offset += 4;

                for (int i = 0; i < msglen; i++)    //마스크 제거
                {
                    decoded[i] = (byte)(dataBuffer[offset + i] ^ masks[i % 4]);
                }

                switch (opcode)
                {
                    case PAYLOAD_DATA_TYPE.Text:
                        MessageManager.ProcessData(this, ByteUtillity.ByteToObject(decoded));
                        break;
                    case PAYLOAD_DATA_TYPE.Binary:
                        //Binary는 아무 동작 없음
                        break;
                    case PAYLOAD_DATA_TYPE.ConnectionClose:
                        //받은 요청이 서버에서 보낸 요청에 대한 응답이 아닌 경우에만 실행
                        if (State != WEB_SOCKET_STATE.CloseSent)
                        {
                            SendCloseRequest(1000, "Graceful Close");
                            State = WEB_SOCKET_STATE.Closed;
                        }
                        Dispose();      // 소켓 닫음
                        return false;
                    default:
                        Console.WriteLine("Unknown Data Type");
                        break;
                }
            }
            else
            {
                // 마스킹 체크 실패
                Console.WriteLine("Error: Mask bit not valid");
            }

            return true;
        }
        protected void SendData(byte[] data, PAYLOAD_DATA_TYPE opcode = PAYLOAD_DATA_TYPE.Text)
        {
            byte[] sendData;
            BitArray firstByte = new BitArray(new bool[] {
                    // opcode
                    opcode == PAYLOAD_DATA_TYPE.Text || opcode == PAYLOAD_DATA_TYPE.Ping,
                    opcode == PAYLOAD_DATA_TYPE.Binary || opcode == PAYLOAD_DATA_TYPE.Pong,
                    false,
                    opcode == PAYLOAD_DATA_TYPE.ConnectionClose || opcode == PAYLOAD_DATA_TYPE.Ping || opcode == PAYLOAD_DATA_TYPE.Pong,
                    false,  //RSV3
                    false,  //RSV2
                    false,  //RSV1
                    true,   //Fin
                });

            if (data.Length < 126)
            {
                sendData = new byte[data.Length + 2];
                firstByte.CopyTo(sendData, 0);
                sendData[1] = (byte)data.Length;    //서버에서는 Mask 비트가 0이어야 함
                data.CopyTo(sendData, 2);
            }
            else
            {
                // 수신과 마찬가지로 32,767이상의 길이(int16 범위 이상)의 데이터에 대응하지 못함
                sendData = new byte[data.Length + 4];
                firstByte.CopyTo(sendData, 0);
                sendData[1] = 126;
                byte[] lengthData = BitConverter.GetBytes((ushort)data.Length);
                Array.Copy(lengthData, 0, sendData, 2, 2);
                data.CopyTo(sendData, 4);
            }

            messageStream.Write(sendData, 0, sendData.Length);  //클라이언트에 전송
        }

        public void SendCloseRequest(ushort code, string reason)
        {
            byte[] closeReq = new byte[2 + reason.Length];
            BitConverter.GetBytes(code).CopyTo(closeReq, 0);
            //왜인지는 알 수 없지만 크롬에서 코드는 자리가 바뀌어야 제대로 인식할 수 있다.
            byte temp = closeReq[0];
            closeReq[0] = closeReq[1];
            closeReq[1] = temp;
            Encoding.UTF8.GetBytes(reason).CopyTo(closeReq, 2);
            SendData(closeReq, PAYLOAD_DATA_TYPE.ConnectionClose);
        }

        public virtual void Dispose()
        {
            Console.WriteLine(" Client Disconnected");
            targetClient.Close();
            targetClient.Dispose(); //모든 소켓에 관련된 자원 해제
        }
    }
}
