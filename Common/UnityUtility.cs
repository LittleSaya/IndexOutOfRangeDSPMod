using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
    static public class UnityUtility
    {
        static public bool TryFindScene (string name, out Scene scene)
        {
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; ++i)
            {
                if (SceneManager.GetSceneAt(i).name == name)
                {
                    scene = SceneManager.GetSceneAt(i);
                    return true;
                }
            }
            scene = default(Scene);
            return false;
        }

        static public bool TryFindRootGameObject (Scene scene, string name, out GameObject obj)
        {
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            foreach (GameObject o in rootGameObjects)
            {
                if (o.name == name)
                {
                    obj = o;
                    return true;
                }
            }
            obj = null;
            return true;
        }
    }
}
