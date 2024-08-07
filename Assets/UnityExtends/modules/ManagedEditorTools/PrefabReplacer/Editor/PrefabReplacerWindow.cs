using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEditor.SceneManagement;
#if FRAMEWORKDEF
using Framework.Extends;
using Framework.Editor;
#else
using AORCore;
using AORCore.Editor;
#endif

namespace UnityEngine.Rendering.Universal.Utility.Editor
{
    public class PrefabReplacerWindow :UnityEditor.EditorWindow
    {

        private static GUIStyle _titleStyle;
        protected static GUIStyle titleStyle
        {
            get {
                if(_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(EditorStyles.largeLabel);
                    _titleStyle.fontSize = 16;
                }
                return _titleStyle;
            }
        }

        private static GUIContent _content_UseSelection;
        protected static GUIContent contentUseSelection
        {
            get {
                if(_content_UseSelection == null)
                {
                    _content_UseSelection = new GUIContent("UseSelection", "使用Selection选中路径");
                }
                return _content_UseSelection;
            }
        }

        private static GUIContent _content_Set;
        protected static GUIContent contentSet
        {
            get {
                if(_content_Set == null)
                {
                    _content_Set = new GUIContent("Set", "设置搜索路径");
                }
                return _content_Set;
            }
        }

        private static PrefabReplacerWindow _instance;

        [MenuItem("Window/FrameworkTools/Prefab/Prefab(Instacne)替换工具", false, 10100)]
        public static PrefabReplacerWindow init()
        {

            _instance = UnityEditor.EditorWindow.GetWindow<PrefabReplacerWindow>();
            _instance.minSize = new Vector2(495, 612);

            return _instance;
        }

        private static string[] m_menuLabels = new string[] { "Hierarchy 替换工具", "Project批量替换工具" };

        private int m_menuIndex = 0;

        private void OnGUI()
        {
            if(EditorApplication.isCompiling)
                Close();

            GUILayout.Space(15);
            _draw_toolTitle_UI();
            GUILayout.Space(15);

            m_menuIndex = GUILayout.Toolbar(m_menuIndex, m_menuLabels, GUILayout.Height(26));
            GUILayout.Space(5);
            _draw_replaceSrc_UI();
            GUILayout.Space(5);

            switch(m_menuIndex)
            {
                case 1:
                    {
                        _draw_batchReplaceTar_UI();
                        GUILayout.Space(5);
                        _draw_targetDirPath_UI();
                        GUILayout.Space(5);
                        GUILayout.FlexibleSpace();
                        _draw_start_UI();
                    }
                break;
                default:
                    {
                        _draw_replaceSrcs_UI();
                        GUILayout.Space(5);
                        GUILayout.FlexibleSpace();
                        _draw_start_UI();
                    }
                break;
            }
        }

        private void _draw_toolTitle_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Prefab(Instacne) 替换工具", titleStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private GameObject m_replaceSrc;
        private void _draw_replaceSrc_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(new GUIContent("替换源 Prefab"));
                    m_replaceSrc = (GameObject)EditorGUILayout.ObjectField(m_replaceSrc, typeof(GameObject), false);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }



        private bool m_UseSelection = true;
        private Vector2 m_replaceSrcScrollPos;
        private readonly List<GameObject> m_replaceTarList = new List<GameObject>();
        private readonly List<GameObject> m_replaceTarDelList = new List<GameObject>();

        private void addSelectionToList(List<GameObject> list)
        {
            foreach(var obj in Selection.objects)
            {
                if(obj is GameObject)
                {
                    GameObject ad = obj as GameObject;
                    if(!list.Contains(ad))
                        list.Add(ad);
                }
            }
        }

