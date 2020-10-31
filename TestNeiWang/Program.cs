using NewLife.Log;
using NewLife.Net;
using System;
using System.Net.Sockets;
using System.Text;


namespace TestNeiWang
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            NetServer svr = new NetServer
            {
                Port = 2020,
                Log = XTrace.Log,
                SessionLog = XTrace.Log,
                LogSend = true,
                LogReceive = true
            };
            svr.Received += (s, e) => //这里有一点问题。报文长度。的事情。2020年10月30日16:57:30
            {
                var clientInService = s as INetSession; //我把这个叫做客户端连接进来的身份

                var uri = new NetUri(NetType.Tcp, "192.168.31.29", 80);
                var client = new TcpClient();
                client.Connect(uri.EndPoint);

                var ns = client.GetStream();
                ns.Write(e.Packet.Data, 0, e.Packet.Count);//发送
                var buf = new Byte[1024 * 64];
                var rs = ns.Read(buf, 0, buf.Length);//回收
                string Lan_Data = Encoding.UTF8.GetString(buf, 0, rs);//内网回发的数据报文内容

                byte[] cc = Encoding.UTF8.GetBytes(Lan_Data);//Lan_Data.GetBytes();


                clientInService.Send(cc);
            };
            svr.Start();

            Console.ReadLine();
        }
    }
}
