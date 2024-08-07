using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.Rendering.Universal.Editor.Utility
{
    /// <summary>
    /// 列表选择器(支持搜索)
    /// 
    /// Author : Aorition
    /// 
    /// Update : 2023/03/17
    /// 
    /// </summary>
    public class SearchAndSelectStateWindow : UnityEditor.EditorWindow
    {

        private const float StateButtonWidth = 200;

        //private static GUIContent m_nameLabel = new GUIContent("名称");

        private static SearchAndSelectStateWindow m_instance;

        public static SearchAndSelectStateWindow Init(string[] labels, Action<object> onApplyStateCallback, bool autoSortLabels = true, bool enableShortcutKey = true, object current = null)
        {
            m_instance = UnityEditor.EditorWindow.GetWindow<SearchAndSelectStateWindow>();
            m_instance.titleContent = new GUIContent("SearchAndSelect");
            //window.minSize = new Vector2(595, 780);c
            m_instance.Setup(labels, labels, onApplyStateCallback, autoSortLabels, enableShortcutKey, current);
            m_instance.ShowUtility();
            return m_instance;
        }

        public static SearchAndSelectStateWindow Init(object[] values, Action<object> onApplyStateCallback, bool autoSortLabels = true, bool enableShortcutKey = true, object current = null)
        {
            m_instance = UnityEditor.EditorWindow.GetWindow<SearchAndSelectStateWindow>();
            m_instance.titleContent = new GUIContent("SearchAndSelect");
            //window.minSize = new Vector2(595, 780);
            string[] labels = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                labels[i] = values[i].ToString();
            }
            m_instance.Setup(labels, values, onApplyStateCallback, autoSortLabels, enableShortcutKey, current);
            m_instance.ShowUtility();
            return m_instance;
        }

        public static SearchAndSelectStateWindow Init(string[] labels, object[] values, Action<object> onApplyStateCallback, bool autoSortLabels = true, bool enableShortcutKey = true, object current = null)
        {
            m_instance = UnityEditor.EditorWindow.GetWindow<SearchAndSelectStateWindow>();
            m_instance.titleContent = new GUIContent("SearchAndSelect");
            //window.minSize = new Vector2(595, 780);
            m_instance.Setup(labels, values, onApplyStateCallback, autoSortLabels, enableShortcutKey, current);
            m_instance.ShowUtility();
            return m_instance;
        }

        private object m_crrent;
        private Action<object> m_onApplyState;

        private readonly List<string> m_labelList = new List<string>(); 
        private readonly Dictionary<string, object> m_valueDic = new Dictionary<string, object>();

        private bool m_enableShortcutKey;
        private bool m_isSetuped;

        public void Setup(string[] labels, object[] values, Action<object> onApplyStateCallback, bool autoSortLabels = true, bool enableShortcutKey = true, object current = null)
        {
            m_onApplyState = onApplyStateCallback;
            m_crrent = current;
            if(labels != null && values != null && labels.Length == values.Length)
            {
                m_labelList.Clear();
                m_valueDic.Clear();
                for (int i = 0; i < labels.Length; i++)
                {
                    string label = labels[i];
                    object value = values[i];
                    m_labelList.Add(label);
                    m_valueDic.Add(label, value);
                }
                if(autoSortLabels)
                    m_labelList.Sort();

                m_enableShortcutKey = enableShortcutKey;
                m_isSetuped = true;
            }
        }

        private void OnFocus()
        {
            FocusInputField();
        }

        private void OnDestroy()
        {
            m_crrent = null;
            m_onApplyState = null;
            m_labelList.Clear();
            m_valueDic.Clear();
            m_searchedStates.Clear();
        }

        public void FocusInputField()
        {
            m_NeedFocusInputField = true;

        }

        private string m_searchKey;
        private readonly List<string> m_searchedStates = new List<string>();

        private bool m_NeedFocusInputField;

        private void OnGUI()
        {
            if (!m_isSetuped) return;

            GUILayout.Space(5);
            GUI.SetNextControlName("InputField");
            m_searchKey = EditorGUILayout.TextField(m_searchKey);
            if (m_NeedFocusInputField)
            {
                EditorGUI.FocusTextInControl("InputField");
                SimulateKeyboardInput.Dispatch(SimulateKeyboardKeyCode.RightArrow);
                m_NeedFocusInputField = false;
            }
            SearchDataUpdate();
            Draw_MainUI(Mathf.Max(Screen.width / (int)StateButtonWidth, 1));
            if(m_enableShortcutKey)
                HandleEvents();
        }

        private void SearchDataUpdate()
        {

            m_searchedStates.Clear();

            if (!string.IsNullOrEmpty(m_searchKey))
                m_searchKey = m_searchKey.Trim();

            if (string.IsNullOrEmpty(m_searchKey))
            {
                for (int i = 0; i < m_labelList.Count; i++)
                {
                    m_searchedStates.Add(m_labelList[i]);
                }
            }
            else
            {
                for (int i = 0; i < m_labelList.Count; i++)
                {
                    if (m_labelList[i].ToLower().StartsWith(m_searchKey.ToLower()))
                    {
                        m_searchedStates.Add(m_labelList[i]);
                    }
                }
            }
        }

        private Vector2 m_mainScrollPos;
        private int m_kbIndex = -1;
        private void Draw_MainUI(int colNum)
        {
            m_mainScrollPos = GUILayout.BeginScrollView(m_mainScrollPos);
            {
                GUILayout.Space(5);
                int index;
                for (int v = 0; v < m_searchedStates.Count; v += colNum)
                {
                    GUILayout.BeginHorizontal();
                    {
                        for (int u = 0; u < colNum; u++)
                        {
                            index = v + u;
                            if (index < m_searchedStates.Count)
                            {
                                string label = m_searchedStates[index];
                                object value = m_valueDic[label];
                                GUI.color = (value != null && value.Equals(m_crrent) ? Color.yellow : (index == m_kbIndex ? Color.cyan : Color.white));
                                if (GUILayout.Button(label, GUILayout.MaxWidth(StateButtonWidth * 2)))
                                {
                                    Apply(value);
                                }
                                GUI.color = Color.white;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(5);
            }
            GUILayout.EndScrollView();
        }

        private void Apply(object value)
        {
            if(m_onApplyState != null)
                m_onApplyState(value);
            this.Close();
        }

        private void HandleEvents()
        {
            Event evt = Event.current;
            if (evt.isKey && evt.type == EventType.KeyUp)
            {

                if (evt.keyCode == KeyCode.UpArrow)
                {
                    int n = m_kbIndex - 1;
                    m_kbIndex = Mathf.Max(0, n);
                    Repaint();
                }
                else if (evt.keyCode == KeyCode.DownArrow)
                {
                    int n = m_kbIndex + 1;
                    m_kbIndex = Mathf.Min(m_searchedStates.Count - 1, n);
                    Repaint();
                }
                else if (evt.keyCode == KeyCode.Tab)
                {
                    if (m_searchedStates.Count > 0)
                    {
                        int idx = Mathf.Clamp(m_kbIndex, 0, m_searchedStates.Count - 1);
                        string label = m_searchedStates[idx];
                        object value = m_valueDic[label];
                        Apply(value);
                    }
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    this.Close();
                }
            }
        }

    }
}
