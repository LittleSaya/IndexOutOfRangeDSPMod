using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DSPAddPlanet
{
    static class PlanetModelingManagerAccess
    {
        public static Thread planetComputeThread
        {
            get => (Thread)AccessTools.Field(typeof(PlanetModelingManager), "planetComputeThread").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "planetComputeThread").SetValue(null, value);
        }

        public static PlanetData currentModelingPlanet
        {
            get => (PlanetData)AccessTools.Field(typeof(PlanetModelingManager), "currentModelingPlanet").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "currentModelingPlanet").SetValue(null, value);
        }

        public static int currentModelingStage
        {
            get => (int)AccessTools.Field(typeof(PlanetModelingManager), "currentModelingStage").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "currentModelingStage").SetValue(null, value);
        }

        public static int currentModelingSeamNormal
        {
            get => (int)AccessTools.Field(typeof(PlanetModelingManager), "currentModelingSeamNormal").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "currentModelingSeamNormal").SetValue(null, value);
        }

        public static PlanetData currentFactingPlanet
        {
            get => (PlanetData)AccessTools.Field(typeof(PlanetModelingManager), "currentFactingPlanet").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "currentFactingPlanet").SetValue(null, value);
        }

        public static int currentFactingStage
        {
            get => (int)AccessTools.Field(typeof(PlanetModelingManager), "currentFactingStage").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "currentFactingStage").SetValue(null, value);
        }

        public static List<Mesh> tmpMeshList
        {
            get => (List<Mesh>)AccessTools.Field(typeof(PlanetModelingManager), "tmpMeshList").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpMeshList").SetValue(null, value);
        }

        public static List<MeshRenderer> tmpMeshRendererList
        {
            get => (List<MeshRenderer>)AccessTools.Field(typeof(PlanetModelingManager), "tmpMeshRendererList").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpMeshRendererList").SetValue(null, value);
        }

        public static List<MeshCollider> tmpMeshColliderList
        {
            get => (List<MeshCollider>)AccessTools.Field(typeof(PlanetModelingManager), "tmpMeshColliderList").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpMeshColliderList").SetValue(null, value);
        }

        public static Collider tmpOceanCollider
        {
            get => (Collider)AccessTools.Field(typeof(PlanetModelingManager), "tmpOceanCollider").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpOceanCollider").SetValue(null, value);
        }

        public static List<Vector3> tmpVerts
        {
            get => (List<Vector3>)AccessTools.Field(typeof(PlanetModelingManager), "tmpVerts").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpVerts").SetValue(null, value);
        }

        public static List<Vector3> tmpNorms
        {
            get => (List<Vector3>)AccessTools.Field(typeof(PlanetModelingManager), "tmpNorms").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpNorms").SetValue(null, value);
        }

        public static List<Vector4> tmpTgnts
        {
            get => (List<Vector4>)AccessTools.Field(typeof(PlanetModelingManager), "tmpTgnts").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpTgnts").SetValue(null, value);
        }

        public static List<Vector2> tmpUvs
        {
            get => (List<Vector2>)AccessTools.Field(typeof(PlanetModelingManager), "tmpUvs").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpUvs").SetValue(null, value);
        }

        public static List<Vector4> tmpUv2s
        {
            get => (List<Vector4>)AccessTools.Field(typeof(PlanetModelingManager), "tmpUv2s").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpUv2s").SetValue(null, value);
        }

        public static List<int> tmpTris
        {
            get => (List<int>)AccessTools.Field(typeof(PlanetModelingManager), "tmpTris").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpTris").SetValue(null, value);
        }

        public static GameObject tmpPlanetGameObject
        {
            get => (GameObject)AccessTools.Field(typeof(PlanetModelingManager), "tmpPlanetGameObject").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpPlanetGameObject").SetValue(null, value);
        }

        public static GameObject tmpPlanetBodyGameObject
        {
            get => (GameObject)AccessTools.Field(typeof(PlanetModelingManager), "tmpPlanetBodyGameObject").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpPlanetBodyGameObject").SetValue(null, value);
        }

        public static GameObject tmpPlanetReformGameObject
        {
            get => (GameObject)AccessTools.Field(typeof(PlanetModelingManager), "tmpPlanetReformGameObject").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpPlanetReformGameObject").SetValue(null, value);
        }

        public static MeshRenderer tmpPlanetReformRenderer
        {
            get => (MeshRenderer)AccessTools.Field(typeof(PlanetModelingManager), "tmpPlanetReformRenderer").GetValue(null);
            set => AccessTools.Field(typeof(PlanetModelingManager), "tmpPlanetReformRenderer").SetValue(null, value);
        }
    }
}
