using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkFramework;

namespace UDPServer
{
    class Program
    {
        static void UdpServerMain()
        {
            // (1) UdpClient 객체 성성. 포트 7777 에서 Listening
            UdpClient cUdpClient = new UdpClient(7777);

            // 클라이언트 IP를 담을 변수
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                // (2) 데이타 수신
                byte[] dgram = cUdpClient.Receive(ref remoteEP);
                Console.WriteLine("[Receive] {0} 로부터 {1} 바이트 수신", remoteEP.ToString(), dgram.Length);
                string strMsg = System.Text.Encoding.UTF8.GetString(dgram);
                Console.WriteLine("Msg:" + strMsg);
                // (3) 데이타 송신
                cUdpClient.Send(dgram, dgram.Length, remoteEP);
                Console.WriteLine("[Send] {0} 로 {1} 바이트 송신", remoteEP.ToString(), dgram.Length);
            }
        }

        static void UdpSoketServerMain()
        {
            int recv = 0;
            byte[] data = new byte[1024];
            int port = 15000;//9050;

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(ep);

            Console.WriteLine("Waiting for a client...");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, port);
            EndPoint remoteEP = (EndPoint)sender;

            recv = server.ReceiveFrom(data, ref remoteEP);

            Console.WriteLine("[first] Message received from {0}", remoteEP.ToString());
            Console.WriteLine("[first] received data : {0}", Encoding.UTF8.GetString(data, 0, recv));

            string welcome = "Welcome to udp server";
            data = Encoding.UTF8.GetBytes(welcome);
            server.SendTo(data, remoteEP);

            while (true)
            {
                data = new byte[1024];
                recv = server.ReceiveFrom(data, ref remoteEP);
                string recvData = Encoding.UTF8.GetString(data, 0, recv);
                Console.WriteLine("received data : {0}", recvData);

                server.SendTo(Encoding.UTF8.GetBytes(recvData), remoteEP);
                Console.WriteLine("send data : {0}", Encoding.UTF8.GetString(data, 0, recv));
                Console.WriteLine("");
            }

            server.Close();
        }

        

        //static void UdpSoketServerWrapperMain()
        //{
        //    int recv = 0;
        //    byte[] data = new byte[1024];
        //    int nPort = 9050;

        //    UDPSocketServer udpSocketServer = new UDPSocketServer();
        //    udpSocketServer.Init();
        //    udpSocketServer.Bind(nPort);

        //    Console.WriteLine("Waiting for a client...");

        //    ReceiveInfo sClientInfo = udpSocketServer.RecivedData();

        //    Console.WriteLine("received data : {0}", sClientInfo.GetMsg());

        //    string welcome = "Welcome to udp server";
        //    udpSocketServer.SendData(sClientInfo.endPoint,welcome);


        //    while (true)
        //    {

        //        ReceiveInfo recvData = udpSocketServer.RecivedData();
        //        string msg = recvData.GetMsg();
        //        Console.WriteLine("received data : {0}", recvData.GetMsg());

        //        udpSocketServer.SendData(recvData.endPoint, msg);
        //    }

        //    udpSocketServer.Close();
        //}


        //static void UdpSoketServerTaskMain()
        //{
        //    int recv = 0;
        //    byte[] data = new byte[1024];
        //    int nPort = 9050;

        //    UDPSocketServer udpSocketServer = new UDPSocketServer();
        //    udpSocketServer.Init();
        //    udpSocketServer.Bind(nPort);

        //    Console.WriteLine("Waiting for a client...");

        //    ReceiveInfo sInfoReceiveInfo = udpSocketServer.RecivedData();
          
        //    Console.WriteLine("received data : {0}", sInfoReceiveInfo.GetMsg());

        //    string welcome = "Welcome to udp server";
        //    udpSocketServer.SendData(sInfoReceiveInfo.endPoint, welcome);

        //    Task taskResive = new Task(udpSocketServer.ReceiveCallBack);
        //    taskResive.Start();

        //    udpSocketServer.StartReceive();
        //    while (udpSocketServer.CheckReceve())
        //    {
        //        if (udpSocketServer.MsgExist())
        //        {
        //            ReceiveInfo sReceivedInfo = udpSocketServer.GetMsg();
        //            string strMsg = sReceivedInfo.GetMsg();
        //            Console.WriteLine("received data : {0}", strMsg);
        //            if (strMsg == "remote_exit")
        //            {
        //                Console.WriteLine("Server Exit : {0}", strMsg);
        //                udpSocketServer.StopReceive();
        //                break;
        //            }
        //            udpSocketServer.SendData(sReceivedInfo.endPoint, strMsg);
        //        }
        //    }

        //    taskResive.Wait();
        //    udpSocketServer.Close();
        //}

        //출처: https://it-jerryfamily.tistory.com/entry/Program-CUDP-통신-기본Socket-콘솔-버전 [IT 이야기]

        static void Main(string[] args)
        {
            //UdpSoketServerWrapperMain();
            //UdpSoketServerWrapperMain();
            //UDPSocketWrapperSample.ChatServerMain();
            UDPSocketWrapperSample.ChatServerMain();
            //UDPSocketWrapperSample.ChatCilentMain();
        }
    }
}
