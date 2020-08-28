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
        //public ActionResult<string> Get(int id, string tlm, string token, string api_key)
        public ActionResult<string> Get(int id, string tlm, string isActive)
        {
            bool Estado = isActive == "on";

            if (Program.AppConfig.DebugMode)
            {
                Models.Tools.guardarLog("Actual isSending2ABRP is: " + Program.carState.isSending2ABRP.ToString()
                    + " | Received: isActive: " + isActive);
            }

            if (id != 0)
            {
                return BadRequest("ID. not valid");
            }

            // Send Data 
            int Counter = Models.Tools.SendData2ABRP(tlm);


            if (Program.carState.isSending2ABRP != Estado)
            {
                // Call HomeAssistant only if its different
                string jsonData = "{" + string.Format("{1}state{1}: {0}", Estado ? 1 : 0, '"') + "}";
                Models.Tools.sendData2HA("sensor.abrp", jsonData);
                Program.carState.isSending2ABRP = Estado;
            }



            return Ok(Counter);
        }
    }
}
