using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

#if FRAMEWORKDEF
using Framework.Extends;
using Framework.Editor;
#else
using AORCore;
#endif

namespace UnityEngine.Rendering.Universal.Utility.Editor
{

    public class AssetBundlesExplorerWindow :UnityEditor.EditorWindow
    {

        #region GUIStyles and GUIContents DeFines

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

        private static GUIContent _loadABLabel;
        protected static GUIContent loadABLabel
        {
            get {
                if(_loadABLabel == null)
                {
                    _loadABLabel = new GUIContent("加载AssetBundle文件");
                }
                return _loadABLabel;
            }
        }

        #endregion

        private static AssetBundlesExplorerWindow _instance;

        [MenuItem("Window/FrameworkTools/AssetBundle/AssetBundlesExplorer", false, 10101)]
        public static AssetBundlesExplorerWindow init()
        {
            _instance = UnityEditor.EditorWindow.GetWindow<AssetBundlesExplorerWindow>();
            //_instance.minSize = new Vector2(495, 612);

            return _instance;
        }

        private void Awake()
        {
            for (int i = 0; i < Caching.cacheCount; i++)
            {
                Caching.GetCacheAt(i).ClearCache();
            }
            Caching.ClearCache();
        }

        private void OnDestroy()
        {
            foreach (var kv in m_bundleInfoCacheDic)
            {
                m_searchedInfos.Remove(kv.Value);
                kv.Value.Destroy();
            }
            m_bundleInfoCacheDic.Clear();
        }

        private readonly Dictionary<string, AssetBundleInfo> m_bundleInfoCacheDic = new Dictionary<string, AssetBundleInfo>();
        private bool m_ignoreCase
        {
            get
            {
                return EditorPrefs.GetBool(EditorIdentifier.EditorIdentifierTag + "Framework.Utility.Editor.AssetBundlesExplorerWindow.ignoreCase");
            }
            set
            {
                EditorPrefs.SetBool(EditorIdentifier.EditorIdentifierTag + "Framework.Utility.Editor.AssetBundlesExplorerWindow.ignoreCase", value);
            }
        }
        private string m_searchKey;
        private readonly List<AssetBundleInfo> m_searchedInfos = new List<AssetBundleInfo>();
        private readonly List<AssetBundleInfo> m_dels = new List<AssetBundleInfo>();
        private Vector2 m_mainSrcollPos;

        private void OnGUI()
        {
            //if(EditorApplication.isCompiling)
            //    Close();

            GUILayout.Space(15);
            _draw_toolTitle_Label(" 简易AssetBundle包查看器 ");
            GUILayout.Space(15);

            _draw_LoadAssetBundleUI();

            GUILayout.Space(12);

            _draw_mainSearchUI();

            GUILayout.Space(5);

            m_mainSrcollPos = GUILayout.BeginScrollView(m_mainSrcollPos, "box");
            {
                GUILayout.Space(5);

                if (m_searchedInfos.Count > 0)
                {

                    for (int i = 0; i < m_searchedInfos.Count; i++)
                    {
                        _draw_AssetBundleInfoUI(i, m_searchedInfos[i]);
                    }

                    //del
                    if(m_dels.Count > 0)
                    {
                        foreach (var del in m_dels)
                        {
                            unloadAssetBundle(del);
                        }
                        m_dels.Clear();
                    }

                }

                GUILayout.Space(5);
            }
            GUILayout.EndScrollView();

        }

        private void _draw_mainSearchUI()
        {

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                //GUILayout.Label("过滤器");
                _draw_toolTitle_Label("过滤器");
                GUILayout.Space(5);
                bool nIc = EditorGUILayout.Toggle("忽略大小写", m_ignoreCase);
                if (nIc != m_ignoreCase)
                {
                    m_ignoreCase = nIc;
                }
                m_searchKey = EditorGUILayout.TextField("过滤关键字", m_searchKey);
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            m_searchedInfos.Clear();
            if (string.IsNullOrEmpty(m_searchKey))
            {
                m_searchedInfos.AddRange(m_bundleInfoCacheDic.Values);
            }
            else
            {
                foreach (var kv in m_bundleInfoCacheDic)
                {
                    //Search Method
                    if(searchKeyFilter(kv.Value))
                    {
                        m_searchedInfos.Add(kv.Value);
                    }
                }
            }
        }

        private bool searchKeyFilter(AssetBundleInfo info)
        {
            return (m_ignoreCase ? info.bundle.name.ToLower().Contains(m_searchKey.ToLower()) : info.bundle.name.Contains(m_searchKey));
        }

        private void _draw_LoadAssetBundleUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                _draw_toolTitle_Label(" ------------------------- ");
                GUILayout.Space(5);

