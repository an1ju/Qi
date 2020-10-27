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
    public class Service : IDisposable
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
        /// 多个户端客：连接到此计算机的通信配置信息
        /// </summary>
        private List<ClientConfig> All_clientList_Config = new List<ClientConfig>();

        private List<ServicePort> PortsForListing = new List<ServicePort>();



        /// <summary>
        /// 构造函数
        /// </summary>
        public Service()
        {
            MakeServiceInitialization();
            MakeListener();


        }

        public void Dispose()
        {
            if (svr.IsRun)
            {
                svr.Stop();
            }
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
            string info = string.Format("一个客户端正在链接:\t\t{0} connect server Session:{1}. Socket Handle:{2}",
             e.Client.ClientSocket.RemoteEndPoint.ToString(),
             e.Client.ID, e.Client.ClientSocket.Handle);
        }

        private void ClientClose(object sender, NetEventArgs e)
        {
            RemoveClientConfig(e);//断开后清理内存
        }



        private void RecvData(object sender, NetEventArgs e)
        {
            try
            {
                ClientConfig temp = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientConfig>(e.Client.Datagram);
                temp.Session = e.Client;

                string key = MakeDataTCP(e, temp);//查询或保存归我们管理的连接

                CreateServicePort(key, temp);

                if (temp.TransportToService !=null)
                {
                    //有转发数据了
                    ShuJuHuiFa(temp);

                }
                //svr.Send(e.Client, "数据已收到");
            }
            catch (Exception ex)
            {
                //不是我们协议的内容，一律干掉。
                //svr.CloseSession(e.Client);
                svr.Send(e.Client, "协议不正确");
            }

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


        #region 私有方法处理内部数据


        /// <summary>
        /// 处理接收来的TCP通讯数据，获取ID
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        /// <returns>返回的ID是 管理试验的ID，注意！不是索引</returns>
        private string MakeDataTCP(NetEventArgs e, ClientConfig data)
        {
            string key = "";

            //默认不知道对应我们的  哪行数据ID，先查一下SESSION，看看有没有。有就可以直接用了。
            //没有进行对比ID操作，进行匹配，数据长度也能获取到，依据配置文件
            //如果还是没有，就中断此链接
            System.Net.EndPoint remote = e.Client.ClientSocket.RemoteEndPoint;
            bool canFindEndPoint = false;
            for (int i = 0; i < All_clientList_Config.Count; i++)
            {
                if (All_clientList_Config[i].SecretKey == data.SecretKey)
                {
                    //找到了
                    canFindEndPoint = true;
                    key = All_clientList_Config[i].SecretKey;

                    //更新 
                    #region 更新数据
                    All_clientList_Config[i].SecretKey = data.SecretKey;
                    All_clientList_Config[i].Session = e.Client;

                    ChaYiGengXin(ref All_clientList_Config[i].LAN_list, data.LAN_list);
                    #endregion

                    break;
                }
            }
            if (canFindEndPoint)
            {
                //找到了
            }
            else
            {
                //没找到，添加一个。
                ClientConfig item = new ClientConfig();
                item.SecretKey = data.SecretKey;
                item.Session = e.Client;
                ChaYiGengXin(ref item.LAN_list, data.LAN_list);
                All_clientList_Config.Add(item);

                key = item.SecretKey;

            }


            return key;
        }
        /// <summary>
        /// 差异更新：相同的不做操作
        /// </summary>
        /// <param name="_old"></param>
        /// <param name="_new"></param>
        private void ChaYiGengXin(ref List<Qi_LAN_Setting> _old, List<Qi_LAN_Setting> _new)
        {
            if (_old.Count == 0)
            {
                //原始没有，直接添加
                for (int i = 0; i < _new.Count; i++)
                {
                    Qi_LAN_Setting temp = new Qi_LAN_Setting();
                    temp.IP = _new[i].IP;
                    temp.Port = _new[i].Port;
                    temp.Type = _new[i].Type;
                    _old.Add(temp);
                }
            }
            else if (_old.Count == _new.Count)
            {
                bool all_the_same = true;
                for (int i = 0; i < _new.Count; i++)
                {
                    if (_old[i].IP == _new[i].IP && _old[i].Port == _new[i].Port && _old[i].Type == _new[i].Type)
                    {
                        //全等
                    }
                    else
                    {
                        all_the_same = false;

                        //因为数量相等，所以直接更新。
                        _old[i].IP = _new[i].IP;
                        _old[i].Port = _new[i].Port;
                        _old[i].Type = _new[i].Type;
                    }
                }

                if (!all_the_same)
                {
                    //只要有一个不等，上面已经更新完毕了。
                }
            }
            else
            {
                //最麻烦的就是这个了，差异化。
                int maxNum = _old.Count > _new.Count ? _old.Count : _new.Count;//取个大数
                int minNum = _old.Count > _new.Count ? _new.Count : _old.Count;//取个小数

                if (_old.Count <= _new.Count)
                {
                    //新的配置比旧的配置多
                    for (int i = 0; i < maxNum; i++) // 考虑数量增加的情况
                    {
                        if (_old.Count > i)
                        {
                            //原来就有，暂时不管。
                        }
                        else
                        {
                            //原先没有。加一行
                            Qi_LAN_Setting temp = new Qi_LAN_Setting();
                            temp.IP = _new[i].IP;
                            temp.Port = _new[i].Port;
                            temp.Type = _new[i].Type;
                            _old.Add(temp);
                        }
                    }
                }
                else
                {
                    //新的配置比旧的配置少
                    List<int> removeIndexS = new List<int>();
                    for (int i = 0; i < maxNum; i++)
                    {
                        if (i > minNum)
                        {
                            removeIndexS.Insert(0, i);
                        }
                        else
                        {
                            //原来就有，暂时不管。
                        }
                    }

                    for (int i = 0; i < removeIndexS.Count; i++)
                    {
                        _old.RemoveAt(removeIndexS[i]);
                    }
                }

            }

        }



        /// <summary>
        /// 断开后清理：断开后清理内存
        /// </summary>
        /// <param name="e"></param>
        private void RemoveClientConfig(NetEventArgs e)
        {
            bool canfind = false;
            for (int i = 0; i < All_clientList_Config.Count; i++)
            {
                if (e.Client.ClientSocket.RemoteEndPoint == All_clientList_Config[i].Session.ClientSocket.RemoteEndPoint)
                {
                    canfind = true;
                    string key = All_clientList_Config[i].SecretKey; // 要用这个关闭服务器端待收数据端口
                    CloseServicePort(key);

                    All_clientList_Config[i].LAN_list.Clear();
                    All_clientList_Config.RemoveAt(i);

                }
            }


        }
        /// <summary>
        /// 关闭端口
        /// </summary>
        /// <param name="key"></param>
        private void CloseServicePort(string key)
        {
            List<int> removeIndexs = new List<int>(); 

            for (int i = 0; i < PortsForListing.Count; i++)
            {
                if (PortsForListing[i].Key == key)
                {
                    //关闭该关掉的端口
                    if (PortsForListing[i].Port.IsRun)
                    {
                        PortsForListing[i].Port.CloseAllClient();
                        PortsForListing[i].Port.Stop();
                    }

                    removeIndexs.Insert(0, i);
                }
            }

            for (int i = 0; i < removeIndexs.Count; i++)
            {
                PortsForListing.RemoveAt(removeIndexs[i]);
            }
        }

        private void CreateServicePort(string key, ClientConfig clientConfig)
        {
            //TcpSvr svr = new TcpSvr(clientConfig.p, This_service_Config.ServiceMaxConnectCount, new Coder(Coder.EncodingMothord.UTF8));

            //1 查个数
            int count = ChaGeShu(key);
            if (count == 0)
            {
                // 开端口
                for (int i = 0; i < clientConfig.LAN_list.Count; i++)
                {
                    ServicePort temp = new ServicePort();
                    temp.Key = key;
                    temp.Client = clientConfig.Session;
                    temp.Lan = clientConfig.LAN_list[i];

                    ushort port = MakePortForClient(); // 这里要内部做一个分配端口的方法。
                    temp.Port = new TcpSvr(port, 25, new Coder(Coder.EncodingMothord.UTF8));

                    //处理客户端连接数已满事件
                    temp.Port.ServerFull += new NetEvent(clientServerFull);
                    //处理新客户端连接事件
                    temp.Port.ClientConn += new NetEvent(clientClientConn);
                    //处理客户端关闭事件
                    temp.Port.ClientClose += new NetEvent(clientClientClose);
                    //处理接收到数据事件
                    temp.Port.RecvData += new NetEvent(clientRecvData);

                    if (!temp.Port.IsRun)
                    {
                        temp.Port.Start();
                    }

                    PortsForListing.Add(temp);
                }
            }
            else
            {
                //2 对比
                if (count == clientConfig.LAN_list.Count)
                {
                    //不动
                }
                else
                {
                    //清除后，重新开。
                    CloseServicePort(key);
                    CreateServicePort(key, clientConfig);
                }
            }
            
        }


        private ushort MakePortForClient()
        {
            ushort[] WhatWeHave = new ushort[] { 9966, 8868, 9205 };

            int index = -1;
            //查询一下当前没被占用的端口
            for (int i = 0; i < WhatWeHave.Length; i++)
            {
                bool thisCanUse = true;
                for (int iPorts = 0; iPorts < PortsForListing.Count; iPorts++)
                {
                    if (PortsForListing[iPorts].Port.Port== WhatWeHave[i])
                    {
                        thisCanUse = false;
                        break;
                    }
                }

                if (thisCanUse)
                {
                    index = i;
                    break;
                }
                else
                {
                    continue;
                }
            }

            return WhatWeHave[index];
        }

        #region 内部开放端口事件

        private void clientServerFull(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void clientClientConn(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void clientClientClose(object sender, NetEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void clientRecvData(object sender, NetEventArgs e)
        {
            //1.看看，哪个端口发来的数据。
            TcpSvr CustomerInClientPort = (TcpSvr)sender; //外网用户连接到我们的哪个端口了。
            //2.找对应的Session
            int customerIndex = -1;
            for (int i = 0; i < PortsForListing.Count; i++)
            {
                if (PortsForListing[i].Port.Port== CustomerInClientPort.Port)
                {
                    //找到了
                    customerIndex = i;
                    // Session session = PortsForListing[i].Client; //客户端的信息，一会用
                    // PortsForListing[i].Lan; // 客户端要连接的内网 ip和端口，一会用
                    break;
                }
            }
            ////3.找目标IP PORT TYPE
            //int clientIndex = -1;
            //for (int i = 0; i < All_clientList_Config.Count; i++)
            //{
            //    if (All_clientList_Config[i].Session.ID== PortsForListing[customerIndex].Client.ID)
            //    {
            //        clientIndex = i;
            //        break;
            //    }
            //}

            //4.去客户端抓取内容
            MessageForClient messageForClient = new MessageForClient();
            messageForClient.MsgID = Guid.NewGuid().ToString();
            messageForClient.MessageType = PortsForListing[customerIndex].Lan.Type;
            messageForClient.IP = PortsForListing[customerIndex].Lan.IP;
            messageForClient.Port = PortsForListing[customerIndex].Lan.Port;
            messageForClient.Content = e.Client.Datagram;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(messageForClient);

            singIn.Add(PortsForListing[customerIndex].Client, messageForClient.MsgID, e.Client);
            svr.Send(PortsForListing[customerIndex].Client, json); //此处待补充 ：2020年10月26日16:01:01
            
            
        }
        #endregion


        private SignIn singIn = new SignIn();
        private void ShuJuHuiFa(ClientConfig clientZhuanFa)
        {
            //1.找msgID
            for (int i = 0; i < singIn.TongXunLu.Count; i++)
            {
                if (singIn.TongXunLu[i].MsgID == clientZhuanFa.TransportToService.MsgID)
                {
                    //2.找外网连接，从【PortsForListing】里面找
                    for (int iPorts = 0; iPorts < PortsForListing.Count; iPorts++)
                    {
                        if (PortsForListing[iPorts].Client.ID== singIn.TongXunLu[i].ClientSession.ID)
                        {
                            PortsForListing[iPorts].Port.Send(singIn.TongXunLu[i].CustomerSession, clientZhuanFa.TransportToService.Content);
                            break;
                        }
                    }

                    //svr.Send(singIn.TongXunLu[i].Session, clientZhuanFa.TransportToService.Content);
                    //singIn.Remove(clientZhuanFa.TransportToService.MsgID);
                    singIn.TongXunLu.RemoveAt(i);
                    break;

                }
            }
        }


        /// <summary>
        /// 查个数：某个key 在内部总共分配了多少个端口
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private int ChaGeShu(string key)
        {
            int r = 0;
            for (int i = 0; i < PortsForListing.Count; i++)
            {
                if (PortsForListing[i].Key == key)
                {
                    r++;
                }
            }
            return r;
        }


        #endregion
    }
}
