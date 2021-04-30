using System;
namespace TodoApi.Models
{
    public class Attributes
    {
        public string unit_of_measurement { get; set; }
        public string friendly_name { get; set; }
        public string icon { get; set; }
    }

    public class Context
    {
        public string id { get; set; }
        public object parent_id { get; set; }
        public object user_id { get; set; }
    }

    public class Sensor
    {
        public string entity_id { get; set; }
        public string state { get; set; }
        public Attributes attributes { get; set; }
        public DateTime last_changed { get; set; }
        public DateTime last_updated { get; set; }
        public Context context { get; set; }
    }

    public class Vehicle
    {
        public double BatteryCapacity { get { return Program.AppConfig.CAR_BATTERY; } }
        public double SOH { get; set; }
        public double SOC { get; set; }
        public double BatteryPercentUtil { get { return BatteryCapacity * SOH / 100; } }
    }
}