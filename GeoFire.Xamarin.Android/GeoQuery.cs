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

using Firebase.Database;
using GeoFire.Xamarin.Android.Core;
using GeoFire.Xamarin.Android.Util;
using System.Collections.Generic;

namespace GeoFire.Xamarin.Android
{
    /// <summary>
    ///    A GeoQuery object can be used for geo queries in a given circle. The GeoQuery class is
    ///    thread safe.
    /// </summary>
    public class GeoQuery
    {
        private const int KILOMETER_TO_METER = 1000;
        private Geofire geoFire;
        private GeoLocation center;
        private double radius;
        private object childLock = new object();

        private IList<IGeoQueryDataEventListener> eventListeners = new List<IGeoQueryDataEventListener>();
        private IList<GeoHashQuery> outstandingQueries = new List<GeoHashQuery>();
        private IList<GeoHashQuery> queries;
        private IDictionary<GeoHashQuery, Query> firebaseQueries = new Dictionary<GeoHashQuery, Query>();
        private IDictionary<string, LocationInfo> locationInfos = new Dictionary<string, LocationInfo>();

        private ChildEventListener childEventListener;

        public GeoLocation Center { get => center; set { center = value; if (HasListeners()) SetupQueries(); } }
        public double Radius { get => radius / KILOMETER_TO_METER; set { radius = GeoUtils.CapRadius(value); if (HasListeners()) SetupQueries(); } }

        /// <summary>
        ///    Creates a new GeoQuery object centered at the given location and with the given radius.
        /// </summary>
        /// <param name="geoFire"> The GeoFire object this GeoQuery uses </param>
        /// <param name="center"> The center of this query </param>
        /// <param name="radius">
        ///    The radius of the query, in kilometers. The maximum radius that is supported is about 8587km.
        /// </param>
        public GeoQuery(Geofire geoFire, GeoLocation center, double radius)
        {
            this.geoFire = geoFire;
            this.center = center;
            this.radius = radius * KILOMETER_TO_METER; // Convert from kilometers to meters.

            childEventListener = new ChildEventListener(this);
        }

        private bool LocationIsInQuery(GeoLocation location)
        {
            return GeoUtils.Distance(location, center) <= radius;
        }

        private void UpdateLocationInfo(DataSnapshot snapshot, GeoLocation location)
        {
            string key = snapshot.Key;
            locationInfos.TryGetValue(key, out LocationInfo oldInfo);
            bool isNew = oldInfo == null;
            bool changedLocation = oldInfo != null && !oldInfo.location.Equals(location);
            bool wasInQuery = oldInfo != null && oldInfo.inGeoQuery;

            bool isInQuery = LocationIsInQuery(location);

            if ((isNew || !wasInQuery) && isInQuery)
            {
                foreach (var listener in eventListeners)
                {
                    geoFire.RaiseEvent(() =>
                    {
                        listener.OnDataEntered(snapshot, location);
                    });
                }
            }
            else if (!isNew && isInQuery)
            {
                foreach (var listener in eventListeners)
                {
                    geoFire.RaiseEvent(() =>
                    {
                        if (changedLocation)
                            listener.OnDataMoved(snapshot, location);

                        listener.OnDataChanged(snapshot, location);
                    });
                }
            }
            else if (wasInQuery && !isInQuery)
            {
                foreach (var listener in eventListeners)
                {
                    geoFire.RaiseEvent(() =>
                    {
                        listener.OnDataExited(snapshot);
                    });
                }
            }

            var newInfo = new LocationInfo(location, LocationIsInQuery(location), snapshot);

            // If the key is already in the dictionary, replace with new value
            locationInfos[key] = newInfo;
        }

        private bool GeoHashQueriesContainGeoHash(GeoHash geoHash)
        {
            if (queries == null)
                return false;

            foreach (var query in queries)
            {
                if (query.ContainsGeoHash(geoHash))
                    return true;
            }

            return false;
        }

        private void Reset()
        {
            foreach (var entry in firebaseQueries.Values)
            {
                entry.RemoveEventListener(childEventListener);
            }

            outstandingQueries.Clear();
            firebaseQueries.Clear();
            locationInfos.Clear();
            queries = null;
        }

        private bool HasListeners()
        {
            return !(eventListeners.Count == 0);
        }

        private bool CanFireReady()
        {
            return (outstandingQueries.Count == 0);
        }

        private void CheckAndFireReady()
        {
            if (CanFireReady())
            {
                foreach (var listener in eventListeners)
                    geoFire.RaiseEvent(() =>
                    {
                        listener.OnGeoQueryReady();
                    });
            }
        }

