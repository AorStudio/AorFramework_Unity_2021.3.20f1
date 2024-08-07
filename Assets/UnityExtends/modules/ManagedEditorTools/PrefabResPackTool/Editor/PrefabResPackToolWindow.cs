using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.Animations;
using CommonSerializeObjectGUI.Editor;
using CommonSerializeObjectGUI;
using AORCore.Editor;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Configuration;
using System.IO;
using Codice.Client.BaseCommands;
using System.Security.Cryptography;

namespace UnityEngine.Rendering.Universal.Utility.Editor
{

    public class PrefabResPackToolWindow : UnityEditor.EditorWindow
    {

        private static GUIStyle _titleStyle;
        protected static GUIStyle TitleStyle
        {
            get
            {
                if (_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(EditorStyles.largeLabel);
                    _titleStyle.fontSize = 16;
                }
                return _titleStyle;
            }
        }

        private static GUIStyle _textStyle;
        protected static GUIStyle TextStyle
        {
            get
            {
                if (_textStyle == null)
                {
                    _textStyle = new GUIStyle(EditorStyles.miniLabel);
                    _textStyle.wordWrap = true;
                }
                return _textStyle;
            }
        }

        #region Inner Data class

        [Serializable]
        private class OptionData
        {
            [CSOFieldLabel("依赖资源显示为路径")]
            public bool DisplayDPath;
            [CSOFieldLabel("依赖资源路径检查")]
            public bool CheckPathRule;
            [CSOFieldLabel("显示依赖Shaders")]
            public bool ShowShaderDependencies;
            [CSOFieldLabel("指定目标路径")]
            public bool IsSpecifySavePath;
            [CSOFieldLabel("整理目标路径"), CSOIgnoreFieldOnDrawInspectorGUI, CSODirPathSelectTool]
            public string SpecifySavePath;
        }

        private class Collector : IDisposable
        {
            public readonly List<UnityEngine.Object> Animations = new List<UnityEngine.Object>();
            public readonly List<UnityEngine.Object> Materials = new List<UnityEngine.Object>();
            public readonly List<UnityEngine.Object> Models = new List<UnityEngine.Object>();
            public readonly List<UnityEngine.Object> Textures = new List<UnityEngine.Object>();
            public readonly List<UnityEngine.Object> Others = new List<UnityEngine.Object>();
            public readonly List<UnityEngine.Object> Shaders = new List<UnityEngine.Object>();

            public readonly List<KeyValuePair<bool, string>> AnimationsPathes = new List<KeyValuePair<bool, string>>();
            public readonly List<KeyValuePair<bool, string>> MaterialsPathes = new List<KeyValuePair<bool, string>>();
            public readonly List<KeyValuePair<bool, string>> ModelsPathes = new List<KeyValuePair<bool, string>>();
            public readonly List<KeyValuePair<bool, string>> TexturesPathes = new List<KeyValuePair<bool, string>>();
            public readonly List<KeyValuePair<bool, string>> OthersPathes = new List<KeyValuePair<bool, string>>();
            public readonly List<KeyValuePair<bool, string>> ShaderPathes = new List<KeyValuePair<bool, string>>();

            public readonly Dictionary<UnityEngine.Object, string> PathDic = new Dictionary<Object, string>();

            public readonly HashSet<string> PathHash = new HashSet<string>();

            public void Dispose()
            {

                Animations.Clear();
                Materials.Clear();
                Models.Clear();
                Textures.Clear();
                Others.Clear();

                AnimationsPathes.Clear();
                MaterialsPathes.Clear();
                ModelsPathes.Clear();
                TexturesPathes.Clear();
                OthersPathes.Clear();

                PathDic.Clear();
                PathHash.Clear();
            }

        }

        private class CollateProcessData : IDisposable 
        {

            public readonly List<KeyValuePair<string, string>> MovingProcessInfos = new List<KeyValuePair<string, string>>();
            public void Dispose()
            {
                MovingProcessInfos.Clear();
            }
        }


        #endregion

        private static PrefabResPackToolWindow _instance;

        [MenuItem("Window/FrameworkTools/Prefab/Prefab资源整理工具", false, 10100)]
        public static PrefabResPackToolWindow init()
        {

            _instance = UnityEditor.EditorWindow.GetWindow<PrefabResPackToolWindow>();
            _instance.titleContent = new GUIContent("PrefabResPackTool");
            //_instance.minSize = new Vector2(495, 612);
            return _instance;
        }

        private string m_editorSaveKey;

        private OptionData m_optionData;
        private CommonSerializeObjectGUIDrawer m_optionDrawer;
        private bool m_optionDataDirty;
        private Vector2 m_mainScrollPos = Vector2.zero;

