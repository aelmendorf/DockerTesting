using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using WeatherStream.Hubs;

namespace WeatherStream {
    public enum Day { }

    public class WeatherForcaster {
        private readonly SemaphoreSlim _forcastUpdateLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _forcastStateLock = new SemaphoreSlim(1, 1);

        private readonly ConcurrentDictionary<string, WeatherForcast> _forcasts = new ConcurrentDictionary<string, WeatherForcast>();
        private readonly Subject<WeatherForcast> _forcastSubject = new Subject<WeatherForcast>(); 
        private readonly int rangePercent = 80;

        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(200);
        private readonly Random _rand = new Random();
        private readonly Random _forcastTypeGen = new Random();

        private Timer _timer;
        private volatile bool _updatingForcast;
        private volatile ForcasterState _forcasterState=ForcasterState.Closed;

        public WeatherForcaster(IHubContext<WeatherStreamHub> hub) {
            this._hub = hub;
            this.LoadDefaultForcast();
        }

        private IHubContext<WeatherStreamHub> _hub { get; set; }

        public ForcasterState State {
            get => this._forcasterState;
            private set => this._forcasterState = value;
        }

        public async Task StartForcast() {
            await this._forcastStateLock.WaitAsync();
            try {
                if (this._forcasterState != ForcasterState.Running) {
                    this._timer = new Timer(this.UpdateAllForcast, null, this._updateInterval, this._updateInterval);
                    this._forcasterState = ForcasterState.Running;
                    await this.BroadcastForcastStateChange(ForcasterState.Running);

                }
            } finally {
                this._forcastStateLock.Release();
            }
        }

        public async Task StopForcast() {
            await this._forcastStateLock.WaitAsync();
            try {
                if (this._forcasterState == ForcasterState.Running) {
                    if (this._timer != null) {
                        this._timer.Dispose();
                    }

                    this._forcasterState = ForcasterState.Closed;
                    await this.BroadcastForcastStateChange(ForcasterState.Closed);
                }
            } finally {
                this._forcastStateLock.Release();
            }
        }

        public async Task ResetForcast() {
            await this._forcastStateLock.WaitAsync();
            try {
                if (this._forcasterState != ForcasterState.Closed) {
                    throw new InvalidOperationException("Forcaster Must Be Closed Before Reset");
                }

                this.LoadDefaultForcast();
                await this.BroadcastForcastReset();

            } finally {
                this._forcastStateLock.Release();
            }
        }

        public IObservable<WeatherForcast> StreamForcasts() {
            return this._forcastSubject;
        }

        public IEnumerable<WeatherForcast> GetWeatherForecasts() {
            return this._forcasts.Values;
        }

        private async Task BroadcastForcastReset() {
            await this._hub.Clients.All.SendAsync("forcastReset");
        }

        private async Task BroadcastForcastStateChange(ForcasterState state) {
            switch (state) {
                case ForcasterState.Running:
                    await this._hub.Clients.All.SendAsync("forcastStarted");
                    break;
                case ForcasterState.Closed:
                    await this._hub.Clients.All.SendAsync("forcastStopped");
                    break;
            }
        }

        private async void UpdateAllForcast(object state) {

            await this._forcastUpdateLock.WaitAsync();
            try {
                if (!this._updatingForcast) {
                    this._updatingForcast = true;
                    foreach(var forcast in this._forcasts.Values) {
                        this.UpdateForcast(forcast);
                        this._forcastSubject.OnNext(forcast);
                    }
                    this._updatingForcast = false;
                }
            } finally {
                this._forcastUpdateLock.Release();
            }
        }

        private void UpdateForcast(WeatherForcast forcast) {
            //var random = new Random((int)Math.Floor(forcast.TemperatureC));
            var random = new Random();
            var change = random.Next(150, 410);
            forcast.TemperatureC = change/10;
            forcast.ForcastType = GenerateForcastType();
        }

        private void LoadDefaultForcast() {
            this._forcasts.Clear();
            var forcasts = new List<WeatherForcast>();
            for (int i = 0; i < 7; i++) {
                var temp = new WeatherForcast() { Date = DateTime.Now.AddDays(i + 1), Day = ((DayOfWeek)i).ToString(), TemperatureC = this._rand.Next(15, 40), ForcastType = this.GenerateForcastType() };
                forcasts.Add(temp);
            }
            forcasts.ForEach(forcast => this._forcasts.TryAdd(forcast.Day, forcast));
        }

        private ForcastType GenerateForcastType() {
            int type = this._forcastTypeGen.Next(0, 3);
            return (ForcastType)type;
        }
    }

    public enum ForcasterState {
        Running,
        Closed
    }
}
