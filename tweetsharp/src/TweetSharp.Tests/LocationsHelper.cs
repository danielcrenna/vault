using System;

namespace TweetSharp.Tests
{
    internal static class TweetLocationsHelper
    {
        public static bool IsWithinSearchRadius(this TwitterStatus s, TwitterGeoLocationSearch searchArea)
        {
            if (s.Location == null)
                return false;

            if (searchArea == null)
                return false;

            //  (x-center_x)^2 + (y - center_y)^2 < radius^2
            var x = s.Location.Coordinates.Latitude;
            var centerX = searchArea.Coordinates.Latitude;

            var y = s.Location.Coordinates.Longitude;
            var centerY = searchArea.Coordinates.Longitude;

            var radius = searchArea.Radius;

            return Math.Pow((x - centerX), 2) + Math.Pow((y - centerY), 2) < Math.Pow(radius, 2);
        }
    }
}