using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSPTransportStat.Extensions;

namespace DSPTransportStat
{
    struct StationInfoBundle
    {
        public StarData Star;

        public PlanetData Planet;

        public StationComponent Station;

        public StationInfoBundle (StarData star, PlanetData planet, StationComponent station)
        {
            Star = star;
            Planet = planet;
            Station = station;
        }

        static public int CompareByLocationAndNameASC (StationInfoBundle a, StationInfoBundle b)
        {
            int temp = a.Star.name.CompareTo(b.Star.name);
            if (temp > 0)
            {
                return 1;
            }
            else if (temp < 0)
            {
                return -1;
            }

            temp = a.Planet.name.CompareTo(b.Planet.name);
            if (temp > 0)
            {
                return 1;
            }
            else if (temp < 0)
            {
                return -1;
            }

            temp = a.Station.GetStationName().CompareTo(b.Station.GetStationName());
            return temp;
        }

        static public int CompareByLocationAndNameDESC (StationInfoBundle a, StationInfoBundle b)
        {
            return -CompareByLocationAndNameASC(a, b);
        }
    }
}
