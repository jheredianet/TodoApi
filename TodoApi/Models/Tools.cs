using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

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

        public static DateTime getCounterFileDate()
        {
            initCounter();
            return System.IO.File.GetLastWriteTime(counterFile);
        }

        public static int addCount()
        {
            var i = getCounter();
            i++;
            setCounter(i);
            return i;
        }

        public static Models.tlm parseTLM(string tlm)
        {
            var myJsonObject = JsonConvert.DeserializeObject<Models.tlm>(tlm);
            return myJsonObject;
        }

        public static string serializeReturnTLM(Models.returnTLM tlm)
        {
            var newTLM = JsonConvert.SerializeObject(tlm);
            return newTLM;
        }


        private static void setCounter(int Counter)
        {
            initCounter();
            TextWriter tw = new StreamWriter(counterFile);
            // Write counter to file
            tw.WriteLine(Counter);


            // close the stream     
            tw.Close();
        }

        public static int getCounter()
        {
            initCounter();

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

        private static void initCounter()
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
            var influxDBClient = InfluxDBClientFactory.CreateV1(Program.AppConfig.InfluxDBServer, Program.AppConfig.InfluxDBUser, Token,Program.AppConfig.InfluxDBDataBase, null);

            using (var writeApi = influxDBClient.GetWriteApi())
            {
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
                    .Timestamp(DateTime.UtcNow.AddSeconds(-5), WritePrecision.Ns);

                writeApi.WritePoint(point);

            }
            influxDBClient.Dispose();
        }
    }
}
