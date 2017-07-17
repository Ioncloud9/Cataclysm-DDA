using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DDAInterop
{
    public struct CGameData
    {
        public IVector3 playerPosition;
        public CCalendar calendar;
        public CWeather weather;
        public CMap map;
    }
}
