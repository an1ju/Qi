using NewLife.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NewLife.Qi.NetFly.Model
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// 客户端配置信息上传
        /// </summary>
        UPLOAD_CLIENT_SETTINGS_TO_SERVER=0xAA,
        /// <summary>
        /// 服务器应答
        /// </summary>
        SERVER_ANSWER = 0xBB,
        /// <summary>
        /// 转发外网数据到客户端
        /// </summary>
        SERVER_TO_CLIENT_FOR_CUSTOMER = 0xCC,
        /// <summary>
        /// 转发外网数据到服务器
        /// </summary>
        CLIENT_TO_SERVER_FOR_CUSTOMER = 0xDD,
    }


    public class Qi_NETFLY_Message
    {
        public string Key { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType MessageType { get; set; } = MessageType.UPLOAD_CLIENT_SETTINGS_TO_SERVER;
        /// <summary>
        /// 局域网信息列表，在{MessageType} = UPLOAD_CLIENT_SETTINGS_TO_SERVER 时有效
        /// 服务器端接收，客户端发送。
        /// </summary>
        public Qi_LAN_Setting[] LAN_list_ClientSettings { get; set; }
        /// <summary>
        /// 服务器应答信息，在{MessageType} = SERVER_ANSWER 时有效
        /// 服务器端发送，客户端接收。
        /// </summary>
        public string AnswerMsg { get; set; }

        public string CustomerIP { get; set; }
        public int CustomerPort { get; set; }
        /// <summary>
        /// 外网用户数据，上传下达都使用这个。
        /// 在{MessageType} = SERVER_TO_CLIENT_FOR_CUSTOMER 时，
        /// 和{MessageType} = CLIENT_TO_SERVER_FOR_CUSTOMER 时有效。
        /// </summary>

        public byte[] CustomerData { get; set; }
        /// <summary>
        /// 外网连接信息
        /// </summary>
        public INetSession CustomerSession { get; set; }
    }
}
