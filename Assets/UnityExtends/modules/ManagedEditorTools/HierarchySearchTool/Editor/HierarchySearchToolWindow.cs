using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Reflection;

namespace UnityEngine.Rendering.Universal.Editor.Utility
{

    /// <summary>
    /// Hierarchy选择器
    /// 
    /// Author : Aorition
    /// 
    /// Update : 2023-03-22
    /// 
    /// 
    /// </summary>
    public class HierarchySearchToolWindow : UnityEditor.EditorWindow
    {

        private static HierarchySearchToolWindow m_Instance;
        [MenuItem("Window/FrameworkTools/HierarchySearch/Hierarchy选择器")]
        public static HierarchySearchToolWindow Init()
        {
            m_Instance = UnityEditor.EditorWindow.GetWindow<HierarchySearchToolWindow>();
            m_Instance.titleContent = new GUIContent("Hierarchy选择器");
           
            return m_Instance;
        }

        public static HierarchySearchToolWindow Init(string searchKey, GameObject searchRoot)
        {
            Init();
            m_Instance.Search(searchKey, searchRoot);
            return m_Instance;
        }

        /// <summary>
        /// 分页最大显示条数
        /// </summary>
        public int LimitPerPage = 500;

        public void Search(string searchKey, GameObject searchRoot)
        {
            m_SearchKey = searchKey;
            m_SearchRoot = searchRoot;
            UpdateSearchDatas();
        }

        private void OnFocus()
        {
            FocusInputField();
        }

        private void Awake()
        {
            //预热静态变量
            var preloadDic = SearchCommandUtility.AllComponentTypes;
        }

        private void OnDestroy()
        {
            m_SearchKey = null;
            m_ActivedHashSet.Clear();
            m_selectedList.Clear();
        }

        private bool m_NeedFocusInputField;

        private bool m_SmartTips = true;
        private GameObject m_SearchRoot;
        private string m_SearchKey;
        private string m_SearchKeyCache;
        private readonly List<GameObject> m_selectedList = new List<GameObject>();
        
        private void OnGUI()
        {

            Draw_SearchUI();

            if (m_selectedList.Count > 0)
            {
                Draw_ResultUI();
            }
            else
            {
                Draw_EmptyUI();
            }

            HandleEvents();
            Repaint();
        }

        public void FocusInputField()
        {
            m_NeedFocusInputField = true;
        }

