using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DSPTransportStat.CacheObjects
{
    /// <summary>
    /// 引用一些游戏中原本就有的对象，方便 mod 进行克隆
    /// </summary>
    static class NativeObjectCache
    {
        static public GameObject CheckBox { get; set; } = null;

        static public UIItemPicker UIItemPicker { get; set; } = null;

        static public GameObject StorageIconEmpty { get; set; } = null;

        static public GameObject ClearAchievementDataBtn { get; set; } = null;

        static public GameObject NameInput { get; set; } = null;

        static public GameObject SepLine1 { get; set; } = null;

        static public GameObject SepLine0 { get; set; } = null;

        static public GameObject SortBtn { get; set; } = null;

        static public GameObject CloseBtn { get; set; } = null;

        /// <summary>
        /// 初始化原生对象缓存
        /// </summary>
        /// <param name="missingInfo">缺失对象的信息</param>
        /// <returns>是否存在缺失的对象</returns>
        static public bool InitializeNativeObjectCache (out string missingInfo)
        {
            bool missingObjects = false;
            StringBuilder missingInfoSB = new StringBuilder();

            // 找到叫 DSPGame 的 scene
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            UnityEngine.SceneManagement.Scene dspGame = default;
            bool foundDspGame = false;
            for (int i = 0; i < sceneCount; ++i)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.name == "DSPGame")
                {
                    foundDspGame = true;
                    dspGame = scene;
                    break;
                }
            }

            if (!foundDspGame)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find scene named 'DSPGame'\r\n");
                missingInfo = missingInfoSB.ToString();
                return missingObjects;
            }

            // 找到叫 UI Root 的 GameObject
            GameObject uiRoot = null;
            bool foundUIRoot = false;
            GameObject[] dspGameRootGameObjects = dspGame.GetRootGameObjects();
            foreach (GameObject go in dspGameRootGameObjects)
            {
                if (go.name == "UI Root")
                {
                    uiRoot = go;
                    foundUIRoot = true;
                    break;
                }
            }

            if (!foundUIRoot)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'UI Root' in scene 'DSPGame'\r\n");
                missingInfo = missingInfoSB.ToString();
                return missingObjects;
            }

            // 找到 UI Root/Overlay Canvas/Top Windows/Option Window/details/content-1/fullscreen/CheckBox
            CheckBox = uiRoot.transform.Find("Overlay Canvas/Top Windows/Option Window/details/content-1/fullscreen/CheckBox").gameObject;
            
            if (CheckBox == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'UI Root/Overlay Canvas/Top Windows/Option Window/details/content-1/fullscreen/CheckBox' in scene 'DSPGame'");
            }

            // 找到 UI Root/Overlay Canvas/In Game/Common Tools/Item Picker 的 UIItemPicker 组件
            UIItemPicker = uiRoot.transform.Find("Overlay Canvas/In Game/Common Tools").GetComponent<UIItemPicker>();

            if (UIItemPicker == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find Component 'UIItemPicker' in GameObject 'UI Root/Overlay Canvas/In Game/Common Tools/Item Picker' in scene 'DSPGame'");
            }

            // 找到 UI Root/Overlay Canvas/In Game/Windows/Station Window/storage-box-0/storage-icon-empty
            StorageIconEmpty = uiRoot.transform.Find("Overlay Canvas/In Game/Windows/Station Window/storage-box-0/storage-icon-empty").gameObject;

            if (StorageIconEmpty == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'UI Root/Overlay Canvas/In Game/Windows/Station Window/storage-box-0/storage-icon-empty' in scene 'DSPGame'");
            }

            // 找到 UI Root/Overlay Canvas/Top Windows/Option Window/details/content-3/list/scroll-view/viewport/content/achievement-clear/clear-achievement-data-btn
            ClearAchievementDataBtn = uiRoot.transform.Find("Overlay Canvas/Top Windows/Option Window/details/content-3/list/scroll-view/viewport/content/achievement-clear/clear-achievement-data-btn").gameObject;

            if (ClearAchievementDataBtn == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'UI Root/Overlay Canvas/Top Windows/Option Window/details/content-3/list/scroll-view/viewport/content/achievement-clear/clear-achievement-data-btn' in scene 'DSPGame'");
            }

            // 找到 UI Root/Overlay Canvas/In Game/Windows/Station Window/name-input
            NameInput = uiRoot.transform.Find("Overlay Canvas/In Game/Windows/Station Window/name-input").gameObject;

            if (NameInput == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'UI Root/Overlay Canvas/In Game/Windows/Station Window/name-input' in scene DSPGame'");
            }

            // 找到 UI Root/Overlay Canvas/In Game/Windows/Statistics Window/product-bg/scroll-view/viewport/content/product-entry/sep-line-1
            SepLine1 = uiRoot.transform.Find("Overlay Canvas/In Game/Windows/Statistics Window/product-bg/scroll-view/viewport/content/product-entry/sep-line-1").gameObject;

            if (SepLine1 == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'UI Root/Overlay Canvas/In Game/Windows/Statistics Window/product-bg/scroll-view/viewport/content/product-entry/sep-line-1' in scene DSPGame");
            }

            // 找到 UI Root/Overlay Canvas/In Game/Windows/Statistics Window/product-bg/scroll-view/viewport/content/product-entry/sep-line-0
            SepLine0 = uiRoot.transform.Find("Overlay Canvas/In Game/Windows/Statistics Window/product-bg/scroll-view/viewport/content/product-entry/sep-line-0").gameObject;

            if (SepLine0 == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'UI Root/Overlay Canvas/In Game/Windows/Statistics Window/product-bg/scroll-view/viewport/content/product-entry/sep-line-0' in scene 'DSPGame'");
            }

            // 找到 UI Root/Overlay Canvas/In Game/Windows/Player Inventory/panel-bg/btn-box/sort-btn
            SortBtn = uiRoot.transform.Find("Overlay Canvas/In Game/Windows/Player Inventory/panel-bg/btn-box/sort-btn").gameObject;

            if (SortBtn == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'Root/Overlay Canvas/In Game/Windows/Player Inventory/panel-bg/btn-box/sort-btn' in scene 'DSPGame'");
            }

            // 找到 UI Root/Overlay Canvas/In Game/Windows/Player Inventory/panel-bg/btn-box/close-btn
            CloseBtn = uiRoot.transform.Find("Overlay Canvas/In Game/Windows/Player Inventory/panel-bg/btn-box/close-btn").gameObject;

            if (CloseBtn == null)
            {
                missingObjects = true;
                missingInfoSB.Append("Can not find GameObject 'Root/Overlay Canvas/In Game/Windows/Player Inventory/panel-bg/btn-box/close-btn' in scene 'DSPGame'");
            }

            missingInfo = missingInfoSB.ToString();
            return missingObjects;
        }
    }
}
