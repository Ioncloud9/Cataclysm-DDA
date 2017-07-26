using System;
using Assets;
using Assets.Scripts;
using UnityEngine;
using System.Runtime.InteropServices;

[Serializable]
public class GameData
{
    public Vector3Int playerPosition;
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
    public Tile tileAt(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            return this.tiles[width * y + x];
        return null;
    }
}

[Serializable]
public struct Weather
{
    public WeatherType Type
    {
        get { return (WeatherType)type; }
    }
    public int type;
    public double temprature;
    public double humidity;
    public double wind;
    public double pressure;
    public bool acidic;
}

[Serializable]
public struct Calendar
{
    public string season;
    public string time;
    public bool isNight;
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Vector3Int
{
    public int x;
    public int y;
    public int z;
    public Vector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public static implicit operator Vector3(Vector3Int v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static Vector3Int operator +(Vector3Int v1, Vector3Int v2)
    {
        return new Vector3Int(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1}, {2})", x, y, z);
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Vector2Int
{
    public int x;
    public int y;
    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public static implicit operator Vector2(Vector2Int v)
    {
        return new Vector2(v.x, v.y);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }
}

[Serializable]
public class Tile
{
    public string ter;
    public Vector3Int loc;
    public string furn;

    public Tile(string terrain, Vector3Int loc, string furniture)
    {
        this.ter = terrain;
        this.furn = furniture;
        this.loc = loc;
    }
}