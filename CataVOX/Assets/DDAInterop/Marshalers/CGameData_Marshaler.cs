using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Assets.DDAInterop.Marshalers
{
    public class CGameData_Marshaler : ICustomMarshaler
    {
        static ICustomMarshaler GetInstance(string cookie)
        {
            return new CGameData_Marshaler();
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            CGameData data = (CGameData)Marshal.PtrToStructure(pNativeData, typeof(CGameData));
            GameData resData = new GameData();
            resData.playerPosition = data.playerPosition;
            resData.calendar = new Calendar();
            resData.calendar.season = ((SeasonType)data.calendar.season).ToString();
            resData.calendar.time = Marshal.PtrToStringAnsi(data.calendar.time);
            resData.calendar.isNight = data.calendar.isNight;
            resData.weather = new Weather();
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
            resData.map = new Map(width, height, tiles);

            for (int i = 0; i < size; i++)
            {
                IntPtr p = new IntPtr(data.map.tiles.ToInt64() + i * Marshal.SizeOf(typeof(CTile)));
                CTile tile = (CTile)Marshal.PtrToStructure(p, typeof(CTile));
                resData.map.tiles[i] = new Tile(tile.ter, tile.loc, tile.furn);
            }
            return resData;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            CGameData data = (CGameData)Marshal.PtrToStructure(pNativeData, typeof(CGameData));
            Marshal.FreeCoTaskMem(data.calendar.time);
            Marshal.FreeCoTaskMem(data.map.tiles);
            Marshal.FreeCoTaskMem(pNativeData);
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
}
