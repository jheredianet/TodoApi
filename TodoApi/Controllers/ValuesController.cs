using System;
using System.Collections.Generic;
using System.Text;
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

        // Post api/values/sendcommand
        [HttpPost("sendCommand", Name = "sendCommand")]
        
        // public ActionResult<Models.CounterData> GetCounter()
        public ActionResult<string> sendCommand(string Command)
        {
            return Models.Tools.SendData2OVMS(Command);
        }

        // GET api/values/getcounter
        [HttpGet("GetCounter", Name = "GetCounter")]
        // public ActionResult<Models.CounterData> GetCounter()
        public ActionResult<string> GetCounter()
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

            return string.Format("Last update: {0} {1} ago. Total records: {2}. State of Charge: {3}%",
                Counter.LastUpdateInSeconds < 60 ? Counter.LastUpdateInSeconds: Convert.ToInt32(Counter.LastUpdateInSeconds/60),
                Counter.LastUpdateInSeconds < 60 ? "seconds" : "minutes",
                Counter.Total,
                Models.PreviousData.getSOC());
        }

        // GET api/values/0
        [HttpGet("{id}")]
        //public ActionResult<string> Get(int id, string tlm, string token, string api_key)
        public ActionResult<string> Get(int id, string tlm, string isActive)
        {
            bool Estado = isActive == "on";

            if (id != 0)
            {
                return BadRequest("ID. not valid");
            }

            // Serialize JSON
            var objTLM = Models.Tools.parseTLM(tlm);

            if (objTLM == null)
            {
                return BadRequest("Cannot parse received TLM");
            }

            // Send Data 
            int Counter = Models.Tools.SendData2ABRP(objTLM);


            if (Program.carState.isSending2ABRP != Estado)
            {
                // Call HomeAssistant only if its different
                string jsonData = "{" + string.Format("{1}state{1}: {0}", Estado ? 1 : 0, '"') + "}";
                Models.Tools.sendData2HA("sensor.abrp", jsonData);

                // Models.Tools.guardarLog("Actual isSending2ABRP is: " + Program.carState.isSending2ABRP.ToString()
                //     + " | Received: isActive: " + isActive);

                StringBuilder msg = new StringBuilder();
                msg.Append(Estado ? "Start " : "Stop ");
                msg.AppendLine("Sending Data to ABRP.");
                msg.AppendLine(Models.Tools.serializeReturnTLM(objTLM));
                Models.Tools.guardarLog(msg.ToString());

                Program.carState.isSending2ABRP = Estado;
            }

            // Reset Previous Data when is stopping sending
            if (!Estado)
            {
                Models.PreviousData.resetData();
            }

            return Ok(Counter);
        }
    }
}
