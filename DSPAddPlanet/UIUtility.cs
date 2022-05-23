using DSPAddPlanet.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DSPAddPlanet
{
    static class UIUtility
    {
        /// <summary>
        /// 创建文本按钮
        /// </summary>
        /// <param name="text"></param>
        /// <param name="callback"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="anchorMin"></param>
        /// <param name="anchorMax"></param>
        /// <param name="offsetMin"></param>
        /// <param name="offsetMax"></param>
        /// <returns></returns>
        static public GameObject CreateTextButton (string text, Action callback, string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            // 克隆设置窗口的“应用设置”按钮
            Transform goApplyButton_cmpTransform = UIRoot.instance.transform.Find("Overlay Canvas/Top Windows/Option Window/apply-button");

            GameObject goNewButton = UnityEngine.Object.Instantiate(goApplyButton_cmpTransform.gameObject, parent);
            goNewButton.name = name;

            RectTransform goNewButton_cmpRectTransform = goNewButton.GetComponent<RectTransform>();
            goNewButton_cmpRectTransform.Zeroize();
            goNewButton_cmpRectTransform.anchorMin = anchorMin;
            goNewButton_cmpRectTransform.anchorMax = anchorMax;
            goNewButton_cmpRectTransform.offsetMin = offsetMin;
            goNewButton_cmpRectTransform.offsetMax = offsetMax;

            Button goNewButton_childButton = goNewButton.GetComponent<Button>();
            goNewButton_childButton.onClick.RemoveAllListeners();
            goNewButton_childButton.onClick.AddListener(() => callback.Invoke());

            GameObject goNewButton_childButtonText = goNewButton.transform.Find("button-text").gameObject;
            
            UnityEngine.Object.Destroy(goNewButton_childButtonText.GetComponent<Localizer>());

            Text goNewButton_childButtonText_cmpText = goNewButton_childButtonText.GetComponent<Text>();
            goNewButton_childButtonText_cmpText.text = text;

            return goNewButton;
        }

        /// <summary>
        /// 创建文本框
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="anchorMin"></param>
        /// <param name="anchorMax"></param>
        /// <param name="offsetMin"></param>
        /// <param name="offsetMax"></param>
        /// <returns></returns>
        static public GameObject CreateText (string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
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
            go_cmpText.font = ResourceCache.FontSAIRASB;
            go_cmpText.text = "";

            return go;
        }
    }
}
