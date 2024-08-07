using System.Collections.Generic;
using AORCore.Editor;
using UnityEditor;
using UnityEngine;

namespace AORCore.Utility.Editor
{

    /// <summary>
    /// Transform 增强 Inspector
    /// 
    /// UpdateDate : 2023-01-01 Aorition
    /// 
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof (Transform), true)]
    public class TransformPlusEditor : UnityEditor.Editor
    {
        static public TransformPlusEditor instance;

        SerializedProperty mPos;
        SerializedProperty mRot;
        SerializedProperty mScale;

        private bool mUseUnifiedScale;

        void OnEnable()
        {
            instance = this;
            mPos = serializedObject.FindProperty("m_LocalPosition");
            mRot = serializedObject.FindProperty("m_LocalRotation");
            mScale = serializedObject.FindProperty("m_LocalScale");
            mUseUnifiedScale = IsUnifiedScale();
        }
        
        void OnDestroy()
        {
            instance = null;
        }

        /// <summary>
        /// 开始绘制Transform
        /// </summary>
        public override void OnInspectorGUI()
        {

            EditorGUIUtility.labelWidth = 15;

            serializedObject.Update();

            //------------------------------------

            DrawPosition();
            DrawRotation();
            DrawScale();

            //-------------

            serializedObject.ApplyModifiedProperties();

        }

        /// <summary>
        /// 绘制坐标
        /// </summary>
        void DrawPosition()
        {
            GUILayout.BeginHorizontal();
            {
                bool dirty = false;
                bool reset = GUILayout.Button("P", "toolbarbutton", GUILayout.Width(20f));
                GUILayout.Button(" ", "toolbarbutton", GUILayout.Width(20f));
                if (EditorGUILayout.PropertyField(mPos.FindPropertyRelative("x"))) dirty = true;
                if (EditorGUILayout.PropertyField(mPos.FindPropertyRelative("y"))) dirty = true;
                if (EditorGUILayout.PropertyField(mPos.FindPropertyRelative("z"))) dirty = true;

                if (reset) { mPos.vector3Value = Vector3.zero; dirty = true; }
                if (dirty)
                {
                    UnityEditor.Undo.RecordObjects(serializedObject.targetObjects, "Change Postion");
                }
            }
            GUILayout.EndHorizontal();
        }


        /// <summary>
        /// 绘制形变
        /// </summary>
        void DrawScale()
        {
            GUILayout.BeginHorizontal();
            {
                bool dirty = false;
                bool reset = GUILayout.Button("S", "toolbarbutton", GUILayout.Width(20f));

                if (mUseUnifiedScale && !IsUnifiedScale())
                    mUseUnifiedScale = false;

                bool nUseUnifiedScale = GUILayout.Toggle(mUseUnifiedScale,"U", "toolbarbutton", GUILayout.Width(20f));
                if(nUseUnifiedScale != mUseUnifiedScale)
                {

                    if (nUseUnifiedScale && !IsUnifiedScale())
                    {
                        if (EditorUtility.DisplayDialog("警告", "ScaleXYZ不等值，是否使用统一值？", "是", "否"))
                        {
                            mUseUnifiedScale = true;
                            mScale.vector3Value = new Vector3(mScale.FindPropertyRelative("x").floatValue, mScale.FindPropertyRelative("x").floatValue, mScale.FindPropertyRelative("x").floatValue);
                            dirty = true;
                        }
                        else
                        {
                            mUseUnifiedScale = false;
                        }
                    }
                    else
                    {
                        mUseUnifiedScale = nUseUnifiedScale;
                    }
                }

                if (mUseUnifiedScale)
                {
                    float uScale = mScale.FindPropertyRelative("x").floatValue;
                    float nUScale = EditorGUILayout.FloatField(" ", uScale);
                    if (!nUScale.Equals(uScale))
                    {
                        mScale.vector3Value = new Vector3(nUScale, nUScale, nUScale);
                        dirty = true;
                    }
                }
                else
                {
                    if (EditorGUILayout.PropertyField(mScale.FindPropertyRelative("x"))) dirty = true;
                    if (EditorGUILayout.PropertyField(mScale.FindPropertyRelative("y"))) dirty = true;
                    if (EditorGUILayout.PropertyField(mScale.FindPropertyRelative("z"))) dirty = true;
                }
                if (reset) { mScale.vector3Value = Vector3.one; dirty = true; }
                if (dirty)
                {
                    UnityEditor.Undo.RecordObjects(serializedObject.targetObjects, "Change Scale");
                }
            }
            GUILayout.EndHorizontal();
        }

        bool IsUnifiedScale()
        {
            return (mScale.FindPropertyRelative("x").floatValue == mScale.FindPropertyRelative("y").floatValue && mScale.FindPropertyRelative("y").floatValue == mScale.FindPropertyRelative("z").floatValue);
        }

        enum Axes : int
        {
            None = 0,
            X = 1,
            Y = 2,
            Z = 4,
            All = 7,
        }

        Axes CheckDifference(Transform t, Vector3 original)
        {
            Vector3 next = t.localEulerAngles;

            Axes axes = Axes.None;

            if (Differs(next.x, original.x)) axes |= Axes.X;
            if (Differs(next.y, original.y)) axes |= Axes.Y;
            if (Differs(next.z, original.z)) axes |= Axes.Z;

            return axes;
        }

        Axes CheckDifference(SerializedProperty property)
        {
            Axes axes = Axes.None;

            if (property.hasMultipleDifferentValues)
            {
                Vector3 original = property.quaternionValue.eulerAngles;

                foreach (Object obj in serializedObject.targetObjects)
                {
                    axes |= CheckDifference(obj as Transform, original);
                    if (axes == Axes.All) break;
                }
            }
            return axes;
        }

        /// <summary>
        /// 绘制一个可编辑的浮动区域
        /// </summary>
        /// <param name="hidden">是否值用 -- 代替</param>
        static bool FloatField(string name, ref float value, bool hidden, GUILayoutOption opt)
        {
            float newValue = value;
            GUI.changed = false;

            if (!hidden)
            {
                newValue = EditorGUILayout.FloatField(name, newValue, opt);
            }
            else
            {
                float.TryParse(EditorGUILayout.TextField(name, "--", opt), out newValue);
            }

            if (GUI.changed && Differs(newValue, value))
            {
                value = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 由于 Mathf.Approximately 太敏感.
        /// </summary>

        static bool Differs(float a, float b)
        {
            return Mathf.Abs(a - b) > 0.0001f;
        }

        /// <summary>
        /// 绘制旋转
        /// </summary>
        void DrawRotation()
        {
            GUILayout.BeginHorizontal();
            {
                bool reset = GUILayout.Button("R", "toolbarbutton", GUILayout.Width(20f));
                GUILayout.Button(" ", "toolbarbutton", GUILayout.Width(20f));
                Vector3 visible = (serializedObject.targetObject as Transform).localEulerAngles;

                visible.x = WrapAngle(visible.x);
                visible.y = WrapAngle(visible.y);
                visible.z = WrapAngle(visible.z);

                Axes changed = CheckDifference(mRot);
                Axes altered = Axes.None;

                GUILayoutOption opt = GUILayout.MinWidth(30f);

                if (FloatField("X", ref visible.x, (changed & Axes.X) != 0, opt)) altered |= Axes.X;
                if (FloatField("Y", ref visible.y, (changed & Axes.Y) != 0, opt)) altered |= Axes.Y;
                if (FloatField("Z", ref visible.z, (changed & Axes.Z) != 0, opt)) altered |= Axes.Z;

                if (reset)
                {
                    mRot.quaternionValue = Quaternion.identity;
                }
                else if (altered != Axes.None)
                {
                    //RegisterUndo("Change Rotation", serializedObject.targetObjects);
                    UnityEditor.Undo.RecordObjects(serializedObject.targetObjects, "Change Rotation");

                    foreach (Object obj in serializedObject.targetObjects)
                    {
                        Transform t = obj as Transform;
                        Vector3 v = t.localEulerAngles;

                        if ((altered & Axes.X) != 0) v.x = visible.x;
                        if ((altered & Axes.Y) != 0) v.y = visible.y;
                        if ((altered & Axes.Z) != 0) v.z = visible.z;

                        t.localEulerAngles = v;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 保证角在 180到-180度之间
        /// </summary>
        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerStepThrough]
        static public float WrapAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

    }
}