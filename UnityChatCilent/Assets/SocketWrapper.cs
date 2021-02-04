
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CSToUnityUtill;
using System;

namespace SocketWrapper
{
    public class SocketInfo
    {
        byte[] m_buffer; //리시브용 버퍼
        bool m_isConnect; //연결상태 확안
        Socket m_socket; //클라이언트의 소켓

        public Socket Socket
        {
            get { return m_socket; }
        }
        public bool Connect
        {
            get { return m_isConnect; }
            set { m_isConnect = value; }
        }
      
        public SocketInfo(Socket socket, int bufSize, bool connect = false)
        {
            m_socket = socket;
            m_buffer = new byte[bufSize];
            m_isConnect = connect;
            Array.Clear(m_buffer, 0, m_buffer.Length);
        }

        public byte[] GetBuffer()
        {
            return m_buffer;
        }

        public void ClearBuffer()
        {
            Array.Clear(m_buffer, 0, m_buffer.Length);
        }
    }

    public class SocketServer
    {
        Socket m_socketServer;
        IPAddress m_iPAddress;
        int m_nPort = -1;
        bool m_isStart = false;
        int m_nAcepptCount = 0;
        List<SocketInfo> m_listSocketInfo = new List<SocketInfo>();

        public int AcepptCount{ get{return m_nAcepptCount;} }
        public List<SocketInfo> ClientList { get { return m_listSocketInfo; } }

        public static IPAddress GetIPAddress()
        {
            IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = null;
            // 처음으로 발견되는 ipv4 주소를 사용한다.
            foreach (IPAddress addr in he.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    iPAddress = addr;
                    break;
                }
            }

            // 주소가 없다면..
            if (iPAddress == null)
                // 로컬호스트 주소를 사용한다.
                iPAddress = IPAddress.Loopback;

            return iPAddress;
        }
        
