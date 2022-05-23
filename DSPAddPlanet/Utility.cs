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

        static public string UniqueStarId (string gameName, string clusterString, string starName)
        {
            return gameName + '-' + clusterString + '-' + starName;
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
                        //gasItems.Append(itemProto.Name.Translate());
                        gasItems.Append(itemProto.Name);
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
                //string waterItem = waterItemProto == null ? "" : waterItemProto.Name.Translate();
                string waterItem = waterItemProto == null ? "" : waterItemProto.Name;

                //string name = theme.DisplayName.Translate();
                string name = theme.DisplayName;

                table.Append($"| {theme.ID} | {name} | {theme.PlanetType} | {theme.Temperature} | {gasItems} | {gasSpeeds} | {theme.Wind} | {theme.IonHeight} | {theme.WaterHeight} | {waterItem} | {theme.CullingRadius} | {theme.IceFlag} |\r\n");
            }

            Plugin.Instance.Logger.LogInfo("\r\n" + table);
        }
    }
}
