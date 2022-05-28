using DSPTransportStat.CacheObjects;
using DSPTransportStat.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DSPTransportStat.Extensions;
using DSPTransportStat.Global;
using DSPTransportStat.Translation;
using UnityEngine.Events;

namespace DSPTransportStat
{
    class UITransportStationsWindow : ManualBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// 是否已经创建过窗口，防止重复创建窗口
        /// </summary>
        static private bool isCreated = false;

        /// <summary>
        /// 鼠标指针是否在窗口内
        /// </summary>
        public bool IsPointerInside { get; set; }

        /// <summary>
        /// 参数面板组件，方便在查询参数发生变化时获取新的查询参数
        /// </summary>
        private UITransportStationsWindowParameterPanel uiTSWParameterPanel = null;

        /// <summary>
        /// 预先创建好的列表项
        /// </summary>
        private UITransportStationsEntry[] uiTransportStationsEntries;

        /// <summary>
        /// 具有翻译功能的用于显示当前列表中站点数量的组件
        /// </summary>
        private UIStationCountInListTranslation uiStationCountInListTranslation = null;

        /// <summary>
        /// 物流运输站列表，包含 StationInfo 、 StarData 和 PlanetData
        /// </summary>
        private readonly List<StationInfoBundle> stations = new List<StationInfoBundle>();

        /// <summary>
        /// 排序参数 - 位置和名称
        /// </summary>
        private SortOrder locationAndNameSortOrder = SortOrder.NONE;

        /// <summary>
        /// 查询参数 - 搜索字符串
        /// </summary>
        private string searchString = "";

        /// <summary>
        /// content RectTransform 的引用，便于修改内容尺寸
        /// </summary>
        private RectTransform contentRectTransform;

        /// <summary>
        /// viewport RectTransform 的引用，便于修改视口的尺寸
        /// </summary>
        private RectTransform viewportRectTransform;

