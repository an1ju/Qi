using System;
using System.Collections.Generic;
using System.Text;

namespace Qi.NetFly.Core.Model
{
    /// <summary>
    /// 消息类型，客户端定义，从服务端下发命令，由客户端执行。
    /// </summary>
    public enum MessageType : byte
    {
        WEB,
        FTP,
        SSH
    }
}
