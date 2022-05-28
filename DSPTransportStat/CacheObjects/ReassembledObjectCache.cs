using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DSPTransportStat.Extensions;

namespace DSPTransportStat.CacheObjects
{
    /// <summary>
    /// 存储一些通过游戏原生对象重新组装而成的对象
    /// </summary>
    static class ReassembledObjectCache
    {
        /// <summary>
        /// 圆形的物品过滤按钮，具有显示物品图标、清空所选物品的功能
        /// </summary>
        static public GameObject GOCircularItemFilterButton { get; set; } = null;

        /// <summary>
        /// 文本按钮
        /// </summary>
        static public GameObject GOTextButton { get; set; } = null;

        /// <summary>
        /// 输入框
        /// </summary>
        static public GameObject GOInputField { get; set; } = null;

        static private GameObject root = null;

        static public void InitializeReassembledObjectCache ()
        {
            // 创建用于容纳所有重组对象的根GameObject
            root = new GameObject("DSPTransportStat_ReassembledObjectCache");

            // 创建圆形的物品过滤按钮
            GOCircularItemFilterButton = UnityEngine.Object.Instantiate(NativeObjectCache.StorageIconEmpty, root.transform);
            GOCircularItemFilterButton.name = "circular-item-filter";

            GameObject GOCircularItemFilterButton_childBg = GOCircularItemFilterButton.transform.Find("white").gameObject;
            GOCircularItemFilterButton_childBg.name = "bg";

            // 重新调整 bg (white) 对象的大小
            RectTransform GOCircularItemFilterButton_childBg_cmpRectTransform = GOCircularItemFilterButton_childBg.GetComponent<RectTransform>();
            GOCircularItemFilterButton_childBg_cmpRectTransform.Zeroize();
            GOCircularItemFilterButton_childBg_cmpRectTransform.anchorMin = new Vector2(0, 0);
            GOCircularItemFilterButton_childBg_cmpRectTransform.anchorMax = new Vector2(1, 1);

            // 调整 UIButton 组件对 bg (white) 对象中 Image 组件的渐变颜色控制
            UIButton GOCircularItemFilterButton_cmpUIButton = GOCircularItemFilterButton.GetComponent<UIButton>();
            UIButton.Transition GOCircularItemFilterButton_cmpUIButton_transitions0 = GOCircularItemFilterButton_cmpUIButton.transitions[0];
            GOCircularItemFilterButton_cmpUIButton_transitions0.damp = 0.5f;
            GOCircularItemFilterButton_cmpUIButton_transitions0.disabledColor = new Color(0.4434f, 0.4434f, 0.4434f, 0.3f);
            GOCircularItemFilterButton_cmpUIButton_transitions0.mouseoverColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            GOCircularItemFilterButton_cmpUIButton_transitions0.normalColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            GOCircularItemFilterButton_cmpUIButton_transitions0.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            // 创建文本按钮
            GOTextButton = UnityEngine.Object.Instantiate(NativeObjectCache.ClearAchievementDataBtn, root.transform);
            GOTextButton.name = "text-button";
            GOTextButton.SetActive(true);

            // 删除 tips
            UIButton GOTextButton_cmpUIButton = GOTextButton.GetComponent<UIButton>();
            GOTextButton_cmpUIButton.tips.corner = 0;
            GOTextButton_cmpUIButton.tips.tipText = "";
            GOTextButton_cmpUIButton.tips.tipTitle = "";

            GameObject GOTextButton_childButtonText = GOTextButton.transform.Find("button-text").gameObject;
            UnityEngine.Object.DestroyImmediate(GOTextButton_childButtonText.GetComponent<Localizer>());
            GOTextButton_childButtonText.GetComponent<Text>().text = "TEXT-BUTTON";

            // 删除 loading
            UnityEngine.Object.DestroyImmediate(GOTextButton.transform.Find("loading").gameObject);

            // 输入框
            GOInputField = UnityEngine.Object.Instantiate(NativeObjectCache.NameInput, root.transform);
            GOInputField.name = "input-field";

            InputField GOInputField_cmpInputField = GOInputField.GetComponent<InputField>();
            GOInputField_cmpInputField.onValueChanged.RemoveAllListeners();
            GOInputField_cmpInputField.onEndEdit.RemoveAllListeners();
            GOInputField_cmpInputField.text = "";
        }
    }
}
