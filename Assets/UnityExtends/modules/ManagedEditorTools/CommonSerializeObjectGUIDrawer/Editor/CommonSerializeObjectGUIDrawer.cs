using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace CommonSerializeObjectGUI.Editor
{

    /// <summary>
    /// 可序列化数据对象GUI绘制器(公共版)
    /// 
    /// Author : Aorition
    /// 
    /// Update : 2024-08-06
    /// 
    /// 说明:
    ///     针对[Serializable] public class ClassName {} 或者 [Serializable] public struct StructName {} 数据类型对象的GUI编辑界面绘制包装器。
    ///     自动对数据对象的可序列化字段进行GUI绘制，搭配Unity字段特性(Attribute)或者CommonSerializeObjectGUI专用特性(Attribute)以获取不同形式(或风格)的GUI控件。
    /// 
    /// 重要提示: 对于字段特性(Attribute)的支持，该类遵循原生序列化特性的所有规则，除[NonSerialized]特性:
    ///     [NonSerialized] 本类将忽略该特性，标记该特性的public字段将正常显示。 如果不需要显示该字段，请标记[HideInInspector]特性
    ///     
    /// 提示2: 如使用内置Undo功能则要求数据对象显示带有[Serializable]特性标签，否则可能造成数据丢失。
    /// 
    /// </summary>
    public class CommonSerializeObjectGUIDrawer
    {

        #region GUIStyle Defines

        protected static GUIStyle m_MultilineTextAreaStyle;
        protected static GUIStyle MultilineTextAreaStyle
        {
            get
            {
                if (m_MultilineTextAreaStyle == null)
                {
                    m_MultilineTextAreaStyle = new GUIStyle(EditorStyles.textArea);
                    m_MultilineTextAreaStyle.wordWrap = true;
                }
                return m_MultilineTextAreaStyle;
            }
        }

        protected static GUIStyle m_HeaderStyle;
        protected static GUIStyle HeaderStyle
        {
            get
            {
                if (m_HeaderStyle == null)
                {
                    m_HeaderStyle = new GUIStyle(EditorStyles.boldLabel);
                }
                return m_HeaderStyle;
            }
        }

        protected static GUIStyle m_TurnPageUILabelStyle;
        protected static GUIStyle TurnPageUILabelStyle
        {
            get
            {
                if (m_TurnPageUILabelStyle == null)
                {
                    m_TurnPageUILabelStyle = new GUIStyle(EditorStyles.boldLabel);
                    m_TurnPageUILabelStyle.alignment = TextAnchor.MiddleCenter;
                }
                return m_TurnPageUILabelStyle;
            }
        }
        #endregion

        #region Check Types

        private static bool TypeIsUObject(Type type)
        {
            Type baseType = typeof(UnityEngine.Object);
            Type c = type;
            while(c != null)
            {
                if (c == baseType)
                    return true;
                c = c.BaseType;
            }
            return false;
        }

        private static bool TypeIsClass(Type type)
        {
            //if(AttributeUtility.HasAttribute<SerializableAttribute>(type,out _))
            //{
                return type.IsClass && !type.IsArray && !type.IsGenericType;
            //}
            //return false;
        }
        
        private static bool TypeIsStruct(Type type)
        {
            //if (AttributeUtility.HasAttribute<SerializableAttribute>(type, out _))
            //{
                return type.IsValueType && !type.IsEnum && !type.IsPrimitive;
            //}
            //return false;
        }

        private static bool TypeIsEnum(Type type)
        {
            return type.IsEnum;
        }

        private static bool TypeIsArray(Type type)
        {
            return type.IsArray;
        }

        private static bool TypeIsList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) && type.GetGenericArguments().Length == 1;
        }

        #endregion

        #region Inner Class

        protected class SubObjInfo 
        {

            public SubObjInfo(int hashCode)
            {
                HashCode = hashCode;
            }

            public SubObjInfo(int hashCode, object value)
            {
                HashCode = hashCode;
                GUIDrawer = new CommonSerializeObjectGUIDrawer(value);
                GUIDrawer.DisplayTitle = false;
            }

            public void Dispose()
            {
                if(GUIDrawer != null)
                {
                    GUIDrawer.Dispose();
                    GUIDrawer = null;
                }
            }

            public int HashCode;
            public bool IsFold;
            public CommonSerializeObjectGUIDrawer GUIDrawer;
            public Vector2 ScrollPos;
            public int pageIndex;
        }


        #endregion

        protected Type m_BindingType;

        protected object m_BindingIns;
        public object BindingIns => m_BindingIns;

        protected bool m_isInit;
        public bool IsInit => m_isInit;

        public bool HasSerializeData
        {
            get { return m_fieldInfoDic.Count > 0; }
        }

        /// <summary>
        /// 是否显示对象题头
        /// </summary>
        public bool DisplayTitle = true;
        /// <summary>
        /// 数据锁定 （为True时数据锁定,无法通过Inspector进行修改）
        /// </summary>
        public bool Locked;
        /// <summary>
        /// 关闭内置Undo功能( 为True时， Undo / Redo / UndoRecord 方法 调用无效)
        /// </summary>
        public bool DisableUndo;
        /// <summary>
        /// 最大Undo次数
        /// </summary>
        public uint UndoLimit = 10;

        protected string m_BindingInsTitle;

        protected readonly Dictionary<string, string> m_fieldLabelDic = new Dictionary<string, string>();
        protected readonly Dictionary<string, FieldInfo> m_fieldInfoDic = new Dictionary<string, FieldInfo>();

        protected readonly Dictionary<int, SubObjInfo> m_subObjInfoDic = new Dictionary<int, SubObjInfo>();

        protected readonly Dictionary<Rect, FieldInfo> m_dragArea = new Dictionary<Rect, FieldInfo>();

        protected bool m_JSONSnapshotDirty = true;
        protected string m_JSONSnapshot;
        protected readonly Stack<string> m_UndoStack = new Stack<string>();
        protected readonly Stack<string> m_RedoStack = new Stack<string>();

        [Obsolete("已废弃API， UndoRecord行为已改为自动行为因此无需手动调用")]
        public void UndoRecord()
        {
            if (DisableUndo) return;
            UndoRecordInternal(m_JSONSnapshot);
            m_JSONSnapshotDirty = true;
        }

        protected void UndoRecordInternal(string json)
        {
            if (m_UndoStack.Count >= UndoLimit)
            {
                while (m_UndoStack.Count >= UndoLimit)
                {
                    m_UndoStack.Pop();
                }
            }
            m_UndoStack.Push(json);
            m_RedoStack.Clear();
        }

        protected void RedoRecordInternal(string json)
        {

            if (m_RedoStack.Count >= UndoLimit)
            {
                while (m_RedoStack.Count >= UndoLimit)
                {
                    m_RedoStack.Pop();
                }
            }
            m_RedoStack.Push(json);
        }

        public void Undo()
        {
            if (DisableUndo || Locked) return;
            if (m_UndoStack.Count > 0)
            {
                GUI.FocusControl(null);
                string undo = m_UndoStack.Pop();
                RedoRecordInternal(m_JSONSnapshot);
                JsonUtility.FromJsonOverwrite(undo, m_BindingIns);
            }
        }

        public void Redo()
        {
            if (DisableUndo || Locked) return;
            if (m_RedoStack.Count > 0)
            {
                GUI.FocusControl(null);
                string redo = m_RedoStack.Pop();
                UndoRecordInternal(m_JSONSnapshot);
                JsonUtility.FromJsonOverwrite(redo, m_BindingIns);
            }
        }

        public CommonSerializeObjectGUIDrawer (object instance)
        {
            m_BindingIns = instance;
            m_BindingType = instance.GetType();
            if (AttributeUtility.HasAttribute<CSOFieldLabelAttribute>(m_BindingType, out var BindingInsTitleIns))
                m_BindingInsTitle = BindingInsTitleIns.Label;
            else
                m_BindingInsTitle = m_BindingType.Name;

            FieldInfo[] fInfos = m_BindingType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            if (fInfos != null && fInfos.Length > 0)
            {
                foreach (var fi in fInfos)
                {
                    if (AttributeUtility.HasAttribute<CSOForceFieldAttribute>(fi, out _) || !AttributeUtility.HasAttribute<HideInInspector>(fi, out _))
                    {
                        m_fieldInfoDic.Add(fi.Name, fi);
                        if (AttributeUtility.HasAttribute<CSOFieldLabelAttribute>(fi, out var attributeIns))
                        {
                            m_fieldLabelDic.Add(fi.Name, attributeIns.Label);
                        }
                    }
                }
            }
            FieldInfo[] fInfos2 = m_BindingType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (fInfos2 != null && fInfos2.Length > 0)
            {
                foreach (var fi in fInfos2)
                {
                    if (AttributeUtility.HasAttribute<CSOForceFieldAttribute>(fi, out _) || (AttributeUtility.HasAttribute<SerializeField>(fi, out _) && !AttributeUtility.HasAttribute<HideInInspector>(fi, out _)))
                    {
                        m_fieldInfoDic.Add(fi.Name, fi);
                        if (AttributeUtility.HasAttribute<CSOFieldLabelAttribute>(fi, out var attributeIns))
                        {
                            m_fieldLabelDic.Add(fi.Name, attributeIns.Label);
                        }
                    }
                }
            }
            m_isInit = true;
        }

        public virtual void Dispose()
        {
            foreach (var kv in m_subObjInfoDic)
            {
                kv.Value.GUIDrawer.Dispose();
            }
            m_subObjInfoDic.Clear();

            m_fieldInfoDic.Clear();
            m_fieldLabelDic.Clear();
            m_dragArea.Clear();
            m_UndoStack.Clear();
            m_RedoStack.Clear();
            m_BindingType = null;
            m_BindingIns = null;
        }
        
        public bool DrawInspectorGUI()
        {
            if (m_BindingIns == null) return false;

            if (!DisableUndo && m_JSONSnapshotDirty) 
            {
                m_JSONSnapshot = JsonUtility.ToJson(m_BindingIns);
                m_JSONSnapshotDirty = false;
            }

            bool dirty = false;
            try
            {
                GUILayout.BeginVertical("box");
                {
                    //GUILayout.Space(5);
                    if (DisplayTitle)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label($"{m_BindingInsTitle}");
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        //GUILayout.Space(12);
                    }
                    if (m_fieldInfoDic.Count > 0)
                    {
                        foreach (var kv in m_fieldInfoDic)
                        {
                            if (m_fieldLabelDic.TryGetValue(kv.Key, out var label))
                            {
                                if (DrawFieldInfoUI(label, kv.Value, m_BindingIns))
                                {
                                    dirty = true;
                                }
                            }
                            else
                            {
                                if (DrawFieldInfoUI(kv.Key, kv.Value, m_BindingIns))
                                {
                                    dirty = true;
                                }
                            }
                        }
                    }
                    //GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                if (dirty && !DisableUndo && !Locked)
                {
                    UndoRecordInternal(m_JSONSnapshot);
                    m_JSONSnapshotDirty = true;
                }
            }
            catch(Exception ex) { }
            return dirty;
        }

        public bool DrawSingleFieldGUI(string fieldName)
        {
            return DrawSingleFieldGUI(fieldName, fieldName);
        }

        public bool DrawSingleFieldGUI(string fieldName, string fieldLabel)
        {
            if (m_BindingIns == null) return false;

            if (!DisableUndo)
                m_JSONSnapshot = JsonUtility.ToJson(m_BindingIns);

            bool dirty = false;
            try
            {
                GUILayout.BeginVertical("box");
                {
                    if (m_fieldInfoDic.Count > 0)
                    {
                        if(m_fieldInfoDic.TryGetValue(fieldName, out var value))
                        {
                            if (m_fieldLabelDic.TryGetValue(fieldName, out var label))
                            {
                                if (DrawFieldInfoSingleUI(label, value, m_BindingIns))
                                {
                                    dirty = true;
                                }
                            }
                            else
                            {
                                if (DrawFieldInfoSingleUI(fieldLabel, value, m_BindingIns))
                                {
                                    dirty = true;
                                }
                            }
                        }
                    }
                    //GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                if (dirty && !DisableUndo)
                {
                    UndoRecordInternal(m_JSONSnapshot);
                    m_JSONSnapshotDirty = true;
                }

            }
            catch (Exception ex) { }
            return dirty;

        }

        /// <summary>
        /// 仅尝试记录当前对象数据快照
        /// </summary>
        public void BeginDrawGUICheck()
        {
            if (m_BindingIns == null) return;

            if (!DisableUndo)
                m_JSONSnapshot = JsonUtility.ToJson(m_BindingIns);
        }
        /// <summary>
        /// 根据isDirty注册Undo数据
        /// </summary>
        /// <param name="isDirty"></param>
        public void EndDrawGUICheck(bool isDirty)
        {
            if (m_BindingIns == null) return;

            if (isDirty && !DisableUndo)
            {
                UndoRecordInternal(m_JSONSnapshot);
                m_JSONSnapshotDirty = true;
            }
        }

        protected bool DrawFieldInfoUI(string label, FieldInfo fieldInfo, object ins)
        {
            bool dirty = false;
            object[] attributes = fieldInfo.GetCustomAttributes(true);

            if (AttributeUtility.HasAttribute<CSOIgnoreFieldOnDrawInspectorGUIAttribute>(attributes, out var _))
            {
                return false;
            }

            if (AttributeUtility.HasAttribute<SpaceAttribute>(attributes, out var SpaceAttributeIns))
            {
                GUILayout.Space(SpaceAttributeIns.height);
            } 

            bool useHeader = AttributeUtility.HasAttribute<HeaderAttribute>(attributes, out var HeaderAttributeIns);
            if (useHeader)
            {
                GUILayout.Space(12);
                EditorGUILayout.LabelField(HeaderAttributeIns.header, HeaderStyle);
                GUILayout.Space(3);
            }
            object value = fieldInfo.GetValue(ins);
            if (DrawValueEditUI(label, fieldInfo.FieldType, value, attributes, out object modifiedValue))
            {
                fieldInfo.SetValue(ins, modifiedValue);
                dirty = true;
            }

            if (useHeader)
                GUILayout.Space(12);
            return dirty;
        }

        protected bool DrawFieldInfoSingleUI(string label, FieldInfo fieldInfo, object ins)
        {
            bool dirty = false;
            object[] attributes = fieldInfo.GetCustomAttributes(true);

            if (AttributeUtility.HasAttribute<SpaceAttribute>(attributes, out var SpaceAttributeIns))
            {
                GUILayout.Space(SpaceAttributeIns.height);
            }

            bool useHeader = AttributeUtility.HasAttribute<HeaderAttribute>(attributes, out var HeaderAttributeIns);
            if (useHeader)
            {
                GUILayout.Space(12);
                EditorGUILayout.LabelField(HeaderAttributeIns.header, HeaderStyle);
                GUILayout.Space(3);
            }
            object value = fieldInfo.GetValue(ins);
            if (DrawValueEditUI(label, fieldInfo.FieldType, value, attributes, out object modifiedValue))
            {
                fieldInfo.SetValue(ins, modifiedValue);
                dirty = true;
            }

            if (useHeader)
                GUILayout.Space(12);
            return dirty;
        }

        protected bool DrawValueEditUI(string label, Type valueType, object value, object[] attributes, out object ModifiedValue)
        {

            if (TypeIsUObject(valueType))
            {
                object n = EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, valueType, false);
                if (n != value && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is AnimationCurve || valueType == typeof(AnimationCurve))
            {
                AnimationCurve o = value == null ? AnimationCurve.Constant(0, 1, 1) : (AnimationCurve)value;
                AnimationCurve n = (AnimationCurve)EditorGUILayout.CurveField(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Gradient || valueType == typeof(Gradient))
            {
                Gradient o =  value == null ? new Gradient() : (Gradient)value;
                Gradient n = (Gradient)EditorGUILayout.GradientField(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Color)
            {
                Color o = (Color)value;
                Color n = (Color)EditorGUILayout.ColorField(label, o);
                if (n != o && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Vector2)
            {
                Vector2 o = (Vector2)value;
                Vector2 n = (Vector2)EditorGUILayout.Vector2Field(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Vector2Int)
            {
                Vector2Int o = (Vector2Int)value;
                Vector2Int n = (Vector2Int)EditorGUILayout.Vector2IntField(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Vector3)
            {
                Vector3 o = (Vector3)value;
                Vector3 n = (Vector3)EditorGUILayout.Vector3Field(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Vector3Int)
            {
                Vector3Int o = (Vector3Int)value;
                Vector3Int n = (Vector3Int)EditorGUILayout.Vector3IntField(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Vector4)
            {
                Vector4 o = (Vector4)value;
                Vector4 n = (Vector4)EditorGUILayout.Vector4Field(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Rect)
            {
                Rect o = (Rect)value;
                Rect n = (Rect)EditorGUILayout.RectField(label, o);
                if (n != o && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is RectInt)
            {
                RectInt o = (RectInt)value;
                RectInt n = (RectInt)EditorGUILayout.RectIntField(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Bounds)
            {
                Bounds o = (Bounds)value;
                Bounds n = (Bounds)EditorGUILayout.BoundsField(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is BoundsInt)
            {
                BoundsInt o = (BoundsInt)value;
                BoundsInt n = (BoundsInt)EditorGUILayout.BoundsIntField(label, o);
                if (!n.Equals(o) && !Locked)
                {
                    ModifiedValue = n;
                    return true;
                }
            }
            else if (value is Matrix4x4)
            {
                Matrix4x4 o = (Matrix4x4)value;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(150));
                float m00 = EditorGUILayout.FloatField(o.m00);
                float m01 = EditorGUILayout.FloatField(o.m01);
                float m02 = EditorGUILayout.FloatField(o.m02);
                float m03 = EditorGUILayout.FloatField(o.m03);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" ", GUILayout.Width(150));
                float m10 = EditorGUILayout.FloatField(o.m10);
                float m11 = EditorGUILayout.FloatField(o.m11);
                float m12 = EditorGUILayout.FloatField(o.m12);
                float m13 = EditorGUILayout.FloatField(o.m13);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" ", GUILayout.Width(150));
                float m20 = EditorGUILayout.FloatField(o.m20);
                float m21 = EditorGUILayout.FloatField(o.m21);
                float m22 = EditorGUILayout.FloatField(o.m22);
                float m23 = EditorGUILayout.FloatField(o.m23);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" ", GUILayout.Width(150));
                float m30 = EditorGUILayout.FloatField(o.m30);
                float m31 = EditorGUILayout.FloatField(o.m31);
                float m32 = EditorGUILayout.FloatField(o.m32);
                float m33 = EditorGUILayout.FloatField(o.m33);
                EditorGUILayout.EndHorizontal();
                if (!Locked && ( !m00.Equals(o.m00) || !m01.Equals(o.m01) || !m02.Equals(o.m02) || !m03.Equals(o.m03)
                            ||   !m10.Equals(o.m10) || !m11.Equals(o.m11) || !m12.Equals(o.m12) || !m13.Equals(o.m13)
                            ||   !m20.Equals(o.m20) || !m21.Equals(o.m21) || !m22.Equals(o.m22) || !m23.Equals(o.m23)
                            ||   !m30.Equals(o.m30) || !m31.Equals(o.m31) || !m32.Equals(o.m32) || !m33.Equals(o.m33)
                    ))
                {
                    Matrix4x4 n = new Matrix4x4();
                    n.m00 = m00;
                    n.m01 = m01;
                    n.m02 = m02;
                    n.m03 = m03;
                    n.m10 = m10;
                    n.m11 = m11;
                    n.m12 = m12;
                    n.m13 = m13;
                    n.m20 = m20;
                    n.m21 = m21;
                    n.m22 = m22;
                    n.m23 = m23;
                    n.m30 = m30;
                    n.m31 = m31;
                    n.m32 = m32;
                    n.m33 = m33;
                    ModifiedValue = n;
                    return true;
                }
            }
            else
            {
                //others
                switch (valueType.Name) 
                {
                    case "Boolean":
                        {
                            bool o = (bool)value;
                            if (Locked)
                            {
                                EditorGUILayout.LabelField(label, o.ToString());
                            }
                            else
                            {
                                bool n = EditorGUILayout.Toggle(label, o);
                                if (n != o && !Locked)
                                {
                                    ModifiedValue = n;
                                    return true;
                                }
                            }
                        }
                        break;
                    case "Single":
                        {
                            float o = (float)value;
                            if (Locked)
                            {
                                EditorGUILayout.LabelField(label, o.ToString());
                            }
                            else
                            {
                                if (AttributeUtility.HasAttribute<RangeAttribute>(attributes, out var RangeAttributeIns))
                                {
                                    float n = EditorGUILayout.Slider(label, o, RangeAttributeIns.min, RangeAttributeIns.max);
                                    if (n != o && !Locked)
                                    {
                                        ModifiedValue = n;
                                        return true;
                                    }
                                }
                                else
                                {
                                    float n = EditorGUILayout.FloatField(label, o);
                                    if (n != o && !Locked)
                                    {
                                        ModifiedValue = n;
                                        return true;
                                    }
                                }
                            }

                        }
                        break;
                    case "Double":
                        {
                            double o = (double)value;
                            if (Locked)
                            {
                                EditorGUILayout.LabelField(label, o.ToString());
                            }
                            else
                            {
                                double n = EditorGUILayout.DoubleField(label, o);
                                if (n != o && !Locked)
                                {
                                    ModifiedValue = n;
                                    return true;
                                }
                            }
                        }
                        break;
                    case "Byte":
                    case "Int16":
                    case "Int32":
                        {
                            int o = (int)value;
                            if (Locked)
                            {
                                EditorGUILayout.LabelField(label, o.ToString());
                            }
                            else
                            {

                                if (AttributeUtility.HasAttribute<RangeAttribute>(attributes, out var RangeAttributeIns))
                                {
                                    int n = EditorGUILayout.IntSlider(label, o, (int)RangeAttributeIns.min, (int)RangeAttributeIns.max);
                                    if (n != o && !Locked)
                                    {
                                        ModifiedValue = n;
                                        return true;
                                    }
                                }
                                else
                                {
                                    int n = EditorGUILayout.IntField(label, o);
                                    if (n != o && !Locked)
                                    {
                                        ModifiedValue = n;
                                        return true;
                                    }
                                }
                            }
                        }
                        break;
                    case "Int64":
                        {
                            long o = (long)value;
                            if (Locked)
                            {
                                EditorGUILayout.LabelField(label, o.ToString());
                            }
                            else
                            {
                                long n = EditorGUILayout.LongField(label, o);
                                if (n != o && !Locked)
                                {
                                    ModifiedValue = n;
                                    return true;
                                }
                            }
                        }
                        break;
                    case "UInt16":
                    case "UInt32":
                        {
                            bool isShort = value is ushort;
                            int o = Convert.ToInt32(value);
                            if (Locked)
                            {
                                EditorGUILayout.LabelField(label, o.ToString());
                            }
                            else
                            {

                                if (AttributeUtility.HasAttribute<RangeAttribute>(attributes, out var RangeAttributeIns))
                                {
                                    int n = EditorGUILayout.IntSlider(label, o, (int)RangeAttributeIns.min, (int)RangeAttributeIns.max);
                                    if (n != o && !Locked)
                                    {
                                        if (isShort)
                                        {
                                            ushort n_ushort = Convert.ToUInt16(n);
                                            ModifiedValue = n_ushort;
                                        }
                                        else
                                        {
                                            uint n_unit = Convert.ToUInt32(n);
                                            ModifiedValue = n_unit;
                                        }
                                        return true;
                                    }
                                }
                                else
                                {
                                    int n = EditorGUILayout.IntField(label, o);
                                    if (n != o && !Locked)
                                    {
                                        if(isShort)
                                        {
                                            ushort n_ushort = Convert.ToUInt16(n);
                                            ModifiedValue = n_ushort;
                                        }
                                        else
                                        {
                                            uint n_unit = Convert.ToUInt32(n);
                                            ModifiedValue = n_unit;
                                        }
                                        return true;
                                    }
                                }
                            }
                        }
                        break;
                    case "UInt64":
                        {
                            long o = Convert.ToInt64(value);
                            if (Locked)
                            {
                                EditorGUILayout.LabelField(label, o.ToString());
                            }
                            else
                            {
                                long n = Math.Max(EditorGUILayout.LongField(label, o), 0);
                                if (n != o && !Locked)
                                {
                                    ulong n_ulong = Convert.ToUInt64(n);
                                    ModifiedValue = n_ulong;
                                    return true;
                                }
                            }
                        }
                        break;
                    case "String":
                        {
                            string o = (string)value;
                            string n = o;
                            if (Locked)
                            {
                                if (AttributeUtility.HasAttribute<MultilineAttribute>(attributes, out var MultilineAttributeIns))
                                {

                                    EditorGUILayout.LabelField(label);
                                    if (MultilineAttributeIns.lines < 1)
                                    {
                                        EditorGUILayout.LabelField(o, MultilineTextAreaStyle);
                                    }
                                    else
                                    {
                                        EditorGUILayout.LabelField(o, MultilineTextAreaStyle, GUILayout.MinHeight(MultilineTextAreaStyle.lineHeight * MultilineAttributeIns.lines + 4));
                                    }

                                }
                                else if (AttributeUtility.HasAttribute<TextAreaAttribute>(attributes, out var TextAreaAttributeIns))
                                {
                                    EditorGUILayout.LabelField(label);
                                    EditorGUILayout.LabelField(o, MultilineTextAreaStyle, GUILayout.MinHeight(MultilineTextAreaStyle.lineHeight * MultilineAttributeIns.lines + 4));
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(label, o);
                                }
                            }
                            else
                            {
                                CSOPathTag pTag = CSOPathTag.AssetPath;
                                if (AttributeUtility.HasAttribute<MultilineAttribute>(attributes, out var MultilineAttributeIns))
                                {

                                    EditorGUILayout.LabelField(label);
                                    if (MultilineAttributeIns.lines < 1)
                                    {
                                        n = EditorGUILayout.TextArea(o, MultilineTextAreaStyle);
                                        if (n != o)
                                        {
                                            ModifiedValue = n;
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        n = EditorGUILayout.TextArea(o, MultilineTextAreaStyle, GUILayout.MinHeight(MultilineTextAreaStyle.lineHeight * MultilineAttributeIns.lines + 4));
                                        if (n != o)
                                        {
                                            ModifiedValue = n;
                                            return true;
                                        }
                                    }

                                }
                                else if (AttributeUtility.HasAttribute<TextAreaAttribute>(attributes, out var TextAreaAttributeIns))
                                {
                                    EditorGUILayout.LabelField(label);
                                    n = EditorGUILayout.TextArea(o, MultilineTextAreaStyle, GUILayout.MinHeight(MultilineTextAreaStyle.lineHeight * TextAreaAttributeIns.minLines + 4), GUILayout.MaxHeight(MultilineTextAreaStyle.lineHeight * TextAreaAttributeIns.maxLines + 4));
                                }
                                else
                                {

                                    bool hasCSOPathToUObjFeild = AttributeUtility.HasAttribute<CSOPathToUObjFeildAttribute>(attributes, out var CSOPathToUObjFeildAttributeIns);
                                    bool hasCSOFilePathSelectTool = AttributeUtility.HasAttribute<CSOFilePathSelectToolAttribute>(attributes, out var CSOFilePathSelectToolAttributeIns);
                                    bool hasCSODirPathSelectTool = AttributeUtility.HasAttribute<CSODirPathSelectToolAttribute>(attributes, out var CSODirPathSelectToolAttributeIns);
                                    bool hasCSOPathOpenDirInExplorerTool = AttributeUtility.HasAttribute<CSOPathOpenDirInExplorerToolAttribute>(attributes, out var CSOPathOpenDirInExplorerToolAttributeIns);

                                    if (hasCSOPathToUObjFeild)
                                        CheckPathTagOverride(ref pTag, CSOPathToUObjFeildAttributeIns.PathTag);
                                    else if(hasCSOFilePathSelectTool)
                                        CheckPathTagOverride(ref pTag, CSOFilePathSelectToolAttributeIns.PathTag);
                                    else if(hasCSODirPathSelectTool)
                                        CheckPathTagOverride(ref pTag, CSODirPathSelectToolAttributeIns.PathTag);
                                    else if (hasCSOPathOpenDirInExplorerTool)
                                        CheckPathTagOverride(ref pTag, CSOPathOpenDirInExplorerToolAttributeIns.PathTag);
                                    
                                    EditorGUILayout.BeginHorizontal();
                                    {

                                        if (hasCSOPathToUObjFeild)
                                        {
                                            string l = CSOPathToUObjFeildAttributeIns.Label;
                                            if (string.IsNullOrEmpty(l))
                                            {
                                                if (AttributeUtility.HasAttribute<CSOFieldLabelAttribute>(attributes, out var CSOFieldLabelAttributeIns))
                                                    l = CSOFieldLabelAttributeIns.Label;
                                                else
                                                    l = label;
                                                }
                                            string p = o;
                                            UnityEngine.Object oIns = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);
                                            UnityEngine.Object nIns = EditorGUILayout.ObjectField(l, oIns, typeof(UnityEngine.Object), false);
                                            if (oIns != nIns)
                                            {
                                                if(nIns != null)
                                                {
                                                string np = AssetDatabase.GetAssetPath(nIns);
                                                if (!string.IsNullOrEmpty(np))
                                                    n = np;
                                                    else
                                                        n = string.Empty;
                                                }
                                                else
                                                {
                                                    n = string.Empty;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            n = EditorGUILayout.TextField(label, o);
                                            Rect rect = GUILayoutUtility.GetLastRect();
                                            if(HandleDropAndDragEvents(ref rect, out var nPath))
                                            {
                                                if (!string.IsNullOrEmpty(nPath))
                                                    n = NormalizePathString(nPath, pTag);
                                            }
                                        }
                                        if (hasCSOFilePathSelectTool)
                                        {
                                            if (GUILayout.Button("Select", GUILayout.Width(50)))
                                            {
                                                string d = string.Empty;
                                                if (!string.IsNullOrEmpty(n))
                                                    d = n.Substring(0, n.LastIndexOf("/"));
                                                string p = EditorUtility.OpenFilePanel("选择文件", d, "");
                                                if (!string.IsNullOrEmpty(p))
                                                    n = NormalizePathString(p, pTag);
                                                    }
                                                }
                                        else if (hasCSODirPathSelectTool)
                                        {
                                            if (GUILayout.Button("Select", GUILayout.Width(50)))
                                            {
                                                string d = string.Empty;
                                                if (!string.IsNullOrEmpty(n))
                                                    d = n.Substring(0, n.LastIndexOf("/"));
                                                string p = EditorUtility.OpenFolderPanel("选择文件夹", d, "");
                                                if (!string.IsNullOrEmpty(p))
                                                {
                                                    n = NormalizePathString(p, pTag);
                                                }
                                            }
                                        }
                                        if (hasCSOPathOpenDirInExplorerTool)
                                        {
                                            if (!string.IsNullOrEmpty(o))
                                            {
                                                if (GUILayout.Button("Explore", GUILayout.Width(60)))
                                                {
                                                    string openFullPath = o.Replace("\\", "/");
                                                    if (openFullPath.StartsWith("Assets/"))
                                                    {
                                                        openFullPath = $"{Application.dataPath.Replace("Assets", "")}{openFullPath}";
                                                    }
                                                    else if (!openFullPath.Contains(":/"))
                                                    {
                                                        openFullPath = $"{Application.dataPath}/Resources/{openFullPath}";
                                                    }
                                                    if (Directory.Exists(openFullPath))
                                                    {
                                                        OpenDirInExplorer(openFullPath);
                                                    }
                                                    else
                                                    {
                                                        openFullPath = openFullPath.Substring(0, openFullPath.LastIndexOf("/"));
                                                        OpenDirInExplorer(openFullPath);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                GUI.color = Color.gray;
                                                if (GUILayout.Button("Explore", GUILayout.Width(50)))
                                                {
                                                    //do nothing ...
                                                }
                                                GUI.color = Color.white;
                                            }
                                        }
                                    
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    if (n != o)
                                    {
                                        ModifiedValue = n;
                                        return true;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        {

                            int hash = value != null ? value.GetHashCode() : 0;

                            //判断该字段是一个类或者结构体
                            if (TypeIsClass(valueType) || TypeIsStruct(valueType))
                            {

                                if (!m_subObjInfoDic.ContainsKey(hash))
                                    m_subObjInfoDic.Add(hash, new SubObjInfo(hash, value));

                                bool subDirty = false;

                                m_subObjInfoDic[hash].IsFold = EditorGUILayout.Foldout(m_subObjInfoDic[hash].IsFold, label);
                                if (m_subObjInfoDic[hash].IsFold)
                                {
                                    GUILayout.Space(2);
                                    EditorGUI.indentLevel++;

                                    COSScrollAttributeCheckDraw(hash, attributes, () =>
                                    {
                                        if (m_subObjInfoDic[hash].GUIDrawer.DrawInspectorGUI())
                                        {
                                            subDirty = true;
                                        }
                                    });

                                    EditorGUI.indentLevel--;

                                }

                                if (subDirty)
                                {
                                    ModifiedValue = m_subObjInfoDic[hash].GUIDrawer.BindingIns;
                                    UpdateSubObjInfo(hash, ModifiedValue.GetHashCode());
                                    return true;
                                }

                            }
                            //枚举
                            else if (TypeIsEnum(valueType))
                            {
                                Enum o = (Enum)value;
                                Enum n = EditorGUILayout.EnumPopup(o);
                                if (!n.Equals(o))
                                {
                                    ModifiedValue = n;                                    
                                    return true;
                                }
                            }
                            //数组
                            else if (TypeIsArray(valueType))
                            {

                                if (value == null)
                                {
                                    ModifiedValue = NewInsByType(valueType);
                                    return true;
                                }

                                if (!m_subObjInfoDic.ContainsKey(hash))
                                    m_subObjInfoDic.Add(hash, new SubObjInfo(hash));

                                m_subObjInfoDic[hash].IsFold = EditorGUILayout.Foldout(m_subObjInfoDic[hash].IsFold, label);
                                if (m_subObjInfoDic[hash].IsFold)
                                {
                                    bool subDirty = false;
                                    var gType = valueType.GetElementType();
                                    GUILayout.Space(2);
                                    EditorGUI.indentLevel++;

                                    Array v = value as Array;
                                    COSScrollAttributeCheckDraw(hash, attributes, () => 
                                    {

                                        int size = EditorGUILayout.IntField("Size", v.Length);
                                        if (size != v.Length)
                                        {
                                            Array nv = Array.CreateInstance(gType, size);
                                            for (int i = 0; i < size; i++)
                                            {
                                                if (i < v.Length)
                                                    nv.SetValue(v.GetValue(i), i);
                                                else
                                                {
                                                    var newValue = NewInsByType(gType);
                                                    nv.SetValue(newValue, i);
                                                }
                                            }
                                            v = nv;
                                            subDirty = true;
                                        }

                                        if (AttributeUtility.HasAttribute<CSOTurnPageAttribute>(attributes, out var COSTPScrollAttributeIns))
                                        {
                                            int pidx = m_subObjInfoDic[hash].pageIndex;
                                            int count = v.Length;
                                            int pageNum = COSTPScrollAttributeIns.LimitPerPage;
                                            int pageCount = Mathf.CeilToInt((float)count / pageNum);
                                            int s = pidx * pageNum;
                                            int len = s + pageNum;
                                            for (; s < len; s++)
                                            {
                                                if(s < count)
                                                {
                                                    object subValue = v.GetValue(s);
                                                    if (DrawValueEditUI($"[{s}]", gType, subValue, null, out var subModifiedValue))
                                                    {
                                                        v.SetValue(subModifiedValue, s);
                                                        subDirty = true;
                                                    }
                                                }
                                            }
                                            if(count > pageNum)
                                                m_subObjInfoDic[hash].pageIndex = DrawTurnPageUI(pidx, pageCount);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < size; i++)
                                            {
                                                object subValue = v.GetValue(i);
                                                if (DrawValueEditUI($"[{i}]", gType, subValue, null, out var subModifiedValue))
                                                {
                                                    v.SetValue(subModifiedValue, i);
                                                    subDirty = true;
                                                }
                                            }
                                        }
                                    });

                                    EditorGUI.indentLevel--;

                                    GUILayout.Space(2);
                                    if (subDirty)
                                    {
                                        ModifiedValue = v;
                                        UpdateSubObjInfo(hash, ModifiedValue.GetHashCode());
                                        return true;
                                    }
                                }
                            }
                            //List
                            else if (TypeIsList(valueType))
                            {

                                if (value == null)
                                {
                                    ModifiedValue = NewInsByType(valueType);
                                    return true;
                                }

                                if (!m_subObjInfoDic.ContainsKey(hash))
                                    m_subObjInfoDic.Add(hash, new SubObjInfo(hash));

                                m_subObjInfoDic[hash].IsFold = EditorGUILayout.Foldout(m_subObjInfoDic[hash].IsFold, label);
                                if (m_subObjInfoDic[hash].IsFold)
                                {
                                    bool subDirty = false;
                                    var gType = valueType.GetGenericArguments()[0];
                                    GUILayout.Space(2);
                                    EditorGUI.indentLevel++;
                                    IList v = value as IList;

                                    COSScrollAttributeCheckDraw(hash, attributes, () =>
                                    {

                                        int size = EditorGUILayout.IntField("Size", v.Count);
                                        if (size != v.Count)
                                        {
                                            IList nv = Activator.CreateInstance(valueType) as IList;
                                            for (int i = 0; i < size; i++)
                                            {
                                                if (i < v.Count)
                                                    nv.Add(v[i]);
                                                else
                                                    nv.Add(NewInsByType(gType));
                                            }
                                            v = nv;
                                            subDirty = true;
                                        }

                                        if (AttributeUtility.HasAttribute<CSOTurnPageAttribute>(attributes, out var COSTPScrollAttributeIns))
                                        {

                                            int pidx = m_subObjInfoDic[hash].pageIndex;
                                            int count = v.Count;
                                            int pageNum = COSTPScrollAttributeIns.LimitPerPage;
                                            int pageCount = Mathf.CeilToInt((float)count / pageNum);
                                            int s = pidx * pageNum;
                                            int len = s + pageNum;
                                            for (; s < len; s++)
                                            {
                                                if (s < count)
                                                {
                                                    object subValue = v[s];
                                                    if (DrawValueEditUI($"[{s}]", gType, subValue, null, out var subModifiedValue))
                                                    {
                                                        v[s] = subModifiedValue;
                                                        subDirty = true;
                                                    }
                                                }
                                            }
                                            if (count > pageNum)
                                                m_subObjInfoDic[hash].pageIndex = DrawTurnPageUI(pidx, pageCount);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < size; i++)
                                            {
                                                object subValue = v[i];
                                                if (DrawValueEditUI($"[{i}]", gType, subValue, null, out var subModifiedValue))
                                                {
                                                    v[i] = subModifiedValue;
                                                    subDirty = true;
                                                }
                                            }

                                        }

                                    });

                                    EditorGUI.indentLevel--;
                                    GUILayout.Space(2);
                                    if (subDirty)
                                    {
                                        ModifiedValue = v;
                                        UpdateSubObjInfo(hash, ModifiedValue.GetHashCode());
                                        return true;
                                    }
                                }

                            }
                            else if (value == null)
                            {
                                //Null值容错显示... 能看到这个数据上可能有些逻辑问题
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(label, "[Null]");
                                if (GUILayout.Button("New", GUILayout.Width(26)))
                                {
                                    ModifiedValue = Activator.CreateInstance(valueType);
                                    return true;
                                }
                                ModifiedValue = value;
                                GUILayout.EndHorizontal();
                                return false;
                            }
                        }
                        break;
                }
            }

            ModifiedValue = value;
            return false;
        }

        protected void CheckPathTagOverride(ref CSOPathTag pTag, CSOPathTag target)
        {
            if (target != CSOPathTag.Inherit)
                pTag = target;
        }

        /// <summary>
        /// 根据CSOPathTag输出标准化的路径字符串
        /// </summary>
        protected string NormalizePathString(string path, CSOPathTag tag)
        {
            path = path.Replace("\\", "/");
            if (path.Contains(":/"))
            {
                //Full Path
                switch (tag)
                {
                    case CSOPathTag.AssetPath:
                        path = path.Replace(Application.dataPath, "Assets");
                        break;
                    case CSOPathTag.ResourcesPath:
                        string dir = path.Substring(0, path.LastIndexOf("/"));
                        string p2 = Path.GetFileNameWithoutExtension(path);
                        path = $"{dir.Replace($"{Application.dataPath}/Resources/", "")}/{p2}";
                        break;
                }

            }else if (path.StartsWith("Assets/"))
            {
                //Asset Path
                switch (tag)
                {
                    case CSOPathTag.FullPath:
                        path = $"{Application.dataPath.Replace("Assets", "")}{path}";
                        break;
                    case CSOPathTag.ResourcesPath:
                        string dir = path.Substring(0, path.LastIndexOf("/"));
                        string p2 = Path.GetFileNameWithoutExtension(path);
                        path = $"{dir.Replace($"Assets/Resources/", "")}/{p2}";
                        break;
                }
            }

            return path;

        }

        protected object NewInsByType(Type valueType)
        {
            if (TypeIsUObject(valueType))
            {
                return null;
            }
            else if (valueType == typeof(AnimationCurve))
            {
                return new AnimationCurve();
            }
            else if (valueType == typeof(Gradient))
            {
                return new Gradient();
            }
            else if(valueType == typeof(Color))
            {
                return Color.black;
            }
            else if (valueType == typeof(Vector2))
            {
                return Vector2.zero;
            }
            else if (valueType == typeof(Vector2Int))
            {
                return Vector2Int.zero;
            }
            else if (valueType == typeof(Vector3))
            {
                return Vector3.zero;
            }
            else if (valueType == typeof(Vector3Int))
            {
                return Vector3Int.zero;
            }
            else if (valueType == typeof(Vector4))
            {
                return Vector4.zero;
            }
            else if (valueType == typeof(Rect))
            {
                return Rect.zero;
            }
            else if (valueType == typeof(RectInt))
            {
                return new RectInt();
            }
            else if (valueType == typeof(Bounds))
            {
                return new Bounds();
            }
            else if (valueType == typeof(BoundsInt))
            {
                return new BoundsInt();
            }
            //others
            switch (valueType.Name)
            {
                case "Boolean":
                    return false;
                case "Single":
                    return 0.0f;
                case "Double":
                    return 0.0d;
                case "Byte":
                    return 0;
                case "Int16":
                case "Int32":
                    return 0;
                case "Int64":
                    return 0;
                case "String":
                    return string.Empty;
                default:
                    {
                        //判断该字段是一个类或者结构体
                        if (TypeIsClass(valueType) || TypeIsStruct(valueType))
                        {
                            return Activator.CreateInstance(valueType);
                        }
                        //枚举
                        else if (TypeIsEnum(valueType))
                        {
                            return Activator.CreateInstance(valueType);
                        }
                        //数组
                        else if (TypeIsArray(valueType))
                        {
                            Type gType = valueType.GetElementType();
                            return  Array.CreateInstance(gType, 0);
                        }
                        //List
                        else if (TypeIsList(valueType))
                        {
                            return Activator.CreateInstance(valueType);
                        }
                    }
                    break;
            }
            return null;
        }
        
        protected bool COSScrollAttributeCheckDraw(int hash, object[] attributes, Action drawing)
        {
            if (AttributeUtility.HasAttribute<CSOScrollAttribute>(attributes, out var COSScrollAttributeIns))
            {
                if (!m_subObjInfoDic.ContainsKey(hash))
                    m_subObjInfoDic.Add(hash, new SubObjInfo(hash));

                bool horizontal = COSScrollAttributeIns.IsHorizontal;
                GUILayoutOption minSize = COSScrollAttributeIns.MinSize > 0 ? (horizontal ? GUILayout.MinWidth(COSScrollAttributeIns.MinSize) : GUILayout.MinHeight(COSScrollAttributeIns.MinSize)) : null;
                GUILayoutOption maxSize = COSScrollAttributeIns.MaxSize > 0 ? (horizontal ? GUILayout.MaxWidth(COSScrollAttributeIns.MaxSize) : GUILayout.MaxHeight(COSScrollAttributeIns.MaxSize)) : null;
                if (minSize != null && maxSize != null)
                    m_subObjInfoDic[hash].ScrollPos = EditorGUILayout.BeginScrollView(m_subObjInfoDic[hash].ScrollPos, minSize, maxSize);
                else if (minSize != null)
                    m_subObjInfoDic[hash].ScrollPos = EditorGUILayout.BeginScrollView(m_subObjInfoDic[hash].ScrollPos, minSize);
                else if (maxSize != null)
                    m_subObjInfoDic[hash].ScrollPos = EditorGUILayout.BeginScrollView(m_subObjInfoDic[hash].ScrollPos, maxSize);
                else
                    m_subObjInfoDic[hash].ScrollPos = EditorGUILayout.BeginScrollView(m_subObjInfoDic[hash].ScrollPos);

                drawing();

                EditorGUILayout.EndScrollView();
                return true;
            }
            else
            {
                drawing();
                return false;
            }
        }

        protected void UpdateSubObjInfo(int oldHash, int newHash)
        {
            if(m_subObjInfoDic.TryGetValue(oldHash, out var v))
            {
                m_subObjInfoDic.Remove(oldHash);
                if (m_subObjInfoDic.ContainsKey(newHash))
                    m_subObjInfoDic[newHash] = v;
                else
                    m_subObjInfoDic.Add(newHash, v);
            }
        }

        protected int DrawTurnPageUI(int pageIndex, int pageCounts)
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
                        GUI.FocusControl(null);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            return pageIndex;
        }

        public static void OpenDirInExplorer(string fullDirPath)
        {
            string args = fullDirPath.Replace("/", "\\");
            RunCmd("explorer.exe", args, out _, out var error);
            if(!string.IsNullOrEmpty(error))
                UnityEngine.Debug.LogError(error);
        }

        protected bool HandleDropAndDragEvents(ref Rect rect, out string nPath)
        {
            Event evt = Event.current;
            if (rect.Contains(evt.mousePosition) && 
                DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (evt.type == EventType.DragExited)
                {
                    evt.Use();
                    nPath = DragAndDrop.paths[0];
                    return true;
                }

            }
            nPath = string.Empty;
            return false;
        }

        //------- Unity 内部执行cmd命令行

        #region Unity 内部执行cmd命令行

        public static void RunCmd(string cmd, string args, out string output, out string error)
        {
            RunCmd(cmd, args, null, out output, out error);
        }

        public static void RunCmd(string cmd, string args, string dir, out string output, out string error)
        {
            var p = CreateCmdProcess(cmd, args, dir);
            output = p.StandardOutput.ReadToEnd();
            error = p.StandardError.ReadToEnd();
            p.Close();
        }

        // cmd 表示第一个参数
        // args 表示第二个参数，可以置空
        // dir 为命令行运行的目录，默认置空
        private static Process CreateCmdProcess(string cmd, string args, string dir = null)
        {
            // 设置进程参数
            var p = new ProcessStartInfo(cmd);

            p.Arguments = args;
            p.CreateNoWindow = true;
            p.UseShellExecute = false;
            p.RedirectStandardError = true;
            p.RedirectStandardInput = true;
            p.RedirectStandardOutput = true;
            p.StandardErrorEncoding = System.Text.Encoding.GetEncoding("gb2312");
            p.StandardOutputEncoding = System.Text.Encoding.GetEncoding("gb2312");

            // 判断工作目录是否为空，如果非空那么就设置工作目录
            if (!string.IsNullOrEmpty(dir))
            {
                p.WorkingDirectory = dir;
            }

            // 一切就绪，启动进程！
            return Process.Start(p);
        }

        #endregion
    }
}
