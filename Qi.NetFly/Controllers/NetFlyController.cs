using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Qi.NetFly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetFlyController : ControllerBase
    {
        // GET: api/<NetFlyController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Yes", "You can fly on net." };
        }

    }
}
