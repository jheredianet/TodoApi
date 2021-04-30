using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { DateTime.Now.ToString(), Environment.MachineName };
        }

        // GET api/values/getcounter
        [HttpGet("GetCounter", Name = "GetCounter")]
        public ActionResult<Models.CounterData> GetCounter()
        {
            Models.CounterData Counter = new Models.CounterData();

            // Read File Date
            Counter.LastUpdate = Models.Tools.getCounterFileDate();
            // Read Counter
            Counter.Total = Models.Tools.getCounter();

            TimeSpan Interval = DateTime.Now - Counter.LastUpdate;
            Counter.LastUpdateInSeconds = Convert.ToInt32(Interval.TotalSeconds);

            // Solo para depuración
            //if (Program.AppConfig.DebugMode)
            //{
            //    Models.Tools.guardarLog(DateTime.Now.ToString() + " - " + Environment.MachineName);
            //}
            return Counter;
        }

        // GET api/values/0
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id, string tlm, string token, string api_key)
        {
            if (id != 0)
            {
                return BadRequest("ID. not valid");
            }
            int Counter = Models.Tools.SaveAndSendData(tlm);
            return Ok(Counter);

        }
    }
}
