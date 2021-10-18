using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_ChatCilent
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketWrapper.Sample.Client();
            //SocketWrapper.Sample.PureSocketServer();
            //SocketWrapper.Sample.PureClientMain();
        }
    }
}
