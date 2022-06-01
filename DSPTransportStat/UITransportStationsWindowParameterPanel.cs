using DSPTransportStat.CacheObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DSPTransportStat.Extensions;
using DSPTransportStat.Global;
using DSPTransportStat.Translation;
using DSPTransportStat.Enum;

namespace DSPTransportStat
{
    class UITransportStationsWindowParameterPanel : MonoBehaviour
    {
        /// <summary>
        /// 查询参数 - 是否显示行星内物流站点
        /// </summary>
        public bool ToggleInPlanet { get; set; }

        /// <summary>
        /// 查询参数 - 是否显示星际物流站点
        /// </summary>
        public bool ToggleInterstellar { get; set; }

        /// <summary>
        /// 查询参数 - 是否显示采集站
        /// </summary>
        public bool ToggleCollector { get; set; }

        /// <summary>
        /// 查询参数 - 过滤相关物品，只显示供应、需求或存储某项物品的物流站点
        /// 物品ID
        /// </summary>
        public int RelatedItemFilter { get; set; }

        /// <summary>
        /// 物品用途类型过滤
        /// </summary>
        public StorageUsageTypeFilter StorageUsageTypeFilter { get; set; }

        private Toggle storageUsageTypeFilterAllToggle = null;

        private Toggle storageUsageTypeFilterLocalToggle = null;

        private Toggle storageUsageTypeFilterRemoteToggle = null;

        private bool storageUsageTypeFilterToggleGroupTransitionFlag = false;

        /// <summary>
        /// 物品用途供需类型过滤
        /// </summary>
        public StorageUsageDirectionFilter StorageUsageDirectionFilter { get; set; }

        private Toggle storageUsageDirectionFilterAllToggle = null;

        private Toggle storageUsageDirectionFilterSupplyToggle = null;

        private Toggle storageUsageDirectionFilterDemandToggle = null;

        private Toggle storageUsageDirectionFilterStorageToggle = null;

        private bool storageUsageDirectionFilterToggleGroupTransitionFlag = false;

