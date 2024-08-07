#if FRAMEWORKDEF
#else

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using AORCore.Editor;

namespace AORCore
{

    /// <summary>
    /// 提供Editor下常用功能静态方法
    /// 
    /// Update date : 2021-05-26
    /// 
    /// </summary>
    public class EditorPlusMethods
    {
        #region  GUIContents Define
        //-------------------------- GUIContents Define ---------------------

        private static GUIContent m_CompilingLabel;
        private static GUIContent CompilingLabel
        {
            get
            {
                if (m_CompilingLabel == null)
                {
                    m_CompilingLabel = new GUIContent("Compiling Please Wait...", "正在编译中 ...");
                }
                return m_CompilingLabel;
            }
        }
        
        //-------------------------- GUIContents Define ----------------  end
        #endregion

        private static EditorApplication.CallbackFunction _UDDoOnce;
        private static Action _UDDoOnceDos;
        /// <summary>
        /// 编辑器在下一次Update时调用
        /// </summary>
        public static void NextEditorApplicationUpdateDo(Action doSomething)
        {

            _UDDoOnceDos += doSomething;
            if (_UDDoOnce == null)
            {
                _UDDoOnce = () =>
                {
                    if (_UDDoOnceDos != null)
                    {
                        Action doing = _UDDoOnceDos;
                        doing();
                        _UDDoOnceDos = null;
                    }
                    EditorApplication.update -= _UDDoOnce;
                    _UDDoOnce = null;
                };
                EditorApplication.update += _UDDoOnce;
            }


        }

        /// <summary>
        /// 在 Project 中创建一个ScriptableObject子类文件
        /// </summary>
        /// <typeparam name="T">ScriptableObject子类</typeparam>
        public static T CreateAssetFile<T>(string assetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            string dir;
            string name = assetName + ".asset";
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                dir = "Assets";
            }
            else
            {

                string dataPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
                if (Selection.objects[0] is DefaultAsset)
                {
                    dir = dataPath;
                }
                else
                {
                    if (string.IsNullOrEmpty(dataPath))
                    {
                        dir = "Assets";
                    }
                    else
                    {
                        dir = new EditorAssetInfo(dataPath).dirPath;
                    }
                }
            }
            string path = dir + "/" + name;
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        #region Edior GUI UI Draw

        /// <summary>
        /// 绘制 按钮-> <立即写入修改数据到文件>
        /// 
        /// @@@ 建议所有.Asset文件的Editor都配备此段代码
        /// 
        /// </summary>
        /// <param name="target">Editor.target</param>
        public static void Draw_AssetFileApplyImmediateUI(UnityEngine.Object target)
        {
            GUILayout.Space(13);
            GUI.color = Color.yellow;
            if (GUILayout.Button("立即写入数据到文件 (Save To Asset Immediate)", GUILayout.Height(26)))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
            GUI.color = Color.white;
        }

        /// <summary>
        /// 如果正在编译阶段 则绘制 提示UI  
        /// @@@ 建议所有EditorWindow.OnGUI都配备此段代码
        /// </summary>
        public static bool Draw_isCompilingUI()
        {
            if (EditorApplication.isCompiling)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(CompilingLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                return true;
            }
            return false;
        }

        public static void Draw_ProgressBar(float value, string label, int RectWidth = 18, int RectHeight = 18)
        {
            try {
                Rect rect = GUILayoutUtility.GetRect(RectWidth, RectHeight, "TextField");
                EditorGUI.ProgressBar(rect, value, label);
            }catch(Exception ex)
            {
                //do nothing
            }
        }

        /// <summary>
        /// 如果正在编译阶段 则绘制 提示 Notification UI  
        /// @@@ 建议所有EditorWindow.OnGUI都配备此段代码
        /// </summary>
        public static bool Draw_isCompilingNotification (EditorWindow editorWindow) {

            #region 正在编译提示
            if (EditorApplication.isCompiling)
            {
                editorWindow.ShowNotification(new GUIContent("Compiling Please Wait..."));
                editorWindow.Repaint();
                return true;
            }
            editorWindow.RemoveNotification();
            #endregion
            return false;
        }
        
