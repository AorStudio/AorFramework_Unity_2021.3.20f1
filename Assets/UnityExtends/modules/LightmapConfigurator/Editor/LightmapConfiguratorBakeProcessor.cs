using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.Rendering.Universal.LightmapConfigurator.Editor
{
    public class LightmapConfiguratorBakeProcessor
    {

        private LightmapConfiguratorShell m_target;
        public LightmapConfiguratorShell target => m_target;

        private Action m_OnBakeFinished;
        private readonly List<LCBakeHelperNodeTag> m_LCBakeHelperNodeTags = new List<LCBakeHelperNodeTag>();
        public LightmapConfiguratorBakeProcessor(LightmapConfiguratorShell target, Action onBakeFinished)
        {
            m_target = target;
            m_target.SaveDir = m_target.SaveDir.EndsWith("/") ? m_target.SaveDir.Substring(0, m_target.SaveDir.Length - 1) : m_target.SaveDir;
            m_OnBakeFinished = onBakeFinished;
        }

        public void Dispose()
        {
            m_target = null;
            m_LCBakeHelperNodeTags.Clear();
            Lightmapping.bakeCompleted -= onBakeCompleted;
            m_OnBakeFinished = null;
        }

        private bool m_isBakeRunning;
        public bool IsBakeRunning => m_isBakeRunning;

        public void BakingStart(bool withBakeProcessWindow = false)
        {
            m_isBakeRunning = true;
            LightmapConfiguratorBakeUtility.CurrentProcessor = this;

            if(withBakeProcessWindow)
                LightmapConfiguratorBakeWindow.Init();

            onPreBake(() =>
            {
                Lightmapping.bakeCompleted -= onBakeCompleted;
                Lightmapping.bakeCompleted += onBakeCompleted;
                Lightmapping.BakeAsync();
            });
        }

        public void BakingCannel()
        {
            Lightmapping.bakeCompleted -= onBakeCompleted;
            Lightmapping.Cancel();
            afterBakeCompleted();
            m_isBakeRunning = false;
        }

        private void onPreBake(Action finishCallback)
        {

            LCBakeHelperNodeTag[] tags = m_target.Configurator.GetComponentsInChildren<LCBakeHelperNodeTag>(true);
            foreach (var tag in tags)
            {
                if(!tag.gameObject.activeSelf)
                {
                    tag.gameObject.SetActive(true);
                    m_LCBakeHelperNodeTags.Add(tag);
                }
            }

            foreach (var rootShell in LightmapConfiguratorBakeUtility.BakePreInfos.RootConfigurators)
            {

                bool isOtherShell = (rootShell != m_target);
                
                Stack <LightmapConfiguratorShell> stack = new Stack<LightmapConfiguratorShell>();
                stack.Push(rootShell);

                while (stack.Count > 0) 
                {

                    LightmapConfiguratorShell shell = stack.Pop();
                    //剔除代理阴影体(设置为ShadowCastingMode.ShadowsOnly的东东)对烘焙造成的影响
                    List<BakeNode> nodes = LightmapConfiguratorBakeUtility.BakePreInfos.LightmapConfiguratorDic[shell];
                    foreach (var node in nodes)
                    {

                        bool dirty = false;

                        if (isOtherShell)
                        {

                            if (node.RenderType == LightmapConfigurator.LightmapRenderType.Render)
                            {
                                node.MeshRenderer.scaleInLightmap = 0;
                                dirty = true;
                            }
                            else if (node.RenderType == LightmapConfigurator.LightmapRenderType.Terrain)
                            {
                                LightmapConfiguratorBakeUtility.SetTerrainScaleInLightmap(node.Terrain, 0);
                                dirty = true;
                            }

                        }

                        if (m_target.Configurator.AutoShadowCastRules)
                        {

                            if (node.RenderType == LightmapConfigurator.LightmapRenderType.Render)
                            {
                                if (node.MeshRenderer.shadowCastingMode == ShadowCastingMode.Off)
                                {
                                    node.MeshRenderer.shadowCastingMode = ShadowCastingMode.On;
                                    dirty = true;
                                }
                                else if (node.MeshRenderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                                {
                                    node.MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                                    dirty = true;
                                }
                            }
                            else if (node.RenderType == LightmapConfigurator.LightmapRenderType.Terrain)
                            {
                                if (node.MeshRenderer.shadowCastingMode == ShadowCastingMode.Off)
                                {
                                    node.MeshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                                    dirty = true;
                                }
                                else if (node.MeshRenderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                                {
                                    node.MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                                    dirty = true;
                                }
                            }

                        }

                        if (dirty)
                        {
                            if (node.RenderType == LightmapConfigurator.LightmapRenderType.Render)
                            {
                                EditorUtility.SetDirty(node.MeshRenderer.gameObject);
                            }
                            else if (node.RenderType == LightmapConfigurator.LightmapRenderType.Terrain)
                            {
                                EditorUtility.SetDirty(node.Terrain.gameObject);
                            }
                        }

                    }

                    //压栈递归 ...
                    if (shell.ChildrenCount > 0)
                    {
                        for (int i = 0; i < shell.ChildrenCount; i++)
                        {
                            LightmapConfiguratorShell sub = shell[i];
                            stack.Push(sub);
                        }
                    }

                }
            }

            finishCallback?.Invoke();
        }

        private void onBakeCompleted()
        {
            Lightmapping.bakeCompleted -= onBakeCompleted;
            if (m_isBakeRunning)
            {
                LightmapConfiguratorBakeUtility.SaveLightmapDatasInLightmapConfigurator(m_target);
                LightmapConfiguratorBakeUtility.ClearDatasInLightmapSettings();
                afterBakeCompleted();
                m_OnBakeFinished?.Invoke();
                //EditorUtility.DisplayDialog("提示", "烘焙Lightmap完成!", "OK");
                m_isBakeRunning = false;
            }
        }

        private void afterBakeCompleted()
        {

            foreach (var tag in m_LCBakeHelperNodeTags)
            {
                tag.gameObject.SetActive(false);
            }

            //还原BakeNode
            foreach (var kv in LightmapConfiguratorBakeUtility.BakePreInfos.LightmapConfiguratorDic)
            {
                List<BakeNode> nodes = kv.Value;
                foreach (var node in nodes)
                {
                    if (node.RenderType == LightmapConfigurator.LightmapRenderType.Render)
                    {
                        LightmapConfiguratorBakeUtility.BakePreInfos.BakeNodeSrcInfoDic[node].Apply(node.MeshRenderer);
                        EditorUtility.SetDirty(node.MeshRenderer.gameObject);
                    }
                    else if (node.RenderType == LightmapConfigurator.LightmapRenderType.Terrain)
                    {
                        LightmapConfiguratorBakeUtility.BakePreInfos.BakeNodeSrcInfoDic[node].Apply(node.Terrain);
                        EditorUtility.SetDirty(node.Terrain.gameObject);
                    }
                }
            }
        }

    }
}