        /// <summary>
        /// 创建左侧的查询参数面板
        /// </summary>
        static public UITransportStationsWindowParameterPanel Create (GameObject baseGameObject, Action onParameterChangeCallback)
        {
            UITransportStationsWindowParameterPanel cmpTSWParamPanel = baseGameObject.AddComponent<UITransportStationsWindowParameterPanel>();

            // 设置默认值

            // 设置大小和位置
            RectTransform cmpRectTransform = baseGameObject.GetComponent<RectTransform>();
            cmpRectTransform.Zeroize();
            cmpRectTransform.anchorMax = new Vector2(0, 1);
            cmpRectTransform.anchorMin = new Vector2(0, 1);
            cmpRectTransform.offsetMax = new Vector2(10, -45);
            cmpRectTransform.offsetMin = new Vector2(-180, -400);

            // 创建 toggle-in-planet-label
            GameObject goToggleInPlanetLabel = new GameObject("toggle-in-planet-label", typeof(RectTransform), typeof(CanvasRenderer));
            goToggleInPlanetLabel.transform.SetParent(baseGameObject.transform);

            RectTransform goToggleInPlanetLabel_cmpRectTransform = goToggleInPlanetLabel.GetComponent<RectTransform>();
            goToggleInPlanetLabel_cmpRectTransform.Zeroize();
            goToggleInPlanetLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleInPlanetLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleInPlanetLabel_cmpRectTransform.offsetMax = new Vector2(10, -10);
            goToggleInPlanetLabel_cmpRectTransform.offsetMin = new Vector2(10, -10);

            Text goToggleInPlanetLabel_cmpText = goToggleInPlanetLabel.AddComponent<Text>();
            goToggleInPlanetLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goToggleInPlanetLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goToggleInPlanetLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goToggleInPlanetLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.ToggleInPlanetLabel;

            // 创建 toggle-in-planet
            GameObject goToggleInPlanet = Instantiate(NativeObjectCache.CheckBox, baseGameObject.transform);
            goToggleInPlanet.name = "toggle-in-planet";

            // 比同一行的字符低2个单位，长宽均为20个单位
            RectTransform goToggleInPlanet_cmpRectTransform = goToggleInPlanet.GetComponent<RectTransform>();
            goToggleInPlanet_cmpRectTransform.Zeroize();
            goToggleInPlanet_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleInPlanet_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleInPlanet_cmpRectTransform.offsetMax = new Vector2(185, -12);
            goToggleInPlanet_cmpRectTransform.offsetMin = new Vector2(165, -32);

            Toggle goToggleInPlanet_cmpToggle = goToggleInPlanet.GetComponent<Toggle>();

            // UI 组件的默认状态
            goToggleInPlanet_cmpToggle.isOn = true;

            // 默认值
            cmpTSWParamPanel.ToggleInPlanet = true;

            // 监听
            goToggleInPlanet_cmpToggle.onValueChanged.AddListener(value =>
            {
                cmpTSWParamPanel.ToggleInPlanet = value;
                onParameterChangeCallback.Invoke();
            });

            // 创建 toggle-interstellar-label
            GameObject goToggleInterstellarLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goToggleInterstellarLabel.name = "toggle-interstellar-label";

            RectTransform goToggleInterstellarLabel_cmpRectTransform = goToggleInterstellarLabel.GetComponent<RectTransform>();
            goToggleInterstellarLabel_cmpRectTransform.Zeroize();
            goToggleInterstellarLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleInterstellarLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleInterstellarLabel_cmpRectTransform.offsetMax = new Vector2(10, -58);
            goToggleInterstellarLabel_cmpRectTransform.offsetMin = new Vector2(10, -58);

            Text goToggleInterstellarLabel_cmpText = goToggleInterstellarLabel.GetComponent<Text>();
            goToggleInterstellarLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goToggleInterstellarLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goToggleInterstellarLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goToggleInterstellarLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.ToggleInterstellarLabel;

            // 创建 toggle-interstellar
            GameObject goToggleInterstellar = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goToggleInterstellar.name = "toggle-interstellar";

            // 比同一行的字符低2个单位，长宽均为20个单位
            RectTransform goToggleInterstellar_cmpRectTransform = goToggleInterstellar.GetComponent<RectTransform>();
            goToggleInterstellar_cmpRectTransform.Zeroize();
            goToggleInterstellar_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleInterstellar_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleInterstellar_cmpRectTransform.offsetMax = new Vector2(185, -60);
            goToggleInterstellar_cmpRectTransform.offsetMin = new Vector2(165, -80);

            Toggle goToggleInterstellar_cmpToggle = goToggleInterstellar.GetComponent<Toggle>();

            // UI 组件的默认状态
            goToggleInterstellar_cmpToggle.isOn = true;

            // 默认值
            cmpTSWParamPanel.ToggleInterstellar = true;

            // 监听
            goToggleInterstellar_cmpToggle.onValueChanged.AddListener(value =>
            {
                cmpTSWParamPanel.ToggleInterstellar = value;
                onParameterChangeCallback.Invoke();
            });

            // 创建 toggle-collector-label
            GameObject goToggleCollectorLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goToggleCollectorLabel.name = "toggle-collector-label";

            RectTransform goToggleCollectorLabel_cmpRectTransform = goToggleCollectorLabel.GetComponent<RectTransform>();
            goToggleCollectorLabel_cmpRectTransform.Zeroize();
            goToggleCollectorLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleCollectorLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleCollectorLabel_cmpRectTransform.offsetMax = new Vector2(10, -106);
            goToggleCollectorLabel_cmpRectTransform.offsetMin = new Vector2(10, -106);

            Text goToggleCollectorLabel_cmpText = goToggleCollectorLabel.GetComponent<Text>();
            goToggleCollectorLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goToggleCollectorLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goToggleCollectorLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goToggleCollectorLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.ToggleCollectorLabel;

            // 创建 toggle-collector
            GameObject goToggleCollector = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goToggleCollector.name = "toggle-collector";

            // 比同一行的字符低2个单位，长宽均为20个单位
            RectTransform goToggleCollector_cmpRectTransform = goToggleCollector.GetComponent<RectTransform>();
            goToggleCollector_cmpRectTransform.Zeroize();
            goToggleCollector_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goToggleCollector_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goToggleCollector_cmpRectTransform.offsetMax = new Vector2(185, -108);
            goToggleCollector_cmpRectTransform.offsetMin = new Vector2(165, -128);

            Toggle goToggleCollector_cmpToggle = goToggleCollector.GetComponent<Toggle>();

            // UI 组件的默认状态
            goToggleCollector_cmpToggle.isOn = true;

            // 默认值
            cmpTSWParamPanel.ToggleCollector = true;

            // 监听
            goToggleCollector_cmpToggle.onValueChanged.AddListener(value =>
            {
                cmpTSWParamPanel.ToggleCollector = value;
                onParameterChangeCallback.Invoke();
            });

            // 创建 item-filter-label
            GameObject goItemFilterLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goItemFilterLabel.name = "item-filter-label";

            RectTransform goItemFilterLabel_cmpRectTransform = goItemFilterLabel.GetComponent<RectTransform>();
            goItemFilterLabel_cmpRectTransform.Zeroize();
            goItemFilterLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goItemFilterLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goItemFilterLabel_cmpRectTransform.offsetMax = new Vector2(10, -156);
            goItemFilterLabel_cmpRectTransform.offsetMin = new Vector2(10, -156);

            Text goItemFilterLabel_cmpText = goItemFilterLabel.GetComponent<Text>();
            goItemFilterLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goItemFilterLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goItemFilterLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goItemFilterLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.ItemFilterLabel;

            // 创建 item-filter
            GameObject goItemFilter = Instantiate(ReassembledObjectCache.GOCircularItemFilterButton, baseGameObject.transform);
            goItemFilter.name = "item-filter";

            RectTransform goItemFilter_cmpRectTransform = goItemFilter.GetComponent<RectTransform>();
            goItemFilter_cmpRectTransform.Zeroize();
            goItemFilter_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goItemFilter_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goItemFilter_cmpRectTransform.offsetMax = new Vector2(185, -150);
            goItemFilter_cmpRectTransform.offsetMin = new Vector2(145, -190);

            Image goItemFilter_childBg_cmpImage = goItemFilter.transform.Find("bg").GetComponent<Image>();

            // 监听选择过滤物品的事件
            // 依赖了 item-filter-clear ，提前声明
            GameObject goItemFilterClear = null;
            goItemFilter.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (UIItemPicker.isOpened)
                {
                    UIItemPicker.Close();
                    return;
                }
                UIItemPicker.Popup(new Vector2(-360f, 180f), (item) =>
                {
                    if (item == null)
                    {
                        goItemFilter_childBg_cmpImage.sprite = ResourceCache.SpriteRound54pxSlice;
                        cmpTSWParamPanel.RelatedItemFilter = Constants.NONE_ITEM_ID;
                        goItemFilterClear.SetActive(false);
                    }
                    else
                    {
                        goItemFilter_childBg_cmpImage.sprite = item.iconSprite;
                        cmpTSWParamPanel.RelatedItemFilter = item.ID;
                        goItemFilterClear.SetActive(true);
                    }
                    onParameterChangeCallback.Invoke();
                });
            });

