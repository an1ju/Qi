using Qi.NetFly.Core.Model;
using Qi.NetFly.TcpCSFramework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Qi.NetFly.Core
{
    /// <summary>
    /// 内网穿透服务器端。整体封装。
    /// 2020年10月22日15:43:34
    /// </summary>
    public class Service
    {

        private Hashtable m_ChannelSock = new Hashtable();
        /// <summary>
        /// TCP通信核心层
        /// </summary>
        private TcpSvr svr = null;


        /// <summary>
        /// 单一服务器：本台服务器的配置
        /// </summary>
        private ServiceConfig This_service_Config = new ServiceConfig();
        /// <summary>
        /// 多个客户端：连接到此计算机的通信配置信息
        /// </summary>
        private List<ClientConfig> All_clientList_Config = new List<ClientConfig>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public Service()
        {
            MakeServiceInitialization();
            MakeListener();
        }

        #region 封装内部处理
        /// <summary>
        /// 服务初始化：这里要导入集成配置文件
        /// </summary>
        private void MakeServiceInitialization()
        {
            //TODO:这部分最后加，现在暂时使用默认值。 2020年10月22日10:57:37


            svr = new TcpSvr(This_service_Config.ServicePort, This_service_Config.ServiceMaxConnectCount, new Coder(Coder.EncodingMothord.UTF8));
        }
        /// <summary>
        /// 开启监听程序
        /// </summary>
        private void MakeListener()
        {
            // svr.Resovlver = new DatagramResolver("0");//这里的设置没啥用，因为我不准备使用它的解析器。
            //处理客户端连接数已满事件
            svr.ServerFull += new NetEvent(ServerFull);
            //处理新客户端连接事件
            svr.ClientConn += new NetEvent(ClientConn);
            //处理客户端关闭事件
            svr.ClientClose += new NetEvent(ClientClose);
            //处理接收到数据事件
            svr.RecvData += new NetEvent(RecvData);

            if (This_service_Config.ServiceAutoRun)
            {
                if (!svr.IsRun)
                {
                    svr.Start();
                    Console.WriteLine("[Qi NET FLY Server] is listen...{0}", svr.ServerSocket.LocalEndPoint.ToString());
                }
            }
        }

        private void ServerFull(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ClientConn(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ClientClose(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void RecvData(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion


        #region 公开调用方法

        public bool ServiceIsRun()
        {
            return svr.IsRun;
        }

        public ServiceConfig Get_ServiceConfig()
        {
            return This_service_Config;
        }

        public bool Run()
        {
            if (!svr.IsRun)
            {
                svr.Start();
            }

            return true;
        }
        public bool Stop()
        {
            if (svr.IsRun)
            {
                svr.Stop();
            }

            return true;
        }

        public int GetConnectCount()
        {
            return svr.SessionCount;
        }

        #endregion
    }
}
