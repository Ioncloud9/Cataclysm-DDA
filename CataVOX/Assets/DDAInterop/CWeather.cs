using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DDAInterop
{
    public struct CWeather
    {
        public /*WeatherType*/int type;
        public double temperature;
        public double humidity;
        public double wind;
        public double pressure;
        public bool acidic;
    }
}