            // 创建 item-filter-clear
            goItemFilterClear = UIUtility.CreateImageButton("item-filter-clear", ResourceCache.SpriteXIcon, () =>
            {
                goItemFilter_childBg_cmpImage.sprite = ResourceCache.SpriteRound54pxSlice;
                cmpTSWParamPanel.RelatedItemFilter = Constants.NONE_ITEM_ID;
                onParameterChangeCallback.Invoke();
                goItemFilterClear.SetActive(false);
            });
            goItemFilterClear.transform.SetParent(baseGameObject.transform);
            goItemFilterClear.SetActive(false);

            RectTransform goItemFilterClear_cmpRectTransform = goItemFilterClear.GetComponent<RectTransform>();
            goItemFilterClear_cmpRectTransform.Zeroize();
            goItemFilterClear_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goItemFilterClear_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goItemFilterClear_cmpRectTransform.offsetMax = new Vector2(190, -145);
            goItemFilterClear_cmpRectTransform.offsetMin = new Vector2(175, -160);

            // 创建 usage-type-filter-label
            GameObject goUsageTypeFilterLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageTypeFilterLabel.name = "usage-type-filter-label";

            RectTransform goUsageTypeFilterLabel_cmpRectTransform = goUsageTypeFilterLabel.GetComponent<RectTransform>();
            goUsageTypeFilterLabel_cmpRectTransform.Zeroize();
            goUsageTypeFilterLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageTypeFilterLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageTypeFilterLabel_cmpRectTransform.offsetMax = new Vector2(90, -200);
            goUsageTypeFilterLabel_cmpRectTransform.offsetMin = new Vector2(10, -224);

            Text goUsageTypeFilterLabel_cmpText = goUsageTypeFilterLabel.GetComponent<Text>();
            goUsageTypeFilterLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageTypeFilterLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageTypeFilterLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageTypeFilterLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageTypeFilterLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageTypeFilterLabel;

            // 创建 usage-type-filter-toggle-all-label
            GameObject goUsageTypeFilterToggleAllLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageTypeFilterToggleAllLabel.name = "usage-type-filter-toggle-all-label";