        private void _draw_replaceSrcs_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                m_UseSelection = EditorGUILayout.ToggleLeft(new GUIContent("使用Selection作为替换目标"), m_UseSelection);
                if(!m_UseSelection)
                {
                    GUILayout.Space(5);
                    m_replaceSrcScrollPos = GUILayout.BeginScrollView(m_replaceSrcScrollPos, "box", GUILayout.MinHeight(Screen.height * 0.5f));
                    {
                        for(int i = 0; i < m_replaceTarList.Count; i++)
                        {
                            if(i > 0)
                                GUILayout.Space(2);

                            GUILayout.BeginHorizontal();
                            {
                                GameObject @new = (GameObject)EditorGUILayout.ObjectField(m_replaceTarList[i], typeof(GameObject), true);
                                if(@new == null)
                                {
                                    GameObject del = m_replaceTarList[i];
                                    m_replaceTarDelList.Add(del);
                                }
                                else if(@new != m_replaceTarList[i])
                                    m_replaceTarList[i] = @new;
                                if(GUILayout.Button("-", GUILayout.Width(28)))
                                {
                                    GameObject del = m_replaceTarList[i];
                                    m_replaceTarDelList.Add(del);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }

                        if(m_replaceTarDelList.Count > 0)
                        {
                            foreach(var del in m_replaceTarDelList)
                            {
                                m_replaceTarList.Remove(del);
                            }
                            m_replaceTarDelList.Clear();
                        }
                    }
                    GUILayout.EndScrollView();
                    GUILayout.Space(2);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if(GUILayout.Button("加入选择目标到目标源列表",GUILayout.Width(Screen.width * 0.4f)))
                            addSelectionToList(m_replaceTarList);
                        if(GUILayout.Button("清除列表", GUILayout.Width(Screen.width * 0.2f)))
                        {
                            if(EditorUtility.DisplayDialog("提示", "确认清除列表?", "确定", "取消"))
                                m_replaceTarList.Clear();
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private bool checkStartCondition() 
        {
            switch(m_menuIndex)
            {
                case 1:
                    return !string.IsNullOrEmpty(m_targetDirPath) && m_replaceSrc && m_batchReplaceTar;
                default:
                    return m_UseSelection ? (Selection.objects.Length > 0 && m_replaceSrc) : (m_replaceTarList.Count > 0 && m_replaceSrc);
            }
        }

        private void _draw_start_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {

                    if(checkStartCondition())
                    {
                        GUI.color = Color.yellow;
                        if(GUILayout.Button("开始替换", GUILayout.Height(28)))
                        {
                            switch(m_menuIndex)
                            {
                                case 1:
                                    {
                                        startBatchProcessLoop();
                                    }
                                break;
                                default:
                                    {
                                        startProcessLoop();
                                    }
                                break;
                            }
                        }
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.gray;
                        if(GUILayout.Button("开始替换", GUILayout.Height(28)))
                        {
                            //do nothing ...
                        }
                        GUI.color = Color.white;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }


        private void startProcessLoop()
        {
            if(m_UseSelection)
            {
                m_replaceTarList.Clear();
                addSelectionToList(m_replaceTarList);
            }

            for(int i = 0; i < m_replaceTarList.Count; i++)
            {

                EditorUtility.DisplayProgressBar("正在替换...", $"请稍后... {i + 1}/{m_replaceTarList.Count}", ((float)(i + 1)) / m_replaceTarList.Count);

                GameObject tar = m_replaceTarList[i];

                string nodePath = EditorPlusMethods.GetNodePath(tar.transform);

                GameObject tPrefabIns = PrefabUtility.GetNearestPrefabInstanceRoot(tar);
                if(tPrefabIns)
                {
                    if(tar == tPrefabIns)
                    {
                        if(tar.transform.parent)
                            replaceProcess(tar.transform.parent.gameObject, nodePath);
                        else
                            replaceCore(tar.transform);
                    }
                    else
                        replaceProcess(tar, nodePath);
                }
                else
                    replaceCore(tar.transform);
            }

            EditorUtility.ClearProgressBar();
        }

        private void replaceCore(Transform nodeT)
        {
            Transform nodeParentT = nodeT.parent;

            int siblingIndex = nodeT.GetSiblingIndex();
            Vector3 scale = nodeT.localScale;
            Quaternion rotation = nodeT.localRotation;
            Vector3 position = nodeT.localPosition;

            GameObject srcIns = (GameObject)PrefabUtility.InstantiatePrefab(m_replaceSrc);
            srcIns.name = m_replaceSrc.name;

            if(nodeParentT)
                srcIns.transform.SetParent(nodeParentT, false);

            srcIns.transform.SetSiblingIndex(siblingIndex);
            srcIns.transform.localScale = scale;
            srcIns.transform.localRotation = rotation;
            srcIns.transform.localPosition = position;

            srcIns.SetActive(nodeT.gameObject.activeSelf);

            GameObject.DestroyImmediate(nodeT.gameObject);
        }

        private void replaceProcess(GameObject rootIns, string nodePath)
        {
            string rootPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(rootIns);
            if(!string.IsNullOrEmpty(rootPrefabPath))
            {
                GameObject content = PrefabUtility.LoadPrefabContents(rootPrefabPath);
                if(content)
                {
                    //Transform nodeT = content.transform.FindWithNodePath(nodePath);
                    Transform nodeT = content.transform.Find(nodePath);
                    if (nodeT)
                        replaceCore(nodeT);

                    PrefabUtility.SaveAsPrefabAsset(content, rootPrefabPath);
                    PrefabUtility.UnloadPrefabContents(content);
                }
            }
        }

        //-------------------------------------------------------------------

        private GameObject m_batchReplaceTar;
        private void _draw_batchReplaceTar_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(new GUIContent("替换目标 Prefab"));
                    m_batchReplaceTar = (GameObject)EditorGUILayout.ObjectField(m_batchReplaceTar, typeof(GameObject), false);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private string m_targetDirPath;
        private void _draw_targetDirPath_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("搜索路径");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    m_targetDirPath = EditorGUILayout.TextField(m_targetDirPath);
                    if(GUILayout.Button(contentUseSelection, GUILayout.Width(120)))
                    {
                        if(Selection.activeObject)
                        {
                            string tp = AssetDatabase.GetAssetPath(Selection.activeObject);
                            if(!string.IsNullOrEmpty(tp))
                            {

                                EditorAssetInfo info = new EditorAssetInfo(tp);
                                m_targetDirPath = info.dirPath;

                            }
                            else
                            {
                                m_targetDirPath = "";
                            }
                        }
                        else
                        {
                            m_targetDirPath = "";
                        }
                    }
                    if(GUILayout.Button(contentSet, GUILayout.Width(50)))
                    {
                        m_targetDirPath = EditorUtility.SaveFolderPanel("设置搜索路径", "", "");
                        m_targetDirPath = m_targetDirPath.Replace(Application.dataPath, "Assets");
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private readonly StringBuilder m_info = new StringBuilder();
        private readonly HashSet<string> m_cacheHash = new HashSet<string>();

        private void startBatchProcessLoop()
        {
            m_info.Clear();
            _infoAppendLine(0, "ProcessStart >");
            m_cacheHash.Clear();
            List<EditorAssetInfo> infoList = EditorAssetInfo.FindEditorAssetInfoInPath(m_targetDirPath, "*.prefab");
            if(infoList != null && infoList.Count > 0)
            {
                string brTarPath = AssetDatabase.GetAssetPath(m_batchReplaceTar);
                GameObject asset;
                int len = infoList.Count;
                for(int i = 0; i < len; i++)
                {
                    EditorUtility.DisplayProgressBar("Progress", "正在处理" + (i + 1) + "/" + len, (float)(i + 1) / len);
                    //#if UNITY_2018_4_OR_NEWER
                    //                    asset = PrefabUtility.LoadPrefabContents(infoList[i].path);
                    //#else
                    //                    asset = AssetDatabase.LoadAssetAtPath<GameObject>(infoList[i].path);
                    //#endif
                    asset = AssetDatabase.LoadAssetAtPath<GameObject>(infoList[i].path);
                    if(asset)
                    {
                        replaceBatchProcess(brTarPath, asset, 1, infoList[i].path);
                    }
                    //#if UNITY_2018_4_OR_NEWER
                    //                    PrefabUtility.UnloadPrefabContents(asset);
                    //#endif
                }
                EditorUtility.ClearProgressBar();
            }
            _infoAppendLine(0, "< ProcessEnd");
        }

        private void replaceBatchProcess(string brTarPath, GameObject asset, int indent, string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            if(!m_cacheHash.Contains(guid))
            {
                m_cacheHash.Add(guid);
                //
                _infoAppendLine(indent, "Prefab> " + asset.name + "(" + path + ")");
                bool dirty = false;
                int indentNext = indent + 1;
                _subLoopProcess(brTarPath, asset.transform, indentNext, ref dirty);
                _infoAppendLine(indent, "");
                if(dirty)
                {
                    EditorUtility.SetDirty(asset);
                    PrefabUtility.SavePrefabAsset(asset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        private void _subLoopProcess(string brTarPath, Transform transform, int indent, ref bool dirty)
        {
            bool isIns = PrefabUtility.IsPartOfPrefabInstance(transform);
            int indentNext = indent + 1;

            if(isIns)
            {
                bool isModify = _checkModify(transform.gameObject);
                if(!isModify)
                {
                    GameObject linkRoot = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(transform.gameObject);
                    if(linkRoot)
                    {
                        string linkPath = AssetDatabase.GetAssetPath(linkRoot);
                        if(linkPath == brTarPath)
                        {
                            replaceCore(transform);
                            dirty = true;
                        }
                        else
                            replaceBatchProcess(brTarPath, linkRoot, indent, linkPath);
                        return;
                    }
                }
            }
            for(int i = 0; i < transform.childCount; i++)
            {
                Transform sub = transform.GetChild(i);
                _subLoopProcess(brTarPath, sub, indentNext, ref dirty);
            }
        }

        private bool _checkModify(GameObject node)
        {
            List<AddedComponent> addedComponents = PrefabUtility.GetAddedComponents(node);
            if(addedComponents != null && addedComponents.Count > 0)
            {
                return true;
            }
            //逻辑需要检查modify不需要检查AddedGameObject
            //List<AddedGameObject> addedGameObjects = PrefabUtility.GetAddedGameObjects(node);
            //if(addedGameObjects != null && addedGameObjects.Count > 0)
            //{
            //    return true;
            //}
            List<ObjectOverride> objectOverrideList = PrefabUtility.GetObjectOverrides(node);
            if(objectOverrideList != null && objectOverrideList.Count > 0)
            {
                return true;
            }
            return false;
        }

        private void _infoAppendLine(int indent, string info)
        {
            m_info.AppendLine(_getIndent(indent) + info);
        }

        private string _getIndent(int indent)
        {
            string t = string.Empty;
            for(int i = 0; i < indent; i++)
            {
                t += "\t";
            }
            return t;
        }

    }
}
