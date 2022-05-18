using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DSPWhiteBuildMode.Cache
{
    internal static class NativeObjectCache
    {
        static public GameObject CheckBox = null;

        static public GameObject FunctionPanel = null;

        static public void Initialize ()
        {
            if (!Common.UnityUtility.TryFindScene("DSPGame", out Scene scene))
            {
                Plugin.Logger.LogError("无法找到场景 DSPGame");
                return;
            }

            if (!Common.UnityUtility.TryFindRootGameObject(scene, "UI Root", out GameObject goUIRoot))
            {
                Plugin.Logger.LogError("无法在场景 DSPGame 中找到根游戏对象 UI Root");
                return;
            }

            CheckBox = goUIRoot.transform.Find("Overlay Canvas/Top Windows/Option Window/details/content-1/fullscreen/CheckBox").gameObject;
            if (CheckBox == null)
            {
                Plugin.Logger.LogError("无法在场景 DSPGame 中找到游戏对象 UI Root/Overlay Canvas/Top Windows/Option Window/details/content-1/fullscreen/CheckBox");
            }

            FunctionPanel = goUIRoot.transform.Find("Overlay Canvas/In Game/Function Panel").gameObject;
            if (FunctionPanel == null)
            {
                Plugin.Logger.LogError("无法在场景 DSPGame 中找到游戏对象 UI Root/Overlay Canvas/In Game/Function Panel");
            }
        }
    }
}
