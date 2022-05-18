using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DSPWhiteBuildMode.Cache
{
    internal static class ReassembledObjectCache
    {
        static public GameObject CheckBox = null;

        static private GameObject root = null;

        static public void Initialize ()
        {
            root = new GameObject("DSPWhiteBuildMode_ReassembledObjectCache");
            root.SetActive(false);

            CheckBox = UnityEngine.Object.Instantiate(NativeObjectCache.CheckBox, root.transform);
            CheckBox.SetActive(false);
            CheckBox.name = "check-box";
            Toggle toggle = CheckBox.GetComponent<Toggle>();
            toggle.isOn = false;
            toggle.onValueChanged.RemoveAllListeners();
        }
    }
}
