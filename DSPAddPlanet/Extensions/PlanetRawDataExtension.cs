using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPAddPlanet.Extensions
{
    public static class PlanetRawDataExtension
    {
        private static readonly Dictionary<PlanetRawData, float> Radius = new Dictionary<PlanetRawData, float>();

        public static void AddRadius (this PlanetRawData planetRawData, PlanetData planet)
        {
            Radius[planetRawData] = planet.radius;
        }

        public static float GetRadius (this PlanetRawData planetRawData)
        {
            return Radius.TryGetValue(planetRawData, out var result) ? result : 200f;
        }

        public static int GetModPlaneInt (this PlanetRawData planetRawData, int index)
        {
            float baseHeight = 20;

            baseHeight += planetRawData.GetRadius() * 100;

            return (int)((planetRawData.modData[index >> 1] >> ((index & 1) << 2) + 2 & 3) * 133 + baseHeight);
        }
    }
}
