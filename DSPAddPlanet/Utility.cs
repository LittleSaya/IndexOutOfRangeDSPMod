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
    }
}
