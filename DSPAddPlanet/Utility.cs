using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPAddPlanet
{
    static class Utility
    {
        static public Dictionary<string, string> ParseQueryString (string queryString)
        {
            Dictionary<string, string> parameterMap = new Dictionary<string, string>();
            string[] splittedQueryString = queryString.Split('&');
            foreach (string s in splittedQueryString)
            {
                string[] pair = s.Split('=');
                parameterMap[pair[0].Trim()] = pair[1].Trim();
            }
            return parameterMap;
        }

        static public string UniqueStarIdWithGameName (string gameName, string clusterString, string starName)
        {
            return gameName + '.' + clusterString + '.' + starName;
        }

        static public string UniqueStarIdWithoutGameName (string clusterString, string starName)
        {
            return clusterString + '.' + starName;
        }

        static public void PrintThemeTable ()
        {
            //string title = "| ID | name | planet type | temperature | gas items | gas speeds | wind | ion height | water height | water item | culling radius | ice flag |\r\n" +
            //              "| --- | ---- | ----------- | ----------- | --------- | ---------- | ---- | ---------- | ------------ | ---------- | -------------- | -------- |\r\n";
            string title = "| ID | 名称 | 行星类型 | 温度 | 气体种类 | 产气速度 | 风 | ion height | 海面高度 | 海洋类型 | culling radius | ice flag |\r\n" +
                          "| --- | ---- | ----------- | ----------- | --------- | ---------- | ---- | ---------- | ------------ | ---------- | -------------- | -------- |\r\n";

            StringBuilder table = new StringBuilder(title);

            List<ThemeProto> themeProtos = LDB.themes.dataArray.ToList();
            themeProtos.Sort((a, b) => a.ID - b.ID);

            foreach (ThemeProto theme in themeProtos)
            {
                StringBuilder gasItems = new StringBuilder();
                if (theme.GasItems.Length > 0)
                {
                    for (int i = 0; i < theme.GasItems.Length; ++i)
                    {
                        ItemProto itemProto = LDB.items.Select(theme.GasItems[i]);

                        gasItems.Append(itemProto.Name.Translate());
                        //gasItems.Append(itemProto.Name);

                        if (i < theme.GasItems.Length - 1)
                        {
                            gasItems.Append(", ");
                        }
                    }
                }

                StringBuilder gasSpeeds = new StringBuilder();
                if (theme.GasSpeeds.Length > 0)
                {
                    for (int i = 0; i < theme.GasSpeeds.Length; ++i)
                    {
                        gasSpeeds.Append(theme.GasSpeeds[i]);
                        if (i < theme.GasSpeeds.Length - 1)
                        {
                            gasSpeeds.Append(", ");
                        }
                    }
                }

                ItemProto waterItemProto = LDB.items.Select(theme.WaterItemId);

                string waterItem = waterItemProto == null ? "" : waterItemProto.Name.Translate();
                //string waterItem = waterItemProto == null ? "" : waterItemProto.Name;

                string name = theme.DisplayName.Translate();
                //string name = theme.DisplayName;

                table.Append($"| {theme.ID} | {name} | {theme.PlanetType} | {theme.Temperature} | {gasItems} | {gasSpeeds} | {theme.Wind} | {theme.IonHeight} | {theme.WaterHeight} | {waterItem} | {theme.CullingRadius} | {theme.IceFlag} |\r\n");
            }

            Plugin.Instance.Logger.LogInfo("\r\n" + table);
        }

        static public string EnumValuesJoin<T> () where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            List<string> strings = new List<string>();
            foreach (object v in values)
            {
                strings.Add(v.ToString());
            }
            return strings.Join();
        }

        /// <summary>
        /// 根据当前的游戏名称、clusterString和恒星名称从配置列表中获取行星配置信息，如果未找到配置信息则返回null
        /// </summary>
        /// <param name="gameName">当前的游戏名称</param>
        /// <param name="clusterString"></param>
        /// <param name="starName"></param>
        /// <param name="globalConfig"></param>
        /// <param name="gameNameSpecificConfig"></param>
        /// <param name="uniqueStarId"></param>
        /// <returns></returns>
        static public List<AdditionalPlanetConfig> GetPlanetConfigList (
            string gameName,
            string clusterString,
            string starName,
            Dictionary<string, List<AdditionalPlanetConfig>> globalConfig,
            Dictionary<string, List<AdditionalPlanetConfig>> gameNameSpecificConfig,
            out string uniqueStarId
        )
        {
            if (globalConfig.Count == 0 && gameNameSpecificConfig.Count == 0)
            {
                uniqueStarId = null;
                return null;
            }

            if (string.IsNullOrWhiteSpace(gameName))
            {
                // 如果当前游戏没有名称，则尝试获取全局行星配置
                uniqueStarId = UniqueStarIdWithoutGameName(clusterString, starName);
                if (globalConfig.ContainsKey(uniqueStarId))
                {
                    return globalConfig[uniqueStarId];
                }
                else
                {
                    // 没有游戏名称，且全局配置列表中没有该恒星的配置
                    return null;
                }
            }
            else
            {
                // 游戏名称不为空，则先尝试获取针对特定游戏名称的行星配置，再尝试获取全局行星配置
                uniqueStarId = UniqueStarIdWithGameName(gameName, clusterString, starName);
                if (gameNameSpecificConfig.ContainsKey(uniqueStarId))
                {
                    return gameNameSpecificConfig[uniqueStarId];
                }
                else
                {
                    uniqueStarId = UniqueStarIdWithoutGameName(clusterString, starName);
                    if (globalConfig.ContainsKey(uniqueStarId))
                    {
                        return globalConfig[uniqueStarId];
                    }
                    else
                    {
                        // 有游戏名称，但是针对特定游戏名称的行星配置列表和全局行星配置列表中都没有该恒星的配置
                        return null;
                    }
                }
            }
        }
    }
}
