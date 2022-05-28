using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPTransportStat.Translation
{
    static class Strings
    {
        static public Language Language { get => language; }

        static private Language language = Language.enUS;

        static public void InitializeTranslations (Language lang)
        {
            language = lang;
        }

        static public class TransportStationsWindow
        {
            static public string Title
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Transport Stations";
                        case Language.zhCN: return "物流运输站";
                        default: return "Transport Stations";
                    };
                }
            }

            static public string CurrentLabel
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Current:";
                        case Language.zhCN: return "当前：";
                        default: return "Current:";
                    }
                }
            }

            static public string MaxLabel
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Max:";
                        case Language.zhCN: return "最大：";
                        default: return "Max:";
                    }
                }
            }

            static public string LocationAndName
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Location & Name";
                        case Language.zhCN: return "位置和名称";
                        default: return "Location & Name";
                    }
                }
            }

            static public string ASC
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "ASC";
                        case Language.zhCN: return "升序";
                        default: return "ASC";
                    }
                }
            }

            static public string DESC
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "DESC";
                        case Language.zhCN: return "降序";
                        default: return "DESC";
                    }
                }
            }

            static public string ItemSlots
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Item Slots";
                        case Language.zhCN: return "物品槽位";
                        default: return "Item Slots";
                    }
                }
            }

            static public string SearchLabel
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Search:";
                        case Language.zhCN: return "搜索：";
                        default: return "Search:";
                    }
                }
            }

            static public string SearchButton
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Search";
                        case Language.zhCN: return "搜索";
                        default: return "Search";
                    }
                }
            }

            static public string ItemConfigPanel
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Config Panel";
                        case Language.zhCN: return "配置面板";
                        default: return "Config Panel";
                    }
                }
            }

            static public string ItemHomePanel
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Home Panel";
                        case Language.zhCN: return "主页";
                        default: return "Home Panel";
                    }
                }
            }

            static public string AllowItemTransferLabel
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "Allow Item Transfer";
                        case Language.zhCN: return "允许存取物品";
                        default: return "Allow Item Transfer";
                    }
                }
            }

            static public class ParameterPanel
            {
                static public string ToggleInPlanetLabel
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Planetary Station";
                            case Language.zhCN: return "行星物流站";
                            default: return "Planetary Station";
                        }
                    }
                }

                static public string ToggleInterstellarLabel
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Interstellar Station";
                            case Language.zhCN: return "星际物流站";
                            default: return "Interstellar Station";
                        }
                    }
                }

                static public string ToggleCollectorLabel
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Collector";
                            case Language.zhCN: return "采集站";
                            default: return "Collector";
                        }
                    }
                }

                static public string ItemFilterLabel
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Item Filter";
                            case Language.zhCN: return "过滤物品";
                            default: return "Item Filter";
                        }
                    }
                }
            }
        }

        static public class Common
        {
            static public string NoOrder
            {
                get
                {
                    switch (language)
                    {
                        case Language.enUS: return "No Order";
                        case Language.zhCN: return "无订单";
                        default: return "No Order";
                    }
                }
            }
            static public class StationType
            {
                static public string InPlanet
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Planetary Station";
                            case Language.zhCN: return "行星物流站";
                            default: return "Planetary Station";
                        }
                    }
                }

                static public string InPlanetCollector
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Planetary Collector";
                            case Language.zhCN: return "行星采集站";
                            default: return "Planetary Collector";
                        }
                    }
                }

                static public string Interstrllar
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Interstellar Station";
                            case Language.zhCN: return "星际物流站";
                            default: return "Interstellar Station";
                        }
                    }
                }

                static public string InterstellarCollector
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Orbital Collector";
                            case Language.zhCN: return "轨道采集站";
                            default: return "Orbital Collector";
                        }
                    }
                }
            }

            static public class StationStoreLogic
            {
                static public string InPlanetStorage
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Local Storage";
                            case Language.zhCN: return "本地仓储";
                            default: return "Local Storage";
                        }
                    }
                }

                static public string InPlanetSupply
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Local Supply";
                            case Language.zhCN: return "本地供应";
                            default: return "Local Supply";
                        }
                    }
                }

                static public string InPlanetDemand
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Local Demand";
                            case Language.zhCN: return "本地需求";
                            default: return "Local Demand";
                        }
                    }
                }
                static public string InterstellarStorage
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Remote Storage";
                            case Language.zhCN: return "星际仓储";
                            default: return "Remote Storage";
                        }
                    }
                }

                static public string InterstellarSupply
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Remote Supply";
                            case Language.zhCN: return "星际供应";
                            default: return "Remote Supply";
                        }
                    }
                }

                static public string InterstellarDemand
                {
                    get
                    {
                        switch (language)
                        {
                            case Language.enUS: return "Remote Demand";
                            case Language.zhCN: return "星际需求";
                            default: return "Remote Demand";
                        }
                    }
                }
            }
        }
    }
}