            RectTransform goUsageTypeFilterToggleAllLabel_cmpRectTransform = goUsageTypeFilterToggleAllLabel.GetComponent<RectTransform>();
            goUsageTypeFilterToggleAllLabel_cmpRectTransform.Zeroize();
            goUsageTypeFilterToggleAllLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageTypeFilterToggleAllLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageTypeFilterToggleAllLabel_cmpRectTransform.offsetMax = new Vector2(55, -224);
            goUsageTypeFilterToggleAllLabel_cmpRectTransform.offsetMin = new Vector2(10, -248);

            Text goUsageTypeFilterToggleAllLabel_cmpText = goUsageTypeFilterToggleAllLabel.GetComponent<Text>();
            goUsageTypeFilterToggleAllLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageTypeFilterToggleAllLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageTypeFilterToggleAllLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageTypeFilterToggleAllLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageTypeFilterToggleAllLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageTypeFilterToggleAllLabel;

            // 创建 usage-type-filter-toggle-all
            GameObject goUsageTypeFilterToggleAll = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goUsageTypeFilterToggleAll.name = "usage-type-filter-toggle-all";

            RectTransform goUsageTypeFilterToggleAll_cmpRectTransform = goUsageTypeFilterToggleAll.GetComponent<RectTransform>();
            goUsageTypeFilterToggleAll_cmpRectTransform.Zeroize();
            goUsageTypeFilterToggleAll_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageTypeFilterToggleAll_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageTypeFilterToggleAll_cmpRectTransform.offsetMax = new Vector2(80, -226);
            goUsageTypeFilterToggleAll_cmpRectTransform.offsetMin = new Vector2(60, -246);

            Toggle goUsageTypeFilterToggleAll_cmpToggle = goUsageTypeFilterToggleAll.GetComponent<Toggle>();

            goUsageTypeFilterToggleAll_cmpToggle.isOn = true;

            cmpTSWParamPanel.StorageUsageTypeFilter = StorageUsageTypeFilter.All;

            goUsageTypeFilterToggleAll_cmpToggle.onValueChanged.AddListener(value =>
            {
                if (cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag)
                {
                    return;
                }

                cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag = true;

                if (value)
                {
                    cmpTSWParamPanel.StorageUsageTypeFilter = StorageUsageTypeFilter.All;
                    cmpTSWParamPanel.storageUsageTypeFilterLocalToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageTypeFilterRemoteToggle.isOn = false;
                }
                else
                {
                    cmpTSWParamPanel.storageUsageTypeFilterAllToggle.isOn = true;
                }

                cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag = false;

                if (value)
                {
                    onParameterChangeCallback.Invoke();
                }
            });

            cmpTSWParamPanel.storageUsageTypeFilterAllToggle = goUsageTypeFilterToggleAll_cmpToggle;

            // 创建 usage-type-filter-toggle-local-label
            GameObject goUsageTypeFilterToggleLocalLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageTypeFilterToggleLocalLabel.name = "usage-type-filter-toggle-local-label";

            RectTransform goUsageTypeFilterToggleLocalLabel_cmpRectTransform = goUsageTypeFilterToggleLocalLabel.GetComponent<RectTransform>();
            goUsageTypeFilterToggleLocalLabel_cmpRectTransform.Zeroize();
            goUsageTypeFilterToggleLocalLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageTypeFilterToggleLocalLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageTypeFilterToggleLocalLabel_cmpRectTransform.offsetMax = new Vector2(55, -248);
            goUsageTypeFilterToggleLocalLabel_cmpRectTransform.offsetMin = new Vector2(10, -272);

            Text goUsageTypeFilterToggleLocalLabel_cmpText = goUsageTypeFilterToggleLocalLabel.GetComponent<Text>();
            goUsageTypeFilterToggleLocalLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageTypeFilterToggleLocalLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageTypeFilterToggleLocalLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageTypeFilterToggleLocalLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageTypeFilterToggleLocalLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageTypeFilterToggleLocalLabel;

            // 创建 usage-type-filter-toggle-local
            GameObject goUsageTypeFilterToggleLocal = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goUsageTypeFilterToggleLocal.name = "usage-type-filter-toggle-local";

            RectTransform goUsageTypeFilterToggleLocal_cmpRectTransform = goUsageTypeFilterToggleLocal.GetComponent<RectTransform>();
            goUsageTypeFilterToggleLocal_cmpRectTransform.Zeroize();
            goUsageTypeFilterToggleLocal_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageTypeFilterToggleLocal_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageTypeFilterToggleLocal_cmpRectTransform.offsetMax = new Vector2(80, -250);
            goUsageTypeFilterToggleLocal_cmpRectTransform.offsetMin = new Vector2(60, -270);

