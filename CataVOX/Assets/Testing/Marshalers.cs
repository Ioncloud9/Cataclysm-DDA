using System;
using System.Runtime.InteropServices;

namespace Marshalers
{
    public struct CStringArray
    {
        public IntPtr /* char** */ stringArray;
        public int len;
    }

	public struct CGameData
	{
		public CCalendar calendar;
		public CWeather weather;
		public CMap map;
	}

	public struct CMap
	{
		public int width;
		public int height;
		public IntPtr /* CTile[width*height] */ tiles; 
	}

	public struct CTile
	{
		public IntPtr /* char* */ ter;
		public IntPtr /* char* */ furn;
		public IVector3 loc;
	}

	public struct CWeather
	{
		public /*WeatherType*/int type;
		public double temperature;
		public double humidity;
		public double wind;
		public double pressure;
		public bool acidic;
	}

	public struct CCalendar
	{
		public /*SeasonType*/int season;
		public IntPtr time;
		public int years;
		public int days;
		public /*MoonPhase*/int moon;
		public bool isNight;
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

	public class CGameData_Marshaler : ICustomMarshaler
	{
		static ICustomMarshaler GetInstance(string cookie) {
			return new CGameData_Marshaler();
		}

		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			CGameData data = (CGameData)Marshal.PtrToStructure (pNativeData, typeof(CGameData));
			GameData resData = new GameData ();
			resData.calendar = new Calendar ();
			resData.calendar.season =  ((SeasonType)data.calendar.season).ToString();
			resData.calendar.time = Marshal.PtrToStringAnsi (data.calendar.time);
			resData.calendar.isNight = data.calendar.isNight;
			resData.weather = new Weather ();
			resData.weather.type = data.weather.type;
			resData.weather.temprature = data.weather.temperature;
			resData.weather.humidity = data.weather.humidity;
			resData.weather.wind = data.weather.wind;
			resData.weather.pressure = data.weather.pressure;
			resData.weather.acidic = data.weather.acidic;

			int width = data.map.width;
			int height = data.map.height;
			int size = width * height;

			Tile[] tiles = new Tile[width * height];
			resData.map = new Map (width, height, tiles);

			for (int i = 0; i < size; i++)
			{
				IntPtr p = new IntPtr (data.map.tiles.ToInt64 () + i * Marshal.SizeOf (typeof(CTile)));
				CTile tile = (CTile)Marshal.PtrToStructure (p, typeof(CTile));
				string ter = Marshal.PtrToStringAnsi (tile.ter);
				string furn = Marshal.PtrToStringAnsi (tile.furn);
				resData.map.tiles [i] = new Tile (ter, tile.loc, furn);
			}
			return resData;
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			CGameData data = (CGameData)Marshal.PtrToStructure(pNativeData, typeof(CGameData));
			Marshal.FreeCoTaskMem(data.calendar.time);
			int size = data.map.width * data.map.height;

			for (int i = 0; i < data.map.width * data.map.height; i++) {
				IntPtr p = new IntPtr (data.map.tiles.ToInt64 () + i * Marshal.SizeOf (typeof(CTile)));
				CTile tile = (CTile)Marshal.PtrToStructure (p, typeof(CTile));
				Marshal.FreeCoTaskMem (tile.furn);
				Marshal.FreeCoTaskMem (tile.ter);
			}
			Marshal.FreeCoTaskMem (data.map.tiles);
			Marshal.FreeCoTaskMem (pNativeData);
		}

		public int GetNativeDataSize()
		{
			return Marshal.SizeOf(typeof(CGameData));
		}

		public void CleanUpManagedData(object ManagedObj)
		{
			// don't send string arrays to c++ yet
		}

		public IntPtr MarshalManagedToNative(object ManagedObj)
		{
			// don't send string arrays to c++ yet
			return IntPtr.Zero;
		}
	}

    public class CStringArray_Marshaler : ICustomMarshaler
    {
        static ICustomMarshaler GetInstance(string cookie) {
            return new CStringArray_Marshaler();
        }
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            CStringArray arr = (CStringArray)Marshal.PtrToStructure(pNativeData, typeof(CStringArray));
            string[] managedStringArray = new string[arr.len];
            IntPtr[] pIntPtrArray = new IntPtr[arr.len];
            Marshal.Copy(arr.stringArray, pIntPtrArray, 0, arr.len);
            for (int i = 0; i < arr.len; i++)
            {
                managedStringArray[i] = Marshal.PtrToStringAnsi(pIntPtrArray[i]);
            }
            return managedStringArray;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            CStringArray arr = (CStringArray)Marshal.PtrToStructure(pNativeData, typeof(CStringArray));
            IntPtr[] pIntPtrArray = new IntPtr[arr.len];
            Marshal.Copy(arr.stringArray, pIntPtrArray, 0, arr.len);
            for (int i = 0; i < arr.len; i++)
            {
                Marshal.FreeCoTaskMem(pIntPtrArray[i]);
            }
            if (arr.len > 0)
            {
                Marshal.FreeCoTaskMem(arr.stringArray);
            }
            Marshal.FreeCoTaskMem(pNativeData);
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            // don't send string arrays to c++ yet
        }

        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(CStringArray));
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            // don't send string arrays to c++ yet
            return IntPtr.Zero;
        }
    }
}