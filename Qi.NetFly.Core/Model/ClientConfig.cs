using Qi.NetFly.TcpCSFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Qi.NetFly.Core.Model
{
    public class ClientConfig
    {
        /// <summary>
        /// 秘钥：每台想要穿透的内网电脑都要有个秘钥。
        /// </summary>
        public string SecretKey { get; set; } = "qizhuhua"; // 默认一个key
        /// <summary>
        /// 信息，这个变量为服务端提供便利。
        /// 在客户端使用时，可以无视。
        /// </summary>
        public Session Session { get; set; } = null; 

        public List<Qi_LAN_Setting> LAN_list = new List<Qi_LAN_Setting>();
        /// <summary>
        /// 向服务器转发回来的数据，平时都是null，只有回发的时候才有数据。
        /// </summary>
        public TransportToService TransportToService = null;
    }

    public class Qi_LAN_Setting
    {
        public MessageType Type { get; set; } = MessageType.WEB;
        public string IP { get; set; }
        public int Port { get; set; }
    }
    /// <summary>
    /// 向服务器端运输数据
    /// </summary>
    public class TransportToService
    {
        public string MsgID { get; set; }
        public string Content { get; set; }
    }
}
