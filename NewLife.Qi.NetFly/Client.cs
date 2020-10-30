using NewLife.Net;
using NewLife.Qi.NetFly.Model;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;


namespace NewLife.Qi.NetFly
{
    public class Client
    {
        Socket socketClient = null;
        /// <summary>
        /// 客户端配置信息
        /// 我想暴露什么内网设备，都标识到这里面去
        /// </summary>
        private ClientConfig This_client_Config = new ClientConfig();

        /// <summary>
        /// 定时检测连接，发送客户端内网 标注的信息。
        /// 没连接就连接，已连接就发送内网结构配置。
        /// </summary>
        private System.Timers.Timer timer_Connect = null;

        public Client()
        {
            Initialization();
            MakeConnectWithListener();

            timer_Connect = new System.Timers.Timer(10000);
            timer_Connect.Elapsed += Timer_Connect_Elapsed;
            timer_Connect.Start();
        }

        private void Timer_Connect_Elapsed(object sender, ElapsedEventArgs e)
        {
            socketClient.Send(MakeSettingMessage());//发送
        }

        private void Initialization()
        {
            //TODO:这部分最后加，现在暂时使用默认值。
            This_client_Config.SecretKey = "qizhuhua";

            This_client_Config.LAN_list.Clear();

            Qi_LAN_Setting temp1 = new Qi_LAN_Setting();
            temp1.Type = ConnectType.WEB;
            temp1.Note = "我的测试网站";
            temp1.IP = "192.168.31.29";
            temp1.Port = 80;
            This_client_Config.LAN_list.Add(temp1);

            Qi_LAN_Setting temp2 = new Qi_LAN_Setting();
            temp2.Type = ConnectType.WEB;
            temp2.Note = "公司电脑远程桌面";
            temp2.IP = "192.168.31.29";
            temp2.Port = 3389;
            This_client_Config.LAN_list.Add(temp2);
        }
        private void MakeConnectWithListener()
        {
            //var uri = new NetUri($"tcp://192.168.99.93:2020");//
            var uri = new NetUri(NetType.Tcp, This_client_Config.Service_IP, This_client_Config.Service_Port);
            socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socketClient.Connect(uri.EndPoint);//连接



            

            //开启一个新的线程不停的接收服务端发来的消息
            Thread th = new Thread(Recive);
            th.IsBackground = true;
            th.Start();


            socketClient.Send(MakeSettingMessage());//发送


        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="obj"></param>
        private void Recive(object obj)
        {
            while (true)
            {
                byte[] buffer = new byte[1024 * 1024 * 3];
                int r = socketClient.Receive(buffer);

                string RecvData = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                Qi_NETFLY_Message temp = Newtonsoft.Json.JsonConvert.DeserializeObject<Qi_NETFLY_Message>(RecvData);

                switch (temp.MessageType)
                {
                    case MessageType.UPLOAD_CLIENT_SETTINGS_TO_SERVER:
                        { }
                        break;
                    case MessageType.SERVER_ANSWER:
                        { }
                        break;
                    case MessageType.SERVER_TO_CLIENT_FOR_CUSTOMER:
                        {
                            // 把 temp.CustomerData 解析出来
                            string CustomerData = Encoding.UTF8.GetString(temp.CustomerData, 0, temp.CustomerData.Length);

                            //在本客户端中再创建一个客户端，去连接内网设备
                            {
                                var uri = new NetUri(NetType.Tcp, temp.CustomerIP, temp.CustomerPort);
                                var client = new TcpClient();
                                client.Connect(uri.EndPoint);

                                var ns = client.GetStream();
                                ns.Write(temp.CustomerData, 0, temp.CustomerData.Length);//发送
                                var buf = new Byte[1024 * 64];
                                var rs = ns.Read(buf, 0, buf.Length);//回收
                                string Lan_Data = Encoding.UTF8.GetString(buf, 0, rs);//内网回发的数据报文内容


                                //修改消息

                                temp.MessageType = MessageType.CLIENT_TO_SERVER_FOR_CUSTOMER;
                                //temp.LAN_list_ClientSettings = new Qi_LAN_Setting[temp.CustomerData.Length];                                
                                temp.CustomerData = Lan_Data.GetBytes(Encoding.UTF8);
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(temp);
                                byte[] vv = json.GetBytes();
                                //new byte[] { 1, 3, 4, 5, 6, 7 }; //

                                socketClient.Send(json.GetBytes());//发送
                            }
                        }
                        break;
                    case MessageType.CLIENT_TO_SERVER_FOR_CUSTOMER:
                        { }
                        break;
                    default:
                        break;
                }
            }
        }



        /// <summary>
        /// 客户端局域网配置信息制作数据包
        /// </summary>
        /// <returns></returns>
        private byte[] MakeSettingMessage()
        {
            //制作消息
            Qi_NETFLY_Message message = new Qi_NETFLY_Message();

            message.Key = This_client_Config.SecretKey;
            message.MessageType = MessageType.UPLOAD_CLIENT_SETTINGS_TO_SERVER;

            //复制配置信息
            message.LAN_list_ClientSettings = new Qi_LAN_Setting[This_client_Config.LAN_list.Count];
            This_client_Config.LAN_list.CopyTo(message.LAN_list_ClientSettings);


            string json = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            return json.GetBytes();
        }
    }
}
