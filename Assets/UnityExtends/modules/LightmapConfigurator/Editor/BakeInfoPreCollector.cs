using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;
using AORCore;
using static UnityEngine.Rendering.Universal.LightmapConfigurator.LightmapConfigurator;


namespace UnityEngine.Rendering.Universal.LightmapConfigurator.Editor
{

    #region Sub Class

    public struct BakeNodeSrcInfo
    {
        public static BakeNodeSrcInfo Create(MeshRenderer meshRenderer)
        {
            return new BakeNodeSrcInfo
            {
                ScaleInLightmap = meshRenderer.scaleInLightmap,
                StichSeams = meshRenderer.stitchLightmapSeams,
                ShadowCastingMode = meshRenderer.shadowCastingMode
            };
        }

        public static BakeNodeSrcInfo Create(Terrain terrain)
        {
            float scaleInLightmap = LightmapConfiguratorBakeUtility.GetTerrainScaleInLightmap(terrain);
            return new BakeNodeSrcInfo
            {
                ScaleInLightmap = scaleInLightmap,
                StichSeams = false,
                ShadowCastingMode = terrain.shadowCastingMode
            };
        }

        public float ScaleInLightmap;
        public bool StichSeams;
        public ShadowCastingMode ShadowCastingMode;

        public void Apply(MeshRenderer meshRenderer)
        {
            meshRenderer.scaleInLightmap = ScaleInLightmap;
            meshRenderer.stitchLightmapSeams = StichSeams;
            meshRenderer.shadowCastingMode = ShadowCastingMode;
            EditorUtility.SetDirty(meshRenderer);
        }

        public void Apply(Terrain terrain)
        {
            LightmapConfiguratorBakeUtility.SetTerrainScaleInLightmap(terrain, ScaleInLightmap);
            terrain.shadowCastingMode = ShadowCastingMode;
        }

    }

    public class BakeNode
    {
        public LightmapRenderType RenderType;
        public MeshRenderer MeshRenderer;
        public Terrain Terrain;
    }

    public class LightmapConfiguratorShell
    {

        private static void SetLinks(LightmapConfiguratorShell parent, LightmapConfiguratorShell child)
        {
            child.m_parent = parent;
            if(!parent.m_children.Contains(child))
                parent.m_children.Add(child);
        }
        private static void RemoveLinks(LightmapConfiguratorShell parent, LightmapConfiguratorShell child)
        {
            parent.m_children.FastRemove(child);
            child.m_parent = null;
        }

        public string SaveDir;

        private LightmapConfigurator m_configurator;
        public LightmapConfigurator Configurator => m_configurator;

        private LightmapConfiguratorShell m_parent;
        public LightmapConfiguratorShell Parent => m_parent;

        private readonly List<LightmapConfiguratorShell> m_children = new List<LightmapConfiguratorShell>();
        public int ChildrenCount => m_children.Count;
        public LightmapConfiguratorShell(LightmapConfigurator configurator)
        {
            m_configurator = configurator;
        }
         
        public void Dispose()
        {
            if(m_parent != null)
            {
                RemoveLinks(m_parent, this);
                m_parent = null;
            }

            if(m_children.Count > 0)
            { 
                List< LightmapConfiguratorShell > loop = new List< LightmapConfiguratorShell >(m_children);
                foreach(LightmapConfiguratorShell child in loop)
                {
                    RemoveLinks(child.Parent, child);
                }
                loop.Clear();
                m_children.Clear();
            }

            m_configurator = null;
        }

        public void UpdateSaveDirPath()
        {
            SaveDir = LightmapConfiguratorBakeUtility.UpdateSaveDirPathForConfigurator(m_configurator);
        }

        public override bool Equals(object obj)
        {
            LightmapConfiguratorShell sell = obj as LightmapConfiguratorShell;
            return sell.m_configurator == m_configurator;
        }

        public override int GetHashCode()
        {
            if(m_configurator)
                return m_configurator.GetHashCode();
            return 0;
        }

        public void SetParent(LightmapConfiguratorShell parent)
        {
            if (m_parent != null)
                RemoveLinks(parent, this);

            if (parent != null)
                SetLinks(parent, this);
        }

        public LightmapConfiguratorShell GetChild(int index)
        {
            if(index >= 0 && index < m_children.Count)
                return m_children[index];
            return null;
        }

        public LightmapConfiguratorShell this[int index]
        {
            get => GetChild(index);
        }

    }

    #endregion

    /// <summary>
    /// 烘焙信息预收集器
    /// </summary>
    public class BakeInfoPreCollector
    {

        private readonly Dictionary<LightmapConfigurator, LightmapConfiguratorShell> m_ShellRefDic = new Dictionary<LightmapConfigurator, LightmapConfiguratorShell>();

        public readonly Dictionary<LightmapConfiguratorShell, List<BakeNode>> LightmapConfiguratorDic = new Dictionary<LightmapConfiguratorShell, List<BakeNode>>();
        //
        public readonly Dictionary<LightmapConfiguratorShell, string> LightmapConfiguratorSaveDirDic = new Dictionary<LightmapConfiguratorShell, string>();
        public readonly Dictionary<BakeNode, BakeNodeSrcInfo> BakeNodeSrcInfoDic = new Dictionary<BakeNode, BakeNodeSrcInfo>();
        public readonly Dictionary<LCGlobalNodeTag, bool> LCGlobalNodeTags = new Dictionary<LCGlobalNodeTag, bool>();
        
        public readonly List<LightmapConfiguratorShell> RootConfigurators = new List<LightmapConfiguratorShell>();

        public BakeInfoPreCollector()
        {
            Init();
        }

        public void Dispose()
        {
            m_ShellRefDic.Clear();
            LightmapConfiguratorDic.Clear();
            LightmapConfiguratorSaveDirDic.Clear();
            BakeNodeSrcInfoDic.Clear();
            LCGlobalNodeTags.Clear();
            RootConfigurators.Clear();
        }

        public LightmapConfiguratorShell FindShellByConfigurator(LightmapConfigurator configurator)
        {
            if (m_ShellRefDic.ContainsKey(configurator))
            {
                return m_ShellRefDic[configurator];
            }
            return null;
        }

        //CollectBakeNodes
        private void Init()
        {
            GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in rootObjs)
            {
                //记录LCGlobalNodeTag状态
                LCGlobalNodeTag[] gnTags = root.GetComponentsInChildren<LCGlobalNodeTag>(true);
                foreach (var tag in gnTags)
                {
                    LCGlobalNodeTags.Add(tag, tag.gameObject.activeSelf);
                }

                LightmapConfigurator[] configurators = root.GetComponentsInChildren<LightmapConfigurator>();
                foreach (var configurator in configurators)
                {

                    if(!configurator.enabled)
                        continue;

                    LightmapConfiguratorShell shell = new LightmapConfiguratorShell(configurator);
                    m_ShellRefDic.Add(configurator, shell);
                    if (LightmapConfiguratorBakeUtility.IsSubLightmapConfigurator(configurator, out var parent))
                    {
                        shell.SetParent(m_ShellRefDic[parent]);
                    }
                    else
                    {
                        RootConfigurators.Add(shell);
                    }
                    var bakeNodeList = new List<BakeNode>();
                    LightmapConfiguratorBakeUtility.CollectShellBakeNodes(shell, BakeNodeSrcInfoDic, ref bakeNodeList);
                    LightmapConfiguratorDic.Add(shell, bakeNodeList);

                }
            }
        }


    }

}
