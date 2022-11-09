using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace DSPAddPlanet
{
    static class ConfigUtility
    {
        public const string MOD_DATA_DIR = "modData/IndexOutOfRange.DSPAddPlanet/";

        public const string XML_CONFIG_FILE = "config.xml";

        public const string TEXT_CONFIG_FILE = "config.txt";

        public const bool DEFAULT_IS_BIRTH_POINT = false;

        public const bool DEFAULT_GAS_GIANT = false;

        public const int DEFAULT_INFO_SEED = 0;

        public const int DEFAULT_GEN_SEED = 0;

        public const bool DEFAULT_FORCE_PLANET_RADIUS = false;

        public const int DEFAULT_RADIUS = 200;

        public const int MIN_RADIUS = 50;

        public const int MAX_RADIUS = 600;

        public const int DEFAULT_ORBITAL_PERIOD = 3600;

        public const int DEFAULT_ROTATION_PERIOD = 3600;

        public const bool DEFAULT_IS_TIDAL_LOCKED = true;

        public const float DEFAULT_ORBIT_INCLINATION = 0;

        public const float DEFAULT_OBLIQUITY = 0;

        public const bool DEFAULT_DONT_GENERATE_VEIN = true;

        /// <summary>
        /// 读取配置，并按 index 对配置进行排序
        /// </summary>
        /// <param name="globalPlanetConfig"></param>
        /// <param name="gameNameSpecificConfig"></param>
        static public void ReadConfig (
            Dictionary<string, List<AdditionalPlanetConfig>> globalPlanetConfig,
            Dictionary<string, List<AdditionalPlanetConfig>> gameNameSpecificConfig
        )
        {
            globalPlanetConfig.Clear();
            gameNameSpecificConfig.Clear();

            // 检查并创建模组数据文件夹
            string modDataDir = GameConfig.gameSaveFolder + MOD_DATA_DIR;
            if (!Directory.Exists(modDataDir))
            {
                Directory.CreateDirectory(modDataDir);
            }

            string xmlConfigFilePath = modDataDir + XML_CONFIG_FILE;
            string textConfigFilePath = modDataDir + TEXT_CONFIG_FILE;
            if (File.Exists(xmlConfigFilePath))
            {
                // 存在 xml 配置，填充 global 和 game name specific
                try
                {
                    ReadXmlConfig (xmlConfigFilePath, globalPlanetConfig, gameNameSpecificConfig);
                }
                catch (Exception e)
                {
                    Plugin.Instance.Logger.LogError(e.Message);
                    Plugin.Instance.Logger.LogError(e.StackTrace);
                    globalPlanetConfig.Clear();
                    gameNameSpecificConfig.Clear();
                }
            }
            else if (File.Exists(textConfigFilePath))
            {
                // 不存在 xml 配置，但是存在 txt 配置，只填充 game name specific
                try
                {
                    ReadTextConfig(textConfigFilePath, gameNameSpecificConfig);
                }
                catch (Exception e)
                {
                    Plugin.Instance.Logger.LogError(e.Message);
                    Plugin.Instance.Logger.LogError(e.Message);
                    gameNameSpecificConfig.Clear();
                }
            }
            else
            {
                // 既不存在 xml 配置，也不存在 txt 配置
                CreateExampleXmlConfigFile(xmlConfigFilePath);
            }

            foreach (var pair in globalPlanetConfig) {
                pair.Value.Sort((a, b) => a.Index - b.Index);
            }
            foreach (var pair in gameNameSpecificConfig) {
                pair.Value.Sort((a, b) => a.Index - b.Index);
            }
        }

        static private void ReadXmlConfig (
            string filePath,
            Dictionary<string, List<AdditionalPlanetConfig>> globalPlanetConfig,
            Dictionary<string, List<AdditionalPlanetConfig>> gameNameSpecificConfig
        )
        {
            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            XmlNode nodeConfig = document.SelectSingleNode("Config");
            if (nodeConfig == null)
            {
                return;
            }

            // 读取全局行星配置，这些行星配置不需要指定GameName
            XmlNode nodeGlobal = nodeConfig.SelectSingleNode("Global");
            if (nodeGlobal != null)
            {
                XmlNode nodePlanets = nodeGlobal.SelectSingleNode("Planets");
                if (nodePlanets != null)
                {
                    Plugin.Instance.Logger.LogInfo("Reading global planet config...");
                    ReadPlanetNodes(nodePlanets, globalPlanetConfig, false);
                    Plugin.Instance.Logger.LogInfo("Global planet config size: " + globalPlanetConfig.Count);
                }
            }

            // 读取指定GameName的行星配置
            // 备注：考虑到xml的tag命名规则与普通文件名的命名规则不一致，不能使用<GameNameSpecific><GameName><Planets><Planet><UniqueStarId>...的方式来组织配置信息
            XmlNode nodeGameNameSpecific = nodeConfig.SelectSingleNode("GameNameSpecific");
            if (nodeGameNameSpecific != null)
            {
                XmlNode nodePlanets = nodeGameNameSpecific.SelectSingleNode("Planets");
                if (nodePlanets != null)
                {
                    Plugin.Instance.Logger.LogInfo("Reading game name specific planet config...");
                    ReadPlanetNodes(nodePlanets, gameNameSpecificConfig, true);
                    Plugin.Instance.Logger.LogInfo("Config size: " + gameNameSpecificConfig.Count);
                }
            }
        }

        static private void ReadPlanetNodes (XmlNode parent, Dictionary<string, List<AdditionalPlanetConfig>> planetConfig, bool isGameNameRequired)
        {
            // 预先获取主题id
            List<int> availableThemeIds = new List<int>();
            foreach (ThemeProto t in LDB.themes.dataArray)
            {
                availableThemeIds.Add(t.ID);
            }

            // 遍历子节点，填充 planetConfig 对象
            XmlNodeList nodesPlanet = parent.ChildNodes;
            for (int i = 0; i < nodesPlanet.Count; ++i)
            {
                XmlNode nodePlanet = nodesPlanet.Item(i);
                if (nodePlanet.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                // (required) unique star id
                string uniqueStarId = ReadUniqueStarId(nodePlanet, isGameNameRequired);
                Plugin.Instance.Logger.LogInfo(uniqueStarId);

                AdditionalPlanetConfig config = default;

                // (optional)
                config.IsBirthPoint = ReadBoolNode(nodePlanet, "IsBirthPoint", false, DEFAULT_IS_BIRTH_POINT);

                // (required) index
                config.Index = ReadIntNode(nodePlanet, "Index", true, default, 0);

                // (required) orbit around
                config.OrbitAround = ReadIntNode(nodePlanet, "OrbitAround", true, default, 0);

                // (required) orbit index
                config.OrbitIndex = ReadIntNode(nodePlanet, "OrbitIndex", true, default, 0);

                // (required) number
                config.Number = ReadIntNode(nodePlanet, "Number", true, default, 1);

                // (optional) gas giant
                config.GasGiant = ReadBoolNode(nodePlanet, "GasGiant", false, DEFAULT_GAS_GIANT);

                // (optional) info seed
                config.InfoSeed = ReadIntNode(nodePlanet, "InfoSeed", false, DEFAULT_INFO_SEED);

                // (optional) gen seed
                config.GenSeed = ReadIntNode(nodePlanet, "GenSeed", false, DEFAULT_GEN_SEED);

                // (optional) force planet radius
                bool forcePlanetRadius = ReadBoolNode(nodePlanet, "ForcePlanetRadius", false, DEFAULT_FORCE_PLANET_RADIUS);

                // (optional) radius
                if (forcePlanetRadius)
                {
                    config.Radius = ReadIntNode(nodePlanet, "Radius", false, DEFAULT_RADIUS, 1);
                }
                else
                {
                    config.Radius = ReadIntNode(nodePlanet, "Radius", false, DEFAULT_RADIUS, MIN_RADIUS, MAX_RADIUS);
                }

                // (optional) orbital period
                config.OrbitalPeriod = ReadIntNode(nodePlanet, "OrbitalPeriod", false, DEFAULT_ORBITAL_PERIOD, 1);

                // (optional) rotation period
                config.RotationPeriod = ReadIntNode(nodePlanet, "RotationPeriod", false, DEFAULT_ROTATION_PERIOD, 1);

                // (optional) is tidal locked
                config.IsTidalLocked = ReadBoolNode(nodePlanet, "IsTidalLocked", false, DEFAULT_IS_TIDAL_LOCKED);

                // (optional) orbit inclination
                config.OrbitInclination = ReadFloatNode(nodePlanet, "OrbitInclination", false, DEFAULT_ORBIT_INCLINATION, 0, 360);

                // (optional) obliquity
                config.Obliquity = ReadFloatNode(nodePlanet, "Obliquity", false, DEFAULT_OBLIQUITY, 0, 360);

                // (optional) dont generate vein
                config.DontGenerateVein = ReadBoolNode(nodePlanet, "DontGenerateVein", false, DEFAULT_DONT_GENERATE_VEIN);

                // (optional) theme id
                config._HasThemeId = nodePlanet.SelectSingleNode("ThemeId") != null;
                if (config._HasThemeId)
                {
                    config.ThemeId = ReadIntNode(nodePlanet, "ThemeId", true, default, availableThemeIds);
                }

                // (optional) orbit longitude
                config._HasOrbitLongitude = nodePlanet.SelectSingleNode("OrbitLongitude") != null;
                if (config._HasOrbitLongitude)
                {
                    config.OrbitLongitude = ReadFloatNode(nodePlanet, "OrbitLongitude", true, default, 0, 360);
                }

                // (optional) replace all veins to
                config._HasReplaceAllVeinsTo = nodePlanet.SelectSingleNode("ReplaceAllVeinsTo") != null;
                if (config._HasReplaceAllVeinsTo)
                {
                    config.ReplaceAllVeinsTo = ReadEnumNode<EVeinType>(nodePlanet, "ReplaceAllVeinsTo", true, default);
                }

                // (optional) vein custom
                config._HasVeinCustom = nodePlanet.SelectSingleNode("VeinCustom") != null;
                if (config._HasVeinCustom)
                {
                    Dictionary<EVeinType, AdditionalPlanetConfig.VeinConfig> veinCustom = new Dictionary<EVeinType, AdditionalPlanetConfig.VeinConfig>();
                    XmlNode nodeVeinCustom = nodePlanet.SelectSingleNode("VeinCustom");
                    XmlNodeList nodesVeinType = nodeVeinCustom.ChildNodes;
                    for (int j = 0; j < nodesVeinType.Count; ++j)
                    {
                        XmlNode nodeVeinType = nodesVeinType.Item(j);
                        if (nodeVeinType.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        if (!Enum.TryParse(nodeVeinType.Name.Trim(), out EVeinType veinType))
                        {
                            throw new Exception($"Invalid vein type '{nodeVeinType.Name}' in <VeinCustom>");
                        }

                        // 存在合法的 vein type ，代表用户希望对该种矿物的生成过程进行干预
                        AdditionalPlanetConfig.VeinConfig veinConfig = default;

                        // 矿脉数量
                        // (optional) vein group count
                        AdditionalPlanetConfig.VeinConfig.CustomValue veinGroupCount = default;
                        XmlNode nodeVeinGroupCount = nodeVeinType.SelectSingleNode("VeinGroupCount");
                        if (nodeVeinGroupCount == null)
                        {
                            veinGroupCount.Type = AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Default;
                        }
                        else
                        {
                            try
                            {
                                veinGroupCount = ReadVeinCustomValue(
                                    nodeVeinGroupCount,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_GROUP_COUNT_RANDOM_BASE_VALUE,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_GROUP_COUNT_RANDOM_COEF,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_GROUP_COUNT_RANDOM_MUL_OFFSET,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_GROUP_COUNT_RANDOM_ADD_OFFSET
                                );
                            }
                            catch (Exception e)
                            {
                                throw new Exception("Error in <VeinCustom><VeinGroupCount>: " + e.Message);
                            }
                        }
                        veinConfig.VeinGroupCount = veinGroupCount;

                        // 矿脉中矿点数量
                        // (optional) vein spot count
                        AdditionalPlanetConfig.VeinConfig.CustomValue veinSpotCount = default;
                        XmlNode nodeVeinSpotCount = nodeVeinType.SelectSingleNode("VeinSpotCount");
                        if (nodeVeinSpotCount == null)
                        {
                            veinSpotCount.Type = AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Default;
                        }
                        else
                        {
                            try
                            {
                                veinSpotCount = ReadVeinCustomValue(
                                    nodeVeinSpotCount,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_SPOT_COUNT_RANDOM_BASE_VALUE,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_SPOT_COUNT_RANDOM_COEF,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_SPOT_COUNT_RANDOM_MUL_OFFSET,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_SPOT_COUNT_RANDOM_ADD_OFFSET
                                );
                            }
                            catch (Exception e)
                            {
                                throw new Exception("Error in <VeinCustom><VeinSpotCount>: " + e.Message);
                            }
                        }
                        veinConfig.VeinSpotCount = veinSpotCount;

                        // 矿点中矿物数量
                        // (optional) vein spot count
                        AdditionalPlanetConfig.VeinConfig.CustomValue veinAmount = default;
                        XmlNode nodeVeinAmount = nodeVeinType.SelectSingleNode("VeinAmount");
                        if (nodeVeinAmount == null)
                        {
                            veinAmount.Type = AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Default;
                        }
                        else
                        {
                            try
                            {
                                veinAmount = ReadVeinCustomValue(
                                    nodeVeinAmount,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_AMOUNT_RANDOM_BASE_VALUE,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_AMOUNT_RANDOM_COEF,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_AMOUNT_RANDOM_MUL_OFFSET,
                                    AdditionalPlanetConfig.VeinConfig.DEFAULT_VEIN_AMOUNT_RANDOM_ADD_OFFSET
                                );
                            }
                            catch (Exception e)
                            {
                                throw new Exception("Error in <VeinCustom><VeinAmount>: " + e.Message);
                            }
                        }
                        veinConfig.VeinAmount = veinAmount;

                        veinCustom.Add(veinType, veinConfig);
                    }

                    config.VeinCustom = veinCustom;
                }

                if (planetConfig.ContainsKey(uniqueStarId))
                {
                    List<AdditionalPlanetConfig> configList = planetConfig[uniqueStarId];
                    if (configList.FindIndex(c => c.Index == config.Index) != -1)
                    {
                        throw new Exception($"Duplicate index '{config.Index}' in <{uniqueStarId}>");
                    }
                    configList.Add(config);
                }
                else
                {
                    planetConfig.Add(uniqueStarId, new List<AdditionalPlanetConfig>() { config });
                }

                Plugin.Instance.Logger.LogInfo($"Found new planet at {uniqueStarId}\r\n{config}");
            }
        }

        static private void CreateExampleXmlConfigFile (string filePath)
        {
            StringBuilder content = new StringBuilder();
            content
                .Append("<Config>\r\n")
                .Append("    <Global>\r\n")
                .Append("        <Planets>\r\n")
                .Append("            <Planet>\r\n")
                .Append("                <UniqueStarId>\r\n")
                .Append("                    <ClusterString>The cluster string</ClusterString>\r\n")
                .Append("                    <Star>The star name</Star>\r\n")
                .Append("                </UniqueStarId>\r\n")
                .Append("                <IsBirthPoint>false</IsBirthPoint>\r\n")
                .Append("                <Index>4</Index>\r\n")
                .Append("                <OrbitAround>0</OrbitAround>\r\n")
                .Append("                <OrbitIndex>2</OrbitIndex>\r\n")
                .Append("                <Number>3</Number>\r\n")
                .Append("                <GasGiant>false</GasGiant>\r\n")
                .Append("                <InfoSeed>0</InfoSeed>\r\n")
                .Append("                <GenSeed>0</GenSeed>\r\n")
                .Append("                <ForcePlanetRadius>false</ForcePlanetRadius>\r\n")
                .Append("                <Radius>200</Radius>\r\n")
                .Append("                <OrbitalPeriod>3600</OrbitalPeriod>\r\n")
                .Append("                <RotationPeriod>3600</RotationPeriod>\r\n")
                .Append("                <IsTidalLocked>true</IsTidalLocked>\r\n")
                .Append("                <OrbitInclination>0</OrbitInclination>\r\n")
                .Append("                <Obliquity>0</Obliquity>\r\n")
                .Append("                <DontGenerateVein>true</DontGenerateVein>\r\n")
                .Append("                <ThemeId>0</ThemeId>\r\n")
                .Append("                <OrbitLongitude>0</OrbitLongitude>\r\n")
                .Append("                <VeinCustom>\r\n")
                .Append("                    <Iron>\r\n")
                .Append("                        <VeinGroupCount>\r\n")
                .Append("                            <Type>Accurate</Type>\r\n")
                .Append("                            <AccurateValue>10</AccurateValue>\r\n")
                .Append("                        </VeinGroupCount>\r\n")
                .Append("                        <VeinSpotCount>\r\n")
                .Append("                            <Type>Random</Type>\r\n")
                .Append("                            <RandomBaseValue>100000</RandomBaseValue>\r\n")
                .Append("                            <RandomCoef>1</RandomCoef>\r\n")
                .Append("                            <RandomMulOffset>0</RandomMulOffset>\r\n")
                .Append("                            <RandomAddOffset>5</RandomAddOffset>\r\n")
                .Append("                        </VeinSpotCount>\r\n")
                .Append("                        <VeinAmount>\r\n")
                .Append("                            <Type>Default</Type>\r\n")
                .Append("                        </VeinAmount>\r\n")
                .Append("                    </Iron>\r\n")
                .Append("                </VeinCustom>\r\n")
                .Append("                <ReplaceAllVeinsTo>Copper</ReplaceAllVeinsTo>\r\n")
                .Append("            </Planet>\r\n")
                .Append("        </Planets>\r\n")
                .Append("    </Global>\r\n")
                .Append("    <GameNameSpecific>\r\n")
                .Append("        <Planets>\r\n")
                .Append("            <Planet>\r\n")
                .Append("                <UniqueStarId>\r\n")
                .Append("                    <GameName>The game name of your save file</GameName>\r\n")
                .Append("                    <ClusterString>The cluster string</ClusterString>\r\n")
                .Append("                    <Star>The star name</Star>\r\n")
                .Append("                </UniqueStarId>\r\n")
                .Append("                <IsBirthPoint>false</IsBirthPoint>\r\n")
                .Append("                <Index>4</Index>\r\n")
                .Append("                <OrbitAround>0</OrbitAround>\r\n")
                .Append("                <OrbitIndex>2</OrbitIndex>\r\n")
                .Append("                <Number>3</Number>\r\n")
                .Append("                <GasGiant>false</GasGiant>\r\n")
                .Append("                <InfoSeed>0</InfoSeed>\r\n")
                .Append("                <GenSeed>0</GenSeed>\r\n")
                .Append("                <ForcePlanetRadius>false</ForcePlanetRadius>\r\n")
                .Append("                <Radius>200</Radius>\r\n")
                .Append("                <OrbitalPeriod>3600</OrbitalPeriod>\r\n")
                .Append("                <RotationPeriod>3600</RotationPeriod>\r\n")
                .Append("                <IsTidalLocked>true</IsTidalLocked>\r\n")
                .Append("                <OrbitInclination>0</OrbitInclination>\r\n")
                .Append("                <Obliquity>0</Obliquity>\r\n")
                .Append("                <DontGenerateVein>true</DontGenerateVein>\r\n")
                .Append("                <ThemeId>0</ThemeId>\r\n")
                .Append("                <OrbitLongitude>0</OrbitLongitude>\r\n")
                .Append("                <VeinCustom>\r\n")
                .Append("                    <Iron>\r\n")
                .Append("                        <VeinGroupCount>\r\n")
                .Append("                            <Type>Accurate</Type>\r\n")
                .Append("                            <AccurateValue>10</AccurateValue>\r\n")
                .Append("                        </VeinGroupCount>\r\n")
                .Append("                        <VeinSpotCount>\r\n")
                .Append("                            <Type>Random</Type>\r\n")
                .Append("                            <RandomBaseValue>100000</RandomBaseValue>\r\n")
                .Append("                            <RandomCoef>1</RandomCoef>\r\n")
                .Append("                            <RandomMulOffset>0</RandomMulOffset>\r\n")
                .Append("                            <RandomAddOffset>5</RandomAddOffset>\r\n")
                .Append("                        </VeinSpotCount>\r\n")
                .Append("                        <VeinAmount>\r\n")
                .Append("                            <Type>Default</Type>\r\n")
                .Append("                        </VeinAmount>\r\n")
                .Append("                    </Iron>\r\n")
                .Append("                </VeinCustom>\r\n")
                .Append("                <ReplaceAllVeinsTo>Copper</ReplaceAllVeinsTo>\r\n")
                .Append("            </Planet>\r\n")
                .Append("        </Planets>\r\n")
                .Append("    </GameNameSpecific>\r\n")
                .Append("</Config>\r\n");
            StreamWriter writer = File.CreateText(filePath);
            writer.Write(content);
            writer.Flush();
            writer.Dispose();
        }

        static private void ReadTextConfig (string filePath, Dictionary<string, List<AdditionalPlanetConfig>> planetConfig)
        {
            // 获取配置
            string[] rawConfigArray = File.ReadAllLines(filePath);

            for (int i = 0; i < rawConfigArray.Length; ++i)
            {
                string row = rawConfigArray[i].Trim();

                if (row.IsNullOrWhiteSpace() || row.StartsWith("#") || row.StartsWith("(EXAMPLE)"))
                {
                    continue;
                }

                Dictionary<string, string> configMap = Utility.ParseQueryString(row);

                // 保持对旧 uniqueStarId 的兼容性（旧版本使用 '-' 符号作为分隔符）
                string uniqueStarId = configMap.GetValueSafe("uniqueStarId");
                uniqueStarId = uniqueStarId.Substring(0, uniqueStarId.IndexOf('-')) + // game name
                    '.' +
                    uniqueStarId.Substring(uniqueStarId.IndexOf('-') + 1, uniqueStarId.LastIndexOf('-')) + // cluster string
                    '.' +
                    uniqueStarId.Substring(uniqueStarId.LastIndexOf('-') + 1); // star name
                if (string.IsNullOrWhiteSpace(uniqueStarId))
                {
                    throw new Exception($"Missing parameter 'uniqueStarId'");
                }
                if (!int.TryParse(configMap.GetValueSafe("index"), out int index))
                {
                    throw new Exception($"Missing parameter 'index'");
                }
                if (!int.TryParse(configMap.GetValueSafe("orbitAround"), out int orbitAround))
                {
                    throw new Exception($"Missing parameter 'orbitAround'");
                }
                if (!int.TryParse(configMap.GetValueSafe("orbitIndex"), out int orbitIndex))
                {
                    throw new Exception($"Missing parameter 'orbitIndex'");
                }
                if (!int.TryParse(configMap.GetValueSafe("number"), out int number))
                {
                    throw new Exception($"Missing parameter 'number'");
                }
                if (!bool.TryParse(configMap.GetValueSafe("gasGiant"), out bool gasGiant))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'gasGiant', pick default value: {DEFAULT_GAS_GIANT}");
                    gasGiant = DEFAULT_GAS_GIANT;
                }
                if (!int.TryParse(configMap.GetValueSafe("info_seed"), out int infoSeed))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'info_seed', pick default value: {DEFAULT_INFO_SEED}");
                    infoSeed = DEFAULT_INFO_SEED;
                }
                if (!int.TryParse(configMap.GetValueSafe("gen_seed"), out int genSeed))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'gen_seed', pick default value: {DEFAULT_GEN_SEED}");
                    genSeed = DEFAULT_GEN_SEED;
                }
                if (!bool.TryParse(configMap.GetValueSafe("forcePlanetRadius"), out bool forcePlanetRadius))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'forcePlanetRadius', pick default value: {DEFAULT_FORCE_PLANET_RADIUS}");
                    forcePlanetRadius = DEFAULT_FORCE_PLANET_RADIUS;
                }
                if (!float.TryParse(configMap.GetValueSafe("planetRadius"), out float planetRadius))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'planetRadius', pick default value: {DEFAULT_RADIUS}");
                    planetRadius = DEFAULT_RADIUS;
                }
                if (planetRadius > MAX_RADIUS)
                {
                    if (!forcePlanetRadius)
                    {
                        throw new Exception($"Current max planet radius is {MAX_RADIUS}, use 'forcePlanetRadius=true' to override this");
                    }
                    else
                    {
                        Plugin.Instance.Logger.LogWarning($"Force planet radius: {planetRadius}");
                    }
                }
                if (!float.TryParse(configMap.GetValueSafe("orbitalPeriod"), out float orbitalPeriod))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'orbitalPeriod', pick default value: {DEFAULT_ORBITAL_PERIOD}");
                    orbitalPeriod = DEFAULT_ORBITAL_PERIOD;
                }
                if (!float.TryParse(configMap.GetValueSafe("rotationPeriod"), out float rotationPeriod))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'rotationPeriod', pick default value: {DEFAULT_ROTATION_PERIOD}");
                    rotationPeriod = DEFAULT_ROTATION_PERIOD;
                }
                if (!bool.TryParse(configMap.GetValueSafe("isTidalLocked"), out bool isTidalLocked))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'isTidalLocked', pick default value: {DEFAULT_IS_TIDAL_LOCKED}");
                    isTidalLocked = DEFAULT_IS_TIDAL_LOCKED;
                }
                if (!float.TryParse(configMap.GetValueSafe("orbitInclination"), out float orbitInclination))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'orbitInclination', pick default value: {DEFAULT_ORBIT_INCLINATION}");
                    orbitInclination = DEFAULT_ORBIT_INCLINATION;
                }
                if (!float.TryParse(configMap.GetValueSafe("obliquity"), out float obliquity))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'obliquity', pick default value: {DEFAULT_OBLIQUITY}");
                    obliquity = DEFAULT_ORBIT_INCLINATION;
                }
                if (!bool.TryParse(configMap.GetValueSafe("dontGenerateVein"), out bool dontGenerateVein))
                {
                    Plugin.Instance.Logger.LogInfo($"Missing parameter 'dontGenerateVein', pick default value: {DEFAULT_DONT_GENERATE_VEIN}");
                    dontGenerateVein = DEFAULT_DONT_GENERATE_VEIN;
                }

                int theme = default;
                bool hasTheme = false;
                if (configMap.ContainsKey("theme"))
                {
                    if (!int.TryParse(configMap.GetValueSafe("theme"), out theme))
                    {
                        throw new Exception($"Invalid parameter 'theme', this parameter must be a positive integer");
                    }
                    else if (!LDB.themes.Exist(theme))
                    {
                        throw new Exception($"Fail to find theme #{theme} in game data");
                    }
                    else if (LDB.themes.Select(theme) == null)
                    {
                        throw new Exception($"Theme exists but is null, theme id: {theme}");
                    }
                    hasTheme = true;
                }

                float orbitLongitude = 0;
                bool hasOrbitLongitude = false;
                if (configMap.ContainsKey("orbitLongitude"))
                {
                    string orbitLongitudeString = configMap["orbitLongitude"];
                    if (string.IsNullOrWhiteSpace(orbitLongitudeString))
                    {
                        throw new Exception($"Parameter 'orbitLongitude' has invalid value, correct format is 'DEGREE,MINUTE', e.g. '60,60'");
                    }

                    string[] orbitLongitudeStrings = orbitLongitudeString.Split(',');
                    if (orbitLongitudeStrings.Length != 2)
                    {
                        throw new Exception($"Parameter 'orbitLongitude' has invalid value, correct format is 'DEGREE,MINUTE', e.g. '60,60'");
                    }

                    if (!float.TryParse(orbitLongitudeStrings[0].Trim(), out float orbitLongitudeDegree))
                    {
                        throw new Exception($"Parameter 'orbitLongitude' has invalid value, correct format is 'DEGREE,MINUTE', e.g. '60,60'");
                    }

                    if (!float.TryParse(orbitLongitudeStrings[1].Trim(), out float orbitLongitudeMinute))
                    {
                        throw new Exception($"Parameter 'orbitLongitude' has invalid value, correct format is 'DEGREE,MINUTE', e.g. '60,60'");
                    }

                    orbitLongitude = orbitLongitudeDegree + orbitLongitudeMinute / 60f;
                    if (orbitLongitude >= 360f)
                    {
                        orbitLongitude = Mathf.Repeat(orbitLongitude, 360f);
                    }
                    else if (orbitLongitude < 0f)
                    {
                        throw new Exception($"Parameter 'orbitLongitude' must be positive");
                    }
                    hasOrbitLongitude = true;
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
                config._HasThemeId = hasTheme;
                config.ThemeId = theme;
                config._HasOrbitLongitude = hasOrbitLongitude;
                config.OrbitLongitude = orbitLongitude;
                config._HasVeinCustom = false;
                config.VeinCustom = null;
                config._HasReplaceAllVeinsTo = false;
                config.ReplaceAllVeinsTo = EVeinType.None;

                if (!planetConfig.ContainsKey(uniqueStarId))
                {
                    planetConfig[uniqueStarId] = new List<AdditionalPlanetConfig>();
                }

                planetConfig[uniqueStarId].Add(config);

                Plugin.Instance.Logger.LogInfo($"Found new planet at {uniqueStarId}\r\n{config}");
            }
        }

        static private string ReadStringNode (XmlNode parent, string childName, bool required, string defaultValue)
        {
            XmlNode childNode = parent.SelectSingleNode(childName);
            if (childNode == null)
            {
                if (required)
                {
                    throw new Exception($"Missing parameter '{childName}'");
                }
                else
                {
                    return defaultValue;
                }
            }
            return childNode.InnerText.Trim();
        }

        static private int ReadIntNode (XmlNode parent, string childName, bool required, int defaultValue, int min = int.MinValue, int max = int.MaxValue)
        {
            XmlNode childNode = parent.SelectSingleNode(childName);
            if (childNode == null)
            {
                if (required)
                {
                    throw new Exception($"Missing parameter '{childName}'");
                }
                else
                {
                    return defaultValue;
                }
            }
            if (!int.TryParse(childNode.InnerText.Trim(), out int result))
            {
                throw new Exception($"Invalid parameter '{childName}', '{childName}' must be an integer, current value: {childNode.InnerText}");
            }
            if (result < min || result > max)
            {
                throw new Exception($"Invalid parameter '{childName}', '{childName}' must be in range [{min}, {max}], current value: {childNode.InnerText}");
            }
            return result;
        }

        static private int ReadIntNode (XmlNode parent, string childName, bool required, int defaultValue, IEnumerable<int> set)
        {
            XmlNode childNode = parent.SelectSingleNode(childName);
            if (childNode == null)
            {
                if (required)
                {
                    throw new Exception($"Missing parameter '{childName}'");
                }
                else
                {
                    return defaultValue;
                }
            }
            if (!int.TryParse(childNode.InnerText.Trim(), out int result))
            {
                throw new Exception($"Invalid parameter '{childName}', '{childName}' must be an integer, current value: {childNode.InnerText}");
            }
            if (!set.Contains(result))
            {
                throw new Exception($"Invalid parameter '{childName}', '{childName}' must be in set {{{set.Join()}}}, current value: {childNode.InnerText}");
            }
            return result;
        }

        static private float ReadFloatNode (XmlNode parent, string childName, bool required, float defaultValue, float min = float.MinValue, float max = float.MaxValue)
        {
            XmlNode childNode = parent.SelectSingleNode(childName);
            if (childNode == null)
            {
                if (required)
                {
                    throw new Exception($"Missing parameter '{childName}'");
                }
                else
                {
                    return defaultValue;
                }
            }
            if (!float.TryParse(childNode.InnerText.Trim(), out float result))
            {
                throw new Exception($"Invalid parameter '{childName}', '{childName}' must be an integer, current value: {childNode.InnerText}");
            }
            if (result < min || result > max)
            {
                throw new Exception($"Invalid parameter '{childName}', '{childName}' must be in range [{min}, {max}], current value: {childNode.InnerText}");
            }
            return result;
        }

        static private bool ReadBoolNode (XmlNode parent, string childName, bool required, bool defaultValue)
        {
            XmlNode childNode = parent.SelectSingleNode(childName);
            if (childNode == null)
            {
                if (required)
                {
                    throw new Exception($"Missing parameter '{childName}'");
                }
                else
                {
                    return defaultValue;
                }
            }
            if (!bool.TryParse(childNode.InnerText.Trim(), out bool result))
            {
                throw new Exception($"Invalid parameter '{childName}', '{childName}' must be 'true' or 'false', current value: {childNode.InnerText}");
            }
            return result;
        }

        static private T ReadEnumNode<T> (XmlNode parent, string childName, bool required, T defaultValue) where T : Enum
        {
            XmlNode childNode = parent.SelectSingleNode(childName);
            if (childNode == null)
            {
                if (required)
                {
                    throw new Exception($"Missing parameter '{childName}'");
                }
                else
                {
                    return defaultValue;
                }
            }
            try
            {
                return (T)Enum.Parse(typeof(T), childNode.InnerText.Trim());
            }
            catch (Exception)
            {
                throw new Exception($"Invalid parameter '{childName}', '{childName}' must be in set {{{Utility.EnumValuesJoin<T>()}}}, current value: {childNode.InnerText}");
            }
        }

        static AdditionalPlanetConfig.VeinConfig.CustomValue ReadVeinCustomValue (XmlNode parent, int defaultRandomBaseValue, float defaultRandomCoef, float defaultRandomMulOffset, int defaultRandomAddOffset)
        {
            XmlNode nodeType = parent.SelectSingleNode("Type");
            if (nodeType == null)
            {
                throw new Exception("Missing parameter 'Type'");
            }

            if (!Enum.TryParse(nodeType.InnerText.Trim(), out AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType type))
            {
                throw new Exception($"'Type' must be in set {{{Utility.EnumValuesJoin<AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType>()}}}");
            }

            AdditionalPlanetConfig.VeinConfig.CustomValue result = default;

            if (type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Default)
            {
                result.Type = type;
                return result;
            }

            if (type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Random)
            {
                // (optional) random base value
                int randomBaseValue = ReadIntNode(parent, "RandomBaseValue", false, defaultRandomBaseValue, 0);

                // (optional) random coef
                float randomCoef = ReadFloatNode(parent, "RandomCoef", false, defaultRandomCoef, 0);

                // (optional) random mul offset
                float randomMulOffset = ReadFloatNode(parent, "RandomMulOffset", false, defaultRandomMulOffset, 0);

                // (optional) random add offset
                int randomAddOffset = ReadIntNode(parent, "RandomAddOffset", false, defaultRandomAddOffset, 0);

                result.Type = type;
                result.RandomBaseValue = randomBaseValue;
                result.RandomCoef = randomCoef;
                result.RandomMulOffset = randomMulOffset;
                result.RandomAddOffset = randomAddOffset;
                return result;
            }

            if (type == AdditionalPlanetConfig.VeinConfig.CustomValue.CustomType.Accurate)
            {
                // (required) accurate value
                int accurateValue = ReadIntNode(parent, "AccurateValue", true, default, 0);

                result.Type = type;
                result.AccurateValue = accurateValue;
                return result;
            }

            throw new Exception("Unrecognizable CustomType: " + type);
        }

        static string ReadUniqueStarId (XmlNode parent, bool isGameNameRequired)
        {
            XmlNode nodeUniqueStarId = parent.SelectSingleNode("UniqueStarId");
            if (nodeUniqueStarId == null)
            {
                throw new Exception("Missing paremeter 'UniqueStarId'");
            }

            string clusterString = ReadStringNode(nodeUniqueStarId, "ClusterString", true, null).Trim();
            string star = ReadStringNode(nodeUniqueStarId, "Star", true, null).Trim();
            if (string.IsNullOrWhiteSpace(clusterString))
            {
                throw new Exception("Parameter 'ClusterString' can not be empty");
            }
            if (string.IsNullOrWhiteSpace(star))
            {
                throw new Exception("Parameter 'Star' can not be empty");
            }
            if (!isGameNameRequired)
            {
                // 如果不需要 GameName 的话，到这里就可以返回结果了
                return Utility.UniqueStarIdWithoutGameName(clusterString, star);
            }

            // 否则，获取 GameName 并返回带有 GameName 的结果
            string gameName = ReadStringNode(nodeUniqueStarId, "GameName", true, null).Trim();
            if (string.IsNullOrWhiteSpace(gameName))
            {
                throw new Exception("Parameter 'GameName' can not be empty when required");
            }
            return Utility.UniqueStarIdWithGameName(gameName, clusterString, star);
        }
    }
}