        //소켓을 초기화하고, 내 IP를 찾아 저장하고, 포트를 설정한다.
        public void Init()
        {
            m_socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            m_iPAddress = GetIPAddress();
            Log.WriteLine("Server IPAddress: {0}...", m_iPAddress);
        }
        //서버를 닫는다.
        public void Close()
        {
            m_isStart = false;
            m_socketServer.Close();
        }
        //서버를 포트에 바인드시키고, 접속 대기상태로 변경한다.
        public bool Bind(int port, int backlog = 10)
        {
            IPEndPoint serverEP = new IPEndPoint(m_iPAddress, port);
            if (serverEP != null)
            {
                m_socketServer.Bind(serverEP);
                Log.WriteLine("Server Bind...{0}...", port);
                m_socketServer.Listen(backlog);
                Log.WriteLine("Server Listening...");
                m_isStart = true;
                return true;
            }
            Log.WriteLine("Server IP or Port Err...");
            return false;
        }
        //현재 클라이언트와 서버의 상태를 확인한다.
        public void ShowEndPointCheck()
        {
            Log.WriteLine("######### Cilent: {0} #########", m_listSocketInfo.Count);
            for(int i = 0; i < m_listSocketInfo.Count; i++)
                Log.WriteLine("{0}:{1}",i,m_listSocketInfo[i].Socket.RemoteEndPoint.ToString());
            Log.WriteLine("######### Server: {0} #########", m_socketServer.LocalEndPoint.ToString());
        }
        //대기하고있는 어셉트가 없으면 새로운 어샙트를 대기는 콜백함수
        public void MultiClientAcceptCallBack()
        {
            do
            {
                //어셉트가 완료되면 다음 사람이 접속할 어셉트를 대기시킨다.
                if (m_listSocketInfo.Count == m_nAcepptCount)
                {
                    Log.WriteLine("AcceptCount:{0}/{1}", m_nAcepptCount, m_listSocketInfo.Count);
                    ThreadStart threadStart = new ThreadStart(AcceptCallBack);
                    Thread thread = new Thread(threadStart);
                    m_nAcepptCount++;
                    thread.Start();
                }
            } while (m_isStart);
        }
        //클라이언트의 접속을 대기하고, 데이터를 받는 콜백함수
        public void AcceptCallBack()
        {
            Console.WriteLine("AcceptCallBack Start!!");
            
            Socket socketClient = null;
            SocketInfo socketInfo = null;
            char[] splitChar = { '\n' };
            try
            {
                //어셉트를 대기한다.
                Console.WriteLine("socket Client Accept...");
                socketClient = m_socketServer.Accept();
                Console.WriteLine(socketClient.RemoteEndPoint.ToString());
                //클라이언트가 접속완료한다.
                Console.WriteLine("socket Client Conneting!!");
                socketInfo = new SocketInfo(socketClient, 1024, true);
                m_listSocketInfo.Add(socketInfo);
                //접속완료된 소켓확인하기
                ShowEndPointCheck();
                string strData = null;
                do
                {
                    byte[] bytes = socketInfo.GetBuffer();
              
                    socketClient.Receive(bytes);

                    strData = System.Text.Encoding.UTF8.GetString(bytes);
                    Log.WriteLine(strData.Split(splitChar)[0]);
                    socketInfo.ClearBuffer();
                    BroadCastMassage(strData);//받은 데이터를 모든 클라에게 전송한다.
                }
                while (socketInfo.Connect);
            }
            catch (Exception e)
            {
                socketInfo.Connect = false;
                Log.WriteLine("Exception:" + e);
            }
            if(socketClient == null)
                Console.WriteLine("Accept Cancle");
            //클라이언트와 연결이 종료되면 클라이언트 리스트에서 삭제한다.
            m_listSocketInfo.Remove(socketInfo);
            m_nAcepptCount--;
            Log.WriteLine("AcceptCount:{0}/{1}", m_nAcepptCount, m_listSocketInfo.Count);
            Log.WriteLine("AcceptCallBack End!!");
        }
        //접속된 특정 클라이언트에 데이터를 전송한다.
        public void SendtoSocket(int idx, string msg)
        {
            Send(m_listSocketInfo[idx].Socket, msg);
        }
        //접속된 모든 클라에게 메세지를 전송한다.
        public void BroadCastMassage(string msg)
        {
            for(int i = 0; i<m_listSocketInfo.Count; i++)
            {
                Send(m_listSocketInfo[i].Socket, msg);
            }
        }
        //특정클라이언트의 소켓에 메세지를 전송한다.
        public void Send(Socket socket, string msg)
        {
            NetworkStream stream = new NetworkStream(socket);
            byte[] bytes = Encoding.ASCII.GetBytes(msg);
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }
    }

    public class SocketClient
    {
        Socket m_socketClient;
        IPAddress m_ServerIPAddress;
        bool m_isRecive = false;
        bool m_isReciveData = false;
        string m_strResiveMsg;

        Queue<byte[]> m_listBufferQueue = new Queue<byte[]>();

        public bool CheckReciveData { get { return m_isReciveData; } }
        public string ResiveMsg { get { m_isReciveData = false; return m_strResiveMsg; } }

        static public IPAddress GetIPAddress()
        {
            IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = null;
            // 처음으로 발견되는 ipv4 주소를 사용한다.
            foreach (IPAddress addr in he.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    iPAddress = addr;
                    break;
                }
            }

            // 주소가 없다면..
            if (iPAddress == null)
                // 로컬호스트 주소를 사용한다.
                iPAddress = IPAddress.Loopback;

            return iPAddress;
        }
        //소켓을 초기화한다.
        public void Init()
        {
            Console.WriteLine("Client Socket Init...");
            m_socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        }
        //서버에 접속을 시도한다.
        public bool Connect(string serverip, int port)
        {
            m_ServerIPAddress = IPAddress.Parse(serverip);
            IPEndPoint iPEndPoint = new IPEndPoint(m_ServerIPAddress, port);
            Log.WriteLine("Client Connecting!" + iPEndPoint);
            try
            {
                m_socketClient.Connect(iPEndPoint);
                Log.WriteLine(m_socketClient.LocalEndPoint.ToString());
                Log.WriteLine(iPEndPoint.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connect Failed!" + ex);
                return false;
            }
            return true;
        }
        //접속된 서버에 메세지를 보낸다.
        public void SendData(string msg)
        {
            try
            {
                string packet = string.Format("{0}:{1}\n", m_ServerIPAddress, msg);
                byte[] bytes = Encoding.UTF8.GetBytes(packet);
                m_socketClient.Send(bytes);
            }
            catch (Exception e)
            {
                Log.WriteLine("Exception:" + e);
            }
        }

        public byte[] GetBuffer()
        {

            byte[] bytes = null;
            if (m_listBufferQueue.Count > 0)
                m_listBufferQueue.Dequeue();
            return bytes;
        }

        //접속된 서버에서 데이터를 받는다.
        public void ReceivedCallBack()
        {
            Console.WriteLine("ReceivedCallBack Start!");
            m_isRecive = true;
            byte[] bytes = null;
            NetworkStream stream = null;
            try
            {
                bytes = new byte[1024];
                stream = new NetworkStream(m_socketClient);

                while (m_isRecive)
                {
                    int byteSize = stream.Read(bytes, 0, bytes.Length);
                    //m_strResiveMsg = Encoding.UTF8.GetString(bytes, 0, byteSize);
                    //Console.WriteLine("Received:" + m_strResiveMsg);
                    //Array.Clear(bytes, 0, 1024);
                    //m_isReciveData = true;
                    m_listBufferQueue.Enqueue(bytes);
                }

                stream.Close();
                Log.WriteLine("ReceivedCallBack End!");
            }
            catch (Exception e)
            {
                Log.WriteLine("Exception:" + e);
            }
        }
    }



    //기능을 테스트하기위해 제공되는 샘플 클래스
    static public class Sample
    {
        //웨퍼를 이용한 다중서버/클라이언트 예제
        static public void Client()
        {
            SocketClient socketClient = new SocketClient();

            socketClient.Init();

            socketClient.Connect("192.168.0.128", 15000);
            //스레드를 이용하여 서버로부터 데이터를 전송받는다.
            ThreadStart threadStart = new ThreadStart(socketClient.ReceivedCallBack);
            Thread thread = new Thread(threadStart);
            thread.Start();

            string msg = null;
            do
            {
                msg = Console.ReadLine();
                socketClient.SendData(msg);
                Log.WriteLine("Send... " + msg);
            } while (msg != "exit");
        }
        static public void Server()
        {
            SocketServer socketServer = new SocketServer();
            //클라이언트의 소켓을 초기화하고 IP를 가져온다.
            socketServer.Init();
            //포트를 지정하고 바인딩하고 리스닝한다.
            socketServer.Bind(15000,10);
            //완료된 서버
            ThreadStart threadStart = new ThreadStart(socketServer.MultiClientAcceptCallBack);
            Thread thread = new Thread(threadStart);

            thread.Start();

            string msg = "";

            do
            {
                Console.Write("MSG:");
                msg = Console.ReadLine();
                //socketServer.SendtoSocket(0,msg); //특정클라이언트만 전송
                socketServer.BroadCastMassage(msg); //모든클라이언트에게 전송
                Log.WriteLine("{0}/{1}", socketServer.AcepptCount, socketServer.ClientList.Count);
            }
            while (msg != "exit");

            Console.WriteLine("Server Close...");
            socketServer.Close();
        }
        //웨퍼없는 소켓으로 구현된 서버/클라이언트 예제
        static public void PureSocketServer()
        {
            Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            IPAddress iPAddress = SocketServer.GetIPAddress();
            int port = 9999;
            IPEndPoint serverEP = new IPEndPoint(iPAddress, port);
            socketServer.Bind(serverEP);
            Log.WriteLine("Server Bind...");
            socketServer.Listen(10);
            Log.WriteLine("Server Listening...");

            Socket socketClient = socketServer.Accept();
            Log.WriteLine("socket Client Accept!!");

            string data = null;
            do
            {
                byte[] bytes = new byte[1024];
                Array.Clear(bytes, 0, 1024);

                socketClient.Receive(bytes);

                data = System.Text.Encoding.UTF8.GetString(bytes);
                Console.WriteLine(data);
                Array.Clear(bytes, 0, 1024);
            }
            while (data != "exit");
            socketServer.Close();
            string msg = Console.ReadLine();
        }
        static public void PureClientMain()
        {
            Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IPAddress iPAddress = SocketServer.GetIPAddress();
            int port = 15000;
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
            try
            {
                socketClient.Connect(iPEndPoint);
                Log.WriteLine(iPEndPoint.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteLine("Connet Fail!" + ex);
            }

            Log.WriteLine("Connet Server...");
            string msg = null;
            do
            {
                msg = Console.ReadLine();

                if (!socketClient.IsBound)
                {
                    Log.WriteLine("서버가 실행되고 있지 않습니다!");
                }

                string packet = string.Format("{0}:{1}\n", iPAddress, msg);
                byte[] bytes = Encoding.UTF8.GetBytes(packet);
                socketClient.Send(bytes);
                Log.WriteLine("Send... " + msg);
            } while (msg != "exit");
        }      
    }
}
