using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DSPMutableGridAltitudeRange.Cache
{
    internal static class ReassembledObjectCache
    {
        static public GameObject CheckBox = null;

        static public GameObject SliderWithHandle = null;

        static private GameObject root = null;

        static public void Initialize ()
        {
            root = new GameObject("DSPMutableGridAltitudeRange_ReassembledObject");
            root.SetActive(false);

            CheckBox = UnityEngine.Object.Instantiate(NativeObjectCache.CheckBox, root.transform);
            CheckBox.SetActive(false);
            CheckBox.name = "check-box";
            Toggle toggle = CheckBox.GetComponent<Toggle>();
            toggle.isOn = false;
            toggle.onValueChanged.RemoveAllListeners();

            SliderWithHandle = UnityEngine.Object.Instantiate(NativeObjectCache.Audio, root.transform);
            SliderWithHandle.name = "slider-with-handle";
            UnityEngine.Object.DestroyImmediate(SliderWithHandle.GetComponent<Localizer>());
            SliderWithHandle.GetComponent<Text>().text = "";
            SliderWithHandle.transform.Find("Slider").GetComponent<Slider>().onValueChanged.RemoveAllListeners();
        }
    }
}
