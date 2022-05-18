using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    static public class UIUtility
    {
        /// <summary>
        /// 创建文本框
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="font"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="anchorMin"></param>
        /// <param name="anchorMax"></param>
        /// <param name="offsetMin"></param>
        /// <param name="offsetMax"></param>
        /// <returns></returns>
        static public GameObject CreateText (string initialValue, Font font, string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.Zeroize();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Text go_cmpText = go.GetComponent<Text>();
            go_cmpText.horizontalOverflow = HorizontalWrapMode.Overflow;
            go_cmpText.verticalOverflow = VerticalWrapMode.Overflow;
            go_cmpText.font = font;
            go_cmpText.text = initialValue;

            return go;
        }
    }
}