        /// <summary>
        /// 绘制 测试窗口大小信息
        /// </summary>
        public static void Draw_DebugWindowSizeUI()
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(0,0, Screen.width,24), "WindowSize > width : " + Screen.width + " , height : " + Screen.height);
            GUI.color = Color.white;
        }

        public static void Draw_TitleLabelUI(string label, GUIStyle style = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (style != null)
                GUILayout.Label(label, style);
            else
                GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #endregion

        [Obsolete("此方法已迁移至EditorAssetInfo.FolderValid")]
        public static void FolderValid(string dir)
        {
            EditorAssetInfo.FolderValid(dir);
        }

        // ------------------------------------------ UsedTags 

        #region 标识计数方法集合

        //标识计数方法池
        private static readonly Dictionary<string, int> UsedTags = new Dictionary<string, int>();

        /// <summary>
        /// 计数Tag机制
        /// 
        /// 添加一个计数
        /// 
        /// </summary>
        public static int AddUsedTag(string tag)
        {
            if (UsedTags.ContainsKey(tag))
            {
                UsedTags[tag]++;
                return UsedTags[tag];
            }
            else
            {
                UsedTags.Add(tag, 1);
                return UsedTags[tag];
            }
        }

        public static int SubUsedTag(string tag)
        {
            if (UsedTags.ContainsKey(tag))
            {
                UsedTags[tag]--;
                if (UsedTags[tag] <= -0) {
                    UsedTags.Remove(tag);
                    return 0;
                }
                return UsedTags[tag];
            }
            else
                return 0;
        }

        public static int UsedTagCount(string tag)
        {
            if (UsedTags.ContainsKey(tag))
                return UsedTags[tag];
            else
                return 0;
        }

        #endregion

        // ------------------------------------------ UsedTags End

        // ------------------------------------------ HashCodePool

        #region HashCode 静态池

        private static readonly HashSet<int> _hashCodePool = new HashSet<int>();

        public static bool RegisterHashCodeInPool(int hashCode)
        {
            if (!_hashCodePool.Contains(hashCode))
            {
                _hashCodePool.Add(hashCode);
                return true;
            }
            return false;
        }

        public static void UnregisterHashCodeFromPool(int hashCode)
        {
            if (_hashCodePool.Contains(hashCode))
                _hashCodePool.Remove(hashCode);
        }

        #endregion

        // ------------------------------------------ HashCodePool End

        // ------------------------------------------ PlusDefindWindow 

        #region 关于Unity编辑界面预制窗口的方法集合

        public enum PlusDefindWindow
        {
            AnimationWindow,
        }

        private static string _getPlusDefindWindowFullName(PlusDefindWindow defind)
        {
            switch (defind)
            {
                case PlusDefindWindow.AnimationWindow:
                    return "UnityEditor.AnimationWindow";
            }
            return null;
        }

        public static EditorWindow GetPlusDefindWindow(PlusDefindWindow defind)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(EditorWindow));
            string fullName = _getPlusDefindWindowFullName(defind);
            if (string.IsNullOrEmpty(fullName)) return null;
            Type t = assembly.GetType(fullName);
            if (t == null) return null;
            EditorWindow aw = EditorWindow.GetWindow(t);
            if (aw == null) return null;
            return aw;
        }

        #endregion

        // ------------------------------------------ PlusDefindWindow End

        public static string GetNodePath(Transform trans)
        {
            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(trans.gameObject);
            if(root)
            {
                if(root.transform == trans)
                {
                    if(trans.parent)
                    {
                        GameObject parentRoot = PrefabUtility.GetNearestPrefabInstanceRoot(trans.parent.gameObject);
                        if(parentRoot)
                            return _getNodePathLoop(trans, parentRoot);
                       
                    }
                    return _getNodePathLoop(trans);
                }
                else
                    return _getNodePathLoop(trans, root);
            }
            return _getNodePathLoop(trans);
        }

        public static string _getNodePathLoop(Transform t, GameObject root = null, string path = null)
        {
            if(string.IsNullOrEmpty(path))
                path = t.gameObject.name;
            else
                path = t.gameObject.name + "/" + path;

            if(root && t.gameObject == root)
                return path;

            if(t.parent != null)
                return _getNodePathLoop(t.parent, root, path);
            else
                return path;
        }

    }

}

#endif