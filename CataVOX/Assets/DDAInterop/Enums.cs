using System;
using System.Runtime.InteropServices;

namespace Assets.DDAInterop
{
    public enum Attitude
    {
        Hostile = 0,
        Neutral,
        Friendly,
        Any
    }

	public enum WeatherType
	{
		Null = 0,
		Clear,
		Sunny,
		Cloudy,
		Drizzle,
		Rainy,
		Thunder,
		Lightning,
		AcidDrizzle,
		AcidRain,
		Flurries,
		Snow,
		SnowStorm,
		NumWeatherTypes
	}

	public enum SeasonType 
	{
		Spring = 0,
		Summer,
		Autumn,
		Winter	
	}

	public enum MoonPhase
	{
		New = 0,
		WaxingCrescent,
		HalfMoonWaxing,
		WaxingGibbous,
		Full,
		WaningGibbous,
		HalfMoonWaning,
		WaningCrescent,
		PhaseMax
	}
}