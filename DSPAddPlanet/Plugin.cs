using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DSPAddPlanet.Extensions;
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
using UnityEngine.Rendering;

namespace DSPAddPlanet
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "IndexOutOfRange.DSPAddPlanet";
        public const string PLUGIN_NAME = "DSPAddPlanet";
        public const string PLUGIN_VERSION = "0.2.2";

        static public Plugin Instance { get => instance; }
        static private Plugin instance = null;
        new public ManualLogSource Logger { get => base.Logger; }

        /// <summary>
        /// 全局行星配置
        /// </summary>
        private Dictionary<string, List<AdditionalPlanetConfig>> globalPlanetConfig = new Dictionary<string, List<AdditionalPlanetConfig>>();

        /// <summary>
        /// 针对特定游戏名称的行星配置
        /// </summary>
        private Dictionary<string, List<AdditionalPlanetConfig>> gameNameSpecificConfig = new Dictionary<string, List<AdditionalPlanetConfig>>();

        /// <summary>
        /// 用户指定的出生恒星
        /// </summary>
        private int specialBirthStarId = 0;

        /// <summary>
        /// 用户指定的出生行星
        /// </summary>
        private int specialBirthPlanetId = 0;

        /// <summary>
        /// UIAddPlanet UI组件
        /// </summary>
        private UIAddPlanet uiAddPlanet = null;

        private void Awake ()
        {
            instance = this;

            ILUtility.Initialize(Logger);

            PlatformSystem.segmentTable = Global.fixedPlatformSystemSegmentTable;

            Harmony harmony = new Harmony(PLUGIN_GUID);

            // 配置文件载入
            harmony.PatchAll(typeof(Patch_GameData));

            // 核心业务功能
            harmony.PatchAll(typeof(Patch_StarGen));
            harmony.PatchAll(typeof(Patch_UniverseGen));
            harmony.PatchAll(typeof(Patch_PlanetAlgorithms));

            // 修正游戏行为
            harmony.PatchAll(typeof(Patch_PlanetModeling));
            harmony.PatchAll(typeof(Patch_TrashSystem));
            harmony.PatchAll(typeof(Patch_PlayerController));
            harmony.PatchAll(typeof(Patch_StationComponent));
            harmony.PatchAll(typeof(Patch_PlanetSimulator));
            harmony.PatchAll(typeof(Patch_PlanetGrid));
            harmony.PatchAll(typeof(Patch_PlatformSystem));
            harmony.PatchAll(typeof(Patch_BuildTool_BlueprintPaste));
            harmony.PatchAll(typeof(Patch_PlanetFactory));

            // 创建用户界面
            harmony.PatchAll(typeof(Patch_UIGame));

            //harmony.PatchAll(typeof(Patch_Debug));

            //Utility.PrintThemeTable();
        }

        /// <summary>
        /// 创建UI组件
        /// </summary>
        private void CreateUI ()
        {
            uiAddPlanet = UIAddPlanet.Create();

            // 在星图界面创建按钮
            UIUtility.CreateTextButton(
                "Add Planet",
                () => {
                    if (uiAddPlanet.active)
                    {
                        uiAddPlanet._Close();
                    }
                    else
                    {
                        uiAddPlanet._Open();
                        uiAddPlanet.transform.SetAsLastSibling();
                    }
                },
                "add-planet-button",
                UIRoot.instance.transform.Find("Overlay Canvas/In Game/Starmap UIs"),
                new Vector2(1, 0),
                new Vector2(1, 0),
                new Vector2(-190, 10),
                new Vector2(-100, 40)
            );
        }

        /// <summary>
        /// 游戏数据的存取
        /// </summary>
        class Patch_GameData
        {
            /// <summary>
            /// 每次载入存档之前，都重新读取一次配置文件，同时清空用户设置的出生点
            /// </summary>
            [HarmonyPrefix, HarmonyPatch(typeof(GameData), nameof(GameData.Import))]
            static void GameData_Import_Prefix ()
            {
                ConfigUtility.ReadConfig(Instance.globalPlanetConfig, Instance.gameNameSpecificConfig);
                Instance.specialBirthStarId = Instance.specialBirthPlanetId = 0;
            }
        }

        /// <summary>
        /// 创建新行星
        /// </summary>
        class Patch_StarGen
        {
            /// <summary>
            /// CreateStarPlanets 会为每个恒星调用一次
            /// </summary>
            /// <param name="galaxy"></param>
            /// <param name="star"></param>
            /// <param name="gameDesc"></param>
            [HarmonyPostfix, HarmonyPatch(typeof(StarGen), nameof(StarGen.CreateStarPlanets))]
            static void StarGen_CreateStarPlanets_Postfix (GalaxyData galaxy, StarData star, GameDesc gameDesc)
            {
                List<AdditionalPlanetConfig> configList = Utility.GetPlanetConfigList(
                    GameMain.gameName,
                    gameDesc.clusterString,
                    star.name,
                    Instance.globalPlanetConfig,
                    Instance.gameNameSpecificConfig,
                    out string uniqueStarId
                );
                if (configList == null)
                {
                    // 未能获取到当前行星的配置
                    return;
                }

                // 确定需要创建多少个新的行星
                int newPlanetsCount = 0;
                foreach (AdditionalPlanetConfig config in configList) {
                    bool exists = false;
                    foreach (PlanetData planet in star.planets) {
                        if (planet.index == config.Index) {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists) {
                        ++newPlanetsCount;
                    }
                }

                Instance.Logger.LogInfo($"{configList.Count} configurations, {newPlanetsCount} new planets in {uniqueStarId} (current game name: {GameMain.gameName}");

                // Index必须连续，不能有空的行星
                const int STATE_OLD = 0;
                const int STATE_NEW = 1;
                const int STATE_EMPTY = 2;
                int[] indexConsistentCheck = new int[star.planets.Length + newPlanetsCount];
                for (int i = 0; i < indexConsistentCheck.Length; ++i) {
                    indexConsistentCheck[i] = STATE_EMPTY;
                }
                foreach (PlanetData planet in star.planets) {
                    indexConsistentCheck[planet.index] = STATE_OLD;
                }
                foreach (AdditionalPlanetConfig config in configList) {
                    indexConsistentCheck[config.Index] = STATE_NEW;
                }
                for (int i = 0; i < indexConsistentCheck.Length; ++i) {
                    if (indexConsistentCheck[i] == STATE_EMPTY) {
                        throw new Exception($"Empty planet in {uniqueStarId}, planet index: {i} (current game name: {GameMain.gameName}). Please check the consistency of planets' indexes");
                    }
                }

                Instance.Logger.LogInfo("Index consistency checked");

                // 创建新的列表，复制原有行星的引用，然后把原来的行星列表替换掉
                int originalPlanetsCount = star.planets.Length;
                PlanetData[] tempPlanets = new PlanetData[star.planets.Length + newPlanetsCount];
                Array.Copy(star.planets, tempPlanets, star.planets.Length);
                star.planets = tempPlanets;
                star.planetCount = tempPlanets.Length;

                // 创建新的行星
                foreach (AdditionalPlanetConfig config in configList)
                {
                    // 检查该配置中的行星index是否是已经存在的行星的index
                    bool isExistingPlanet = false;
                    int existingPlanetIndex = 0;
                    for (int i = 0; i < originalPlanetsCount; ++i)
                    {
                        if (star.planets[i].index == config.Index)
                        {
                            isExistingPlanet = true;
                            existingPlanetIndex = config.Index;
                            break;
                        }
                    }

                    PlanetData planet;
                    if (!isExistingPlanet)
                    {
                        // 阶段：创建行星
                        planet = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, config.Index, config.OrbitAround, config.OrbitIndex, config.Number, config.GasGiant, config.InfoSeed, config.GenSeed);
                        Instance.Logger.LogInfo($"Created new planet at {uniqueStarId}. Index: {config.Index}, Number: {config.Number}, Orbit index: {config.OrbitIndex}, Gas giant: {config.GasGiant}, Theme id: {config.ThemeId}");
                    }
                    else
                    {
                        // 获取已经存在的行星
                        planet = star.planets[existingPlanetIndex];
                        Instance.Logger.LogInfo($"Replace existing planet at {uniqueStarId}. Index: {config.Index}, Number: {config.Number}, Orbit index: {config.OrbitIndex}, Gas giant: {config.GasGiant}, Theme id: {config.ThemeId}");
                    }

                    // 检查并更新出生位置
                    if (config.IsBirthPoint)
                    {
                        if (Instance.specialBirthStarId != 0 || Instance.specialBirthPlanetId != 0)
                        {
                            Instance.Logger.LogWarning($"Birth point overriding in {uniqueStarId}, from star id {Instance.specialBirthStarId}, planet id {Instance.specialBirthPlanetId}, to star id {planet.star.id}, planet id {planet.id}");
                        }
                        Instance.specialBirthStarId = planet.star.id;
                        Instance.specialBirthPlanetId = planet.id;
                    }

                    // 阶段：后处理
                    // 调整行星半径
                    if (!config.GasGiant)
                    {
                        planet.radius = config.Radius;
                        planet.scale = 1f;
                        planet.precision = 200;
                        planet.segment = 5;
                        star.galaxy.astrosData[planet.id].uRadius = planet.realRadius;
                    }

                    // 潮汐锁定
                    planet.orbitalPeriod = config.OrbitalPeriod;
                    planet.rotationPeriod = config.RotationPeriod;
                    if (config.IsTidalLocked)
                    {
                        planet.singularity |= EPlanetSingularity.TidalLocked;
                    }

                    // 轨道倾角和地轴倾角
                    planet.orbitInclination = config.OrbitInclination;
                    planet.runtimeOrbitRotation = Quaternion.AngleAxis(planet.orbitLongitude, Vector3.up) * Quaternion.AngleAxis(planet.orbitInclination, Vector3.forward);
                    if (planet.orbitAroundPlanet != null)
                    {
                        planet.runtimeOrbitRotation = planet.orbitAroundPlanet.runtimeOrbitRotation * planet.runtimeOrbitRotation;
                    }
                    planet.obliquity = config.Obliquity;
                    planet.runtimeSystemRotation = planet.runtimeOrbitRotation * Quaternion.AngleAxis(planet.obliquity, Vector3.forward);

                    // 升交点经度
                    if (config._HasOrbitLongitude)
                    {
                        if (planet.orbitInclination >= 0)
                        {
                            planet.orbitLongitude = 180f - config.OrbitLongitude;
                        }
                        else
                        {
                            planet.orbitLongitude = -config.OrbitLongitude;
                        }
                        planet.runtimeOrbitRotation = Quaternion.AngleAxis(planet.orbitLongitude, Vector3.up) * Quaternion.AngleAxis(planet.orbitInclination, Vector3.forward);
                        if (planet.orbitAroundPlanet != null)
                        {
                            planet.runtimeOrbitRotation = planet.orbitAroundPlanet.runtimeOrbitRotation * planet.runtimeOrbitRotation;
                        }
                        planet.runtimeSystemRotation = planet.runtimeOrbitRotation * Quaternion.AngleAxis(planet.obliquity, Vector3.forward);
                    }

                    // 行星主题（逻辑源自 PlanetGen.SetPlanetTheme ）
                    if (config._HasThemeId)
                    {
                        ThemeProto theme = LDB.themes.Select(config.ThemeId);
                        DotNet35Random random = new DotNet35Random(config.InfoSeed);
                        double rand1 = random.NextDouble();
                        double rand2 = random.NextDouble();
                        double rand3 = random.NextDouble();
                        double rand4 = random.NextDouble();
                        int themeSeed = random.Next();

                        planet.theme = config.ThemeId;
                        planet.algoId = 0;
                        if (theme.Algos != null && theme.Algos.Length != 0)
                        {
                            planet.algoId = theme.Algos[(int)(rand2 * (double)theme.Algos.Length) % theme.Algos.Length];
                            planet.mod_x = (double)theme.ModX.x + rand3 * (double)(theme.ModX.y - theme.ModX.x);
                            planet.mod_y = (double)theme.ModY.x + rand4 * (double)(theme.ModY.y - theme.ModY.x);
                        }

                        planet.style = themeSeed % 60;
                        planet.type = theme.PlanetType;
                        planet.ionHeight = theme.IonHeight;
                        planet.windStrength = theme.Wind;
                        planet.waterHeight = theme.WaterHeight;
                        planet.waterItemId = theme.WaterItemId;
                        planet.levelized = theme.UseHeightForBuild;
                        planet.iceFlag = theme.IceFlag;
                        if (planet.type == EPlanetType.Gas)
                        {
                            int num2 = theme.GasItems.Length;
                            int num3 = theme.GasSpeeds.Length;
                            int[] array = new int[num2];
                            float[] array2 = new float[num3];
                            float[] array3 = new float[num2];
                            for (int n = 0; n < num2; n++)
                            {
                                array[n] = theme.GasItems[n];
                            }
                            double num4 = 0.0;
                            DotNet35Random dotNet35Random = new DotNet35Random(themeSeed);
                            for (int num5 = 0; num5 < num3; num5++)
                            {
                                float num6 = theme.GasSpeeds[num5];
                                num6 *= (float)dotNet35Random.NextDouble() * 0.190909147f + 0.9090909f;
                                array2[num5] = num6 * Mathf.Pow(planet.star.resourceCoef, 0.3f);
                                ItemProto itemProto = LDB.items.Select(array[num5]);
                                array3[num5] = itemProto.HeatValue;
                                num4 += (double)(array3[num5] * array2[num5]);
                            }
                            planet.gasItems = array;
                            planet.gasSpeeds = array2;
                            planet.gasHeatValues = array3;
                            planet.gasTotalHeat = num4;
                        }
                    }

                    // 替换或者新增行星
                    if (isExistingPlanet) {
                        star.planets[existingPlanetIndex] = planet;
                    } else {
                        star.planets[planet.index] = planet;
                    }
                }
            }
        }

        /// <summary>
        /// 设置玩家的出生点
        /// </summary>
        class Patch_UniverseGen
        {
            [HarmonyPostfix, HarmonyPatch(typeof(UniverseGen), nameof(UniverseGen.CreateGalaxy))]
            static void UniverseGen_CreateGalaxy_Postfix (GalaxyData __result)
            {
                if (Instance.specialBirthStarId != 0 && Instance.specialBirthPlanetId != 0)
                {
                    __result.birthStarId = Instance.specialBirthStarId;
                    __result.birthPlanetId = Instance.specialBirthPlanetId;
                }
            }
        }

        /// <summary>
        /// 矿脉与地形
        /// </summary>
        class Patch_PlanetAlgorithms
        {
            static DotNet35Random random = new DotNet35Random();

            // GenerateVeins Prefix 开始
            // 阶段：矿脉生成Prefix
            // 主要影响矿脉是否生成
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
            static bool PlanetAlgorithm_GenerateVeins_Prefix (PlanetAlgorithm __instance) => PlanetAlgorithmX_GenerateVeins_Prefix(typeof(PlanetAlgorithm), __instance);

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm0), nameof(PlanetAlgorithm0.GenerateVeins))]
            static bool PlanetAlgorithm0_GenerateVeins_Prefix (PlanetAlgorithm0 __instance) => PlanetAlgorithmX_GenerateVeins_Prefix(typeof(PlanetAlgorithm0), __instance);

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm7), nameof(PlanetAlgorithm7.GenerateVeins))]
            static bool PlanetAlgorithm7_GenerateVeins_Prefix (PlanetAlgorithm7 __instance) => PlanetAlgorithmX_GenerateVeins_Prefix(typeof(PlanetAlgorithm7), __instance);

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm11), nameof(PlanetAlgorithm11.GenerateVeins))]
            static bool PlanetAlgorithm11_GenerateVeins_Prefix (PlanetAlgorithm11 __instance) => PlanetAlgorithmX_GenerateVeins_Prefix(typeof(PlanetAlgorithm11), __instance);

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm12), nameof(PlanetAlgorithm12.GenerateVeins))]
            static bool PlanetAlgorithm12_GenerateVeins_Prefix (PlanetAlgorithm12 __instance) => PlanetAlgorithmX_GenerateVeins_Prefix(typeof(PlanetAlgorithm12), __instance);

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm13), nameof(PlanetAlgorithm13.GenerateVeins))]
            static bool PlanetAlgorithm13_GenerateVeins_Prefix (PlanetAlgorithm13 __instance) => PlanetAlgorithmX_GenerateVeins_Prefix(typeof(PlanetAlgorithm13), __instance);

            static bool PlanetAlgorithmX_GenerateVeins_Prefix (Type type, object instance)
            {
                PlanetData planet = (PlanetData)AccessTools.Field(type, "planet").GetValue(instance);
                List<AdditionalPlanetConfig> configList = Utility.GetPlanetConfigList(
                    GameMain.gameName,
                    GameMain.data.gameDesc.clusterString,
                    planet.star.name,
                    Instance.globalPlanetConfig,
                    Instance.gameNameSpecificConfig,
                    out string _
                );
                if (configList == null)
                {
                    // 没有针对该恒星的配置，正常生成行星的矿脉
                    return true;
                }

                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index)
                    {
                        continue;
                    }

                    if (!config.DontGenerateVein)
                    {
                        // 如果新增行星需要生成矿脉，则返回 true ，执行游戏自带的 GenerateVeins 函数
                        return true;
                    }
                    else
                    {
                        // 如果新增行星不需要生成矿脉，则返回 false ，跳过游戏自带的 GenerateVeins 函数
                        return false;
                    }
                }

                // 虽然该恒星有新增行星，但当前正在生成矿脉的行星还不是新的行星
                return true;
            }
            // GenerateVeins Prefix 结束

            // GenerateVeins PostFix 开始
            // 阶段：矿脉生成Postfix
            // 主要使 ReplaceAllVeinsTo 发挥作用
            [HarmonyPostfix, HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
            static void PlanetAlgorithm_GenerateVeins_Postfix (PlanetAlgorithm __instance) => PlanetAlgorithmX_GenerateVeins_Postfix(typeof(PlanetAlgorithm), __instance);

            [HarmonyPostfix, HarmonyPatch(typeof(PlanetAlgorithm0), nameof(PlanetAlgorithm0.GenerateVeins))]
            static void PlanetAlgorithm0_GenerateVeins_Postfix (PlanetAlgorithm0 __instance) => PlanetAlgorithmX_GenerateVeins_Postfix(typeof(PlanetAlgorithm0), __instance);

            [HarmonyPostfix, HarmonyPatch(typeof(PlanetAlgorithm7), nameof(PlanetAlgorithm7.GenerateVeins))]
            static void PlanetAlgorithm7_GenerateVeins_Postfix (PlanetAlgorithm7 __instance) => PlanetAlgorithmX_GenerateVeins_Postfix(typeof(PlanetAlgorithm7), __instance);

            [HarmonyPostfix, HarmonyPatch(typeof(PlanetAlgorithm11), nameof(PlanetAlgorithm11.GenerateVeins))]
            static void PlanetAlgorithm11_GenerateVeins_Postfix (PlanetAlgorithm11 __instance) => PlanetAlgorithmX_GenerateVeins_Postfix(typeof(PlanetAlgorithm11), __instance);

            [HarmonyPostfix, HarmonyPatch(typeof(PlanetAlgorithm12), nameof(PlanetAlgorithm12.GenerateVeins))]
            static void PlanetAlgorithm12_GenerateVeins_Postfix (PlanetAlgorithm12 __instance) => PlanetAlgorithmX_GenerateVeins_Postfix(typeof(PlanetAlgorithm12), __instance);

            [HarmonyPostfix, HarmonyPatch(typeof(PlanetAlgorithm13), nameof(PlanetAlgorithm13.GenerateVeins))]
            static void PlanetAlgorithm13_GenerateVeins_Postfix (PlanetAlgorithm13 __instance) => PlanetAlgorithmX_GenerateVeins_Postfix(typeof(PlanetAlgorithm13), __instance);

            static void PlanetAlgorithmX_GenerateVeins_Postfix (Type type, object instance)
            {
                PlanetData planet = (PlanetData)AccessTools.Field(type, "planet").GetValue(instance);
                List<AdditionalPlanetConfig> configList = Utility.GetPlanetConfigList(
                    GameMain.gameName,
                    GameMain.data.gameDesc.clusterString,
                    planet.star.name,
                    Instance.globalPlanetConfig,
                    Instance.gameNameSpecificConfig,
                    out string _
                );
                if (configList == null)
                {
                    // 没有针对该恒星的配置，不考虑替换矿脉的情况
                    return;
                }

                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index || !config._HasReplaceAllVeinsTo)
                    {
                        continue;
                    }

                    if (planet.index == config.Index)
                    {
                        // 替换 PlanetRawData.veinPool 中的内容
                        DotNet35Random dotNet35Random = new DotNet35Random(planet.seed);
                        int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
                        int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
                        int[] veinProducts = PlanetModelingManager.veinProducts;
                        int replacedVeinTypeIndex = (int)config.ReplaceAllVeinsTo;

                        PlanetRawData raw = planet.data;
                        VeinData[] veinDatas = raw.veinPool;
                        for (int i = 0; i < veinDatas.Length; ++i)
                        {
                            if (veinDatas[i].id != 0)
                            {
                                veinDatas[i].type = config.ReplaceAllVeinsTo;
                                veinDatas[i].modelIndex = (short)dotNet35Random.Next(veinModelIndexs[replacedVeinTypeIndex], veinModelIndexs[replacedVeinTypeIndex] + veinModelCounts[replacedVeinTypeIndex]);
                                veinDatas[i].productId = veinProducts[replacedVeinTypeIndex];
                            }
                        }
                    }
                }

                // 虽然该恒星有新增行星，但当前行星还不是新的行星
                return;
            }
            // GenerateVeins PostFix 结束

            // GenerateVeins Transpiler 开始
            // 阶段：矿脉生成Transpiler
            // 主要使 VeinCustom 发挥作用
            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
            static IEnumerable<CodeInstruction> PlanetAlgorithm_GenerateVeins_Transpiler (IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                => PlanetAlgorithmX_GenerateVeins_Transpiler (typeof(PlanetAlgorithm), instructions, generator);

            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetAlgorithm7), nameof(PlanetAlgorithm7.GenerateVeins))]
            static IEnumerable<CodeInstruction> PlanetAlgorithm7_GenerateVeins_Transpiler (IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                => PlanetAlgorithmX_GenerateVeins_Transpiler(typeof(PlanetAlgorithm7), instructions, generator);

            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetAlgorithm11), nameof(PlanetAlgorithm11.GenerateVeins))]
            static IEnumerable<CodeInstruction> PlanetAlgorithm11_GenerateVeins_Transpiler (IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                => PlanetAlgorithmX_GenerateVeins_Transpiler(typeof(PlanetAlgorithm11), instructions, generator);

            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetAlgorithm12), nameof(PlanetAlgorithm12.GenerateVeins))]
            static IEnumerable<CodeInstruction> PlanetAlgorithm12_GenerateVeins_Transpiler (IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                => PlanetAlgorithmX_GenerateVeins_Transpiler(typeof(PlanetAlgorithm12), instructions, generator);

            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetAlgorithm13), nameof(PlanetAlgorithm13.GenerateVeins))]
            static IEnumerable<CodeInstruction> PlanetAlgorithm13_GenerateVeins_Transpiler (IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                => PlanetAlgorithmX_GenerateVeins_Transpiler(typeof(PlanetAlgorithm13), instructions, generator);

            static IEnumerable<CodeInstruction> PlanetAlgorithmX_GenerateVeins_Transpiler (Type type, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                CodeMatcher matcher = new CodeMatcher(instructions);

                // 一、初始化随机数生成器
                matcher.Start();
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetAlgorithm), "planet")),
                    new CodeInstruction(OpCodes.Call, typeof(Patch_PlanetAlgorithms).GetMethod("InitializeRandomGenerator", BindingFlags.Static | BindingFlags.NonPublic))
                );

                // 二、修改各类矿物的矿脉数量
                // 寻找第一个 conv.u1 指令（把数字转换为枚举的指令）
                matcher.MatchForward(true, new CodeMatch(OpCodes.Conv_U1));
                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError($"PlanetAlgorithmX_GenerateVeins_Transpiler：无法在{type.Name}的GenerateVeins函数中找到指令conv.u1");
                    return instructions;
                }

                // 找到枚举变量的索引
                matcher.Advance(1); // stloc.s 36
                int stage2_eVeinType_index = ((LocalBuilder)matcher.Operand).LocalIndex;

                // 寻找形如 if (A > 1) 的代码，记录其中本地变量 A （ num11 ，当前种类的矿物有多少个矿脉）的索引，记录 Ble 指令的位置
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Ble)
                );
                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError($"PlanetAlgorithmX_GenerateVeins_Transpiler：无法在{type.Name}的GenerateVeins函数中找到形如 if (A > 1) 的代码");
                    return instructions;
                }

                int stage2_num11_index = ((LocalBuilder)matcher.Operand).LocalIndex;

                matcher.Advance(2);
                int stage2_ble_position = matcher.Pos;

                // 寻找 for 循环的开头，形如 for (int A = 0; 的代码
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Br)
                );
                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError($"PlanetAlgorithmX_GenerateVeins_Transpiler：无法在{type.Name}的GenerateVeins函数中找到正确的for循环的头部代码");
                    return instructions;
                }

                // 调用 GetVeinGroupCount ，然后将返回值重新赋予 num11
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetAlgorithm), "planet")),
                    new CodeInstruction(OpCodes.Ldloc_S, stage2_eVeinType_index),
                    new CodeInstruction(OpCodes.Ldloc_S, stage2_num11_index),
                    new CodeInstruction(OpCodes.Call, typeof(Patch_PlanetAlgorithms).GetMethod("GetVeinGroupCount", BindingFlags.Static | BindingFlags.NonPublic)),
                    new CodeInstruction(OpCodes.Stloc_S, stage2_num11_index)
                );

                // 上述 if (A > 1) 的代码，其跳转语句应该跳转到刚刚插入的代码的开头，而不是 for 循环的开头
                Label stage2_newBranchTarget = generator.DefineLabel();
                matcher.Instruction.labels.Add(stage2_newBranchTarget);
                matcher.Start();
                matcher.Advance(stage2_ble_position);
                matcher.SetOperandAndAdvance(stage2_newBranchTarget);

                // 三、修改每个矿脉中矿点的数量
                // 寻找形如 if (A == EVeinType.Oil) { B = 1; } 的代码，
                // 记录其中本地变量 A （ eVeinType2 ，当前矿脉的矿物类型）和 B （ num19 ，矿脉中矿点数量）的索引，并为其中的本地变量 B 重新赋值
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S), // A
                    new CodeMatch(OpCodes.Ldc_I4_7), // EVeinType.Oil
                    new CodeMatch(OpCodes.Bne_Un), // 相等
                    new CodeMatch(OpCodes.Ldc_I4_1), // 1
                    new CodeMatch(OpCodes.Stloc_S) // B 赋值
                );
                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError($"PlanetAlgorithmX_GenerateVeins_Transpiler：无法在{type.Name}的GenerateVeins函数中找到正确的给矿脉中矿点数量赋值的代码");
                    return instructions;
                }

                int stage3_eVeinType2_index = ((LocalBuilder)matcher.Operand).LocalIndex;

                matcher.Advance(4);
                int stage3_num19_index = ((LocalBuilder)matcher.Operand).LocalIndex;

                // 重新回到刚刚插入的代码的开头，函数调用需要插入在 if 语句之前
                matcher.Advance(-4);

                // 调用 GetVeinCount ，然后将返回值重新赋予 num11
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetAlgorithm), "planet")),
                    new CodeInstruction(OpCodes.Ldloc_S, stage3_eVeinType2_index),
                    new CodeInstruction(OpCodes.Ldloc_S, stage3_num19_index),
                    new CodeInstruction(OpCodes.Call, typeof(Patch_PlanetAlgorithms).GetMethod("GetVeinCount", BindingFlags.Static | BindingFlags.NonPublic)),
                    new CodeInstruction(OpCodes.Stloc_S, stage3_num19_index)
                );

                // 四、修改每个矿点中矿物的数量
                // 寻找形如 if (A && B.type != EVeinType.Oil) { B.amount = 1000000000; } 的代码，
                // 记录其中 brfalse 和 beq 指令的索引。
                // 这段代码应该是最后一处对 vein.amount 进行赋值的代码
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldfld && instruction.operand.ToString() == "EVeinType type"),
                    new CodeMatch(OpCodes.Ldc_I4_7),
                    new CodeMatch(OpCodes.Beq),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldc_I4, 1000000000),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Stfld && instruction.operand.ToString() == "System.Int32 amount")
                );
                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError($"PlanetAlgorithmX_GenerateVeins_Transpiler：无法在{type.Name}的GenerateVeins函数中找到最后一处给矿脉中矿点矿物数量赋值的代码");
                    return instructions;
                }

                matcher.Advance(1);
                int stage4_brfalse_position = matcher.Pos;

                matcher.Advance(4);
                int stage4_beq_position = matcher.Pos;

                // 找到本地变量 vein 的索引
                matcher.Advance(1);
                int stage4_vein_index = ((LocalBuilder)matcher.Operand).LocalIndex;

                // 移动到 if 语句的末尾
                matcher.Advance(3);

                // 调用 GetVeinAmount ，然后将返回值重新赋给 vein.amount
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloca_S, stage4_vein_index),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetAlgorithm), "planet")),
                    new CodeInstruction(OpCodes.Ldloc_S, stage3_eVeinType2_index),
                    new CodeInstruction(OpCodes.Ldloc_S, stage4_vein_index),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), "amount")),
                    new CodeInstruction(OpCodes.Call, typeof(Patch_PlanetAlgorithms).GetMethod("GetVeinAmount", BindingFlags.Static | BindingFlags.NonPublic)),
                    new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(VeinData), "amount"))
                );

                // 将该段代码移出 if 语句块
                Label stage4_newBranchTarget = generator.DefineLabel();
                matcher.Instruction.labels.Add(stage4_newBranchTarget);
                matcher.Start();
                matcher.Advance(stage4_brfalse_position);
                matcher.SetOperandAndAdvance(stage4_newBranchTarget);
                matcher.Start();
                matcher.Advance(stage4_beq_position);
                matcher.SetOperandAndAdvance(stage4_newBranchTarget);

                return matcher.InstructionEnumeration();
            }

            static void InitializeRandomGenerator (PlanetData planet)
            {
                random = new DotNet35Random(planet.seed);
            }

            /// <summary>
            /// 获取矿脉数量
            /// </summary>
            /// <param name="_"></param>
            /// <param name="planet"></param>
            /// <param name="veinType"></param>
            /// <param name="originalValue"></param>
            /// <returns></returns>
            static int GetVeinGroupCount (PlanetAlgorithm _, PlanetData planet, EVeinType veinType, int originalValue)
            {
                List<AdditionalPlanetConfig> configList = Utility.GetPlanetConfigList(
                    GameMain.gameName,
                    GameMain.data.gameDesc.clusterString,
                    planet.star.name,
                    Instance.globalPlanetConfig,
                    Instance.gameNameSpecificConfig,
                    out string _
                );
                if (configList == null)
                {
                    // 没有针对该恒星的配置，直接返回原始值
                    return originalValue;
                }

                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index || !config._HasVeinCustom || !config.VeinCustom.ContainsKey(veinType))
                    {
                        continue;
                    }

                    AdditionalPlanetConfig.VeinConfig.CustomValue customValue = config.VeinCustom[veinType].VeinGroupCount;

                    if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Default)
                    {
                        //Instance.Logger.LogInfo($"Modify vein group count at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Default");
                        return originalValue;
                    }
                    else if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Accurate)
                    {
                        //Instance.Logger.LogInfo($"Modify vein group count at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Accurate, value: {customValue.AccurateValue}");
                        return customValue.AccurateValue;
                    }
                    else if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Random)
                    {
                        int value = customValue.GetRandomResult(random);
                        //Instance.Logger.LogInfo($"Modify vein group count at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Random, value: {value}");
                        return value;
                    }
                }
                return originalValue;
            }

            /// <summary>
            /// 获取每个矿脉中的矿点数量
            /// </summary>
            /// <param name="_"></param>
            /// <param name="planet"></param>
            /// <param name="veinType"></param>
            /// <param name="originalValue"></param>
            /// <returns></returns>
            static int GetVeinCount (PlanetAlgorithm _, PlanetData planet, EVeinType veinType, int originalValue)
            {
                List<AdditionalPlanetConfig> configList = Utility.GetPlanetConfigList(
                    GameMain.gameName,
                    GameMain.data.gameDesc.clusterString,
                    planet.star.name,
                    Instance.globalPlanetConfig,
                    Instance.gameNameSpecificConfig,
                    out string _
                );
                if (configList == null)
                {
                    // 没有针对该恒星的配置，直接返回原始值
                    return originalValue;
                }

                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index || !config._HasVeinCustom || !config.VeinCustom.ContainsKey(veinType))
                    {
                        continue;
                    }

                    AdditionalPlanetConfig.VeinConfig.CustomValue customValue = config.VeinCustom[veinType].VeinSpotCount;

                    if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Default)
                    {
                        //Instance.Logger.LogInfo($"Modify vein count at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Default");
                        return originalValue;
                    }
                    else if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Accurate)
                    {
                        //Instance.Logger.LogInfo($"Modify vein count at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Accurate, value: {customValue.AccurateValue}");
                        return customValue.AccurateValue;
                    }
                    else if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Random)
                    {
                        int value = customValue.GetRandomResult(random);
                        //Instance.Logger.LogInfo($"Modify vein count at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Random, value: {value}");
                        return value;
                    }
                }
                return originalValue;
            }

            /// <summary>
            /// 获取每个矿点中的矿物数量
            /// </summary>
            /// <param name="_"></param>
            /// <param name="planet"></param>
            /// <param name="veinType"></param>
            /// <param name="originalValue"></param>
            /// <returns></returns>
            static int GetVeinAmount (PlanetAlgorithm _, PlanetData planet, EVeinType veinType, int originalValue)
            {
                List<AdditionalPlanetConfig> configList = Utility.GetPlanetConfigList(
                    GameMain.gameName,
                    GameMain.data.gameDesc.clusterString,
                    planet.star.name,
                    Instance.globalPlanetConfig,
                    Instance.gameNameSpecificConfig,
                    out string _
                );
                if (configList == null)
                {
                    // 没有针对该恒星的配置，直接返回原始值
                    return originalValue;
                }

                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index || !config._HasVeinCustom || !config.VeinCustom.ContainsKey(veinType))
                    {
                        continue;
                    }

                    AdditionalPlanetConfig.VeinConfig.CustomValue customValue = config.VeinCustom[veinType].VeinAmount;

                    if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Default)
                    {
                        //Instance.Logger.LogInfo($"Modify vein amount at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Default");
                        return originalValue;
                    }
                    else if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Accurate)
                    {
                        //Instance.Logger.LogInfo($"Modify vein amount at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Accurate, value: {customValue.AccurateValue}");
                        return customValue.AccurateValue;
                    }
                    else if (customValue.Type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Random)
                    {
                        int value = customValue.GetRandomResult(random);
                        //Instance.Logger.LogInfo($"Modify vein amount at {uniqueStarId}, planet index: {planet.index}, vein type: {veinType}, custom type: Random, value: {value}");
                        return value;
                    }
                }
                return originalValue;
            }
            // GenerateVeins Transpiler 结束
        }

        /// <summary>
        /// 行星建模
        /// </summary>
        class Patch_PlanetModeling
        {
            /// <summary>
            /// 对行星建模时，建立行星半径与行星 PlanetRawData 之间的关系
            /// </summary>
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetModelingManager), "ModelingPlanetMain")]
            static bool ModelingPlanetMain (PlanetData planet)
            {
                planet.data.AddRadius(planet);
                return true;
            }

            /// <summary>
            /// 将对GetModPlane的调用修改为对GetModPlaneInt的调用，新的方法的返回值与行星的实际半径相关
            /// </summary>
            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetModelingManager), "ModelingPlanetMain")]
            static IEnumerable<CodeInstruction> ModelingPlanetMainTranspiler (IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                {
                    if (codes[i].Calls(typeof(PlanetRawData).GetMethod("GetModPlane")))
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, typeof(PlanetRawDataExtension).GetMethod("GetModPlaneInt"));
                    }
                }
                return codes.AsEnumerable();
            }

            /// <summary>
            /// 将对GetModPlane的调用修改为对GetModPlaneInt的调用，新的方法的返回值与行星的实际半径相关
            /// </summary>
            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetData), "UpdateDirtyMesh")]
            static IEnumerable<CodeInstruction> UpdateDirtyMeshTranspiler (IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                {
                    if (codes[i].Calls(typeof(PlanetRawData).GetMethod("GetModPlane")))
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, typeof(PlanetRawDataExtension).GetMethod("GetModPlaneInt"));
                    }
                }
                return codes.AsEnumerable();
            }

            /// <summary>
            /// 将对GetModPlane的调用修改为对GetModPlaneInt的调用，新的方法的返回值与行星的实际半径相关
            /// </summary>
            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetRawData), "QueryModifiedHeight")]
            static IEnumerable<CodeInstruction> QueryModifiedHeightTranspiler (IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                {
                    if (codes[i].Calls(typeof(PlanetRawData).GetMethod("GetModPlane")))
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, typeof(PlanetRawDataExtension).GetMethod("GetModPlaneInt"));
                    }
                }
                return codes.AsEnumerable();
            }
        }

        /// <summary>
        /// 垃圾系统
        /// </summary>
        class Patch_TrashSystem
        {
            /// <summary>
            /// 修正垃圾受重力影响的逻辑
            /// </summary>
            /// <param name="instructions"></param>
            /// <returns></returns>
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(TrashSystem), "Gravity")]
            static IEnumerable<CodeInstruction> TrashSystem_Gravity_Transpiler (IEnumerable<CodeInstruction> instructions)
            {
                CodeMatcher matcher = new CodeMatcher(instructions);

                // 某种情况下高度大于600的垃圾会直接消失，这里把这个限制改成800
                matcher.MatchForward(
                    true,
                    new CodeMatch(OpCodes.Ldc_R8, 600.0)
                );

                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError("无法找到数字 600.0");
                    return instructions;
                }

                matcher.Set(OpCodes.Ldc_R8, 800.0);

                // 寻找一段形如 A <= B + 8 的代码，然后将 +8 改为 +99
                matcher.MatchForward(false,
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && ((LocalBuilder)instruction.operand).LocalIndex == 6),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldc_I4_8),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Ble)
                );

                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError("无法找到第形如 A <= B + 8 的代码");
                    return instructions;
                }

                matcher.Advance(2);
                matcher.Set(OpCodes.Ldc_I4_S, 99);

                return matcher.InstructionEnumeration();
            }
        }

        /// <summary>
        /// 玩家行为
        /// </summary>
        class Patch_PlayerController
        {
            /// <summary>
            /// 进入蓝图模式时将蓝图相机的行星半径修改为当前的行星半径
            /// </summary>
            [HarmonyPostfix, HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OpenBlueprintCopyMode))]
            static void PlayerController_OpenBlueprintCopyMode_Postfix ()
            {
                if (GameMain.localPlanet != null)
                {
                    GameCamera.instance.blueprintPoser.planetRadius = GameMain.localPlanet.realRadius;
                }
            }

            /// <summary>
            /// 进入蓝图模式时将蓝图相机的行星半径修改为当前的行星半径
            /// </summary>
            [HarmonyPostfix, HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OpenBlueprintPasteMode))]
            static void PlayerController_OpenBlueprintPasteMode_Postfix ()
            {
                if (GameMain.localPlanet != null)
                {
                    GameCamera.instance.blueprintPoser.planetRadius = GameMain.localPlanet.realRadius;
                }
            }
        }

        /// <summary>
        /// 物流站点和物流飞船的行为
        /// </summary>
        class Patch_StationComponent
        {
            /// <summary>
            /// 1. 根据游戏的机制，一个恒星周围最多允许存在99个行星（编号为恒星ID+1到恒星ID+99），但是游戏原本只考虑了恒星自身以及恒星周围的9个行星，导致存在更多行星时运输船无法正常停靠，
            ///     这段代码参考了 GalacticScale 的代码，并在其基础上加以完善，允许运输船在更多行星上停靠
            /// 2. 参考 GalacticScale 的代码，游戏中运输船在寻路时会绕开距离恒星 2.5 倍半径的范围，导致靠近大恒星的行星无法正常停靠
            /// </summary>
            /// <param name="instructions"></param>
            /// <returns></returns>
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(StationComponent), "InternalTickRemote")]
            public static IEnumerable<CodeInstruction> InternalTickRemoteTranspiler (IEnumerable<CodeInstruction> instructions)
            {
                // 首先修正搜索行星的范围
                // 确定开始搜索的位置，应该在 if (shipData.stage == 0) 的位置
                CodeMatcher matcher = new CodeMatcher(instructions);
                matcher.MatchForward(false,
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Br),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand.ToString().StartsWith("ShipData")),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldfld && instruction.operand.ToString().Equals("System.Int32 stage")),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Brtrue)
                );

                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError("无法找到代码 if (shipData.stage == 0) 的位置");
                    return instructions;
                }

                // 找到两处形如 A < B + 10 的代码，然后将 +10 修改为 +100
                matcher.MatchForward(false,
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand.ToString().StartsWith("System.Int32")),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand.ToString().StartsWith("System.Int32")),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == 10),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Add),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Blt)
                );

                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError("无法找到第一处形如 A < B + 10 的代码");
                    return instructions;
                }

                matcher.Advance(2);
                matcher.SetOperandAndAdvance(100);

                matcher.MatchForward(false,
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand.ToString().StartsWith("System.Int32")),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand.ToString().StartsWith("System.Int32")),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == 10),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Add),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Blt)
                );

                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError("无法找到第二处形如 A < B + 10 的代码");
                    return instructions;
                }

                matcher.Advance(2);
                matcher.SetOperandAndAdvance(100);

                // 然后开始修正规避恒星半径的范围
                // 这段代码应该在一段形如 if (A % 100 == 0) B *= 2.5f 的代码块中
                matcher.MatchForward(true,
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand.ToString().StartsWith("System.Int32")),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == 100),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Rem),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Brtrue),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand.ToString().StartsWith("System.Single")),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 2.5f),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Mul),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Stloc_S && instruction.operand.ToString().StartsWith("System.Single"))
                );

                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError("无法找到形如 if (A % 100 == 0) B *= 2.5f 的代码");
                    return instructions;
                }

                matcher.Advance(-2);
                matcher.SetOperandAndAdvance(1f);

                return matcher.InstructionEnumeration();
            }
        }

        /// <summary>
        /// 行星大气、行星位置的渲染
        /// </summary>
        class Patch_PlanetSimulator
        {
            /// <summary>
            /// 修正行星大气的渲染半径，使其与行星半径有关
            /// TODO: 改成使用 Transpiler
            /// </summary>
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetSimulator), nameof(PlanetSimulator.SetPlanetData))]
            static bool PlanetSimulator_SetPlanetData_Prefix (
                PlanetData planet,
                PlanetSimulator __instance,
                ref PlanetData ___planetData,
                ref Transform ___atmoTrans0,
                ref Transform ___atmoTrans1,
                ref Material ___atmoMat,
                ref Material ___atmoMatLate,
                ref Vector4 ___atmoMatRadiusParam,
                ref Transform ___lookCamera,
                ref UniverseSimulator ___universe,
                ref StarSimulator ___star
            )
            {
                ___planetData = planet;
                if (___planetData.atmosMaterial != null) {
                    GameObject gameObject = new GameObject("Atmosphere");
                    gameObject.layer = 31;
                    ___atmoTrans0 = gameObject.transform;
                    ___atmoTrans0.parent = __instance.transform;
                    ___atmoTrans0.localPosition = Vector3.zero;
                    GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    gameObject2.layer = 31;
                    ___atmoTrans1 = gameObject2.transform;
                    ___atmoTrans1.parent = ___atmoTrans0;
                    ___atmoTrans1.localPosition = Vector3.zero;
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<Collider>());
                    Renderer component = gameObject2.GetComponent<Renderer>();
                    ___atmoMat = ___planetData.atmosMaterial;
                    ___atmoMatLate = ___planetData.atmosMaterialLate;
                    component.sharedMaterials = new Material[2] { ___atmoMat, ___atmoMatLate };
                    component.shadowCastingMode = ShadowCastingMode.Off;
                    component.receiveShadows = false;
                    component.lightProbeUsage = LightProbeUsage.Off;
                    component.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    ___atmoTrans1.localScale = Vector3.one * (planet.realRadius * 5f);
                    ___atmoMatRadiusParam = ___atmoMat.GetVector("_PlanetRadius");
                }
                ___lookCamera = Camera.main.transform;
                ___universe = GameMain.universeSimulator;
                ___star = ___universe.FindStarSimulator(planet.star);

                return false;
            }
        }

        /// <summary>
        /// 行星网格，似乎偏重于数学计算
        /// </summary>
        class Patch_PlanetGrid
        {
            /// <summary>
            /// 修正行星网格中两点间距离的计算（大概是这么一个意思吧）
            /// TODO：改成使用 Transpiler
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="posR"></param>
            /// <param name="posA"></param>
            /// <param name="posB"></param>
            /// <param name="__result"></param>
            /// <returns></returns>
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetGrid), "CalcSegmentsAcross")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "<挂起>")]
            static bool PlanetGrid_CalcSegmentsAcross_Prefix (PlanetGrid __instance, Vector3 posR, Vector3 posA, Vector3 posB, ref float __result)
            {
                posR.Normalize();
                posA.Normalize();
                posB.Normalize();
                var num = Mathf.Asin(posR.y);
                var f = num / ((float)Math.PI * 2f) * __instance.segment;
                var latitudeIndex = Mathf.FloorToInt(Mathf.Max(0f, Mathf.Abs(f) - 0.1f));
                float num2 = PlanetGrid.DetermineLongitudeSegmentCount(latitudeIndex, __instance.segment);
                //Replaced the fixed value 0.0048 with 1/segments * 0.96 [based on planet size 200: 1/200 = 0.005; 0.005 * 0.96 = 0.0048
                //since the value has to become smaller the larger the planet is, the inverse value (1/x) is used in the calculation
                var num3 = Mathf.Max(1.0f / __instance.segment * 0.96f, Mathf.Cos(num) * (float)Math.PI * 2f / (num2 * 5f));
                var num4 = (float)Math.PI * 2f / (__instance.segment * 5f);
                var num5 = Mathf.Asin(posA.y);
                var num6 = Mathf.Atan2(posA.x, 0f - posA.z);
                var num7 = Mathf.Asin(posB.y);
                var num8 = Mathf.Atan2(posB.x, 0f - posB.z);
                var num9 = Mathf.Abs(Mathf.DeltaAngle(num6 * 57.29578f, num8 * 57.29578f) * ((float)Math.PI / 180f));
                var num10 = Mathf.Abs(num5 - num7);
                var num11 = num10 + num9;
                var num12 = 0f;
                var num13 = 1f;
                if (num11 > 0f)
                {
                    num12 = num9 / num11;
                    num13 = num10 / num11;
                }

                var num14 = num3 * num12 + num4 * num13;
                __result = (posA - posB).magnitude / num14;
                return false;
            }
        }

        /// <summary>
        /// UI创建与销毁
        /// </summary>
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

                ResourceCache.InitializeResourceCache();

                Instance.CreateUI();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnFree")]
            static void UIGame__OnFree_Postfix ()
            {
                Instance.uiAddPlanet._Free();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnUpdate")]
            static void UIGame__OnUpdate_Postfix ()
            {
                if (GameMain.isPaused || !GameMain.isRunning)
                {
                    return;
                }
                Instance.uiAddPlanet._Update();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnDestroy")]
            static void UIGame__OnDestroy_Postfix ()
            {
                Instance.uiAddPlanet._Destroy();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnInit")]
            static void UIGame__OnInit_Postfix ()
            {
                if (!Instance.uiAddPlanet.inited)
                {
                    Instance.uiAddPlanet._Init(Instance.uiAddPlanet.data);
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "ShutAllFunctionWindow")]
            static void UIGame_ShutAllFunctionWindow_Postfix ()
            {
                Instance.uiAddPlanet._Close();
            }
        }

        /// <summary>
        /// 确保读档时 PlatformSystem 的 maxReformCount 和 reformData.Length 相等
        /// </summary>
        class Patch_PlatformSystem
        {
            [HarmonyPostfix, HarmonyPatch(typeof(PlatformSystem), nameof(PlatformSystem.Import))]
            static void PlatformSystem_Import_Postfix (PlatformSystem __instance)
            {
                if (__instance.reformData != null && __instance.maxReformCount > __instance.reformData.Length)
                {
                    Array.Resize(ref __instance.reformData, __instance.maxReformCount);
                }
            }
        }

        /// <summary>
        /// 修正蓝图在不同于200半径的行星上的行为
        /// </summary>
        class Patch_BuildTool_BlueprintPaste
        {
            [HarmonyTranspiler, HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CheckBuildConditions))]
            static IEnumerable<CodeInstruction> BuildTool_BlueprintPaste_CheckBuildConditions_Transpiler (IEnumerable<CodeInstruction> instructions)
            {
                CodeMatcher matcher = new CodeMatcher(instructions);

                // 找到形如 A = ? - 200.2f 的代码
                matcher.MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldc_R4, 200.2f),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(instruction => instruction.opcode == OpCodes.Stloc_S)
                );

                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError("BuildTool_BlueprintPaste_CheckBuildConditions_Transpiler ：无法找到形如 A = ? - 200.2f 的代码");
                    return instructions;
                }

                matcher.Set(OpCodes.Call, typeof(Patch_BuildTool_BlueprintPaste).GetMethod("GetLocalPlanetRadius", BindingFlags.NonPublic | BindingFlags.Static));

                return matcher.InstructionEnumeration();
            }

            static float GetLocalPlanetRadius ()
            {
                return GameMain.localPlanet.realRadius;
            }
        }

        /// <summary>
        /// 修复由于行星半径过大导致的若干问题
        /// </summary>
        class Patch_PlanetFactory
        {
            [HarmonyTranspiler, HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.FlattenTerrain))]
            static IEnumerable<CodeInstruction> PlanetFactory_FlattenTerrain_Transpiler (IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                LocalBuilder intNum5 = generator.DeclareLocal(typeof(int));

                CodeMatcher matcher = new CodeMatcher(instructions);

                // 找到形如 (short)(A * 100f + 20f) 的代码
                matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_R4, 100f),
                    new CodeMatch(OpCodes.Mul),
                    new CodeMatch(OpCodes.Ldc_R4, 20f),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Conv_I2),
                    new CodeMatch(OpCodes.Stloc_S)
                );

                if (matcher.IsInvalid)
                {
                    Instance.Logger.LogError("PlanetFactory_FlattenTerrain_Transpiler: matcher.IsInvalid");
                    return instructions;
                }

                // 记录原来被赋值的变量的索引
                int oldNum5Index = ((LocalBuilder)matcher.Operand).LocalIndex;

                // 将值赋给新的本地变量
                matcher.Advance(-1);
                matcher.SetOpcodeAndAdvance(OpCodes.Conv_I4); // conv.i2 -> conv.i4
                matcher.SetAndAdvance(OpCodes.Stloc_S, intNum5.LocalIndex); // stloc.s 13 -> stloc.s intNum5

                // 寻找读取旧变量的地方，并将其替换成新的本地变量
                while (matcher.IsValid)
                {
                    matcher.MatchForward(true, new CodeMatch(
                        instruction => instruction.opcode == OpCodes.Ldloc_S && ((LocalBuilder)instruction.operand).LocalIndex == oldNum5Index
                    ));
                    if (matcher.IsValid)
                    {
                        matcher.SetAndAdvance(OpCodes.Ldloc_S, intNum5.LocalIndex);
                    }
                }

                return matcher.InstructionEnumeration();
            }
        }

        class Patch_Debug
        {
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetGen), nameof(PlanetGen.SetPlanetTheme))]
            static bool PlanetGen_SetPlanetTheme_Prefix (PlanetData planet, int[] themeIds) {
                Instance.Logger.LogInfo($"planet.index: {planet.index}, planet.star.planets.Length: {planet.star.planets.Length}, themeIds.Length: {themeIds.Length}");
                return true;
            }
        }
    }
}
