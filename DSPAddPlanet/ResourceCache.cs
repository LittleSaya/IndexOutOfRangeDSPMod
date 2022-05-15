using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DSPAddPlanet
{
    static class ResourceCache
    {
        static public Font FontSAIRASB = null;

        static public void InitializeResourceCache ()
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            for (int i = 0; i < fonts.Length; ++i)
            {
                if (fonts[i].name == "SAIRASB")
                {
                    FontSAIRASB = fonts[i];
                }
            }
        }
    }
}
