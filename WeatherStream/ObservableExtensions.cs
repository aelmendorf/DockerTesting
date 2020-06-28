using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WeatherStream {
    public static class ObservableExtensions {
        public static ChannelReader<T> AsChannelReader<T>(this IObservable<T> observable,int? maxBufferSize = null) {

            var channel = maxBufferSize != null ? Channel.CreateBounded<T>(maxBufferSize.Value) : Channel.CreateUnbounded<T>();
            var disposable=observable.Subscribe(
                value => channel.Writer.TryWrite(value), 
                error => channel.Writer.TryComplete(error), 
                () => channel.Writer.TryComplete());

            channel.Reader.Completion.ContinueWith(task => disposable.Dispose());
            return channel.Reader;
        }
    }
}
