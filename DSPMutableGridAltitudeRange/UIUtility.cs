using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Common;
using UnityEngine.Events;
using UnityEngine.UI;
using DSPMutableGridAltitudeRange.Cache;

namespace DSPMutableGridAltitudeRange
{
    internal static class UIUtility
    {
        static public GameObject CreateImageButton (Sprite sprite, UnityAction callback, string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = UnityEngine.Object.Instantiate(NativeObjectCache.CloseBtn);
            obj.transform.SetParent(parent);
            obj.name = name;

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.Zeroize();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            // 修改 x 所使用的图片和大小
            Transform x = obj.transform.Find("x");

            Image x_cmpImage = x.GetComponent<Image>();

            x_cmpImage.sprite = sprite;

            RectTransform x_cmpRectTransform = x.GetComponent<RectTransform>();
            x_cmpRectTransform.Zeroize();
            x_cmpRectTransform.anchorMax = new Vector2(1, 1);

            // 修改 col 的大小
            Transform col = obj.transform.Find("col");

            RectTransform col_cmpRectTransform = col.GetComponent<RectTransform>();
            col_cmpRectTransform.Zeroize();
            col_cmpRectTransform.anchorMax = new Vector2(1, 1);

            // 调整按钮原先自带的渐变效果
            Image cmpImage = obj.GetComponent<Image>();
            UIButton cmpUIButton = obj.GetComponent<UIButton>();
            foreach (UIButton.Transition t in cmpUIButton.transitions)
            {
                if (t.target.GetInstanceID() == cmpImage.GetInstanceID())
                {
                    // 背景图片，将所有的渐变颜色都修改为完全透明，默认情况下没有背景的渐变色
                    t.disabledColor = Color.clear;
                    t.highlightColorOverride = Color.clear;
                    t.mouseoverColor = Color.clear;
                    t.normalColor = Color.clear;
                    t.pressedColor = Color.clear;
                }
                else if (t.target.GetInstanceID() == x_cmpImage.GetInstanceID())
                {
                    // 按钮图片，略微调低亮度
                    t.disabledColor = new Color(0, 0, 0, 0.2157f);
                    t.highlightColorOverride = new Color(0, 0, 0, 0);
                    t.mouseoverColor = new Color(0.6557f, 0.9145f, 1, 0.5f);
                    t.normalColor = new Color(0.6557f, 0.9145f, 1, 0.3765f);
                    t.pressedColor = new Color(0.3821f, 0.8455f, 1, 0.502f);
                }
            }

            // 添加点击事件
            Button button = obj.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(callback);

            return obj;
        }

        static public GameObject CreateCheckBox (bool initialValue, UnityAction<bool> onValueChange, string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = UnityEngine.Object.Instantiate(ReassembledObjectCache.CheckBox, parent);
            go.SetActive(true);
            go.name = name;

            Toggle toggle = go.GetComponent<Toggle>();
            toggle.isOn = initialValue;
            toggle.onValueChanged.AddListener(onValueChange);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.Zeroize();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            return go;
        }

        static public GameObject CreateSliderWithHandle (
            float initialValue,
            float minValue,
            float maxValue,
            bool wholeNumbers,
            string label,
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            params UnityAction<float>[] onValueChange
        )
        {
            GameObject go = UnityEngine.Object.Instantiate(ReassembledObjectCache.SliderWithHandle, parent);
            go.name = name;

            go.GetComponent<Text>().text = label;

            GameObject goSlider = go.transform.Find("Slider").gameObject;
            Slider cmpSlider = goSlider.GetComponent<Slider>();
            cmpSlider.minValue = minValue;
            cmpSlider.maxValue = maxValue;
            cmpSlider.wholeNumbers = wholeNumbers;
            cmpSlider.value = initialValue;
            foreach (var action in onValueChange)
            {
                cmpSlider.onValueChanged.AddListener(action);
            }

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            return go;
        }
    }
}
