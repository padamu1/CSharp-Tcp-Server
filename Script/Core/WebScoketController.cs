using CShapr_Tcp_Server.Manager;
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
            State = WEB_SOCKET_STATE.Connecting;
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

            byte[] httpRequestRaw = new byte[7];    
                                                    
            Array.Copy(dataBuffer, httpRequestRaw, httpRequestRaw.Length);
            string httpRequest = Encoding.UTF8.GetString(httpRequestRaw);

            //GET 요청인지 여부 확인
            if (Regex.IsMatch(httpRequest, "^GET", RegexOptions.IgnoreCase))
            {
                HandshakeToClient(size);
                State = WEB_SOCKET_STATE.Open;
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
            bool fin = (dataBuffer[0] & 0b10000000) != 0; 
            bool mask = (dataBuffer[1] & 0b10000000) != 0;
            PAYLOAD_DATA_TYPE opcode = (PAYLOAD_DATA_TYPE)(dataBuffer[0] & 0b00001111);

            int msglen = dataBuffer[1] - 128;
            int offset = 2;
            if (msglen == 126)
            {
                msglen = BitConverter.ToInt16(new byte[] { dataBuffer[3], dataBuffer[2] });
                offset = 4;
            }
            else if (msglen == 127)
            {
                Console.WriteLine("Error: over int16 size");
                return true;
            }

            if (mask)
            {
                byte[] decoded = new byte[msglen];
                byte[] masks = new byte[4] { dataBuffer[offset], dataBuffer[offset + 1], dataBuffer[offset + 2], dataBuffer[offset + 3] };
                offset += 4;

                for (int i = 0; i < msglen; i++)
                {
                    decoded[i] = (byte)(dataBuffer[offset + i] ^ masks[i % 4]);
                }

                switch (opcode)
                {
                    case PAYLOAD_DATA_TYPE.Text:
                        // String으로 전송되는 경우
                        MessageManager.ProcessData(this, ByteUtillity.ByteToObject(decoded));
                        break;
                    case PAYLOAD_DATA_TYPE.Binary:
                        // Byte 배열로 전송되는 경우
                        break;
                    case PAYLOAD_DATA_TYPE.ConnectionClose:
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
                    opcode == PAYLOAD_DATA_TYPE.Text || opcode == PAYLOAD_DATA_TYPE.Ping,
                    opcode == PAYLOAD_DATA_TYPE.Binary || opcode == PAYLOAD_DATA_TYPE.Pong,
                    false,
                    opcode == PAYLOAD_DATA_TYPE.ConnectionClose || opcode == PAYLOAD_DATA_TYPE.Ping || opcode == PAYLOAD_DATA_TYPE.Pong,
                    false,  
                    false,  
                    false,  
                    true,   
                });

            if (data.Length < 126)
            {
                sendData = new byte[data.Length + 2];
                firstByte.CopyTo(sendData, 0);
                sendData[1] = (byte)data.Length;
                data.CopyTo(sendData, 2);
            }
            else
            {
                sendData = new byte[data.Length + 4];
                firstByte.CopyTo(sendData, 0);
                sendData[1] = 126;
                byte[] lengthData = BitConverter.GetBytes((ushort)data.Length);
                lengthData.Reverse();
                Array.Copy(lengthData, 0, sendData, 2, 2);
                data.CopyTo(sendData, 4);
            }

            messageStream.Write(sendData, 0, sendData.Length);
        }

        public void SendCloseRequest(ushort code, string reason)
        {
            byte[] closeReq = new byte[2 + reason.Length];
            BitConverter.GetBytes(code).CopyTo(closeReq, 0);
            byte temp = closeReq[0];
            closeReq[0] = closeReq[1];
            closeReq[1] = temp;
            Encoding.UTF8.GetBytes(reason).CopyTo(closeReq, 2);
            SendData(closeReq, PAYLOAD_DATA_TYPE.ConnectionClose);
        }

        public virtual void Dispose()
        {
            Console.WriteLine(" Client Disconnected");
            State = WEB_SOCKET_STATE.Closed;
            targetClient.Close();
            targetClient.Dispose(); //모든 소켓에 관련된 자원 해제
        }
    }
}
