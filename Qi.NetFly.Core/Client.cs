using Qi.NetFly.Core.Model;
using Qi.NetFly.TcpCSFramework;
using System;
using System.Net;
using System.Net.Sockets;

namespace Qi.NetFly.Core
{
    public class Client
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string Service_IP { get; set; } = "127.0.0.1";
        /// <summary>
        /// 服务器端口号
        /// </summary>
        public int Service_Port { get; set; } = 2020;

        /// <summary>
        /// 当前这一台客户机的配置，我想暴露什么，都标识到这里面去
        /// </summary>
        private ClientConfig This_clientConfig = new ClientConfig();

        /// <summary>
        /// TCP通信核心层(客户端)
        /// </summary>
        private TcpCli cli = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Client()
        {
            Initialization();
            MakeConnectWithListener();
        }

        #region 封装内部处理
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialization()
        {
            This_clientConfig.SecretKey = "qizhuhua";

            Qi_LAN_Setting temp1 = new Qi_LAN_Setting();
            temp1.IP = "192.168.99.93";
            temp1.Port = 5002;
            This_clientConfig.LAN_list.Add(temp1);

            Qi_LAN_Setting temp2 = new Qi_LAN_Setting();
            temp2.IP = "192.168.99.93";
            temp2.Port = 80;
            This_clientConfig.LAN_list.Add(temp2);

        }
        /// <summary>
        /// 构建连接
        /// </summary>
        private void MakeConnectWithListener()
        {
            cli = new TcpCli(new Coder(Coder.EncodingMothord.UTF8));

            cli.Resovlver = new DatagramResolver("]}");
            cli.ReceivedDatagram += new NetEvent(RecvData);
            cli.DisConnectedServer += new NetEvent(ClientClose);
            cli.ConnectedServer += new NetEvent(ClientConn);


            cli.Connect(Service_IP, Service_Port);
            System.Threading.Thread.Sleep(2000);
            cli.Send("wo shi qizhuhua");
        }

        private void ClientConn(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ClientClose(object sender, NetEventArgs e)
        {
            
        }

        private void RecvData(object sender, NetEventArgs e)
        {
            
        }
        #endregion

    }
}
