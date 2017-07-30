using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public static class Utils
    {
        public static string WeatherImage(WeatherType type, bool day)
        {
            var pfx = (day ? "day_" : "night_");
            switch (type)
            {
                case WeatherType.WEATHER_CLEAR: return pfx + "Clear";
                case WeatherType.WEATHER_CLOUDY: return pfx + "Cloudy";
                case WeatherType.WEATHER_RAINY: return pfx + "Rain";
                case WeatherType.WEATHER_LIGHTNING: return pfx + "Storm";
                case WeatherType.WEATHER_SNOW: return pfx + "Snow";
                case WeatherType.WEATHER_DRIZZLE: return pfx + "Drizzle";
                case WeatherType.WEATHER_FLURRIES: return pfx + "Flurry";
                default: return "na";
            }
        }

        public static string SeasonImage(string season)
        {
            switch (season.ToLower())
            {
                case "spring": return "spring";
                case "fall": return "fall";
                case "winter": return "winter";
                case "summer": return "summer";
                default: return "na";
            }
        }
    }
    public static class DirectionUtils
    {

        public static Direction ModMoveRelCamera(Direction cameraDirection, Direction moveDirection)
        {
            //if (cameraDirection == Direction.N) return moveDirection;
            //if (moveDirection == Direction.N) return cameraDirection;
            //if (cameraDirection == moveDirection) return Direction.N;


            /*
             * 7  0  1
             * 6     2
             * 5  4  3
             * 
             * cameraDirection = 7
             * moveDirection = 2
             * 7 + 2 = 9
             * 9 - 8 = 1
             * return NE
             * 
             * cameraDirection = 7
             * moveDirection = 6
             * 7 + 6 = 13
             * 13 - 8 = 5
             * return SW
             */
            var offsetDir = (int)cameraDirection + (int)moveDirection;
            if (offsetDir > 7) offsetDir -= 8;
            return (Direction)offsetDir;
        }
    }
}
