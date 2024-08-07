//
// Copyright (c) 2018 Wayne Zheng
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// 把Unity的日志保存到txt文件中
// Hack by : Aorition
// Update : 2023-07-29
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using AORCore.Editor;

namespace AORCore.UI.Editor.Utility
{

    public class ExportConsoleEditorWindow : EditorWindow
    {

        private static ExportConsoleEditorWindow m_instance;

        [MenuItem("Window/FrameworkTools/ExportConsole/ExportConsoleWindow")]
        private static ExportConsoleEditorWindow Init()
        {
            m_instance = EditorWindow.GetWindow<ExportConsoleEditorWindow>();
            m_instance.titleContent = new GUIContent("Console日志导出器");
            m_instance.Show();
            return m_instance;
        }

        [MenuItem("Debug/Export Console", validate = true)]
        private static bool Validate()
        {
            if(!ExportConsoleUtility.IsSupport)
                ExportConsoleUtility.Initialization();
            return ExportConsoleUtility.IsSupport && ExportConsoleUtility.GetEntryCount() > 0;
        }

        private static string m_reserveBaseKey;
        private static string ReserveBaseKey
        {
            get
            {
                if (string.IsNullOrEmpty(m_reserveBaseKey))
                {
                    m_reserveBaseKey = $"{Application.dataPath}|ExportConsole";
                }
                return m_reserveBaseKey;
            }
        }

        private void LoadReserve()
        {
            _lastSavedDir = EditorPrefs.GetString($"{ReserveBaseKey}|DIR", Application.dataPath);
            _lastFileName = EditorPrefs.GetString($"{ReserveBaseKey}|FN", "ConsoleLog");
        }

        private void SaveReserve(string path)
        {
            EditorAssetInfo info = new EditorAssetInfo(path);
            _lastSavedDir = info.dirPath;
            _lastFileName = info.name;
            EditorPrefs.SetString($"{ReserveBaseKey}|DIR", _lastSavedDir);
            EditorPrefs.SetString($"{ReserveBaseKey}|FN", _lastFileName);
        }

        protected bool _detail = false;
        protected string _lastSavedDir;
        protected string _lastFileName;

        private void Awake()
        {
            if (!ExportConsoleUtility.IsSupport)
                ExportConsoleUtility.Initialization();
            LoadReserve();
        }

        private void OnGUI()
        {
            GUILayout.Space(5);
            _detail = EditorGUILayout.Toggle("Detail", _detail);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Export", GUILayout.Height(26)))
            {
                if (DoExportConsole(_detail))
                {
                    Close();
                }
            }
            GUILayout.Space(5);
        }

        protected bool DoExportConsole(bool detail)
        {
            string path = EditorUtility.SaveFilePanel("Export Console", _lastSavedDir, _lastFileName, "txt");
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            SaveReserve(path);
            return ExportConsoleUtility.DoExportConsole(path, detail);
        }

    }

    public static class ExportConsoleUtility 
    {
        private static Type _LogEntriesType;
        private static Type _logEntryType;
        private static MethodInfo _GetCountMethod;
        private static MethodInfo _StartGettingEntriesMethod;
        private static MethodInfo _GetEntryInternalMethod;
        private static MethodInfo _EndGettingEntriesMethod;
#if UNITY_2019_1_OR_NEWER
        private static FieldInfo _messageField;
#else
        private static FieldInfo _conditionField;
#endif
        private static bool _isSupport = false;
        public static bool IsSupport => _isSupport;

        public static void Initialization()
        {
            _LogEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
            if (_LogEntriesType != null)
            {
                _GetCountMethod = _LogEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
                _StartGettingEntriesMethod = _LogEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public);
                _GetEntryInternalMethod = _LogEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
                _EndGettingEntriesMethod = _LogEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public);
            }
            _logEntryType = Type.GetType("UnityEditor.LogEntry,UnityEditor");
            if (_logEntryType != null)
            {
#if UNITY_2019_1_OR_NEWER
                _messageField = _logEntryType.GetField("message", BindingFlags.Public | BindingFlags.Instance);
#else
                _conditionField = _logEntryType.GetField("condition", BindingFlags.Public | BindingFlags.Instance);
#endif
            }

            if (_LogEntriesType == null ||
                _logEntryType == null ||
                _GetCountMethod == null ||
                _StartGettingEntriesMethod == null ||
                _GetEntryInternalMethod == null ||
                _EndGettingEntriesMethod == null ||
#if UNITY_2019_1_OR_NEWER
                _messageField == null)
            {
                _isSupport = false;
            }
            else
            {
                _isSupport = true;
            }
#else
                _conditionField == null)
            {
                _isSupport = false;
            }
            else
            {
                _isSupport = true;
            }
#endif

        }

        public static bool DoExportConsole(string path, bool detail, bool OpenDirAfterExport = true)
        {
            if (!_isSupport)
            {
                Initialization();
            }

            if (!_isSupport || string.IsNullOrEmpty(path)) return false;

            string[] logs = GetConsoleEntries();
            if (!detail)
            {
                for (int i = 0; i < logs.Length; ++i)
                {
                    using (var sr = new StringReader(logs[i]))
                    {
                        logs[i] = sr.ReadLine();
                    }
                }
            }
            File.WriteAllLines(path, logs);
            if(OpenDirAfterExport)
                EditorUtility.OpenWithDefaultApp(path);
            return true;
        }

        private static string[] GetConsoleEntries()
        {
            if (!_isSupport)
            {
                return null;
            }
            List<string> entries = new List<string>();
            object countObj = _GetCountMethod.Invoke(null, null);
            _StartGettingEntriesMethod.Invoke(null, null);
            int count = int.Parse(countObj.ToString());
            for (int i = 0; i < count; ++i)
            {
                object logEntry = Activator.CreateInstance(_logEntryType);
                object result = _GetEntryInternalMethod.Invoke(null, new object[] { i, logEntry });
                if (bool.Parse(result.ToString()))
                {
#if UNITY_2019_1_OR_NEWER
                    entries.Add(_messageField.GetValue(logEntry).ToString());
#else
                    entries.Add(_conditionField.GetValue(logEntry).ToString());
#endif
                }
            }
            _EndGettingEntriesMethod.Invoke(null, null);
            return entries.ToArray();
        }

        public static int GetEntryCount()
        {
            if (!_isSupport)
            {
                return 0;
            }
            object countObj = _GetCountMethod.Invoke(null, null);
            return int.Parse(countObj.ToString());
        }

    }

}