            Toggle goUsageTypeFilterToggleLocal_cmpToggle = goUsageTypeFilterToggleLocal.GetComponent<Toggle>();

            goUsageTypeFilterToggleLocal_cmpToggle.isOn = false;

            goUsageTypeFilterToggleLocal_cmpToggle.onValueChanged.AddListener(value =>
            {
                if (cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag)
                {
                    return;
                }

                cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag = true;

                if (value)
                {
                    cmpTSWParamPanel.StorageUsageTypeFilter = StorageUsageTypeFilter.Local;
                    cmpTSWParamPanel.storageUsageTypeFilterAllToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageTypeFilterRemoteToggle.isOn = false;
                }
                else
                {
                    cmpTSWParamPanel.storageUsageTypeFilterLocalToggle.isOn = true;
                }

                cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag = false;

                if (value)
                {
                    onParameterChangeCallback.Invoke();
                }
            });

            cmpTSWParamPanel.storageUsageTypeFilterLocalToggle = goUsageTypeFilterToggleLocal_cmpToggle;

            // 创建 usage-type-filter-toggle-remote-label
            GameObject goUsageTypeFilterToggleRemoteLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageTypeFilterToggleRemoteLabel.name = "usage-type-filter-toggle-remote-label";

            RectTransform goUsageTypeFilterToggleRemoteLabel_cmpRectTransform = goUsageTypeFilterToggleRemoteLabel.GetComponent<RectTransform>();
            goUsageTypeFilterToggleRemoteLabel_cmpRectTransform.Zeroize();
            goUsageTypeFilterToggleRemoteLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageTypeFilterToggleRemoteLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageTypeFilterToggleRemoteLabel_cmpRectTransform.offsetMax = new Vector2(55, -272);
            goUsageTypeFilterToggleRemoteLabel_cmpRectTransform.offsetMin = new Vector2(10, -296);

            Text goUsageTypeFilterToggleRemoteLabel_cmpText = goUsageTypeFilterToggleRemoteLabel.GetComponent<Text>();
            goUsageTypeFilterToggleRemoteLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageTypeFilterToggleRemoteLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageTypeFilterToggleRemoteLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageTypeFilterToggleRemoteLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageTypeFilterToggleRemoteLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageTypeFilterToggleRemoteLabel;

            // 创建 usage-type-filter-toggle-remote
            GameObject goUsageTypeFilterToggleRemote = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goUsageTypeFilterToggleRemote.name = "usage-type-filter-toggle-remote";

            RectTransform goUsageTypeFilterToggleRemote_cmpRectTransform = goUsageTypeFilterToggleRemote.GetComponent<RectTransform>();
            goUsageTypeFilterToggleRemote_cmpRectTransform.Zeroize();
            goUsageTypeFilterToggleRemote_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageTypeFilterToggleRemote_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageTypeFilterToggleRemote_cmpRectTransform.offsetMax = new Vector2(80, -274);
            goUsageTypeFilterToggleRemote_cmpRectTransform.offsetMin = new Vector2(60, -294);

            Toggle goUsageTypeFilterToggleRemote_cmpToggle = goUsageTypeFilterToggleRemote.GetComponent<Toggle>();

            goUsageTypeFilterToggleRemote_cmpToggle.isOn = false;

            goUsageTypeFilterToggleRemote_cmpToggle.onValueChanged.AddListener(value =>
            {
                if (cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag)
                {
                    return;
                }

                cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag = true;

                if (value)
                {
                    cmpTSWParamPanel.StorageUsageTypeFilter = StorageUsageTypeFilter.Remote;
                    cmpTSWParamPanel.storageUsageTypeFilterAllToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageTypeFilterLocalToggle.isOn = false;
                }
                else
                {
                    cmpTSWParamPanel.storageUsageTypeFilterRemoteToggle.isOn = true;
                }

                cmpTSWParamPanel.storageUsageTypeFilterToggleGroupTransitionFlag = false;

                if (value)
                {
                    onParameterChangeCallback.Invoke();
                }
            });

            cmpTSWParamPanel.storageUsageTypeFilterRemoteToggle = goUsageTypeFilterToggleRemote_cmpToggle;

            // 创建 usage-direction-filter-label
            GameObject goUsageDirectionFilterLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageDirectionFilterLabel.name = "usage-direction-filter-label";

