using System;
using UnityEditor;
using UnityEngine;


namespace UnityEngine.Rendering.Universal.module.Editor
{
    /// <summary>
    /// Update data: 2021-05-31 : Aorition
    /// </summary>
    //[CustomEditor(typeof(Spline)), CanEditMultipleObjects]
    [CustomEditor(typeof(Spline))]
    public class SplineEditor :UnityEditor.Editor
    {

        [MenuItem("GameObject/Create Other/Spline", false, 0)]
        public static void CreateCurvySpline()
        {
            Spline spl = Spline.Create();
            Selection.activeObject = spl;
            Undo.RegisterCreatedObjectUndo(spl.gameObject, "Create Spline");
        }

        static GUIStyle mindexLabel;
        public static GUIStyle indexLabel
        {
            get {
                if(mindexLabel == null)
                {
                    mindexLabel = new GUIStyle(EditorStyles.label);
                    mindexLabel.alignment = TextAnchor.MiddleCenter;
                }
                return mindexLabel;
            }
        }

        static GUIStyle mstFoldout;
        public static GUIStyle stFoldout
        {
            get {
                if(mstFoldout == null)
                {
                    mstFoldout = new GUIStyle(EditorStyles.foldout);
                    mstFoldout.fontStyle = FontStyle.Bold;
                }
                return mstFoldout;
            }
        }

        public static bool Foldout(ref bool state, string text) { return Foldout(ref state, new GUIContent(text)); }

        public static bool Foldout(ref bool state, GUIContent content)
        {
            Rect r = GUILayoutUtility.GetRect(content, stFoldout);
            int lvl = EditorGUI.indentLevel;
            EditorGUI.indentLevel = Mathf.Max(0, EditorGUI.indentLevel - 1);
            r = EditorGUI.IndentedRect(r);
            r.x += 3;
            state = GUI.Toggle(r, state, content, stFoldout);

            EditorGUI.indentLevel = lvl;

            return state;
        }

        private const int STEPS_PER_CURVE = 4;
        private const float VELOCITIES_SCALE = 0.4f;
        private const float ACCELERATION_SCALE = 0.1f;
        protected const float HANDLE_SIZE = 0.04f;
        protected const float PICK_SIZE = 0.06f;

        private static Color[] _modeColors = 
        {
            Color.white,    // corner
	        Color.yellow,   // aligned
	        Color.green     // smooth
        };

        private Spline _spline;
        protected Transform _handleTransform;
        protected Quaternion _handleRotation;
        private int _selectedIndex = -1;

        private TextEditor _textEditor;

        private bool[] foldouts = new bool[1] { true };

        void Awake()
        {
            _spline = target as Spline;
            _textEditor = new TextEditor();
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_id"));
 //           EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowGizmo"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowNumbers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowVelocities"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowAccelerations"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GizmosColor"));
            serializedObject.ApplyModifiedProperties();

            if(!_spline.HasDatas)
            {
                if(GUILayout.Button("Spline Reset"))
                {
                    _spline.Reset();
                }
                return;
            }

            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Loop", _spline.Loop);
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(serializedObject.targetObjects, "Toggle Loop");
                foreach(UnityEngine.Object o in serializedObject.targetObjects)
                {
                    Spline s = o as Spline;
                    s.Loop = loop;
                    EditorUtility.SetDirty(s);
                }
            }

            if(_selectedIndex >= 0 && _selectedIndex <= _spline.curveCount * 3 && _textEditor != null)
            {
                _DrawSelectedPointInspector();
            }

            if(GUILayout.Button("Add Point", GUILayout.Height(25)))
            {
                Undo.RecordObject(_spline, "Add Point");
                _spline.AddPoint(_selectedIndex);
                EditorUtility.SetDirty(_spline);
            }

