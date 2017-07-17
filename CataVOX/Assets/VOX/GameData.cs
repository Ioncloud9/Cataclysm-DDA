using System;
using Assets;
using Assets.Scripts;
using UnityEngine;
using System.Runtime.InteropServices;

[Serializable]
public class GameData
{
    public IVector3 playerPosition;
    public Calendar calendar;
    public Weather weather;
    public Map map;
}

[Serializable]
public class Map
{
    public int width;
    public int height;
    public Tile[] tiles;
    public Map(int width, int height, Tile[] tiles)
    {
        this.width = width;
        this.height = height;
        this.tiles = tiles;
    }
	public Tile tileAt(int x, int y) {
		if (x >=0 && y >= 0 && x < width && y < height)
			return this.tiles [width * y + x];
		return null;
	}
}

[Serializable]
public class Weather
{
    public WeatherType Type
    {
        get { return (WeatherType) type; }
    }
    public int type;
    public double temprature;
    public double humidity;
    public double wind;
    public double pressure;
    public bool acidic;
}

[Serializable]
public class Calendar
{
    public string season;
    public string time;
    public bool isNight;
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public class IVector3
{
	public int x;
	public int y;
	public int z;
	public IVector3(int x, int y, int z) 
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
	public static implicit operator Vector3(IVector3 v) {
		return new Vector3 (v.x, v.y, v.z);
	}

    public override string ToString() {
        return string.Format("({0}, {1}, {2})", x, y, z);
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public class IVector2
{
	public int x;
	public int y;
	public IVector2(int x, int y) 
	{
		this.x = x;
		this.y = y;
	}
	public static implicit operator Vector2(IVector2 v) {
		return new Vector2 (v.x, v.y);
	}

    public override string ToString() {
        return string.Format("({0}, {1})", x, y);
    }
}

[Serializable]
public class Tile
{
    public string ter;
    public IVector3 loc;
    public string furn;

    public Tile(string terrain, IVector3 loc, string furniture)
    {
        this.ter = terrain;
        this.furn = furniture;
        this.loc = loc;
    }
}