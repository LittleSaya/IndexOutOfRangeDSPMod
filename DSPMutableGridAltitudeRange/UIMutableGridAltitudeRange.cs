using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Common;
using DSPMutableGridAltitudeRange.Cache;

namespace DSPMutableGridAltitudeRange
{
    internal class UIMutableGridAltitudeRange
    {
        static public GameObject Create (bool initialValue, UnityAction<bool> onValueCange, UnityAction onConfigButtonClick)
        {
            GameObject goWin = new GameObject("Mutable Grid Altitude Range Window", typeof(RectTransform), typeof(Image));
            goWin.transform.SetParent(NativeObjectCache.FunctionPanel.transform);
            goWin.SetActive(false);

            Image goWin_cmpImage = goWin.GetComponent<Image>();
            goWin_cmpImage.sprite = ResourceCache.SpriteRectP1;
            goWin_cmpImage.color = new Color(0.125f, 0.125f, 0.125f, 0.85f);

            RectTransform goWin_cmpRect = goWin.GetComponent<RectTransform>();
            goWin_cmpRect.Zeroize();
            goWin_cmpRect.anchorMin = goWin_cmpRect.anchorMax = new Vector2(1, 0);
            goWin_cmpRect.offsetMin = new Vector2(30, 44);
            goWin_cmpRect.offsetMax = new Vector2(177, 84);

            Common.UIUtility.CreateText("Mutable Alt", ResourceCache.FontSAIRASB, "label", goWin.transform, Vector2.zero, Vector2.one, new Vector2(14, 0), new Vector2(0, -9));

            UIUtility.CreateCheckBox(initialValue, onValueCange, "check-box", goWin.transform, new Vector2(1, 0), Vector2.one, new Vector2(-71, 2), new Vector2(-37, -2));

            UIUtility.CreateImageButton(ResourceCache.SpriteSignal504, onConfigButtonClick, "config-button", goWin.transform, new Vector2(1, 0), Vector2.one, new Vector2(-42, 2), new Vector2(-8, -2));

            return goWin;
        }
    }
}
