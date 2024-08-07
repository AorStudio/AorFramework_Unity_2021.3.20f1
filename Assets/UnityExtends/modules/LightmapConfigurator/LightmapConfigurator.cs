using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Rendering.Universal.LightmapConfigurator
{

    /// <summary>
    /// Author : Aorition
    /// Update : 2023-06-27
    /// </summary>
    //[DisallowMultipleComponent]
    public class LightmapConfigurator : MonoBehaviour
    {
#if UNITY_EDITOR
        public static bool CheckDuplicates(GameObject node)
        {
            LightmapConfigurator[] configurators = node.GetComponents<LightmapConfigurator>();
            if(configurators.Length > 1)
            {
                Queue<LightmapConfigurator> delQueue = new Queue<LightmapConfigurator>();
                for (int i = 0; i < configurators.Length; i++)
                {
                    if(i > 0)
                    {
                        delQueue.Enqueue(configurators[i]);
                    }
                }
                while(delQueue.Count > 0)
                {
                    LightmapConfigurator del = delQueue.Dequeue();
                    if (Application.isPlaying)
                        Object.Destroy(del);
                    else
                    {
                        Object.DestroyImmediate(del);
                    }
                }
                return false;
            }
            return true;
        }
#endif
        #region Inner Classies

        public enum LightmapRenderType
        {
            Render,
            Terrain
        }

        [Serializable]
        public class LightmapRecorder
        {
            public LightmapRenderType type;
            public UnityEngine.Object renderer;
            public int lightmapIndex;
            public Vector4 lightmapScaleOffset;
            //public int realtimeLightmapIndex;
            //public Vector4 realtimeLightmapScaleOffset;

        }

        [Serializable]
        public struct LightMapDataRecorder
        {
            public Texture2D lightmapColor;
            public Texture2D lightmapDir;
            public Texture2D shadowMask;

            private bool m_calHashCode;
            private int m_hashCode;
            public override int GetHashCode()
            {
                if (!m_calHashCode)
                {
                    m_hashCode = (lightmapColor ? lightmapColor.GetHashCode() << 16 : 0)
                    | (lightmapDir ? lightmapDir.GetHashCode() << 8 : 0)
                    | (shadowMask ? shadowMask.GetHashCode() : 0);
                    m_calHashCode = true;
                }
                return m_hashCode;
            }

            public override bool Equals(object obj)
            {
                LightMapDataRecorder b = (LightMapDataRecorder)obj;
                return b.lightmapColor == lightmapColor 
                    && b.lightmapDir == lightmapDir 
                    && b.shadowMask == shadowMask;
            }

            //public int GetLightmapDataKey()
            //{
            //    return (lightmapColor ? lightmapColor.GetHashCode() << 16 : 0)
            //        | (lightmapDir ? lightmapDir.GetHashCode() << 8 : 0)
            //        | (shadowMask ? shadowMask.GetHashCode() : 0);
            //}

            public void Restore(LightmapData data)
            {
                data.lightmapColor = lightmapColor;
                data.lightmapDir = lightmapDir;
                data.shadowMask = shadowMask;
            }

            public void Record(LightmapData data)
            {
                lightmapColor = data.lightmapColor;
                lightmapDir = data.lightmapDir;
                shadowMask = data.shadowMask;
            }

        }

        #endregion

        public LightmapsMode lightmapMode;
        public LightmapRecorder[] lightmapRecorders;
        public LightMapDataRecorder[] lightMapDataRecorders;
        public LightProbes lightProbes;
        //public bool StaticBatching;
        //public List<GameObject> StaticBatchingList;

        public bool HasSerialzeDatas
        {
            get
            {
                return lightMapDataRecorders != null
                       && lightMapDataRecorders.Length > 0
                       && lightmapRecorders != null
                       && lightmapRecorders.Length > 0;
            }
        }

        public void ClearSerialzeDatas()
        {
            if (lightMapDataRecorders != null)
                lightMapDataRecorders = null;
            if (lightmapRecorders != null)
                lightmapRecorders = null;
            lightProbes = null;
        }

        //[HideInInspector] public bool IsSubComponent;

        //private int[] m_runtime_LightmapDataKeys;
        //public int[] LightmapDataKeys => m_runtime_LightmapDataKeys;

        private LightmapConfiguratorManager m_Manager => LightmapConfiguratorManager.Instance;

#if UNITY_EDITOR

        //编辑器功能序列化字段

        [HideInInspector] public bool NormalizationLightmapFileName = true;

        /// <summary>
        /// 使用目标预制体路径保存Lightmap资源
        /// </summary>
        [HideInInspector] public bool UseTargetPrefabPathToSaveLightmapAssets = true;

        [HideInInspector] public String SaveLightmapAssetsDirPath;

        /// <summary>
        /// 烘焙完成后自动保存预制体
        /// </summary>
        [HideInInspector] public bool OverridesPrefabModifiy = true;

        [HideInInspector] public bool IgnoreOverridePos = true;
        [HideInInspector] public bool IgnoreOverrideRotation = true;
        [HideInInspector] public bool IgnoreOverrideScale = true;

        /// <summary>
        /// 使用"Editor Only"Tag识别辅助烘焙对象
        /// </summary>
        //[HideInInspector] public bool UsingEOTagToIdentifyHelperBakedObjects = true;

        /// <summary>
        /// 自动ShadowCast处理规则
        /// </summary>
        [HideInInspector] public bool AutoShadowCastRules = true;

        //---- Gizmos Setting

        [HideInInspector] public bool ShowBakeObjectBounds;
        [HideInInspector] public bool ShowMergedBounds = true;
        [HideInInspector] public Color BakeObjectBoundsColor = new Color(0.0784313725490196f, 0.7490196078431373f, 0.9137254901960784f);
        [HideInInspector] public Color ZeroScaleInLightmapBoundsColor = new Color(1f, 0.1f, 0.1f);
        [HideInInspector] public Color NonBakeBoundsColor = new Color(0.35f, 0f, 0.85f);

        private void OnDrawGizmos()
        {
            if (ShowBakeObjectBounds)
            {
                
                MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
                if (meshRenderers != null && meshRenderers.Length > 0)
                {
                    Bounds mBounds = new Bounds();
                    int i = 0;
                    foreach (var meshRenderer in meshRenderers)
                    {
                        if (meshRenderer.gameObject.activeSelf)
                        {
                            Bounds bounds = meshRenderer.bounds;
                            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(meshRenderer.gameObject);
                            if (flags.HasFlag(StaticEditorFlags.ContributeGI))
                            {

                                if (i > 0)
                                {
                                    mBounds.Encapsulate(bounds);
                                }
                                else
                                {
                                    mBounds = new Bounds(bounds.center, bounds.size);
                                }
                                if (meshRenderer.scaleInLightmap > 0)
                                {
                                    if (!ShowMergedBounds)
                                    {
                                        Gizmos.color = BakeObjectBoundsColor;
                                        Gizmos.DrawWireCube(bounds.center, bounds.size);
                                    }
                                }
                                else
                                {
                                    Gizmos.color = ZeroScaleInLightmapBoundsColor;
                                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                                }
                                i++;
                            }
                            else
                            {
                                Gizmos.color = NonBakeBoundsColor;
                                Gizmos.DrawWireCube(bounds.center, bounds.size);
                            }
                        }
                    }

                    if (ShowMergedBounds)
                    {
                        Gizmos.color = BakeObjectBoundsColor;
                        Gizmos.DrawWireCube(mBounds.center, mBounds.size);
                    }
                }

            }
        }

#endif
        
        //private void Awake()
        //{

        //    //运行时防止嵌套使用导致Lightmap加载错误
        //    if (transform.parent)
        //    {
        //        LightmapConfigurator parent = transform.parent.GetComponentInParent<LightmapConfigurator>();
        //        if (parent)
        //        {
        //            IsSubComponent = true;
        //            enabled = false;
        //            return;
        //        }
        //    }


        //    if (lightMapDataRecorders != null && lightMapDataRecorders.Length > 0)
        //    {
        //        m_runtime_LightmapDataKeys = new int[lightMapDataRecorders.Length];
        //        for (int i = 0; i < lightMapDataRecorders.Length; i++)
        //        {
        //            m_runtime_LightmapDataKeys[i] = lightMapDataRecorders[i].GetLightmapDataKey();
        //        }
        //    }
        //}

        private void OnEnable()
        {
            //if (IsSubComponent) return;
            m_Manager.Register(this);
        }

        private void OnDisable()
        {
            //if (IsSubComponent) return;
            m_Manager.Unregister(this);
        }

        public void DisableLightmapParams()
        {
            if (lightmapRecorders != null && lightmapRecorders.Length > 0)
            {
                for (int i = 0; i < lightmapRecorders.Length; i++)
                {
                    LightmapRecorder info = lightmapRecorders[i];
                    if (info == null || info.renderer == null)
                        continue;

                    switch (info.type)
                    {
                        case LightmapRenderType.Render:
                            {
                                Renderer renderer = (Renderer)info.renderer;
                                renderer.lightmapIndex = -1;
                                renderer.lightmapScaleOffset = new Vector4(1, 1, 0, 0);
                                //renderer.realtimeLightmapIndex = -1;
                                //renderer.realtimeLightmapScaleOffset = new Vector4(1, 1, 0, 0);

                            }
                            break;
                        case LightmapRenderType.Terrain:
                            {
                                Terrain terrain = (Terrain)info.renderer;
                                terrain.lightmapIndex = -1;
                                terrain.lightmapScaleOffset = new Vector4(1, 1, 0, 0);
                                //terrain.realtimeLightmapIndex = -1;
                                //terrain.realtimeLightmapScaleOffset = new Vector4(1, 1, 0, 0);
                            }
                            break;
                    }
                }
            }
        }

        public void UpdateRenderersLightmapSetting()
        {
            if (lightmapRecorders != null && lightmapRecorders.Length > 0)
            {
                for (int i = 0; i < lightmapRecorders.Length; i++)
                {
                    LightmapRecorder info = lightmapRecorders[i];

                    if (info == null || info.renderer == null)
                        continue;

                    switch (info.type)
                    {
                        case LightmapRenderType.Render:
                            {
                                Renderer renderer = (Renderer)info.renderer;
                                if (info.lightmapIndex > -1)
                                {
                                    renderer.lightmapIndex = info.lightmapIndex;
                                    renderer.lightmapScaleOffset = info.lightmapScaleOffset;
                                }
                                //if (info.realtimeLightmapIndex > -1)
                                //{
                                //    renderer.realtimeLightmapIndex = info.realtimeLightmapIndex;
                                //    renderer.realtimeLightmapScaleOffset = info.realtimeLightmapScaleOffset;
                                //}
                            }

                            break;
                        case LightmapRenderType.Terrain:
                            {
                                Terrain terrain = (Terrain)info.renderer;
                                if (info.lightmapIndex > -1)
                                {
                                    terrain.lightmapIndex = info.lightmapIndex;
                                    terrain.lightmapScaleOffset = info.lightmapScaleOffset;
                                }
                                //if (info.realtimeLightmapIndex > -1)
                                //{
                                //    terrain.realtimeLightmapIndex = info.realtimeLightmapIndex;
                                //    terrain.realtimeLightmapScaleOffset = info.realtimeLightmapScaleOffset;
                                //}
                            }
                            break;
                    }
                }
            }
        }

        public void UpdateRenderersLightmapSetting(int[] refKeys)
        {
            if (lightmapRecorders != null && lightmapRecorders.Length > 0)
            {

                int refKeysLen = refKeys.Length;

                for (int i = 0; i < lightmapRecorders.Length; i++)
                {
                    LightmapRecorder info = lightmapRecorders[i];

                    if (info == null || info.renderer == null)
                        continue;

                    switch (info.type)
                    {
                        case LightmapRenderType.Render:
                            {
                                Renderer renderer = (Renderer)info.renderer;
                                if(info.lightmapIndex > -1 && info.lightmapIndex < refKeysLen) 
                                {
                                    renderer.lightmapIndex = refKeys[info.lightmapIndex];
                                    renderer.lightmapScaleOffset = info.lightmapScaleOffset;
                                }
                                //if(info.realtimeLightmapIndex > -1 && info.realtimeLightmapIndex < refKeysLen)
                                //{
                                //    renderer.realtimeLightmapIndex = refKeys[info.realtimeLightmapIndex];
                                //    renderer.realtimeLightmapScaleOffset = info.realtimeLightmapScaleOffset;
                                //}
                            }

                            break;
                        case LightmapRenderType.Terrain:
                            {
                                Terrain terrain = (Terrain)info.renderer;
                                if (info.lightmapIndex > -1 && info.lightmapIndex < refKeysLen)
                                {
                                    terrain.lightmapIndex = refKeys[info.lightmapIndex];
                                    terrain.lightmapScaleOffset = info.lightmapScaleOffset;
                                }
                                //if (info.realtimeLightmapIndex > -1 && info.realtimeLightmapIndex < refKeysLen)
                                //{
                                //    terrain.realtimeLightmapIndex = refKeys[info.realtimeLightmapIndex];
                                //    terrain.realtimeLightmapScaleOffset = info.realtimeLightmapScaleOffset;
                                //}
                            }
                            break;
                    }
                }
            }
        }
    }

}


