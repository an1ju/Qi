using System;
using System.Collections.Generic;
using System.Text;

namespace Qi.NetFly.Core.Model
{
    public class ClientConfig
    {
        public string SecretKey { get; set; }
        public List<Qi_LAN_Setting> LAN_list = new List<Qi_LAN_Setting>();
    }

    public class Qi_LAN_Setting
    {
        public string IP { get; set; }
        public string Port { get; set; }
    }
}
