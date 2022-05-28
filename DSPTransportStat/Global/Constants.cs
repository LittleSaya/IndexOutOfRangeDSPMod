using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPTransportStat.Global
{
    static public class Constants
    {
        /// <summary>
        /// 实际存在于场景中，用于显示的物流运输站条目数量
        /// </summary>
        public const int DISPLAYED_TRANSPORT_STATIONS_ENTRY_COUNT = 10;

        /// <summary>
        /// 每一个物流运输条目的高度
        /// </summary>
        public const int TRANSPORT_STATIONS_ENTRY_HEIGHT = 120;

        /// <summary>
        /// 表示没有物品的物品ID
        /// </summary>
        public const int NONE_ITEM_ID = int.MinValue;

        /// <summary>
        /// 默认情况下，每一个站点列表项中有多少个物品槽位
        /// </summary>
        public const int TRANSPORT_STATIONS_ENTRY_DEFAULT_ITEM_SLOT_NUMBER = 5;

        /// <summary>
        /// 站点列表项中每一个槽位的宽度
        /// </summary>
        public const int TRANSPORT_STATIONS_ENTRY_ITEM_SLOT_WIDTH = 230;
    }
}
