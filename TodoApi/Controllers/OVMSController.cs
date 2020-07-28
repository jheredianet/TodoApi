using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OVMSController : Controller
    {
        // GET: OVMS
        [HttpGet]
        public ActionResult<IEnumerable<string>> Index()
        {
            return new string[] { DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString() };
        }
    }
}