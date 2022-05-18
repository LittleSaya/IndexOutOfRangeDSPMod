using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Common;

namespace DSPWhiteBuildMode
{
    internal static class UIUtility
    {
        static public GameObject CreateCheckBox (bool initialValue, UnityAction<bool> onValueChange, string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = UnityEngine.Object.Instantiate(Cache.ReassembledObjectCache.CheckBox, parent);
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
    }
}
