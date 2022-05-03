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
    ///    GeoQuery notifies listeners with this interface about keys that entered, exited, or moved
    ///    within the query.
    /// </summary>
    public interface IGeoQueryEventListener
    {
        /// <summary>
        ///    Called if a key entered the search area of the GeoQuery. 
        ///    <para>
        ///       This method is called for every key currently in the search area at the time of
        ///       adding the listener.
        ///    </para>
        ///    <para>
        ///       This method is once per key, and is only called again if OnKeyExited was called in
        ///       the meantime.
        ///    </para>
        /// </summary>
        /// <param name="key"> The key that entered the search area </param>
        /// <param name="location"> The location for this key as a GeoLocation object </param>
        void OnKeyEntered(string key, GeoLocation location);

        /// <summary>
        ///    Called if a key exited the search area of the GeoQuery. 
        ///    <para>
        ///       This is method is only called if OnKeyEntered was called for the key.
        ///    </para>
        /// </summary>
        /// <param name="key"> The key that exited the search area </param>
        void OnKeyExited(string key);

        /// <summary>
        ///    Called if a key moved within the search area. 
        ///    <para> This method can be called multiple times. </para>
        /// </summary>
        /// <param name="key"> The key that moved within the search area </param>
        /// <param name="location"> The location for this key as a GeoLocation object </param>
        void OnKeyMoved(string key, GeoLocation location);

        /// <summary>
        ///    Called once all initial GeoFire data has been loaded and the relevant events have been
        ///    fired for this query.
        ///    <para>
        ///       Every time the query criteria is updated, this observer will be called after the
        ///       updated query has fired the appropriate key entered or key exited events.
        ///    </para>
        /// </summary>
        void OnGeoQueryReady();

        /// <summary>
        ///    Called in case an error occurred while retrieving locations for a query, e.g.
        ///    violating security rules.
        /// </summary>
        /// <param name="error"> The error that occurred while retrieving the query </param>
        void OnGeoQueryError(DatabaseError error);
    }
}