using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor;
using static UnityEngine.Rendering.Universal.LightmapConfigurator.LightmapConfigurator;
using AORCore.Editor;

namespace UnityEngine.Rendering.Universal.LightmapConfigurator.Editor
{
	/// <summary>
	/// Author : Aorition
	/// Update : 2023-06-30
	/// </summary>
    public class LightmapConfiguratorBakeUtility
    {

        public static BakeInfoPreCollector BakePreInfos;
        public static LightmapConfiguratorBakeProcessor CurrentProcessor;

        private static readonly List<LightmapConfiguratorShell> m_BatchBakingList = new List<LightmapConfiguratorShell>();
        public static List<LightmapConfiguratorShell> BatchBakingList => m_BatchBakingList;

        private static int m_BatchBakingIndex = -1;
        public static int BatchBakingIndex => m_BatchBakingIndex;

        public static void RestBakeUtility()
        {
            if(BakePreInfos != null)
            {
                BakePreInfos.Dispose();
                BakePreInfos = null;
            }
            if(CurrentProcessor != null)
            {
                CurrentProcessor.Dispose();
                CurrentProcessor = null;
            }
            m_BatchBakingList.Clear();
            m_BatchBakingIndex = -1;
        }

        public static void BakingCannel()
        {
            if(CurrentProcessor != null)
                CurrentProcessor.BakingCannel();

            //还原ScaleInLightmap
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

            ClearDatasInLightmapSettings();
            RestBakeUtility();
        }

        public static void BakingTarget(LightmapConfigurator configurator, Action finishCallback)
        {

            RestBakeUtility();
            //收集场景中的可用信息
            BakePreInfos = new BakeInfoPreCollector();
            //CollectBakeNodes(BakeInfos);

            LightmapConfiguratorShell shell = BakePreInfos.FindShellByConfigurator(configurator);

            //获取configurator的保存路径
            //string saveDir = UpdateSaveDirPathForConfigurator(configurator);
            shell.UpdateSaveDirPath();

            //打开LCGlobalNodeTags
            foreach (var kv in BakePreInfos.LCGlobalNodeTags)
            {
                if (!kv.Value)
                    kv.Key.gameObject.SetActive(true);
            }

            //开始烘焙
            CurrentProcessor = new LightmapConfiguratorBakeProcessor(shell, () => 
            {
                //还原LCGlobalNodeTags
                foreach (var kv in BakePreInfos.LCGlobalNodeTags)
                {
                    if (!kv.Value)
                    {
                        kv.Key.gameObject.SetActive(false);
                    }
                }

                //还原ScaleInLightmap
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

                RestBakeUtility();
                finishCallback?.Invoke();
            });
            CurrentProcessor.BakingStart();
        }

        //--------------- BatchBaking
        
        public static void BatchBaking(Action finishCallback)
        {
            RestBakeUtility();
            //收集场景中的可用信息
            BakePreInfos = new BakeInfoPreCollector();
            //CollectBakeNodes(BakeInfos);

            if(BakePreInfos.LightmapConfiguratorDic.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到场景中存在LightmapConfigurator对象,批量烘焙终止。", "OK");
                return;
            }

            //获取configurator的保存路径
            foreach (var shell in BakePreInfos.RootConfigurators)
            {
                //LightmapConfigurator configurator = shell.Configurator;
                //string saveDir = UpdateSaveDirPathForConfigurator(configurator);
                //BakeInfos.LightmapConfiguratorSaveDirDic.Add(configurator, saveDir);
                //shell.SaveDir = saveDir;
                shell.UpdateSaveDirPath();
            }

            //打开LCGlobalNodeTags
            foreach (var kv in BakePreInfos.LCGlobalNodeTags)
            {
                if (!kv.Value)
                    kv.Key.gameObject.SetActive(true);
            }

            //批量开始烘焙
            m_BatchBakingList.Clear();
            m_BatchBakingIndex = 0;
            foreach (var shell in BakePreInfos.RootConfigurators)
            {
                LightmapConfigurator configurator = shell.Configurator;
                if (configurator.enabled)
                    m_BatchBakingList.Add(shell);
            }
            BatchBakingLoop(finishCallback);
        }

        public static string UpdateSaveDirPathForConfigurator(LightmapConfigurator configurator)
        {
            string saveDir = string.Empty;
            if (configurator.UseTargetPrefabPathToSaveLightmapAssets && PrefabUtility.IsPartOfPrefabInstance(configurator.gameObject))
            {
                string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(configurator.gameObject);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    EditorAssetInfo pathInfo = new EditorAssetInfo(prefabPath);
                    saveDir = $"{pathInfo.dirPath}/{pathInfo.name}_LightmapAssets";
                }
                configurator.SaveLightmapAssetsDirPath = saveDir;
                return saveDir;
            }

            if (!string.IsNullOrEmpty(configurator.SaveLightmapAssetsDirPath))
                saveDir = configurator.SaveLightmapAssetsDirPath;
            while (string.IsNullOrEmpty(saveDir))
            {
                saveDir = SaveFolderPanel(configurator);
            }
            configurator.SaveLightmapAssetsDirPath = saveDir.Replace(Application.dataPath, "Assets");
            return saveDir;
        }

        public static string SaveFolderPanel(LightmapConfigurator configurator)
        {
            string p = EditorUtility.SaveFolderPanel($"烘焙资源({configurator.name})存放路径设置", configurator.SaveLightmapAssetsDirPath, "");
            if (!string.IsNullOrEmpty(p))
                p = p.Replace(Application.dataPath, "Assets");
            return p;
        }