        private void AddValueToReadyListener(Query firebase, GeoHashQuery query)
        {
            firebase.AddListenerForSingleValueEvent(new AddValueListener(this, query));
        }

        private void SetupQueries()
        {
            IList<GeoHashQuery> oldQueries = queries ?? new List<GeoHashQuery>();
            IList<GeoHashQuery> newQueries = new List<GeoHashQuery>(GeoHashQuery.QueriesAtLocation(center, radius));
            queries = newQueries;

            foreach (var query in oldQueries)
            {
                if (!newQueries.Contains(query))
                {
                    firebaseQueries[query].RemoveEventListener(childEventListener);
                    firebaseQueries.Remove(query);
                    outstandingQueries.Remove(query);
                }
            }

            foreach (var query in newQueries)
            {
                if (!oldQueries.Contains(query))
                {
                    outstandingQueries.Add(query);
                    var databaseRef = geoFire.DatabaseReference;
                    var firebaseQuery = databaseRef.OrderByChild("g").StartAt(query.StartValue).EndAt(query.EndValue);
                    firebaseQuery.AddChildEventListener(childEventListener);
                    AddValueToReadyListener(firebaseQuery, query);
                    firebaseQueries.Add(query, firebaseQuery);
                }
            }

            foreach (var info in locationInfos)
            {
                var oldInfo = info.Value;

                if (oldInfo != null)
                    UpdateLocationInfo(oldInfo.snapshot, oldInfo.location);
            }

            // remove locations that are not part of the geo query anymore
            foreach (var entry in locationInfos)
            {
                if (!GeoHashQueriesContainGeoHash(entry.Value.geoHash))
                    locationInfos.Remove(entry.Key);
            }

            //var en = locationInfos.EntrySet().GetEnumerator();
            //while (en.MoveNext())
            //{
            //    IMapEntry entry = (IMapEntry)en.Current;
            //    if (GeoHashQueriesContainGeoHash(((LocationInfo)entry.Value).geoHash))
            //        locationInfos.Remove(entry.Key);
            //}

            CheckAndFireReady();
        }

        private void ChildAdded(DataSnapshot snapshot)
        {
            GeoLocation location = Geofire.GetLocationValue(snapshot);
            if (location != null)
                UpdateLocationInfo(snapshot, location);
            else
                throw new System.Exception("Got Datasnapshot without location with key " + snapshot.Key);
        }

        private void ChildChanged(DataSnapshot snapshot)
        {
            GeoLocation location = Geofire.GetLocationValue(snapshot);
            if (location != null)
                UpdateLocationInfo(snapshot, location);
            else
                throw new System.Exception("Got Datasnapshot without location with key " + snapshot.Key);
        }

        private void ChildRemoved(DataSnapshot snapshot)
        {
            string key = snapshot.Key;
            LocationInfo info = locationInfos[key];
            if (info != null)
            {
                geoFire.DatabaseReferenceForKey(key).AddListenerForSingleValueEvent(new ChildRemovedListener(this, key));
            }
        }

        /// <summary> Adds a new GeoQueryEventListener to this GeoQuery. </summary>
        /// <param name="listener"> The listener to add </param>
        public void AddGeoQueryEventListener(IGeoQueryEventListener listener)
        {
            AddGeoQueryDataEventListener(new EventListenerBridge(listener));
        }

        /// <summary> Adds a new GeoQueryEventListener to this GeoQuery. </summary>
        /// <param name="listener"> The listener to add </param>
        public void AddGeoQueryDataEventListener(IGeoQueryDataEventListener listener)
        {
            if (eventListeners.Contains(listener))
                throw new System.ArgumentException("Added the same listener twice to a GeoQuery!");

            eventListeners.Add(listener);

            if (queries == null)
                SetupQueries();
            else
            {
                foreach (var entry in locationInfos)
                {
                    var key = entry.Key;
                    var info = entry.Value;

                    if (info.inGeoQuery)
                        geoFire.RaiseEvent(() =>
                        {
                            listener.OnDataEntered(info.snapshot, info.location);
                        });
                }

                if (CanFireReady())
                    geoFire.RaiseEvent(() =>
                    {
                        listener.OnGeoQueryReady();
                    });
            }
        }

        /// <summary> Removes an event listener. </summary>
        /// <param name="listener"> The listener to remove </param>
        public void RemoveGeoQueryEventListener(IGeoQueryEventListener listener)
        {
            RemoveGeoQueryEventListener(new EventListenerBridge(listener));
        }

