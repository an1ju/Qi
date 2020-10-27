using Qi.NetFly.TcpCSFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Qi.NetFly.Core.Model
{

    /// <summary>
    /// 转发签到表：
    /// 服务器端和客户端要转发数据，都要签到，
    /// 等回发的数据回来，在消掉。
    /// </summary>
    public class SignIn
    {
        public List<TongXun> TongXunLu = new List<TongXun>();

        public void Add(Session ClientSession, string MsgID,Session CustomerSession)
        {
            TongXun temp = new TongXun();
            temp.ClientSession = ClientSession;
            temp.MsgID = MsgID;
            temp.CustomerSession = CustomerSession;
            TongXunLu.Add(temp);
        }
        public void Remove(string MsgID)
        {
            for (int i = 0; i < TongXunLu.Count; i++)
            {
                if (TongXunLu[i].MsgID == MsgID)
                {
                    TongXunLu.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// 通信登记簿
    /// 服务器端和客户端都要用
    /// </summary>
    public class TongXun
    {
        /// <summary>
        /// 客户端Session
        /// </summary>
        public Session ClientSession { get; set; }
        public string MsgID { get; set; }
        /// <summary>
        /// 外网用户连接Session
        /// </summary>
        public Session CustomerSession { get; set; }
    }
}