        private static void BatchBakingLoop(Action finishCallback)
        {
            if (CurrentProcessor != null)
            {
                CurrentProcessor.Dispose();
                CurrentProcessor = null;
            }
            if(m_BatchBakingIndex < m_BatchBakingList.Count)
            {
                //LightmapConfigurator configurator = m_BatchBakingList[m_BatchBakingIndex];
                LightmapConfiguratorShell shell = m_BatchBakingList[m_BatchBakingIndex];
                LightmapConfiguratorBakeProcessor processor = new LightmapConfiguratorBakeProcessor(shell, () => 
                {
                    m_BatchBakingIndex++;
                    BatchBakingLoop(finishCallback);
                });
                processor.BakingStart(true);
                CurrentProcessor = processor;
            }
            else
            {
                BatchBakingEnd(finishCallback);
            }
        }

        private static void BatchBakingEnd(Action finishCallback)
        {
            //还原LCGlobalNodeTags
            foreach (var kv in BakePreInfos.LCGlobalNodeTags)
            {
                if (!kv.Value)
                {
                    kv.Key.gameObject.SetActive(false);
                }
            }

            //还原ScaleInLightmap
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

            RestBakeUtility();
            finishCallback?.Invoke();
        }

        //--------------- BatchBaking  END

        //旧方法
        //private static void CollectBakeNodes(BakeInfoCollector infoCollector)
        //{

        //    GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
        //    foreach (var root in rootObjs)
        //    {
        //        LCGlobalNodeTag[] gnTags = root.GetComponentsInChildren<LCGlobalNodeTag>(true);
        //        foreach (var tag in gnTags)
        //        {
        //            infoCollector.LCGlobalNodeTags.Add(tag, tag.gameObject.activeSelf);
        //        }
        //        LightmapConfigurator[] configurators = root.GetComponentsInChildren<LightmapConfigurator>();
        //        foreach (var configurator in configurators)
        //        {
        //            if (IsRootLightmapConfigurator(configurator))
        //            {   
        //                configurator.IsSubComponent = false;
        //                infoCollector.LightmapConfiguratorDic.Add(configurator, new List<BakeNode>());
        //                MeshRenderer[] meshRenderers = configurator.GetComponentsInChildren<MeshRenderer>();
        //                foreach (var meshRenderer in meshRenderers)
        //                {
        //                    if(CheckHasStaticEditorFlags(meshRenderer.gameObject, StaticEditorFlags.ContributeGI))
        //                    {
        //                        BakeNode node = new BakeNode();
        //                        node.RenderType = LightmapRenderType.Render;
        //                        node.MeshRenderer = meshRenderer;
        //                        infoCollector.LightmapConfiguratorDic[configurator].Add(node);
        //                        infoCollector.BakeNodeSrcInfoDic.Add(node, BakeNodeSrcInfo.Create(meshRenderer));

        //                        if(CheckHasStaticEditorFlags(meshRenderer.gameObject, StaticEditorFlags.BatchingStatic))
        //                        {
        //                            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(meshRenderer.gameObject);
        //                            GameObjectUtility.SetStaticEditorFlags(meshRenderer.gameObject, (flags & ~StaticEditorFlags.BatchingStatic));
        //                            EditorUtility.SetDirty(meshRenderer.gameObject);
        //                        }

        //                    }
        //                }
        //                Terrain[] terrains = configurator.GetComponentsInChildren<Terrain>();
        //                foreach (var terrain in terrains)
        //                {
        //                    if (CheckHasStaticEditorFlags(terrain.gameObject, StaticEditorFlags.ContributeGI))
        //                    {
        //                        BakeNode node = new BakeNode();
        //                        node.RenderType = LightmapRenderType.Terrain;
        //                        node.Terrain = terrain;
        //                        infoCollector.LightmapConfiguratorDic[configurator].Add(node);
        //                        infoCollector.BakeNodeSrcInfoDic.Add(node, BakeNodeSrcInfo.Create(terrain));

        //                        if (CheckHasStaticEditorFlags(terrain.gameObject, StaticEditorFlags.BatchingStatic))
        //                        {
        //                            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(terrain.gameObject);
        //                            GameObjectUtility.SetStaticEditorFlags(terrain.gameObject, (flags & ~StaticEditorFlags.BatchingStatic));
        //                            EditorUtility.SetDirty(terrain.gameObject);
        //                        }

        //                    }
        //                }
        //            }
        //            else
        //                configurator.IsSubComponent = true;
        //        }
        //    }
        //}
        /// <summary>
        /// 检查LightmapConfigurator是否是顶层LightmapConfigurator;
        /// </summary>
        public static bool IsRootLightmapConfigurator(LightmapConfigurator configurator)
        {
            if (configurator.transform.parent)
            {
                return !configurator.transform.parent.GetComponentInParent<LightmapConfigurator>();
            }
            return true;
        }
        public static bool IsSubLightmapConfigurator(LightmapConfigurator configurator)
        {
            if (configurator.transform.parent)
            {
                return configurator.transform.parent.GetComponentInParent<LightmapConfigurator>();
            }
            return true;
        }

        public static float GetTerrainScaleInLightmap(Terrain terrain)
        {
            //获取Terrain对象上ScaleLightmap值的神奇方式
            var serializedObject = new SerializedObject(terrain);
            float scaleInLightmap = serializedObject.FindProperty("m_ScaleInLightmap").floatValue;
            serializedObject.Dispose();
            return scaleInLightmap;
        }
        public static void SetTerrainScaleInLightmap(Terrain terrain, float scaleInLightmap)
        {
            var serializedObject = new SerializedObject(terrain);
            serializedObject.Update();
            serializedObject.FindProperty("m_ScaleInLightmap").floatValue = scaleInLightmap;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Dispose();
            EditorUtility.SetDirty(terrain);
        }