            RectTransform goUsageDirectionFilterLabel_cmpRectTransform = goUsageDirectionFilterLabel.GetComponent<RectTransform>();
            goUsageDirectionFilterLabel_cmpRectTransform.Zeroize();
            goUsageDirectionFilterLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterLabel_cmpRectTransform.offsetMax = new Vector2(180, -200);
            goUsageDirectionFilterLabel_cmpRectTransform.offsetMin = new Vector2(100, -224);

            Text goUsageDirectionFilterLabel_cmpText = goUsageDirectionFilterLabel.GetComponent<Text>();
            goUsageDirectionFilterLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageDirectionFilterLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageDirectionFilterLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageDirectionFilterLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageDirectionFilterLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageDirectionFilterLabel;

            // 创建 usage-direction-filter-toggle-all-label
            GameObject goUsageDirectionFilterToggleAllLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageDirectionFilterToggleAllLabel.name = "usage-direction-filter-toggle-all-label";

            RectTransform goUsageDirectionFilterToggleAllLabel_cmpRectTransform = goUsageDirectionFilterToggleAllLabel.GetComponent<RectTransform>();
            goUsageDirectionFilterToggleAllLabel_cmpRectTransform.Zeroize();
            goUsageDirectionFilterToggleAllLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterToggleAllLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterToggleAllLabel_cmpRectTransform.offsetMax = new Vector2(155, -224);
            goUsageDirectionFilterToggleAllLabel_cmpRectTransform.offsetMin = new Vector2(100, -248);

            Text goUsageDirectionFilterToggleAllLabel_cmpText = goUsageDirectionFilterToggleAllLabel.GetComponent<Text>();
            goUsageDirectionFilterToggleAllLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageDirectionFilterToggleAllLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageDirectionFilterToggleAllLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageDirectionFilterToggleAllLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageDirectionFilterToggleAllLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageDirectionFilterToggleAllLabel;

            // 创建 usage-direction-filter-toggle-all
            GameObject goUsageDirectionFilterToggleAll = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goUsageDirectionFilterToggleAll.name = "usage-direction-filter-toggle-all";

            RectTransform goUsageDirectionFilterToggleAll_cmpRectTransform = goUsageDirectionFilterToggleAll.GetComponent<RectTransform>();
            goUsageDirectionFilterToggleAll_cmpRectTransform.Zeroize();
            goUsageDirectionFilterToggleAll_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterToggleAll_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterToggleAll_cmpRectTransform.offsetMax = new Vector2(180, -226);
            goUsageDirectionFilterToggleAll_cmpRectTransform.offsetMin = new Vector2(160, -246);

            Toggle goUsageDirectionFilterToggleAll_cmpToggle = goUsageDirectionFilterToggleAll.GetComponent<Toggle>();

            goUsageDirectionFilterToggleAll_cmpToggle.isOn = true;

            cmpTSWParamPanel.StorageUsageDirectionFilter = StorageUsageDirectionFilter.All;

            goUsageDirectionFilterToggleAll_cmpToggle.onValueChanged.AddListener(value =>
            {
                if (cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag)
                {
                    return;
                }

                cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag = true;

                if (value)
                {
                    cmpTSWParamPanel.StorageUsageDirectionFilter = StorageUsageDirectionFilter.All;
                    cmpTSWParamPanel.storageUsageDirectionFilterSupplyToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageDirectionFilterDemandToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageDirectionFilterStorageToggle.isOn = false;
                }
                else
                {
                    cmpTSWParamPanel.storageUsageDirectionFilterAllToggle.isOn = true;
                }

                cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag = false;

                if (value)
                {
                    onParameterChangeCallback.Invoke();
                }
            });

            cmpTSWParamPanel.storageUsageDirectionFilterAllToggle = goUsageDirectionFilterToggleAll_cmpToggle;

            // 创建 usage-direction-filter-toggle-supply-label
            GameObject goUsageDirectionFilterToggleSupplyLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageDirectionFilterToggleSupplyLabel.name = "usage-direction-filter-toggle-supply-label";

            RectTransform goUsageDirectionFilterToggleSupplyLabel_cmpRectTransform = goUsageDirectionFilterToggleSupplyLabel.GetComponent<RectTransform>();
            goUsageDirectionFilterToggleSupplyLabel_cmpRectTransform.Zeroize();
            goUsageDirectionFilterToggleSupplyLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterToggleSupplyLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterToggleSupplyLabel_cmpRectTransform.offsetMax = new Vector2(155, -248);
            goUsageDirectionFilterToggleSupplyLabel_cmpRectTransform.offsetMin = new Vector2(100, -272);

