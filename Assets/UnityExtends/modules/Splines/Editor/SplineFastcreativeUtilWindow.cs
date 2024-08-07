using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if FRAMEWORKDEF
#else

#endif
namespace UnityEngine.Rendering.Universal.module.Editor
{
    public class SplineFastcreativeUtilWindow :EditorWindow
    {

        private static SplineFastcreativeUtilWindow m_instance;

        private static string[] m_toolbarLabels = new string[] { "布点生成", "桥接生成", "合并生成" };

        public static SplineFastcreativeUtilWindow init(Spline target)
        {
            m_instance = EditorWindow.GetWindow<SplineFastcreativeUtilWindow>("SplineFastCreativeUtil");
            m_instance.target = target;
            return m_instance;
        }

        public Spline target;

        private int m_toolIndex;
        private Rect m_DragArea;

        private void OnGUI()
        {

            GUILayout.Space(5);

            _draw_title_UI();

            GUILayout.Space(5);

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("生成目标");
                target = (Spline)EditorGUILayout.ObjectField(target, typeof(Spline), true);
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);

            bool reset = false;
            int newTIdx = GUILayout.Toolbar(m_toolIndex, m_toolbarLabels);
            if(newTIdx != m_toolIndex)
            {
                m_toolIndex = newTIdx;
                reset = true;
            }

            switch(m_toolIndex)
            {

                case 1:
                    {
                        if(reset)
                            GenerationOfDistributionPointsReset();
                        GenerationOfBridgeSplines_UI();
                    }
                    break;
                case 2:
                    {
                        if(reset)
                            MergeGenerationReset();
                        _draw_MergeGeneration_UI();
                    }
                    break;
                default: //0
                    {
                        if(reset)
                            GenerationOfSplicingSplinesReset();
                        _draw_GenerationOfDistributionPoints_UI();
                    }
                    break;
            }

            GUILayout.Space(5);

            //handle DragAndDrop
            switch(Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                if(!m_DragArea.Contains(Event.current.mousePosition))
                {
                    break;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if(Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    for(int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                    {
                        UnityEngine.Object temp = DragAndDrop.objectReferences[i];
                        if(temp != null && temp is GameObject)
                        {
                            GameObject go = temp as GameObject;
                            switch(m_toolIndex)
                            {
                                case 2:
                                    {
                                        Spline sp = go.GetComponent<Spline>();
                                        if(sp)
                                            m_MRD_splineList.Add(sp);
                                    }
                                    break;
                                default: //0
                                    {
                                        m_GODP_pointList.Add(go.transform);
                                    }
                                    break;
                            }
                        }
                    }
                }

                Event.current.Use();
                break;
                default:
                break;
            }

            this.Repaint();
        }

        private void _draw_title_UI()
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Space(12);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Spline快速生成工具");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(12);
            }
            EditorGUILayout.EndVertical();
        }

        #region 布点生成

        private enum InnerSortTag
        {
            SiblingIndex,
            X,
            Y,
            Z
        }

        private float m_pointListUIHeightPValue = 0.25f;

        private readonly List<Transform> m_GODP_pointList = new List<Transform>();
        private readonly List<Transform> m_GODP_pDelList = new List<Transform>();
        private Vector2 m_GODP_UI_ScrollPos;

        private InnerSortTag m_innerST;
        
        private void GenerationOfDistributionPointsReset()
        {
            m_GODP_pointList.Clear();
            m_GODP_pDelList.Clear();
        }

