using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

namespace UnityEngine.Rendering.Universal.Editor.Utility
{

    public class CSFliesEncodingToolWindow : EditorWindow
    {

        private static GUIStyle _tipStyle;
        protected static GUIStyle TipStyle
        {
            get
            {
                if (_tipStyle == null)
                {
                    _tipStyle = new GUIStyle(EditorStyles.miniLabel);
                    _tipStyle.fontSize = 12;
                }
                return _tipStyle;
            }
        }

        private static CSFliesEncodingToolWindow m_Instance;

        [MenuItem("Window/FrameworkTools/CSFilesEncodingTool")]
        private static CSFliesEncodingToolWindow Init()
        {
            m_Instance = EditorWindow.GetWindow<CSFliesEncodingToolWindow>();
            //m_Instance.minSize = new Vector2(350, 300);
            m_Instance.titleContent = new GUIContent("CS文件编码管理");
            return m_Instance;
        }

        public static void DrawGUIButton(string label, Func<bool> verifunc, Action onClickDo, params GUILayoutOption[] options)
        {
            bool vf = verifunc();
            GUI.color = vf ? Color.white : Color.gray;
            if (GUILayout.Button(label, options))
            {
                if (!vf) return;
                onClickDo?.Invoke();
            }
            GUI.color = Color.white;
        }

        private bool m_NoBOM = false;

        private void OnGUI()
        {
            GUILayout.Space(12);
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("options");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                m_NoBOM = EditorGUILayout.ToggleLeft("转换为 UTF-8不带BOM?", m_NoBOM);
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            DrawGUIButton($"转换所选文件下所有CSFile为 UTF-8{(m_NoBOM ? "" : " (BOM)")} 编码格式", 
                () => Selection.objects != null && Selection.objects.Length > 0 && (Selection.objects[0] is DefaultAsset || Selection.objects[0] is TextAsset), 
                () => 
                { 
                    DoProcess(); 
                }, GUILayout.Height(32));
            GUILayout.Space(12);
            Repaint();
        }

        private void DoProcess()
        {
            try
            {
                List<string> filePathList = new List<string>();
                foreach (var Obj in Selection.objects)
                {
                    if(Obj is DefaultAsset)
                    {
                        string fullDir = AssetDatabase.GetAssetPath(Obj).ToNormalizedFullPath();
                        string[] filePaths = Directory.GetFiles(fullDir, "*.cs", SearchOption.AllDirectories);
                        foreach (string filePath in filePaths) 
                        {
                            filePathList.Add(filePath);
                        }
                    }else
                    {
                        string path = AssetDatabase.GetAssetPath(Obj);
                        if (!string.IsNullOrEmpty(path))
                        {
                            var info = new FileInfo(path);
                            if(info.Extension == ".cs" || info.Extension == ".txt" || info.Extension == ".cfg" || info.Extension == ".lua")
                                filePathList.Add(path);
                        }
                    }
                }

                if (filePathList.Count == 0) return;

                var newEncoding = new UTF8Encoding(!m_NoBOM);

                int count = filePathList.Count;
                for (int i = 0; i < count; i++)
                {
                    string path = filePathList[i];
                    string fileName = Path.GetFileName(path);
                    EditorUtility.DisplayProgressBar("进度", $"{i+1} / {count}", (float)(i + 1) / count);
                    string fileContent = File.ReadAllText(path);
                    if (!string.IsNullOrEmpty(fileContent))
                    {
                        File.WriteAllText(path, fileContent, newEncoding);
                    }
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();

            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

        }

    }

}


