using System;
using GeoCoordinatePortable;

namespace TodoApi.Models
{
    public class PreviousData
    {
        private static GeoCoordinate lastLocation;
        private static double lastSOC;

        public static double getDistance(double lat, double lng)
        {
            double distanceBetween = 0.001; // 10 Centimos de precisión en caso de que no haya localización previa 
            GeoCoordinate currentLocation = new GeoCoordinate(lat, lng);
            if (lastLocation != null)
            {
                distanceBetween = lastLocation.GetDistanceTo(currentLocation);

            }
            lastLocation = currentLocation;
            return distanceBetween;
        }

        public static double getDiffSOC(double soc)
        {
            double diff = 0.0001;
            if (lastSOC > 0)
            {
                diff = Math.Abs(soc - lastSOC);
            }
            lastSOC = soc;
            return diff;
        }
    }
}
