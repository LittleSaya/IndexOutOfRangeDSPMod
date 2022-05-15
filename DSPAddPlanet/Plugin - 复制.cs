using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            harmony.PatchAll(typeof(Patch_Debug));
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

        class Patch_Debug
        {
            /// <summary>
            /// 行星引用列表
            /// </summary>
            static private List<PlanetData> planets = new List<PlanetData>();

            /// <summary>
            /// PlanetRawData 到其所属的 PlanetData 对象的映射
            /// </summary>
            static private Dictionary<PlanetRawData, PlanetData> rawDataToPlanet = new Dictionary<PlanetRawData, PlanetData>();

            /// <summary>
            /// 构造行星引用的列表
            /// </summary>
            /// <param name="__result"></param>
            [HarmonyPostfix, HarmonyPatch(typeof(UniverseGen), nameof(UniverseGen.CreateGalaxy))]
            static void UniverseGen_CreateGalaxy_Postfix (ref GalaxyData __result)
            {
                Instance.Logger.LogInfo("UniverseGen_CreateGalaxy_Postfix");
                planets.Clear();
                rawDataToPlanet.Clear();
                foreach (var star in __result.stars)
                {
                    foreach (var planet in star.planets)
                    {
                        planets.Add(planet);
                    }
                }
            }

            /// <summary>
            /// 完全替换游戏中原有的方法
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="__result"></param>
            /// <param name="dirtyIdx"></param>
            /// <returns></returns>
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetData), nameof(PlanetData.UpdateDirtyMesh))]
            static bool PlanetData_UpdateDirtyMesh_Prefix (PlanetData __instance, ref bool __result, int dirtyIdx)
            {
                if (__instance.dirtyFlags[dirtyIdx])
                {
                    __instance.dirtyFlags[dirtyIdx] = false;
                    int num = __instance.precision / __instance.segment;
                    int num2 = __instance.segment * __instance.segment;
                    int num3 = dirtyIdx / num2;
                    int num4 = num3 % 2;
                    int num5 = num3 / 2;
                    int num6 = dirtyIdx % num2;
                    int num7 = num6 % __instance.segment * num + num4 * __instance.data.substride;
                    int num8 = num6 / __instance.segment * num + num5 * __instance.data.substride;
                    int stride = __instance.data.stride;
                    float num9 = __instance.radius * __instance.scale + 0.2f;
                    Mesh mesh = __instance.meshes[dirtyIdx];
                    Vector3[] vertices = mesh.vertices;
                    Vector3[] normals = mesh.normals;
                    int num10 = 0;
                    for (int i = num8; i <= num8 + num; i++)
                    {
                        for (int j = num7; j <= num7 + num; j++)
                        {
                            int num11 = j + i * stride;
                            float num12 = (float)(int)__instance.data.heightData[num11] * 0.01f * __instance.scale;
                            float num13 = (float)__instance.data.GetModLevel(num11) * 0.3333333f;
                            float num14 = num9;
                            if (num13 > 0f)
                            {
                                // num14 = (float)__instance.data.GetModPlane(num11) * 0.01f * __instance.scale;
                                float modPlane = GetModPlane(__instance, num11);
                                num14 = modPlane * 0.01f * __instance.scale;

                                Instance.Logger.LogInfo($"修改的 ModPlane: {modPlane}");
                            }
                            float num15 = num12 * (1f - num13) + num14 * num13;
                            vertices[num10].x = __instance.data.vertices[num11].x * num15;
                            vertices[num10].y = __instance.data.vertices[num11].y * num15;
                            vertices[num10].z = __instance.data.vertices[num11].z * num15;
                            normals[num10].x = __instance.data.normals[num11].x * (1f - num13) + __instance.data.vertices[num11].x * num13;
                            normals[num10].y = __instance.data.normals[num11].y * (1f - num13) + __instance.data.vertices[num11].y * num13;
                            normals[num10].z = __instance.data.normals[num11].z * (1f - num13) + __instance.data.vertices[num11].z * num13;
                            normals[num10].Normalize();
                            num10++;
                        }
                    }
                    mesh.vertices = vertices;
                    mesh.normals = normals;
                    __instance.meshColliders[dirtyIdx].sharedMesh = null;
                    __instance.meshColliders[dirtyIdx].sharedMesh = mesh;
                    __result = true;
                    return false;
                }
                __result = false;
                return false;
            }

            /// <summary>
            /// 完全替换游戏中原有的方法
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="__result"></param>
            /// <param name="vpos"></param>
            /// <returns></returns>
            [HarmonyPrefix, HarmonyPatch(typeof(PlanetRawData), nameof(PlanetRawData.QueryModifiedHeight))]
            static bool PlanetRawData_QueryModifiedHeight_Prefix (PlanetRawData __instance, ref float __result, Vector3 vpos)
            {
                PlanetData planet = FindPlanetDataByRaw(__instance);

                vpos.Normalize();
                int num = __instance.PositionHash(vpos);
                int num2 = __instance.indexMap[num];
                float num3 = (float)Math.PI / (float)(__instance.precision * 2) * 1.2f;
                float num4 = num3 * num3;
                float num5 = 0f;
                float num6 = 0f;
                int num7 = __instance.stride;
                for (int i = -1; i <= 3; i++)
                {
                    for (int j = -1; j <= 3; j++)
                    {
                        int num8 = num2 + i + j * num7;
                        if ((uint)num8 >= __instance.dataLength)
                        {
                            continue;
                        }
                        float sqrMagnitude = (__instance.vertices[num8] - vpos).sqrMagnitude;
                        if (sqrMagnitude > num4)
                        {
                            continue;
                        }
                        float num9 = 1f - Mathf.Sqrt(sqrMagnitude) / num3;
                        int modLevel = __instance.GetModLevel(num8);
                        float num10 = (int)__instance.heightData[num8];
                        if (modLevel > 0)
                        {
                            //float num11 = GetModPlane(num8);
                            float num11 = GetModPlane(planet, num8);
                            Instance.Logger.LogInfo($"修改的 ModPlane: {num11}");

                            if (modLevel == 3)
                            {
                                num10 = num11;
                            }
                            else
                            {
                                float num12 = (float)modLevel * 0.3333333f;
                                num10 = (float)(int)__instance.heightData[num8] * (1f - num12) + num11 * num12;
                            }
                        }
                        num5 += num9;
                        num6 += num10 * num9;
                    }
                }
                if (num5 == 0f)
                {
                    Debug.LogWarning("bad query");
                    __result = (float)(int)__instance.heightData[0] * 0.01f;
                    return false;
                }
                __result = num6 / num5 * 0.01f;
                return false;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetModelingManager), "ModelingPlanetMain")]
            static bool PlanetModelingManager_ModelingPlanetMain_Prefix (PlanetData planet)
            {
                ThemeProto themeProto = LDB.themes.Select(planet.theme);
                if (PlanetModelingManagerAccess.currentModelingStage == 0)
                {
                    if (!planet.wanted)
                    {
                        PlanetModelingManagerAccess.currentModelingStage = 4;
                        return false;
                    }
                    if (PlanetModelingManagerAccess.tmpMeshList == null)
                    {
                        PlanetModelingManagerAccess.tmpMeshList = new List<Mesh>(100);
                        PlanetModelingManagerAccess.tmpMeshRendererList = new List<MeshRenderer>(100);
                        PlanetModelingManagerAccess.tmpMeshColliderList = new List<MeshCollider>(100);
                        PlanetModelingManagerAccess.tmpOceanCollider = null;
                        PlanetModelingManagerAccess.tmpVerts = new List<Vector3>(1700);
                        PlanetModelingManagerAccess.tmpNorms = new List<Vector3>(1700);
                        PlanetModelingManagerAccess.tmpTgnts = new List<Vector4>(1700);
                        PlanetModelingManagerAccess.tmpUvs = new List<Vector2>(1700);
                        PlanetModelingManagerAccess.tmpUv2s = new List<Vector4>(1700);
                        PlanetModelingManagerAccess.tmpTris = new List<int>(10000);
                    }
                    if (planet.heightmap == null)
                    {
                        RenderTextureDescriptor desc = new RenderTextureDescriptor(512, 512, RenderTextureFormat.RGHalf, 0);
                        desc.dimension = TextureDimension.Cube;
                        desc.useMipMap = false;
                        desc.autoGenerateMips = false;
                        planet.heightmap = new RenderTexture(desc);
                    }
                    if (PlanetModelingManager.heightmapCamera == null)
                    {
                        GameObject gameObject = new GameObject("Heightmap Camera");
                        PlanetModelingManager.heightmapCamera = gameObject.AddComponent<Camera>();
                        PlanetModelingManager.heightmapCamera.cullingMask = 1073741824;
                        PlanetModelingManager.heightmapCamera.enabled = false;
                        PlanetModelingManager.heightmapCamera.farClipPlane = 900f;
                        PlanetModelingManager.heightmapCamera.nearClipPlane = 10f;
                        PlanetModelingManager.heightmapCamera.renderingPath = RenderingPath.Forward;
                        PlanetModelingManager.heightmapCamera.allowDynamicResolution = false;
                        PlanetModelingManager.heightmapCamera.allowMSAA = false;
                        PlanetModelingManager.heightmapCamera.allowHDR = true;
                        PlanetModelingManager.heightmapCamera.depthTextureMode = DepthTextureMode.None;
                        PlanetModelingManager.heightmapCamera.clearFlags = CameraClearFlags.Color;
                        PlanetModelingManager.heightmapCamera.backgroundColor = Color.black;
                        PlanetModelingManager.heightmapCamera.depth = 0f;
                        PlanetModelingManager.heightmapCamera.SetReplacementShader(Configs.builtin.heightmapShader, "ReplaceTag");
                        gameObject.SetActive(value: false);
                    }
                    if (planet.terrainMaterial == null)
                    {
                        if (themeProto != null && themeProto.terrainMat != null)
                        {
                            planet.terrainMaterial = UnityEngine.Object.Instantiate(themeProto.terrainMat[planet.style % themeProto.terrainMat.Length]);
                            planet.terrainMaterial.name = planet.displayName + " Terrain";
                            planet.terrainMaterial.SetFloat("_Radius", planet.realRadius);
                            if (planet.terrainMaterial.HasProperty("_LightColorScreen"))
                            {
                                planet.groundScreenColor = planet.terrainMaterial.GetColor("_LightColorScreen");
                            }
                            else
                            {
                                planet.groundScreenColor = Color.black;
                            }
                        }
                        else
                        {
                            planet.terrainMaterial = UnityEngine.Object.Instantiate(Configs.builtin.planetSurfaceMatProto);
                            planet.groundScreenColor = Color.black;
                        }
                    }
                    if (planet.oceanMaterial == null)
                    {
                        if (themeProto != null && themeProto.oceanMat != null)
                        {
                            planet.oceanMaterial = UnityEngine.Object.Instantiate(themeProto.oceanMat[planet.style % themeProto.oceanMat.Length]);
                            planet.oceanMaterial.name = planet.displayName + " Ocean";
                            planet.oceanMaterial.SetFloat("_Radius", planet.realRadius);
                        }
                        else
                        {
                            planet.oceanMaterial = null;
                        }
                    }
                    if (planet.atmosMaterial == null)
                    {
                        if (themeProto != null && themeProto.atmosMat != null)
                        {
                            planet.atmosMaterial = UnityEngine.Object.Instantiate(themeProto.atmosMat[planet.style % themeProto.atmosMat.Length]);
                            planet.atmosMaterial.name = planet.displayName + " Atmos";
                        }
                        else
                        {
                            planet.atmosMaterial = null;
                        }
                    }
                    if (planet.reformMaterial0 == null)
                    {
                        planet.reformMaterial0 = UnityEngine.Object.Instantiate(Configs.builtin.planetReformMatProto0);
                    }
                    if (planet.reformMaterial1 == null)
                    {
                        planet.reformMaterial1 = UnityEngine.Object.Instantiate(Configs.builtin.planetReformMatProto1);
                    }
                    if (planet.ambientDesc == null)
                    {
                        if (themeProto != null && themeProto.ambientDesc != null)
                        {
                            planet.ambientDesc = themeProto.ambientDesc[planet.style % themeProto.ambientDesc.Length];
                        }
                        else
                        {
                            planet.ambientDesc = null;
                        }
                    }
                    if (planet.ambientSfx == null && themeProto != null && themeProto.ambientSfx != null)
                    {
                        planet.ambientSfx = themeProto.ambientSfx[planet.style % themeProto.ambientSfx.Length];
                        planet.ambientSfxVolume = themeProto.SFXVolume;
                    }
                    if (planet.minimapMaterial == null)
                    {
                        if (themeProto != null && themeProto.minimapMat != null)
                        {
                            planet.minimapMaterial = UnityEngine.Object.Instantiate(themeProto.minimapMat[planet.style % themeProto.minimapMat.Length]);
                        }
                        else
                        {
                            planet.minimapMaterial = UnityEngine.Object.Instantiate(Configs.builtin.planetMinimapDefault);
                        }
                        planet.minimapMaterial.name = planet.displayName + " Minimap";
                        planet.minimapMaterial.SetTexture("_HeightMap", planet.heightmap);
                    }
                    PlanetModelingManagerAccess.tmpMeshList.Clear();
                    PlanetModelingManagerAccess.tmpMeshRendererList.Clear();
                    PlanetModelingManagerAccess.tmpMeshColliderList.Clear();
                    PlanetModelingManagerAccess.tmpOceanCollider = null;
                    PlanetModelingManagerAccess.tmpTris.Clear();
                    PlanetModelingManagerAccess.currentModelingStage = 1;
                }
                else if (PlanetModelingManagerAccess.currentModelingStage == 1)
                {
                    if (!planet.wanted)
                    {
                        PlanetModelingManagerAccess.currentModelingStage = 4;
                        return false;
                    }
                    PlanetModelingManagerAccess.tmpPlanetGameObject = new GameObject(planet.displayName);
                    PlanetModelingManagerAccess.tmpPlanetGameObject.layer = 31;
                    PlanetSimulator sim = PlanetModelingManagerAccess.tmpPlanetGameObject.AddComponent<PlanetSimulator>();
                    GameMain.universeSimulator.SetPlanetSimulator(sim, planet);
                    PlanetModelingManagerAccess.tmpPlanetGameObject.transform.localPosition = Vector3.zero;
                    PlanetModelingManagerAccess.tmpPlanetBodyGameObject = new GameObject("Planet Body");
                    PlanetModelingManagerAccess.tmpPlanetBodyGameObject.transform.SetParent(PlanetModelingManagerAccess.tmpPlanetGameObject.transform, worldPositionStays: false);
                    PlanetModelingManagerAccess.tmpPlanetBodyGameObject.layer = 31;
                    PlanetModelingManagerAccess.tmpPlanetReformGameObject = new GameObject("Terrain Reform");
                    PlanetModelingManagerAccess.tmpPlanetReformGameObject.transform.SetParent(PlanetModelingManagerAccess.tmpPlanetBodyGameObject.transform, worldPositionStays: false);
                    PlanetModelingManagerAccess.tmpPlanetReformGameObject.layer = 14;
                    MeshFilter meshFilter = PlanetModelingManagerAccess.tmpPlanetReformGameObject.AddComponent<MeshFilter>();
                    PlanetModelingManagerAccess.tmpPlanetReformRenderer = PlanetModelingManagerAccess.tmpPlanetReformGameObject.AddComponent<MeshRenderer>();
                    meshFilter.sharedMesh = Configs.builtin.planetReformMesh;
                    Material[] sharedMaterials = new Material[2] { planet.reformMaterial0, planet.reformMaterial1 };
                    PlanetModelingManagerAccess.tmpPlanetReformRenderer.sharedMaterials = sharedMaterials;
                    PlanetModelingManagerAccess.tmpPlanetReformRenderer.receiveShadows = false;
                    PlanetModelingManagerAccess.tmpPlanetReformRenderer.lightProbeUsage = LightProbeUsage.Off;
                    PlanetModelingManagerAccess.tmpPlanetReformRenderer.shadowCastingMode = ShadowCastingMode.Off;
                    float num = (planet.realRadius + 0.2f + 0.025f) * 2f;
                    PlanetModelingManagerAccess.tmpPlanetReformRenderer.transform.localScale = new Vector3(num, num, num);
                    PlanetModelingManagerAccess.tmpPlanetReformRenderer.transform.rotation = Quaternion.identity;
                    if (planet.waterItemId != 0)
                    {
                        GameObject gameObject2 = UnityEngine.Object.Instantiate(Configs.builtin.oceanSphere, PlanetModelingManagerAccess.tmpPlanetBodyGameObject.transform);
                        gameObject2.name = "Ocean Sphere";
                        gameObject2.layer = 31;
                        gameObject2.transform.localPosition = Vector3.zero;
                        gameObject2.transform.localScale = Vector3.one * ((planet.realRadius + planet.waterHeight) * 2f);
                        Renderer component = gameObject2.GetComponent<Renderer>();
                        PlanetModelingManagerAccess.tmpOceanCollider = gameObject2.GetComponent<Collider>();
                        if (component != null)
                        {
                            component.enabled = planet.oceanMaterial != null;
                            component.shadowCastingMode = ShadowCastingMode.Off;
                            component.receiveShadows = false;
                            component.lightProbeUsage = LightProbeUsage.Off;
                            component.sharedMaterial = planet.oceanMaterial;
                        }
                    }
                    int num2 = planet.precision / planet.segment;
                    int num3 = num2 + 1;
                    for (int i = 0; i < num2; i++)
                    {
                        for (int j = 0; j < num2; j++)
                        {
                            PlanetModelingManagerAccess.tmpTris.Add(i + 1 + (j + 1) * num3);
                            PlanetModelingManagerAccess.tmpTris.Add(i + (j + 1) * num3);
                            PlanetModelingManagerAccess.tmpTris.Add(i + j * num3);
                            PlanetModelingManagerAccess.tmpTris.Add(i + j * num3);
                            PlanetModelingManagerAccess.tmpTris.Add(i + 1 + j * num3);
                            PlanetModelingManagerAccess.tmpTris.Add(i + 1 + (j + 1) * num3);
                        }
                    }
                    PlanetModelingManagerAccess.currentModelingStage = 2;
                }
                else if (PlanetModelingManagerAccess.currentModelingStage == 2)
                {
                    if (!planet.wanted)
                    {
                        PlanetModelingManagerAccess.currentModelingStage = 4;
                        return false;
                    }
                    int precision = planet.precision;
                    int num4 = precision / planet.segment;
                    PlanetRawData data = planet.data;
                    float scale = planet.scale;
                    float num5 = planet.radius * scale + 0.2f;
                    int stride = data.stride;
                    int num6 = 0;
                    int num7 = (GameMain.isLoading ? 3 : 2);
                    int num8 = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        int num9 = k % 2 * (precision + 1);
                        int num10 = k / 2 * (precision + 1);
                        for (int l = 0; l < precision; l += num4)
                        {
                            for (int m = 0; m < precision; m += num4)
                            {
                                if (num8 == 0 && num6 < PlanetModelingManagerAccess.tmpMeshList.Count)
                                {
                                    num6++;
                                    continue;
                                }
                                Mesh mesh = new Mesh();
                                PlanetModelingManagerAccess.tmpMeshList.Add(mesh);
                                PlanetModelingManagerAccess.tmpVerts.Clear();
                                PlanetModelingManagerAccess.tmpNorms.Clear();
                                PlanetModelingManagerAccess.tmpTgnts.Clear();
                                PlanetModelingManagerAccess.tmpUvs.Clear();
                                PlanetModelingManagerAccess.tmpUv2s.Clear();
                                GameObject gameObject3 = new GameObject("Surface");
                                gameObject3.layer = 30;
                                gameObject3.transform.SetParent(PlanetModelingManagerAccess.tmpPlanetBodyGameObject.transform, worldPositionStays: false);
                                for (int n = l; n <= l + num4 && n <= precision; n++)
                                {
                                    for (int num11 = m; num11 <= m + num4 && num11 <= precision; num11++)
                                    {
                                        int num12 = num9 + num11;
                                        int num13 = num10 + n;
                                        int num14 = num12 + num13 * stride;
                                        int num15 = num14;
                                        if (n == 0)
                                        {
                                            int num16 = (k + 3) % 4;
                                            int num17 = num16 % 2 * (precision + 1);
                                            int num18 = num16 / 2 * (precision + 1);
                                            int num19 = precision;
                                            int num20 = precision - num11;
                                            int num21 = num17 + num19;
                                            int num22 = num18 + num20;
                                            num15 = num21 + num22 * stride;
                                        }
                                        else if (num11 == 0)
                                        {
                                            int num23 = (k + 3) % 4;
                                            int num24 = num23 % 2 * (precision + 1);
                                            int num25 = num23 / 2 * (precision + 1);
                                            int num26 = precision - n;
                                            int num27 = precision;
                                            int num28 = num24 + num26;
                                            int num29 = num25 + num27;
                                            num15 = num28 + num29 * stride;
                                        }
                                        if (n == precision)
                                        {
                                            int num30 = (k + 1) % 4;
                                            int num31 = num30 % 2 * (precision + 1);
                                            int num32 = num30 / 2 * (precision + 1);
                                            int num33 = 0;
                                            int num34 = precision - num11;
                                            int num35 = num31 + num33;
                                            int num36 = num32 + num34;
                                            num15 = num35 + num36 * stride;
                                        }
                                        else if (num11 == precision)
                                        {
                                            int num37 = (k + 1) % 4;
                                            int num38 = num37 % 2 * (precision + 1);
                                            int num39 = num37 / 2 * (precision + 1);
                                            int num40 = precision - n;
                                            int num41 = 0;
                                            int num42 = num38 + num40;
                                            int num43 = num39 + num41;
                                            num15 = num42 + num43 * stride;
                                        }
                                        float num44 = (float)(int)data.heightData[num14] * 0.01f * scale;
                                        float num45 = (float)data.GetModLevel(num14) * 0.3333333f;
                                        float num46 = num5;
                                        if (num45 > 0f)
                                        {
                                            //num46 = (float)data.GetModPlane(num14) * 0.01f * scale;
                                            num46 = GetModPlane(planet, num14);
                                            Plugin.Instance.Logger.LogInfo($"修改的 ModPlane: {num46}");
                                        }
                                        float num47 = num44 * (1f - num45) + num46 * num45;
                                        Vector3 item = data.vertices[num14] * num47;
                                        PlanetModelingManagerAccess.tmpVerts.Add(item);
                                        PlanetModelingManagerAccess.tmpNorms.Add(data.vertices[num14]);
                                        Vector3 vector = Vector3.Cross(data.vertices[num14], Vector3.up).normalized;
                                        if (vector.sqrMagnitude == 0f)
                                        {
                                            vector = Vector3.right;
                                        }
                                        PlanetModelingManagerAccess.tmpTgnts.Add(new Vector4(vector.x, vector.y, vector.z, 1f));
                                        PlanetModelingManagerAccess.tmpUvs.Add(new Vector2(((float)num12 + 0.5f) / (float)stride, ((float)num13 + 0.5f) / (float)stride));
                                        PlanetModelingManagerAccess.tmpUv2s.Add(new Vector4((float)(int)data.biomoData[num14] * 0.01f, (float)data.temprData[num14] * 0.01f, (float)num14 + 0.3f, (float)num15 + 0.3f));
                                    }
                                }
                                mesh.indexFormat = IndexFormat.UInt16;
                                mesh.SetVertices(PlanetModelingManagerAccess.tmpVerts);
                                mesh.SetNormals(PlanetModelingManagerAccess.tmpNorms);
                                mesh.SetTangents(PlanetModelingManagerAccess.tmpTgnts);
                                mesh.SetUVs(0, PlanetModelingManagerAccess.tmpUvs);
                                mesh.SetUVs(1, PlanetModelingManagerAccess.tmpUv2s);
                                mesh.SetTriangles(PlanetModelingManagerAccess.tmpTris, 0, calculateBounds: true, 0);
                                mesh.RecalculateNormals();
                                mesh.GetNormals(PlanetModelingManagerAccess.tmpNorms);
                                for (int num48 = 0; num48 < PlanetModelingManagerAccess.tmpNorms.Count; num48++)
                                {
                                    int num49 = (int)PlanetModelingManagerAccess.tmpUv2s[num48].z;
                                    int num50 = (int)PlanetModelingManagerAccess.tmpUv2s[num48].w;
                                    data.normals[num49] = data.normals[num49] + PlanetModelingManagerAccess.tmpNorms[num48];
                                    data.normals[num50] = data.normals[num50] + PlanetModelingManagerAccess.tmpNorms[num48];
                                }
                                MeshFilter meshFilter2 = gameObject3.AddComponent<MeshFilter>();
                                MeshRenderer meshRenderer = gameObject3.AddComponent<MeshRenderer>();
                                MeshCollider meshCollider = gameObject3.AddComponent<MeshCollider>();
                                meshFilter2.sharedMesh = mesh;
                                meshRenderer.sharedMaterial = planet.terrainMaterial;
                                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                                meshRenderer.receiveShadows = false;
                                meshRenderer.lightProbeUsage = LightProbeUsage.Off;
                                meshCollider.sharedMesh = mesh;
                                PlanetModelingManagerAccess.tmpMeshRendererList.Add(meshRenderer);
                                PlanetModelingManagerAccess.tmpMeshColliderList.Add(meshCollider);
                                num8++;
                                if (num8 == num7)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    num6 = 0;
                    int num51 = (GameMain.isLoading ? 15 : 5);
                    for (int num52 = 0; num52 < PlanetModelingManagerAccess.tmpMeshList.Count; num52++)
                    {
                        int num53 = num52 / num51;
                        if (num53 >= PlanetModelingManagerAccess.currentModelingSeamNormal)
                        {
                            if (num53 > PlanetModelingManagerAccess.currentModelingSeamNormal)
                            {
                                PlanetModelingManagerAccess.currentModelingSeamNormal++;
                                return false;
                            }
                            Mesh mesh2 = PlanetModelingManagerAccess.tmpMeshList[num52];
                            PlanetModelingManagerAccess.tmpNorms.Clear();
                            PlanetModelingManagerAccess.tmpUv2s.Clear();
                            int vertexCount = mesh2.vertexCount;
                            mesh2.GetUVs(1, PlanetModelingManagerAccess.tmpUv2s);
                            for (int num54 = 0; num54 < vertexCount; num54++)
                            {
                                int num55 = (int)PlanetModelingManagerAccess.tmpUv2s[num54].z;
                                PlanetModelingManagerAccess.tmpNorms.Add(data.normals[num55].normalized);
                            }
                            mesh2.SetNormals(PlanetModelingManagerAccess.tmpNorms);
                        }
                    }
                    PlanetModelingManagerAccess.currentModelingStage = 3;
                }
                else if (PlanetModelingManagerAccess.currentModelingStage == 3)
                {
                    if (!planet.wanted)
                    {
                        PlanetModelingManagerAccess.currentModelingStage = 4;
                        return false;
                    }
                    PlanetModelingManagerAccess.tmpPlanetBodyGameObject.SetActive(value: true);
                    PlanetModelingManagerAccess.tmpPlanetReformGameObject.SetActive(value: true);
                    PlanetModelingManager.heightmapCamera.transform.localPosition = PlanetModelingManagerAccess.tmpPlanetGameObject.transform.localPosition;
                    PlanetModelingManager.heightmapCamera.RenderToCubemap(planet.heightmap, 63);
                    PlanetModelingManagerAccess.currentModelingStage = 4;
                }
                else
                {
                    if (PlanetModelingManagerAccess.currentModelingStage != 4)
                    {
                        return false;
                    }
                    if (planet.wanted)
                    {
                        planet.gameObject = PlanetModelingManagerAccess.tmpPlanetGameObject;
                        planet.bodyObject = PlanetModelingManagerAccess.tmpPlanetBodyGameObject;
                        PlanetSimulator component2 = PlanetModelingManagerAccess.tmpPlanetGameObject.GetComponent<PlanetSimulator>();
                        component2.surfaceRenderer = new Renderer[PlanetModelingManagerAccess.tmpMeshRendererList.Count];
                        component2.surfaceCollider = new Collider[PlanetModelingManagerAccess.tmpMeshColliderList.Count];
                        for (int num56 = 0; num56 < PlanetModelingManagerAccess.tmpMeshList.Count; num56++)
                        {
                            planet.meshes[num56] = PlanetModelingManagerAccess.tmpMeshList[num56];
                            planet.meshRenderers[num56] = PlanetModelingManagerAccess.tmpMeshRendererList[num56];
                            planet.meshColliders[num56] = PlanetModelingManagerAccess.tmpMeshColliderList[num56];
                        }
                        for (int num57 = 0; num57 < PlanetModelingManagerAccess.tmpMeshRendererList.Count; num57++)
                        {
                            PlanetModelingManagerAccess.tmpMeshRendererList[num57].gameObject.layer = 31;
                            PlanetModelingManagerAccess.tmpMeshRendererList[num57].sharedMaterial = planet.terrainMaterial;
                            PlanetModelingManagerAccess.tmpMeshRendererList[num57].receiveShadows = false;
                            PlanetModelingManagerAccess.tmpMeshRendererList[num57].shadowCastingMode = ShadowCastingMode.Off;
                            component2.surfaceRenderer[num57] = PlanetModelingManagerAccess.tmpMeshRendererList[num57];
                            component2.surfaceCollider[num57] = PlanetModelingManagerAccess.tmpMeshColliderList[num57];
                        }
                        component2.oceanCollider = PlanetModelingManagerAccess.tmpOceanCollider;
                        component2.sphereCollider = PlanetModelingManagerAccess.tmpPlanetBodyGameObject.AddComponent<SphereCollider>();
                        if (component2.sphereCollider != null)
                        {
                            component2.sphereCollider.enabled = false;
                        }
                        component2.sphereCollider.radius = planet.realRadius;
                        component2.reformRenderer = PlanetModelingManagerAccess.tmpPlanetReformRenderer;
                        component2.reformMat0 = planet.reformMaterial0;
                        component2.reformMat1 = planet.reformMaterial1;
                        Material sharedMaterial = component2.surfaceRenderer[0].sharedMaterial;
                        if (planet.type != EPlanetType.Gas)
                        {
                            component2.reformMat0.SetColor("_AmbientColor0", sharedMaterial.GetColor("_AmbientColor0"));
                            component2.reformMat0.SetColor("_AmbientColor1", sharedMaterial.GetColor("_AmbientColor1"));
                            component2.reformMat0.SetColor("_AmbientColor2", sharedMaterial.GetColor("_AmbientColor2"));
                            component2.reformMat0.SetColor("_LightColorScreen", sharedMaterial.GetColor("_LightColorScreen"));
                            component2.reformMat0.SetFloat("_Multiplier", sharedMaterial.GetFloat("_Multiplier"));
                            component2.reformMat0.SetFloat("_AmbientInc", sharedMaterial.GetFloat("_AmbientInc"));
                            component2.reformMat1.SetColor("_AmbientColor0", sharedMaterial.GetColor("_AmbientColor0"));
                            component2.reformMat1.SetColor("_AmbientColor1", sharedMaterial.GetColor("_AmbientColor1"));
                            component2.reformMat1.SetColor("_AmbientColor2", sharedMaterial.GetColor("_AmbientColor2"));
                            component2.reformMat1.SetColor("_LightColorScreen", sharedMaterial.GetColor("_LightColorScreen"));
                            component2.reformMat1.SetFloat("_Multiplier", sharedMaterial.GetFloat("_Multiplier"));
                            component2.reformMat1.SetFloat("_AmbientInc", sharedMaterial.GetFloat("_AmbientInc"));
                        }
                        PlanetModelingManagerAccess.tmpPlanetGameObject.transform.localPosition = Vector3.zero;
                        PlanetModelingManager.heightmapCamera.transform.localPosition = Vector3.zero;
                        PlanetModelingManagerAccess.tmpPlanetBodyGameObject.SetActive(value: true);
                        PlanetModelingManagerAccess.tmpPlanetReformGameObject.SetActive(value: true);
                        PlanetModelingManagerAccess.tmpPlanetGameObject = null;
                        PlanetModelingManagerAccess.tmpPlanetBodyGameObject = null;
                        PlanetModelingManagerAccess.tmpPlanetReformGameObject = null;
                        PlanetModelingManagerAccess.tmpPlanetReformRenderer = null;
                        PlanetModelingManagerAccess.tmpMeshList.Clear();
                        PlanetModelingManagerAccess.tmpMeshRendererList.Clear();
                        PlanetModelingManagerAccess.tmpMeshColliderList.Clear();
                        PlanetModelingManagerAccess.tmpOceanCollider = null;
                        PlanetModelingManagerAccess.tmpTris.Clear();
                        PlanetModelingManagerAccess.tmpVerts.Clear();
                        PlanetModelingManagerAccess.tmpNorms.Clear();
                        PlanetModelingManagerAccess.tmpTgnts.Clear();
                        PlanetModelingManagerAccess.tmpUvs.Clear();
                        PlanetModelingManagerAccess.tmpUv2s.Clear();
                        PlanetModelingManagerAccess.currentModelingPlanet = null;
                        PlanetModelingManagerAccess.currentModelingStage = 0;
                        PlanetModelingManagerAccess.currentModelingSeamNormal = 0;
                        planet.NotifyLoaded();
                        if (planet.star.loaded)
                        {
                            planet.star.NotifyLoaded();
                        }
                    }
                    else
                    {
                        for (int num58 = 0; num58 < PlanetModelingManagerAccess.tmpMeshList.Count; num58++)
                        {
                            UnityEngine.Object.Destroy(PlanetModelingManagerAccess.tmpMeshList[num58]);
                        }
                        UnityEngine.Object.Destroy(PlanetModelingManagerAccess.tmpPlanetGameObject);
                        PlanetModelingManagerAccess.tmpPlanetGameObject = null;
                        PlanetModelingManagerAccess.tmpPlanetBodyGameObject = null;
                        PlanetModelingManagerAccess.tmpPlanetReformGameObject = null;
                        PlanetModelingManagerAccess.tmpPlanetReformRenderer = null;
                        PlanetModelingManagerAccess.tmpMeshList.Clear();
                        PlanetModelingManagerAccess.tmpTris.Clear();
                        PlanetModelingManagerAccess.tmpVerts.Clear();
                        PlanetModelingManagerAccess.tmpNorms.Clear();
                        PlanetModelingManagerAccess.tmpTgnts.Clear();
                        PlanetModelingManagerAccess.tmpUvs.Clear();
                        PlanetModelingManagerAccess.tmpUv2s.Clear();
                        PlanetModelingManagerAccess.currentModelingPlanet = null;
                        PlanetModelingManagerAccess.currentModelingStage = 0;
                        PlanetModelingManagerAccess.currentModelingSeamNormal = 0;
                    }
                }
                return false;
            }

            static private float GetModPlane (PlanetData planet, int index)
            {
                return ((planet.data.modData[index >> 1] >> ((index & 1) << 2) + 2) & 3) * 133 + (int)(planet.radius + 0.1f) * 100 + 20;
            }

            static private PlanetData FindPlanetDataByRaw (PlanetRawData raw)
            {
                // 如果映射表中还没有这个 PlanetRawData 对象，则先构造映射关系
                if (!rawDataToPlanet.ContainsKey(raw))
                {
                    PlanetData planet = planets.Find(p => p.data.Equals(raw));
                    if (planet == default(PlanetData))
                    {
                        Instance.Logger.LogInfo("未能在行星列表中找到该 PlanetRawData 所属的行星");
                        return null;
                    }
                    rawDataToPlanet.Add(raw, planet);
                    return planet;
                }
                return rawDataToPlanet[raw];
            }
        }
    }
}
