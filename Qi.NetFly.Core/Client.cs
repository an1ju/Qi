using Qi.NetFly.Core.Model;
using Qi.NetFly.TcpCSFramework;
using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Qi.NetFly.Core
{
    public class Client
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string Service_IP { get; set; } = "192.168.99.93";
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
        /// 状态消息：默认初始化
        /// 后续会有：正常、连接失败、消息发送失败  这类提示信息，供调用者查看。
        /// 2020年10月23日10:19:55
        /// </summary>
        public string StatusMessage = "初始化";
        /// <summary>
        /// 定时检测连接，发送客户端内网 标注的信息。
        /// 没连接就连接，已连接就发送内网结构配置。
        /// </summary>
        private Timer timer_Connect = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Client()
        {
            Initialization();
            MakeConnectWithListener();

            timer_Connect = new Timer(10000);
            timer_Connect.Elapsed += Timer_Connect_Elapsed;
            timer_Connect.Start();
        }
        private void Timer_Connect_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!cli.IsConnected)
            {
                try
                {
                    cli.Connect(Service_IP, Service_Port);
                    StatusMessage = "已连接";
                }
                catch (Exception)
                {
                    StatusMessage = "连接远程服务器失败";
                    cli.Close();
                }
            }
            else
            {

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(This_clientConfig);
                cli.Send(json);
            }
        }

        #region 封装内部处理
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialization()
        {
            This_clientConfig.SecretKey = "qizhuhua";

            Qi_LAN_Setting temp1 = new Qi_LAN_Setting();
            temp1.Type = MessageType.WEB;
            temp1.IP = "192.168.99.93";
            temp1.Port = 5002;
            This_clientConfig.LAN_list.Add(temp1);

            Qi_LAN_Setting temp2 = new Qi_LAN_Setting();
            temp2.Type = MessageType.WEB;
            temp2.IP = "192.168.99.93";
            temp2.Port = 3389;
            This_clientConfig.LAN_list.Add(temp2);


        }
        /// <summary>
        /// 构建连接
        /// </summary>
        private void MakeConnectWithListener()
        {
            cli = new TcpCli(new Coder(Coder.EncodingMothord.UTF8));

            //cli.Resovlver = new DatagramResolver("}");
            cli.ReceivedDatagram += new NetEvent(RecvData);
            cli.DisConnectedServer += new NetEvent(ClientClose);
            cli.ConnectedServer += new NetEvent(ClientConn);

            if (!cli.IsConnected) // 启动就连接
            {
                try
                {
                    cli.Connect(Service_IP, Service_Port);
                    StatusMessage = "已连接";
                }
                catch (Exception ex)
                {
                    StatusMessage = "连接远程服务器失败";
                    cli.Close();
                }
            }
            
        }

        

        private void ClientConn(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ClientClose(object sender, NetEventArgs e)
        {
            cli.Close();
        }


        private void RecvData(object sender, NetEventArgs e)
        {
            try
            {
                //if (e.Client.Datagram.EndsWith("}")&& e.Client.Datagram.StartsWith("{"))
                if (e.Client.Datagram!= "数据已收到")
                {

                    MessageForClient temp = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageForClient>(e.Client.Datagram);

                    singIn.Add(e.Client, temp.MsgID, null);
                    // 收到消息，要进行处理
                    Dispatch(temp);


                }
            }
            catch (Exception ex)
            {
                
                //throw;
            }

        }


        #endregion

        private SignIn singIn = new SignIn();
        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="temp"></param>
        private void Dispatch(MessageForClient customer)
        {
            switch (customer.MessageType)
            {
                case MessageType.WEB:
                    {
                        TcpCli client = new TcpCli(new Coder(Coder.EncodingMothord.UTF8));
                        client.ReceivedDatagram += new NetEvent(ccRecvData);
                        client.DisConnectedServer += new NetEvent(ccClientClose);
                        client.ConnectedServer += new NetEvent(ccClientConn);

                        if (!client.IsConnected)
                        {
                            client.Connect(customer.IP, customer.Port);
                            //System.Threading.Thread.Sleep(500);
                        }


                        int run1ci = 0;
                        while (!client.IsConnected)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                        client.Send(customer.Content);

                    }
                    break;
                case MessageType.FTP:
                    break;
                case MessageType.SSH:
                    break;
                default:
                    break;
            }
        }

        private void ccClientConn(object sender, NetEventArgs e)
        {
            
        }

        private void ccClientClose(object sender, NetEventArgs e)
        {
            e.Client.Close();
        }

        private void ccRecvData(object sender, NetEventArgs e)
        {
            if (singIn.TongXunLu.Count > 0)
            {
                This_clientConfig.TransportToService = new TransportToService();
                This_clientConfig.TransportToService.MsgID = singIn.TongXunLu[0].MsgID;
                This_clientConfig.TransportToService.Content = e.Client.Datagram;

                try
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(This_clientConfig);
                    cli.Send(json);
                    This_clientConfig.TransportToService = null;
                }
                catch (Exception ex)
                {

                    throw;
                }

                singIn.TongXunLu.RemoveAt(0);
            }
        }
    }
}