        private void _draw_GenerationOfDistributionPoints_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("选取生成点");
                GUILayout.Space(5);
                if(m_GODP_pointList.Count > 0)
                {
                    m_GODP_UI_ScrollPos = GUILayout.BeginScrollView(m_GODP_UI_ScrollPos, "box", GUILayout.Height(Screen.height * m_pointListUIHeightPValue));
                    {
                        GUILayout.Space(5);
                        for(int i = 0; i < m_GODP_pointList.Count; i++)
                        {
                            if(i > 0)
                                GUILayout.Space(2);

                            GUILayout.BeginHorizontal();
                            {
                                m_GODP_pointList[i] = (Transform)EditorGUILayout.ObjectField(m_GODP_pointList[i], typeof(Transform), true);
                                GUI.color = Color.red;
                                if(GUILayout.Button("-", GUILayout.Width(22)))
                                {
                                    Transform del = m_GODP_pointList[i];
                                    m_GODP_pDelList.Add(del);
                                }
                                GUI.color = Color.white;
                            }
                            GUILayout.EndHorizontal();

                        }
                        GUILayout.Space(5);
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.BeginVertical("box", GUILayout.Height(Screen.height * m_pointListUIHeightPValue));
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUI.color = Color.gray;
                            GUILayout.Label("[生成节点数据为空]");
                            GUI.color = Color.white;
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUI.color = Color.gray;
                            GUILayout.Label("(拖拽目标或者点击下方按钮加入目标)");
                            GUI.color = Color.white;
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndVertical();
                }

                m_DragArea = GUILayoutUtility.GetLastRect();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUI.color = Color.yellow;
                    if(GUILayout.Button("选择节点加入列表", GUILayout.Width(180), GUILayout.Height(26)))
                    {
                        m_addSelectionToList();
                    }
                    GUI.color = Color.white;
                    if(m_GODP_pointList.Count > 0)
                    {
                        GUI.color = Color.red;
                        if(GUILayout.Button("Clear", GUILayout.Width(46), GUILayout.Height(26)))
                        {
                            if(EditorUtility.DisplayDialog("提示", "确认清除目标列表?", "确定", "取消"))
                            {
                                m_GODP_pointList.Clear();
                            }
                        }
                        GUI.color = Color.white;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("按");
                        m_innerST = (InnerSortTag)EditorGUILayout.EnumPopup(m_innerST, GUILayout.Width(96));
                        GUILayout.Label("方式");
                        if(GUILayout.Button("排序",GUILayout.Width(96)))
                        {
                            m_sortListByInnerSortTag();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if(m_GODP_pointList.Count > 1)
                {
                    GUI.color = Color.yellow;
                    if(GUILayout.Button("生成Spline",GUILayout.Height(26)))
                    {
                        List<Vector3> bpsList = new List<Vector3>();
                        int i;
                        int lenth = m_GODP_pointList.Count;
                        for(i = 0; i < m_GODP_pointList.Count; i++)
                        {
                            bpsList.Add(m_GODP_pointList[i].position);
                        }
                        SplineUtils.FastBuildingUseBigPoints(bpsList.ToArray(), target);
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.gray;
                    if(GUILayout.Button("生成Spline", GUILayout.Height(26)))
                    {
                        //no thing;
                    }
                    GUI.color = Color.white;
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            //del
            if(m_GODP_pDelList.Count > 0)
            {
                for(int i = 0; i < m_GODP_pDelList.Count; i++)
                {
                    m_GODP_pointList.Remove(m_GODP_pDelList[i]);
                }
                m_GODP_pDelList.Clear();
            }
        }

        private void m_sortListByInnerSortTag()
        {
            switch(m_innerST)
            {
                case InnerSortTag.X:
                    {
                        m_GODP_pointList.Sort((a, b) => { 
                            
                            if(a.position.x < b.position.x)
                            {
                                return -1;
                            }
                            return 1;
                        });
                    }
                break;
                case InnerSortTag.Y:
                    {
                        m_GODP_pointList.Sort((a, b) => {

                            if(a.position.y < b.position.y)
                            {
                                return -1;
                            }
                            return 1;
                        });
                    }
                    break;
                case InnerSortTag.Z:
                    {
                        m_GODP_pointList.Sort((a, b) => {

                            if(a.position.z < b.position.z)
                            {
                                return -1;
                            }
                            return 1;
                        });
                    }
                    break;
                default://SiblingIndex
                    {
                        m_GODP_pointList.Sort((a, b) => {

                            if(a.GetSiblingIndex() < b.GetSiblingIndex())
                            {
                                return -1;
                            }
                            return 1;
                        });
                    }
                break;
            }
        }

        private void m_addSelectionToList()
        {
            if(Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "未选择任何有用的目标节点(GameObject)","确定");
                return;
            }

            List<GameObject> select = new List<GameObject>(Selection.gameObjects);
            if(select.Count > 1)
            {
                select.Sort((a, b) => {
                    if(a.transform.GetSiblingIndex() < b.transform.GetSiblingIndex())
                        return -1;
                    else
                        return 1;
                });
            }

            foreach(var go in select)
            {
                m_GODP_pointList.Add(go.transform);
            }

        }

        #endregion

        #region 拼接生成

        private Spline m_splineA;
        private Spline m_splineB;

        private bool m_GOSS_createNewSpline = true;
        private string m_GOSS_newSplineRename;


        //private float m_dotv = -1;

        private void GenerationOfSplicingSplinesReset()
        {
            m_splineA = null;
            m_splineB = null;
            m_GOSS_createNewSpline = true;
            m_GOSS_newSplineRename = string.Empty;
        }

        private void GenerationOfBridgeSplines_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);

                    GUILayout.Label("桥接目标");

                    GUILayout.Space(5);
                    GUILayout.Label("Spline A");
                    m_splineA = (Spline)EditorGUILayout.ObjectField(m_splineA, typeof(Spline), true);

                    GUILayout.Space(5);
                    GUILayout.Label(" 桥接到 ");
                    GUILayout.Space(5);
                    GUILayout.Label("Spline B");
                    m_splineB = (Spline)EditorGUILayout.ObjectField(m_splineB, typeof(Spline), true);

                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);
                    GUILayout.Label($"选项");
                    GUILayout.Space(5);
                    m_GOSS_createNewSpline = EditorGUILayout.Toggle("生成新的Spline对象", m_GOSS_createNewSpline);
                    if(m_GOSS_createNewSpline)
                        m_GOSS_newSplineRename = EditorGUILayout.TextField("重命名", m_GOSS_newSplineRename);
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                //if(m_splineA && m_splineB)
                //{
                //    Vector3 dirA = getSplineDir(m_splineA);
                //    Vector3 dirB = getSplineDir(m_splineB);
                //    m_dotv = Vector3.Dot(dirA, dirB);
                //}
                //else
                //{
                //    m_dotv = -1;
                //}

                //if(m_dotv != -1)
                //{
                //    GUILayout.Space(5);
                //    GUILayout.BeginVertical("box");
                //    {
                //        GUILayout.Space(5);
                //        GUILayout.Label($"Spline方向Dot值:{m_dotv}");
                //        GUILayout.Space(5);
                //    }
                //    GUILayout.EndVertical();
                //    if(m_dotv < 0.0f)
                //    {
                //        GUILayout.Space(5);
                //        GUI.color = Color.red;
                //        GUILayout.BeginVertical("box");
                //        {
                //            GUILayout.Space(5);
                //            GUILayout.Label($"<提示! 拼接Spline方向不吻合,拼接可能出现问题!!>");
                //            GUILayout.Space(5);
                //        }
                //        GUILayout.EndVertical();
                //        GUI.color = Color.white;
                //    }
                //}

                GUILayout.FlexibleSpace();
                if(m_splineA && m_splineB)
                {
                    GUI.color = Color.yellow;
                    if(GUILayout.Button("创建桥接Spline", GUILayout.Height(26)))
                    {
                        if(m_GOSS_createNewSpline)
                        {
                            Spline @new = Spline.Create();
                            if(!string.IsNullOrEmpty(m_GOSS_newSplineRename))
                                @new.name = m_GOSS_newSplineRename;
                            SplineUtils.BuildingSplicingSpline(m_splineA, m_splineB, @new);
                        }
                        else
                        {
                            SplineUtils.BuildingSplicingSpline(m_splineA, m_splineB, Spline.Create());
                        }
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.gray;
                    if(GUILayout.Button("创建桥接Spline", GUILayout.Height(26)))
                    {
                        //Do nothing ...
                    }
                    GUI.color = Color.white;
                }

                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private Vector3 getSplineDir(Spline spline)
        {
            Vector3 s = spline.GetPoint(0.95f);
            Vector3 e = spline.GetPoint(1.0f);
            Vector3 dir = (e - s).normalized;
            return dir;
        }

        #endregion

        #region 合并生成

        private readonly List<Spline> m_MRD_splineList = new List<Spline>();
        private readonly List<Spline> m_MRD_pDelList = new List<Spline>();
        private Vector2 m_MRD_UI_ScrollPos;

        private bool m_MRD_createNewSpline = true;
        private string m_MRD_newSplineRename;
        private bool m_MRD_UseBrigdeMethod;
        private float m_MRD_birdgeThreshold = 0.3f;
        private void MergeGenerationReset()
        {
            m_MRD_splineList.Clear();
            m_MRD_pDelList.Clear();
            m_MRD_createNewSpline = true;
            m_MRD_newSplineRename = string.Empty;
            m_MRD_UseBrigdeMethod = false;
            m_MRD_birdgeThreshold = 0.3f;
        }

        private void _draw_MergeGeneration_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("选取合并Splines");
                GUILayout.Space(5);
                if(m_MRD_splineList.Count > 0)
                {
                    m_MRD_UI_ScrollPos = GUILayout.BeginScrollView(m_MRD_UI_ScrollPos, "box", GUILayout.Height(Screen.height * m_pointListUIHeightPValue));
                    {
                        GUILayout.Space(5);
                        for(int i = 0; i < m_MRD_splineList.Count; i++)
                        {
                            if(i > 0)
                                GUILayout.Space(2);

                            GUILayout.BeginHorizontal();
                            {
                                m_MRD_splineList[i] = (Spline)EditorGUILayout.ObjectField(m_MRD_splineList[i], typeof(Spline), true);
                                GUI.color = Color.red;
                                if(GUILayout.Button("-", GUILayout.Width(22)))
                                {
                                    Spline del = m_MRD_splineList[i];
                                    m_MRD_pDelList.Add(del);
                                }
                                GUI.color = Color.white;
                            }
                            GUILayout.EndHorizontal();

                        }
                        GUILayout.Space(5);
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.BeginVertical("box", GUILayout.Height(Screen.height * m_pointListUIHeightPValue));
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUI.color = Color.gray;
                            GUILayout.Label("[生成Spline数据为空]");
                            GUI.color = Color.white;
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUI.color = Color.gray;
                            GUILayout.Label("(拖拽目标或者点击下方按钮加入目标)");
                            GUI.color = Color.white;
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndVertical();
                }

                m_DragArea = GUILayoutUtility.GetLastRect();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUI.color = Color.yellow;
                    if(GUILayout.Button("选择节点加入列表", GUILayout.Width(180), GUILayout.Height(26)))
                    {
                        m_addSelectionSplinesToList();
                    }
                    GUI.color = Color.white;
                    if(m_MRD_splineList.Count > 0)
                    {
                        GUI.color = Color.red;
                        if(GUILayout.Button("Clear", GUILayout.Width(46), GUILayout.Height(26)))
                        {
                            if(EditorUtility.DisplayDialog("提示", "确认清除目标列表?", "确定", "取消"))
                            {
                                m_MRD_splineList.Clear();
                            }
                        }
                        GUI.color = Color.white;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);
                    GUILayout.Label($"选项");
                    GUILayout.Space(5);
                    m_MRD_createNewSpline = EditorGUILayout.Toggle("生成新的Spline对象", m_MRD_createNewSpline);
                    if(m_MRD_createNewSpline)
                        m_MRD_newSplineRename = EditorGUILayout.TextField("重命名", m_MRD_newSplineRename);
                    m_MRD_UseBrigdeMethod = EditorGUILayout.Toggle("断开部分自动桥接", m_MRD_UseBrigdeMethod);
                    if(m_MRD_UseBrigdeMethod)
                        m_MRD_birdgeThreshold = EditorGUILayout.FloatField("自动桥接阈值", m_MRD_birdgeThreshold);
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if(m_MRD_splineList.Count > 1)
                {
                    GUI.color = Color.yellow;
                    if(GUILayout.Button("合并生成Spline", GUILayout.Height(26)))
                    {
                        if(m_MRD_createNewSpline)
                        {
                            Spline @new = Spline.Create();
                            if(!string.IsNullOrEmpty(m_MRD_newSplineRename))
                                @new.name = m_MRD_newSplineRename;
                            if(m_MRD_UseBrigdeMethod)
                                SplineUtils.MergeSplinesToNewSplineWithBridge(m_MRD_splineList.ToArray(), @new, m_MRD_birdgeThreshold);
                            else
                                SplineUtils.MergeSplinesToNewSpline(m_MRD_splineList.ToArray(), @new);
                        }
                        else
                        {
                            if(m_MRD_UseBrigdeMethod)
                                SplineUtils.MergeSplinesToNewSplineWithBridge(m_MRD_splineList.ToArray(), target, m_MRD_birdgeThreshold);
                            else
                                SplineUtils.MergeSplinesToNewSpline(m_MRD_splineList.ToArray(), target);
                        }
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.gray;
                    if(GUILayout.Button("合并生成Spline", GUILayout.Height(26)))
                    {
                        //no thing;
                    }
                    GUI.color = Color.white;
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            //del
            if(m_MRD_pDelList.Count > 0)
            {
                for(int i = 0; i < m_MRD_pDelList.Count; i++)
                {
                    m_MRD_splineList.Remove(m_MRD_pDelList[i]);
                }
                m_MRD_pDelList.Clear();
            }
        }

        private void m_addSelectionSplinesToList()
        {
            if(Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "未选择任何有用的目标节点(GameObject)", "确定");
                return;
            }

            List<GameObject> select = new List<GameObject>(Selection.gameObjects);
            if(select.Count > 1)
            {
                select.Sort((a, b) => {
                    if(a.transform.GetSiblingIndex() < b.transform.GetSiblingIndex())
                        return -1;
                    else
                        return 1;
                });
            }

            for(int i = 0; i < select.Count; i++)
            {
                Spline sp = select[i].GetComponentInChildren<Spline>();
                if(sp)
                {
                    m_MRD_splineList.Add(sp);
                }
            }
            
        }

        #endregion

    }

}
