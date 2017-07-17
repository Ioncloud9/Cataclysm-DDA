using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Assets.DDAInterop.Marshalers
{
	public class CMap_Marshaler : ICustomMarshaler
	{
		static ICustomMarshaler GetInstance(string cookie)
		{
			return new CMap_Marshaler();
		}

		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			CMap map = (CMap)Marshal.PtrToStructure(pNativeData, typeof(CMap));

			int size = map.width * map.height;
			Tile[] tiles = new Tile[size];

			for (int i = 0; i < size; i++)
			{
				IntPtr p = new IntPtr(map.tiles.ToInt64() + i * Marshal.SizeOf(typeof(CTile)));
				CTile tile = (CTile)Marshal.PtrToStructure(p, typeof(CTile));
				string ter = Marshal.PtrToStringAnsi(tile.ter);
				string furn = Marshal.PtrToStringAnsi(tile.furn);
				tiles[i] = new Tile(ter, tile.loc, furn);
			}
			Map res = new Map (map.width, map.height, tiles);
			return res;
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			CMap map = (CMap)Marshal.PtrToStructure(pNativeData, typeof(CMap));
			int size = map.width * map.height;

			for (int i = 0; i < size; i++)
			{
				IntPtr p = new IntPtr(map.tiles.ToInt64() + i * Marshal.SizeOf(typeof(CTile)));
				CTile tile = (CTile)Marshal.PtrToStructure(p, typeof(CTile));
				Marshal.FreeCoTaskMem(tile.furn);
				Marshal.FreeCoTaskMem(tile.ter);
			}
			Marshal.FreeCoTaskMem(map.tiles);
			Marshal.FreeCoTaskMem(pNativeData);
		}

		public int GetNativeDataSize()
		{
			return Marshal.SizeOf(typeof(CMap));
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

