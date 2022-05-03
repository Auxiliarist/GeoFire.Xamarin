using Android.OS;
using System;

namespace GeoFire.Xamarin.Android
{
    public class AndroidEventRaiser : IEventRaiser
    {
        private readonly Handler mainThreadHandler;

        public AndroidEventRaiser()
        {
            mainThreadHandler = new Handler(Looper.MainLooper);
        }

        public void RaiseEvent(Action r)
        {
            mainThreadHandler.Post(r);
        }
    }
}