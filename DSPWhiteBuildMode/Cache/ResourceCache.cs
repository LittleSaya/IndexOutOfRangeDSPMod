using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DSPWhiteBuildMode.Cache
{
    internal static class ResourceCache
    {
        static public Sprite SpriteRectP1 = null;

        static public Font FontSAIRASB = null;

        static public void Initialize ()
        {
            Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite s in sprites)
            {
                if (s.name == "rect_p1")
                {
                    SpriteRectP1 = s;
                }
            }

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
