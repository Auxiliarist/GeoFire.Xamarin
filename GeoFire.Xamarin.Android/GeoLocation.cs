/*
 * Firebase GeoFire Java Library
 *
 * Copyright © 2014 Firebase - All Rights Reserved
 * https://www.firebase.com
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this
 * list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binaryform must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY FIREBASE AS IS AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL FIREBASE BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace GeoFire.Xamarin.Android
{
    /// <summary> A wrapper class for location coordinates. </summary>
    public sealed class GeoLocation
    {
        /// <summary> The latitude of this location in the range of [-90, 90] </summary>
        public double Latitude { get; }

        /// <summary> The longitude of this location in the range of [-180, 180] </summary>
        public double Longitude { get; }

        /// <summary> Creates a new GeoLocation with the given latitude and longitude. </summary>
        /// <param name="latitude"> The latitude in the range of [-90, 90] </param>
        /// <param name="longitude"> The longitude in the range of [-180, 180] </param>
        public GeoLocation(double latitude, double longitude)
        {
            if (!CoordinatesValid(latitude, longitude))
                throw new ArgumentException("Not a valid geo location: " + latitude + ", " + longitude);

            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary> Checks if these coordinates are valid geo coordinates. </summary>
        /// <param name="latitude"> The latitude must be in the range [-90, 90] </param>
        /// <param name="longitude"> The longitude must be in the range [-180, 180] </param>
        /// <returns> True if these are valid geo coordinates </returns>
        public static bool CoordinatesValid(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || GetType() != obj.GetType())
                return false;

            GeoLocation that = (GeoLocation)obj;

            if (that.Latitude.CompareTo(Latitude) != 0)
                return false;

            if (that.Longitude.CompareTo(Longitude) != 0)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result;
            long temp;

            temp = BitConverter.DoubleToInt64Bits(Latitude);
            result = (int)(temp ^ ((uint)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(Longitude);
            result = 31 * result + (int)(temp ^ ((uint)temp >> 32));

            return result;
        }

        public override string ToString()
        {
            return "GeoLocation(" + Latitude + ", " + Longitude + ")";
        }
    }
}