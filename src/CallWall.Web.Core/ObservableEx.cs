using System;
using System.Reactive.Linq;

namespace CallWall.Web
{
    public static class ObservableEx
    {
        public static IObservable<T> TakeUntil<T>(this IObservable<T> source, Func<T, bool> terminator)
        {
            return Observable.Create<T>(o =>
                source.Subscribe(x =>
                                 {
                                     o.OnNext(x);
                                     if (terminator(x))
                                         o.OnCompleted();
                                 },
                    o.OnError,
                    o.OnCompleted));
        }
    }
}