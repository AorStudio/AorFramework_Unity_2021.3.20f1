using System;
using UnityEditor;
using UnityEngine.Rendering.Universal.Editor.Utility;

namespace UnityEngine.Rendering.Universal.LightmapConfigurator.Editor
{
    /// <summary>
    /// Author : Aorition
    /// Update : 2023-06-27
    /// </summary>
    [CustomEditor(typeof(LightmapConfigurator))]
    public class LightmapConfiguratorEditor : UnityEditor.Editor
    {
        
        private GUIContent m_ShowSerializeFields_GCT = new GUIContent("显示序列化数据");
        //private GUIContent m_ExcludeOtherBakeObjects_GCT = new GUIContent("排除其他烘焙对象");
        private GUIContent m_NormalizationLightmapFileName_GCT = new GUIContent("规格化Lightmap文件名称");
        private GUIContent m_UseTargetPrefabPathToSaveLightmapAssets_GCT = new GUIContent("使用目标预制体路径保存Lightmap资源");
        private GUIContent m_SaveLightmapAssetsDirPath_GCT = new GUIContent("指定保存Lightmap资源保存路径");
        private GUIContent m_OverridesPrefabModifiy_GCT = new GUIContent("烘焙完成后自动将数据变动保存到所属预制体");
        private GUIContent m_IgnoreOverridePos_GCT = new GUIContent("忽略位置保存", "保存预制体时将忽略位置信息保存");
        private GUIContent m_IgnoreOverrideRotation_GCT = new GUIContent("忽略旋转保存", "保存预制体时将忽略旋转信息保存");
        private GUIContent m_IgnoreOverrideScale_GCT = new GUIContent("忽略缩放保存", "保存预制体时将忽略缩放信息保存");
        //private GUIContent m_UsingEOTagToIdentifyHelperBakedObjects_GCT = new GUIContent("使用\"Editor Only\"Tag识别辅助烘焙对象");
        private GUIContent m_ShowBakeObjectBounds_GCT = new GUIContent("显示烘焙对象Bounds");
        private GUIContent m_ShowMergedBounds_GCT = new GUIContent("显示合并Bounds");
        private GUIContent m_BakeButton_GCT = new GUIContent("Baking Lightmap Datas", "烘焙当前LightmapConfigurator");
        private GUIContent m_RecordDataButton_GCT = new GUIContent("Record Scene Lightmap Datas", "记录当前场景烘焙数据");
        private GUIContent m_ClearSceneDataButton_GCT = new GUIContent("Clear Scene Lightmap Datas", "清理当前场景烘焙数据");
        private GUIContent m_AutoShadowCastRules_GCT = new GUIContent("自动ShadowCast处理规则", "烘焙时使用规则处理烘焙对象的ShadowCast");
        private GUIContent m_selectParent_GCT = new GUIContent("选择上层Configurator对象", "选择上层Configurator对象");
        private GUIContent m_GetLCDataFromParent_GCT = new GUIContent("向上获取烘焙数据", "此方法只能向上一级获取LC烘焙数据，请确保上级LC烘焙数据已经包含当前节点已下的所有烘焙数据");
        private GUIContent m_DistributeLCDataForChildren_GCT = new GUIContent("分发(复制)烘焙数据到下级", "分发(复制)烘焙数据到下级Configurator对象，请确保当前对象已有包含下级节点的烘焙数据");

        private LightmapConfigurator m_target;

        private bool m_hasSerialzeDatas;
        private bool m_ShowSerializeFields;
        //private bool m_isBLRunning;
        //private bool m_needUnlockInswindow;

        private bool m_isSubConfigurator;
        private bool m_hasSubConfigurators;
        private LightmapConfigurator m_parent;

        private void Awake()
        {
            m_target = target as LightmapConfigurator;

        }

        public override void OnInspectorGUI()
        {

            //防止脚本重复
            if (m_target && m_target.gameObject)
                LightmapConfigurator.CheckDuplicates(m_target.gameObject);

            m_hasSerialzeDatas = m_target.HasSerialzeDatas;
            m_isSubConfigurator = LightmapConfiguratorBakeUtility.IsSubLightmapConfigurator(m_target, out m_parent);
            m_hasSubConfigurators = LightmapConfiguratorBakeUtility.HasSubLightmapConfigurator(m_target);

            drawInfoUI();

            GUILayout.Space(5);

            drawFurthersToolUI();

            GUILayout.Space(5);

            drawGizmosSettingUI();

            GUILayout.Space(5);

            if (!m_isSubConfigurator) 
            { 
                drawBCsettingUI();
                GUILayout.Space(5);
            }

            if (drawOptionsUI())
                EditorUtility.SetDirty(m_target);

            GUILayout.Space(5);

            drawBakingButtonUI();

            if (Lightmapping.isRunning)
                this.Repaint();
        }
        private void drawFurthersToolUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("常用指令 >");
                GUILayout.Space(5);