        /// <summary> Removes an event listener. </summary>
        /// <param name="listener"> The listener to remove </param>
        public void RemoveGeoQueryEventListener(IGeoQueryDataEventListener listener)
        {
            if (!eventListeners.Contains(listener))
                throw new System.ArgumentException("Trying to remove listener that was removed or not added!");

            eventListeners.Remove(listener);
            if (HasListeners())
                Reset();
        }

        /// <summary> Removes all event listeners from this GeoQuery. </summary>
        public void RemoveAllListeners()
        {
            eventListeners.Clear();
            Reset();
        }

        /// <summary>
        ///    Sets the center and radius (in kilometers) of this query, and triggers new events if necessary.
        /// </summary>
        /// <param name="center"> The new center </param>
        /// <param name="radius">
        ///    The radius of the query, in kilometers. The maximum radius that is supported is about 8587km.
        /// </param>
        public void SetLocation(GeoLocation center, double radius)
        {
            this.center = center;
            // convert radius to meters
            this.radius = GeoUtils.CapRadius(radius) * KILOMETER_TO_METER;

            if (HasListeners())
                SetupQueries();
        }

        private class LocationInfo
        {
            public readonly GeoLocation location;
            public readonly bool inGeoQuery;
            public readonly GeoHash geoHash;
            public readonly DataSnapshot snapshot;

            public LocationInfo(GeoLocation location, bool inGeoQuery, DataSnapshot snapshot)
            {
                this.location = location;
                this.inGeoQuery = inGeoQuery;
                this.geoHash = new GeoHash(location);
                this.snapshot = snapshot;
            }
        }

        private class AddValueListener : Java.Lang.Object, IValueEventListener
        {
            private GeoQuery GeoQuery;
            private GeoHashQuery GeoHashQuery;

            public AddValueListener(GeoQuery geoQuery, GeoHashQuery query)
            {
                GeoQuery = geoQuery;
                GeoHashQuery = query;
            }

            public void OnCancelled(DatabaseError error)
            {
                lock (GeoQuery)
                {
                    foreach (var listener in GeoQuery.eventListeners)
                    {
                        GeoQuery.geoFire.RaiseEvent(() =>
                        {
                            listener.OnGeoQueryError(error);
                        });
                    }
                }
            }

            public void OnDataChange(DataSnapshot snapshot)
            {
                lock (GeoQuery)
                {
                    GeoQuery.outstandingQueries.Remove(GeoHashQuery);
                    GeoQuery.CheckAndFireReady();
                }
            }
        }

        private class ChildEventListener : Java.Lang.Object, IChildEventListener
        {
            private GeoQuery geoQuery;

            public ChildEventListener(GeoQuery geoQuery)
            {
                this.geoQuery = geoQuery;
            }

            public void OnChildAdded(DataSnapshot snapshot, string previousChildName)
            {
                lock (geoQuery.childLock)
                {
                    geoQuery.ChildAdded(snapshot);
                }
            }

            public void OnChildChanged(DataSnapshot snapshot, string previousChildName)
            {
                lock (geoQuery.childLock)
                {
                    geoQuery.ChildChanged(snapshot);
                }
            }

            public void OnChildRemoved(DataSnapshot snapshot)
            {
                lock (geoQuery.childLock)
                {
                    geoQuery.ChildRemoved(snapshot);
                }
            }

            public void OnChildMoved(DataSnapshot snapshot, string previousChildName)
            {
                // ignore, this should be handled by onChildChanged
            }

            public void OnCancelled(DatabaseError error)
            {
                // ignore, our API does not support onCancelled
            }
        }

        private class ChildRemovedListener : Java.Lang.Object, IValueEventListener
        {
            private GeoQuery geoQuery;
            private string key;

            public ChildRemovedListener(GeoQuery geoQuery, string key)
            {
                this.geoQuery = geoQuery;
                this.key = key;
            }

            public void OnCancelled(DatabaseError error)
            {
                // tough luck
            }

            public void OnDataChange(DataSnapshot snapshot)
            {
                GeoLocation location = Geofire.GetLocationValue(snapshot);
                GeoHash hash = location != null ? new GeoHash(location) : null;
                if (hash == null || !geoQuery.GeoHashQueriesContainGeoHash(hash))
                {
                    var locationInfo = geoQuery.locationInfos[key];

                    if (locationInfo.inGeoQuery)
                    {
                        var info = geoQuery.locationInfos.Remove(key);

                        if (info)
                        {
                            foreach (var listener in geoQuery.eventListeners)
                            {
                                listener.OnDataExited(locationInfo.snapshot);
                            }
                        }
                    }
                }
            }
        }
    }
}