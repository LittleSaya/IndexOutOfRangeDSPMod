using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DSPTransportStat.CacheObjects;
using DSPTransportStat.Translation;
using System;
using System.Reflection;
using System.IO;

namespace DSPTransportStat
{
    [BepInPlugin(__GUID__, __NAME__, __VERSION__)]
    public class Plugin : BaseUnityPlugin
    {
        public const string __NAME__ = "DSPTransportStat";
        public const string __GUID__ = "IndexOutOfRange.DSPTransportStat";
        public const string __VERSION__ = "0.0.13";

        static public Plugin Instance { get; set; } = null;

        /// <summary>
        /// 插件日志对象
        /// </summary>
        new public ManualLogSource Logger { get => base.Logger; }

        private KeyboardShortcut transportStationsWindowShortcut;

        private UITransportStationsWindow uiTransportStationsWindow;

        private bool isAllowItemTransfer = false;

        private void Awake ()
        {
            Instance = this;

            TryReadConfig();

            transportStationsWindowShortcut = KeyboardShortcut.Deserialize("F + LeftControl");

            Harmony harmony = new Harmony(__GUID__);
            harmony.PatchAll(typeof(Patch_GameMain));
            harmony.PatchAll(typeof(Patch_UIGame));
            harmony.PatchAll(typeof(Patch_VFInput));
            harmony.PatchAll(typeof(Patch_UIStationWindow));
            harmony.PatchAll(typeof(Patch_UIStationStorage));
        }

        private void Update ()
        {
            if (!GameMain.isRunning || GameMain.isPaused || GameMain.instance.isMenuDemo || VFInput.inputing)
            {
                return;
            }

            if (VFInput.inputing)
            {
                return;
            }

            if (transportStationsWindowShortcut.IsDown())
            {
                ToggleTransportStationsWindow();
            }
            //else if (VFInput._closePanelE)
            //{
            //    if (uiTransportStationsWindow.active)
            //    {
            //        uiTransportStationsWindow._Close();
            //    }
            //}
        }

        private void ToggleTransportStationsWindow ()
        {
            if (uiTransportStationsWindow.active)
            {
                uiTransportStationsWindow._Close();
            }
            else
            {
                uiTransportStationsWindow._Open();
                uiTransportStationsWindow.transform.SetAsLastSibling();
                uiTransportStationsWindow.ComputeTransportStationsWindow_LoadStations();
            }
        }

