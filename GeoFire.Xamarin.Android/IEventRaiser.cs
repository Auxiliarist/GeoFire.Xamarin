using System;

namespace GeoFire.Xamarin.Android
{
    public interface IEventRaiser
    {
        void RaiseEvent(Action r);
    }
}