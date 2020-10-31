using System;
using System.Collections.Generic;
using System.Text;

namespace NewLife.Qi.NetFly.Model
{
    public class ClientConfig
    {
        /// <summary>
        /// 秘钥：每台想要穿透的内网电脑都要有个秘钥。
        /// </summary>
        public string SecretKey { get; set; } = "qizhuhua"; // 默认一个key
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string Service_IP { get; set; } = "192.168.31.29";
        /// <summary>
        /// 服务器端口号
        /// </summary>
        public int Service_Port { get; set; } = 9877;

        /// <summary>
        /// 局域网信息列表
        /// </summary>
        public List<Qi_LAN_Setting> LAN_list = new List<Qi_LAN_Setting>();


        
    }
    /// <summary>
    /// 局域网内部配置类
    /// </summary>
    public class Qi_LAN_Setting
    {
        /// <summary>类型</summary>
        public ConnectType Type { get; set; } = ConnectType.WEB;
        /// <summary>备注</summary>
        public string Note { get; set; }
        /// <summary>IP地址</summary>
        public string IP { get; set; }
        /// <summary>端口号</summary>
        public int Port { get; set; }
    }

    /// <summary>
    /// 消息类型，客户端定义，从服务端下发命令，由客户端执行。
    /// </summary>
    public enum ConnectType : byte
    {
        WEB,
        FTP,
        SSH
    }
}
