using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Common;
using UnityEngine.Events;

namespace DSPWhiteBuildMode
{
    internal class UIWhiteBuildModeConfigWindow : MonoBehaviour
    {
        static public GameObject Create (bool initialValue, UnityAction<bool> onConfigChange)
        {
            GameObject goWin = new GameObject("White Build Mode Config Window", typeof(RectTransform), typeof(Image));
            goWin.transform.SetParent(Cache.NativeObjectCache.FunctionPanel.transform);
            goWin.SetActive(false);

            Image goWin_cmpImage = goWin.GetComponent<Image>();
            goWin_cmpImage.sprite = Cache.ResourceCache.SpriteRectP1;
            goWin_cmpImage.color = new Color(0.125f, 0.125f, 0.125f, 0.85f);

            RectTransform goWin_cmpRect = goWin.GetComponent<RectTransform>();
            goWin_cmpRect.Zeroize();
            goWin_cmpRect.anchorMin = goWin_cmpRect.anchorMax = new Vector2(1, 0);
            goWin_cmpRect.offsetMin = new Vector2(-120, 0);
            goWin_cmpRect.offsetMax = new Vector2(-5, 40);

            Common.UIUtility.CreateText("White Mode", Cache.ResourceCache.FontSAIRASB, "label", goWin.transform, Vector2.zero, Vector2.one, new Vector2(14, 0), new Vector2(0, -9));

            UIUtility.CreateCheckBox(initialValue, onConfigChange, "check-box", goWin.transform, new Vector2(1, 0), Vector2.one, new Vector2(-42, 2), new Vector2(-8, -2));

            return goWin;
        }
    }
}
