using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Common;

namespace DSPColorfulBuildingGrid
{
    internal class UIColorfulBuildingGridColorPickerWindow
    {
        static public GameObject Create (float redInitialValue, float greenInitialValue, float blueInitialValue, float alphaInitialValue, UnityAction<float> onRedValueChange, UnityAction<float> onGreenValueChange, UnityAction<float> onBlueValueChange, UnityAction<float> onAlphaValueChange)
        {
            GameObject goWin = new GameObject("Colorful Building Grid Color Picker Window", typeof(RectTransform), typeof(Image));
            goWin.transform.SetParent(Cache.NativeObjectCache.FunctionPanel.transform);
            goWin.SetActive(false);

            Image goWin_cmpImage = goWin.GetComponent<Image>();
            goWin_cmpImage.sprite = Cache.ResourceCache.SpriteRectP1;
            goWin_cmpImage.color = new Color(0.125f, 0.125f, 0.125f, 0.85f);

            RectTransform goWin_cmpRect = goWin.GetComponent<RectTransform>();
            goWin_cmpRect.Zeroize();
            goWin_cmpRect.anchorMin = goWin_cmpRect.anchorMax = new Vector2(1, 0);
            goWin_cmpRect.offsetMin = new Vector2(-128, 84);
            goWin_cmpRect.offsetMax = new Vector2(125, 224);

            // 颜色预览
            GameObject goColorPreview = new GameObject("Color Preview", typeof(RectTransform), typeof(Image));
            goColorPreview.transform.SetParent(goWin.transform);

            Image goColorPreview_cmpImage = goColorPreview.GetComponent<Image>();
            goColorPreview_cmpImage.sprite = Cache.ResourceCache.SpriteRectP1;
            goColorPreview_cmpImage.color = new Color(redInitialValue / 255, greenInitialValue / 255, blueInitialValue / 255, alphaInitialValue / 255);
            //goColorPreview_cmpImage.material = Cache.ResourceCache.MaterialBuildGridMat;

            RectTransform goColorPreview_cmpRect = goColorPreview.GetComponent<RectTransform>();
            goColorPreview_cmpRect.Zeroize();
            goColorPreview_cmpRect.anchorMin = new Vector2(0, 1);
            goColorPreview_cmpRect.anchorMax = new Vector2(1, 1);
            goColorPreview_cmpRect.offsetMin = new Vector2(6, -24);
            goColorPreview_cmpRect.offsetMax = new Vector2(-6, -14);

            // 红色
            Text redText = null;

            GameObject goRed = UIUtility.CreateSliderWithHandle(
                redInitialValue, 0, 255, true, "Red",
                "red", goWin.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -58), new Vector2(145, -28),
                onRedValueChange,
                value =>
                {
                    Color color = goColorPreview_cmpImage.color;
                    color.r = value / 255;
                    goColorPreview_cmpImage.color = color;
                    redText.text = $"{(int)value}";
                }
            );

            RectTransform redSliderRect = goRed.transform.Find("Slider").GetComponent<RectTransform>();
            redSliderRect.Zeroize();
            redSliderRect.anchorMin = redSliderRect.anchorMax = new Vector2(0, 1);
            redSliderRect.offsetMin = new Vector2(50, -26);
            redSliderRect.offsetMax = new Vector2(200, -6);

            redText = goRed.transform.Find("Slider/Handle Slide Area/Handle/Text").GetComponent<Text>();
            redText.text = $"{(int)redInitialValue}";

            // 绿色
            Text greenText = null;

            GameObject goGreen = UIUtility.CreateSliderWithHandle(
                greenInitialValue, 0, 255, true, "Green",
                "green", goWin.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -82), new Vector2(145, -52),
                onGreenValueChange,
                value =>
                {
                    Color color = goColorPreview_cmpImage.color;
                    color.g = value / 255;
                    goColorPreview_cmpImage.color = color;
                    greenText.text = $"{(int)value}";
                }
            );

            RectTransform greenSliderRect = goGreen.transform.Find("Slider").GetComponent<RectTransform>();
            greenSliderRect.Zeroize();
            greenSliderRect.anchorMin = greenSliderRect.anchorMax = new Vector2(0, 1);
            greenSliderRect.offsetMin = new Vector2(50, -26);
            greenSliderRect.offsetMax = new Vector2(200, -6);

            greenText = goGreen.transform.Find("Slider/Handle Slide Area/Handle/Text").GetComponent<Text>();
            greenText.text = $"{(int)greenInitialValue}";

            // 蓝色
            Text blueText = null;

            GameObject goBlue = UIUtility.CreateSliderWithHandle(
                blueInitialValue, 0, 255, true, "Blue",
                "blue", goWin.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -106), new Vector2(145, -76),
                onBlueValueChange,
                value =>
                {
                    Color color = goColorPreview_cmpImage.color;
                    color.b = value / 255;
                    goColorPreview_cmpImage.color = color;
                    blueText.text = $"{(int)value}";
                }
            );

            RectTransform blueSliderRect = goBlue.transform.Find("Slider").GetComponent<RectTransform>();
            blueSliderRect.Zeroize();
            blueSliderRect.anchorMin = blueSliderRect.anchorMax = new Vector2(0, 1);
            blueSliderRect.offsetMin = new Vector2(50, -26);
            blueSliderRect.offsetMax = new Vector2(200, -6);

            blueText = goBlue.transform.Find("Slider/Handle Slide Area/Handle/Text").GetComponent<Text>();
            blueText.text = $"{(int)blueInitialValue}";

            // 透明度
            Text alphaText = null;

            GameObject goAlpha = UIUtility.CreateSliderWithHandle(
                alphaInitialValue, 0, 255, true, "Alpha",
                "alpha", goWin.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -130), new Vector2(145, -100),
                onAlphaValueChange,
                value =>
                {
                    Color color = goColorPreview_cmpImage.color;
                    color.a = value / 255;
                    goColorPreview_cmpImage.color = color;
                    alphaText.text = $"{(int)value}";
                }
            );

            RectTransform alphaSliderRect = goAlpha.transform.Find("Slider").GetComponent<RectTransform>();
            alphaSliderRect.Zeroize();
            alphaSliderRect.anchorMin = alphaSliderRect.anchorMax = new Vector2(0, 1);
            alphaSliderRect.offsetMin = new Vector2(50, -26);
            alphaSliderRect.offsetMax = new Vector2(200, -6);

            alphaText = goAlpha.transform.Find("Slider/Handle Slide Area/Handle/Text").GetComponent<Text>();
            alphaText.text = $"{(int)blueInitialValue}";

            return goWin;
        }
    }
}
