using Microsoft.AspNetCore.Mvc;
using NewLife.Qi.NetFly;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Qi.NetFly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetFlyController : ControllerBase
    {
        private readonly Service s; // 服务就在这里啦

        public NetFlyController(NewLife.Qi.NetFly.Service s)
        {
            this.s = s;
        }
        // GET: api/<NetFlyController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Yes", "You can fly on net." };
        }

        #region 常用方法接口

        [HttpGet("Run")]
        public string Run()
        {
            return s.Run().ToString();
        }

        [HttpGet("Stop")]
        public string Stop()
        {
            return s.Stop().ToString();
        }
        [HttpGet("ReStart")]
        public void ReStart()
        {
            s.Stop();
            s.Run();
        }
        [HttpGet("GetServiceStatus")]
        public bool GetServiceStatus()
        {
            return s.ServiceIsRun();
        }
        [HttpGet("GetCount")]
        public int GetCount()
        {
            return s.GetConnectCount();
        }
        #endregion

    }
}
