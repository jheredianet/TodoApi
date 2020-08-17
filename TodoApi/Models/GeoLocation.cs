using System;
using GeoCoordinatePortable;

namespace TodoApi.Models
{
    public class GeoLocation
    {
        public static GeoCoordinate lastLocation;

        public static double getDistance(double lat, double lng)
        {
            double distanceBetween = 0;
            GeoCoordinate currentLocation = new GeoCoordinate(lat, lng);
            if (lastLocation != null)
            {
                distanceBetween = lastLocation.GetDistanceTo(currentLocation);

            }
            lastLocation = currentLocation;
            return distanceBetween;
        }
    }
}
