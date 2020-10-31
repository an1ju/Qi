using NewLife.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NewLife.Qi.NetFly.Model
{
    /// <summary>
    /// 服务器端配置项
    /// </summary>
    public class ServiceConfig
    {
        /// <summary>
        /// IP地址，这个IP作用不大，将来应该使用的是域名。
        /// </summary>
        public string ServiceIP { get; set; } = "127.0.0.1";
        /// <summary>
        /// 端口号
        /// </summary>
        public ushort ServicePort { get; set; } = 9877;        
        /// <summary>
        /// 服务启动时，是否自动启动监控服务：默认自动
        /// </summary>
        public bool ServiceAutoRun { get; set; } = true;

        /// <summary>
        /// 为外网用户提供的端口，按需分配。
        /// </summary>
        public ushort[] WhatWeHavePortsForCustomer { get; set; } = new ushort[] { 2020, 8868, 9205 };
    }

    public class PortManager_For_Customer
    {
        public string SecretKey { get; set; } // key 标识谁。
        /// <summary>
        /// 客户端连接
        /// </summary>
        public INetSession ServerForClient { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ServicePort_Setting> ServicePort_Setting_List = new List<ServicePort_Setting>();

    }

    public class ServicePort_Setting : Qi_LAN_Setting
    {
        /// <summary>
        /// 记录客户端数据，同时随机分配端口。
        /// </summary>
        public int OpenPort_In_Service_For_Customer { get; set; }

        public NetServer ServerForCustomer { get; set; }
    }
}
