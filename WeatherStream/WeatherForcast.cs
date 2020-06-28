using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherStream {

    public enum ForcastType {
        Rainy,
        Overcast,
        Sunny,
        Fog
    }

    public class WeatherForcast {

        public DateTime Date { get; set; }

        public string Day { get; set; }

        public double TemperatureC { get; set; }

        public double TemperatureF => 32 + (double)(TemperatureC / 0.5556);

        public ForcastType ForcastType { get; set; }

    }
}