                //Todo ...

                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("搜索内部所有未激活的对象"))
                        {
                            HierarchySearchToolWindow.Init("{activeSelf=false}", m_target.gameObject);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("搜索内部所有LODGroup"))
                        {
                            HierarchySearchToolWindow.Init("t:LODGroup", m_target.gameObject);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("搜索内部所有不产生阴影的对象"))
                        {
                            HierarchySearchToolWindow.Init("t:MeshRenderer{castShadows=false}", m_target.gameObject);
                        }
                    }
                    GUILayout.EndHorizontal();

                    //Todo ... 反射能够正常执行，但是没有执行效果 ！！！！
                    //GUILayout.BeginHorizontal();
                    //{
                    //    if (GUILayout.Button("内部所有LODGroup重新计算ScaleInLightmap"))
                    //    {
                    //        var list = SearchCommandUtility.ExecSearchCommand("t:LODGroup", m_target.gameObject);
                    //        if(list.Count > 0)
                    //        {
                    //            Type LODGroupEditorType = null;
                    //            Type[] types = typeof(UnityEditor.Editor).Assembly.GetTypes();
                    //            foreach (Type t in types)
                    //            {
                    //                if(t.Name == "LODGroupEditor")
                    //                {
                    //                    LODGroupEditorType = t;
                    //                    break;
                    //                }
                    //            }
                    //            MethodInfo MethodInfo_SendPercentagesToLightmapScale = LODGroupEditorType.GetMethod("SendPercentagesToLightmapScale", BindingFlags.Instance | BindingFlags.NonPublic);

                    //            foreach (var item in list)
                    //            {
                    //                UnityEngine.LODGroup group = item.GetComponent<UnityEngine.LODGroup>();
                    //                if (group)
                    //                {
                    //                    //UnityEditor.LODUtility.CalculateLODGroupBoundingBox(group);
                    //                    UnityEditor.Editor editorIns = UnityEditor.Editor.CreateEditor(group, LODGroupEditorType);
                    //                    if(editorIns.serializedObject != null)
                    //                    {
                    //                        MethodInfo_SendPercentagesToLightmapScale.Invoke(editorIns, null);
                    //                    }
                    //                    //UnityEditor.Editor.DestroyImmediate(editorIns);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void drawGizmosSettingUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("Gizmos 设置 >");
                GUILayout.Space(5);


                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    {
                        m_target.ShowBakeObjectBounds = EditorGUILayout.ToggleLeft(m_ShowBakeObjectBounds_GCT, m_target.ShowBakeObjectBounds);
                        if (m_target.ShowBakeObjectBounds)
                            m_target.ShowMergedBounds = EditorGUILayout.ToggleLeft(m_ShowMergedBounds_GCT, m_target.ShowMergedBounds);
                    }
                    GUILayout.EndHorizontal();
                    if (m_target.ShowBakeObjectBounds)
                    {
                        m_target.BakeObjectBoundsColor = EditorGUILayout.ColorField("烘焙 对象", m_target.BakeObjectBoundsColor);
                        m_target.ZeroScaleInLightmapBoundsColor = EditorGUILayout.ColorField("SIL Zero 对象", m_target.ZeroScaleInLightmapBoundsColor);
                        m_target.NonBakeBoundsColor = EditorGUILayout.ColorField("非烘焙 对象", m_target.NonBakeBoundsColor);
                    }
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void drawInfoUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("基本信息 >");
                GUILayout.Space(5);
                GUILayout.Label("LightmapMode :" + m_target.lightmapMode.ToString());
                GUILayout.Space(5);
                if (m_hasSerialzeDatas)
                {
                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("序列化数据 :");
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("C", GUILayout.Width(22)))
                            {
                                if(EditorUtility.DisplayDialog("提示","确定清除已序列化的数据?! (该操作无法回退)", "确定", "取消"))
                                {
                                    m_target.ClearSerialzeDatas();
                                    m_hasSerialzeDatas = false;
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                        if (m_hasSerialzeDatas)
                        {
                            GUILayout.Label($"\t LightmapDataRecorders : {m_target.lightMapDataRecorders.Length}");
                            GUILayout.Label($"\t LightmapRecorders : {m_target.lightmapRecorders.Length}");
                            if (m_target.lightProbes != null)
                            {
                                GUILayout.Label($"\t lightProbes : ");
                                GUILayout.Label($"\t\t Light probe count: {m_target.lightProbes.count}");
                                GUILayout.Label($"\t\t Cell count: {m_target.lightProbes.cellCount}");
                            }
                            GUILayout.Space(5);
                        }
                    }
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUI.color = Color.gray;
                            GUILayout.Label("尚无 <Lightmap序列化> 数据");
                            GUI.color = Color.white;
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                {
                    GUI.color = m_hasSerialzeDatas ? Color.white : Color.gray;
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;
                    m_ShowSerializeFields = EditorGUILayout.Foldout(m_ShowSerializeFields, m_ShowSerializeFields_GCT);
                    GUILayout.Space(5);
                    GUI.color = Color.white;
                    if (m_ShowSerializeFields)
                    {
                        GUILayout.Space(5);
                        EditorGUI.indentLevel++;
                        base.OnInspectorGUI();
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void drawBCsettingUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("烘焙参数备份工具 >");
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    //LoadBLSetting
                    if (GUILayout.Button("加载烘焙参数(.bls)"))
                    {
                        BakeLightingSettingDataStorage.LoadJSONAndApplyToEnv();
                    }
                    //SaveBLSetting
                    if (GUILayout.Button("保存烘焙参数(.bls)"))
                    {
                        BakeLightingSettingDataStorage.CreateAndSaveToJSON();
                    }
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("设置烘焙参数"))
                {
                    Type[] types = typeof(UnityEditor.Editor).Assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if(type.FullName == "UnityEditor.LightingWindow")
                        {
                            EditorWindow ew = EditorWindow.GetWindow(type);
                            ew.Show(true);
                            break;
                        }
                    }
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private bool drawOptionsUI()
        {
            bool dirty = false;
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("选项设置 >");
                GUILayout.Space(5);

                //自动ShadowCast处理规则
                bool nAutoShadowCastRules = EditorGUILayout.ToggleLeft(m_AutoShadowCastRules_GCT, m_target.AutoShadowCastRules);
                if(m_target.AutoShadowCastRules != nAutoShadowCastRules)
                {
                    m_target.AutoShadowCastRules = nAutoShadowCastRules;
                    dirty = true;
                }

                //使用目标预制体路径保存Lightmap资源
                bool nUseTargetPrefabPathToSaveLightmapAssets = m_isSubConfigurator ? m_parent.UseTargetPrefabPathToSaveLightmapAssets
                    : EditorGUILayout.ToggleLeft(m_UseTargetPrefabPathToSaveLightmapAssets_GCT, m_target.UseTargetPrefabPathToSaveLightmapAssets);
                if (m_target.UseTargetPrefabPathToSaveLightmapAssets != nUseTargetPrefabPathToSaveLightmapAssets)
                {
                    m_target.UseTargetPrefabPathToSaveLightmapAssets = nUseTargetPrefabPathToSaveLightmapAssets;
                    dirty = true;
                }
                if (!m_isSubConfigurator && !m_target.UseTargetPrefabPathToSaveLightmapAssets)
                {
                    GUILayout.BeginHorizontal();
                    {
                        string nSaveLightmapAssetsDirPath = EditorGUILayout.TextField(m_SaveLightmapAssetsDirPath_GCT, m_target.SaveLightmapAssetsDirPath);
                        if (GUILayout.Button("S", GUILayout.Width(26)))
                        {
                            nSaveLightmapAssetsDirPath = LightmapConfiguratorBakeUtility.SaveFolderPanel(m_target);
                        }
                        if(m_target.SaveLightmapAssetsDirPath != nSaveLightmapAssetsDirPath)
                        {
                            m_target.SaveLightmapAssetsDirPath = nSaveLightmapAssetsDirPath;
                            dirty = true;
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                //规格化Lightmap文件名称
                bool nNormalizationLightmapFileName = m_isSubConfigurator ? false : EditorGUILayout.ToggleLeft(m_NormalizationLightmapFileName_GCT, m_target.NormalizationLightmapFileName);
                if(m_target.NormalizationLightmapFileName != nNormalizationLightmapFileName)
                {
                    m_target.NormalizationLightmapFileName = nNormalizationLightmapFileName;
                    dirty = true;
                }

                //烘焙完成后自动将数据变动保存到所属预制体
                bool nOverridesPrefabModifiy = EditorGUILayout.ToggleLeft(m_OverridesPrefabModifiy_GCT, m_target.OverridesPrefabModifiy);
                if (m_target.OverridesPrefabModifiy != nOverridesPrefabModifiy) 
                {
                    m_target.OverridesPrefabModifiy = nOverridesPrefabModifiy;
                    dirty = true;
                }

                if (nOverridesPrefabModifiy)
                {
                    EditorGUI.indentLevel++;
                    m_target.IgnoreOverridePos = EditorGUILayout.ToggleLeft(m_IgnoreOverridePos_GCT, m_target.IgnoreOverridePos);
                    m_target.IgnoreOverrideRotation = EditorGUILayout.ToggleLeft(m_IgnoreOverrideRotation_GCT, m_target.IgnoreOverrideRotation);
                    m_target.IgnoreOverrideScale = EditorGUILayout.ToggleLeft(m_IgnoreOverrideScale_GCT, m_target.IgnoreOverrideScale);
                    EditorGUI.indentLevel--;
                }


                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
            return dirty;
        }

        private void drawBakingButtonUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                if (Lightmapping.isRunning)
                {
                    if (LightmapConfiguratorBakeUtility.CurrentProcessor != null && LightmapConfiguratorBakeUtility.CurrentProcessor.target.Configurator == m_target)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            drawBakingProcessUI(Lightmapping.buildProgress, Screen.width * 0.6f);
                            GUILayout.FlexibleSpace();
                            GUI.color = Color.yellow;
                            if (GUILayout.Button("Cannel", GUILayout.Width(80), GUILayout.Height(24)))
                            {
                                if (EditorUtility.DisplayDialog("警告", "是否中断在执行的Lightmap烘焙进程?", "确定", "取消"))
                                {
                                    LightmapConfiguratorBakeUtility.BakingCannel();
                                }
                            }
                            GUI.color = Color.white;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {

                    if (m_isSubConfigurator)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(m_selectParent_GCT, GUILayout.Height(28)))
                            {
                                Selection.activeGameObject = m_parent.gameObject;
                                EditorGUIUtility.PingObject(Selection.activeGameObject);
                            }

                            if (GUILayout.Button("[Batch Baking]", GUILayout.Height(28), GUILayout.Width(120)))
                            {
                                LightmapConfiguratorBakeWindow.Init();
                            }

                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(m_BakeButton_GCT, GUILayout.Height(28)))
                            {
                                if (EditorUtility.DisplayDialog("提示", "确认开始烘焙此LightmapConfigurator?!", "确认", "取消"))
                                {
                                    LightmapConfiguratorBakeUtility.BakingTarget(m_target, () =>
                                    {
                                        EditorUtility.DisplayDialog("提示", "烘焙Lightmap完成!", "OK");
                                    });
                                }
                            }

                            if (GUILayout.Button("[Batch Baking]", GUILayout.Height(28), GUILayout.Width(120)))
                            {
                                LightmapConfiguratorBakeWindow.Init();
                            }

                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    {

                        if (m_isSubConfigurator)
                        {
                            if (m_parent.HasSerialzeDatas)
                            {
                                if (GUILayout.Button(m_GetLCDataFromParent_GCT, GUILayout.Height(22)))
                                {
                                    if (EditorUtility.DisplayDialog("提示", "确认向上获取烘焙数据?!", "确认", "取消"))
                                    {
                                        BakeInfoPreCollector bakePreInfos = new BakeInfoPreCollector();
                                        LightmapConfiguratorBakeUtility.CopyLCDataFormParentLightmapConfigurator(m_target, bakePreInfos);
                                        bakePreInfos.Dispose();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(m_RecordDataButton_GCT, GUILayout.Height(22)))
                            {
                                if (EditorUtility.DisplayDialog("提示", "确认直接记录当前场景烘焙数据?!", "确认", "取消"))
                                {
                                    LightmapConfiguratorBakeUtility.RecordLightmapDatasInLightmapConfigurator(m_target, LightmapConfiguratorBakeUtility.UpdateSaveDirPathForConfigurator(m_target));
                                }
                            }
                        }
                        if (GUILayout.Button(m_ClearSceneDataButton_GCT, GUILayout.Height(22)))
                        {
                            if (EditorUtility.DisplayDialog("提示", "确认清理当前场景烘焙数据?!", "确认", "取消"))
                            {
                                LightmapConfiguratorBakeUtility.ClearDatasInLightmapSettings();
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (m_hasSerialzeDatas && m_hasSubConfigurators)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(m_DistributeLCDataForChildren_GCT, GUILayout.Height(28)))
                            {
                                if (EditorUtility.DisplayDialog("提示", "确认分发(复制)烘焙数据到下级?!", "确认", "取消"))
                                {
                                    BakeInfoPreCollector bakePreInfos = new BakeInfoPreCollector();
                                    LightmapConfiguratorBakeUtility.DistributeLCDataForChildren(m_target, bakePreInfos);
                                    bakePreInfos.Dispose();
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void drawBakingProcessUI(float process, float width)
        {
            process = Mathf.Clamp01(process);
            GUI.color = Color.gray;
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(15));
            }
            GUILayout.EndHorizontal();
            Rect rect = GUILayoutUtility.GetLastRect();
            float pw = rect.width * process;
            GUI.color = Color.green;
            GUI.Box(new Rect(rect.position, new Vector2(pw, rect.size.y)), "");
            GUI.color = Color.green;
            float pv = process * 100;
            GUI.Box(rect, $"Baking Lightmap ... ({pv.ToString("F2")}%)");
            GUI.color = Color.white;
        }

    }

}