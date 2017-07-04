using System;
using Assets;
using Assets.Scripts;

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
    public string furn;
    public Tile(string terrain, string furniture)
    {
        this.ter = terrain;
        this.furn = furniture;
    }
}