        private readonly Dictionary<GameObject, Collector> m_CollectorDic = new Dictionary<GameObject, Collector>();
        private readonly Dictionary<GameObject, string> m_BasePathDic = new Dictionary<GameObject, string>();
        private readonly Dictionary<GameObject, CollateProcessData> m_MovingProcessInfoDic = new Dictionary<GameObject, CollateProcessData>();

        private void OnValidate()
        {
            try
            {
                Awake();
            }
            catch { }
        }

        private void Awake()
        {
            m_editorSaveKey = $"{Application.dataPath}/PrefabResPackToolWindow.OptionData";
            m_optionData = LoadFormEditorPrefs(m_editorSaveKey);
            m_optionDrawer = new CommonSerializeObjectGUIDrawer(m_optionData);
            m_optionDrawer.DisplayTitle = false;
        }

        private void OnDestroy()
        {

            if(m_optionDrawer != null)
            {
                m_optionDrawer.Dispose();
                m_optionDrawer = null;
            }

            if (m_optionData != null)
            {
                SaveToEditorPrefs(m_editorSaveKey, m_optionData);
                m_optionData = null;
            }

            ClearCollectorsCache();
            m_MovingProcessInfoDic.Clear();
        }

        private void OnGUI()
        {
            GUILayout.Space(5);
            Draw_TitleAndDescUI();
            GUILayout.Space(5);
            Draw_optionsUI();
            GUILayout.Space(5);
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
            {
                GUILayout.Space(5);
                m_mainScrollPos = GUILayout.BeginScrollView(m_mainScrollPos, "box");
                {
                    GUILayout.Space(5);
                    for (int i = 0; i < Selection.gameObjects.Length; i++)
                    {
                        if (i > 0)
                            GUILayout.Space(2);
                        var t = Selection.gameObjects[i];
                        if(CheckIsFBX(t))
                        {
                            EditorGUILayout.ObjectField("FBX", t, typeof(GameObject), false);
                        }
                        else
                        {
                            Draw_TargetUI(Selection.gameObjects[i]);
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndScrollView();
                if (m_optionData.DisplayDPath)
                {
                    Draw_CollateProcessUI();
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUI.color = Color.gray;
                DrawHCenterLabel("[未选择预制体对象]", TitleStyle);
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(5);

            if(m_optionDataDirty)
            {
                SaveToEditorPrefs(m_editorSaveKey, m_optionData);
                m_optionDataDirty = false;
            }

            Repaint();
        }

        private void Draw_TitleAndDescUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(12);
                DrawHCenterLabel("【 预制体资源整理工具 】", TitleStyle);
                DrawHCenterLabel("在Project面板里选择需要整理的预制体", TextStyle);
                GUILayout.Space(12);
            }
            GUILayout.EndVertical();
        }

        private void Draw_optionsUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                DrawHCenterLabel("[ 工具选项 ]", TitleStyle);
                GUILayout.Space(5);
                if (m_optionDrawer.DrawInspectorGUI())
                    m_optionDataDirty = true;
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void Draw_TargetUI(GameObject target)
        {

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                EditorGUILayout.ObjectField("预制体", target, typeof(GameObject), false);
                GUILayout.Space(12);
                GUILayout.Label("Dependencies:");
                GUILayout.Space(5);
                var collector = GetOrCreateCollector(target, out string basePath);

                if(m_optionData.IsSpecifySavePath && !string.IsNullOrEmpty(m_optionData.SpecifySavePath))
                    basePath = m_optionData.SpecifySavePath;

                EditorGUI.indentLevel++;
                Draw_CollectorDepenSubUI("Animations:", new string[] {
                                        $"{basePath}/Animations", $"{basePath}/animations",
                                        $"{basePath}/Animation", $"{basePath}/animation"
                                    }, collector.AnimationsPathes, collector.Animations, collector.PathDic);
                Draw_CollectorDepenSubUI("Materials:", new string[] {
                                        $"{basePath}/Materials", $"{basePath}/Material",
                                        $"{basePath}/materials", $"{basePath}/material"
                                    }, collector.MaterialsPathes, collector.Materials, collector.PathDic);
                Draw_CollectorDepenSubUI("Models:", new string[] {
                                        $"{basePath}/Models", $"{basePath}/Model",
                                        $"{basePath}/models", $"{basePath}/model",
                                        $"{basePath}/Mesh", $"{basePath}/mesh",
                                        $"{basePath}/Meshes", $"{basePath}/meshes",
                                        $"{basePath}/FBX", $"{basePath}/fbx"
                                    }, collector.ModelsPathes, collector.Models, collector.PathDic);    
                Draw_CollectorDepenSubUI("Textures:", new string[] {
                                        $"{basePath}/Textures", $"{basePath}/textures",
                                        $"{basePath}/Texture", $"{basePath}/texture"
                                    }, collector.TexturesPathes, collector.Textures, collector.PathDic);
                Draw_CollectorDepenSubUI("Others:", null, collector.OthersPathes, collector.Others, collector.PathDic);
                if (m_optionData.ShowShaderDependencies)
                    Draw_CollectorDepenSubUI("Shaders:", null, collector.ShaderPathes, collector.Shaders, collector.PathDic);
                EditorGUI.indentLevel--;
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void Draw_CollectorDepenSubUI(string title, string[] checkPaths, 
            List<KeyValuePair<bool, string>> pathes, List<UnityEngine.Object> list, Dictionary<UnityEngine.Object, string> pathDic)
        {
            if (list.Count == 0) return;
            EditorGUILayout.LabelField(title);
            EditorGUI.indentLevel++;

            if (m_optionData.DisplayDPath)
            {
                for (int i = 0; i < pathes.Count; i++)
                {
                    var kv = pathes[i];
                    bool tag = kv.Key;
                    string path = kv.Value;

                    bool warning = CheckPathInRule(path, checkPaths);
                    GUILayout.BeginHorizontal();
                    {
                        bool nTag = EditorGUILayout.Toggle(tag, GUILayout.Width(50));
                        if (tag != nTag)
                        {
                            pathes[i] = new KeyValuePair<bool, string>(nTag, path);
                        }
                        GUI.color = warning ? Color.yellow : Color.white;
                        EditorGUILayout.LabelField(path);
                        GUI.color = Color.white;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i];
                    GUILayout.BeginHorizontal();
                    {
                        if(obj == null)
                        {
                            GUI.color = Color.red;
                            EditorGUILayout.ObjectField(null, typeof(UnityEngine.Object), false);
                            GUI.color = Color.white;
                        }
                        else
                        {
                            string path = pathDic[obj];
                            bool warning = CheckPathInRule(path, checkPaths);
                            GUI.color = warning ? Color.yellow : Color.white;
                            EditorGUILayout.ObjectField(obj, obj.GetType(), false);
                            GUI.color = Color.white;
                        }
                    }
                    GUILayout.EndHorizontal();

                }
            }


            EditorGUI.indentLevel--;
        }

        private void Draw_CollateProcessUI()
        {

            if (m_optionData.IsSpecifySavePath)
            {
                if (m_optionDrawer.DrawSingleFieldGUI("SpecifySavePath"))
                    m_optionDataDirty = true;
            }
            
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button( "【整理开始】", GUILayout.Height(26)))
                {
                    if (EditorUtility.DisplayDialog("提示", "确定开始整理选择资源?!", "确定", "取消"))
                    {
                        CollateProcess();
                        ClearCollectorsCache();
                        AssetDatabase.Refresh();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        //------------------------------

        private void DrawHCenterLabel(string title, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(title, style);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        private bool CheckPathInRule(string path, string[] checkPaths)
        {
            bool warning = false;
            if (m_optionData.CheckPathRule && checkPaths != null && checkPaths.Length > 0)
            {
                bool pass = false;
                foreach (var checkPath in checkPaths)
                {
                    if (string.IsNullOrEmpty(checkPath))
                        continue;
                    pass = path.Contains(checkPath);
                    if (pass)
                        break;
                }
                warning = !pass;
            }
            return warning;
        }

        private bool CheckIsFBX(GameObject gameObject)
        {
            var type = PrefabUtility.GetPrefabAssetType(gameObject);
            return type == PrefabAssetType.Model;
        }

        private Collector GetOrCreateCollector(GameObject key, out string basePath)
        {
            if (m_CollectorDic.TryGetValue(key, out var collector))
            {
                basePath = m_BasePathDic[key];
                return collector;
            }
            
            collector = new Collector();
            var dependencies = EditorUtility.CollectDependencies(new Object[] { key });
            if (dependencies != null && dependencies.Length > 0)
            {
                List<UnityEngine.Object> DelayedProcessingList = new List<Object>();
                foreach (var dependency in dependencies)
                {
                    string path = AssetDatabase.GetAssetPath(dependency);
                    //延后处理此分类(同时处理可能造成FBX文件分类错误)
                    if (dependency is AnimatorController || dependency is AnimatorOverrideController || dependency is AnimationClip || dependency is Avatar || dependency is AvatarMask)
                    {
                        DelayedProcessingList.Add(dependency);
                    }
                    else if(dependency is Mesh || (dependency is GameObject && CheckIsFBX(dependency as GameObject)))
                    {
                        collector.Models.Add(dependency);
                        if (!collector.PathHash.Contains(path))
                        {
                            collector.PathHash.Add(path);
                            collector.ModelsPathes.Add(new KeyValuePair<bool, string>(true, path));
                        }
                    }
                    else if (dependency is Material)
                    {
                        collector.Materials.Add(dependency);
                        if (!collector.PathHash.Contains(path))
                        {
                            collector.PathHash.Add(path);
                            collector.MaterialsPathes.Add(new KeyValuePair<bool, string>(true, path));
                        }
                    }
                    else if (dependency is Texture)
                    {
                        collector.Textures.Add(dependency);
                        if (!collector.PathHash.Contains(path))
                        {
                            collector.PathHash.Add(path);
                            collector.TexturesPathes.Add(new KeyValuePair<bool, string>(true, path));
                        }
                    }
                    else if ( dependency is Shader)
                    {
                        collector.Shaders.Add(dependency);
                        if (!collector.PathHash.Contains(path))
                        {
                            collector.PathHash.Add(path);
                            collector.ShaderPathes.Add(new KeyValuePair<bool, string>(true, path));
                        }
                    }
                    else
                    {
                        if (dependency is GameObject || dependency is Transform ||  dependency is Component || dependency is MonoScript || dependency is null)
                            continue;

                        collector.Others.Add(dependency);
                        if (!collector.PathHash.Contains(path))
                        {
                            collector.PathHash.Add(path);
                            collector.OthersPathes.Add(new KeyValuePair<bool, string>(true, path));
                        }
                    }
                    collector.PathDic.Add(dependency, path);
                }

                if(DelayedProcessingList.Count > 0)
                {
                    foreach (var dependency in DelayedProcessingList)
                    {
                        collector.Animations.Add(dependency);
                        string path = AssetDatabase.GetAssetPath(dependency);
                        if (!collector.PathHash.Contains(path))
                        {
                            collector.PathHash.Add(path);
                            collector.AnimationsPathes.Add(new KeyValuePair<bool, string>(true, path));
                        }
                    }
                    DelayedProcessingList.Clear();
                }

            }
            m_CollectorDic.Add(key, collector);
            string loadPath = AssetDatabase.GetAssetPath(key);
            if (!string.IsNullOrEmpty(loadPath))
            {
                EditorAssetInfo info = new EditorAssetInfo(loadPath);
                string dirName = info.dirName.ToLower();
                if (dirName == "prefab" || dirName == "prefabs")
                    loadPath = info.parentDirPath;
                else
                    loadPath = info.dirPath;
            }
            m_BasePathDic.Add(key, loadPath);
            basePath = loadPath;
            return collector;
        }

        private void ClearCollectorsCache()
        {
            if (m_CollectorDic.Count > 0)
            {
                foreach (var kv in m_CollectorDic)
                {
                    kv.Value.Dispose();
                }
                m_BasePathDic.Clear();
            }
            m_CollectorDic.Clear();
        }

        private OptionData LoadFormEditorPrefs(string saveKey)
        {
            string json = EditorPrefs.GetString(saveKey);
            if (string.IsNullOrEmpty(json))
            {
                var data = new OptionData();
                data.CheckPathRule = true;
                data.DisplayDPath = true;
                return data;
            }
            else
                return JsonUtility.FromJson<OptionData>(json);
        }

        private void SaveToEditorPrefs(string saveKey, OptionData data)
        {
            EditorPrefs.SetString(saveKey, JsonUtility.ToJson(data));
        }

        private void BuildingMovingProcessInfoDic()
        {
            m_MovingProcessInfoDic.Clear();
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
            {
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    var target = Selection.gameObjects[i];
                    if (!CheckIsFBX(target))
                    {
                        var collector = GetOrCreateCollector(target, out string basePath);
                        if (m_optionData.IsSpecifySavePath && !string.IsNullOrEmpty(m_optionData.SpecifySavePath))
                            basePath = m_optionData.SpecifySavePath;

                        string fullDir = basePath.ToNormalizedFullPath();
                        if(!Directory.Exists(fullDir))
                        {
                            Directory.CreateDirectory(fullDir);
                            AssetDatabase.Refresh();
                        }

                        var info = new CollateProcessData();
                        //Models
                        foreach (var kv in collector.ModelsPathes)
                        {
                            if (!kv.Key) continue;
                            string path = kv.Value;
                            string dirName = TryFindDirName(basePath, "Models", new string[] { "Models", "Model", "Meshes", "Mesh", "FBX" });
                            EditorAssetInfo e = new EditorAssetInfo(path);
                            string desPath = $"{basePath}/{dirName}/{e.name}{e.extension}";
                            if (path == desPath)
                                continue;
                            info.MovingProcessInfos.Add(new KeyValuePair<string, string>(path, desPath));
                        }
                        //Textures
                        foreach (var kv in collector.TexturesPathes)
                        {
                            if (!kv.Key) continue;
                            string path = kv.Value;
                            string dirName = TryFindDirName(basePath, "Textures", new string[] { "Textures", "Texture", "Bitmaps", "Bitmap" });
                            EditorAssetInfo e = new EditorAssetInfo(path);
                            string desPath = $"{basePath}/{dirName}/{e.name}{e.extension}";
                            if (path == desPath)
                                continue;
                            info.MovingProcessInfos.Add(new KeyValuePair<string, string>(path, desPath));
                        }
                        //Materials
                        foreach (var kv in collector.MaterialsPathes)
                        {
                            if (!kv.Key) continue;
                            string path = kv.Value;
                            string dirName = TryFindDirName(basePath, "Materials", new string[] { "Materials", "Material", "Mats", "Mat" });
                            EditorAssetInfo e = new EditorAssetInfo(path);
                            string desPath = $"{basePath}/{dirName}/{e.name}{e.extension}";
                            if (path == desPath)
                                continue;
                            info.MovingProcessInfos.Add(new KeyValuePair<string, string>(path, desPath));
                        }
                        //Animations
                        foreach (var kv in collector.AnimationsPathes)
                        {
                            if (!kv.Key) continue;
                            string path = kv.Value;
                            string dirName = TryFindDirName(basePath, "Animations", new string[] { "Animations", "Animation", "Anims", "Anim" });
                            EditorAssetInfo e = new EditorAssetInfo(path);
                            string desPath = $"{basePath}/{dirName}/{e.name}{e.extension}";
                            if (path == desPath)
                                continue;
                            info.MovingProcessInfos.Add(new KeyValuePair<string, string>(path, desPath));
                        }
                        //Others
                        foreach (var kv in collector.OthersPathes)
                        {
                            if (!kv.Key) continue;
                            string path = kv.Value;
                            string dirName = TryFindDirName(basePath, "Others", new string[] { "Others", "Other", "Misc" });
                            EditorAssetInfo e = new EditorAssetInfo(path);
                            string desPath = $"{basePath}/{dirName}/{e.name}{e.extension}";
                            if (path == desPath)
                                continue;
                            info.MovingProcessInfos.Add(new KeyValuePair<string, string>(path, desPath));
                        }
                        m_MovingProcessInfoDic.Add(target, info);
                    }
                }
            }
        }

        private void CollateProcess()
        {

            BuildingMovingProcessInfoDic();

            foreach (var kv in m_MovingProcessInfoDic)
            {

                var target = kv.Key;
                var info = kv.Value;
                foreach (var pathKV in info.MovingProcessInfos)
                {
                    var src = pathKV.Key;
                    var dst = pathKV.Value;
                    var errorMessage = AssetDatabase.MoveAsset(src, dst);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Debug.LogError(errorMessage);
                    }
                }

                //Target Moving ...
                string tSrc = AssetDatabase.GetAssetPath(target);
                if (!string.IsNullOrEmpty(tSrc))
                {
                    EditorAssetInfo e = new EditorAssetInfo(tSrc);
                    var collector = GetOrCreateCollector(target, out string basePath);
                    if (m_optionData.IsSpecifySavePath && !string.IsNullOrEmpty(m_optionData.SpecifySavePath))
                        basePath = m_optionData.SpecifySavePath;

                    string tDst = $"{basePath}/{e.name}{e.extension}";

                    if (tSrc == tDst)
                        continue;

                    var errorMessage = AssetDatabase.MoveAsset(tSrc, tDst);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Debug.LogError(errorMessage);
                    }
                }

            }

        }

        private string TryFindDirName(string basePath, string createDirName, string[] checkNames)
        {
            string fullBasePath = basePath.ToNormalizedFullPath();
            if (fullBasePath.EndsWith("/"))
                fullBasePath = fullBasePath.Substring(0, fullBasePath.Length - 1);
            foreach (var name in checkNames) 
            {
                if (Directory.Exists($"{fullBasePath}/{name}"))
                    return name;
            }

            string fullDir = $"{fullBasePath}/{createDirName}";
            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
                AssetDatabase.Refresh();
            }
            return createDirName;
        }

    }

}
