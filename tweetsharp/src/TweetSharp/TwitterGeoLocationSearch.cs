namespace TweetSharp
{
    /// <summary>
    ///     Used for Twitter searches where it takes a geocode paramater
    /// </summary>
    public class TwitterGeoLocationSearch : TwitterGeoLocation
    {
        /// <summary>
        /// Radius type in Miles or Kilometers
        /// </summary>
        public enum RadiusType
        {
            Mi,
            Km
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterGeoLocationSearch"/> struct.
        /// </summary>
        public TwitterGeoLocationSearch()
        {
        }

        /// <summary>
        /// /// <summary>
        /// Initializes a new instance of the <see cref="TwitterGeoLocation"/> struct.
        /// </summary>
        /// </summary>
        /// <param name="latitutde">The latitude of search location.</param>
        /// <param name="longitude">The longitude of search location.</param>
        /// <param name="radius">The radius of search location.</param>
        /// <param name="unitOfMeasurement">The unit of measurement (Mi or Km).</param>
        public TwitterGeoLocationSearch(double latitutde, double longitude, int radius, RadiusType unitOfMeasurement)
            : base(latitutde, longitude)
        {
            Radius = radius;
            UnitOfMeasurement = unitOfMeasurement;
        }

        /// <summary>
        ///     Radius in specified <see cref="RadiusType" />
        /// </summary>
        /// <seealso cref="UnitOfMeasurement" />
        public int Radius { get; set; }

        public RadiusType UnitOfMeasurement { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}{3}", Coordinates.Latitude, Coordinates.Longitude, Radius,
                                 UnitOfMeasurement.ToString().ToLower());
        }
    }
}