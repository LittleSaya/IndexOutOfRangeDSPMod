using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DSPAddPlanet
{
    static class RectTransformExtensions
    {
        /// <summary>
        /// 将该 RectTransform 的所有属性都初始化为零向量
        /// </summary>
        /// <param name="rectTransform"></param>
        static public void Zeroize (this RectTransform rectTransform)
        {
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.offsetMax = Vector3.zero;
            rectTransform.offsetMin = Vector3.zero;
            rectTransform.pivot = Vector3.zero;
            rectTransform.sizeDelta = Vector3.zero;
            rectTransform.anchorMax = Vector3.zero;
            rectTransform.anchorMin = Vector3.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}