                if(GUILayout.Button(loadABLabel, GUILayout.Height(26)))
                {
                    string loadPath = EditorUtility.OpenFilePanel("Open AssetBundle File", "", "");
                    if(!string.IsNullOrEmpty(loadPath))
                    {
                        loadAssetBundle(loadPath);
                    }
                }

                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void _draw_AssetBundleInfoUI (int index, AssetBundleInfo info)
        {

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(info.bundle.name, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(" - ", GUILayout.Width(25)))
                    {
                        m_dels.Add(info);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);

                    for (int i = 0; i < info.assets.Length; i++)
                    {
                        if (i > 0)
                            GUILayout.Space(2);

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(info.assets[i].asset.name);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(info.assets[i].tpyeName);
                            //GUILayout.FlexibleSpace();
                            //GUILayout.Label(info.assets[i].sizeLabel);
                        }
                        GUILayout.EndHorizontal();

                    }

                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                foreach(var p in info.scenePaths)
                {
                    GUILayout.Label(p);
                }

                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

        }

        #region inner datas

        private struct AssetObjectInfo
        {
            //private static string GetSizeString(long size)
            //{
            //    if (size < 1024L)
            //    {
            //        return string.Format("{0} Bytes", size.ToString());
            //    }

            //    if (size < 1024L * 1024L)
            //    {
            //        return string.Format("{0} KB", (size / 1024f).ToString("F2"));
            //    }

            //    if (size < 1024L * 1024L * 1024L)
            //    {
            //        return string.Format("{0} MB", (size / 1024f / 1024f).ToString("F2"));
            //    }

            //    if (size < 1024L * 1024L * 1024L * 1024L)
            //    {
            //        return string.Format("{0} GB", (size / 1024f / 1024f / 1024f).ToString("F2"));
            //    }

            //    return string.Format("{0} TB", (size / 1024f / 1024f / 1024f / 1024f).ToString("F2"));
            //}

            public UnityEngine.Object asset;
            //public Type type;
            public string tpyeName;
            //public long size;
            //public string sizeLabel;

            public AssetObjectInfo(UnityEngine.Object asset)
            {
                this.asset = asset;
                this.tpyeName = this.asset.GetType().Name;
            }

            //public AssetObjectInfo(long size, UnityEngine.Object asset)
            //{
            //    this.asset = asset;
            //    this.type = this.asset.GetType();
            //    this.tpyeName = this.type.Name;
            //    this.size = size;
            //    this.sizeLabel = GetSizeString(this.size);
            //}

            public void Destroy()
            {
                if (this.asset)
                {
                    GameObject.DestroyImmediate(asset, true);
                }
                //this.type = null;
            }
        }

        private struct AssetBundleInfo 
        {

            public static long CulObjectSize(UnityEngine.Object obj)
            {
                if(obj is Texture2D)
                {
                    return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(obj);

                }
                else
                {
                    return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(obj);
                }
            }

            public AssetBundle bundle;
            public string path;
            public string[] assetNames;
            public string[] scenePaths;
            public AssetObjectInfo[] assets;
            public AssetBundleInfo(string path, AssetBundle bundle)
            {
                this.bundle = bundle;
                this.path = path;
                this.assetNames = bundle.GetAllAssetNames();
                this.scenePaths = bundle.GetAllScenePaths();
                assets = new AssetObjectInfo[assetNames.Length];
                //AssetBundleRequest request = this.bundle.LoadAllAssetsAsync();
                //request.
                for (int i = 0; i < assetNames.Length; i++)
                {
                    string p = assetNames[i];
                    UnityEngine.Object obj = bundle.LoadAsset(p);
                    //long size = CulObjectSize(obj);
                    //assets[i] = new AssetObjectInfo(size, obj);
                    assets[i] = new AssetObjectInfo(obj);
                }
            }

            public void Destroy()
            {
                if(assets != null)
                {
                    for (int i = assets.Length - 1; i > 0; --i)
                    {
                        assets[i].Destroy();
                    }
                    assets = null;
                }
                if(assetNames != null)
                {
                    assetNames = null;
                }
                if (scenePaths != null)
                {
                    scenePaths = null;
                }
                if (bundle)
                {
                    bundle.Unload(true);
                }
                bundle = null;
            }
        }

        #endregion

        #region inner methods
        private void _draw_toolTitle_Label(string label)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(label, titleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private void unloadAssetBundle(AssetBundleInfo info)
        {
            m_searchedInfos.Remove(info);
            m_bundleInfoCacheDic.Remove(info.path);
            info.Destroy();
        }

        private void loadAssetBundle(string loadPath)
        {

            if(!m_bundleInfoCacheDic.ContainsKey(loadPath))
            {

                AssetBundle assetBundle = AssetBundle.LoadFromFile(loadPath);
                if(assetBundle)
                {
                    AssetBundleInfo info = new AssetBundleInfo(loadPath, assetBundle);
                    m_bundleInfoCacheDic.Add(loadPath, info);
                }
                else
                {
                    //Error load Fail !
                }
            }

        }

        #endregion

    }

}
