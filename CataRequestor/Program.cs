using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace CataRequestor
{
    class Program
    {
        static void Main(string[] args)
        {
            var q = "";
            while (q != "q")
            {
                string response;
                using (var client = new RequestSocket(">tcp://localhost:3332"))
                {
                    client.SendFrame(Encoding.UTF8.GetBytes("MapData"), false);
                    response = client.ReceiveFrameString();
                }
                Console.WriteLine(response);
                q = Console.ReadLine();
            }
        }
    }
}
