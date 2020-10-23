using System;
using System.Collections.Generic;
using System.Text;

namespace Qi.NetFly.Core.Model
{
    public class ClientConfig
    {
        public string SecretKey { get; set; } = "qizhuhua"; // 默认一个key
        public List<Qi_LAN_Setting> LAN_list = new List<Qi_LAN_Setting>();
    }

    public class Qi_LAN_Setting
    {
        public MessageType Type { get; set; } = MessageType.WEB;
        public string IP { get; set; }
        public int Port { get; set; }
    }
}
