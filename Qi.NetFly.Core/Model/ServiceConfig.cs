using Qi.NetFly.TcpCSFramework;

namespace Qi.NetFly.Core.Model
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
        public ushort ServicePort { get; set; } = 2020;
        /// <summary>
        /// 服务器最大连接数
        /// </summary>
        public ushort ServiceMaxConnectCount { get; set; } = 30;
        /// <summary>
        /// 服务启动时，是否自动启动监控服务：默认自动
        /// </summary>
        public bool ServiceAutoRun { get; set; } = true;
    }


    public class ServicePort
    {
        public string Key { get; set; }
        public TcpSvr Port { get; set; }

        public Session Client { get; set; }

        public Qi_LAN_Setting Lan { get; set; }

    }
}