        /// <summary>
        /// 读取配置文件，如果配置文件不存在的话，会创建配置文件
        /// </summary>
        private void TryReadConfig ()
        {
            // 尝试读取配置文件
            string modDataDir = GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPTransportStat/";
            if (!Directory.Exists(modDataDir))
            {
                Directory.CreateDirectory(modDataDir);
            }
            string configFilePath = modDataDir + "config.txt";
            if (!File.Exists(configFilePath))
            {
                // 如果没有找到配置文件，则创建配置文件
                StreamWriter writer = File.CreateText(configFilePath);

                // 写入默认值
                writer.Write("false");
                writer.Flush();
                writer.Dispose();

                isAllowItemTransfer = false;
                return;
            }

            // 如果存在配置文件，则使用配置文件中的内容初始化
            string[] config = File.ReadAllText(configFilePath).Split(',');
            isAllowItemTransfer = bool.Parse(config[0]);
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        private void SaveConfig ()
        {
            Logger.LogInfo($"SaveConfig: {isAllowItemTransfer}");
            string configFilePath = GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPTransportStat/config.txt";
            string configValue = $"{isAllowItemTransfer}";
            File.WriteAllText(configFilePath, configValue);
        }

        /// <summary>
        /// 退出游戏时保存配置，如果窗口没有关闭的话，关闭窗口
        /// </summary>
        class Patch_GameMain
        {
            [HarmonyPostfix, HarmonyPatch(typeof(GameMain), nameof(GameMain.End))]
            static void GameMain_End_Postfix ()
            {
                Instance.SaveConfig();
                if (Instance.uiTransportStationsWindow.active)
                {
                    Instance.uiTransportStationsWindow._Close();
                }
            }
        }

        class Patch_UIGame
        {
            static private bool isCreated = false;

            static private bool isInit = false;

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            static private void UIGame__OnCreate_Postfix ()
            {
                if (!isCreated)
                {
                    isCreated = true;
                    ResourceCache.InitializeResourceCache();
                    bool success = NativeObjectCache.InitializeNativeObjectCache(out string missingInfo);
                    if (!success)
                    {
                        Instance.Logger.LogError(missingInfo);
                    }
                    Strings.InitializeTranslations(DSPGame.globalOption.language);
                    ReassembledObjectCache.InitializeReassembledObjectCache();
                    NewObjectCache.InitializeNewObjectCache();
                    Instance.uiTransportStationsWindow = UITransportStationsWindow.Create(Instance.isAllowItemTransfer, value => Instance.isAllowItemTransfer = value);
                }
            }

            //[HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnFree")]
            //static public void UIGame__OnFree_Postfix ()
            //{
            //    Instance.uiTransportStationsWindow._Free();
            //}

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnUpdate")]
            static public void UIGame__OnUpdate_Postfix ()
            {
                if (GameMain.isPaused || !GameMain.isRunning)
                {
                    return;
                }
                Instance.uiTransportStationsWindow._Update();

                // 对 esc 键做出反应
                if (Instance.uiTransportStationsWindow.active && VFInput.escape)
                {
                    VFInput.UseEscape();
                    Instance.uiTransportStationsWindow._Close();
                }
            }

            //[HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnDestroy")]
            //static public void UIGame__OnDestroy_Postfix ()
            //{
            //    Instance.uiTransportStationsWindow._Destroy();
            //}

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnInit")]
            static private void UIGame__OnInit_Postfix (UIGame __instance)
            {
                if (!isInit)
                {
                    isInit = true;
                    Instance.uiTransportStationsWindow._Init(Instance.uiTransportStationsWindow.data);
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "ShutAllFunctionWindow")]
            static void UIGame_ShutAllFunctionWindow_Postfix ()
            {
                Instance.uiTransportStationsWindow._Close();
            }
        }

        class Patch_VFInput
        {
            [HarmonyPostfix, HarmonyPatch(typeof(VFInput), "_cameraZoomIn", MethodType.Getter)]
            static private void VFInput__cameraZoomIn_Postfix (ref float __result)
            {
                if (Instance.uiTransportStationsWindow != null && Instance.uiTransportStationsWindow.IsPointerInside)
                {
                    __result = 0f;
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(VFInput), "_cameraZoomOut", MethodType.Getter)]
            static private void VFInput__cameraZoomOut_Postfix (ref float __result)
            {
                if (Instance.uiTransportStationsWindow != null && Instance.uiTransportStationsWindow.IsPointerInside)
                {
                    __result = 0f;
                }
            }
        }

        public class Patch_UIStationWindow
        {
            static public bool IsOpenedFromPlugin
            {
                get => currentStationWindow != null;
            }

            static private UIStationWindow currentStationWindow = null;

            /// <summary>
            /// 打开任意一个物流运输站的站点窗口
            /// </summary>
            /// <param name="factory"></param>
            /// <param name="stationId"></param>
            static public void OpenStationWindowOfAnyStation (PlanetFactory factory, int stationId)
            {
                UIStationWindow win = UIRoot.instance.uiGame.stationWindow;

                // 模拟对 ManualBehaviour._Open 和 UIStationWindow._OnOpen 的调用
                // 这个模拟调用的过程与原有过程不同，以实现打开任意目标站点面板的功能（即使玩家与目标站点不在同一个星球上）
                if (win.inited && win.active)
                {
                    win._Close();
                }

                if (win.inited && !win.active)
                {
                    typeof(ManualBehaviour).GetProperty("active").SetValue(win, true);
                    if (!win.gameObject.activeSelf)
                    {
                        win.gameObject.SetActive(value: true);
                    }

                    try
                    {
                        win.factory = factory;
                        win.factorySystem = factory.factorySystem;
                        win.player = GameMain.mainPlayer;
                        win.powerSystem = factory.powerSystem;
                        win.transport = factory.transport;
                        win.stationId = stationId;

                        if (win.active)
                        {
                            win.nameInput.onValueChanged.AddListener(s => typeof(UIStationWindow).GetMethod("OnNameInputSubmit", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(win, new object[1] { s }));
                            win.nameInput.onEndEdit.AddListener(s => typeof(UIStationWindow).GetMethod("OnNameInputSubmit", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(win, new object[1] { s }));
                            currentStationWindow = win;
                            win.player.onIntendToTransferItems += OnPlayerIntendToTransferItems;
                        }
                        win.transform.SetAsLastSibling();

                        // 设置 UIGame 的 inspectStationId ，使之与手动打开的情况保持一致
                        typeof(UIGame).GetField("inspectStationId", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(UIRoot.instance.uiGame, stationId);
                    }
                    catch (Exception message)
                    {
                        Instance.Logger.LogError(message);
                    }
                }
            }

            /// <summary>
            /// 远程打开其他行星上的物流塔时，会将 factory, transport, powerSystem 和 factorySystem 替换成其他行星上的值
            /// 调用 OnStationIdChange 之前需要将它们重新赋予本地行星的值
            /// </summary>
            /// <param name="__instance"></param>
            [HarmonyPrefix, HarmonyPatch(typeof(UIStationWindow), "OnStationIdChange")]
            static void UIStationWindow_OnStationIdChange_Prefix (UIStationWindow __instance)
            {
                if (IsOpenedFromPlugin)
                {
                    PlanetFactory factory = GameMain.localPlanet.factory;
                    __instance.factory = factory;
                    __instance.transport = factory.transport;
                    __instance.powerSystem = factory.powerSystem;
                    __instance.factorySystem = factory.factorySystem;

                    // 通过 OnStationIdChange 打开物流塔，等于把从 mod 中远程打开的物流塔界面关闭
                    currentStationWindow = null;
                }
            }

            [HarmonyPrefix, HarmonyPatch(typeof(UIStationWindow), "_OnClose")]
            static void UIStationWindow__OnClose_Prefix ()
            {
                if (currentStationWindow != null && currentStationWindow.player != null)
                {
                    currentStationWindow.player.onIntendToTransferItems -= OnPlayerIntendToTransferItems;
                    currentStationWindow = null;
                }
            }

            static private void OnPlayerIntendToTransferItems (int _itemId, int _itemCount, int _itemInc)
            {
                MethodInfo method = typeof(UIStationWindow).GetMethod("OnPlayerIntendToTransferItems", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(currentStationWindow, new object[3] { _itemId, _itemCount, _itemInc });
            }
        }

        class Patch_UIStationStorage
        {
            [HarmonyPrefix, HarmonyPatch(typeof(UIStationStorage), nameof(UIStationStorage.OnItemIconMouseDown))]
            static bool UIStationStorage_OnItemIconMouseDown_Prefix ()
            {
                return Instance.isAllowItemTransfer || !Patch_UIStationWindow.IsOpenedFromPlugin;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(UIStationStorage), nameof(UIStationStorage.OnIconEnter))]
            static bool UIStationStorage_OnIconEnter_Prefix ()
            {
                return Instance.isAllowItemTransfer || !Patch_UIStationWindow.IsOpenedFromPlugin;
            }
        }
    }
}
