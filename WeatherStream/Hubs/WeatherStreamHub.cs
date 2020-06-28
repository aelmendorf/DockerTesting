using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace WeatherStream.Hubs {
    public class WeatherStreamHub:Hub {
        private readonly WeatherForcaster _forcaster;

        public WeatherStreamHub(WeatherForcaster forcaster) {
            this._forcaster = forcaster;         
        }

        public IEnumerable<WeatherForcast> GetAllForcast() {
            return this._forcaster.GetWeatherForecasts();
        }

        public ChannelReader<WeatherForcast> StreamForcast() {
            return this._forcaster.StreamForcasts().AsChannelReader(10);
        }

        public ForcasterState GetState() {
            return this._forcaster.State;
        }

        public async Task StartForcast() {
            await this._forcaster.StartForcast();
        }

        public async Task StopForcast() {
            await this._forcaster.StopForcast();
        }

        public async Task ResetForcast() {
            await this._forcaster.ResetForcast();
        }
    }
}
