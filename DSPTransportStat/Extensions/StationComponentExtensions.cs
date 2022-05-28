using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPTransportStat.Extensions
{
    static class StationComponentExtensions
    {
        /// <summary>
        /// 获取站点名称，对已经设置名称的站点，直接返回名称，对没有设置名称的站点，返回使用ID拼成的名称
        /// </summary>
        /// <param name="stationComponent"></param>
        /// <returns></returns>
        static public string GetStationName (this StationComponent stationComponent)
        {
            if (string.IsNullOrEmpty(stationComponent.name))
            {
                if (stationComponent.isStellar)
                {
                    return "星际站点号".Translate() + stationComponent.gid.ToString();
                }
                else
                {
                    return "本地站点号".Translate() + stationComponent.id.ToString();
                }
            }
            else
            {
                return stationComponent.name;
            }
        }
    }
}