            Text goUsageDirectionFilterToggleSupplyLabel_cmpText = goUsageDirectionFilterToggleSupplyLabel.GetComponent<Text>();
            goUsageDirectionFilterToggleSupplyLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageDirectionFilterToggleSupplyLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageDirectionFilterToggleSupplyLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageDirectionFilterToggleSupplyLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageDirectionFilterToggleSupplyLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageDirectionFilterToggleSupplyLabel;

            // 创建 usage-direction-filter-toggle-supply
            GameObject goUsageDirectionFilterToggleSupply = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goUsageDirectionFilterToggleSupply.name = "usage-direction-filter-toggle-supply";

            RectTransform goUsageDirectionFilterToggleSupply_cmpRectTransform = goUsageDirectionFilterToggleSupply.GetComponent<RectTransform>();
            goUsageDirectionFilterToggleSupply_cmpRectTransform.Zeroize();
            goUsageDirectionFilterToggleSupply_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterToggleSupply_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterToggleSupply_cmpRectTransform.offsetMax = new Vector2(180, -250);
            goUsageDirectionFilterToggleSupply_cmpRectTransform.offsetMin = new Vector2(160, -270);

            Toggle goUsageDirectionFilterToggleSupply_cmpToggle = goUsageDirectionFilterToggleSupply.GetComponent<Toggle>();

            goUsageDirectionFilterToggleSupply_cmpToggle.isOn = false;

            goUsageDirectionFilterToggleSupply_cmpToggle.onValueChanged.AddListener(value =>
            {
                if (cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag)
                {
                    return;
                }

                cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag = true;

                if (value)
                {
                    cmpTSWParamPanel.StorageUsageDirectionFilter = StorageUsageDirectionFilter.Supply;
                    cmpTSWParamPanel.storageUsageDirectionFilterAllToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageDirectionFilterDemandToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageDirectionFilterStorageToggle.isOn = false;
                }
                else
                {
                    cmpTSWParamPanel.storageUsageDirectionFilterSupplyToggle.isOn = true;
                }

                cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag = false;

                if (value)
                {
                    onParameterChangeCallback.Invoke();
                }
            });

            cmpTSWParamPanel.storageUsageDirectionFilterSupplyToggle = goUsageDirectionFilterToggleSupply_cmpToggle;

            // 创建 usage-direction-filter-toggle-demand-label
            GameObject goUsageDirectionFilterToggleDemandLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageDirectionFilterToggleDemandLabel.name = "usage-direction-filter-toggle-demand-label";

            RectTransform goUsageDirectionFilterToggleDemandLabel_cmpRectTransform = goUsageDirectionFilterToggleDemandLabel.GetComponent<RectTransform>();
            goUsageDirectionFilterToggleDemandLabel_cmpRectTransform.Zeroize();
            goUsageDirectionFilterToggleDemandLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterToggleDemandLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterToggleDemandLabel_cmpRectTransform.offsetMax = new Vector2(155, -272);
            goUsageDirectionFilterToggleDemandLabel_cmpRectTransform.offsetMin = new Vector2(100, -296);

            Text goUsageDirectionFilterToggleDemandLabel_cmpText = goUsageDirectionFilterToggleDemandLabel.GetComponent<Text>();
            goUsageDirectionFilterToggleDemandLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageDirectionFilterToggleDemandLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageDirectionFilterToggleDemandLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageDirectionFilterToggleDemandLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageDirectionFilterToggleDemandLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageDirectionFilterToggleDemandLabel;

            // 创建 usage-direction-filter-toggle-demand
            GameObject goUsageDirectionFilterToggleDemand = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goUsageDirectionFilterToggleDemand.name = "usage-direction-filter-toggle-demand";

            RectTransform goUsageDirectionFilterToggleDemand_cmpRectTransform = goUsageDirectionFilterToggleDemand.GetComponent<RectTransform>();
            goUsageDirectionFilterToggleDemand_cmpRectTransform.Zeroize();
            goUsageDirectionFilterToggleDemand_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterToggleDemand_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterToggleDemand_cmpRectTransform.offsetMax = new Vector2(180, -274);
            goUsageDirectionFilterToggleDemand_cmpRectTransform.offsetMin = new Vector2(160, -294);

            Toggle goUsageDirectionFilterToggleDemand_cmpToggle = goUsageDirectionFilterToggleDemand.GetComponent<Toggle>();

            goUsageDirectionFilterToggleDemand_cmpToggle.isOn = false;