        /// <summary>
        /// 创建物流运输站窗口
        /// </summary>
        /// <returns></returns>
        static public UITransportStationsWindow Create (bool isAllowItemTransfer, UnityAction<bool> onIsAllowItemTransferValueChange)
        {
            if (isCreated)
            {
                throw new Exception("UITransportStationsWindow has beed created before");
            }
            isCreated = true;

            // 克隆原来的统计面板
            UIStatisticsWindow statWindow = UIRoot.instance.uiGame.statWindow;
            UIStatisticsWindow clonedStatWindow = Instantiate(statWindow, statWindow.transform.parent);

            // 通过克隆的 statWindow 删除不需要的对象
            // 删除左侧菜单
            Destroy(clonedStatWindow.verticalTab.gameObject);

            // 保留水平标签列表，删除里面的按钮
            // Destroy(clonedStatWindow.horizontalTab.gameObject);
            for (int i = 0; i < clonedStatWindow.horizontalTab.transform.childCount; ++i)
            {
                Destroy(clonedStatWindow.horizontalTab.transform.GetChild(i).gameObject);
            }

            // 删除里程碑、电力和研究等面板
            Destroy(clonedStatWindow.achievementPanelUI.gameObject);
            Destroy(clonedStatWindow.dysonPanel);
            Destroy(clonedStatWindow.milestonePanelUI.gameObject);
            Destroy(clonedStatWindow.performancePanel);
            Destroy(clonedStatWindow.powerPanel);

            // 保留生产面板，但是删除 content 中所有旧的列表项
            // Destroy(clonedStatWindow.productPanel);
            GameObject goScrollView = clonedStatWindow.productPanel.transform.Find("scroll-view").gameObject;
            GameObject goViewport = clonedStatWindow.productPanel.transform.Find("scroll-view/viewport").gameObject;
            GameObject goContent = clonedStatWindow.productPanel.transform.Find("scroll-view/viewport/content").gameObject;
            for (int i = 0; i < goContent.transform.childCount; ++i)
            {
                Destroy(goContent.transform.GetChild(i).gameObject);
            }

            Destroy(clonedStatWindow.propertyPanelUI.gameObject);
            Destroy(clonedStatWindow.researchPanel);

            // ==========
            // 初始化窗口整体属性
            // ==========
            GameObject goTSW = clonedStatWindow.gameObject;
            UITransportStationsWindow uiTSW = Create_InitWindow(goTSW);

            // ==========
            // 初始化查询参数面板
            // ==========
            uiTSW.uiTSWParameterPanel = Create_InitParameterPanel(clonedStatWindow.horizontalTab, () => uiTSW.OnParameterChange());

            // ==========
            // 初始化列表面板
            // ==========
            uiTSW.uiTransportStationsEntries = Create_InitTable(clonedStatWindow.productPanel);

            // ==========
            // 其他初始化过程
            // ==========

            // 调整 scroll-view 的上边距，使之适应没有 top 对象后的内容高度
            // 40 是表头的高度
            goScrollView.GetComponent<RectTransform>().offsetMax = new Vector2(0, -40);

            // 初始化指针状态
            uiTSW.IsPointerInside = false;

            // 保留一些引用
            uiTSW.viewportRectTransform = goViewport.GetComponent<RectTransform>();
            uiTSW.contentRectTransform = goContent.GetComponent<RectTransform>();

            // ==========
            // 开始创建UI组件（表头）
            // ==========

            // 创建 headers
            GameObject goHeaders = new GameObject("headers", typeof(RectTransform), typeof(CanvasRenderer));
            goHeaders.transform.parent = clonedStatWindow.productPanel.transform;

            RectTransform goHeaders_cmpRectTransform = goHeaders.GetComponent<RectTransform>();
            goHeaders_cmpRectTransform.Zeroize();
            goHeaders_cmpRectTransform.anchorMin = new Vector2(0, 1);
            goHeaders_cmpRectTransform.anchorMax = new Vector2(1, 1);
            goHeaders_cmpRectTransform.offsetMin = new Vector2(0, -40);

            // 创建 headers > location-and-name
            GameObject goHeaders_childLocationAndName = new GameObject("location-and-name", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            goHeaders_childLocationAndName.transform.parent = goHeaders.transform;

            RectTransform goHeaders_childLocationAndName_cmpRectTransform = goHeaders_childLocationAndName.GetComponent<RectTransform>();
            goHeaders_childLocationAndName_cmpRectTransform.Zeroize();
            goHeaders_childLocationAndName_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goHeaders_childLocationAndName_cmpRectTransform.offsetMax = new Vector2(100, -10);
            goHeaders_childLocationAndName_cmpRectTransform.offsetMin = new Vector2(10, 0);

            Text goHeaders_childLocationAndName_cmpText = goHeaders_childLocationAndName.GetComponent<Text>();
            goHeaders_childLocationAndName_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goHeaders_childLocationAndName_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goHeaders_childLocationAndName_cmpText.font = ResourceCache.FontSAIRASB;
            goHeaders_childLocationAndName_cmpText.text = Strings.TransportStationsWindow.LocationAndName;

            // 创建 headers > location-and-name > sort
            GameObject goHeaders_childLocationAndName_childSort = Instantiate(ReassembledObjectCache.GOTextButton, goHeaders_childLocationAndName.transform);
            goHeaders_childLocationAndName_childSort.name = "sort";
            goHeaders_childLocationAndName_childSort.SetActive(true);

            RectTransform goHeaders_childLocationAndName_childSort_cmpRectTransform = goHeaders_childLocationAndName_childSort.GetComponent<RectTransform>();
            goHeaders_childLocationAndName_childSort_cmpRectTransform.Zeroize();
            goHeaders_childLocationAndName_childSort_cmpRectTransform.anchorMin = new Vector2(1, 0);
            goHeaders_childLocationAndName_childSort_cmpRectTransform.anchorMax = new Vector2(1, 1);
            goHeaders_childLocationAndName_childSort_cmpRectTransform.offsetMax = new Vector2(40, 0);
            goHeaders_childLocationAndName_childSort_cmpRectTransform.offsetMin = new Vector2(0, 5);

            GameObject goHeaders_childLocationAndName_childSort_childButtonText = goHeaders_childLocationAndName_childSort.transform.Find("button-text").gameObject;
            Text goHeaders_childLocationAndName_childSort_childButtonText_cmpText = goHeaders_childLocationAndName_childSort_childButtonText.GetComponent<Text>();

            // 默认的位置和名称顺序为升序
            goHeaders_childLocationAndName_childSort_childButtonText_cmpText.text = Strings.TransportStationsWindow.ASC;
            uiTSW.locationAndNameSortOrder = SortOrder.ASC;

            goHeaders_childLocationAndName_childSort.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (uiTSW.locationAndNameSortOrder == SortOrder.ASC)
                {
                    uiTSW.locationAndNameSortOrder = SortOrder.DESC;
                    goHeaders_childLocationAndName_childSort_childButtonText_cmpText.text = Strings.TransportStationsWindow.DESC;
                }
                else if (uiTSW.locationAndNameSortOrder == SortOrder.DESC)
                {
                    uiTSW.locationAndNameSortOrder = SortOrder.ASC;
                    goHeaders_childLocationAndName_childSort_childButtonText_cmpText.text = Strings.TransportStationsWindow.ASC;
                }
                else
                {
                    uiTSW.locationAndNameSortOrder = SortOrder.ASC;
                    goHeaders_childLocationAndName_childSort_childButtonText_cmpText.text = Strings.TransportStationsWindow.ASC;
                }
                uiTSW.OnSort();
            });

