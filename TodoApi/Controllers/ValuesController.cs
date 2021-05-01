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

            return string.Format("State of Charge: {0}% - Last update: {1} {2} ago.",
                Models.PreviousData.getSOC(),
                Counter.LastUpdateInSeconds < 60 ? Counter.LastUpdateInSeconds : Convert.ToInt32(Counter.LastUpdateInSeconds / 60),
                Counter.LastUpdateInSeconds < 60 ? "seconds" : "minutes");
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
            int Counter = Models.Tools.SaveAndSendData(tlm, true);

            // Call HomeAssistant only if Estado is different
            if (Program.carState.ShouldSend2ABRP != Estado)
            {
                //msg.AppendLine(Models.Tools.SerializeReturnTLM(objTLM));
                // In order to see how we send to ABRP - Reparse TLM
                //string stringTLMParameter = serializeReturnTLM(new returnTLM(objTLM));
                Models.Tools.updateABRPSensorOnHA(Estado);
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
