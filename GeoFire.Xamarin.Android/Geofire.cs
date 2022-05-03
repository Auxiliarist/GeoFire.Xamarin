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

using Android.Runtime;
using Firebase.Database;
using GeoFire.Xamarin.Android.Core;
using GeoFire.Xamarin.Android.Util;
using Java.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GeoFire.Xamarin.Android
{
    /// <summary> A GeoFire instance is used to store geo location data in Firebase. </summary>
    public class Geofire
    {
        /// <summary>
        ///    A listener that can be used to be notified about a successful write or an error on writing.
        /// </summary>
        public interface ICompletionListener
        {
            /// <summary>
            ///    Called once a location was successfully saved on the server or an error occurred. 
            ///    <para>
            ///       On success, the parameter error will be null; in case of an error, the error
            ///       will be passed to this method.
            ///    </para>
            /// </summary>
            /// <param name="key"> The key whose location was saved </param>
            /// <param name="error"> The error or null if no error occurred </param>
            void OnComplete(string key, DatabaseError error);
        }

        private IEventRaiser eventRaiser;

        /// <summary> The Firebase reference this GeoFire instance uses </summary>
        public DatabaseReference DatabaseReference { get; }

        public DatabaseReference DatabaseReferenceForKey(string key)
        {
            return DatabaseReference.Child(key);
        }

        /// <summary> Creates a new GeoFire instance at the given Firebase reference. </summary>
        /// <param name="databaseReference">
        ///    The Firebase reference this GeoFire instance uses
        /// </param>
        public Geofire(DatabaseReference databaseReference)
        {
            DatabaseReference = databaseReference;
            IEventRaiser eventRaiser;
            try
            {
                eventRaiser = new AndroidEventRaiser();
            }
            catch (Exception)
            {
                // We're not on Android, use the ThreadEventRaiser
                eventRaiser = new ThreadEventRaiser();
            }

            this.eventRaiser = eventRaiser;
        }

        /// <summary> Sets the location for a given key. </summary>
        /// <param name="key"> The key to save the location for </param>
        /// <param name="location"> The location of this key </param>
        public void SetLocation(string key, GeoLocation location)
        {
            SetLocation(key, location, null);
        }

        /// <summary> Sets the location for a given key. </summary>
        /// <param name="key"> The key to save the location for </param>
        /// <param name="location"> The location of this key </param>
        /// <param name="listener">
        ///    A listener that is called once the location was successfully saved on the server or an
        ///    error occurred
        /// </param>
        public void SetLocation(string key, GeoLocation location, ICompletionListener listener)
        {
            if (key == null)
                throw new NullReferenceException();

            var keyRef = DatabaseReferenceForKey(key);
            var geoHash = new GeoHash(location);

            IMap map = new HashMap();
            map.Put("g", geoHash.Hash);
            map.Put("l", (Java.Lang.Object)Arrays.AsList(location.Latitude, location.Longitude));

            if (listener != null)
                keyRef.SetValue((Java.Lang.Object)map, geoHash.Hash, new CompletionListener(key, listener));
            else
                keyRef.SetValue((Java.Lang.Object)map, geoHash.Hash);
        }

        /// <summary> Sets the location for a given key. </summary>
        /// <param name="key"> The key to save the location for </param>
        /// <param name="location"> The location of this key </param>
        public async Task SetLocationAsync(string key, GeoLocation location)
        {
            if (key == null)
                throw new NullReferenceException();

            var keyRef = DatabaseReferenceForKey(key);
            var geoHash = new GeoHash(location);

            IMap map = new HashMap();
            map.Put("g", geoHash.Hash);
            map.Put("l", (Java.Lang.Object)Arrays.AsList(location.Latitude, location.Longitude));

            await keyRef.SetValueAsync((Java.Lang.Object)map, geoHash.Hash);
        }

        /// <summary>
        /// Sets a spot location for a given key.
        /// </summary>
        /// <param name="key"> The key to save the location for </param>
        /// <param name="address"> Address string for the location </param>
        /// <param name="accuracy"> Accuracy of the location </param>
        /// <param name="location"> The location of this key </param>
        /// <returns></returns>
        public async Task SetSpotAsync(string key, string address, double accuracy, GeoLocation location)
        {
            if (key == null)
                throw new NullReferenceException();

            var keyRef = DatabaseReferenceForKey(key);
            var geoHash = new GeoHash(location);

            IMap map = new HashMap();
         
            map.Put("g", geoHash.Hash);
            map.Put("a", address);
            map.Put("l", (Java.Lang.Object)Arrays.AsList(location.Latitude, location.Longitude));
            map.Put("p", accuracy);
            map.Put("t", (Java.Lang.Object)ServerValue.Timestamp);

            await keyRef.SetValueAsync((Java.Lang.Object)map, geoHash.Hash);
        }

        /// <summary> Removes the location for a key from this GeoFire. </summary>
        /// <param name="key"> The key to remove from this GeoFire </param>
        public void RemoveLocation(string key)
        {
            RemoveLocation(key, null);
        }

        /// <summary> Removes the location for a key from this GeoFire. </summary>
        /// <param name="key"> The key to remove from this GeoFire </param>
        /// <param name="listener">
        ///    A completion listener that is called once the location is successfully removed from
        ///    the server or an error occurred
        /// </param>
        public void RemoveLocation(string key, ICompletionListener listener)
        {
            if (key == null)
                throw new NullReferenceException();

            var keyRef = DatabaseReferenceForKey(key);

            if (listener != null)
                keyRef.SetValue(null, listener: new CompletionListener(key, listener));
            else
                keyRef.SetValue(null);
        }

        /// <summary> Removes the location for a key from this GeoFire. </summary>
        /// <param name="key"> The key to remove from this GeoFire </param>
        public async Task RemoveLocationAsync(string key)
        {
            if (key == null)
                throw new NullReferenceException();

            var keyRef = DatabaseReferenceForKey(key);

            await keyRef.SetValueAsync(null);
        }

        public static GeoLocation GetLocationValue(DataSnapshot snapshot)
        {
            try
            {
                var location = snapshot?.Child("l")?.Value?.JavaCast<ArrayList>().ToEnumerable<double>();

                double latitude = location.ElementAt(0);
                double longitude = location.ElementAt(1);

                if (location.Count() == 2 && GeoLocation.CoordinatesValid(latitude, longitude))
                    return new GeoLocation(latitude, longitude);
                else
                    return null;
            }
            catch (System.ArgumentNullException) // For Deleting Location
            {
                return null;
            }
            catch (Java.Lang.ClassCastException)
            {
                return null;
            }
        }

        /// <summary>
        ///    Gets the current location for a key and calls the callback with the current value.
        /// </summary>
        /// <param name="key"> The key whose location to get </param>
        /// <param name="callback"> The callback that is called once the location is retrieved </param>
        public void GetLocation(string key, ILocationCallback callback)
        {
            var keyRef = DatabaseReferenceForKey(key);
            LocationValueEventListener valueEventListener = new LocationValueEventListener(callback);
            keyRef.AddListenerForSingleValueEvent(valueEventListener);
        }

        /// <summary>
        ///    Returns a new Query object centered at the given location and with the given radius.
        /// </summary>
        /// <param name="center"> The center of the query </param>
        /// <param name="radius">
        ///    The radius of the query, in kilometers. The maximum radius that is supported is about 8587km.
        /// </param>
        /// <returns></returns>
        public GeoQuery QueryAtLocation(GeoLocation center, double radius)
        {
            return new GeoQuery(this, center, GeoUtils.CapRadius(radius));
        }

        public void RaiseEvent(Action r)
        {
            eventRaiser.RaiseEvent(r);
        }

        private class CompletionListener : Java.Lang.Object, DatabaseReference.ICompletionListener
        {
            private string key;
            private ICompletionListener listener;

            public CompletionListener(string key, ICompletionListener listener)
            {
                this.key = key;
                this.listener = listener;
            }

            public void OnComplete(DatabaseError error, DatabaseReference @ref)
            {
                listener.OnComplete(key, error);
            }
        }

        /// <summary> A small wrapper class to forward any events to the LocationEventListener. </summary>
        private class LocationValueEventListener : Java.Lang.Object, IValueEventListener
        {
            private ILocationCallback callback;

            public LocationValueEventListener(ILocationCallback callback)
            {
                this.callback = callback;
            }

            public void OnCancelled(DatabaseError error)
            {
                callback.OnCancelled(error);
            }

            public void OnDataChange(DataSnapshot snapshot)
            {
                if (snapshot.Value == null)
                    callback.OnLocationResult(snapshot.Key, null);
                else
                {
                    GeoLocation location = GetLocationValue(snapshot);
                    if (location != null)
                        callback.OnLocationResult(snapshot.Key, location);
                    else
                    {
                        string message = "GeoFire data has invalid format: " + snapshot.Value.ToString();
                        callback.OnCancelled(DatabaseError.FromException(new Java.Lang.Throwable(message)));
                    }
                }
            }
        }
    }
}