using Qi.NetFly.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Qi.NetFly.Core
{
    public class Service
    {
        /// <summary>
        /// 单一服务器
        /// </summary>
        private ServiceConfig service = new ServiceConfig();
        /// <summary>
        /// 多个客户端
        /// </summary>
        private List<ClientConfig> clientList = new List<ClientConfig>();

        /// <summary>
        /// 构造函数
        /// </summary>
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
            //TODO:
        }


    }
}
