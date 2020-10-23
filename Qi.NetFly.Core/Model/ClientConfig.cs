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
    }

    public class Qi_LAN_Setting
    {
        public MessageType Type { get; set; } = MessageType.WEB;
        public string IP { get; set; }
        public int Port { get; set; }
    }
}
