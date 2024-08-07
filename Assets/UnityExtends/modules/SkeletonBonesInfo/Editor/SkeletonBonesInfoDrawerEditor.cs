using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.Rendering.Universal.EditorSupportTool.Editor
{

    [CustomEditor(typeof(SkeletonBonesInfoDrawer))]
    public class SkeletonBonesInfoDrawerEditor : UnityEditor.Editor
    {

        private SkeletonBonesInfoDrawer m_target;
        private void Awake()
        {
            m_target = (SkeletonBonesInfoDrawer)target;
        }

        private void OnDestroy()
        {
            m_target = null;
        }

        private bool m_fold_infos = true;
        private bool m_fold_options = true;

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                m_fold_options = EditorGUILayout.Foldout(m_fold_options, "Gizmo Options");
                if (m_fold_options)
                {
                    GUILayout.Space(5);
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("BoneColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SimpleLineDraw"));
                    if (!serializedObject.FindProperty("SimpleLineDraw").boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("BoneHRadius"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("BoneCenterLerp"));
                    }
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawBoneTopSphere"));
                    if (serializedObject.FindProperty("DrawBoneTopSphere").boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("BoneTopSphereScale"));
                    }
                    serializedObject.ApplyModifiedProperties();
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.Space(12);

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                m_fold_infos = EditorGUILayout.Foldout(m_fold_options, "Bones Info");
                if (m_fold_infos)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField($"Bones Count : {m_target.BonesCount}");
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

    }

}