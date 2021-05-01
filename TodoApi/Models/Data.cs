using System;

namespace TodoApi.Models
{
    public class CounterData
    {
        public int LastUpdateInSeconds { get; set; }
        public DateTime LastUpdate { get; set; }
        public int Total { get; set; }
    }

    public class CarState
    {
        // public bool isSending2ABRP { get; set; }
        public bool isOn { get; set; }
        public string chargerstate { get; set; }
        public double chargercurrent { get; set; }
        public bool isOnAuxRecuperation { get; set; }
    }

    public class GlobalSettings
    {
        public string ABRPUrl { get; set; }
        public string ABRP_api_key { get; set; }
        public string ABRP_token { get; set; }
        public string CAR_MODEL { get; set; }
        public double CAR_BATTERY { get; set; }
        public bool DebugMode { get; set; }
        public int TimerSeconds { get; set; }
        public string InfluxDBToken { get; set; }
        public string InfluxDBUser { get; set; }
        public string InfluxDBDataBase { get; set; }
        public string InfluxDBServer { get; set; }
        public string CLOUDMQTT_SERVER { get; set; }
        public int CLOUDMQTT_PORT { get; set; }
        public string CLOUDMQTT_USER { get; set; }
        public string CLOUDMQTT_PASSWORD { get; set; }
    }

    public class tlm
    {
        // {"utc":1596007220,"soc":76,"soh":100,"speed":0,"car_model":"hyundai:kona:19:39:other",
        // "lat":"40.346","lon":"-3.676","alt":"587.8",
        // "ext_temp":29.5,"is_charging":0,"batt_temp":33,"voltage":355.5,"current":0.3,"power":"0.1"}


        // {"is_charging": no, "lat": 40.3458, "lon": -3.67613, "alt": 585, "soc": 70.5, "soh": 100,
        // "speed": , "ext_temp": 25.5, "batt_temp": 27, "voltage": 349.2,"current": 0.1, "power": 0.03492}

        //'ovms/jchm/KonaEV/metric/v/c/charging'
        //'ovms/jchm/KonaEV/metric/v/p/latitude'
        //'ovms/jchm/KonaEV/metric/v/p/longitude'
        //'ovms/jchm/KonaEV/metric/v/p/altitude'
        //'ovms/jchm/KonaEV/metric/v/b/soc'
        //'ovms/jchm/KonaEV/metric/v/b/soh'
        //'ovms/jchm/KonaEV/metric/v/p/speed'
        //'ovms/jchm/KonaEV/metric/v/e/temp'
        //'ovms/jchm/KonaEV/metric/v/b/temp'
        //'ovms/jchm/KonaEV/metric/v/b/voltage'
        //'ovms/jchm/KonaEV/metric/v/b/current'
        //'ovms/jchm/KonaEV/metric/v/b/power'

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


        public void setData(string Topic, string Value)
        {
            switch (Topic)
            {
                //case "ovms/jchm/KonaEV/metric/v/c/charging":
                //    this.is_charging = Value.Equals("yes") ? true : false;
                //    break;
                case "ovms/jchm/KonaEV/metric/v/c/current":
                    Program.carState.chargercurrent = Convert.ToDouble(Value);
                    Program.carState.isOnAuxRecuperation = is_charging && !(Program.carState.chargercurrent > 0);
                    break;
                case "ovms/jchm/KonaEV/metric/v/c/state":
                    Program.carState.chargerstate = Value;
                    is_charging = Value.Equals("charging") || Value.Equals("topoff");
                    break;
                case "ovms/jchm/KonaEV/metric/v/p/latitude":
                    lat = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/p/longitude":
                    lon = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/p/altitude":
                    alt = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/b/soc":
                    soc = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/b/soh":
                    soh = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/p/speed":
                    speed = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/e/temp":
                    ext_temp = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/b/temp":
                    batt_temp = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/b/voltage":
                    voltage = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/b/current":
                    current = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/b/power":
                    power = Convert.ToDouble(Value);
                    break;
                case "ovms/jchm/KonaEV/metric/v/e/on":
                    Program.carState.isOn = Value.Equals("no") ? false : true;
                    break;
                    //case "abrp/status":
                    //    Program.carState.isSending2ABRP = Value.Equals("1") ? true : false;
                    //    break;
            }
        }
    }

    //public class returnTLM : tlm
    //{
    //    public new int soc { get; set; }

    //    public returnTLM(tlm t)
    //    {
    //        this.alt = t.alt;
    //        this.batt_temp = t.batt_temp;
    //        this.car_model = t.car_model;
    //        this.current = t.current;
    //        this.ext_temp = t.ext_temp;
    //        this.is_charging = t.is_charging;
    //        this.lat = t.lat;
    //        this.lon = t.lon;
    //        this.power = t.power;
    //        this.soc = Convert.ToInt32(Math.Floor(t.soc));
    //        this.soh = t.soh;
    //        this.speed = t.speed;
    //        this.utc = t.utc;
    //        this.voltage = t.voltage;
    //    }
    //}
}
