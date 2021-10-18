
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
    //기능을 테스트하기위해 제공되는 샘플 클래스
    static public class Sample
    {
        //웨퍼를 이용한 다중서버/클라이언트 예제
        static public void Client()
        {
            SocketClient socketClient = new SocketClient();

            socketClient.Init();

            socketClient.Connect("192.168.56.1", 15000);
            //스레드를 이용하여 서버로부터 데이터를 전송받는다.
            ThreadStart threadStart = new ThreadStart(socketClient.ReceivedCallBack);
            Thread thread = new Thread(threadStart);
            thread.Start();

            ThreadStart threadStartProcess = new ThreadStart(
                () => 
                {
                    
                    while (true)
                    {
                        byte[] bytes = socketClient.GetBuffer();
                        if(bytes != null)
                            Console.WriteLine(Encoding.UTF8.GetString(bytes).Trim());
                    }
                }
                );

            Thread threadProcess = new Thread(threadStartProcess);
            threadProcess.Start();
            //큐잉테스트용 테스트
            int idx = 0;
            while (idx < 99999)
            {
                string strTest = string.Format("test{0}", idx);
                socketClient.SendData(strTest);
                //Log.WriteLine("Send... " + msg);
                idx++;
            }
                //입력 전송
            string msg = null;
            do
            {
                msg = Console.ReadLine();
                //for (int i = 0; i < 999; i++)
                {
                    socketClient.SendData(msg);
                    //Log.WriteLine("Send... " + msg);
                }
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
            int port = 9999;
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
