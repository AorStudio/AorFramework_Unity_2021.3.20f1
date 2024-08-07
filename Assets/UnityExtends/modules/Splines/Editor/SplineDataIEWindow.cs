
using UnityEditor;
using UnityEngine;


#if FRAMEWORKDEF
using Framework.Editor;
using AorBaseUtility;
#else
using AORCore;
using AORCore.Editor;
#endif
using System;

namespace UnityEngine.Rendering.Universal.module.Editor
{
    public class SplineDataIEWindow :EditorWindow
    {


        private static SplineDataIEWindow m_instance;

        private static string[] m_toolbarLabels = new string[] { "导入数据", "导出数据" };

        public static SplineDataIEWindow init(Spline spline)
        {
            m_instance = EditorWindow.GetWindow<SplineDataIEWindow>("SplineDataIEWindow");
            m_instance.Setup(spline);
            return m_instance;
        }

        private int m_toolIndex;

        private void OnGUI()
        {

            GUILayout.Space(5);

            _draw_title_UI();

            GUILayout.Space(5);

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("目标Spline");

                m_spline = (Spline)EditorGUILayout.ObjectField(m_spline, typeof(Spline), true);

                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);

            if(!m_spline)
                return;

            m_toolIndex = GUILayout.Toolbar(m_toolIndex, m_toolbarLabels);

            switch(m_toolIndex)
            {

                case 1:
                    {
                        _draw_dataExport_UI();
                    }
                    break;
                default: //0
                    {
                        _draw_dataImport_UI();
                    }
                    break;
            }

            GUILayout.Space(5);
        }

        private Spline m_spline;
        public void Setup(Spline spline)
        {
            m_spline = spline;
        }

        void OnDestroy()
        {
            m_spline = null;
        }

        private void _draw_title_UI()
        {
            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Space(12);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Spline 数据导入导出工具");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(12);
            }
            EditorGUILayout.EndVertical();
        }


        private TextAsset m_importTextAsset;
        private int m_importType;
        private void _draw_dataImport_UI()
        {

            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);

                GUILayout.Label("导入数据 ( JSON / TXT )");
                TextAsset newImport = (TextAsset)EditorGUILayout.ObjectField(m_importTextAsset, typeof(TextAsset), false);
                if(m_importTextAsset != newImport)
                {
                    m_importTextAsset = newImport;
                    if(m_importTextAsset == null)
                    {
                        m_importType = 0;
                    }
                    else
                    {
                        string path = AssetDatabase.GetAssetPath(m_importTextAsset);
                        EditorAssetInfo info = new EditorAssetInfo(path);
                        if(info.suffix == ".json")
                        {
                            m_importType = 1;
                        }
                        else if(info.suffix == ".txt")
                        {
                            m_importType = 2;
                        }
                        else
                        {
                            m_importType = 0;
                        }
                    }
                }

                if(m_importTextAsset && m_importType > 0)
                {
                    GUILayout.FlexibleSpace();

                    if(m_importType == 1)
                    {
                        //json
                        if(GUILayout.Button("导入JSON数据", GUILayout.Height(26)))
                        {
                            _Report(true, SplineUtils.SplineInitByJSON(m_spline, m_importTextAsset.text), "JSON");
                        }

                    }
                    else if(m_importType == 2)
                    {
                        //txt
                        if(GUILayout.Button("导入TXT数据", GUILayout.Height(26)))
                        {
                            _Report(true, SplineUtils.SplineInitByStringList(m_spline, m_importTextAsset.text), "TXT");
                        }
                    }
                    GUILayout.Space(5);
                }
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
        }

        private void _Report(bool isImport, bool sccess, string dataTag, Action sccessCallback = null)
        {
            EditorUtility.DisplayDialog("提示", $"{(isImport ? "导入" : "导出")}{dataTag}数据{(sccess ? "成功" : "失败")}", "确定");
            if(sccessCallback != null && sccess)
                sccessCallback();
        }

        private string m_savePath;
        private string m_fileName;
        private void _draw_dataExport_UI()
        {

            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);

                _draw_savePath_UI();

                GUILayout.Space(5);

                GUILayout.Label("导出数据文件名");

                m_fileName = EditorGUILayout.TextField(m_fileName);

                GUILayout.FlexibleSpace();

                if(!string.IsNullOrEmpty(m_savePath))
                {
                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button("导出JSON数据", GUILayout.Height(26)))
                        {
                            string path = _getSavePath(".json");
                            string contents = m_spline.ToJsonString();

                            if(!string.IsNullOrEmpty(contents))
                                _Report(false, AorIO.SaveStringToFile(path, contents), "JSON", () => {
                                    AssetDatabase.Refresh();
                                });
                            else
                                _Report(false, false, "JSON");

                        }
                        if(GUILayout.Button("导出TXT数据", GUILayout.Height(26)))
                        {
                            string path = _getSavePath(".txt");
                            string contents = m_spline.ToStringList();
                            if(!string.IsNullOrEmpty(contents))
                                _Report(false, AorIO.SaveStringToFile(path, contents), "TXT", () => {
                                    AssetDatabase.Refresh();
                                });
                            else
                                _Report(false, false, "TXT");
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
        }

        private string _getSavePath(string suffix)
        {
            return m_savePath + "/" + (string.IsNullOrEmpty(m_fileName) ? m_spline.name : m_fileName) + suffix;
        }

        private void _draw_savePath_UI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(5);

                GUILayout.Label("导出路径 (文件夹)");

                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);

                    m_savePath = EditorGUILayout.TextField(m_savePath);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if(GUILayout.Button("UseSelection", GUILayout.Width(120)))
                        {
                            if(Selection.activeObject)
                            {
                                string tp = AssetDatabase.GetAssetPath(Selection.activeObject);
                                if(!string.IsNullOrEmpty(tp))
                                {

                                    EditorAssetInfo info = new EditorAssetInfo(tp);
                                    m_savePath = info.dirPath;

                                }
                                else
                                {
                                    m_savePath = "";
                                }
                            }
                            else
                            {
                                m_savePath = "";
                            }
                        }
                        if(GUILayout.Button("Set", GUILayout.Width(50)))
                        {
                            m_savePath = EditorUtility.SaveFolderPanel("设置保存路径", "", "");
                            m_savePath = m_savePath.Replace(Application.dataPath, "Assets");
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

    }

}



