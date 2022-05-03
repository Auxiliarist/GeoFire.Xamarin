using GeoFire.Xamarin.Android.Util;

namespace GeoFire.Xamarin.Android.Core
{
    public class GeoHash
    {
        public string Hash { get; }

        /// <summary> The default precision of a geohash </summary>
        private const int DEFAULT_PRECISION = 10;

        /// <summary> The maximal precision of a geohash </summary>
        public const int MAX_PRECISION = 22;

        /// <summary> The maximal number of bits precision for a geohash </summary>
        public const int MAX_PRECISION_BITS = MAX_PRECISION * Base32Utils.BITS_PER_BASE32_CHAR;

        public GeoHash(double latitude, double longitude) : this(latitude, longitude, DEFAULT_PRECISION)
        { }

        public GeoHash(GeoLocation location) : this(location.Latitude, location.Longitude, DEFAULT_PRECISION)
        { }

        public GeoHash(double latitude, double longitude, int precision)
        {
            if (precision < 1)
                throw new System.ArgumentException("Precision of GeoHash must be larger than zero!");

            if (precision > MAX_PRECISION)
                throw new System.ArgumentException("Precision of a GeoHash must be less than " + (MAX_PRECISION + 1) + "!");

            if (!GeoLocation.CoordinatesValid(latitude, longitude))
                throw new System.ArgumentException(Java.Lang.String.Format(Java.Util.Locale.Us, "Not valid location coordinates: [%f, %f]", latitude, longitude));

            double[] longitudeRange = { -180, 180 };
            double[] latitudeRange = { -90, 90 };

            char[] buffer = new char[precision];

            for (int i = 0; i < precision; i++)
            {
                int hashValue = 0;

                for (int j = 0; j < Base32Utils.BITS_PER_BASE32_CHAR; j++)
                {
                    bool even = (((i * Base32Utils.BITS_PER_BASE32_CHAR) + j) % 2) == 0;
                    double val = even ? longitude : latitude;
                    double[] range = even ? longitudeRange : latitudeRange;
                    double mid = (range[0] + range[1]) / 2;

                    if (val > mid)
                    {
                        hashValue = (hashValue << 1) + 1;
                        range[0] = mid;
                    }
                    else
                    {
                        hashValue = hashValue << 1;
                        range[1] = mid;
                    }
                }

                buffer[i] = Base32Utils.ValueToBase32Char(hashValue);
            }

            Hash = new string(buffer);
        }

        public GeoHash(string hash)
        {
            if (hash.Length == 0 || !Base32Utils.IsValidBase32String(hash))
                throw new System.ArgumentException("Not a valid geoHash: " + hash);

            Hash = hash;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || this.GetType() != obj.GetType())
                return false;

            GeoHash other = (GeoHash)obj;

            return Hash.Equals(other.Hash);
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        public override string ToString()
        {
            return "GeoHash{" + "geoHash='" + Hash + '\'' + '}';
        }
    }
}