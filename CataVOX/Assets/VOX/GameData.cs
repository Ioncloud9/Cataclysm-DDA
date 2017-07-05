using System;
using Assets;
using Assets.Scripts;
using UnityEngine;

[Serializable]
public class GameData
{
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
public class Tile
{
    public string ter;
    public string loc;
    public string furn;

    public Vector3 Location
    {
        get
        {
            var sp = loc.Split(',');
            //Unity's Z axis is the Y axis, so convert it from DDA to Unity1
            return new Vector3(float.Parse(sp[0]), float.Parse(sp[2]), float.Parse(sp[1]));
        }
    }
    public Tile(string terrain, string loc, string furniture)
    {
        this.ter = terrain;
        this.furn = furniture;
        this.loc = loc;
    }
}