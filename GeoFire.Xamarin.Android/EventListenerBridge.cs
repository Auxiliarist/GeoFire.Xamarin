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

namespace GeoFire.Xamarin.Android
{
    /// <summary>
    ///    GeoQuery notifies listeners with this interface about dataSnapshots that entered, exited,
    ///    or moved within the query.
    /// </summary>
    public sealed class EventListenerBridge : IGeoQueryDataEventListener
    {
        private readonly IGeoQueryEventListener listener;

        public EventListenerBridge(IGeoQueryEventListener listener)
        {
            this.listener = listener;
        }

        public void OnDataChanged(DataSnapshot dataSnapshot, GeoLocation location)
        {
            // No-op.
        }

        public void OnDataEntered(DataSnapshot dataSnapshot, GeoLocation location)
        {
            listener.OnKeyEntered(dataSnapshot.Key, location);
        }

        public void OnDataExited(DataSnapshot dataSnapshot)
        {
            listener.OnKeyExited(dataSnapshot.Key);
        }

        public void OnDataMoved(DataSnapshot dataSnapshot, GeoLocation location)
        {
            listener.OnKeyMoved(dataSnapshot.Key, location);
        }

        public void OnGeoQueryError(DatabaseError error)
        {
            listener.OnGeoQueryError(error);
        }

        public void OnGeoQueryReady()
        {
            listener.OnGeoQueryReady();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || GetType() != obj.GetType())
                return false;

            EventListenerBridge that = (EventListenerBridge)obj;

            return listener.Equals(that.listener);
        }

        public override int GetHashCode()
        {
            return listener.GetHashCode();
        }
    }
}