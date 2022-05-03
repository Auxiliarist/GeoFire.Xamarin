using System.Linq;

namespace GeoFire.Xamarin.Android.Util
{
    public static class Base32Utils
    {
        /// <summary>
        /// Number of bits per base 32 character
        /// </summary>
        public const int BITS_PER_BASE32_CHAR = 5;

        private const string BASE32_CHARS = "0123456789bcdefghjkmnpqrstuvwxyz";

        public static char ValueToBase32Char(int value)
        {
            if (value < 0 || value >= BASE32_CHARS.Length)
                throw new System.ArgumentException("Not a valid base32 value: " + value);

            return BASE32_CHARS.ElementAt(value);
        }

        public static int Base32CharToValue(char base32char)
        {
            int value = BASE32_CHARS.IndexOf(base32char);
            if (value == -1)
                throw new System.ArgumentException("Not a valid base32 char: " + base32char);
            else
                return value;
        }

        public static bool IsValidBase32String(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, "^[" + BASE32_CHARS + "]*$");
            //return str.Matches("^[" + BASE32_CHARS + "]*$");
        }
    }
}