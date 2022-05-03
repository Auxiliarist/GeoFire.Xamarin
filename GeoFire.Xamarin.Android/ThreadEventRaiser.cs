using Java.Lang;
using Java.Util.Concurrent;
using System;

namespace GeoFire.Xamarin.Android
{
    public class ThreadEventRaiser : Java.Lang.Object, IEventRaiser
    {
        private readonly IExecutorService executorService;

        public ThreadEventRaiser()
        {
            executorService = Executors.NewSingleThreadExecutor();
        }

        public void RaiseEvent(Action r)
        {
            executorService.Submit(new Runnable(r));
        }

    }
}