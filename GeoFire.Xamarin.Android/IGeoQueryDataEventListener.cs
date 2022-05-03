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
    public interface IGeoQueryDataEventListener
    {
        /// <summary>
        ///    Called if a dataSnapshot entered the search area of the GeoQuery. This method is
        ///    called for every dataSnapshot currently in the search area at the time of adding the listener.
        ///    <para>
        ///       This method is once per datasnapshot, and is only called again if OnDataExited was
        ///       called in the meantime.
        ///    </para>
        /// </summary>
        /// <param name="dataSnapshot">
        ///    The associated dataSnapshot that entered the search area
        /// </param>
        /// <param name="location">
        ///    The location for this dataSnapshot as a GeoLocation object
        /// </param>
        void OnDataEntered(DataSnapshot dataSnapshot, GeoLocation location);

        /// <summary>
        ///    Called if a datasnapshot exited the search area of the GeoQuery. This is method is
        ///    only called if OnDataEntered was called for the datasnapshot.
        /// </summary>
        /// <param name="dataSnapshot">
        ///    The associated dataSnapshot that exited the search area
        /// </param>
        void OnDataExited(DataSnapshot dataSnapshot);

        /// <summary>
        ///    Called if a dataSnapshot moved within the search area. 
        ///    <para> This method can be called multiple times. </para>
        /// </summary>
        /// <param name="dataSnapshot">
        ///    The associated dataSnapshot that moved within the search area
        /// </param>
        /// <param name="location">
        ///    The location for this dataSnapshot as a GeoLocation object
        /// </param>
        void OnDataMoved(DataSnapshot dataSnapshot, GeoLocation location);

        /// <summary>
        ///    Called if a dataSnapshot changed within the search area. 
        ///    <para>
        ///       An OnDataMoved() is always followed by OnDataChanged() but it is be possible to see
        ///       OnDataChanged() without an preceding OnDataMoved().
        ///    </para>
        ///    <para>
        ///       This method can be called multiple times for a single location change, due to the
        ///       way the Realtime Database handles floating point numbers.
        ///    </para>
        ///    Note: this method is not related to ValueEventListener#onDataChange(DataSnapshot). 
        /// </summary>
        /// <param name="dataSnapshot">
        ///    The associated dataSnapshot that moved within the search area
        /// </param>
        /// <param name="location">
        ///    The location for this dataSnapshot as a GeoLocation object
        /// </param>
        void OnDataChanged(DataSnapshot dataSnapshot, GeoLocation location);

        /// <summary>
        ///    Called once all initial GeoFire data has been loaded and the relevant events have been
        ///    fired for this query.
        ///    <para>
        ///       Every time the query criteria is updated, this observer will be called after the
        ///       updated query has fired the appropriate dataSnapshot entered or dataSnapshot exited events.
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