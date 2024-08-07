using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace UnityEngine.Rendering.Universal.LightmapConfigurator.Editor
{
    /// <summary>
    /// Author : Aorition
    /// Update : 2023-06-27
    /// </summary>
    public class LightmapConfiguratorBakeWindow : UnityEditor.EditorWindow
    {

        private static LightmapConfiguratorBakeWindow m_Instance;

        [MenuItem("Engine/LightmapConfigurator/BatchBakingWindow")]
        public static LightmapConfiguratorBakeWindow Init()
        {
            m_Instance = UnityEditor.EditorWindow.GetWindow<LightmapConfiguratorBakeWindow>();
            m_Instance.titleContent = new GUIContent("LC烘焙管理器");
            return m_Instance;
        }

        //--------------------------------------------------

        private void OnDestroy()
        {
            m_ShellRefDic.Clear();
            m_lcFoldDic.Clear();
            m_lcFoldDic_subNodes.Clear();
            m_lcFoldDic_subLC.Clear();
            //m_LCSStack.Clear();
        }

        private void OnGUI()
        {

            GUILayout.Space(5);

            Draw_titleUI("LightmapConfigurator 批量烘焙管理器");

            GUILayout.Space(5);

            if (LightmapConfiguratorBakeUtility.BakePreInfos == null)
            {
                //未烘焙
                Draw_mainUI();
            }
            else
            {
                Draw_BakingProcessUI();
            }
            if(Lightmapping.isRunning)
                Repaint();
        }

        private void Draw_titleUI(string title)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(title);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private Vector2 m_mainScrollPos;
        private void Draw_mainUI()
        {

            CollectRootLightmapConfiguratorShells();

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label($"场景中找到{rootLightmapConfiguratorShells.Count}个LightmapConfiguratorShell");
                m_mainScrollPos = GUILayout.BeginScrollView(m_mainScrollPos);
                {
                    foreach (var shell in rootLightmapConfiguratorShells)
                    {
                        Draw_LightmapConfiguratorUI(shell);
                    }

                }
                GUILayout.EndScrollView();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                if(rootLightmapConfiguratorShells.Count > 0)
                {
                    if (GUILayout.Button("开始批量烘焙Lightmap数据",GUILayout.Height(26)))
                    {
                        if(EditorUtility.DisplayDialog("提示","确认开始批量烘焙?!", "确认", "取消"))
                        {
                            LightmapConfiguratorBakeUtility.BatchBaking(() =>
                            {
                                EditorUtility.DisplayDialog("提示", "批量烘焙Lightmap完成!", "OK");
                            });
                        }
                    }
                }
                else
                {
                    GUI.color = Color.gray;
                    if (GUILayout.Button("开始批量烘焙Lightmap数据", GUILayout.Height(26)))
                    {
                        //do nothing ...
                    }
                    GUI.color = Color.white;
                }
            }
            GUILayout.EndVertical();

        }

        private readonly Dictionary<LightmapConfigurator, bool> m_lcFoldDic = new Dictionary<LightmapConfigurator, bool>();
        private readonly Dictionary<LightmapConfigurator, bool> m_lcFoldDic_subNodes = new Dictionary<LightmapConfigurator, bool>();
        private readonly Dictionary<LightmapConfigurator, bool> m_lcFoldDic_subLC = new Dictionary<LightmapConfigurator, bool>();

        //private readonly Stack<LightmapConfiguratorShell> m_LCSStack = new Stack<LightmapConfiguratorShell>();

        private void Draw_LightmapConfiguratorUI(LightmapConfiguratorShell shell)
        {
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical("box");
            {
                LightmapConfigurator configurator = shell.Configurator;

                if (!m_lcFoldDic.ContainsKey(configurator))
                {
                    m_lcFoldDic.Add(configurator, false);
                    m_lcFoldDic_subNodes.Add(configurator, false);
                    m_lcFoldDic_subLC.Add(configurator, false);
                }

                GUILayout.BeginHorizontal();
                {
                    m_lcFoldDic[configurator] = EditorGUILayout.Foldout(m_lcFoldDic[configurator], $"{configurator.name}{(!configurator.enabled ? "\t(无烘焙)" : "")}");
                    if (GUILayout.Button(">", GUILayout.Width(22)))
                    {
                        Selection.activeGameObject = configurator.gameObject;
                    }
                }
                GUILayout.EndHorizontal();
                if (m_lcFoldDic[configurator])
                {
                    EditorGUI.indentLevel++;
                    List<MeshRenderer> mrs = new List<MeshRenderer>();
                    List<Terrain> ters = new List<Terrain>();
                    CollectNodesInLightmapConfigurator(configurator, out int nodeCounts, ref mrs, ref ters);
                    //EditorGUILayout.LabelField($"Sub Nodes [{nodeCounts}]");
                    m_lcFoldDic_subNodes[configurator] = EditorGUILayout.Foldout(m_lcFoldDic_subNodes[configurator], $"Sub Nodes [{nodeCounts}]");
                    if (m_lcFoldDic_subNodes[configurator])
                    {
                        EditorGUI.indentLevel++;
                        int idx = 0;
                        foreach (var r in mrs)
                        {
                            Draw_SubNodeInfoUI(idx, r.gameObject, r.scaleInLightmap);
                            idx++;
                        }
                        foreach (var t in ters)
                        {
                            float sil = LightmapConfiguratorBakeUtility.GetTerrainScaleInLightmap(t);
                            Draw_SubNodeInfoUI(idx, t.gameObject, sil);
                            idx++;
                        }
                        EditorGUI.indentLevel--;
                    }
                    if (shell.ChildrenCount > 0)
                    {
                        m_lcFoldDic_subLC[configurator] = EditorGUILayout.Foldout(m_lcFoldDic_subLC[configurator], $"Sub Configurators [{shell.ChildrenCount}]");
                        if (m_lcFoldDic_subLC[configurator])
                        {
                            for (int i = 0; i < shell.ChildrenCount; i++)
                            {
                                LightmapConfiguratorShell subShell = shell[i];
                                Draw_LightmapConfiguratorUI(subShell);
                            }
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        private void Draw_SubNodeInfoUI(int index, GameObject node, float scaleInLightmap)
        {
            bool isBake = LightmapConfiguratorBakeUtility.CheckHasStaticEditorFlags(node, StaticEditorFlags.ContributeGI);
            GUI.color = ((scaleInLightmap > 0 && isBake) ? Color.white : (isBake ? Color.yellow : Color.grey));
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField($"[{index}]\t({scaleInLightmap})", node.name);
                if(GUILayout.Button(">", GUILayout.Width(22)))
                {
                    Selection.activeGameObject = node;
                    EditorGUIUtility.PingObject(node);
                }
            }
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private Vector2 m_BakingProcessScrollPos;
        private void Draw_BakingProcessUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"... 正在批量烘焙作业中 ...");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                m_BakingProcessScrollPos = GUILayout.BeginScrollView(m_BakingProcessScrollPos);
                {
                    for (int i = 0; i < LightmapConfiguratorBakeUtility.BatchBakingList.Count; i++)
                    {
                        LightmapConfigurator configurator = LightmapConfiguratorBakeUtility.BatchBakingList[i].Configurator;
                        if (i < LightmapConfiguratorBakeUtility.BatchBakingIndex)
                        {
                            GUI.color = Color.gray;
                            GUILayout.BeginHorizontal("box");
                            {
                                EditorGUILayout.LabelField($"[已完成]\t{configurator.name}");
                            }
                            GUILayout.EndHorizontal();
                            GUI.color = Color.white;
                        }
                        else if (i == LightmapConfiguratorBakeUtility.BatchBakingIndex)
                        {
                            GUI.color = Color.white;
                            GUILayout.BeginVertical("helpBox");
                            {
                                EditorGUILayout.LabelField(configurator.name);
                                DrawBakingProcessUI(Lightmapping.buildProgress, Screen.width - 39);
                            }
                            GUILayout.EndVertical();
                            GUI.color = Color.white;
                        }
                        else
                        {
                            GUILayout.BeginHorizontal("box");
                            {
                                EditorGUILayout.LabelField( $"[等待ing]\t{configurator.name}");
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                if (GUILayout.Button("取消批量烘焙作业", GUILayout.Height(26)))
                {
                    if (EditorUtility.DisplayDialog("提示", "确认取消批量烘焙作业?!", "确认", "取消"))
                    {
                        LightmapConfiguratorBakeUtility.BakingCannel();
                    }
                }
            }
            GUILayout.EndVertical();

        }


        //----------------------------

        private void DrawBakingProcessUI(float process, float width)
        {
            process = Mathf.Clamp01(process);
            GUI.color = Color.white;
            GUI.backgroundColor = Color.gray;
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(15));
            }
            GUILayout.EndHorizontal();
            Rect rect = GUILayoutUtility.GetLastRect();
            float pw = rect.width * process;
            GUI.color = Color.green;
            GUI.backgroundColor = Color.green;
            GUI.Box(new Rect(rect.position, new Vector2(pw, rect.size.y)), "");
            float pv = process * 100;
            GUI.color = Color.green;
            GUI.backgroundColor = Color.white;
            GUI.Box(rect, $"{pv.ToString("F2")}%");
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }

        private readonly Dictionary<LightmapConfigurator, LightmapConfiguratorShell> m_ShellRefDic = new Dictionary<LightmapConfigurator, LightmapConfiguratorShell>();
        private readonly List<LightmapConfiguratorShell> rootLightmapConfiguratorShells = new List<LightmapConfiguratorShell>();
       
        private void CollectRootLightmapConfiguratorShells()
        {
            rootLightmapConfiguratorShells.Clear();
            GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in rootObjs)
            {
                LightmapConfigurator[] configurators = root.GetComponentsInChildren<LightmapConfigurator>();
                foreach (var configurator in configurators)
                {

                    if (!configurator.enabled)
                        continue;

                    LightmapConfiguratorShell shell;
                    if (!m_ShellRefDic.ContainsKey(configurator))
                    {
                        shell = new LightmapConfiguratorShell(configurator);
                        m_ShellRefDic.Add(configurator, shell);
                    }
                    else
                    {
                        shell = m_ShellRefDic[configurator];
                    }

                    if (LightmapConfiguratorBakeUtility.IsSubLightmapConfigurator(configurator, out var parent))
                    {
                        shell.SetParent(m_ShellRefDic[parent]);
                    }
                    else
                    {
                        rootLightmapConfiguratorShells.Add(shell);
                    }
                }
            }
        }

        private void CollectNodesInLightmapConfigurator(LightmapConfigurator configurator, out int counts, ref List<MeshRenderer> meshRendererList, ref List<Terrain> terrainList)
        {
            counts = 0;
            if (meshRendererList == null)
                meshRendererList = new List<MeshRenderer>();
            else
                meshRendererList.Clear();
            if (terrainList == null)
                terrainList = new List<Terrain>();
            else
                terrainList.Clear();

            //MeshRenderer[] renderers = configurator.GetComponentsInChildren<MeshRenderer>();
            //foreach (var r in renderers)
            //{
            //    meshRendererList.Add(r);
            //    counts++;
            //}

            //Terrain[] terrains = configurator.GetComponentsInChildren<Terrain>();
            //foreach (var t in terrains)
            //{
            //    terrainList.Add(t);
            //    counts++;
            //}

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(configurator.transform);
            while (stack.Count > 0)
            {
                Transform n = stack.Pop();
                if (LightmapConfiguratorBakeUtility.CheckHasStaticEditorFlags(n.gameObject, StaticEditorFlags.ContributeGI))
                {

                    MeshRenderer meshRenderer = n.GetComponent<MeshRenderer>();
                    if (meshRenderer)
                    {
                        meshRendererList.Add(meshRenderer);
                        counts++;
                    }

                    Terrain terrain = n.GetComponent<Terrain>();
                    if (terrain)
                    {
                        terrainList.Add(terrain);
                        counts++;
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
                        if(subLC && subLC.enabled)
                            continue;

                        stack.Push(sub);
                    }
                }

            }

        }

    }
}
