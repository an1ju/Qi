﻿using Microsoft.AspNetCore.Mvc;
using Qi.NetFly.Core;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Qi.NetFly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetFlyController : ControllerBase
    {
        private readonly Service s; // 服务就在这里啦

        public NetFlyController(Qi.NetFly.Core.Service s)
        {
            this.s = s;
        }
        // GET: api/<NetFlyController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Yes", "You can fly on net.", s.ServiceIsRun().ToString() };
        }

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
    }
}
