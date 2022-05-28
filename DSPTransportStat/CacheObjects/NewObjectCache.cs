using DSPTransportStat.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DSPTransportStat.CacheObjects
{
    /// <summary>
    /// 存储一些全新的对象
    /// </summary>
    static class NewObjectCache
    {
        /// <summary>
        /// 一个只包含 Text 和 RectTransform 组件的游戏对象，其中 Text 组件除了少数属性外，其余属性都采用默认值
        /// </summary>
        static public GameObject SimpleText;

        static public void InitializeNewObjectCache ()
        {
            // 创建用于容纳所有重组对象的根GameObject
            GameObject root = new GameObject("DSPTransportStat_NewObjectCache");

            SimpleText = new GameObject("simple-text", typeof(RectTransform), typeof(Text));
            SimpleText.transform.SetParent(root.transform);

            SimpleText.GetComponent<RectTransform>().Zeroize();

            Text SimpleText_cmpText = SimpleText.GetComponent<Text>();
            SimpleText_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            SimpleText_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            SimpleText_cmpText.font = ResourceCache.FontSAIRASB;
            SimpleText_cmpText.text = "";
        }
    }
}
