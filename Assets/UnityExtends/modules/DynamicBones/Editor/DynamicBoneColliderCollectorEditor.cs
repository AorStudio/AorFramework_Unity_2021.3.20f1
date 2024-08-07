using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.Rendering.Universal.DynamicBones.Editor 
{
    [CustomEditor(typeof(DynamicBoneColliderCollector))]
    public class DynamicBoneColliderCollectorEditor : UnityEditor.Editor
    {

        #region Inner Styles

        private static GUIStyle m_innerTitleStyle;
        protected static GUIStyle InnerTitleStyle
        {
            get
            {
                if(m_innerTitleStyle == null)
                {
                    m_innerTitleStyle = new GUIStyle(EditorStyles.label);
                    m_innerTitleStyle.wordWrap = true;
                    m_innerTitleStyle.fontStyle = FontStyle.Bold;
                }
                return m_innerTitleStyle;
            }
        }

        #endregion

        #region Inner Datas

        private struct InnerAddData
        {
            public InnerAddData(string key, DynamicBoneCollider collider)
            {
                this.key = key;
                this.collider = collider;
            }
            public string key;
            public DynamicBoneCollider collider;
        }

        private struct InnerModifyData
        {
            public InnerModifyData(string oldKey, string newKey, DynamicBoneCollider collider)
            {
                this.oldKey = oldKey;
                this.newKey = newKey;
                this.collider = collider;
            }
            public string oldKey;
            public string newKey;
            public DynamicBoneCollider collider;
        }

        #endregion

        private DynamicBoneColliderCollector m_target;
        private void Awake()
        {
            m_target = (DynamicBoneColliderCollector)target;
        }

        private readonly Queue<InnerAddData> m_needAddDatas = new Queue<InnerAddData>();
        private readonly Queue<InnerModifyData> m_needModifyDatas = new Queue<InnerModifyData>();
        private readonly Queue<string> m_needDelDatas = new Queue<string>();

        private bool m_isAdd;
        private bool m_isModify;

        public override void OnInspectorGUI()
        {

            //显示已经存在的数据
            if (m_target.CollidersDic.Count > 0)
            {

                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);
                    Draw_InnerTitle("DynamicBoneColliders");
                    GUILayout.Space(5);

                    foreach (var kv in m_target.CollidersDic)
                    {
                        Draw_DataItemUI(kv);
                    }

                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.Space(5);

            }
            else
            {

                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);
                    Draw_InnerTitle("DynamicBoneColliders");
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUI.color = Color.gray;
                        GUILayout.Label("[No Collider Datas]", InnerTitleStyle);
                        GUI.color = Color.white;
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.Space(5);

            }

            if (m_isModify)
            {
                Draw_ModifyUI(true);
            }
            else
            {
                if (m_isAdd)
                {
                    Draw_ModifyUI();
                }
                else
                {
                    GUILayout.BeginHorizontal("box");
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("+", GUILayout.Width(64)))
                        {
                            m_modifyBackgroundColor = Color.green;
                            m_isAdd = true;
                        }
                        if (GUILayout.Button("AUTO", GUILayout.Width(64)))
                        {
                            if (EditorUtility.DisplayDialog("提示", "自动添加节点以下所有的DynamicBoneCollider,并以节点名命名,是否确认操作?", "确认", "取消"))
                            {
                                DynamicBoneCollider[] finded = m_target.GetComponentsInChildren<DynamicBoneCollider>(true);
                                if(finded != null && finded.Length > 0)
                                {
                                    foreach (var f in finded)
                                    {
                                        if (m_target.CollidersDic.ContainsKey(f.gameObject.name))
                                        {
                                            m_needModifyDatas.Enqueue(new InnerModifyData(f.gameObject.name, f.gameObject.name, f));
                                        }
                                        else
                                        {
                                            m_needAddDatas.Enqueue(new InnerAddData(f.gameObject.name, f));
                                        }
                                    }
                                }
                                EditorUtility.SetDirty(m_target);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            //先删除, 再修改, 最后添加
            bool dirty = false;
            if(m_needDelDatas.Count > 0)
            {
                while (m_needDelDatas.Count > 0)
                {
                    string key = m_needDelDatas.Dequeue();
                    if (m_target.CollidersDic.ContainsKey(key))
                    {
                        m_target.CollidersDic.Remove(key);
                        dirty = true;
                    }
                }
            }
            if (m_needModifyDatas.Count > 0)
            {
                while (m_needModifyDatas.Count > 0)
                {
                    InnerModifyData modifyData = m_needModifyDatas.Dequeue();
                    if(modifyData.oldKey == modifyData.newKey)
                    {
                        if (m_target.CollidersDic.ContainsKey(modifyData.oldKey))
                        {
                            m_target.CollidersDic[modifyData.oldKey] = modifyData.collider;
                        }
                        else
                        {
                            //应该走不到这里来。。。
                            m_target.CollidersDic.Add(modifyData.newKey, modifyData.collider);
                        }
                        dirty = true;
                    }
                    else
                    {
                        if (m_target.CollidersDic.ContainsKey(modifyData.oldKey) && !m_target.CollidersDic.ContainsKey(modifyData.newKey))
                        {
                            m_target.CollidersDic.Remove(modifyData.oldKey);
                            m_target.CollidersDic.Add(modifyData.newKey, modifyData.collider);
                            dirty = true;
                        }
                        else
                        {
                            //Error ...
                        }
                    }
                }
            }
            if (m_needAddDatas.Count > 0)
            {
                while (m_needAddDatas.Count > 0)
                {
                    InnerAddData innerData = m_needAddDatas.Dequeue();
                    if (m_target.CollidersDic.ContainsKey(innerData.key))
                    {
                        m_target.CollidersDic[innerData.key] = innerData.collider;
                    }
                    else
                    {
                        m_target.CollidersDic.Add(innerData.key, innerData.collider);
                    }
                    dirty = true;
                }
            }
            if (dirty)
            {
                EditorUtility.SetDirty(m_target);
            }
        }
        
        private void Draw_InnerTitle(string title)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(title, InnerTitleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        private void Draw_DataItemUI(KeyValuePair<string, DynamicBoneCollider> kv)
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(kv.Key))
                {
                    //复制内容到剪贴板
                    GUIUtility.systemCopyBuffer = kv.Key;
                    EditorUtility.DisplayDialog("提示", "已复制 " + kv.Key + " 到剪贴板!", "OK");
                }
                EditorGUILayout.ObjectField(kv.Value, typeof(DynamicBoneCollider), true);
                if (GUILayout.Button("M", GUILayout.Width(22)))
                {
                    m_modifyBackgroundColor = Color.yellow;
                    m_modify_key = m_modify_keyCache = kv.Key;
                    m_modify_collider = kv.Value;
                    m_isModify = true;
                }
                if (GUILayout.Button("-", GUILayout.Width(16)))
                {
                    if (EditorUtility.DisplayDialog("提示", "确认删除此条数据?", "确认", "取消"))
                    {
                        m_needDelDatas.Enqueue(kv.Key);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private string m_modify_keyCache;
        private string m_modify_key;
        private DynamicBoneCollider m_modify_collider;

        private void ResetModifyDatas()
        {
            m_modify_key = "";
            m_modify_keyCache = "";
            m_modify_collider = null;
        }

        private Color m_modifyBackgroundColor = Color.yellow;

        private void Draw_ModifyUI(bool isModify = false)
        {
            GUI.backgroundColor = m_modifyBackgroundColor;
            GUILayout.BeginVertical("box");
            {
                m_modify_key = EditorGUILayout.TextField("Key", m_modify_key);
                m_modify_collider = (DynamicBoneCollider)EditorGUILayout.ObjectField("Collider", m_modify_collider, typeof(DynamicBoneCollider), true);
                GUILayout.BeginHorizontal();
                {
                    if(!string.IsNullOrEmpty(m_modify_key) && m_modify_collider)
                    {
                        if (GUILayout.Button("确定"))
                        {
                            string k = m_modify_key;
                            DynamicBoneCollider c = m_modify_collider;
                            if (isModify)
                            {
                                string m = m_modify_keyCache;
                                m_needModifyDatas.Enqueue(new InnerModifyData(m, k, c));
                            }
                            else
                            {
                                m_needAddDatas.Enqueue(new InnerAddData(k, c));
                            }
                            ResetModifyDatas();
                            m_isAdd = false;
                            m_isModify = false;
                        }
                    }
                    else
                    {
                        GUI.color = Color.gray;
                        if (GUILayout.Button("确定"))
                        {
                            //Do nothing ...
                        }
                        GUI.color = Color.white;
                    }
                    if (GUILayout.Button("取消"))
                    {
                        ResetModifyDatas();
                        m_isAdd = false;
                        m_isModify = false;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

    }

}


