using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DSPWhiteBuildMode.Cache;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace DSPWhiteBuildMode
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "IndexOutOfRange.DSPWhiteBuildMode";
        public const string PLUGIN_NAME = "DSPWhiteBuildMode";
        public const string PLUGIN_VERSION = "0.0.2";

        static public Plugin Instance { get; set; }

        new static public ManualLogSource Logger { get => Logger; }

        private bool isWhiteModeOn = false;

        private GameObject goConfigWindow = null;

        private void Awake ()
        {
            Instance = this;

            TryReadConfig();

            Harmony harmony = new Harmony(PLUGIN_GUID);

            // 针对mod新增的UI组件的补丁
            harmony.PatchAll(typeof(Patch_UIGame));

            harmony.PatchAll(typeof(Patch_PlayerAction_Build));

            harmony.PatchAll(typeof(Patch_FactoryModel));

            harmony.PatchAll(typeof(Patch_GameMain));
        }

        /// <summary>
        /// 读取配置文件，如果配置文件不存在的话，会创建配置文件
        /// </summary>
        private void TryReadConfig ()
        {
            // 尝试读取配置文件
            string modDataDir = GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPWhiteBuildMode/";
            if (!Directory.Exists(modDataDir))
            {
                Directory.CreateDirectory(modDataDir);
            }
            string configFilePath = modDataDir + "config.txt";
            if (!File.Exists(configFilePath))
            {
                // 如果没有找到配置文件，则创建配置文件
                StreamWriter writer = File.CreateText(configFilePath);
                // 写入默认值0，默认不打开白色建造模式
                writer.Write('0');
                writer.Flush();
                writer.Dispose();

                isWhiteModeOn = false;
                return;
            }

            // 如果存在配置文件，则使用配置文件中的内容初始化 isWhiteModeOn 成员
            if (!int.TryParse(File.ReadAllText(configFilePath).Trim(), out int value))
            {
                // 内容解析失败
                isWhiteModeOn = false;
                Logger.LogError("配置文件读取失败");
                return;
            }

            isWhiteModeOn = value != 0;
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        private void SaveConfig ()
        {
            File.WriteAllText(GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPWhiteBuildMode/config.txt", isWhiteModeOn ? "1" : "0");
        }

        /// <summary>
        /// 退出游戏时保存配置
        /// </summary>
        class Patch_GameMain
        {
            [HarmonyPostfix, HarmonyPatch(typeof(GameMain), nameof(GameMain.End))]
            static void GameMain_End_Postfix ()
            {
                Instance.SaveConfig();
            }
        }

        class Patch_UIGame
        {
            static private bool isCreated = false;

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            static void UIGame__OnCreate_Postfix ()
            {
                if (isCreated)
                {
                    return;
                }
                isCreated = true;

                ResourceCache.Initialize();
                NativeObjectCache.Initialize();
                ReassembledObjectCache.Initialize();

                Instance.goConfigWindow = UIWhiteBuildModeConfigWindow.Create(Instance.isWhiteModeOn, value => Instance.isWhiteModeOn = value);
            }
        }

        /// <summary>
        /// 检测是否在建造模式
        /// </summary>
        class Patch_PlayerAction_Build
        {
            public static bool IsInBuildMode = false;

            [HarmonyPostfix, HarmonyPatch(typeof(PlayerAction_Build), nameof(PlayerAction_Build.Open))]
            static void PlayerAction_Build_Open_Postfix ()
            {
                IsInBuildMode = true;
                if (Instance.goConfigWindow != null)
                {
                    Instance.goConfigWindow.SetActive(true);
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(PlayerAction_Build), nameof(PlayerAction_Build.Close))]
            static void PlayerAction_Build_Close_Postfix ()
            {
                IsInBuildMode = false;
                if (Instance.goConfigWindow != null)
                {
                    Instance.goConfigWindow.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 建造模式也像蓝图模式一样显示白模
        /// </summary>
        class Patch_FactoryModel
        {
            [HarmonyPrefix, HarmonyPatch(typeof(FactoryModel), nameof(FactoryModel.SetGlobalRenderState))]
            static bool FactoryModel_SetGlobalRenderState_Prefix (FactoryModel __instance)
            {
                FactoryModel.whiteMode0 = GameMain.mainPlayer.controller.actionBuild.blueprintMode > EBlueprintMode.None || (Patch_PlayerAction_Build.IsInBuildMode && Instance.isWhiteModeOn);
                Shader.SetGlobalFloat("_Global_WhiteMode0", FactoryModel.whiteMode0 ? 1 : 0);
                __instance.gpuiManager.renderVegetable = !FactoryModel.whiteMode0;
                return false;
            }
        }
    }
}
