using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DDAInterop
{
    public struct CCalendar
    {
        public /*SeasonType*/int season;
        public IntPtr time;
        public int years;
        public int days;
        public /*MoonPhase*/int moon;
        public bool isNight;
    }
}
