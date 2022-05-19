using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DSPMutableGridAltitudeRange
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "IndexOutOfRange.DSPMutableGridAltitudeRange";
        public const string PLUGIN_NAME = "DSPMutableGridAltitudeRange";
        public const string PLUGIN_VERSION = "0.0.1";

        private const float DEFAULT_ZMIN = -0.5f;
        private const float DEFAULT_ZMAX = 1f;

        static public Plugin Instance { get; set; }

        static private float GetZMin ()
        {
            return Instance.useCustomRange ? Instance._ZMin : DEFAULT_ZMIN;
        }

        static private float GetZMax ()
        {
            return Instance.useCustomRange ? Instance._ZMax : DEFAULT_ZMAX;
        }

        new public ManualLogSource Logger { get => base.Logger; }

        private float _ZMin = -0.5f;

        private float _ZMax = 1f;

        private bool useCustomRange = false;

        private GameObject goMutableGridAltitudeRange = null;

        private GameObject goMutableGridAltitudeRangeConfig = null;

        private void Awake ()
        {
            Instance = this;

            TryReadConfig();

            Harmony harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patch_GameMain));
            harmony.PatchAll(typeof(Patch_UIBuildingGrid));
            harmony.PatchAll(typeof(Patch_PlayerAction_Build));
            harmony.PatchAll(typeof(Patch_UIGame));
        }

        /// <summary>
        /// 读取配置文件，如果配置文件不存在的话，会创建配置文件
        /// </summary>
        private void TryReadConfig ()
        {
            // 尝试读取配置文件
            string modDataDir = GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPMutableGridAltitudeRange/";
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
                writer.Write("0,-0.5,1");
                writer.Flush();
                writer.Dispose();

                useCustomRange = false;
                _ZMin = DEFAULT_ZMIN;
                _ZMax = DEFAULT_ZMAX;
                return;
            }

            // 如果存在配置文件，则使用配置文件中的内容初始化
            string[] config = File.ReadAllText(configFilePath).Split(',');
            useCustomRange = int.Parse(config[0]) != 0;
            _ZMin = float.Parse(config[1]);
            _ZMax = float.Parse(config[2]);
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        private void SaveConfig ()
        {
            string configFilePath = GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPMutableGridAltitudeRange/config.txt";
            string configValue = $"{(useCustomRange ? '1' : '0')},{_ZMin},{_ZMax}";
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

                Instance.goMutableGridAltitudeRange = UIMutableGridAltitudeRange.Create(
                    Instance.useCustomRange,
                    value =>
                    {
                        Instance.useCustomRange = value;
                    },
                    () =>
                    {
                        Instance.goMutableGridAltitudeRangeConfig.SetActive(!Instance.goMutableGridAltitudeRangeConfig.activeSelf);
                    }
                );

                Instance.goMutableGridAltitudeRangeConfig = UIMutableGridAltitudeRangeConfig.Create(
                    Instance._ZMin,
                    Instance._ZMax,
                    value =>
                    {
                        Instance._ZMin = value;
                    },
                    value =>
                    {
                        Instance._ZMax = value;
                    }
                );
            }
        }

        class Patch_UIBuildingGrid
        {
            [HarmonyTranspiler, HarmonyPatch(typeof(UIBuildingGrid), "Update")]
            static IEnumerable<CodeInstruction> UIBuildingGrid_Update_Transpiler (IEnumerable<CodeInstruction> instructions)
            {
                CodeMatcher matcher = new CodeMatcher(instructions);

                matcher.End();

                // 找到最后一个 ldstr "_ZMin"
                matcher.MatchBack(true, new CodeMatch(OpCodes.Ldstr, "_ZMin"));

                if (matcher.Pos >= matcher.Length)
                {
                    Instance.Logger.LogError("找不到 ldstr \"_ZMin\"");
                    return instructions;
                }

                matcher.Advance(-2);
                bool flag1 = matcher.Opcode == OpCodes.Ldarg_0;
                CodeInstruction code1 = matcher.Instruction.Clone();

                matcher.Advance(1);
                bool flag2 = matcher.Opcode == OpCodes.Ldfld && matcher.Operand.ToString() == "UnityEngine.Material material";
                CodeInstruction code2 = matcher.Instruction.Clone();

                matcher.Advance(1);
                bool flag3 = matcher.Opcode == OpCodes.Ldstr && matcher.Operand.ToString() == "_ZMin";
                CodeInstruction code3 = matcher.Instruction.Clone("_ZMax");

                matcher.Advance(1);
                bool flag4 = matcher.Opcode == OpCodes.Ldc_R4 && (float)matcher.Operand < 0f;
                CodeInstruction code4 = new CodeInstruction(OpCodes.Call, typeof(Plugin).GetMethod(nameof(Plugin.GetZMax), BindingFlags.Static | BindingFlags.NonPublic));

                matcher.Advance(1);
                bool flag5 = matcher.Opcode == OpCodes.Callvirt && matcher.Operand.ToString() == "Void SetFloat(System.String, Single)";
                CodeInstruction code5 = matcher.Instruction.Clone();

                if (!flag1 || !flag2 || !flag3 || !flag4 || !flag5)
                {
                    Instance.Logger.LogError($"ldstr \"_ZMin\" 附近的 IL 码不正确");
                    return instructions;
                }

                // 修改 _ZMin
                matcher.Advance(-1);
                matcher.Set(OpCodes.Call, typeof(Plugin).GetMethod(nameof(Plugin.GetZMin), BindingFlags.Static | BindingFlags.NonPublic));
                matcher.Advance(1);

                // 修改 _ZMax
                matcher.Advance(1);
                matcher.Insert(code1, code2, code3, code4, code5);

                return matcher.InstructionEnumeration();
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
                if (Instance.goMutableGridAltitudeRange != null)
                {
                    Instance.goMutableGridAltitudeRange.SetActive(true);
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(PlayerAction_Build), nameof(PlayerAction_Build.Close))]
            static void PlayerAction_Build_Close_Postfix ()
            {
                IsInBuildMode = false;
                if (Instance.goMutableGridAltitudeRange != null)
                {
                    Instance.goMutableGridAltitudeRange.SetActive(false);
                    Instance.goMutableGridAltitudeRangeConfig.SetActive(false);
                }
            }
        }
    }
}