        private void Draw_SearchUI()
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                {
                    m_SearchRoot = (GameObject)EditorGUILayout.ObjectField("搜索根节点", m_SearchRoot, typeof(GameObject), true);
                    if(GUILayout.Button("C", GUILayout.Width(22)))
                    {
                        m_SearchRoot = null;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    m_SmartTips = EditorGUILayout.ToggleLeft("检索指令智能辅助", m_SmartTips);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.SetNextControlName("InputField");
                    m_SearchKeyCache = EditorGUILayout.TextField(m_SearchKey);
                    if (m_NeedFocusInputField)
                    {
                        EditorGUI.FocusTextInControl("InputField");
                        SimulateKeyboardInput.Dispatch(SimulateKeyboardKeyCode.RightArrow);
                        m_NeedFocusInputField = false;
                    }
                    if (m_SearchKey != m_SearchKeyCache)
                    {
                        m_SearchKey = m_SearchKeyCache;
                        InputPerception();
                    }
                    if(GUILayout.Button("Search", GUILayout.Width(72)))
                    {
                        GUI.FocusControl(null);
                        UpdateSearchDatas();
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
        }
         
        private void Draw_EmptyUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUI.color = Color.gray;
                    GUILayout.Label("[检索结果为空，请确认搜索指令并单击搜索按钮]");
                    GUI.color = Color.white;
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }

        private Vector2 m_resultScrollPos;
        private readonly HashSet<GameObject> m_ActivedHashSet = new HashSet<GameObject>();

        private int m_pageIndex = 0;

        private void Draw_ResultUI()
        {
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.Label($"检索到{m_selectedList.Count}个GameObject对象In [{(m_SearchRoot ? m_SearchRoot.name : "Hierarchy")}]");
            }
            GUILayout.EndHorizontal();
            m_resultScrollPos = GUILayout.BeginScrollView(m_resultScrollPos, "box");
            
                GUILayout.Space(5);

                int count = m_selectedList.Count;
                int pageNum = LimitPerPage;
                int pageCount = Mathf.CeilToInt((float)count / pageNum);
                int s = m_pageIndex * pageNum;
                int len = s + pageNum;
                for (; s < len; s++)
                {
                    if (s < count)
                    {
                        GameObject item = m_selectedList[s];
                        GUILayout.BeginHorizontal();
                        {
                            GUI.backgroundColor = m_ActivedHashSet.Contains(item) ? Color.yellow : Color.white;
                            bool clicked = GUILayout.Button($"[{s}]", GUILayout.MaxWidth(46));
                            if(GUILayout.Button(item.name))
                                clicked = true;
                            if (clicked)
                            {
                                if (m_ActivedHashSet.Contains(item))
                                    m_ActivedHashSet.Remove(item);
                                else
                                    m_ActivedHashSet.Add(item);
                            }
                            if (GUILayout.Button(">", GUILayout.Width(26)))
                            {
                                Selection.activeGameObject = item;
                            }
                            GUI.backgroundColor = Color.white;
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.Space(5);
            
            GUILayout.EndScrollView();

            if (count > pageNum)
                m_pageIndex = DrawTurnPageUI(m_pageIndex, pageCount);

            GUILayout.BeginHorizontal("box");
            {
                if(GUILayout.Button("Mark All"))
                {
                    m_ActivedHashSet.Clear();
                    foreach (var item in m_selectedList)
                    {
                        m_ActivedHashSet.Add(item);
                    }
                }
                if (GUILayout.Button("Mark None"))
                {
                    m_ActivedHashSet.Clear();
                }
                if (GUILayout.Button("Send Marked To Hierarchy"))
                {
                    List<GameObject> list = new List<GameObject>();
                    if(m_ActivedHashSet.Count == 0)
                    {
                        if(EditorUtility.DisplayDialog("提示", "是否发送全部检索对象到Hierarchy?", "确定", "取消"))
                        {
                            foreach (var item in m_selectedList)
                            {
                                m_ActivedHashSet.Add(item);
                                list.Add(item);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in m_ActivedHashSet)
                        {
                            list.Add(item);
                        }
                    }
                    Selection.objects = null;
                    Selection.objects = list.ToArray();
                }
            }
            GUILayout.EndHorizontal();
        }

        private int DrawTurnPageUI(int pageIndex, int pageCounts)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.Width(320));
                {
                    if (GUILayout.Button("<", GUILayout.Width(36)))
                    {
                        pageIndex--;
                        pageIndex = Math.Max(pageIndex, 0);
                        m_resultScrollPos = Vector2.zero;
                        GUI.FocusControl(null);
                    }
                    int nPageIndex = EditorGUILayout.IntField(pageIndex + 1, GUILayout.Width(150)) - 1;
                    if (pageIndex != nPageIndex)
                    {
                        pageIndex = Mathf.Clamp(nPageIndex, 0, pageCounts - 1);
                    }
                    GUILayout.Label($"/{pageCounts}", GUILayout.Width(150));
                    if (GUILayout.Button(">", GUILayout.Width(36)))
                    {
                        pageIndex++;
                        pageIndex = Math.Min(pageIndex, pageCounts - 1);
                        m_resultScrollPos = Vector2.zero;
                        GUI.FocusControl(null);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            return pageIndex;
        }

        private void InputPerception()
        {
            if (m_SearchKeyCache.EndsWith(":"))
            {

                if (Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\At:") || Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\b+t:"))
                {
                    List<string> inputs = new List<string>();
                    foreach (var kv in SearchCommandUtility.AllComponentTypes)
                    {
                        inputs.Add(kv.Key);
                    }
                    var w = SearchAndSelectStateWindow.Init(inputs.ToArray(), s => 
                    {
                        m_SearchKey = m_SearchKeyCache + s;
                        Focus();
                        FocusInputField();
                    });
                    w.FocusInputField();
                }else if (Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\Atag:") || Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\b+tag:"))
                {
                    var w = SearchAndSelectStateWindow.Init(UnityEditorInternal.InternalEditorUtility.tags, s =>
                    {
                        m_SearchKey = m_SearchKeyCache + s;
                        Focus();
                        FocusInputField();
                    }, false);
                    w.FocusInputField();
                }
                else if (Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\Al:") || Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\b+l:")
                    || Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\Alayer:") || Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\b+layer:")
                    )
                {
                    var w = SearchAndSelectStateWindow.Init(UnityEditorInternal.InternalEditorUtility.layers, s =>
                    {
                        m_SearchKey = m_SearchKeyCache + s;
                        Focus();
                        FocusInputField();
                    }, false);
                    w.FocusInputField();
                }


            }
            else if (m_SearchKeyCache.EndsWith("{") && (Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\At:(.+){") || Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\b+t:(.+){")))
            {
                var ms = Regex.Matches(m_SearchKeyCache, @"t:(.+){");
                if (ms.Count > 0 && ms[ms.Count - 1].Groups.Count == 2)
                {
                    string typeName = ms[ms.Count - 1].Groups[1].Value;
                    if (!string.IsNullOrEmpty(typeName))
                    {
                        List<string> inputs = new List<string>();
                        List<string> values = new List<string>();
                        Type t = SearchCommandUtility.AllComponentTypes[typeName][0];
                        if (t != null)
                        {
                            PropertyInfo[] pInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                            foreach (var pInfo in pInfos)
                            {
                                if (pInfo.CanRead && CheckSubType(pInfo.PropertyType))
                                {
                                    inputs.Add($"{pInfo.Name}\t({pInfo.PropertyType.Name})");
                                    values.Add(pInfo.Name);
                                }
                            }
                            FieldInfo[] fInfos = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                            foreach (var fInfo in fInfos)
                            {
                                if (CheckSubType(fInfo.FieldType))
                                {
                                    inputs.Add($"{fInfo.Name}\t({fInfo.FieldType.Name})");
                                    values.Add(fInfo.Name);
                                }
                            }
                            var w = SearchAndSelectStateWindow.Init(inputs.ToArray(), values.ToArray(), s =>
                            {
                                m_SearchKey = m_SearchKeyCache + s;
                                Focus();
                                FocusInputField();
                            });
                            w.FocusInputField();
                        }
                    }
                }

            }
            else if (m_SearchKeyCache.EndsWith("{") && (Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\A{") || Regex.IsMatch(m_SearchKeyCache.ToLower(), @"\b+{")))
            {
                List<string> inputs = new List<string>();
                List<string> values = new List<string>();
                Type t = typeof(GameObject);
                if (t != null)
                {
                    PropertyInfo[] pInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var pInfo in pInfos)
                    {
                        if (pInfo.CanRead && CheckSubType(pInfo.PropertyType))
                        {
                            inputs.Add($"{pInfo.Name}\t({pInfo.PropertyType.Name})");
                            values.Add(pInfo.Name);
                        }
                    }
                    FieldInfo[] fInfos = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var fInfo in fInfos)
                    {
                        if (CheckSubType(fInfo.FieldType))
                        {
                            inputs.Add($"{fInfo.Name}\t({fInfo.FieldType.Name})");
                            values.Add(fInfo.Name);
                        }
                    }
                    var w = SearchAndSelectStateWindow.Init(inputs.ToArray(), values.ToArray(), s =>
                    {
                        m_SearchKey = m_SearchKeyCache + s;
                        Focus();
                        FocusInputField();
                    });
                    w.FocusInputField();
                }

            }
        }

        /// <summary>
        /// 检查受支持的字段类型
        /// (暂时不支持v2,v3,v4等字段类型)
        /// </summary>
        private bool CheckSubType(Type subType)
        {
            switch (subType.Name)
            {
                case "Boolean":
                case "Single":
                case "Byte":
                case "Int16":
                case "Int32":
                case "String":
                    return true;
                default:
                    Type baseType = typeof(UnityEngine.Object);
                    Type curType = subType;
                    while(curType != null)
                    {
                        if (curType.FullName == baseType.FullName)
                            return true;
                        curType = curType.BaseType;
                    }
                    return false;
            }
        }

        private void UpdateSearchDatas()
        {
            m_pageIndex = 0;
            m_selectedList.Clear();
            if (string.IsNullOrEmpty(m_SearchKey))
                return;

            SearchCommandUtility.ExecSearchCommand(m_selectedList, m_SearchKey, m_SearchRoot);
        }

        private void HandleEvents()
        {
            Event evt = Event.current;
            if (evt.isKey && evt.type == EventType.KeyUp)
            {
                //监听回车行为(保证逻辑正确需要按两次)
                if(EditorGUIUtility.textFieldHasSelection && (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return))
                {
                    GUI.FocusControl(null);
                    UpdateSearchDatas();
                }
            }
        }

    }

}


