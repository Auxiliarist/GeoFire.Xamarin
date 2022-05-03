namespace GeoFire.Xamarin.Android.Util
{
    public static class Constants
    {
        /// <summary> Length of a degree latitude at the equator </summary>
        public const double METERS_PER_DEGREE_LATITUDE = 110574;

        /// <summary> The equatorial circumference of the earth in meters </summary>
        public const double EARTH_MERIDIONAL_CIRCUMFERENCE = 40007860;

        /// <summary> The equatorial radius of the earth in meters </summary>
        public const double EARTH_EQ_RADIUS = 6378137;

        /// <summary> The meridional radius of the earth in meters </summary>
        public const double EARTH_POLAR_RADIUS = 6357852.3;

        /* The following value assumes a polar radius of
         * r_p = 6356752.3
         * and an equatorial radius of
         * r_e = 6378137
         * The value is calculated as e2 == (r_e^2 - r_p^2)/(r_e^2)
         * Use exact value to avoid rounding errors
         */
        public const double EARTH_E2 = 0.00669447819799;

        /// <summary> Cutoff for floating point calculations </summary>
        public const double EPSILON = 1e-12;
    }
}