            goUsageDirectionFilterToggleDemand_cmpToggle.onValueChanged.AddListener(value =>
            {
                if (cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag)
                {
                    return;
                }

                cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag = true;

                if (value)
                {
                    cmpTSWParamPanel.StorageUsageDirectionFilter = StorageUsageDirectionFilter.Demand;
                    cmpTSWParamPanel.storageUsageDirectionFilterAllToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageDirectionFilterSupplyToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageDirectionFilterStorageToggle.isOn = false;
                }
                else
                {
                    cmpTSWParamPanel.storageUsageDirectionFilterDemandToggle.isOn = true;
                }

                cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag = false;

                if (value)
                {
                    onParameterChangeCallback.Invoke();
                }
            });

            cmpTSWParamPanel.storageUsageDirectionFilterDemandToggle = goUsageDirectionFilterToggleDemand_cmpToggle;

            // 创建 usage-direction-filter-toggle-storage-label
            GameObject goUsageDirectionFilterToggleStorageLabel = Instantiate(goToggleInPlanetLabel, baseGameObject.transform);
            goUsageDirectionFilterToggleStorageLabel.name = "usage-direction-filter-toggle-storage-label";

            RectTransform goUsageDirectionFilterToggleStorageLabel_cmpRectTransform = goUsageDirectionFilterToggleStorageLabel.GetComponent<RectTransform>();
            goUsageDirectionFilterToggleStorageLabel_cmpRectTransform.Zeroize();
            goUsageDirectionFilterToggleStorageLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterToggleStorageLabel_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterToggleStorageLabel_cmpRectTransform.offsetMax = new Vector2(155, -296);
            goUsageDirectionFilterToggleStorageLabel_cmpRectTransform.offsetMin = new Vector2(100, -320);

            Text goUsageDirectionFilterToggleStorageLabel_cmpText = goUsageDirectionFilterToggleStorageLabel.GetComponent<Text>();
            goUsageDirectionFilterToggleStorageLabel_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goUsageDirectionFilterToggleStorageLabel_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goUsageDirectionFilterToggleStorageLabel_cmpText.alignment = TextAnchor.MiddleCenter;
            goUsageDirectionFilterToggleStorageLabel_cmpText.font = ResourceCache.FontSAIRASB;
            goUsageDirectionFilterToggleStorageLabel_cmpText.text = Strings.TransportStationsWindow.ParameterPanel.UsageDirectionFilterToggleStorageLabel;

            // 创建 usage-direction-filter-toggle-storage
            GameObject goUsageDirectionFilterToggleStorage = Instantiate(goToggleInPlanet, baseGameObject.transform);
            goUsageDirectionFilterToggleStorage.name = "usage-direction-filter-toggle-storage";

            RectTransform goUsageDirectionFilterToggleStorage_cmpRectTransform = goUsageDirectionFilterToggleStorage.GetComponent<RectTransform>();
            goUsageDirectionFilterToggleStorage_cmpRectTransform.Zeroize();
            goUsageDirectionFilterToggleStorage_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goUsageDirectionFilterToggleStorage_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goUsageDirectionFilterToggleStorage_cmpRectTransform.offsetMax = new Vector2(180, -298);
            goUsageDirectionFilterToggleStorage_cmpRectTransform.offsetMin = new Vector2(160, -318);

            Toggle goUsageDirectionFilterToggleStorage_cmpToggle = goUsageDirectionFilterToggleStorage.GetComponent<Toggle>();

            goUsageDirectionFilterToggleStorage_cmpToggle.isOn = false;

            goUsageDirectionFilterToggleStorage_cmpToggle.onValueChanged.AddListener(value =>
            {
                if (cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag)
                {
                    return;
                }

                cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag = true;

                if (value)
                {
                    cmpTSWParamPanel.StorageUsageDirectionFilter = StorageUsageDirectionFilter.Storage;
                    cmpTSWParamPanel.storageUsageDirectionFilterAllToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageDirectionFilterSupplyToggle.isOn = false;
                    cmpTSWParamPanel.storageUsageDirectionFilterDemandToggle.isOn = false;
                }
                else
                {
                    cmpTSWParamPanel.storageUsageDirectionFilterStorageToggle.isOn = true;
                }

                cmpTSWParamPanel.storageUsageDirectionFilterToggleGroupTransitionFlag = false;

                if (value)
                {
                    onParameterChangeCallback.Invoke();
                }
            });

            cmpTSWParamPanel.storageUsageDirectionFilterStorageToggle = goUsageDirectionFilterToggleStorage_cmpToggle;

            return cmpTSWParamPanel;
        }
    }
}
