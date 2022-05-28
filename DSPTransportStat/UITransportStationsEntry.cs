using DSPTransportStat.CacheObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DSPTransportStat.Extensions;
using DSPTransportStat.Translation;
using DSPTransportStat.Global;
using System.Reflection;

namespace DSPTransportStat
{
    /// <summary>
    /// 物流站点窗口中的列表项，每一个 TransportStationsEntry 对应一个预先创建好的 transport-stations-entry GameObject ，通过对 UI 组件的引用来控制显示的内容，其具体位置在外部（ UITrasnportStationsWindow ）通过虚拟滚动进行控制
    /// 
    /// 主要功能是根据传入的 StationComponent 更新 UI 组件中显示的内容
    /// </summary>
    class UITransportStationsEntry : MonoBehaviour
    {
        // 常量
        static readonly public Color PositiveColor = new Color(0.3804f, 0.8471f, 1f, 0.698f);

        static readonly public Color NegativeColor = new Color(0.9922f, 0.5882f, 0.3686f, 0.698f);

        static readonly public Color UsageStorageColor = new Color(0.6981f, 0.6981f, 0.6981f, 1); // old: rgb(0.6981f, 0.6981f, 0.6981f, 1) rgb(178, 178, 178, 255)

        static readonly public Color UsageDemandColor = new Color(0.9373f, 0.5476f, 0.3683f, 1); // old: rgb(0.8774f, 0.5476f, 0.3683f, 1) rgb(224, 140, 94, 255)
        
        static readonly public Color UsageSupplyColor = new Color(0.2353f, 0.5451f, 0.7882f, 1); // old: rgb(0.2353f, 0.5451f, 0.651f, 1) rgb(60, 139, 166, 255)

        /// <summary>
        /// 相关对象的引用，由 UITransportStationsEntry.Create 内部初始化
        /// </summary>
        public RectTransform RectTransform { get; set; } = null;

        /// <summary>
        /// 站点信息，由外部赋值
        /// </summary>
        public StationComponent StationComponent { get; set; } = null;

        /// <summary>
        /// 恒星信息，由外部赋值
        /// </summary>
        public StarData StarData { get; set; } = null;

        /// <summary>
        /// 行星信息，由外部赋值
        /// </summary>
        public PlanetData PlanetData { get; set; } = null;

        // UI控件
        public Text Star { get; set; } = null;

        public Text Planet { get; set; } = null;

        public Text StationType { get; set; } = null;

        public Text Name { get; set; } = null;

        public UIItem[] UIItems { get; set; } = null;

        public void Update ()
        {
            // 由外层的 UITransportStationsWindow 来控制传入的 StationComponent
            //if (StationComponent == null || StationComponent.entityId == 0)
            //{
            //    return;
            //}

            Star.text = StarData.displayName;

            Planet.text = PlanetData.displayName;

            if (StationComponent.isCollector)
            {
                if (StationComponent.isStellar)
                {
                    StationType.text = Strings.Common.StationType.InterstellarCollector;
                }
                else
                {
                    StationType.text = Strings.Common.StationType.InPlanetCollector;
                }
            }
            else
            {
                if (StationComponent.isStellar)
                {
                    StationType.text = Strings.Common.StationType.Interstrllar;
                }
                else
                {
                    StationType.text = Strings.Common.StationType.InPlanet;
                }
            }

            // StationComponent 的 name 字段不一定有值，没有值的情况下使用 gid 和 id 拼凑名称
            //Name.text = StationComponent.name;
            if (string.IsNullOrEmpty(StationComponent.name))
            {
                if (StationComponent.isStellar)
                {
                    Name.text = "星际站点号".Translate() + StationComponent.gid.ToString();
                }
                else
                {
                    Name.text = "本地站点号".Translate() + StationComponent.id.ToString();
                }
            }
            else
            {
                Name.text = StationComponent.name;
            }

            for (int i = 0; i < Constants.TRANSPORT_STATIONS_ENTRY_DEFAULT_ITEM_SLOT_NUMBER; ++i)
            {
                if (i >= StationComponent.storage.Length)
                {
                    // 该物品槽位不存在
                    UIItems[i].gameObject.SetActive(false);
                }
                else
                {
                    // 物品槽位存在，具体如何显示交给下面处理，包括槽位中没有设置物品的情况
                    UIItems[i].gameObject.SetActive(true);
                    UIItems[i].StationStore = StationComponent.storage[i];
                }
            }
        }

