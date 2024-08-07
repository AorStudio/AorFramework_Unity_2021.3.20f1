using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UnityEngine.Rendering.Universal.Editor.Utility
{

    public class UndoInfoViewerWindow : EditorWindow
    {
        
        private static UndoInfoViewerWindow _instance;

        [MenuItem("Window/FrameworkTools/辅助工具/Undo Info Viewer", false, 505)]
        public static UndoInfoViewerWindow Init()
        {
            _instance = EditorWindow.GetWindow<UndoInfoViewerWindow>();
            _instance.titleContent = new GUIContent("Undo信息查看器");
            return _instance;
        }

        private MethodInfo m_UndoGetRecords;

        private void Awake()
        {
            m_UndoGetRecords = typeof(Undo).GetMethod("GetRecords", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private void OnDestroy()
        {
            m_UndoGetRecords = null;
            //if (m_GUIDrawer != null)
            //{
            //    m_GUIDrawer.Dispose();
            //    m_GUIDrawer = null;
            //}
            //m_InnerData = null;
        }

        private const int m_btnHeightDefine = 22;
        private const float m_itemLineHegithDefine = 20;

        private readonly List<string> m_undoRecords = new List<string>();
        private Vector2 m_undoSPos;
        private int m_uCountCache;
        private readonly List<string> m_redoRecords = new List<string>();
        private Vector2 m_redoSPos;
        private int m_rCountCache;

        private void OnGUI()
        {

            if (m_UndoGetRecords == null)
            {
                Awake();
                return;
            }

            int i;
            m_undoRecords.Clear();
            m_redoRecords.Clear();
            m_UndoGetRecords.Invoke(null, new object[] { m_undoRecords, m_redoRecords });

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("[ Current Record ]");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("helpbox");
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(Undo.GetCurrentGroupName());
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.Space(12);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("[ Undo Records List ]");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                float uViewHeight = Screen.height * 0.4f;
                //更新检测
                if (m_undoRecords.Count != m_uCountCache)
                {
                    m_uCountCache = m_undoRecords.Count;
                    //计算m_undoSPos最大值
                    float uHMax = m_undoRecords.Count * m_itemLineHegithDefine + 10;
                    if(uHMax >= uViewHeight) 
                        m_undoSPos = new Vector2(0, uHMax);
                }

                m_undoSPos = GUILayout.BeginScrollView(m_undoSPos, "helpbox", GUILayout.Height(uViewHeight));
                {
                    GUILayout.Space(5);

                    for (i = 0; i < m_undoRecords.Count; i++)
                    {
                        GUILayout.Label($"[{i}]\t| \t{m_undoRecords[i]}");
                    }

                    GUILayout.Space(5);
                }
                GUILayout.EndScrollView();

                GUILayout.Space(12);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("[ Redo Records List ]");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                float rViewHeight = Screen.height * 0.2f;

                if (m_redoRecords.Count != m_rCountCache)
                {
                    m_rCountCache = m_redoRecords.Count;
                    //计算m_undoSPos最大值
                    float rHMax = m_redoRecords.Count * m_itemLineHegithDefine + 10;
                    if (rHMax >= rViewHeight)
                        m_undoSPos = new Vector2(0, rHMax);
                }

                m_redoSPos = GUILayout.BeginScrollView(m_redoSPos, "helpbox", GUILayout.Height(rViewHeight));
                {
                    GUILayout.Space(5);
                    for (i = 0; i < m_redoRecords.Count; i++)
                    {
                        GUILayout.Label($"[{i}]\t| \t{m_redoRecords[i]}");
                    }
                    GUILayout.Space(5);
                }
                GUILayout.EndScrollView();

                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            {
                if (Selection.activeGameObject) 
                {
                    if (GUILayout.Button("Clear Undo", GUILayout.Height(m_btnHeightDefine)))
                    {
                        Undo.ClearUndo(Selection.activeGameObject);
                    }
                }
                else
                {
                    GUI.color = Color.gray;
                    if (GUILayout.Button("Clear Undo", GUILayout.Height(m_btnHeightDefine)))
                    {
                        //do nothing ...
                    }
                    GUI.color = Color.white;
                }

                if (GUILayout.Button("Clear All Undo", GUILayout.Height(m_btnHeightDefine)))
                {
                    Undo.ClearAll();
                }
            }
            GUILayout.EndHorizontal();

            Repaint();
        }

    }

}