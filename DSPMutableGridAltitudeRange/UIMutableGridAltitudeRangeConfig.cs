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
    internal class UIMutableGridAltitudeRangeConfig
    {
        static public GameObject Create (float zMinInitialValue, float zMaxInitialValue, UnityAction<float> onZMinValueChange, UnityAction<float> onZMaxValueChange)
        {
            GameObject goWin = new GameObject("Mutable Grid Altitude Range Config", typeof(RectTransform), typeof(Image));
            goWin.transform.SetParent(NativeObjectCache.FunctionPanel.transform);
            goWin.SetActive(false);

            Image goWin_cmpImage = goWin.GetComponent<Image>();
            goWin_cmpImage.sprite = ResourceCache.SpriteRectP1;
            goWin_cmpImage.color = new Color(0.125f, 0.125f, 0.125f, 0.85f);

            RectTransform goWin_cmpRect = goWin.GetComponent<RectTransform>();
            goWin_cmpRect.Zeroize();
            goWin_cmpRect.anchorMin = goWin_cmpRect.anchorMax = new Vector2(1, 0);
            goWin_cmpRect.offsetMin = new Vector2(27, 84);
            goWin_cmpRect.offsetMax = new Vector2(220, 156);

            // ZMin
            Text zMinText = null;

            GameObject goZMin = UIUtility.CreateSliderWithHandle(
                (float)Math.Truncate(-zMinInitialValue * 10), 1, 200, true, "High",
                "zMin", goWin.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -68), new Vector2(145, -38),
                value =>
                {
                    onZMinValueChange.Invoke(-value / 10);
                    zMinText.text = $"{value / 10}";
                }
            );

            RectTransform zMinSliderRect = goZMin.transform.Find("Slider").GetComponent<RectTransform>();
            zMinSliderRect.Zeroize();
            zMinSliderRect.anchorMin = zMinSliderRect.anchorMax = new Vector2(0, 1);
            zMinSliderRect.offsetMin = new Vector2(50, -26);
            zMinSliderRect.offsetMax = new Vector2(150, -6);

            zMinText = goZMin.transform.Find("Slider/Handle Slide Area/Handle/Text").GetComponent<Text>();
            zMinText.text = $"{-zMinInitialValue}";

            // ZMax
            Text zMaxText = null;

            GameObject goZMax = UIUtility.CreateSliderWithHandle(
                (float)Math.Truncate(zMaxInitialValue * 10), 1, 200, true, "Low",
                "zMax", goWin.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -34), new Vector2(145, -4),
                value =>
                {
                    onZMaxValueChange.Invoke(value / 10);
                    zMaxText.text = $"{value / 10}";
                }
            );

            RectTransform zMaxSliderRect = goZMax.transform.Find("Slider").GetComponent<RectTransform>();
            zMaxSliderRect.Zeroize();
            zMaxSliderRect.anchorMin = zMaxSliderRect.anchorMax = new Vector2(0, 1);
            zMaxSliderRect.offsetMin = new Vector2(50, -26);
            zMaxSliderRect.offsetMax = new Vector2(150, -6);

            zMaxText = goZMax.transform.Find("Slider/Handle Slide Area/Handle/Text").GetComponent<Text>();
            zMaxText.text = $"{zMaxInitialValue}";

            return goWin;
        }
    }
}