        static public UITransportStationsEntry Create ()
        {
            // 创建 transport-stations-entry
            GameObject goTSE = new GameObject("transport-stations-entry", typeof(RectTransform), typeof(UITransportStationsEntry));
            UITransportStationsEntry uiTSE = goTSE.GetComponent<UITransportStationsEntry>();

            // 将位置和大小归零
            RectTransform cmpRectTransform = goTSE.GetComponent<RectTransform>();
            cmpRectTransform.Zeroize();

            // 给每一个列表项保留一个 RectTransform 的引用，便于外部控制列表项的位置和大小
            uiTSE.RectTransform = cmpRectTransform;

            // 创建 transport-stations-entry > star
            GameObject childStar = new GameObject("star", typeof(RectTransform));
            childStar.transform.SetParent(goTSE.transform);

            RectTransform childStar_cmpRectTransform = childStar.GetComponent<RectTransform>();
            childStar_cmpRectTransform.Zeroize();
            childStar_cmpRectTransform.anchorMax = childStar_cmpRectTransform.anchorMin = new Vector2(0, 1);
            childStar_cmpRectTransform.offsetMax = childStar_cmpRectTransform.offsetMin = new Vector2(10, -10);

            Text childStar_cmpText = childStar.AddComponent<Text>();
            childStar_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            childStar_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            childStar_cmpText.font = ResourceCache.FontSAIRASB;
            childStar_cmpText.text = "STAR_NAME";

            uiTSE.Star = childStar_cmpText;

            // 创建 transport-stations-entry > planet
            GameObject childPlanet = Instantiate(childStar, goTSE.transform);
            childPlanet.name = "planet";

            RectTransform childPlanet_cmpRectTransform = childPlanet.GetComponent<RectTransform>();
            childPlanet_cmpRectTransform.Zeroize();
            childPlanet_cmpRectTransform.anchorMax = childPlanet_cmpRectTransform.anchorMin = new Vector2(0, 1);
            childPlanet_cmpRectTransform.offsetMax = childPlanet_cmpRectTransform.offsetMin = new Vector2(10, -35);

            Text childPlanet_cmpText = childPlanet.GetComponent<Text>();
            childPlanet_cmpText.text = "PLANET_NAME";

            uiTSE.Planet = childPlanet_cmpText;

            // 创建 transport-stations-entry > station-type
            GameObject childStationType = Instantiate(childStar, goTSE.transform);
            childStationType.name = "station-type";

            RectTransform childStationType_cmpRectTransform = childStationType.GetComponent<RectTransform>();
            childStationType_cmpRectTransform.Zeroize();
            childStationType_cmpRectTransform.anchorMax = childStationType_cmpRectTransform.anchorMin = new Vector2(0, 1);
            childStationType_cmpRectTransform.offsetMax = childStationType_cmpRectTransform.offsetMin = new Vector2(10, -60);

            Text childStationType_cmpText = childStationType.GetComponent<Text>();
            childStationType_cmpText.text = "STATION_TYPE";

            uiTSE.StationType = childStationType_cmpText;

            // 创建 transport-stations-entry > name
            GameObject childName = Instantiate(childStar, goTSE.transform);
            childName.name = "name";

            RectTransform childName_cmpRectTransform = childName.GetComponent<RectTransform>();
            childName_cmpRectTransform.Zeroize();
            childName_cmpRectTransform.anchorMax = childName_cmpRectTransform.anchorMin = new Vector2(0, 1);
            childName_cmpRectTransform.offsetMax = childName_cmpRectTransform.offsetMin = new Vector2(10, -85);

            Text childName_cmpText = childName.GetComponent<Text>();
            childName_cmpText.text = "STATION_NAME";

            uiTSE.Name = childName_cmpText;

            // 创建 transport-stations-entry > sep-line-1 和 transport-stations-entry > sep-line-0
            GameObject childSepLine1 = Instantiate(NativeObjectCache.SepLine1, goTSE.transform);

            RectTransform childSepLine1_cmpRectTransform = childSepLine1.GetComponent<RectTransform>();
            childSepLine1_cmpRectTransform.Zeroize();
            childSepLine1_cmpRectTransform.anchorMax = new Vector2(1, 0);
            childSepLine1_cmpRectTransform.offsetMax = new Vector2(-1, 1);
            childSepLine1_cmpRectTransform.offsetMin = new Vector2(1, 0);

            GameObject childSepLine0 = Instantiate(NativeObjectCache.SepLine0, goTSE.transform);

            RectTransform childSepLine0_cmpRectTransform = childSepLine0.GetComponent<RectTransform>();
            childSepLine0_cmpRectTransform.Zeroize();
            childSepLine0_cmpRectTransform.anchorMax = new Vector2(1, 0);
            childSepLine0_cmpRectTransform.offsetMax = new Vector2(-1, 0);
            childSepLine0_cmpRectTransform.offsetMin = new Vector2(1, -1);

            // 创建 transport-stations-entry > config-button
            GameObject childConfigButton = UIUtility.CreateImageButton("config-button", ResourceCache.SpriteSignal504, () =>
            {
                Plugin.Patch_UIStationWindow.OpenStationWindowOfAnyStation(uiTSE.PlanetData.factory, uiTSE.StationComponent.gene);
            });
            childConfigButton.transform.SetParent(uiTSE.transform);

            RectTransform childConfigButton_cmpRectTransform = childConfigButton.GetComponent<RectTransform>();
            childConfigButton_cmpRectTransform.Zeroize();
            childConfigButton_cmpRectTransform.anchorMax = childConfigButton_cmpRectTransform.anchorMin = new Vector2(0, 1);
            childConfigButton_cmpRectTransform.offsetMax = new Vector2(145, -5);
            childConfigButton_cmpRectTransform.offsetMin = new Vector2(115, -35);

            // 创建 transport-stations-entry > items
            GameObject childItems = new GameObject("items", typeof(RectTransform));
            childItems.transform.SetParent(uiTSE.transform);

            RectTransform childItems_cmpRectTransform = childItems.GetComponent<RectTransform>();
            childItems_cmpRectTransform.Zeroize();
            childItems_cmpRectTransform.anchorMax = childItems_cmpRectTransform.anchorMin = new Vector2(0, 1);
            childItems_cmpRectTransform.offsetMax = new Vector2(150, 0);
            childItems_cmpRectTransform.offsetMin = new Vector2(150, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

            uiTSE.UIItems = new UIItem[Constants.TRANSPORT_STATIONS_ENTRY_DEFAULT_ITEM_SLOT_NUMBER];
            for (int i = 0; i < uiTSE.UIItems.Length; ++i)
            {
                uiTSE.UIItems[i] = UIItem.Create();
                uiTSE.UIItems[i].transform.SetParent(childItems.transform);

                RectTransform childItems_childItem_cmpRectTransform = uiTSE.UIItems[i].GetComponent<RectTransform>();
                childItems_childItem_cmpRectTransform.Zeroize();
                childItems_childItem_cmpRectTransform.anchorMin = childItems_childItem_cmpRectTransform.anchorMax = new Vector2(0, 1);
                childItems_childItem_cmpRectTransform.offsetMin = new Vector2(i * Constants.TRANSPORT_STATIONS_ENTRY_ITEM_SLOT_WIDTH, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT); // X：物品槽位左边缘的位置，Y：物品槽位下边缘的位置（与列表项的高度相等）
                childItems_childItem_cmpRectTransform.offsetMax = new Vector2(i * Constants.TRANSPORT_STATIONS_ENTRY_ITEM_SLOT_WIDTH + Constants.TRANSPORT_STATIONS_ENTRY_ITEM_SLOT_WIDTH, 0); // X：物品槽位右边缘的位置，Y：物品槽位上边缘的位置
            }

            return uiTSE;
        }

        public class UIItem : MonoBehaviour
        {
            public _UIHomePanel UIHomePanel { get; set; }

            public _DisplayMode DisplayMode { get; set; }

            public StationStore StationStore { get; set; }

            private void Awake ()
            {
                DisplayMode = _DisplayMode.Home;
            }

            private void Update ()
            {
                if (StationStore.itemId == 0)
                {
                    // 槽位中没有设置物品
                    UIHomePanel.ItemNotSet();
                    return;
                }

                UIHomePanel.ItemSet();
                
                // 设置 UIHomePanel 的属性
                UIHomePanel.Icon.sprite = LDB.items.Select(StationStore.itemId).iconSprite;
                UIHomePanel.CurrentAmount.text = StationStore.GetCountAsString();
                int totalOrder = StationStore.totalOrdered;
                if (totalOrder == 0)
                {
                    UIHomePanel.OrderAmount.text = Strings.Common.NoOrder;
                    UIHomePanel.OrderAmount.color = Color.white;
                    UIHomePanel.OrderAmount.material = ResourceCache.MaterialDefaultUIMaterial;
                }
                else if (totalOrder < 0)
                {
                    UIHomePanel.OrderAmount.text = $"{totalOrder}";
                    UIHomePanel.OrderAmount.color = NegativeColor;
                    UIHomePanel.OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }
                else
                {
                    UIHomePanel.OrderAmount.text = $"+{totalOrder}";
                    UIHomePanel.OrderAmount.color = PositiveColor;
                    UIHomePanel.OrderAmount.material = ResourceCache.MaterialWidgetTextAlpha5x;
                }

                UIHomePanel.MaxAmount.text = StationStore.GetMaxAsString();

                UIHomePanel.InPlanetStorageUsage.text = StationStore.GetLocalLogicAsString();
                if (StationStore.localLogic == ELogisticStorage.None)
                {
                    UIHomePanel.InPlanetStorageUsage.color = UsageStorageColor;
                }
                else if (StationStore.localLogic == ELogisticStorage.Supply)
                {
                    UIHomePanel.InPlanetStorageUsage.color = UsageSupplyColor;
                }
                else if (StationStore.localLogic == ELogisticStorage.Demand)
                {
                    UIHomePanel.InPlanetStorageUsage.color = UsageDemandColor;
                }

                UIHomePanel.InterstellarStorageUsage.text = StationStore.GetRemoteLogicAsString();
                if (StationStore.remoteLogic == ELogisticStorage.None)
                {
                    UIHomePanel.InterstellarStorageUsage.color = UsageStorageColor;
                }
                else if (StationStore.remoteLogic == ELogisticStorage.Supply)
                {
                    UIHomePanel.InterstellarStorageUsage.color = UsageSupplyColor;
                }
                else if (StationStore.remoteLogic == ELogisticStorage.Demand)
                {
                    UIHomePanel.InterstellarStorageUsage.color = UsageDemandColor;
                }
            }

            static public UIItem Create ()
            {
                GameObject goItem = new GameObject("item", typeof(RectTransform), typeof(UIItem));
                UIItem uiItem = goItem.GetComponent<UIItem>();

                // 创建 home-panel
                uiItem.UIHomePanel = _UIHomePanel.Create();
                uiItem.UIHomePanel.transform.SetParent(uiItem.transform);

                RectTransform childHomePanel_cmpRectTransform = uiItem.UIHomePanel.GetComponent<RectTransform>();
                childHomePanel_cmpRectTransform.Zeroize();
                childHomePanel_cmpRectTransform.anchorMin = childHomePanel_cmpRectTransform.anchorMax = new Vector2(0, 1);
                childHomePanel_cmpRectTransform.offsetMin = new Vector2(0, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);
                childHomePanel_cmpRectTransform.offsetMax = new Vector2(Constants.TRANSPORT_STATIONS_ENTRY_ITEM_SLOT_WIDTH, 0);

                return uiItem;
            }

            public class _UIHomePanel : MonoBehaviour
            {
                public Image Icon { get; set; } = null;

                public Text CurrentLabel { get; set; } = null;

                public Text CurrentAmount { get; set; } = null;

                public Text OrderAmount { get; set; } = null;

                public Text MaxLabel { get; set; } = null;

                public Text MaxAmount { get; set; } = null;

                public Text InPlanetStorageUsage { get; set; } = null;

                public Text InterstellarStorageUsage { get; set; } = null;

                static public _UIHomePanel Create ()
                {
                    GameObject goHomePanel = new GameObject("home-panel", typeof(RectTransform), typeof(_UIHomePanel));
                    _UIHomePanel uiHomePanel = goHomePanel.GetComponent<_UIHomePanel>();

                    // 创建 home-panel > icon-bg
                    GameObject goHomePanel_childIconBg = new GameObject("icon-bg", typeof(RectTransform), typeof(Image));
                    goHomePanel_childIconBg.transform.SetParent(goHomePanel.transform);

                    RectTransform goHomePanel_childIconBg_cmpRectTransform = goHomePanel_childIconBg.GetComponent<RectTransform>();
                    goHomePanel_childIconBg_cmpRectTransform.Zeroize();
                    goHomePanel_childIconBg_cmpRectTransform.anchorMax = goHomePanel_childIconBg_cmpRectTransform.anchorMin = new Vector2(0, 1);
                    goHomePanel_childIconBg_cmpRectTransform.offsetMax = new Vector2(55, -5);
                    goHomePanel_childIconBg_cmpRectTransform.offsetMin = new Vector2(5, -55);

                    Image goHomePanel_childIconBg_cmpImage = goHomePanel_childIconBg.GetComponent<Image>();
                    goHomePanel_childIconBg_cmpImage.sprite = ResourceCache.SpriteRound256;
                    goHomePanel_childIconBg_cmpImage.color = new Color(0, 0, 0, 0.5f);

                    // 创建 home-panel > icon
                    GameObject goHomePanel_childIcon = Instantiate(goHomePanel_childIconBg, uiHomePanel.transform);
                    goHomePanel_childIcon.name = "icon";

                    RectTransform goHomePanel_childIcon_cmpRectTransform = goHomePanel_childIcon.GetComponent<RectTransform>();
                    goHomePanel_childIcon_cmpRectTransform.offsetMax = new Vector2(50, -10);
                    goHomePanel_childIcon_cmpRectTransform.offsetMin = new Vector2(10, -50);

                    Image goHomePanel_childIcon_cmpImage = goHomePanel_childIcon.GetComponent<Image>();
                    goHomePanel_childIcon_cmpImage.color = Color.white;

                    uiHomePanel.Icon = goHomePanel_childIcon_cmpImage;

                    // 创建 home-panel > current-label
                    GameObject goHomePanel_childCurrentLabel = new GameObject("current-label", typeof(RectTransform), typeof(Text));
                    goHomePanel_childCurrentLabel.transform.SetParent(goHomePanel.transform);

                    RectTransform goHomePanel_childCurrentLabel_cmpRectTransform = goHomePanel_childCurrentLabel.GetComponent<RectTransform>();
                    goHomePanel_childCurrentLabel_cmpRectTransform.Zeroize();
                    goHomePanel_childCurrentLabel_cmpRectTransform.anchorMax = goHomePanel_childCurrentLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
                    goHomePanel_childCurrentLabel_cmpRectTransform.offsetMin = goHomePanel_childCurrentLabel_cmpRectTransform.offsetMax = new Vector2(60, -10);

                    Text goHomePanel_childCurrentLabel_cmpText = goHomePanel_childCurrentLabel.GetComponent<Text>();
                    goHomePanel_childCurrentLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    goHomePanel_childCurrentLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
                    goHomePanel_childCurrentLabel_cmpText.font = ResourceCache.FontSAIRASB;
                    goHomePanel_childCurrentLabel_cmpText.text = Strings.TransportStationsWindow.CurrentLabel;

                    uiHomePanel.CurrentLabel = goHomePanel_childCurrentLabel_cmpText;

                    // 创建 home-panel > current-amount
                    GameObject goHomePanel_childCurrentAmount = Instantiate(goHomePanel_childCurrentLabel, goHomePanel.transform);
                    goHomePanel_childCurrentAmount.name = "current-amount";

                    RectTransform goHomePanel_childCurrentAmount_cmpRectTransform = goHomePanel_childCurrentAmount.GetComponent<RectTransform>();
                    goHomePanel_childCurrentAmount_cmpRectTransform.Zeroize();
                    goHomePanel_childCurrentAmount_cmpRectTransform.anchorMax = goHomePanel_childCurrentAmount_cmpRectTransform.anchorMin = new Vector2(0, 1);
                    goHomePanel_childCurrentAmount_cmpRectTransform.offsetMin = goHomePanel_childCurrentAmount_cmpRectTransform.offsetMax = new Vector2(110, -10);

                    Text goHomePanel_childCurrentAmount_cmpText = goHomePanel_childCurrentAmount.GetComponent<Text>();
                    goHomePanel_childCurrentAmount_cmpText.text = "000000";

                    uiHomePanel.CurrentAmount = goHomePanel_childCurrentAmount_cmpText;

                    // 创建 home-panel > order-amount
                    GameObject goHomePanel_childOrderAmount = Instantiate(goHomePanel_childCurrentLabel, goHomePanel.transform);
                    goHomePanel_childOrderAmount.name = "order-amount";

                    RectTransform goHomePanel_childOrderAmount_cmpRectTransform = goHomePanel_childOrderAmount.GetComponent<RectTransform>();
                    goHomePanel_childOrderAmount_cmpRectTransform.Zeroize();
                    goHomePanel_childOrderAmount_cmpRectTransform.anchorMax = goHomePanel_childOrderAmount_cmpRectTransform.anchorMin = new Vector2(0, 1);
                    goHomePanel_childOrderAmount_cmpRectTransform.offsetMin = goHomePanel_childOrderAmount_cmpRectTransform.offsetMax = new Vector2(160, -10);

                    Text goHomePanel_childOrderAmount_cmpText = goHomePanel_childOrderAmount.GetComponent<Text>();
                    goHomePanel_childOrderAmount_cmpText.text = "+000000";

                    uiHomePanel.OrderAmount = goHomePanel_childOrderAmount_cmpText;

                    // 创建 home-panel > max-label
                    GameObject goHomePanel_childMaxLabel = Instantiate(goHomePanel_childCurrentLabel, goHomePanel.transform);
                    goHomePanel_childMaxLabel.name = "max-label";

                    RectTransform goHomePanel_childMaxLabel_cmpRectTransform = goHomePanel_childMaxLabel.GetComponent<RectTransform>();
                    goHomePanel_childMaxLabel_cmpRectTransform.Zeroize();
                    goHomePanel_childMaxLabel_cmpRectTransform.anchorMax = goHomePanel_childMaxLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
                    goHomePanel_childMaxLabel_cmpRectTransform.offsetMin = goHomePanel_childMaxLabel_cmpRectTransform.offsetMax = new Vector2(60, -35);

                    Text goHomePanel_childMaxLabel_cmpText = goHomePanel_childMaxLabel.GetComponent<Text>();
                    goHomePanel_childMaxLabel_cmpText.text = Strings.TransportStationsWindow.MaxLabel;

                    uiHomePanel.MaxLabel = goHomePanel_childMaxLabel_cmpText;

                    // 创建 home-panel > max-amount
                    GameObject goHomePanel_childMaxAmount = Instantiate(goHomePanel_childCurrentLabel, goHomePanel.transform);
                    goHomePanel_childMaxAmount.name = "max-amount";

                    RectTransform goHomePanel_childMaxAmount_cmpRectTransform = goHomePanel_childMaxAmount.GetComponent<RectTransform>();
                    goHomePanel_childMaxAmount_cmpRectTransform.Zeroize();
                    goHomePanel_childMaxAmount_cmpRectTransform.anchorMax = goHomePanel_childMaxAmount_cmpRectTransform.anchorMin = new Vector2(0, 1);
                    goHomePanel_childMaxAmount_cmpRectTransform.offsetMin = goHomePanel_childMaxAmount_cmpRectTransform.offsetMax = new Vector2(110, -35);

                    Text goHomePanel_childMaxAmount_cmpText = goHomePanel_childMaxAmount.GetComponent<Text>();
                    goHomePanel_childMaxAmount_cmpText.text = "000000";

                    uiHomePanel.MaxAmount = goHomePanel_childMaxAmount_cmpText;

                    // 创建 home-panel > in-planet-storage-usage
                    GameObject goHomePanel_childInPlanetStorageUsage = Instantiate(goHomePanel_childCurrentLabel, goHomePanel.transform);
                    goHomePanel_childInPlanetStorageUsage.name = "in-planet-storage-usage";

                    RectTransform goHomePanel_childInPlanetStorageUsage_cmpRectTransform = goHomePanel_childInPlanetStorageUsage.GetComponent<RectTransform>();
                    goHomePanel_childInPlanetStorageUsage_cmpRectTransform.Zeroize();
                    goHomePanel_childInPlanetStorageUsage_cmpRectTransform.anchorMax = goHomePanel_childInPlanetStorageUsage_cmpRectTransform.anchorMin = new Vector2(0, 1);
                    goHomePanel_childInPlanetStorageUsage_cmpRectTransform.offsetMax = goHomePanel_childInPlanetStorageUsage_cmpRectTransform.offsetMin = new Vector2(10, -60);

                    Text goHomePanel_childInPlanetStorageUsage_cmpText = goHomePanel_childInPlanetStorageUsage.GetComponent<Text>();
                    goHomePanel_childInPlanetStorageUsage_cmpText.text = "IN_PLANET_STORAGE_USAGE";

                    uiHomePanel.InPlanetStorageUsage = goHomePanel_childInPlanetStorageUsage_cmpText;

                    // 创建 home-panel > interstellar-storage-usage
                    GameObject goHomePanel_childInterstellarStorageUsage = Instantiate(goHomePanel_childCurrentLabel, goHomePanel.transform);
                    goHomePanel_childInterstellarStorageUsage.name = "interstellar-storage-usage";

                    RectTransform goHomePanel_childInterstellarStorageUsage_cmpRectTransform = goHomePanel_childInterstellarStorageUsage.GetComponent<RectTransform>();
                    goHomePanel_childInterstellarStorageUsage_cmpRectTransform.Zeroize();
                    goHomePanel_childInterstellarStorageUsage_cmpRectTransform.anchorMax = goHomePanel_childInterstellarStorageUsage_cmpRectTransform.anchorMin = new Vector2(0, 1);
                    goHomePanel_childInterstellarStorageUsage_cmpRectTransform.offsetMax = goHomePanel_childInterstellarStorageUsage_cmpRectTransform.offsetMin = new Vector2(10, -85);

                    Text goHomePanel_childInterstellarStorageUsage_cmpText = goHomePanel_childInterstellarStorageUsage.GetComponent<Text>();
                    goHomePanel_childInterstellarStorageUsage_cmpText.text = "INTERSTELLAR_STORAGE_USAGE";

                    uiHomePanel.InterstellarStorageUsage = goHomePanel_childInterstellarStorageUsage_cmpText;

                    // 创建 home-panel > sep-line-1 和 home-panel > sep-line-0
                    GameObject goHomePanel_childSepLine1 = Instantiate(NativeObjectCache.SepLine1, goHomePanel.transform);

                    RectTransform goHomePanel_childSepLine1_cmpRectTransform = goHomePanel_childSepLine1.GetComponent<RectTransform>();
                    goHomePanel_childSepLine1_cmpRectTransform.Zeroize();
                    goHomePanel_childSepLine1_cmpRectTransform.anchorMax = new Vector2(0, 1);
                    goHomePanel_childSepLine1_cmpRectTransform.offsetMax = new Vector2(0, -1);
                    goHomePanel_childSepLine1_cmpRectTransform.offsetMin = new Vector2(-1, 1);

                    GameObject goHomePanel_childSepLine0 = Instantiate(NativeObjectCache.SepLine0, goHomePanel.transform);

                    RectTransform goHomePanel_childSepLine0_cmpRectTransform = goHomePanel_childSepLine0.GetComponent<RectTransform>();
                    goHomePanel_childSepLine0_cmpRectTransform.Zeroize();
                    goHomePanel_childSepLine0_cmpRectTransform.anchorMax = new Vector2(0, 1);
                    goHomePanel_childSepLine0_cmpRectTransform.offsetMax = new Vector2(1, -1);
                    goHomePanel_childSepLine0_cmpRectTransform.offsetMin = new Vector2(0, 1);

                    return uiHomePanel;
                }

                /// <summary>
                /// 当没有设置物品时，UIHomePanel应该如何显示
                /// </summary>
                public void ItemNotSet ()
                {
                    Icon.gameObject.SetActive(false);
                    CurrentLabel.gameObject.SetActive(false);
                    CurrentAmount.text = "";
                    OrderAmount.text = "";
                    MaxLabel.gameObject.SetActive(false);
                    MaxAmount.text = "";
                    InPlanetStorageUsage.text = "";
                    InterstellarStorageUsage.text = "";
                }

                /// <summary>
                /// 当设置物品时，UIHomePanel应该如何显示
                /// </summary>
                public void ItemSet ()
                {
                    Icon.gameObject.SetActive(true);
                    CurrentLabel.gameObject.SetActive(true);
                    MaxLabel.gameObject.SetActive(true);
                }
            }

            public enum _DisplayMode
            {
                Home
            }
        }
    }
}
