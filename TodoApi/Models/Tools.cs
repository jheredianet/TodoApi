﻿using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace TodoApi.Models
{
    public class Tools
    {
        private static string counterFile;

        public static void guardarLog(string Texto)
        {
            using (StreamWriter w = System.IO.File.AppendText(FicheroLog()))
            {
                w.WriteLine("{0} - {1}", DateTime.Now.ToString(), Texto);
                w.Flush();
                w.Close();
            }
        }

        private static string FicheroLog()
        {
            string Dir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "logs");

            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }
            string fichero = Path.Combine(Dir, DateTime.Today.ToString("yyyyMMdd") + ".log");
            if (!System.IO.File.Exists(fichero))
            {
                System.IO.File.Create(fichero).Close();
            }
            return fichero;
        }

        public static ActionResult<string> SendData2OVMS(string command)
        {
            // List of Posible commands

            string resultContent;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Program.AppConfig.OVMSUrl);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("noidsave", ""),
                    new KeyValuePair<string, string>("_mod", "ovms/cmd"),
                    new KeyValuePair<string, string>("_vehicleid", Program.AppConfig.OVMSid),
                    new KeyValuePair<string, string>("_carpass", Program.AppConfig.OVMSpass),
                    new KeyValuePair<string, string>("_cmd", command),
                });
                var response = client.PostAsync("", content);
                resultContent =
                    response.Result.Content.ReadAsStringAsync().Result.Replace("<span>",string.Empty).Replace("</span>", string.Empty).Trim();
                
                if (!response.IsCompletedSuccessfully)
                {
                    resultContent = "Error sending to OVMS: " + resultContent;
                    Models.Tools.guardarLog(resultContent);
                }
                else
                {
                    Models.Tools.guardarLog("Command sent to OVMS: " + command);
                    Models.Tools.guardarLog("Result: " + resultContent);
                    // Parse result (if we get a lot of data, just return a short response)
                    if (resultContent.Contains("bat temp"))
                    {
                        string[] lines = resultContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var newresult = string.Empty;
                        foreach (var line in lines)
                        {
                            if (!line.Contains("="))
                            {
                                newresult += line;
                            }
                        }
                        resultContent = newresult;
                    }
                }

            }
            return resultContent;
        }

        public static DateTime getCounterFileDate()
        {
            InitCounter();
            return System.IO.File.GetLastWriteTime(counterFile);
        }

        public static int AddCount()
        {
            var i = GetCounter();
            i++;
            SetCounter(i);
            return i;
        }

        public static Models.tlm ParseTLM(string tlm)
        {
            Models.tlm myJsonObject = null;
            try
            {
                myJsonObject = JsonConvert.DeserializeObject<Models.tlm>(tlm);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += " | " + ex.InnerException.Message;
                }
                Models.Tools.guardarLog("parseTLM:" + msg);
            }

            return myJsonObject;
        }

        public static string SerializeReturnTLM(returnTLM tlm)
        {
            string newTLM = JsonConvert.SerializeObject(tlm);
            return newTLM;
        }

        private static void SetCounter(int Counter)
        {
            InitCounter();
            TextWriter tw = new StreamWriter(counterFile);
            // Write counter to file
            tw.WriteLine(Counter);


            // close the stream     
            tw.Close();
        }

        public static int GetCounter()
        {
            InitCounter();

            TextReader tr = new StreamReader(counterFile);
            // read file
            var n = tr.ReadLine();
            // close the stream
            tr.Close();

            int Counter;
            if (!int.TryParse(n, out Counter)) Counter = 0;
            return Counter;
        }

        public static GlobalSettings getConfig()
        {
            var configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json");
            GlobalSettings config = null;
            using (StreamReader r = new StreamReader(configFile))
            {
                string json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<GlobalSettings>(json);
            }
            return config;
        }

        private static void InitCounter()
        {
            counterFile = Path.Combine(Path.GetDirectoryName(FicheroLog()), "counter.txt");
            if (!System.IO.File.Exists(counterFile))
            {
                System.IO.File.Create(counterFile).Close();
            }
        }

        public static void SaveDataIntoInfluxDB(tlm objTLM)
        {
            char[] Token = Program.AppConfig.InfluxDBToken.ToCharArray();
            var influxDBClient = InfluxDBClientFactory.CreateV1(Program.AppConfig.InfluxDBServer,
                Program.AppConfig.InfluxDBUser, Token, Program.AppConfig.InfluxDBDataBase, null);

            using (var writeApi = influxDBClient.GetWriteApi())
            {

                // Let's calculate the distance between the previous point.
                var distance = Models.PreviousData.getDistance(objTLM.lat, objTLM.lon);

                // Let's calculate Consumption kWh
                // Car Battery degradation
                var percentUtil = Program.AppConfig.CAR_BATTERY * objTLM.soh / 100;
                var ConsumptionkWh = Models.PreviousData.getDiffSOC(objTLM.soc) * percentUtil / 100;

                // Let's calculate Consumption kWh/100
                // Not Necesary, it's calculated in Grafana
                // var ConsumptionkWh100 = ConsumptionkWh / (distance / 1000) * 100;

                // Let's conver the UTC to datetime
                var timeFromOVMS = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(objTLM.utc * 1000.0);

                //
                // Write by Point
                //
                var point = PointData.Measurement("ovms")
                    .Tag("vehicle", "konaev")
                    .Field("alt", objTLM.alt)
                    .Field("batt_temp", objTLM.batt_temp)
                    .Field("current", objTLM.current)
                    .Field("ext_temp", objTLM.ext_temp)
                    .Field("is_charging", objTLM.is_charging)
                    .Field("lat", objTLM.lat)
                    .Field("lon", objTLM.lon)
                    .Field("power", objTLM.power)
                    .Field("soc", objTLM.soc)
                    .Field("soh", objTLM.soh)
                    .Field("speed", objTLM.speed)
                    .Field("utc", objTLM.utc)
                    .Field("distance", distance)
                    .Field("consumptionkwh", ConsumptionkWh)
                    .Field("voltage", objTLM.voltage)
                    //.Field("Consumptionkwh100", ConsumptionkWh100)
                    .Timestamp(timeFromOVMS, WritePrecision.Ns);

                writeApi.WritePoint(point);

            }
            influxDBClient.Dispose();
        }

        public static int SendData2ABRP(tlm objTLM)
        {
            int Counter = 0;

            // Just Continue if GPS coordinates are different from 0
            if (objTLM.lat == 0 && objTLM.lon == 0 && objTLM.alt == 0)
            {
                // There is no GPS data yet
                return 0;
            }

            // Fill utc and car model
            objTLM.car_model = Program.AppConfig.CAR_MODEL;
            // utc is comming from tlm
            //objTLM.utc = Convert.ToInt32(DateTime.UtcNow.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds / 1000);

            // Reparse TLM
            var returnTLM = new returnTLM(objTLM);
            string stringTLMParameter = SerializeReturnTLM(returnTLM);

            // Send data to ABRP (read from config)
            var URL = Program.AppConfig.ABRPUrl;

            var urljson = URL += "?";
            urljson += "api_key=" + Program.AppConfig.ABRP_api_key;
            urljson += "&";
            urljson += "token=" + Program.AppConfig.ABRP_token;
            urljson += "&";
            urljson += "tlm=" + stringTLMParameter;

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
                        Counter = Models.Tools.AddCount();

                    }
                    if (Program.AppConfig.DebugMode)
                    {
                        Models.Tools.guardarLog(log);
                        // Guardar en log los datos enviados
                        // Models.Tools.guardarLog(stringTLMParameter);
                    }
                }
                // Guardar los datos en InfluxDB
                Models.Tools.SaveDataIntoInfluxDB(objTLM);
            }
            catch (Exception ex)
            {
                string msg = "Source: " + ex.Source + " | " + ex.Message;
                if (ex.InnerException != null)
                {
                    msg += " | " + ex.InnerException.Message;
                }
                Models.Tools.guardarLog(msg);
            }

            return Counter;
        }

        public static void SendData2HA(string Sensor, string jsonInString)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Program.AppConfig.HomeAssistantToken);
            string uri = $"{Program.AppConfig.HomeAssistantServer}/api/states/{Sensor}";
            var response = client.PostAsync(uri, new StringContent(jsonInString, Encoding.UTF8, "application/json")).Result;
            if (!response.IsSuccessStatusCode)
            {
                //if (Program.AppConfig.DebugMode)
                //{
                Models.Tools.guardarLog("Error sending to HA: " + response.ToString());
                //}
            }
        }


        //public static async System.Threading.Tasks.Task ReadDataFromInfluxDBAsync(tlm objTLM)
        //{
        //    char[] Token = Program.AppConfig.InfluxDBToken.ToCharArray();
        //    var influxDBClient = InfluxDBClientFactory.CreateV1(Program.AppConfig.InfluxDBServer,
        //        Program.AppConfig.InfluxDBUser, Token, Program.AppConfig.InfluxDBDataBase, null);

        //    //
        //    // Query data
        //    // 

        //    var flux = "from(bucket:\"homeassistant/autogen\") " +
        //        "|> range(start: -48h) " +
        //        "|> filter(fn: (r) => r._measurement == \"ovms\" and (r._field == \"lat\" or r._field == \"lon\"))";

        //    var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(flux);
        //    fluxTables.ForEach(fluxTable =>
        //    {
        //        var fluxRecords = fluxTable.Records;
        //        fluxRecords.ForEach(fluxRecord =>
        //        {
        //            Console.WriteLine($"{fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
        //        });
        //    });

        //    influxDBClient.Dispose();

        //}
    }
}
