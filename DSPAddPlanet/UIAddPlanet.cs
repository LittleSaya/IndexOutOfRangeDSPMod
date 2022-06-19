using DSPAddPlanet.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DSPAddPlanet
{
    /// <summary>
    /// 备注：
    /// left-panel 中的内容在每次加载完毕存档时动态创建
    /// right-panel 中的内容在用户点选 left-panel 中的列表项时动态销毁和重建
    /// </summary>
    class UIAddPlanet : ManualBehaviour
    {
        public Text UniqueStarId { get; set; }

        public Text ExtraPlanetsInfo { get; set; }

        private GameObject goContentLeft = null;

        private GameObject goContentRight = null;

        private GameObject goStarPrefab = null;

        private UIStarmap uiStarmap = null;

        private string uniqueStarIdString = "";

        static public UIAddPlanet Create ()
        {
            GameObject goTutorialWindow = UIRoot.instance.transform.Find("Overlay Canvas/In Game/Windows/Tutorial Window").gameObject;
            GameObject goAddPlanet = Instantiate(goTutorialWindow, goTutorialWindow.transform.parent);
            goAddPlanet.name = "Add Planet";

            // 添加 UIAddPlanet 组件
            UIAddPlanet uiAddPlanet = goAddPlanet.AddComponent<UIAddPlanet>();
            uiAddPlanet._Create();

            // 销毁不需要的子对象
            Destroy(goAddPlanet.transform.Find("video-camera").gameObject);
            Destroy(goAddPlanet.transform.Find("video-player").gameObject);

            // 销毁左侧面板中所有的内容
            uiAddPlanet.goContentLeft = goAddPlanet.transform.Find("left-panel/ListView/Mask/Content Panel").gameObject;
            for (int i = 0; i < uiAddPlanet.goContentLeft.transform.childCount; ++i)
            {
                Destroy(uiAddPlanet.goContentLeft.transform.GetChild(i).gameObject);
            }
            GameObject goLeftListView = goAddPlanet.transform.Find("left-panel/ListView").gameObject;
            Destroy(goLeftListView.GetComponent<UIListView>());

            // 左侧显示滚动条
            goLeftListView.GetComponent<ScrollRect>().vertical = true;

            // 销毁右侧面板中所有的内容
            uiAddPlanet.goContentRight = goAddPlanet.transform.Find("right-panel/Scroll View/Viewport/Content").gameObject;
            for (int i = 0; i < uiAddPlanet.goContentRight.transform.childCount; ++i)
            {
                Destroy(uiAddPlanet.goContentRight.transform.GetChild(i).gameObject);
            }

            // 删除原有的 UITutorialWindow
            Destroy(goAddPlanet.GetComponent<UITutorialWindow>());

            // 修改标题
            GameObject goTitleText = goAddPlanet.transform.Find("panel-bg/title-text").gameObject;
            Destroy(goTitleText.GetComponent<Localizer>());
            goTitleText.GetComponent<Text>().text = "Add Planet";

            // 添加窗口关闭事件
            Button cmpCloseBtn = goAddPlanet.transform.Find("panel-bg/close-btn").GetComponent<Button>();
            cmpCloseBtn.onClick.RemoveAllListeners();
            cmpCloseBtn.onClick.AddListener(uiAddPlanet._Close);

            // 预先通过 item-prefab 创建 star-prefab
            GameObject goItemPrefab = UIRoot.instance.transform.Find("Overlay Canvas/In Game/Windows/Tutorial Window/left-panel/ListView/Mask/Content Panel/item-prefab").gameObject;
            GameObject goStarPrefab = Instantiate(goItemPrefab, uiAddPlanet.goContentLeft.transform);
            goStarPrefab.name = "star-prefab";
            Destroy(goStarPrefab.GetComponent<UITutorialListEntry>());
            goStarPrefab.GetComponent<Button>().onClick.RemoveAllListeners();
            uiAddPlanet.goStarPrefab = goStarPrefab;

            // 获取 UIStarmap 的引用
            uiAddPlanet.uiStarmap = UIRoot.instance.transform.Find("Overlay Canvas/In Game/Starmap UIs").GetComponent<UIStarmap>();

            // 在右侧内容面板中创建文本组件
            GameObject goUniqueStarId = UIUtility.CreateText("unique-star-id", uiAddPlanet.goContentRight.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -22), new Vector2(210, 0));
            uiAddPlanet.UniqueStarId = goUniqueStarId.GetComponent<Text>();
            uiAddPlanet.UniqueStarId.alignment = TextAnchor.MiddleCenter;
            GameObject goExtraPlanetsInfo = UIUtility.CreateText("extra-planets-info", uiAddPlanet.goContentRight.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -400), new Vector2(0, -26));
            uiAddPlanet.ExtraPlanetsInfo = goExtraPlanetsInfo.GetComponent<Text>();

            // 创建一个复制唯一恒星编号的按钮
            UIUtility.CreateTextButton(
                "Copy",
                () =>
                {
                    GUIUtility.systemCopyBuffer = uiAddPlanet.uniqueStarIdString;
                },
                "copy-unique-star-id",
                uiAddPlanet.goContentRight.transform,
                new Vector2(0, 1),
                new Vector2(0, 1),
                new Vector2(210, -22),
                new Vector2(260, 0)
            );

            return uiAddPlanet;
        }

        private void SelectStar (StarData star)
        {
            // 唯一恒星编号
            if (string.IsNullOrWhiteSpace(GameMain.gameName))
            {
                uniqueStarIdString = UniqueStarId.text = Utility.UniqueStarIdWithoutGameName(GameMain.data.gameDesc.clusterString, star.name);
            }
            else
            {
                uniqueStarIdString = UniqueStarId.text = Utility.UniqueStarIdWithGameName(GameMain.gameName, GameMain.data.gameDesc.clusterString, star.name);
            }

            // 额外行星信息
            StringBuilder extraPlanetsInfo = new StringBuilder();
            foreach (PlanetData planet in star.planets)
            {
                extraPlanetsInfo
                    .Append(planet.name)
                    .Append(": ")
                    .Append("index=")
                    .Append(planet.index)
                    .Append(", orbitAround=")
                    .Append(planet.orbitAround)
                    .Append(", orbitIndex=")
                    .Append(planet.orbitIndex)
                    .Append(", number=")
                    .Append(planet.number)
                    .Append(", gasGiant=")
                    .Append(planet.type == EPlanetType.Gas)
                    .Append(", info_seed=")
                    .Append(planet.infoSeed)
                    .Append(", gen_seed=")
                    .Append(planet.seed)
                    .Append("\r\n");
            }
            ExtraPlanetsInfo.text = extraPlanetsInfo.ToString();
        }

        /// <summary>
        /// 打开窗口时，动态生成左侧的恒星列表
        /// 
        /// 备注：左侧列表中列表项的大小和位置似乎不是在这里用 RectTransform 直接控制的
        /// </summary>
        protected override void _OnOpen ()
        {
            // 删除除了 star-prefab 之外的所有子对象
            for (int i = 0; i < goContentLeft.transform.childCount; ++i)
            {
                GameObject child = goContentLeft.transform.GetChild(i).gameObject;
                if (child.name != "star-prefab")
                {
                    Destroy(child);
                }
            }

            // 在最上方添加用户当前聚焦的恒星
            StarData viewStar = uiStarmap.viewStarSystem;
            if (viewStar != null)
            {
                GameObject go = Instantiate(goStarPrefab, goContentLeft.transform);
                go.name = viewStar.name + " (Current)";
                go.SetActive(true);

                // 设置列表项的名称（恒星名称）
                go.transform.Find("name-text").GetComponent<Text>().text = viewStar.name + " (Current)";

                // 设置列表项的位置
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.Zeroize();
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.offsetMax = new Vector2(226, -4);
                rect.offsetMin = new Vector2(4, -28);

                // 点击事件
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectStar(viewStar);
                });
            }
            else
            {
                GameObject go = Instantiate(goStarPrefab, goContentLeft.transform);
                go.name = "N/A";
                go.SetActive(true);

                // 设置列表项的名称（恒星名称）
                go.transform.Find("name-text").GetComponent<Text>().text = "N/A (Current)";

                // 设置列表项的位置
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.Zeroize();
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.offsetMax = new Vector2(226, -4);
                rect.offsetMin = new Vector2(4, -28);
            }

            // 列出所有恒星
            for (int i = 0; i < GameMain.galaxy.stars.Length; ++i)
            {
                StarData star = GameMain.galaxy.stars[i];

                GameObject go = Instantiate(goStarPrefab, goContentLeft.transform);
                go.name = star.name;
                go.SetActive(true);

                // 设置列表项的名称（恒星名称）
                go.transform.Find("name-text").GetComponent<Text>().text = star.name;

                // 设置列表项的位置
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.Zeroize();
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.offsetMax = new Vector2(226, -4 - (i + 1) * 26);
                rect.offsetMin = new Vector2(4, -28 - (i + 1) * 26);

                // 点击事件
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectStar(star);
                });
            }

            // 调整左侧内容区域的高度
            RectTransform rectLeft = goContentLeft.GetComponent<RectTransform>();
            rectLeft.Zeroize();
            rectLeft.anchorMin = rectLeft.anchorMax = new Vector2(0, 1);
            rectLeft.offsetMax = new Vector2(230, 0);
            rectLeft.offsetMin = new Vector2(0, -4 - (GameMain.galaxy.stars.Length + 1) * 26);
        }

        protected override bool _OnInit ()
        {
            return true;
        }
    }
}