        /// <summary>
        /// 记录当前场景Lightmap烘焙数据到LightmapConfigurator
        /// </summary>
        public static void RecordLightmapDatasInLightmapConfigurator(LightmapConfigurator configurator, string saveDir)
        {

            if(LightmapSettings.lightmaps == null || LightmapSettings.lightmaps.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "场景无Lightmap烘焙数据可供记录。", "确定");
                return;
            }

            if (saveDir.EndsWith("/"))
                saveDir = saveDir.Substring(0, saveDir.Length - 1);

            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
                AssetDatabase.Refresh();
            }

            //lightmapMode
            configurator.lightmapMode = LightmapSettings.lightmapsMode;

            //lightMapDataRecorders
            LightmapData[] lightmapDatas = LightmapSettings.lightmaps;
            List<LightmapConfigurator.LightMapDataRecorder> lightmapDataList = new List<LightmapConfigurator.LightMapDataRecorder>();
            for (int i = 0; i < lightmapDatas.Length; i++)
            {
                LightmapData lightmapData = lightmapDatas[i];
                LightmapConfigurator.LightMapDataRecorder recorder = new LightmapConfigurator.LightMapDataRecorder();
                if (lightmapData.lightmapColor)
                {
                    string src = AssetDatabase.GetAssetPath(lightmapData.lightmapColor);
                    EditorAssetInfo info = new EditorAssetInfo(src);
                    string des = $"{saveDir}/{info.name}{info.suffix}";
                    AssetDatabase.DeleteAsset(des);
                    AssetDatabase.CopyAsset(src, des);
                    recorder.lightmapColor = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
                }
                if (lightmapData.lightmapDir)
                {
                    string src = AssetDatabase.GetAssetPath(lightmapData.lightmapDir);
                    EditorAssetInfo info = new EditorAssetInfo(src);
                    string des = $"{saveDir}/{info.name}{info.suffix}";
                    AssetDatabase.DeleteAsset(des);
                    AssetDatabase.CopyAsset(src, des);
                    recorder.lightmapDir = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
                }
                if (lightmapData.shadowMask)
                {
                    string src = AssetDatabase.GetAssetPath(lightmapData.shadowMask);
                    EditorAssetInfo info = new EditorAssetInfo(src);
                    string des = $"{saveDir}/{info.name}{info.suffix}";
                    AssetDatabase.DeleteAsset(des);
                    AssetDatabase.CopyAsset(src, des);
                    recorder.shadowMask = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
                }
                lightmapDataList.Add(recorder);
            }
            AssetDatabase.Refresh();
            configurator.lightMapDataRecorders = lightmapDataList.ToArray();

            //LightMapDataRecorders && lightmapRecorders
            List<LightmapConfigurator.LightmapRecorder> lightmapRecorderList = new List<LightmapConfigurator.LightmapRecorder>();
            MeshRenderer[] meshRenderers = configurator.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                if (CheckHasStaticEditorFlags(meshRenderer.gameObject, StaticEditorFlags.ContributeGI))
                {
                    LightmapConfigurator.LightmapRecorder recorder = new LightmapConfigurator.LightmapRecorder();
                    recorder.type = LightmapConfigurator.LightmapRenderType.Render;
                    recorder.renderer = meshRenderer;
                    recorder.lightmapIndex = meshRenderer.lightmapIndex;
                    recorder.lightmapScaleOffset = meshRenderer.lightmapScaleOffset;
                    //recorder.realtimeLightmapIndex = meshRenderer.realtimeLightmapIndex;
                    //recorder.realtimeLightmapScaleOffset = meshRenderer.realtimeLightmapScaleOffset;
                    lightmapRecorderList.Add(recorder);
                }
            }

            Terrain[] terrains = configurator.GetComponentsInChildren<Terrain>();
            foreach (var terrain in terrains)
            {
                if (CheckHasStaticEditorFlags(terrain.gameObject, StaticEditorFlags.ContributeGI))
                {
                    LightmapConfigurator.LightmapRecorder recorder = new LightmapConfigurator.LightmapRecorder();
                    recorder.type = LightmapConfigurator.LightmapRenderType.Render;
                    recorder.renderer = terrain;
                    recorder.lightmapIndex = terrain.lightmapIndex;
                    recorder.lightmapScaleOffset = terrain.lightmapScaleOffset;
                    lightmapRecorderList.Add(recorder);
                }
            }
            configurator.lightmapRecorders = lightmapRecorderList.ToArray();

            //lightProbes
            if (LightmapSettings.lightProbes != null)
            {
                string lpPath = $"{saveDir}/lightProbe.asset";
                var lp = LightProbes.Instantiate(LightmapSettings.lightProbes);
                AssetDatabase.DeleteAsset(lpPath);
                AssetDatabase.CreateAsset(lp, lpPath);
                AssetDatabase.Refresh();
                configurator.lightProbes = AssetDatabase.LoadAssetAtPath<LightProbes>(lpPath);
            }
            EditorUtility.SetDirty(configurator);
            AssetDatabase.Refresh();
        }

        //旧实现
        //public static void SaveLightmapDatasInLightmapConfigurator(LightmapConfiguratorShell shell)
        //{

        //    if (!Directory.Exists(shell.SaveDir))
        //    {
        //        Directory.CreateDirectory(shell.SaveDir);
        //        AssetDatabase.Refresh();
        //    }

        //    //if (configurator.StaticBatchingList == null)
        //    //    configurator.StaticBatchingList = new List<GameObject>();

        //    var configurator = shell.Configurator;

        //    //lightmapMode
        //    configurator.lightmapMode = LightmapSettings.lightmapsMode;
        //    //lightMapDataRecorders
        //    LightmapData[] lightmapDatas = LightmapSettings.lightmaps;
        //    List<LightmapConfigurator.LightMapDataRecorder> lightmapDataList = new List<LightmapConfigurator.LightMapDataRecorder>();
        //    for (int i = 0; i < lightmapDatas.Length; i++)
        //    {
        //        LightmapData lightmapData = lightmapDatas[i];
        //        LightmapConfigurator.LightMapDataRecorder recorder = new LightmapConfigurator.LightMapDataRecorder();
        //        if (lightmapData.lightmapColor)
        //        {
        //            string src = AssetDatabase.GetAssetPath(lightmapData.lightmapColor);
        //            EditorAssetInfo info = new EditorAssetInfo(src);
        //            string fileName = info.name;
        //            if (configurator.NormalizationLightmapFileName)
        //                fileName = NormalizationLightmapFileName(fileName);
        //            string des = $"{shell.SaveDir}/{fileName}{info.suffix}";
        //            AssetDatabase.DeleteAsset(des);
        //            AssetDatabase.CopyAsset(src, des);
        //            recorder.lightmapColor = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
        //        }
        //        if (lightmapData.lightmapDir)
        //        {
        //            string src = AssetDatabase.GetAssetPath(lightmapData.lightmapDir);
        //            EditorAssetInfo info = new EditorAssetInfo(src);
        //            string fileName = info.name;
        //            if (configurator.NormalizationLightmapFileName)
        //                fileName = NormalizationLightmapFileName(fileName);
        //            string des = $"{shell.SaveDir}/{fileName}{info.suffix}";
        //            AssetDatabase.DeleteAsset(des);
        //            AssetDatabase.CopyAsset(src, des);
        //            recorder.lightmapDir = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
        //        }
        //        if (lightmapData.shadowMask)
        //        {
        //            string src = AssetDatabase.GetAssetPath(lightmapData.shadowMask);
        //            EditorAssetInfo info = new EditorAssetInfo(src);
        //            string fileName = info.name;
        //            if (configurator.NormalizationLightmapFileName)
        //                fileName = NormalizationLightmapFileName(fileName);
        //            string des = $"{shell.SaveDir}/{fileName}{info.suffix}";
        //            AssetDatabase.DeleteAsset(des);
        //            AssetDatabase.CopyAsset(src, des);
        //            recorder.shadowMask = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
        //        }
        //        lightmapDataList.Add(recorder);
        //    }
        //    AssetDatabase.Refresh();
        //    configurator.lightMapDataRecorders = lightmapDataList.ToArray();
        //    //lightmapRecorders
        //    List<LightmapConfigurator.LightmapRecorder> lightmapRecorderList = new List<LightmapConfigurator.LightmapRecorder>();
        //    List<BakeNode> bakeNodes = BakeInfos.LightmapConfiguratorDic[shell];
        //    foreach (var node in bakeNodes)
        //    {
        //        if(node.RenderType == LightmapRenderType.Render)
        //        {
        //            var meshRenderer = node.MeshRenderer;
        //            if (CheckHasStaticEditorFlags(meshRenderer.gameObject, StaticEditorFlags.ContributeGI))
        //            {
        //                LightmapConfigurator.LightmapRecorder recorder = new LightmapConfigurator.LightmapRecorder();
        //                recorder.type = LightmapConfigurator.LightmapRenderType.Render;
        //                recorder.renderer = meshRenderer;
        //                recorder.lightmapIndex = meshRenderer.lightmapIndex;
        //                recorder.lightmapScaleOffset = meshRenderer.lightmapScaleOffset;
        //                recorder.realtimeLightmapIndex = meshRenderer.realtimeLightmapIndex;
        //                recorder.realtimeLightmapScaleOffset = meshRenderer.realtimeLightmapScaleOffset;
        //                lightmapRecorderList.Add(recorder);
        //            }
        //        }
        //        else if(node.RenderType == LightmapRenderType.Terrain)
        //        {
        //            var terrain = node.Terrain;
        //            if (CheckHasStaticEditorFlags(terrain.gameObject, StaticEditorFlags.ContributeGI))
        //            {
        //                LightmapConfigurator.LightmapRecorder recorder = new LightmapConfigurator.LightmapRecorder();
        //                recorder.type = LightmapConfigurator.LightmapRenderType.Render;
        //                recorder.renderer = terrain;
        //                recorder.lightmapIndex = terrain.lightmapIndex;
        //                recorder.lightmapScaleOffset = terrain.lightmapScaleOffset;
        //                recorder.realtimeLightmapIndex = terrain.realtimeLightmapIndex;
        //                recorder.realtimeLightmapScaleOffset = terrain.realtimeLightmapScaleOffset;
        //                lightmapRecorderList.Add(recorder);
        //            }
        //        }
        //    }

        //    configurator.lightmapRecorders = lightmapRecorderList.ToArray();



        //    //lightProbes
        //    if (LightmapSettings.lightProbes != null)
        //    {
        //        string lpPath = $"{shell.SaveDir}/lightProbe.asset";
        //        var lp = LightProbes.Instantiate(LightmapSettings.lightProbes);
        //        AssetDatabase.DeleteAsset(lpPath);
        //        AssetDatabase.CreateAsset(lp, lpPath);
        //        AssetDatabase.Refresh();
        //        configurator.lightProbes = AssetDatabase.LoadAssetAtPath<LightProbes>(lpPath);
        //    }
        //    EditorUtility.SetDirty(configurator);

        //  //OverridesPrefabModifiy
        //  TryOverridesPrefabModifiy(configurator);

        //}

        public static void SaveLightmapDatasInLightmapConfigurator(LightmapConfiguratorShell rootShell)
        {

            if (!Directory.Exists(rootShell.SaveDir))
            {
                Directory.CreateDirectory(rootShell.SaveDir);
                AssetDatabase.Refresh();
            }

            //处理所有Lightmap迁移逻辑并获取totalLightmapDatas缓存
            LightmapData[] lightmapDatas = LightmapSettings.lightmaps;
            List<LightmapConfigurator.LightMapDataRecorder> totalLightmapDatas = new List<LightmapConfigurator.LightMapDataRecorder>();
            bool normalizationLightmapFileName = rootShell.Configurator.NormalizationLightmapFileName;
            for (int i = 0; i < lightmapDatas.Length; i++)
            {
                LightmapData lightmapData = lightmapDatas[i];
                LightmapConfigurator.LightMapDataRecorder recorder = new LightmapConfigurator.LightMapDataRecorder();
                if (lightmapData.lightmapColor)
                {
                    string src = AssetDatabase.GetAssetPath(lightmapData.lightmapColor);
                    EditorAssetInfo info = new EditorAssetInfo(src);
                    string fileName = info.name;
                    if (normalizationLightmapFileName)
                        fileName = NormalizationLightmapFileName(fileName);
                    string des = $"{rootShell.SaveDir}/{fileName}{info.suffix}";
                    AssetDatabase.DeleteAsset(des);
                    AssetDatabase.CopyAsset(src, des);
                    recorder.lightmapColor = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
                }
                if (lightmapData.lightmapDir)
                {
                    string src = AssetDatabase.GetAssetPath(lightmapData.lightmapDir);
                    EditorAssetInfo info = new EditorAssetInfo(src);
                    string fileName = info.name;
                    if (normalizationLightmapFileName)
                        fileName = NormalizationLightmapFileName(fileName);
                    string des = $"{rootShell.SaveDir}/{fileName}{info.suffix}";
                    AssetDatabase.DeleteAsset(des);
                    AssetDatabase.CopyAsset(src, des);
                    recorder.lightmapDir = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
                }
                if (lightmapData.shadowMask)
                {
                    string src = AssetDatabase.GetAssetPath(lightmapData.shadowMask);
                    EditorAssetInfo info = new EditorAssetInfo(src);
                    string fileName = info.name;
                    if (normalizationLightmapFileName)
                        fileName = NormalizationLightmapFileName(fileName);
                    string des = $"{rootShell.SaveDir}/{fileName}{info.suffix}";
                    AssetDatabase.DeleteAsset(des);
                    AssetDatabase.CopyAsset(src, des);
                    recorder.shadowMask = AssetDatabase.LoadAssetAtPath<Texture2D>(des);
                }
                totalLightmapDatas.Add(recorder);
            }
            AssetDatabase.Refresh();

            //Todo ... 简单处理 lightProbes 
            string lightProbesLoadPath = string.Empty;
            if (LightmapSettings.lightProbes != null)
            {
                lightProbesLoadPath = $"{rootShell.SaveDir}/lightProbe.asset";
                var lp = LightProbes.Instantiate(LightmapSettings.lightProbes);
                AssetDatabase.DeleteAsset(lightProbesLoadPath);
                AssetDatabase.CreateAsset(lp, lightProbesLoadPath);
                AssetDatabase.Refresh();

            }

            Stack<LightmapConfiguratorShell> stack = new Stack<LightmapConfiguratorShell>();
            stack.Push(rootShell);

            while (stack.Count > 0) 
            {

                var shell = stack.Pop();
                var configurator = shell.Configurator;

                //lightProbes
                if (!string.IsNullOrEmpty(lightProbesLoadPath))
                {
                    configurator.lightProbes = AssetDatabase.LoadAssetAtPath<LightProbes>(lightProbesLoadPath);
                }

                //lightmapMode
                configurator.lightmapMode = LightmapSettings.lightmapsMode;

                //lightmapRecorders

                List<LightmapConfigurator.LightMapDataRecorder> lightMapDataRecorders = new List<LightMapDataRecorder> ();
                List<LightmapConfigurator.LightmapRecorder> lightmapRecorderList = new List<LightmapConfigurator.LightmapRecorder>();

                List<BakeNode> bakeNodes = BakePreInfos.LightmapConfiguratorDic[shell];
                foreach (var node in bakeNodes)
                {
                    if (node.RenderType == LightmapRenderType.Render)
                    {
                        var meshRenderer = node.MeshRenderer;
                        if (CheckHasStaticEditorFlags(meshRenderer.gameObject, StaticEditorFlags.ContributeGI))
                        {
                            LightmapConfigurator.LightmapRecorder recorder = new LightmapConfigurator.LightmapRecorder();
                            recorder.type = LightmapConfigurator.LightmapRenderType.Render;
                            recorder.renderer = meshRenderer;
                            recorder.lightmapIndex = CollectorLightMapDataRecorderInUse(meshRenderer.lightmapIndex, totalLightmapDatas, ref lightMapDataRecorders);
                            recorder.lightmapScaleOffset = meshRenderer.lightmapScaleOffset;
                            //recorder.realtimeLightmapIndex = meshRenderer.realtimeLightmapIndex;
                            //recorder.realtimeLightmapScaleOffset = meshRenderer.realtimeLightmapScaleOffset;
                            lightmapRecorderList.Add(recorder);
                        }
                    }
                    else if (node.RenderType == LightmapRenderType.Terrain)
                    {
                        var terrain = node.Terrain;
                        if (CheckHasStaticEditorFlags(terrain.gameObject, StaticEditorFlags.ContributeGI))
                        {
                            LightmapConfigurator.LightmapRecorder recorder = new LightmapConfigurator.LightmapRecorder();
                            recorder.type = LightmapConfigurator.LightmapRenderType.Render;
                            recorder.renderer = terrain;
                            recorder.lightmapIndex = CollectorLightMapDataRecorderInUse(terrain.lightmapIndex, totalLightmapDatas, ref lightMapDataRecorders);
                            recorder.lightmapScaleOffset = terrain.lightmapScaleOffset;
                            //recorder.realtimeLightmapIndex = terrain.realtimeLightmapIndex;
                            //recorder.realtimeLightmapScaleOffset = terrain.realtimeLightmapScaleOffset;
                            lightmapRecorderList.Add(recorder);
                        }
                    }
                }

                configurator.lightMapDataRecorders = lightMapDataRecorders.ToArray();
                configurator.lightmapRecorders = lightmapRecorderList.ToArray();

                EditorUtility.SetDirty(configurator);

                //OverridesPrefabModifiy
                TryOverridesPrefabModifiy(configurator);

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

        public static int CollectorLightMapDataRecorderInUse(int totalIndex, List<LightmapConfigurator.LightMapDataRecorder> totalLightmapDatas, ref List<LightmapConfigurator.LightMapDataRecorder> usedLightmapDatas)
        {
            if(totalIndex >= 0 && totalIndex < totalLightmapDatas.Count)
            {
                var current = totalLightmapDatas[totalIndex];
                int index = usedLightmapDatas.IndexOf(current);
                if (index == -1)
                {
                    index = usedLightmapDatas.Count;
                    usedLightmapDatas.Add(current);
                }
                return index;
            }
            return -1;
        }

        public static void ClearDatasInLightmapSettings()
        {
            //Lightmapping.Clear(); 只Clear清不干净
            LightmapSettings.lightmaps = null;
            LightmapSettings.lightProbes = null;
            Lightmapping.Clear();
            Lightmapping.ClearLightingDataAsset();
        }

        public static bool CheckHasStaticEditorFlags(GameObject gameObject, StaticEditorFlags hasFlags)
        {
            return GameObjectUtility.GetStaticEditorFlags(gameObject).HasFlag(hasFlags);
        }

        private static string NormalizationLightmapFileName(string fileName)
        {
            return fileName.Replace(" ", "").Replace("-", "_");
        }

        public static bool IsSubLightmapConfigurator(LightmapConfigurator configurator, out LightmapConfigurator parent)
        {
            if (configurator.transform.parent)
            {
                parent = configurator.transform.parent.GetComponentInParent<LightmapConfigurator>();
                if (parent && parent.enabled)
                    return true;
            }
            parent = null;
            return false;
        }

        public static bool HasSubLightmapConfigurator(LightmapConfigurator configurator)
        {
            if(configurator.transform.childCount > 0)
            {
                var children = configurator.GetComponentsInChildren<LightmapConfigurator>();
                foreach (var child in children)
                {
                    if (child == configurator)
                        continue;
                    if (child.enabled)
                    {
                        //只要有一个符合判断就可以认定结果
                        return true;
                    }

                }
            }
            return false;
        }

        public static void CollectShellBakeNodes(LightmapConfiguratorShell shell, Dictionary<BakeNode, BakeNodeSrcInfo> BakeNodeSrcInfoDic, ref List<BakeNode> collectList)
        {
            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(shell.Configurator.transform);
            while (stack.Count > 0)
            {
                Transform n = stack.Pop();
                if (LightmapConfiguratorBakeUtility.CheckHasStaticEditorFlags(n.gameObject, StaticEditorFlags.ContributeGI))
                {

                    //强制取消标记BatchingStatic的StaticEditorFlags
                    if (LightmapConfiguratorBakeUtility.CheckHasStaticEditorFlags(n.gameObject, StaticEditorFlags.BatchingStatic))
                    {
                        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(n.gameObject);
                        GameObjectUtility.SetStaticEditorFlags(n.gameObject, (flags & ~StaticEditorFlags.BatchingStatic));
                        EditorUtility.SetDirty(n.gameObject);
                    }

                    MeshRenderer meshRenderer = n.GetComponent<MeshRenderer>();
                    if (meshRenderer)
                    {
                        BakeNode node = new BakeNode();
                        node.RenderType = LightmapRenderType.Render;
                        node.MeshRenderer = meshRenderer;
                        collectList.Add(node);
                        BakeNodeSrcInfoDic.Add(node, BakeNodeSrcInfo.Create(meshRenderer));
                    }

                    Terrain terrain = n.GetComponent<Terrain>();
                    if (terrain)
                    {
                        BakeNode node = new BakeNode();
                        node.RenderType = LightmapRenderType.Terrain;
                        node.Terrain = terrain;
                        collectList.Add(node);
                        BakeNodeSrcInfoDic.Add(node, BakeNodeSrcInfo.Create(terrain));
                    }
                }

                if (n.childCount > 0)
                {
                    for (int i = 0; i < n.childCount; i++)
                    {
                        Transform sub = n.GetChild(i);

                        //截断未激活节点
                        if (!sub.gameObject.activeInHierarchy || !sub.gameObject.activeSelf)
                            continue;

                        //截断LightmapConfigurator节点
                        var subLC = sub.GetComponent<LightmapConfigurator>();
                        if (subLC && subLC.enabled)
                            continue;

                        stack.Push(sub);
                    }
                }

            }
        }


        /// <summary>
        /// 复制上级LightmapConfigurator烘焙数据
        /// (此方法只能向上一级复制LC烘焙数据，请保证上级LC烘焙数据已经包含当前节点已下的所有烘焙数据)
        /// </summary>
        /// <param name="target"></param>
        public static void CopyLCDataFormParentLightmapConfigurator(LightmapConfigurator target, BakeInfoPreCollector BakePreInfos)
        {
            LightmapConfiguratorShell targetShell = BakePreInfos.FindShellByConfigurator(target);
            if (targetShell.Parent != null && targetShell.Parent.Configurator != null && targetShell.Parent.Configurator.HasSerialzeDatas)
            {
                LightmapConfigurator parent = targetShell.Parent.Configurator;
                DistributeLCDataForChildren(parent, BakePreInfos);
            }
        }

        /// <summary>
        /// 分发(复制)LC烘焙数据到子LightmapConfigurator
        /// </summary>
        public static void DistributeLCDataForChildren(LightmapConfigurator target, BakeInfoPreCollector BakePreInfos)
        {

            LightmapConfiguratorShell targetShell = BakePreInfos.FindShellByConfigurator(target);

            if (targetShell.ChildrenCount == 0) 
                return;

            string lightProbesLoadPath = string.Empty;
            if (target.lightProbes != null)
            {
                lightProbesLoadPath = AssetDatabase.GetAssetPath(target.lightProbes);
            }

            List<LightmapConfigurator.LightMapDataRecorder> totalLightmapDatas = new List<LightMapDataRecorder>(target.lightMapDataRecorders);
            List<LightmapConfigurator.LightmapRecorder> totalRecorders = new List<LightmapRecorder>(target.lightmapRecorders);
            Stack<LightmapConfiguratorShell> stack = new Stack<LightmapConfiguratorShell>();

            for (int i = 0; i < targetShell.ChildrenCount; i++)
            {
                LightmapConfiguratorShell sub = targetShell[i];
                stack.Push(sub);
            }

            while (stack.Count > 0)
            {
                var shell = stack.Pop();
                var configurator = shell.Configurator;

                //lightProbes
                if (!string.IsNullOrEmpty(lightProbesLoadPath))
                {
                    configurator.lightProbes = AssetDatabase.LoadAssetAtPath<LightProbes>(lightProbesLoadPath);
                }

                //lightmapMode
                configurator.lightmapMode = target.lightmapMode;

                //LightMapDataRecorders && lightmapRecorders
                List<LightmapConfigurator.LightMapDataRecorder> lightMapDataRecorders = new List<LightMapDataRecorder>();
                List<LightmapConfigurator.LightmapRecorder> lightmapRecorderList = new List<LightmapConfigurator.LightmapRecorder>();

                List<BakeNode> bakeNodes = BakePreInfos.LightmapConfiguratorDic[shell];
                foreach (var node in bakeNodes)
                {
                    if (node.RenderType == LightmapRenderType.Render)
                    {
                        var meshRenderer = node.MeshRenderer;
                        if (CheckHasStaticEditorFlags(meshRenderer.gameObject, StaticEditorFlags.ContributeGI))
                        {
                            LightmapConfigurator.LightmapRecorder recorder = totalRecorders.Find(x => x.type == LightmapConfigurator.LightmapRenderType.Render && x.renderer == meshRenderer);
                            if (recorder != null)
                            {
                                recorder.lightmapIndex = CollectorLightMapDataRecorderInUse(recorder.lightmapIndex, totalLightmapDatas, ref lightMapDataRecorders);
                                lightmapRecorderList.Add(recorder);
                            }
                        }
                    }
                    else if (node.RenderType == LightmapRenderType.Terrain)
                    {
                        var terrain = node.Terrain;
                        if (CheckHasStaticEditorFlags(terrain.gameObject, StaticEditorFlags.ContributeGI))
                        {
                            LightmapConfigurator.LightmapRecorder recorder = totalRecorders.Find(x => x.type == LightmapConfigurator.LightmapRenderType.Terrain && x.renderer == terrain);
                            if (recorder != null)
                            {
                                recorder.lightmapIndex = CollectorLightMapDataRecorderInUse(recorder.lightmapIndex, totalLightmapDatas, ref lightMapDataRecorders);
                                lightmapRecorderList.Add(recorder);
                            }
                        }
                    }
                }

                configurator.lightMapDataRecorders = lightMapDataRecorders.ToArray();
                configurator.lightmapRecorders = lightmapRecorderList.ToArray();

                EditorUtility.SetDirty(configurator);

                //OverridesPrefabModifiy
                TryOverridesPrefabModifiy(configurator);

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

        /// <summary>
        /// 将数据改动覆盖保存到预制体上
        /// </summary>
        public static void TryOverridesPrefabModifiy(LightmapConfigurator configurator)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(configurator.gameObject) && configurator.OverridesPrefabModifiy)
            {
                GameObject nRoot = PrefabUtility.GetNearestPrefabInstanceRoot(configurator.gameObject);
                if(nRoot)
                {
                    GameObject tempIns = DuplicateInstanceRoot(nRoot);

                    //处理覆盖忽略设置
                    if(configurator.IgnoreOverridePos || configurator.IgnoreOverrideRotation || configurator.IgnoreOverrideScale)
                    {
                        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(nRoot);
                        if (!string.IsNullOrEmpty(prefabPath))
                        {
                            GameObject prefabAsset = PrefabUtility.LoadPrefabContents(prefabPath);
                            if (prefabAsset)
                            {
                                if (configurator.IgnoreOverridePos)
                                    tempIns.transform.localPosition = prefabAsset.transform.localPosition;
                                if (configurator.IgnoreOverrideRotation)
                                    tempIns.transform.localRotation = prefabAsset.transform.localRotation;
                                if (configurator.IgnoreOverrideScale)
                                    tempIns.transform.localScale = prefabAsset.transform.localScale;
                                PrefabUtility.UnloadPrefabContents(prefabAsset);
                            }
                        }
                    }

                    //PrefabUtility.ApplyObjectOverride(pIns, prefabPath, InteractionMode.AutomatedAction); //报错
                    PrefabUtility.ApplyPrefabInstance(tempIns, InteractionMode.AutomatedAction);
                    //删除临时对象
                    if (Application.isPlaying)
                        GameObject.Destroy(tempIns);
                    else
                        GameObject.DestroyImmediate(tempIns);
                    AssetDatabase.Refresh();
                }
            }

        }

        /// <summary>
        /// 利用Editor的DuplicateAPI实现对insRoot的复制
        /// (不要问我为毛需要复制...当你遇到预制体嵌套你就明白了)
        /// </summary>
        private static GameObject DuplicateInstanceRoot(GameObject insRoot)
        {
            Selection.activeGameObject = insRoot;
            Unsupported.DuplicateGameObjectsUsingPasteboard();
            GameObject tempIns = Selection.activeGameObject;
            Selection.activeGameObject = null;
            tempIns.name = insRoot.name;
            return tempIns;
        }

        /// <summary>
        /// 将场景中所有LightmapConfigurator的烘焙结果应用到场景中
        /// </summary>
        [MenuItem("Engine/LightmapConfigurator/ApplyBakedDatasToScene")]
        public static void ApplyBakedDatasToSceneEx()
        {
            ApplyBakedDatasToScene(null);
        }

        [Obsolete("废弃")]
        public static void ApplyBakedDatasToScene(bool dialog, Action finishCallback)
        {
            ApplyBakedDatasToScene(finishCallback);
        }

        //旧实现
        //public static void ApplyBakedDatasToScene(bool dialog, Action finishCallback)
        //{

        //    MethodInfo InitMethod = null;
        //    if (!Application.isPlaying)
        //        InitMethod = typeof(LightmapConfigurator).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

        //    List<LightmapConfigurator> configuratorList = new List<LightmapConfigurator>();
        //    GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
        //    foreach (var root in rootObjs)
        //    {
        //        LightmapConfigurator[] configurators = root.GetComponentsInChildren<LightmapConfigurator>();
        //        foreach (var configurator in configurators)
        //        {
        //            if (IsRootLightmapConfigurator(configurator) && configurator.HasSerialzeDatas)
        //            {
        //                configuratorList.Add(configurator);
        //            }
        //        }
        //    }

        //    if (configuratorList.Count == 0) return;

        //    //refreshLightmapSettings
        //    var m_managedRefKeyDic = new Dictionary<int, int>();
        //    List<LightmapData> m_managedLightmapDatas;
        //    if (dialog)
        //    {
        //        if (EditorUtility.DisplayDialog("提示","是否保留原有烘焙贴图？", "是", "否"))
        //        {
        //            m_managedLightmapDatas =
        //                (LightmapSettings.lightmaps != null && LightmapSettings.lightmaps.Length > 0)
        //                    ? new List<LightmapData>(LightmapSettings.lightmaps)
        //                    : new List<LightmapData>();
        //        }
        //        else
        //        {
        //            m_managedLightmapDatas = new List<LightmapData>();
        //        }
        //    }
        //    else
        //    {
        //        m_managedLightmapDatas = new List<LightmapData>();
        //    }


        //    LightProbes m_managedLightProbes = null;
        //    int len = configuratorList.Count;
        //    for (int i = 0; i < len; i++)
        //    {
        //        LightmapConfigurator lcr = configuratorList[i];
        //        if (i == 0)
        //        {
        //            LightmapSettings.lightmapsMode = lcr.lightmapMode;
        //            m_managedLightProbes = lcr.lightProbes;
        //        }

        //        if (lcr.lightMapDataRecorders != null && lcr.lightMapDataRecorders.Length > 0)
        //        {
        //            int jlen = lcr.lightMapDataRecorders.Length;
        //            int[] localRefkeys = new int[jlen];

        //            if (!Application.isPlaying)
        //            {
        //                InitMethod.Invoke(lcr, null);
        //            }

        //            for (int j = 0; j < jlen; j++)
        //            {
        //                int key = lcr.LightmapDataKeys[j];
        //                if (!m_managedRefKeyDic.ContainsKey(key))
        //                {
        //                    localRefkeys[j] = m_managedLightmapDatas.Count;
        //                    LightmapData lightmapData = new LightmapData();
        //                    lcr.lightMapDataRecorders[j].Restore(lightmapData);
        //                    m_managedLightmapDatas.Add(lightmapData);
        //                    m_managedRefKeyDic.Add(key, j);
        //                }
        //                else
        //                {
        //                    localRefkeys[j] = m_managedRefKeyDic[key];
        //                }
        //            }

        //            lcr.UpdateRenderersLightmapSetting(localRefkeys);
        //        }
        //    }

        //    LightmapSettings.lightmaps = m_managedLightmapDatas.ToArray();
        //    LightmapSettings.lightProbes = m_managedLightProbes;

        //    finishCallback?.Invoke();
        //}

        public static void ApplyBakedDatasToScene(Action finishCallback)
        {

            //MethodInfo InitMethod = null;
            //if (!Application.isPlaying)
            //    InitMethod = typeof(LightmapConfigurator).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

            List<LightmapConfigurator> configuratorList = new List<LightmapConfigurator>();
            GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in rootObjs)
            {
                LightmapConfigurator[] configurators = root.GetComponentsInChildren<LightmapConfigurator>();
                foreach (var configurator in configurators)
                {
                    if (configurator.enabled && configurator.HasSerialzeDatas)
                    {
                        configuratorList.Add(configurator);
                    }
                }
            }
            if (configuratorList.Count == 0) return;

            LightmapConfiguratorManager.Instance.Reset();

            foreach (var configurator in configuratorList)
            {
                LightmapConfiguratorManager.Instance.Register(configurator);
            }

            finishCallback?.Invoke();
        }

    }
}
