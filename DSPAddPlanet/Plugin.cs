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

namespace DSPAddPlanet
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "IndexOutOfRange.DSPAddPlanet";
        public const string PLUGIN_NAME = "DSPAddPlanet";
        public const string PLUGIN_VERSION = "0.0.1";

        public const float MAX_PLANET_RADIUS = 600;

        public const int DEFAULT_INFO_SEED = 0;
        public const int DEFAULT_GEN_SEED = 0;
        public const bool DEFAULT_FORCE_PLANET_RADIUS = false;
        public const float DEFAULT_PLANET_RADIUS = 200;
        public const float DEFAULT_ORBITAL_PERIOD = 3600;
        public const float DEFAULT_ROTATION_PERIOD = 3600;
        public const bool DEFAULT_IS_TIDAL_LOCKED = true;
        public const float DEFAULT_ORBIT_INCLINATION = 0;
        public const float DEFAULT_OBLIQUITY = 0;
        public const bool DEFAULT_DONT_GENERATE_VEIN = true;

        static public Plugin Instance { get => instance; }
        static private Plugin instance = null;
        new public ManualLogSource Logger { get => base.Logger; }

        /// <summary>
        /// 是否有配置文件
        /// </summary>
        private bool hasConfig = false;

        /// <summary>
        /// 所有需要新增的星球，List中的内容是按照配置文件中约定的顺序存储的
        /// </summary>
        private Dictionary<string, List<AdditionalPlanetConfig>> additionalPlanets = new Dictionary<string, List<AdditionalPlanetConfig>>();

        /// <summary>
        /// UIAddPlanet UI组件
        /// </summary>
        private UIAddPlanet uiAddPlanet = null;

        private void Awake ()
        {
            instance = this;

            TryReadConfig();

            Harmony harmony = new Harmony(PLUGIN_GUID);

            // _OnCreate 的 Postfix 补丁会调用 Init 函数，负责初始化 UI 以及读取配置文件
            harmony.PatchAll(typeof(Patch_UIGame__OnCreate));

            // _OnFree 、 _OnUpdate 、 _OnDestroy 、 _OnInit 和 ShutAllFunctionWindow 都是为那几个 UI 组件服务的
            harmony.PatchAll(typeof(Patch_UIGame__OnFree));
            harmony.PatchAll(typeof(Patch_UIGame__OnUpdate));
            harmony.PatchAll(typeof(Patch_UIGame__OnDestroy));
            harmony.PatchAll(typeof(Patch_UIGame__OnInit));
            harmony.PatchAll(typeof(Patch_UIGame_ShutAllFunctionWindow));

            // _CreateStarPlanets 的 Postfix 补丁是为新增行星这一核心业务服务的
            harmony.PatchAll(typeof(Patch_StarGen_CreateStarPlanets));

            // 针对生成矿脉和 vege 的补丁
            harmony.PatchAll(typeof(Patch_PlanetAlgorithms));

            // 针对行星模型的补丁
            harmony.PatchAll(typeof(Patch_PlanetModeling));
        }

        /// <summary>
        /// 尝试读取配置
        /// </summary>
        private void TryReadConfig ()
        {
            // 尝试读取配置文件
            string modDataDir = GameConfig.gameSaveFolder + "modData/IndexOutOfRange.DSPAddPlanet/";
            if (!Directory.Exists(modDataDir))
            {
                Directory.CreateDirectory(modDataDir);
            }
            string configFilePath = modDataDir + "config.txt";
            if (!File.Exists(configFilePath))
            {
                // 如果没有找到配置文件，则创建配置文件
                StreamWriter writer = File.CreateText(configFilePath);

                // 写入样例配置
                // uniqueStarId：唯一恒星ID
                // index：星球的索引，从0开始
                // orbitAround：星球是否是其他星球的卫星，如果是卫星的话，是哪个星球的卫星（被环绕卫星的number）
                // orbitIndex：星球公转轨道的半径
                // number：星球的编号，（似乎）从1开始
                // gasGiant：星球是否是气态巨星
                // info_seed：种子
                // gen_seed：种子
                // planetRadius：行星半径
                // forcePlanetRadius：是否忽略行星半径的限制（最大600）
                // orbitalPeriod：公转周期（秒）
                // rotationPeriod：自转周期（秒）
                // isTidalLocked：是否潮汐锁定
                // orbitInclination：轨道倾角（度）
                // obliquity：地轴倾角（度）
                // dontGenerateVein：是否不生成矿脉
                writer.WriteLine("# Add additional planets to your game.");
                writer.WriteLine("# New planets will be added in the same order as they are written in this file.");
                writer.WriteLine("# The format of the config value is similar to URL query string (but not the same, the parser used here is extremely simple)");
                writer.WriteLine("# For detailed description, please refer to https://dsp.thunderstore.io/package/IndexOutOfRange/DSPAddPlanet/");
                writer.WriteLine(
                    new StringBuilder()
                        .Append("(EXAMPLE)")
                        .Append("uniqueStarId=UNIQUE_STAR_ID")
                        .Append("&index=INDEX")
                        .Append("&orbitAround=ORBIT_AROUND")
                        .Append("&orbitIndex=ORBIT_INDEX")
                        .Append("&number=NUMBER")
                        .Append("&gasGiant=GAS_GIANT")
                        .Append("&info_seed=INFO_SEED")
                        .Append("&gen_seed=GEN_SEED")
                        .Append("&planetRadius=PLANET_RADIUS")
                        .Append("&forcePlanetRadius=FORCE_PLANET_RADIUS")
                        .Append("&orbitalPeriod=ORBITAL_PERIOD")
                        .Append("&rotationPeriod=ROTATION_PERIOD")
                        .Append("&isTidalLocked=IS_TIDAL_LOCKED")
                        .Append("&orbitInclination=ORBIT_INCLINATION")
                        .Append("&obliquity=OBLIQUITY")
                        .Append("&dontGenerateVein=DONT_GENERATE_VEIN")
                        .ToString()
                );

                writer.Flush();
                writer.Dispose();

                // 在没有配置文件的情况下，不对游戏进行修改
                hasConfig = false;
                return;
            }

            ReadConfig(configFilePath);
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="configFilePath"></param>
        private void ReadConfig (string configFilePath)
        {
            // 获取配置
            string[] rawConfigArray = File.ReadAllLines(configFilePath);

            for (int i = 0; i < rawConfigArray.Length; ++i)
            {
                string row = rawConfigArray[i].Trim();

                if (row.IsNullOrWhiteSpace() || row.StartsWith("#") || row.StartsWith("(EXAMPLE)"))
                {
                    continue;
                }

                Dictionary<string, string> configMap = Utility.ParseQueryString(row);

                string uniqueStarId = configMap.GetValueSafe("uniqueStarId");
                if (string.IsNullOrWhiteSpace(uniqueStarId))
                {
                    Instance.Logger.LogError($"Missing 'uniqueStarId', Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    return;
                }
                if (!int.TryParse(configMap.GetValueSafe("index"), out int index))
                {
                    Instance.Logger.LogError($"Missing 'index', Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    return;
                }
                if (!int.TryParse(configMap.GetValueSafe("orbitAround"), out int orbitAround))
                {
                    Instance.Logger.LogError($"Missing 'orbitAround', Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    return;
                }
                if (!int.TryParse(configMap.GetValueSafe("orbitIndex"), out int orbitIndex))
                {
                    Instance.Logger.LogError($"Missing 'orbitIndex', Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    return;
                }
                if (!int.TryParse(configMap.GetValueSafe("number"), out int number))
                {
                    Instance.Logger.LogError($"Missing 'number', Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    return;
                }
                if (!bool.TryParse(configMap.GetValueSafe("gasGiant"), out bool gasGiant))
                {
                    Instance.Logger.LogError($"Missing 'gasGiant', Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    return;
                }
                if (!int.TryParse(configMap.GetValueSafe("info_seed"), out int infoSeed))
                {
                    Instance.Logger.LogInfo($"Missing 'info_seed', pick default value: {DEFAULT_INFO_SEED}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    infoSeed = DEFAULT_INFO_SEED;
                }
                if (!int.TryParse(configMap.GetValueSafe("gen_seed"), out int genSeed))
                {
                    Instance.Logger.LogInfo($"Missing 'gen_seed', pick default value: {DEFAULT_GEN_SEED}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    genSeed = DEFAULT_GEN_SEED;
                }
                if (!bool.TryParse(configMap.GetValueSafe("forcePlanetRadius"), out bool forcePlanetRadius))
                {
                    Instance.Logger.LogInfo($"Missing 'forcePlanetRadius', pick default value: {DEFAULT_FORCE_PLANET_RADIUS}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    forcePlanetRadius = DEFAULT_FORCE_PLANET_RADIUS;
                }
                if (!float.TryParse(configMap.GetValueSafe("planetRadius"), out float planetRadius))
                {
                    Instance.Logger.LogInfo($"Missing 'planetRadius', pick default value: {DEFAULT_PLANET_RADIUS}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    planetRadius = DEFAULT_PLANET_RADIUS;
                }
                if (planetRadius > MAX_PLANET_RADIUS)
                {
                    if (!forcePlanetRadius)
                    {
                        Instance.Logger.LogError($"Current max planet radius is {MAX_PLANET_RADIUS}, use 'forcePlanetRadius=true' to override this, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                        return;
                    }
                    else
                    {
                        Instance.Logger.LogWarning($"Force planet radius: {planetRadius}");
                    }
                }
                if (!float.TryParse(configMap.GetValueSafe("orbitalPeriod"), out float orbitalPeriod))
                {
                    Instance.Logger.LogInfo($"Missing 'orbitalPeriod', pick default value: {DEFAULT_ORBITAL_PERIOD}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    orbitalPeriod = DEFAULT_ORBITAL_PERIOD;
                }
                if (!float.TryParse(configMap.GetValueSafe("rotationPeriod"), out float rotationPeriod))
                {
                    Instance.Logger.LogInfo($"Missing 'rotationPeriod', pick default value: {DEFAULT_ROTATION_PERIOD}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    rotationPeriod = DEFAULT_ROTATION_PERIOD;
                }
                if (!bool.TryParse(configMap.GetValueSafe("isTidalLocked"), out bool isTidalLocked))
                {
                    Instance.Logger.LogInfo($"Missing 'isTidalLocked', pick default value: {DEFAULT_IS_TIDAL_LOCKED}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    isTidalLocked = DEFAULT_IS_TIDAL_LOCKED;
                }
                if (!float.TryParse(configMap.GetValueSafe("orbitInclination"), out float orbitInclination))
                {
                    Instance.Logger.LogInfo($"Missing 'orbitInclination', pick default value: {DEFAULT_ORBIT_INCLINATION}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    orbitInclination = DEFAULT_ORBIT_INCLINATION;
                }
                if (!float.TryParse(configMap.GetValueSafe("obliquity"), out float obliquity))
                {
                    Instance.Logger.LogInfo($"Missing 'obliquity', pick default value: {DEFAULT_OBLIQUITY}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    obliquity = DEFAULT_ORBIT_INCLINATION;
                }
                if (!bool.TryParse(configMap.GetValueSafe("dontGenerateVein"), out bool dontGenerateVein))
                {
                    Instance.Logger.LogInfo($"Missing 'dontGenerateVein', pick default value: {DEFAULT_DONT_GENERATE_VEIN}, Section: General, Key: AdditionalPlanets, Value#{i}: {row}");
                    dontGenerateVein = DEFAULT_DONT_GENERATE_VEIN;
                }

                AdditionalPlanetConfig config = new AdditionalPlanetConfig();
                config.Index = index;
                config.OrbitAround = orbitAround;
                config.OrbitIndex = orbitIndex;
                config.Number = number;
                config.GasGiant = gasGiant;
                config.InfoSeed = infoSeed;
                config.GenSeed = genSeed;
                config.Radius = planetRadius;
                config.OrbitalPeriod = orbitalPeriod;
                config.RotationPeriod = rotationPeriod;
                config.IsTidalLocked = isTidalLocked;
                config.OrbitInclination = orbitInclination;
                config.Obliquity = obliquity;
                config.DontGenerateVein = dontGenerateVein;

                if (!additionalPlanets.ContainsKey(uniqueStarId))
                {
                    additionalPlanets[uniqueStarId] = new List<AdditionalPlanetConfig>();
                }

                additionalPlanets[uniqueStarId].Add(config);

                Instance.Logger.LogInfo($"新增行星：{uniqueStarId}: {config}");
            }

            hasConfig = true;
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

        [HarmonyPatch(typeof(UIGame), "_OnCreate")]
        class Patch_UIGame__OnCreate
        {
            static private bool isCreated = false;

            static void Postfix ()
            {
                if (isCreated)
                {
                    return;
                }
                isCreated = true;

                ResourceCache.InitializeResourceCache();

                Instance.CreateUI();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnFree")]
        class Patch_UIGame__OnFree
        {
            static void Postfix ()
            {
                Instance.uiAddPlanet._Free();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnUpdate")]
        class Patch_UIGame__OnUpdate
        {
            static void Postfix ()
            {
                if (GameMain.isPaused || !GameMain.isRunning)
                {
                    return;
                }
                Instance.uiAddPlanet._Update();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnDestroy")]
        class Patch_UIGame__OnDestroy
        {
            static void Postfix ()
            {
                Instance.uiAddPlanet._Destroy();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnInit")]
        class Patch_UIGame__OnInit
        {
            static private bool isInit = false;

            static void Postfix ()
            {
                if (isInit)
                {
                    return;
                }
                isInit = true;

                Instance.uiAddPlanet._Init(Instance.uiAddPlanet.data);
            }
        }

        [HarmonyPatch(typeof(UIGame), "ShutAllFunctionWindow")]
        class Patch_UIGame_ShutAllFunctionWindow
        {
            static void Postfix ()
            {
                Instance.uiAddPlanet._Close();
            }
        }

        [HarmonyPatch(typeof(StarGen), nameof(StarGen.CreateStarPlanets))]
        class Patch_StarGen_CreateStarPlanets
        {
            static void Postfix (GalaxyData galaxy, StarData star, GameDesc gameDesc)
            {
                if (!Instance.hasConfig)
                {
                    // 如果没有配置文件的话，不对游戏进行任何修改
                    return;
                }

                string uniqueStarId = Utility.UniqueStarId(GameMain.gameName, gameDesc.clusterString, star.name);
                if (!Instance.additionalPlanets.ContainsKey(uniqueStarId))
                {
                    // 指定位置没有新的行星
                    return;
                }

                Instance.Logger.LogInfo($"位置 {uniqueStarId} 有新增行星");

                // 需要新增的行星
                List<AdditionalPlanetConfig> configList = Instance.additionalPlanets[uniqueStarId];

                // 修改行星数量
                star.planetCount += configList.Count;

                // 创建新的数组，复制原有行星的引用
                PlanetData[] planets = new PlanetData[star.planets.Length + configList.Count];
                int i = 0;
                for (; i < star.planets.Length; ++i)
                {
                    planets[i] = star.planets[i];
                }
                star.planets = planets;

                // 创建新的行星
                foreach (AdditionalPlanetConfig config in configList)
                {
                    Instance.Logger.LogInfo($"    {config}");
                    PlanetData planet = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, config.Index, config.OrbitAround, config.OrbitIndex, config.Number, config.GasGiant, config.InfoSeed, config.GenSeed);

                    Instance.Logger.LogInfo($"    planetAlgo={planet.algoId}");

                    // 增大行星体积
                    if (!config.GasGiant)
                    {
                        planet.radius = config.Radius;
                        planet.scale = 1f;
                        planet.precision = 200;
                        planet.segment = 5;
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

                    star.planets[i++] = planet;
                }
            }
        }

        class Patch_PlanetAlgorithms
        {
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
            static bool PlanetAlgorithm_GenerateVeins_Prefix (PlanetAlgorithm __instance)
            {
                PlanetData planet = (PlanetData)AccessTools.Field(typeof(PlanetAlgorithm), "planet").GetValue(__instance);
                string uniqueStarId = Utility.UniqueStarId(GameMain.gameName, GameMain.data.gameDesc.clusterString, planet.star.name);

                Instance.Logger.LogInfo($"正在生成矿脉，恒星：{uniqueStarId}，行星：{planet.name}");

                if (!Instance.additionalPlanets.ContainsKey(uniqueStarId))
                {
                    // 如果这个恒星没有新增的行星，则不考虑是否生成矿脉的配置，执行游戏自带的 GenerateVeins 函数
                    return true;
                }

                List<AdditionalPlanetConfig> configList = Instance.additionalPlanets[uniqueStarId];
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

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm0), nameof(PlanetAlgorithm0.GenerateVeins))]
            static bool PlanetAlgorithm0_GenerateVeins_Prefix (PlanetAlgorithm0 __instance)
            {
                PlanetData planet = (PlanetData)AccessTools.Field(typeof(PlanetAlgorithm0), "planet").GetValue(__instance);
                string uniqueStarId = Utility.UniqueStarId(GameMain.gameName, GameMain.data.gameDesc.clusterString, planet.star.name);

                Instance.Logger.LogInfo($"正在生成矿脉，恒星：{uniqueStarId}，行星：{planet.name}");

                if (!Instance.additionalPlanets.ContainsKey(uniqueStarId))
                {
                    return true;
                }

                List<AdditionalPlanetConfig> configList = Instance.additionalPlanets[uniqueStarId];
                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index)
                    {
                        continue;
                    }

                    if (!config.DontGenerateVein)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm7), nameof(PlanetAlgorithm7.GenerateVeins))]
            static bool PlanetAlgorithm7_GenerateVeins_Prefix (PlanetAlgorithm7 __instance)
            {
                PlanetData planet = (PlanetData)AccessTools.Field(typeof(PlanetAlgorithm7), "planet").GetValue(__instance);
                string uniqueStarId = Utility.UniqueStarId(GameMain.gameName, GameMain.data.gameDesc.clusterString, planet.star.name);

                Instance.Logger.LogInfo($"正在生成矿脉，恒星：{uniqueStarId}，行星：{planet.name}");

                if (!Instance.additionalPlanets.ContainsKey(uniqueStarId))
                {
                    return true;
                }

                List<AdditionalPlanetConfig> configList = Instance.additionalPlanets[uniqueStarId];
                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index)
                    {
                        continue;
                    }

                    if (!config.DontGenerateVein)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm11), nameof(PlanetAlgorithm11.GenerateVeins))]
            static bool PlanetAlgorithm11_GenerateVeins_Prefix (PlanetAlgorithm11 __instance)
            {
                PlanetData planet = (PlanetData)AccessTools.Field(typeof(PlanetAlgorithm11), "planet").GetValue(__instance);
                string uniqueStarId = Utility.UniqueStarId(GameMain.gameName, GameMain.data.gameDesc.clusterString, planet.star.name);

                Instance.Logger.LogInfo($"正在生成矿脉，恒星：{uniqueStarId}，行星：{planet.name}");

                if (!Instance.additionalPlanets.ContainsKey(uniqueStarId))
                {
                    return true;
                }

                List<AdditionalPlanetConfig> configList = Instance.additionalPlanets[uniqueStarId];
                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index)
                    {
                        continue;
                    }

                    if (!config.DontGenerateVein)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm12), nameof(PlanetAlgorithm12.GenerateVeins))]
            static bool PlanetAlgorithm12_GenerateVeins_Prefix (PlanetAlgorithm12 __instance)
            {
                PlanetData planet = (PlanetData)AccessTools.Field(typeof(PlanetAlgorithm12), "planet").GetValue(__instance);
                string uniqueStarId = Utility.UniqueStarId(GameMain.gameName, GameMain.data.gameDesc.clusterString, planet.star.name);

                Instance.Logger.LogInfo($"正在生成矿脉，恒星：{uniqueStarId}，行星：{planet.name}");

                if (!Instance.additionalPlanets.ContainsKey(uniqueStarId))
                {
                    return true;
                }

                List<AdditionalPlanetConfig> configList = Instance.additionalPlanets[uniqueStarId];
                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index)
                    {
                        continue;
                    }

                    if (!config.DontGenerateVein)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetAlgorithm13), nameof(PlanetAlgorithm13.GenerateVeins))]
            static bool PlanetAlgorithm13_GenerateVeins_Prefix (PlanetAlgorithm13 __instance)
            {
                PlanetData planet = (PlanetData)AccessTools.Field(typeof(PlanetAlgorithm13), "planet").GetValue(__instance);
                string uniqueStarId = Utility.UniqueStarId(GameMain.gameName, GameMain.data.gameDesc.clusterString, planet.star.name);

                Instance.Logger.LogInfo($"正在生成矿脉，恒星：{uniqueStarId}，行星：{planet.name}");

                if (!Instance.additionalPlanets.ContainsKey(uniqueStarId))
                {
                    return true;
                }

                List<AdditionalPlanetConfig> configList = Instance.additionalPlanets[uniqueStarId];
                foreach (AdditionalPlanetConfig config in configList)
                {
                    if (config.Index != planet.index)
                    {
                        continue;
                    }

                    if (!config.DontGenerateVein)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        class Patch_PlanetModeling
        {
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetModelingManager), "ModelingPlanetMain")]
            static bool ModelingPlanetMain (PlanetData planet)
            {
                planet.data.AddRadius(planet);
                return true;
            }

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(PlanetModelingManager), "ModelingPlanetMain")]
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

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(PlanetData), "UpdateDirtyMesh")]
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

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(PlanetRawData), "QueryModifiedHeight")]
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
    }
}
