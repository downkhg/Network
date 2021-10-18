using CSToUnityUtill;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkFramework
{
    public struct ReceiveInfo
    {
        public EndPoint endPoint;
        public byte[] buffer;
        public int reciveCount;

        public ReceiveInfo(EndPoint endPoint, byte[] buffer, int reciveCount)
        {
            this.endPoint = endPoint;
            this.buffer = buffer;
            this.reciveCount = reciveCount;
        }

        public string GetMsg()
        {
            return Encoding.UTF8.GetString(buffer, 0, reciveCount);
        }
    }

    class ReceiveCallBack
    {
        public delegate ReceiveInfo ReceiveFunction();
        //받은데이터를 관리하기 위한 큐
        Queue<ReceiveInfo> m_queReceives = new Queue<ReceiveInfo>();
        bool m_isReceiving = false;

        ReceiveFunction m_receiveFunction;

        public ReceiveCallBack(ReceiveFunction receiveFunction)
        {
            m_receiveFunction = receiveFunction;
        }

        public bool MsgExist()
        {
            return (m_queReceives.Count > 0);
        }

        public ReceiveInfo GetMsg()
        {
            return m_queReceives.Dequeue();
        }

        public bool CheckReceve()
        {
            return m_isReceiving;
        }

        public void StartReceive()
        {
            m_isReceiving = true;
        }

        public void StopReceive()
        {
            m_isReceiving = false;
        }

        public void ProcessReceive()
        {
            Log.WriteLine("ProcessRecive Start");
            while (m_isReceiving)
            {
                m_queReceives.Enqueue(m_receiveFunction());
            }
            Log.WriteLine("ProcessRecive End");
        }
    }

    class UDPSocketWrapper
    {
        Socket m_socketClient;
        int m_nBufferSize = 1024;

        HashSet<EndPoint> m_listConnectEndIp = new HashSet<EndPoint>();

        public void Init() //소켓초기화
        {
            Log.WriteLine("UDPSocketWrapper Init...");
            m_socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Bind(int port)//서버측에서 접속 대기
        {
            Log.WriteLine("UDPSocketWrapper Bind...");
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            m_socketClient.Bind(ipEndPoint);
            Log.WriteLine("UDPSocketWrapper Bind..."+ ipEndPoint);
        }

        public void Close()
        {
            m_socketClient.Close();
        }
        //문자열메세지를 보냄 //지정경로로 보냄
        public void SendStringMsg(string msg, string ip, int port)
        {
            Log.WriteLine("UDPSocketWrapper Send_", ip, port, msg);
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(ip), port);
            m_socketClient.SendTo(bytes, bytes.Length, SocketFlags.None, serverEP);
        }
        //지정된 EndPoint로 메세지전송
        public void SendStringMsg(EndPoint sendEP, string msg)
        {
            Log.WriteLine("UDPSocketWrapper Send_", sendEP, msg);
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            m_socketClient.SendTo(bytes, bytes.Length, SocketFlags.None, sendEP);
        }

        public ReceiveInfo RecivedData()
        {
            Log.WriteLine("UDPSocketWrapper Received Start...");
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteEP = (EndPoint)sender;
            byte[] bytes = new byte[m_nBufferSize];
            int nRecvCount = m_socketClient.ReceiveFrom(bytes, ref remoteEP);
            ReceiveInfo receiveData = new ReceiveInfo(remoteEP, bytes, nRecvCount);
            m_listConnectEndIp.Add(remoteEP);
            Log.WriteLine("UDPSocketWrapper Received End...");
            return receiveData;
        }

        public void SendBoradCast(string msg)
        {
            foreach (var endPoint in m_listConnectEndIp)
            {
                SendStringMsg(endPoint, msg);
            }
        }
    }

    static class UDPSocketWrapperSample
    {
        public static void TestServerMain()
        {
            int recv = 0;
            byte[] data = new byte[1024];
            int nPort = 9050;

            UDPSocketWrapper udpSocketServer = new UDPSocketWrapper();
            udpSocketServer.Init();
            udpSocketServer.Bind(nPort);

            Log.WriteLine("Waiting for a client...");

            ReceiveInfo sInfoReceiveInfo = udpSocketServer.RecivedData();

            Log.WriteLine("received data : {0}", sInfoReceiveInfo.GetMsg());

            string welcome = "Welcome to udp server";
            udpSocketServer.SendStringMsg(sInfoReceiveInfo.endPoint, welcome);

            ReceiveCallBack receiveCallBack = new ReceiveCallBack(udpSocketServer.RecivedData);
            Task taskResive = new Task(receiveCallBack.ProcessReceive);
            taskResive.Start();

            receiveCallBack.StartReceive();
            while (receiveCallBack.CheckReceve())
            {
                if (receiveCallBack.MsgExist())
                {
                    ReceiveInfo sReceivedInfo = receiveCallBack.GetMsg();
                    string strMsg = sReceivedInfo.GetMsg();
                    Console.WriteLine("received data : {0}", strMsg);
                    if (strMsg == "remote_exit")
                    {
                        receiveCallBack.StopReceive();
                        Log.WriteLine("Server Exit : {0}", strMsg);
                        break;
                    }
                    else
                        udpSocketServer.SendStringMsg(sReceivedInfo.endPoint, strMsg);
                }
            }

            taskResive.Wait();
            udpSocketServer.Close();
        }

        public static void TestCilentMain()
        {
            string input;

            UDPSocketWrapper udpSocketClient = new UDPSocketWrapper();
            string ip = "127.0.0.1";
            int port = 9050;

            udpSocketClient.Init();
            udpSocketClient.SendStringMsg("hello, udp server?", ip, port);

            ReceiveInfo sInfoReceiveInfo = udpSocketClient.RecivedData();
            Log.WriteLine("received data : {0}", sInfoReceiveInfo.GetMsg());

            ReceiveCallBack receiveCallBack = new ReceiveCallBack(udpSocketClient.RecivedData);
            Task taskResive = new Task(receiveCallBack.ProcessReceive);
            taskResive.Start();

            receiveCallBack.StartReceive();
            while (receiveCallBack.CheckReceve())
            {
                if (receiveCallBack.MsgExist())
                {
                    ReceiveInfo sReceivedInfo = receiveCallBack.GetMsg();
                    string strMsg = sReceivedInfo.GetMsg();
                    Log.WriteLine("received data : {0}", strMsg);
                    if (strMsg == "remote_exit")
                    {
                        Log.WriteLine("Server Exit : {0}", strMsg);
                        receiveCallBack.StopReceive();
                        break;
                    }
                    else
                        udpSocketClient.SendStringMsg(sReceivedInfo.endPoint, strMsg);
                }
                else
                {
                    Console.Write("send data : ");
                    input = Console.ReadLine();
                    if (input == "exit")
                        break;

                    udpSocketClient.SendStringMsg(input, ip, port);
                }
            }

            taskResive.Wait();
            Log.WriteLine("Stopping client");
            udpSocketClient.Close();
        }

        public static void ChatServerMain()
        {
            int recv = 0;
            byte[] data = new byte[1024];
            //int nPort = 9050;
            int nPort = 15000;

            UDPSocketWrapper udpSocketServer = new UDPSocketWrapper();
            Console.WriteLine("Server Start...");
            udpSocketServer.Init();

            udpSocketServer.Bind(nPort);

            Console.WriteLine("Waiting for a client...");
            ReceiveInfo sInfoReceiveInfo = udpSocketServer.RecivedData();

            Console.WriteLine("Recvie ClientInfo : {0}", sInfoReceiveInfo.GetMsg());

            string strConnetMsg = "Connet Udp Chat!";
            udpSocketServer.SendStringMsg(sInfoReceiveInfo.endPoint, strConnetMsg);

            //입력처리용 태스크 생성 및 처리
            Console.WriteLine("Waiting Client Msg... CallBack Ready...");
            ReceiveCallBack receiveCallBack = new ReceiveCallBack(udpSocketServer.RecivedData);
            Task taskResive = new Task(receiveCallBack.ProcessReceive);
            taskResive.Start();
            Console.WriteLine("Waiting Client Msg... CallBack Start!");
            receiveCallBack.StartReceive();
            while (receiveCallBack.CheckReceve())
            {
                if (receiveCallBack.MsgExist())
                {
                    Console.WriteLine("Client Message Process....");
                    ReceiveInfo sReceivedInfo = receiveCallBack.GetMsg();
                    string strMsg = sReceivedInfo.GetMsg();
                    Console.WriteLine("Client Receive Msg : {0}", strMsg);
                    if (strMsg == "remote_exit")
                    {
                        receiveCallBack.StopReceive();
                        Console.WriteLine("Server Exit : {0}", strMsg);
                        break;
                    }
                    else
                        udpSocketServer.SendStringMsg(sReceivedInfo.endPoint, strMsg);
                    Console.WriteLine("Client Msg Retrun : {0}", strMsg);
                }
            }
            Console.WriteLine("Waiting Client Msg... Message Process end");
            taskResive.Wait();
            Log.WriteLine("Waiting Client Msg... CallBack End");
            udpSocketServer.Close();
            Log.WriteLine("Server Close...");
        }

        public static void ChatCilentMain()
        {
            UDPSocketWrapper udpSocketClient = new UDPSocketWrapper();
            string strIP = "127.0.0.1";
            int nPort = 9050;
            string strID;

            //Console.Write("ServerIP:");
            //strIP = Console.ReadLine();
            //Console.Write("Port:");
            //nPort = int.Parse(Console.ReadLine());

            Console.Write("ID:");
            strID = Console.ReadLine();

            udpSocketClient.Init();
            Log.WriteLine(strID + " Connect to Server.. [" + strIP + "/" + nPort + "]");
            udpSocketClient.SendStringMsg("Connet ID:" + strID, strIP, nPort);

            Log.WriteLine(strID + " Connect to Wait.. [" + strIP + "/" + nPort + "]");
            ReceiveInfo sInfoReceiveInfo = udpSocketClient.RecivedData();
            Log.WriteLine("received data :", sInfoReceiveInfo.GetMsg());
            Log.WriteLine(strID + " Connect to Wait.. [" + strIP + "/" + nPort + "]");

            Log.WriteLine("Waiting Server Msg... CallBack Ready...");
            ReceiveCallBack receiveCallBack = new ReceiveCallBack(udpSocketClient.RecivedData);
            Task taskResive = new Task(receiveCallBack.ProcessReceive);
            Log.WriteLine("Waiting Server Msg... CallBack Start...");
            taskResive.Start();
            receiveCallBack.StartReceive();
            while (receiveCallBack.CheckReceve())
            {
                if (receiveCallBack.MsgExist())
                {
                    Log.WriteLine("Server Message Process....");
                    ReceiveInfo sReceivedInfo = receiveCallBack.GetMsg();
                    string strMsg = sReceivedInfo.GetMsg();
                    Log.WriteLine("Server Received Message :", strMsg);
                    if (strMsg == "remote_exit")
                    {
                        Log.WriteLine("Server Exit_", strMsg);
                        receiveCallBack.StopReceive();
                        break;
                    }
                    //else
                    //    udpSocketClient.SendStringMsg(sReceivedInfo.endPoint, strMsg);
                }
                else
                {
                    Log.WriteLine("Send Msg:");
                    string inputMsg = Console.ReadLine();
                    if (inputMsg == "exit")
                        break;
                    string strInput = string.Format("{0}:{1}", strID, inputMsg);
                    udpSocketClient.SendStringMsg(strInput, strIP, nPort);
                }
            }

            taskResive.Wait();
            Log.WriteLine("Stopping client");
            udpSocketClient.Close();
        }
    }
}
