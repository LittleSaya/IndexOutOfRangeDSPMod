using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DSPColorfulBuildingGrid.Cache
{
    /// <summary>
    /// 引用一些游戏中原本就有的对象，方便 mod 进行克隆
    /// </summary>
    internal static class NativeObjectCache
    {
        static public GameObject CheckBox = null;

        static public GameObject CloseBtn = null;

        static public GameObject FunctionPanel = null;

        static public GameObject Audio = null;

        static public UIBuildingGrid UIBuildingGrid = null;

        static public void Initialize ()
        {
            if (!Common.UnityUtility.TryFindScene("DSPGame", out Scene scene))
            {
                Plugin.Instance.Logger.LogError("无法找到场景 DSPGame");
                return;
            }

            if (!Common.UnityUtility.TryFindRootGameObject(scene, "UI Root", out GameObject goUIRoot))
            {
                Plugin.Instance.Logger.LogError("无法在场景 DSPGame 中找到根游戏对象 UI Root");
                return;
            }

            CheckBox = goUIRoot.transform.Find("Overlay Canvas/Top Windows/Option Window/details/content-1/fullscreen/CheckBox").gameObject;
            if (CheckBox == null)
            {
                Plugin.Instance.Logger.LogError("无法在场景 DSPGame 中找到游戏对象 UI Root/Overlay Canvas/Top Windows/Option Window/details/content-1/fullscreen/CheckBox");
            }

            CloseBtn = goUIRoot.transform.Find("Overlay Canvas/In Game/Windows/Player Inventory/panel-bg/btn-box/close-btn").gameObject;
            if (CloseBtn == null)
            {
                Plugin.Instance.Logger.LogError("无法在场景 DSPGame 中找到游戏对象 UI Root/Overlay Canvas/In Game/Windows/Player Inventory/panel-bg/btn-box/close-btn");
            }

            FunctionPanel = goUIRoot.transform.Find("Overlay Canvas/In Game/Function Panel").gameObject;
            if (FunctionPanel == null)
            {
                Plugin.Instance.Logger.LogError("无法在场景 DSPGame 中找到游戏对象 UI Root/Overlay Canvas/In Game/Function Panel");
            }

            Audio = goUIRoot.transform.Find("Overlay Canvas/Top Windows/Option Window/details/content-2/audio").gameObject;
            if (Audio == null)
            {
                Plugin.Instance.Logger.LogError("无法在场景 DSPGame 中找到游戏对象 UI Root/Overlay Canvas/Top Windows/Option Window/details/content-2/audio");
            }

            UIBuildingGrid = goUIRoot.transform.Find("Auxes/Build Grid").GetComponent<UIBuildingGrid>();
            if (UIBuildingGrid == null)
            {
                Plugin.Instance.Logger.LogError("无法在场景 DSPGame 中找到游戏对象 Auxes/Build Grid 的组件 UIBuildingGrid");
            }
        }
    }
}