            // 创建 headers > item-slots
            GameObject goHeaders_childItemSlots = new GameObject("item-slots", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            goHeaders_childItemSlots.transform.parent = goHeaders.transform;

            RectTransform goHeaders_childItemSlots_cmpRectTransform = goHeaders_childItemSlots.GetComponent<RectTransform>();
            goHeaders_childItemSlots_cmpRectTransform.Zeroize();
            goHeaders_childItemSlots_cmpRectTransform.anchorMax = new Vector2(1, 1);
            goHeaders_childItemSlots_cmpRectTransform.offsetMin = new Vector2(150, 0);

            Text goHeaders_childItemSlots_cmpText = goHeaders_childItemSlots.GetComponent<Text>();
            goHeaders_childItemSlots_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            goHeaders_childItemSlots_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            goHeaders_childItemSlots_cmpText.font = ResourceCache.FontSAIRASB;
            goHeaders_childItemSlots_cmpText.text = Strings.TransportStationsWindow.ItemSlots;
            goHeaders_childItemSlots_cmpText.alignment = TextAnchor.MiddleCenter;

            // 创建 headers > item-slots > sep-line-1 和 headers > item-slots > sep-line-0
            GameObject goHeaders_childItemSlots_childSepLine1 = Instantiate(NativeObjectCache.SepLine1, goHeaders_childItemSlots.transform);

            RectTransform goHeaders_childItemSlots_childSepLine1_cmpRectTransform = goHeaders_childItemSlots_childSepLine1.GetComponent<RectTransform>();
            goHeaders_childItemSlots_childSepLine1_cmpRectTransform.Zeroize();
            goHeaders_childItemSlots_childSepLine1_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goHeaders_childItemSlots_childSepLine1_cmpRectTransform.offsetMax = new Vector2(0, -1);
            goHeaders_childItemSlots_childSepLine1_cmpRectTransform.offsetMin = new Vector2(-1, 1);

            GameObject goHeaders_childItemSlots_childSepLine0 = Instantiate(NativeObjectCache.SepLine0, goHeaders_childItemSlots.transform);

            RectTransform goHeaders_childItemSlots_childSepLine0_cmpRectTransform = goHeaders_childItemSlots_childSepLine0.GetComponent<RectTransform>();
            goHeaders_childItemSlots_childSepLine0_cmpRectTransform.Zeroize();
            goHeaders_childItemSlots_childSepLine0_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goHeaders_childItemSlots_childSepLine0_cmpRectTransform.offsetMax = new Vector2(1, -1);
            goHeaders_childItemSlots_childSepLine0_cmpRectTransform.offsetMin = new Vector2(0, 1);

            // 创建 headers > sep-line-1 和 headers > sep-line-0
            GameObject goHeaders_childSepLine1 = Instantiate(NativeObjectCache.SepLine1, goHeaders.transform);

            RectTransform goHeaders_childSepLine1_cmpRectTransform = goHeaders_childSepLine1.GetComponent<RectTransform>();
            goHeaders_childSepLine1_cmpRectTransform.Zeroize();
            goHeaders_childSepLine1_cmpRectTransform.anchorMax = new Vector2(1, 0);
            goHeaders_childSepLine1_cmpRectTransform.offsetMax = new Vector2(-1, 1);
            goHeaders_childSepLine1_cmpRectTransform.offsetMin = new Vector2(1, 0);

            GameObject goHeaders_childSepLine0 = Instantiate(NativeObjectCache.SepLine0, goHeaders.transform);

            RectTransform goHeaders_childSepLine0_cmpRectTransform = goHeaders_childSepLine0.GetComponent<RectTransform>();
            goHeaders_childSepLine0_cmpRectTransform.Zeroize();
            goHeaders_childSepLine0_cmpRectTransform.anchorMax = new Vector2(1, 0);
            goHeaders_childSepLine0_cmpRectTransform.offsetMax = new Vector2(-1, 0);
            goHeaders_childSepLine0_cmpRectTransform.offsetMin = new Vector2(1, -1);

            // ==========
            // UI组件创建完成（表头）
            // ==========

            // ==========
            // 开始创建UI组件（标题栏）
            // ==========

            GameObject goPanelBg = uiTSW.transform.Find("panel-bg").gameObject;

            // 创建 panel-bg > search-label
            GameObject goPanelBg_childSearchLabel = Instantiate(NewObjectCache.SimpleText, goPanelBg.transform);
            goPanelBg_childSearchLabel.name = "search-label";

            RectTransform goPanelBg_childSearchLabel_cmpRectTransform = goPanelBg_childSearchLabel.GetComponent<RectTransform>();
            goPanelBg_childSearchLabel_cmpRectTransform.Zeroize();
            goPanelBg_childSearchLabel_cmpRectTransform.anchorMin = goPanelBg_childSearchLabel_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goPanelBg_childSearchLabel_cmpRectTransform.offsetMin = new Vector2(200, -38);
            goPanelBg_childSearchLabel_cmpRectTransform.offsetMax = new Vector2(250, -13);

            Text goPanelBg_childSearchLabel_cmpText = goPanelBg_childSearchLabel.GetComponent<Text>();
            goPanelBg_childSearchLabel_cmpText.text = Strings.TransportStationsWindow.SearchLabel;

            // 创建 panel-bg > search
            GameObject goPanelBg_childSearch = Instantiate(ReassembledObjectCache.GOInputField, goPanelBg.transform);
            goPanelBg_childSearch.name = "search";

            RectTransform goPanelBg_childSearch_cmpRectTransform = goPanelBg_childSearch.GetComponent<RectTransform>();
            goPanelBg_childSearch_cmpRectTransform.Zeroize();
            goPanelBg_childSearch_cmpRectTransform.anchorMin = goPanelBg_childSearch_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goPanelBg_childSearch_cmpRectTransform.offsetMin = new Vector2(250, -37);
            goPanelBg_childSearch_cmpRectTransform.offsetMax = new Vector2(400, -12);

            InputField goPanelBg_childSearch_cmpInputField = goPanelBg_childSearch.GetComponent<InputField>();

            // 监听编辑结束的事件
            // 由于依赖了 search-clear ，这里对其进行提前声明
            GameObject goPanelBg_childSearchClear = null;
            goPanelBg_childSearch_cmpInputField.onEndEdit.AddListener((value) =>
            {
                uiTSW.searchString = value;
                uiTSW.OnSearch();

                // 搜索字符串不为空时才显示 search-clear 按钮
                goPanelBg_childSearchClear.SetActive(value.Length > 0);
            });

            UIButton goPanelBg_childSearch_cmpUIButton = goPanelBg_childSearch.GetComponent<UIButton>();
            goPanelBg_childSearch_cmpUIButton.transitions[0].normalColor = new Color(1, 1, 1, 0.15f);
            goPanelBg_childSearch_cmpUIButton.transitions[0].mouseoverColor = new Color(1, 1, 1, 0.2f);
            goPanelBg_childSearch_cmpUIButton.transitions[0].pressedColor = new Color(1, 1, 1, 0.05f);

            Image goPanelBg_childSearch_cmpImage = goPanelBg_childSearch.GetComponent<Image>();
            goPanelBg_childSearch_cmpImage.color = new Color(1, 1, 1, 0.15f);

            // 创建 panel-bg > search-clear
            goPanelBg_childSearchClear = UIUtility.CreateImageButton("search-clear", ResourceCache.SpriteXIcon, () =>
            {
                uiTSW.searchString = goPanelBg_childSearch_cmpInputField.text = "";
                uiTSW.OnSearch();

                goPanelBg_childSearchClear.SetActive(false);
            });
            goPanelBg_childSearchClear.transform.SetParent(goPanelBg.transform);
            goPanelBg_childSearchClear.SetActive(false);

            RectTransform goPanelBg_childSearchClear_cmpRectTransform = goPanelBg_childSearchClear.GetComponent<RectTransform>();
            goPanelBg_childSearchClear_cmpRectTransform.Zeroize();
            goPanelBg_childSearchClear_cmpRectTransform.anchorMin = goPanelBg_childSearchClear_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goPanelBg_childSearchClear_cmpRectTransform.offsetMin = new Vector2(378, -32);
            goPanelBg_childSearchClear_cmpRectTransform.offsetMax = new Vector2(395, -15);

            // 创建 panel-bg > search-button
            GameObject goPanelBg_childSearchButton = Instantiate(ReassembledObjectCache.GOTextButton, goPanelBg.transform);
            goPanelBg_childSearchButton.name = "search-button";

            RectTransform goPanelBg_childSearchButton_cmpRectTransform = goPanelBg_childSearchButton.GetComponent<RectTransform>();
            goPanelBg_childSearchButton_cmpRectTransform.Zeroize();
            goPanelBg_childSearchButton_cmpRectTransform.anchorMin = goPanelBg_childSearchButton_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goPanelBg_childSearchButton_cmpRectTransform.offsetMin = new Vector2(400, -37);
            goPanelBg_childSearchButton_cmpRectTransform.offsetMax = new Vector2(450, -12);

            goPanelBg_childSearchButton.transform.Find("button-text").GetComponent<Text>().text = Strings.TransportStationsWindow.SearchButton;

            goPanelBg_childSearchButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                // do nothing
            });

            // 创建 panel-bg > station-count-in-list
            GameObject goStationCountInList = UIStationCountInListTranslation.Create();
            goStationCountInList.transform.SetParent(goPanelBg.transform);
            goStationCountInList.name = "station-count-in-list";

            RectTransform goStationCountInList_cmpRectTransform = goStationCountInList.GetComponent<RectTransform>();
            goStationCountInList_cmpRectTransform.Zeroize();
            goStationCountInList_cmpRectTransform.anchorMin = goStationCountInList_cmpRectTransform.anchorMax = new Vector2(0, 1);
            goStationCountInList_cmpRectTransform.offsetMin = new Vector2(500, -40);
            goStationCountInList_cmpRectTransform.offsetMax = new Vector2(700, -10);

            uiTSW.uiStationCountInListTranslation = goStationCountInList.GetComponent<UIStationCountInListTranslation>();

            // 创建 panel-bg > allow-item-transfer-label
            GameObject goAllowItemTransferLabel = Instantiate(NewObjectCache.SimpleText, goPanelBg.transform);
            goAllowItemTransferLabel.name = "allow-item-transfer-label";

            RectTransform goAllowItemTransferLabel_cmpRectTransform = goAllowItemTransferLabel.GetComponent<RectTransform>();
            goAllowItemTransferLabel_cmpRectTransform.Zeroize();
            goAllowItemTransferLabel_cmpRectTransform.anchorMin = goAllowItemTransferLabel_cmpRectTransform.anchorMax = new Vector2(1, 1);
            goAllowItemTransferLabel_cmpRectTransform.offsetMin = new Vector2(-250, -38);
            goAllowItemTransferLabel_cmpRectTransform.offsetMax = new Vector2(-100, -13);

            Text goAllowItemTransferLabel_cmpText = goAllowItemTransferLabel.GetComponent<Text>();
            goAllowItemTransferLabel_cmpText.text = Strings.TransportStationsWindow.AllowItemTransferLabel;
            goAllowItemTransferLabel_cmpText.alignment = TextAnchor.MiddleRight;

            // 创建 panel-bg > allow-item-transfer-checkbox
            GameObject goAllowItemTransferCheckbox = Instantiate(NativeObjectCache.CheckBox, goPanelBg.transform);
            goAllowItemTransferCheckbox.name = "allow-item-transfer-checkbox";

            RectTransform goAllowItemTransferCheckbox_cmpRectTransform = goAllowItemTransferCheckbox.GetComponent<RectTransform>();
            goAllowItemTransferCheckbox_cmpRectTransform.Zeroize();
            goAllowItemTransferCheckbox_cmpRectTransform.anchorMin = goAllowItemTransferCheckbox_cmpRectTransform.anchorMax = new Vector2(1, 1);
            goAllowItemTransferCheckbox_cmpRectTransform.offsetMin = new Vector2(-90, -37);
            goAllowItemTransferCheckbox_cmpRectTransform.offsetMax = new Vector2(-70, -17);

            Toggle goAllowItemTransferCheckbox_cmpToggle = goAllowItemTransferCheckbox.GetComponent<Toggle>();

            goAllowItemTransferCheckbox_cmpToggle.isOn = isAllowItemTransfer;

            goAllowItemTransferCheckbox_cmpToggle.onValueChanged.AddListener(onIsAllowItemTransferValueChange);

            // 创建 panel-bg > debug-button
            //GameObject goPanelBg_childDebugButton = Instantiate(ReassembledObjectCache.GOTextButton, goPanelBg.transform);
            //goPanelBg_childDebugButton.name = "debug-button";

            //RectTransform goPanelBg_childDebugButton_cmpRectTransform = goPanelBg_childDebugButton.GetComponent<RectTransform>();
            //goPanelBg_childDebugButton_cmpRectTransform.Zeroize();
            //goPanelBg_childDebugButton_cmpRectTransform.anchorMin = goPanelBg_childDebugButton_cmpRectTransform.anchorMax = new Vector2(0, 1);
            //goPanelBg_childDebugButton_cmpRectTransform.offsetMin = new Vector2(900, -37);
            //goPanelBg_childDebugButton_cmpRectTransform.offsetMax = new Vector2(950, -12);

            //goPanelBg_childDebugButton.transform.Find("button-text").GetComponent<Text>().text = "DBG";

            //goPanelBg_childDebugButton.GetComponent<Button>().onClick.AddListener(() =>
            //{
            //    Plugin.Instance.Logger.LogInfo($"Language: {DSPGame.globalOption.language}");
            //});

            // ==========
            // UI组件创建完成（标题栏）
            // ==========

            // 删除克隆的 statWindow 组件
            Destroy(clonedStatWindow);

            return uiTSW;
        }

        /// <summary>
        /// 初始化窗口整体属性
        /// </summary>
        /// <param name="goTSW"></param>
        /// <returns></returns>
        static private UITransportStationsWindow Create_InitWindow (GameObject goTSW)
        {
            UITransportStationsWindow uiTSW = goTSW.AddComponent<UITransportStationsWindow>();
            uiTSW._Create(); // ManualBehaviour._Create()

            // 调整大小
            goTSW.GetComponent<RectTransform>().sizeDelta = new Vector2(1400, 880);

            // 修改名称
            goTSW.name = "DSPTransportStat_TransportStationsWindow";

            // 修改标题
            Transform titleTextTransform = goTSW.transform.Find("panel-bg/title-text");
            Destroy(titleTextTransform.GetComponent<Localizer>());
            titleTextTransform.GetComponent<Text>().text = Strings.TransportStationsWindow.Title;

            // 为右上角的关闭按钮添加事件
            goTSW.transform.Find("panel-bg/x").GetComponent<Button>().onClick.AddListener(uiTSW._Close);

            return uiTSW;
        }

        /// <summary>
        /// 初始化参数面板
        /// </summary>
        /// <param name="baseGameObject"></param>
        /// <param name="onParameterCallback"></param>
        /// <returns></returns>
        static private UITransportStationsWindowParameterPanel Create_InitParameterPanel (GameObject baseGameObject, Action onParameterChangeCallback)
        {
            baseGameObject.name = "parameter-panel";
            return UITransportStationsWindowParameterPanel.Create(baseGameObject, onParameterChangeCallback);
        }

        /// <summary>
        /// 初始化站点列表
        /// </summary>
        /// <param name="baseGameObject"></param>
        /// <returns></returns>
        static private UITransportStationsEntry[] Create_InitTable (GameObject baseGameObject)
        {
            baseGameObject.name = "transport-stations-bg";
            baseGameObject.SetActive(true);

            // 兼容 LSTM
            // 有些 mod 会给原来的 product-bg 添加新的按钮，如果那些 mod 先于本 mod 初始化，这里就会把其他 mod 的按钮复制过来
            // 所以这里要把除了 scroll-view 之外的其他子对象都删除
            for (int i = 0; i < baseGameObject.transform.childCount; ++i)
            {
                if (baseGameObject.transform.GetChild(i).gameObject.name != "scroll-view")
                {
                    Destroy(baseGameObject.transform.GetChild(i).gameObject);
                }
            }

            Transform listEntryParent = baseGameObject.transform.Find("scroll-view/viewport/content");

            UITransportStationsEntry[] uiTransportStationsEntries = new UITransportStationsEntry[Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT];
            for (int i = 0; i < Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT; ++i)
            {
                UITransportStationsEntry entry = UITransportStationsEntry.Create();
                entry.transform.SetParent(listEntryParent);
                uiTransportStationsEntries[i] = entry;
            };

            return uiTransportStationsEntries;
        }

        /// <summary>
        /// 加载所有的物流运输站、根据当前的物流运输站数量设置 content 的高度并重置滚动条的位置
        /// </summary>
        public void ComputeTransportStationsWindow_LoadStations ()
        {
            bool toggleInPlanet = uiTSWParameterPanel.ToggleInPlanet;
            bool toggleInterstellar = uiTSWParameterPanel.ToggleInterstellar;
            bool toggleCollector = uiTSWParameterPanel.ToggleCollector;
            int relatedItemFilter = uiTSWParameterPanel.RelatedItemFilter;

            stations.Clear();

            // 遍历每一个恒星中的每一个行星中的每一个物流运输站
            for (int i = 0; i < GameMain.galaxy.stars.Length; ++i)
            {
                StarData star = GameMain.galaxy.stars[i];

                for (int j = 0; j < star.planets.Length; ++j)
                {
                    PlanetData planet = star.planets[j];

                    // 玩家未抵达的行星上没有工厂
                    if (planet.factory?.transport?.stationPool == null)
                    {
                        continue;
                    }

                    for (int k = 0; k < planet.factory.transport.stationPool.Length; ++k)
                    {
                        StationComponent station = planet.factory.transport.stationPool[k];

                        // 如果拆除物流站点的话，会出现 station 不为 null 但是 entityId 为 0 的情况
                        if (station == null || station.entityId == 0)
                        {
                            continue;
                        }

                        // 是否显示行星内物流站
                        if (!toggleInPlanet && !station.isCollector && !station.isStellar)
                        {
                            continue;
                        }

                        // 是否显示星际物流运输站
                        if (!toggleInterstellar && !station.isCollector && station.isStellar)
                        {
                            continue;
                        }

                        // 是否显示采集站
                        if (!toggleCollector && station.isCollector)
                        {
                            continue;
                        }

                        // 通过搜索字符串对站点进行过滤
                        if (!string.IsNullOrWhiteSpace(searchString) && !star.name.Contains(searchString) && !planet.name.Contains(searchString) && !station.GetStationName().Contains(searchString))
                        {
                            continue;
                        }

                        // 过滤相关物品
                        if (relatedItemFilter != Constants.NONE_ITEM_ID)
                        {
                            // 该站点至少有一个槽位包含用户选择的物品
                            int ii = 0;
                            for (; ii < station.storage.Length; ++ii)
                            {
                                if (station.storage[ii].itemId == relatedItemFilter)
                                {
                                    break;
                                }
                            }
                            if (ii == station.storage.Length)
                            {
                                continue;
                            }
                        }

                        stations.Add(new StationInfoBundle(star, planet, station));
                    }
                }
            }

            // 加载站点列表时排一次序
            OnSort();

            // 设置 content 的高度为：物流运输站的数量 x 物流运输站的高度
            contentRectTransform.offsetMin = new Vector2(0, -Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT * stations.Count);

            // 重置滚动条
            contentRectTransform.offsetMax = new Vector2(0, 0);

            // 设置列表中的站点数量
            uiStationCountInListTranslation.SetNumber(stations.Count);
        }

        /// <summary>
        /// 根据当前 content 的高度、当前 content 的位置、当前 viewport 的高度以及每一项的高度，计算哪些物流运输站的数据需要被实际显示，
        /// 并设置 transport-stations-entry 的位置，使之恰好处于可视的 viewport 的范围内
        /// 
        /// 虚拟滚动
        /// </summary>
        private void ComputeTransportStationsWindow_VirtualScroll ()
        {
            // 内容高度
            float contentHeight = contentRectTransform.rect.height;

            // 内容向上滚动的距离就是当前内容的位置
            float contentPosition = contentRectTransform.offsetMax.y;

            // 视口高度
            float viewportHeight = viewportRectTransform.rect.height;

            int firstEntryIndex = (int)Math.Floor((double)contentPosition / (double)Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);
            if (firstEntryIndex < 0)
            {
                // 视口位置太靠上，视口的上边缘超过内容的上边缘
                firstEntryIndex = 0;
            }

            float firstEntryPosition = firstEntryIndex * Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT;
            int lastEntryIndex = (int)Math.Floor(((double)contentPosition + (double)viewportHeight) / (double)Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

            // 最后一个项目的索引必须在范围之内
            if (lastEntryIndex < 0)
            {
                // 视口位置太太靠上，视口的下边缘超过内容的上边缘
                lastEntryIndex = 0;
            }
            if (lastEntryIndex > stations.Count - 1)
            {
                // 视口位置太靠下，或者现有的站点数量太少，最后一个站点的位置触不到视口的下边缘
                // 如果没有站点的话， lastEntryIndex 会被置为 -1
                lastEntryIndex = stations.Count - 1;
            }

            if (lastEntryIndex < 0)
            {
                // 没有站点，不进行计算，将所有的 entry 都 SetActive(false)
                for (int i = 0; i < uiTransportStationsEntries.Length; ++i)
                {
                    GameObject go = uiTransportStationsEntries[i].RectTransform.gameObject;
                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                    }
                }
                return;
            }

            // 万一预先创建的 entry 对象不够多，也要限制最后一个项目的索引位置
            if (lastEntryIndex - firstEntryIndex + 1 > Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT)
            {
                Plugin.Instance.Logger.LogWarning($"Insufficient pre-created transport-stations-entry objects.");
                Plugin.Instance.Logger.LogWarning($"    Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT: {Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT}");
                Plugin.Instance.Logger.LogWarning($"    Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT: {Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT}");
                Plugin.Instance.Logger.LogWarning($"    contentHeight: {contentHeight}, contentPosition: {contentPosition}, viewportHeight: {viewportHeight}");
                Plugin.Instance.Logger.LogWarning($"    firstEntryIndex: {firstEntryIndex}, firstEntryPosition: {firstEntryPosition}, lastEntryIndex: {lastEntryIndex}");

                lastEntryIndex = Constants.DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT + firstEntryIndex - 1;

                Plugin.Instance.Logger.LogWarning($"    lastEntryIndex(modified): {lastEntryIndex}");
            }

            // 准备使用的 entry 对象个数
            int entriesToUseCount = lastEntryIndex - firstEntryIndex + 1;

            for (int i = 0; i < entriesToUseCount; ++i)
            {
                int entryIndex = firstEntryIndex + i;
                float entryPosition = firstEntryPosition + i * Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT;

                // 设置 stationComponent 和相关引用
                uiTransportStationsEntries[i].StationComponent = stations[entryIndex].Station;
                uiTransportStationsEntries[i].StarData = stations[entryIndex].Star;
                uiTransportStationsEntries[i].PlanetData = stations[entryIndex].Planet;

                // 设置大小和位置
                uiTransportStationsEntries[i].RectTransform.Zeroize();
                uiTransportStationsEntries[i].RectTransform.anchorMax = new Vector2(1, 1);
                uiTransportStationsEntries[i].RectTransform.anchorMin = new Vector2(0, 1);
                uiTransportStationsEntries[i].RectTransform.offsetMax = new Vector2(0, -entryPosition); // y 轴正方向朝上
                uiTransportStationsEntries[i].RectTransform.offsetMin = new Vector2(0, -entryPosition - Constants.TRANSPORT_STATIONS_ENTRY_HEIGHT);

                // 设置激活状态
                uiTransportStationsEntries[i].RectTransform.gameObject.SetActive(true);
            }

            for (int i = entriesToUseCount; i < uiTransportStationsEntries.Length; ++i)
            {
                // 剩余取消激活状态
                uiTransportStationsEntries[i].RectTransform.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 查询参数发生变化
        /// </summary>
        public void OnParameterChange ()
        {
            ComputeTransportStationsWindow_LoadStations();
        }

        /// <summary>
        /// 使用排序参数对 stations 进行排序
        /// </summary>
        public void OnSort ()
        {
            if (locationAndNameSortOrder == SortOrder.ASC)
            {
                stations.Sort(StationInfoBundle.CompareByLocationAndNameASC);
            }
            else if (locationAndNameSortOrder == SortOrder.DESC)
            {
                stations.Sort(StationInfoBundle.CompareByLocationAndNameDESC);
            }
            else
            {
                // NONE 不进行排序
            }
        }

        /// <summary>
        /// 用户输入搜索字符串
        /// </summary>
        public void OnSearch ()
        {
            ComputeTransportStationsWindow_LoadStations();
        }

        private void Update ()
        {
            ComputeTransportStationsWindow_VirtualScroll();
            //for (int i = 0; i < uiTransportStationsEntries.Length; ++i)
            //{
            //    uiTransportStationsEntries[i].Update();
            //}
        }

        public void OnPointerEnter (PointerEventData eventData)
        {
            IsPointerInside = true;
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            IsPointerInside = false;
        }

        protected override bool _OnInit ()
        {
            return true;
        }

        protected override void _OnClose ()
        {
            IsPointerInside = false;
            base._OnClose();
        }
    }
}
