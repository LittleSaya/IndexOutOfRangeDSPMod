using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
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

namespace DSPColorfulBuildingGrid
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "IndexOutOfRange.DSPColorfulBuildingGrid";
        public const string PLUGIN_NAME = "DSPColorfulBuildingGrid";
        public const string PLUGIN_VERSION = "0.0.1";

        static public Plugin Instance { get; set; }

        new public ManualLogSource Logger { get => base.Logger; }

        private bool isModEnabled = false;

        private int red = 0;

        private int green = 0;

        private int blue = 0;

        private int alpha = 0;

        private Color color { get => new Color((float)red / 255, (float)green / 255, (float)blue / 255, (float)alpha / 255); }

        private Color defaultColor;

        private GameObject goConfigWindow = null;

        private GameObject goColorPickerWindow = null;

        private void Awake ()
        {
            Instance = this;

            TryReadConfig();

            Harmony harmony = new Harmony(PLUGIN_GUID);

            harmony.PatchAll(typeof(Patch_GameMain));
            harmony.PatchAll(typeof(Patch_UIGame));
            harmony.PatchAll(typeof(Patch_PlayerAction_Build));
        }

        private void UpdateColor ()
        {
            Cache.NativeObjectCache.UIBuildingGrid.buildColor = isModEnabled ? color : defaultColor;
        }

        /// <summary>
        /// 读取配置文件，如果配置文件不存在的话，会创建配置文件
        /// </summary>
        private void TryReadConfig ()
        {
            // 尝试读取配置文件
            string modDataDir = GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPColorfulBuildingGrid/";
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
                writer.Write("0,0,0,0,0");
                writer.Flush();
                writer.Dispose();

                isModEnabled = false;
                red = green = blue = 0;
                return;
            }

            // 如果存在配置文件，则使用配置文件中的内容初始化
            string[] config = File.ReadAllText(configFilePath).Split(',');
            isModEnabled = int.Parse(config[0]) != 0;
            red = int.Parse(config[1]);
            green = int.Parse(config[2]);
            blue = int.Parse(config[3]);
            alpha = int.Parse(config[4]);
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        private void SaveConfig ()
        {
            string configFilePath = GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPColorfulBuildingGrid/config.txt";
            string configValue = $"{(isModEnabled ? '1' : '0')},{red},{green},{blue},{alpha}";
            File.WriteAllText(configFilePath, configValue);
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

                Cache.ResourceCache.Initialize();
                Cache.NativeObjectCache.Initialize();
                Cache.ReassembledObjectCache.Initialize();

                Instance.goConfigWindow = UIColorfulBuildingGridConfigWindow.Create(
                    Instance.isModEnabled,
                    value =>
                    {
                        Instance.isModEnabled = value;
                        Instance.UpdateColor();
                    },
                    () =>
                    {
                        Instance.goColorPickerWindow.SetActive(!Instance.goColorPickerWindow.activeSelf);
                    }
                );

                Instance.defaultColor = Cache.NativeObjectCache.UIBuildingGrid.buildColor;

                Instance.goColorPickerWindow = UIColorfulBuildingGridColorPickerWindow.Create(
                    Instance.red,
                    Instance.green,
                    Instance.blue,
                    Instance.alpha,
                    value =>
                    {
                        Instance.red = (int)value;
                        Instance.UpdateColor();
                    },
                    value =>
                    {
                        Instance.green = (int)value;
                        Instance.UpdateColor();
                    },
                    value =>
                    {
                        Instance.blue = (int)value;
                        Instance.UpdateColor();
                    },
                    value =>
                    {
                        Instance.alpha = (int)value;
                        Instance.UpdateColor();
                    }
                );

                Instance.UpdateColor();
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
                    Instance.goColorPickerWindow.SetActive(false);
                }
            }
        }
    }
}
