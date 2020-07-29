using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;

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

            // Solo para depuración
            // Models.Tools.guardarLog(tlm + " | " + token + " | " + api_key);

            // Serialize JSON
            var objTLM = Models.Tools.parseTLM(tlm);

            // Send data to ABRP
            var URL = "http://api.iternio.com/1/tlm/send";

            var urljson = URL += "?";
            urljson += "api_key=" + api_key;
            urljson += "&";
            urljson += "token=" + token;
            urljson += "&";
            urljson += "tlm=" + tlm;

            int Counter = 0;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(urljson);
                    //HTTP GET
                    var responseTask = client.GetAsync("");
                    responseTask.Wait();

                    var result = responseTask.Result;
                    var log = result.Version + " | " + result.ReasonPhrase + " | " + result.RequestMessage;
                    if (result.IsSuccessStatusCode)
                    {
                        // Ha ido bien
                        Counter = Models.Tools.addCount();

                    }
                    Models.Tools.guardarLog(log);
                }

                // Guardar los datos en InfluxDB
                Models.Tools.SaveDataIntoInfluxDB(objTLM);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += " | " + ex.InnerException.Message;
                }

                Models.Tools.guardarLog(msg);
                return BadRequest(ex);
            }

            return Ok(Counter);

        }


    }
}
