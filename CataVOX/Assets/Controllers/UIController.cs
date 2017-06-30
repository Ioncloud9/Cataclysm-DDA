using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Controllers
{
    public class UIController : GameBase
    {
        public Weather Weather;
        public Calendar Calendar;

        public Image imgWeather;
        public Text txtTime;
        public RectTransform mapArrow;

        private void Start()
        {
        }

        public void SetUI(Weather weather, Calendar calendar)
        {
            Weather = weather;
            Calendar = calendar;
            txtTime.text = calendar.time;
            var path = string.Format("UIImages/{0}", Utils.WeatherImage(weather.Type, !calendar.isNight));
            Debug.Log(string.Format("Weather {0}", path));
            imgWeather.sprite = Resources.Load<Sprite>(path);
        }

        public void AdjustMiniMap(Direction facing)
        {
            switch (Game.Camera.Facing)
            {
                case Direction.N:
                    mapArrow.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Direction.NE:
                    mapArrow.transform.localRotation = Quaternion.Euler(0, 0, -45);
                    break;
                case Direction.E:
                    mapArrow.transform.localRotation = Quaternion.Euler(0, 0, -90);
                    break;
                case Direction.SE:
                    mapArrow.transform.localRotation = Quaternion.Euler(0, 0, -135);
                    break;
                case Direction.S:
                    mapArrow.transform.localRotation = Quaternion.Euler(0, 0, -180);
                    break;
                case Direction.SW:
                    mapArrow.transform.localRotation = Quaternion.Euler(0, 0, -225);
                    break;
                case Direction.W:
                    mapArrow.transform.localRotation = Quaternion.Euler(0, 0, -270);
                    break;
                case Direction.NW:
                    mapArrow.transform.localRotation = Quaternion.Euler(0, 0, -315);
                    break;
            }
        }
    }
}
