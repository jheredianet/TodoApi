using System;

namespace TodoApi.Models
{
    public class CounterData
    {
        public int LastUpdateInSeconds { get; set; }
        public DateTime LastUpdate { get; set; }
        public int Total { get; set; }
    }

    public class GlobalSettings
    {
        public string ABRPUrl { get; set; }
        public bool DebugMode { get; set; }
        public string InfluxDBToken { get; set; }
        public string InfluxDBUser { get; set; }
        public string InfluxDBDataBase { get; set; }
        public string InfluxDBServer { get; set; }
    }

    public class tlm
    {
        // {"utc":1596007220,"soc":76,"soh":100,"speed":0,"car_model":"hyundai:kona:19:39:other",
        // "lat":"40.346","lon":"-3.676","alt":"587.8",
        // "ext_temp":29.5,"is_charging":0,"batt_temp":33,"voltage":355.5,"current":0.3,"power":"0.1"}

        public int utc { get; set; }
        public string car_model { get; set; }
        public bool is_charging { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public double alt { get; set; }
        public double soc { get; set; }
        public double soh { get; set; }
        public double speed { get; set; }
        public double ext_temp { get; set; }
        public double batt_temp { get; set; }
        public double voltage { get; set; }
        public double current { get; set; }
        public double power { get; set; }
    }

    public class returnTLM: tlm
    {
        public new int soc { get; set; }

        public returnTLM(tlm t)
        {
            this.alt = t.alt;
            this.batt_temp = t.batt_temp;
            this.car_model = t.car_model;
            this.current = t.current;
            this.ext_temp = t.ext_temp;
            this.is_charging = t.is_charging;
            this.lat = t.lat;
            this.lon = t.lon;
            this.power = t.power;
            this.soc = Convert.ToInt32(Math.Floor(t.soc));
            this.soh = t.soh;
            this.speed = t.speed;
            this.utc = t.utc;
            this.voltage = t.voltage;
        }       
    }
}