            if(_spline.curveCount >= 2 && (_selectedIndex == 0 || _selectedIndex % 3 == 0))
            {
                if(GUILayout.Button("Delete Point", GUILayout.Height(25)))
                {
                    Undo.RecordObject(_spline, "Delete Point");
                    _spline.DeletePoint(_selectedIndex);
                    _selectedIndex = -1;
                    EditorUtility.SetDirty(_spline);
                }
            }

            if(Foldout(ref foldouts[0], "Spline Info")) 
            {
                EditorGUILayout.LabelField("Total Length: " + _spline.Length);
            }

            if(_spline.HasDatas)
            {
                GUILayout.Space(5);
                _draw_dataIE_UI();
            }

            GUILayout.Space(5);

            _draw_fastCreat_UI();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal("box");
            {
                GUILayout.FlexibleSpace();
                GUI.color = Color.red;
                if(GUILayout.Button("Reset?",GUILayout.Width(120)))
                {
                    if(EditorUtility.DisplayDialog("!!","Are u sure ?","sure","not sure"))
                    {
                        _spline.Reset();
                    }
                }
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();
        }

        private void _draw_fastCreat_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUI.color = Color.magenta;
                if(GUILayout.Button("快速生成工具", GUILayout.Height(25)))
                {
                    SplineFastcreativeUtilWindow.init(_spline);
                }
                GUI.color = Color.white;
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void _draw_dataIE_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                if(GUILayout.Button("数据导入导出", GUILayout.Height(25)))
                {
                    SplineDataIEWindow.init(target as Spline);
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void _DrawSelectedPointInspector()
        {

            GUILayout.Space(5);
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.Label("Selected Point");
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(12);
                    int idxMax = _spline.ControlPointLength - 1;
                    if((_selectedIndex - 1) >= 0)
                    {
                        if(GUILayout.Button("Previous"))
                        {
                            _selectedIndex--;
                            Repaint();
                        }
                    }
                    else
                    {
                        GUI.color = Color.gray;
                        if(GUILayout.Button("Previous"))
                        {
                            //do nothing...
                        }
                        GUI.color = Color.white;
                    }
                    GUILayout.Space(6);
                    GUILayout.Label($"Index : {_selectedIndex} / {idxMax}", indexLabel);
                    GUILayout.Space(6);
                    if((_selectedIndex + 1) <= idxMax)
                    {
                        if(GUILayout.Button("Next"))
                        {
                            _selectedIndex++;
                            Repaint();
                        }
                    }
                    else
                    {
                        GUI.color = Color.gray;
                        if(GUILayout.Button("Next"))
                        {
                            //do nothing...
                        }
                        GUI.color = Color.white;
                    }

                    GUILayout.Space(12);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 point = EditorGUILayout.Vector3Field("", _spline.GetControlPoint(_selectedIndex));
                    if(EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_spline, "Move Point");
                        EditorUtility.SetDirty(_spline);
                        _spline.SetControlPoint(_selectedIndex, point);
                    }
                    EditorGUI.BeginChangeCheck();
                    if(GUILayout.Button(new GUIContent("C", "复制"), GUILayout.Width(20)))
                    {
                        string copyStr = "{" + point.x + "," + point.y + "," + point.z + "}";
                        //                _textEditor.content = new GUIContent(copyStr);
                        _textEditor.text = copyStr;
                        _textEditor.SelectAll();
                        _textEditor.Copy();
                        Debug.Log("点数据" + copyStr + "已复制到剪贴板中了");
                    }

                    if(_textEditor.CanPaste())
                    {
                        if(GUILayout.Button(new GUIContent("P", "粘贴"), GUILayout.Width(20)))
                        {
                            _textEditor.OnFocus();
                            _textEditor.Paste();
                            //                    string pasteStr = _textEditor.content.text;
                            string pasteStr = _textEditor.text;
                            if(pasteStr != null || pasteStr.Trim() != "")
                            {
                                try
                                {
                                    pasteStr = pasteStr.Replace("{", "").Replace("}", "");
                                    string[] ds = pasteStr.Split(',');
                                    if(ds.Length == 3)
                                    {
                                        Vector3 np = new Vector3(float.Parse(ds[0]), float.Parse(ds[1]), float.Parse(ds[2]));
                                        _spline.SetControlPoint(_selectedIndex, np);
                                        Debug.Log("已粘贴剪贴板数据");
                                    }
                                }
                                catch(Exception ex)
                                {
                                    //do noting
                                }
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
                BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", _spline.GetControlPointMode(_selectedIndex));
                if(EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Change Point Mode");
                    _spline.SetControlPointMode(_selectedIndex, mode);
                    EditorUtility.SetDirty(_spline);
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        protected virtual void OnSceneGUI()
        {

            if(!_spline.HasDatas)
                return;

            _handleTransform = _spline.transform;
            _handleRotation = UnityEditor.Tools.pivotRotation == PivotRotation.Local ?
            _handleTransform.rotation : Quaternion.identity;

            Vector3 p0 = _ShowPoint(0);
            for(int i = 1; i < _spline.curveCount * 3; i += 3)
            {
                Vector3 p1 = _ShowPoint(i);
                Vector3 p2 = _ShowPoint(i + 1);
                Vector3 p3 = _ShowPoint(i + 2);

                Handles.color = Color.gray;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);

                //Handles.DrawBezier(p0, p3, p1, p2, _spline.color, null, 2f);
                p0 = p3;
            }

            if(_spline.ShowVelocities)
                _ShowVelocities();
            if(_spline.ShowAccelerations)
                _ShowAccelerations();
        }

        private void _ShowVelocities()
        {
            Handles.color = Color.green;
            Vector3 point;

            for(int c = 0; c < _spline.curveCount; c++)
                for(int i = 0; i < STEPS_PER_CURVE; i++)
                {
                    point = _spline.GetPoint(c, c + 1, i / (float)STEPS_PER_CURVE);
                    Handles.DrawLine(point, point + _spline.GetVelocity(c, c + 1, i / (float)STEPS_PER_CURVE) * VELOCITIES_SCALE);
                }

            point = _spline.GetPoint(1f);
            Handles.DrawLine(point, point + _spline.GetVelocity(1f) * VELOCITIES_SCALE);
        }

        private void _ShowAccelerations()
        {
            Handles.color = Color.yellow;
            Vector3 point;

            for(int c = 0; c < _spline.curveCount; c++)
                for(int i = 0; i <= STEPS_PER_CURVE; i++)
                {
                    point = _spline.GetPoint(c, c + 1, i / (float)STEPS_PER_CURVE);
                    Handles.DrawLine(point, point + _spline.GetAcceleration(c, c + 1, i / (float)STEPS_PER_CURVE) * ACCELERATION_SCALE);
                }
        }

        private Vector3 _ShowPoint(int index)
        {
            Vector3 point = _handleTransform.TransformPoint(_spline.GetControlPoint(index));
            float size = HandleUtility.GetHandleSize(point);

            if(index % 3 == 0 && _spline.ShowNumbers)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = _spline.GizmosColor;
                style.fontSize = 20;
                Handles.Label(point + Vector3.right * size * 0.1f, "" + (index / 3), style);
            }

            if(index == 0)
            {
                size *= 2f;
            }

            Handles.color = _modeColors[(int)_spline.GetControlPointMode(index)];

            if(index % 3 != 0)
                Handles.color = Color.grey;

#if UNITY_2018_1_OR_NEWER
            if(Handles.Button(point, _handleRotation, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotHandleCap))
            {
                _selectedIndex = index;
                Repaint();
            }
#elif UNITY_5
        if(Handles.Button(point, _handleRotation, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotCap))
            {
                _selectedIndex = index;
                Repaint();
            }

#endif

            if(_selectedIndex == index)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.PositionHandle(point, _handleRotation);
                if(EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Move Point");
                    EditorUtility.SetDirty(_spline);
                    _spline.SetControlPoint(index, _handleTransform.InverseTransformPoint(point));
                }
            }
            return point;
        }
    }

}


    