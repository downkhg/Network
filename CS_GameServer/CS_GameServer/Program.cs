using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS_GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer socketServer = new GameServer();
            //클라이언트의 소켓을 초기화하고 IP를 가져온다.
            socketServer.Init();
            //포트를 지정하고 바인딩하고 리스닝한다.
            socketServer.Bind(15000, 10);
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
                Console.WriteLine("{0}/{1}", socketServer.AcepptCount, socketServer.ClientList.Count);
            }
            while (msg != "exit");
            Console.WriteLine("Server Close...");
            socketServer.Close();
        }
    }
}
