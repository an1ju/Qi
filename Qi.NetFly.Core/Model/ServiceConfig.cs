using System;
using System.Collections.Generic;
using System.Text;

namespace Qi.NetFly.Core.Model
{
    /// <summary>
    /// 服务器端配置项
    /// </summary>
    public class ServiceConfig
    {
        public string ServiceIP { get; set; } = "127.0.0.1";
        public int ServicePort { get; set; } = 2020;
    }
}
