using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public enum Direction
    {
        N = 0,
        NE = 1,
        E = 2,
        SE = 3,
        S = 4,
        SW = 5,
        W = 6,
        NW = 7
    }

    [Serializable]
    public enum WeatherType : int
    {
        WEATHER_NULL,         //!< For data and stuff
        WEATHER_CLEAR,        //!< No effects
        WEATHER_SUNNY,        //!< Glare if no eye protection
        WEATHER_CLOUDY,       //!< No effects
        WEATHER_DRIZZLE,      //!< Light rain
        WEATHER_RAINY,        //!< Lots of rain, sight penalties
        WEATHER_THUNDER,      //!< Warns of lightning to come
        WEATHER_LIGHTNING,    //!< Rare lightning strikes!
        WEATHER_ACID_DRIZZLE, //!< No real effects; warning of acid rain
        WEATHER_ACID_RAIN,    //!< Minor acid damage
        WEATHER_FLURRIES,     //!< Light snow
        WEATHER_SNOW,         //!< Medium snow
        WEATHER_SNOWSTORM,    //!< Heavy snow
        NUM_WEATHER_TYPES     //!< Sentinel value
    }
}
