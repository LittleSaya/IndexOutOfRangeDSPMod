using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DSPMutableGridAltitudeRange.Cache
{
    /// <summary>
    /// 游戏资源缓存
    /// </summary>
    static class ResourceCache
    {
        static public Font FontSAIRASB = null;

        static public Sprite SpriteSignal504 = null;

        static public Sprite SpriteRectP1 = null;

        static public Material MaterialBuildGridMat = null;

        static public void Initialize ()
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            foreach (var f in fonts)
            {
                if (f.name == "SAIRASB")
                {
                    FontSAIRASB = f;
                }
            }

            Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var s in sprites)
            {
                if (s.name == "signal-504")
                {
                    SpriteSignal504 = s;
                }
                else if (s.name == "rect_p1")
                {
                    SpriteRectP1 = s;
                }
            }

            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
            foreach (var m in materials)
            {
                if (m.name == "build-grid-mat")
                {
                    MaterialBuildGridMat = m;
                }
            }
        }
    }
}
