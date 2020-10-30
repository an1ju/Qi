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
    public class AboutController : ControllerBase
    {
        // GET: api/<AboutController>
        [HttpGet]
        public IEnumerable<NewLife.Qi.NetFly.Model.Author> Get()
        {
            return new NewLife.Qi.NetFly.Model.Author[] { new NewLife.Qi.NetFly.Model.Author() };
        }


    }
}
