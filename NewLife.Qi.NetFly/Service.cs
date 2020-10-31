using NewLife.Log;
using NewLife.Net;
using NewLife.Qi.NetFly.Model;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NewLife.Qi.NetFly
{
    /// <summary>
    /// 内网穿透服务器端。整体封装。
    /// 这次试用XCode做底层
    /// 2020年10月29日15:54:21
    /// </summary>
    public class Service
    {
        /// <summary>
        /// 主服务器，一切都以此作为依托。
        /// </summary>
        private NetServer svr = null;

        /// <summary>
        /// 单一服务器：本台服务器的配置
        /// </summary>
        private ServiceConfig This_service_Config = new ServiceConfig();

        /// <summary>
        /// 从客户端的配置信息，进行的端口监听。
        /// </summary>
        private List<PortManager_For_Customer> PortsForListing = new List<PortManager_For_Customer>();

        private List<int> PortsHaveBeenUsed = new List<int>();

        /// <summary>
        /// 签到表：
        /// 谁发来的数据，外网
        /// 做个记号，等数据回来，发送到这里。
        /// 使用 CustomerSession_MsgID 认领
        /// </summary>
        private List<Sing> SingList = new List<Sing>();


        public Service()
        {
            MakeServiceInitialization();
            MakeListener();
        }

        /// <summary>
        /// 服务初始化：这里要导入集成配置文件
        /// </summary>
        private void MakeServiceInitialization()
        {
            //TODO:这部分最后加，现在暂时使用默认值。 2020年10月22日10:57:37

        }
        /// <summary>
        /// 开启监听程序
        /// </summary>
        private void MakeListener()
        {
            if (svr == null)
            {
                svr = new NetServer
                {
                    Port = This_service_Config.ServicePort,
                    Log = XTrace.Log,
                    SessionLog = XTrace.Log,
                    LogSend = true,
                    LogReceive = true
                };

                
                svr.Received += (s, e) => //这里有一点问题。报文长度。的事情。2020年10月30日16:57:30
                {
                    var clientInService = s as INetSession; //我把这个叫做客户端连接进来的身份
                    
                    //这样就能给客户端发送数据 屏蔽掉，先不用。
                    //clientInService.Send(new byte[] { 1, 2, 3 });


                    //接收数据
                    string RecvData = Encoding.UTF8.GetString(e.Packet.Data, 0, e.Packet.Count);
                    try
                    {
                        
                        Qi_NETFLY_Message temp = Newtonsoft.Json.JsonConvert.DeserializeObject<Qi_NETFLY_Message>(RecvData);
                                                
                        DealMessages(clientInService, ref temp);//处理消息
                    }
                    catch (Exception ex)
                    {
                        //接收数据异常
                        //制作消息
                        Qi_NETFLY_Message message = new Qi_NETFLY_Message();
                        message.MessageType = MessageType.SERVER_ANSWER;
                        message.AnswerMsg = ex.Message;
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(message);

                        byte[] replyMessage = json.GetBytes();

                        clientInService.Send(replyMessage);//回复消息
                    }

                };
            }

            if (This_service_Config.ServiceAutoRun) //配置是否自启动
            {
                if (!svr.Active) //当前没启动，就启动。 理应是这样的。
                {
                    svr.Start();
                }
            }
            
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="clientInService"></param>
        /// <param name="message"></param>
        private void DealMessages(INetSession clientInService,ref Qi_NETFLY_Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.UPLOAD_CLIENT_SETTINGS_TO_SERVER:
                    {
                        MakeCustomerPort(clientInService, message); //分配端口，更新客户端发送过来的信息


                        //加工消息                        
                        message.MessageType = MessageType.SERVER_ANSWER;
                        message.AnswerMsg = "配置信息已收到";
                        message.LAN_list_ClientSettings = null;//回发消息不需要带着这些
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                        byte[] replyMessage = json.GetBytes();
                        clientInService.Send(replyMessage);//回复消息
                    }
                    break;
                case MessageType.SERVER_ANSWER:
                    break;
                case MessageType.SERVER_TO_CLIENT_FOR_CUSTOMER:
                    break;
                case MessageType.CLIENT_TO_SERVER_FOR_CUSTOMER:
                    {
                        string CustomerData = Encoding.UTF8.GetString(message.CustomerData, 0, message.CustomerData.Length);
                        //现在，我就差把message.CustomerData 这个数据发到刚刚的那个外网链接上就可以了。
                        for (int i = 0; i < SingList.Count; i++)
                        {
                            if (SingList[i].CustomerSession_MsgID == message.CustomerSession_MsgID)
                            {
                                SingList[i].CustomerSession.Send(message.CustomerData);
                                SingList.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 分配端口，更新客户端发送过来的信息
        /// </summary>
        /// <param name="message"></param>
        private void MakeCustomerPort(INetSession clientInService, Qi_NETFLY_Message message)
        {
            //1.查看Key在不在?不在就新增，在就更新。
            int keyIndex = -1;
            for (int i = 0; i < PortsForListing.Count; i++)
            {
                if (PortsForListing[i].SecretKey == message.Key)
                {
                    //发现
                    keyIndex = i;
                    break;
                }
            }
            //2.
            if (keyIndex == -1)
            {
                //新增流程
                PortManager_For_Customer row = new PortManager_For_Customer();
                row.SecretKey = message.Key;
                row.ServerForClient = clientInService;

                for (int i = 0; i < message.LAN_list_ClientSettings.Length; i++)
                {
                    ServicePort_Setting portItem = new ServicePort_Setting();
                    portItem.IP = message.LAN_list_ClientSettings[i].IP;
                    portItem.Port = message.LAN_list_ClientSettings[i].Port;
                    portItem.Type = message.LAN_list_ClientSettings[i].Type;
                    portItem.Note = message.LAN_list_ClientSettings[i].Note;
                    
                    int portForCustomer = FenPeiPortForCustomer();
                    portItem.OpenPort_In_Service_For_Customer = portForCustomer;

                    //端口已经分配好了，除了 -1 的情况，都应该可以分配，并制作成监听。
                    portItem.ServerForCustomer = MakeCustomerListenPort(portForCustomer);

                    row.ServicePort_Setting_List.Add(portItem);
                }
                PortsForListing.Add(row);
            }
            else
            {
                //更新流程：2020年10月30日 23点12分 这个还没做，别忘了。
                PortsForListing[keyIndex].SecretKey = message.Key;
                PortsForListing[keyIndex].ServerForClient = clientInService;

                ChaYiGengXin_ClientLanSettings(ref PortsForListing[keyIndex].ServicePort_Setting_List, message.LAN_list_ClientSettings);


            }

        }

        /// <summary>
        /// 差异更新：相同的不做操作
        /// </summary>
        /// <param name="_old"></param>
        /// <param name="_new"></param>
        private void ChaYiGengXin_ClientLanSettings(ref List<ServicePort_Setting> _old, Qi_LAN_Setting[] _new)
        {
            if (_old.Count < _new.Length)
            {
                //数量增多了
                for (int i = _old.Count; i < _new.Length; i++)
                {
                    ServicePort_Setting portItem = new ServicePort_Setting();
                    portItem.IP = _new[i].IP;
                    portItem.Port = _new[i].Port;
                    portItem.Type = _new[i].Type;
                    portItem.Note = _new[i].Note;

                    int portForCustomer = FenPeiPortForCustomer();
                    portItem.OpenPort_In_Service_For_Customer = portForCustomer;

                    //端口已经分配好了，除了 -1 的情况，都应该可以分配，并制作成监听。
                    portItem.ServerForCustomer = MakeCustomerListenPort(portForCustomer);

                    _old.Add(portItem);
                }
            }
            else if (_old.Count == _new.Length)
            {
                //数量相同，主要更新在这里进行。注意：原数据相同就不更新。
                for (int i = 0; i < _old.Count; i++)
                {
                    if (_old[i].IP == _new[i].IP && _old[i].Port == _new[i].Port && _old[i].Type == _new[i].Type && _old[i].Note == _new[i].Note)
                    {
                        //完全相同，就不改变了。
                    }
                    else
                    {
                        // 有变化，更新一下。
                        _old[i].IP = _new[i].IP;
                        _old[i].Port = _new[i].Port;
                        _old[i].Type = _new[i].Type;
                        _old[i].Note = _new[i].Note;
                        //注意：已经开放的端口不做改变。
                    }
                    
                }
            }
            else if (_old.Count > _new.Length)
            {
                //数量减少了，把过多的端口释放掉。如果更新，等这步骤完事后，在相同数量时处理。
                for (int i = _new.Length; i > _old.Count; i--)
                {
                    _old[i].ServerForCustomer.Stop("配置减少");
                    _old[i].ServerForCustomer.Dispose();
                    for (int iUsedPort = 0; iUsedPort < PortsHaveBeenUsed.Count; iUsedPort++)
                    {
                        if (PortsHaveBeenUsed[iUsedPort] == _old[i].OpenPort_In_Service_For_Customer) //把已用端口标识去掉 这个刚刚释放的。
                        {
                            PortsHaveBeenUsed.RemoveAt(iUsedPort);
                        }
                    }

                    
                    _old.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 监听外网消息
        /// </summary>
        /// <param name="portForCustomer"></param>
        /// <returns></returns>
        private NetServer MakeCustomerListenPort(int portForCustomer)
        {
            NetServer ForCustomerSer = new NetServer
            {
                Port = portForCustomer,
                Log = XTrace.Log,
                SessionLog = XTrace.Log,
                LogSend = true,
                LogReceive = true
            };
            ForCustomerSer.Received += (s, e) =>
            {
                var customerInService = s as INetSession; //最后数据还要回发到此外网连接中。


                //制作消息
                Qi_NETFLY_Message message = new Qi_NETFLY_Message();
                message.MessageType = MessageType.SERVER_TO_CLIENT_FOR_CUSTOMER;
                message.CustomerSession_MsgID = Guid.NewGuid().ToString(); //customerInService;

                SingList.Add(new Sing() { CustomerSession_MsgID = message.CustomerSession_MsgID, CustomerSession = customerInService });

                //从端口号判定，应该发送到哪个客户端。
                INetSession clientSession = null;
                int index = GetClientSession_Index_From_PortsForListing(customerInService.Host.Local.Port, ref message);



                if (index == -1)
                {
                    clientSession = null;
                }
                else
                {
                    clientSession = PortsForListing[index].ServerForClient;
                }

                if (clientSession != null)
                {
                    //补充消息                   
                    message.CustomerData = e.Packet.Data;
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(message);

                    byte[] replyMessage = json.GetBytes();
                    clientSession.Send(replyMessage); // 转发过去给客户端
                }

                

            };
            ForCustomerSer.Start();

            return ForCustomerSer;
        }

        private int GetClientSession_Index_From_PortsForListing(int port,ref Qi_NETFLY_Message message)
        {
            int index = -1;
            for (int i = 0; i < PortsForListing.Count; i++)
            {
                for (int iPorts = 0; iPorts < PortsForListing[i].ServicePort_Setting_List.Count; iPorts++)
                {
                    if (PortsForListing[i].ServicePort_Setting_List[iPorts].OpenPort_In_Service_For_Customer == port)
                    {
                        index = i;
                        message.CustomerIP = PortsForListing[i].ServicePort_Setting_List[iPorts].IP;
                        message.CustomerPort = PortsForListing[i].ServicePort_Setting_List[iPorts].Port;
                        break;
                    }
                }
            }

            return index;
        }

        /// <summary>
        /// Port 分配
        /// </summary>
        /// <returns></returns>
        private int FenPeiPortForCustomer()
        {
            int index = -1;
            //查询一下当前没被占用的端口

            #region 循环查。

            for (int i = 0; i < This_service_Config.WhatWeHavePortsForCustomer.Length; i++) //全部待用端口
            {
                bool thisCanUse = true;
                for (int portIndex = 0; portIndex < PortsHaveBeenUsed.Count; portIndex++) //客户端组
                {
                    if (PortsHaveBeenUsed[portIndex] == This_service_Config.WhatWeHavePortsForCustomer[i])
                    {
                        thisCanUse = false;
                        break;
                    }

                }

                if (thisCanUse)
                {
                    index = i;
                    PortsHaveBeenUsed.Add(This_service_Config.WhatWeHavePortsForCustomer[index]);
                    break;
                }
                else
                {
                    continue;
                }
            }
            #endregion

            //如果 端口都被占用了，怎么办? 到时候这个 index 是 -1 ，从数组 就要报错了。
            if (index == -1)
            {
                return -1;
            }
            else
            {
                return This_service_Config.WhatWeHavePortsForCustomer[index];
            }
            
        }

        #region 公开调用方法

        public bool ServiceIsRun()
        {
            return svr.Active;
        }

        public ServiceConfig Get_ServiceConfig()
        {
            return This_service_Config;
        }

        public bool Run()
        {
            if (!svr.Active)
            {
                svr.Start();
            }

            return true;
        }
        public bool Stop()
        {
            if (svr.Active)
            {
                svr.Stop("接口命令");
            }

            return true;
        }

        public int GetConnectCount()
        {
            return svr.SessionCount;
        }

        /// <summary>
        /// 通过key，查询自己在服务器的配置以及分配的端口信息。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PortManager_For_Customer GetClientInServer_PortsForListing(string key)
        {
            int index = -1;
            for (int i = 0; i < PortsForListing.Count; i++)
            {
                if (PortsForListing[i].SecretKey == key)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return null;
            }
            else
            {
                return PortsForListing[index];
            }
        }


        #endregion
    }
}
