# CSharp-Tcp-Server

- Photon과 유사하게 동작하게 만들어본 TCP Socket Server
<br/>

### 사용방법

- dotnet run / debug project / build 세가지 방식 중 편한 방식으로 프로젝트 실행
- ipaddress 에 3000 포트를 붙여 서버에 접속

<br/>
<hr/>

- DataFormat 클래스에 맞추어 데이터 생성 ( 서버와 클라간 데이터 송 수신에 사용되는 포맷 )

<br/>

- WebSocketController - SendPacket(byte eventCode, Dictionary<byte, object> param) -> 클라에 메시지를 보낼 때 사용 
- MessageManager - ProcessData(WebSocketController wsController, DataFormat dataFormat)  -> 클라에서 온 데이터 처리를 할 때 사용 
     * 클라에서 데이터를 보내면 자동으로 넘어옴 
     * 포맷은 DataFormat에 맞추어야 함
     * 변수명 주의할 것

<br />

C#으로 만든 Tcp server (ms 가이드 참고)

