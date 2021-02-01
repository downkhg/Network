/*##################################
게임에코서버_스레드(포트폴리오 수업용)
파일명: GameServer.cs
작성자 : 김홍규(downkhg@gmail.com)
마지막수정날짜 : 2019.11.28
버전 : 1.00
###################################*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS_GameServer
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

    public class GameServer
    {
        Socket m_socketServer;
        IPAddress m_iPAddress;
        int m_nPort = -1;
        bool m_isStart = false;
        int m_nAcepptCount = 0;
        List<SocketInfo> m_listSocketInfo = new List<SocketInfo>();

        public int AcepptCount { get { return m_nAcepptCount; } }
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
            Console.WriteLine("Server IPAddress: {0}...", m_iPAddress);
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
                Console.WriteLine("Server Bind...{0}...", port);
                m_socketServer.Listen(backlog);
                Console.WriteLine("Server Listening...");
                m_isStart = true;
                return true;
            }
            Console.WriteLine("Server IP or Port Err...");
            return false;
        }
        //현재 클라이언트와 서버의 상태를 확인한다.
        public void ShowEndPointCheck()
        {
            Console.WriteLine("######### Cilent: {0} #########", m_listSocketInfo.Count);
            for (int i = 0; i < m_listSocketInfo.Count; i++)
                Console.WriteLine("{0}:{1}", i, m_listSocketInfo[i].Socket.RemoteEndPoint.ToString());
            Console.WriteLine("######### Server: {0} #########", m_socketServer.LocalEndPoint.ToString());
        }
        //대기하고있는 어셉트가 없으면 새로운 어샙트를 대기는 콜백함수
        public void MultiClientAcceptCallBack()
        {
            do
            {
                //어셉트가 완료되면 다음 사람이 접속할 어셉트를 대기시킨다.
                if (m_listSocketInfo.Count == m_nAcepptCount)
                {
                    Console.WriteLine("AcceptCount:{0}/{1}", m_nAcepptCount, m_listSocketInfo.Count);
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
                BroadCastMassage(string.Format("client:{0}",m_listSocketInfo.Count));
                //접속완료된 소켓확인하기
                ShowEndPointCheck();
                string strData = null;
                do
                {
                    byte[] bytes = socketInfo.GetBuffer();

                    socketClient.Receive(bytes);

                    strData = System.Text.Encoding.UTF8.GetString(bytes);
                    Console.WriteLine(strData.Split(splitChar)[0]);
                    socketInfo.ClearBuffer();
                    BroadCastMassage(strData);
                }
                while (socketInfo.Connect);
            }
            catch (Exception e)
            {
                socketInfo.Connect = false;
                Console.WriteLine("Exception:" + e);
            }
            if (socketClient == null)
                Console.WriteLine("Accept Cancle");
            //클라이언트와 연결이 종료되면 클라이언트 리스트에서 삭제한다.
            m_listSocketInfo.Remove(socketInfo);
            m_nAcepptCount--;
            BroadCastMassage(string.Format("client:{0}", m_listSocketInfo.Count));
            Console.WriteLine("AcceptCount:{0}/{1}", m_nAcepptCount, m_listSocketInfo.Count);
            Console.WriteLine("AcceptCallBack End!!");
        }
        //접속된 특정 클라이언트에 데이터를 전송한다.
        public void SendtoSocket(int idx, string msg)
        {
            Send(m_listSocketInfo[idx].Socket, msg);
        }
        //접속된 모든 클라에게 메세지를 전송한다.
        public void BroadCastMassage(string msg)
        {
            for (int i = 0; i < m_listSocketInfo.Count; i++)
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
}
