using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AORCore;

namespace UnityEngine.Rendering.Universal.DynamicBones.Editor
{

    [CustomEditor(typeof(DynamicBoneLookAt))]
    public class DynamicBoneLookAtEditor : UnityEditor.Editor
    {
        private DynamicBoneLookAt m_target;
        private void Awake()
        {
            m_target = target as DynamicBoneLookAt;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {

                GUILayout.Space(12);
                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(12);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUI.color = Color.gray;
                        GUILayout.Label("(Runtime DynamicBoneLookAtData)");
                        GUI.color = Color.white;
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    DynamicBoneLookAtData data = (DynamicBoneLookAtData)m_target.GetNonPublicField("m_Data");
                    Draw_DynamicBoneLookAtData_UI(data);

                    GUILayout.Space(12);
                }
                GUILayout.EndVertical();

                Repaint();
            }
        }
        
        private void Draw_DynamicBoneLookAtData_UI(DynamicBoneLookAtData data)
        {
            EditorGUILayout.ObjectField("BindingNode", data.BindingNode, typeof(Transform), true);
            EditorGUILayout.ObjectField("LookAtNode", data.LookAtNode, typeof(Transform), true);
            EditorGUILayout.Vector3Field("AngleOffset", data.AngleOffset);
            EditorGUILayout.FloatField("AngleOffsetInterpolationSpeed", data.AngleOffsetInterpolationSpeed);
            EditorGUILayout.Vector3Field("ForwardReference", data.ForwardReference);
            EditorGUILayout.IntField("ParentBonesDepth", data.ParentBonesDepth);
            EditorGUILayout.FloatField("ParentBonesWeight", data.ParentBonesWeight);
            EditorGUILayout.ToggleLeft("ParentBonesWeightUseCurve", data.ParentBonesWeightUseCurve);
            EditorGUILayout.CurveField("ParentBonesWeightCurve", data.ParentBonesWeightCurve);
            EditorGUILayout.ToggleLeft("UseAdditionLocalEulerOffset", data.UseAdditionLocalEulerOffset);
            EditorGUILayout.CurveField("AdditionLocalEulerOffsetX", data.AdditionLocalEulerOffsetX);
            EditorGUILayout.CurveField("AdditionLocalEulerOffsetY", data.AdditionLocalEulerOffsetY);
            EditorGUILayout.CurveField("AdditionLocalEulerOffsetZ", data.AdditionLocalEulerOffsetZ);
            EditorGUILayout.FloatField("EffectWeight", m_target.EffectWeight);

            float m_effectWeight_FadeValue = (float)m_target.GetNonPublicField("m_effectWeight_FadeValue");
            GUI.color = Color.gray;
            float e = m_effectWeight_FadeValue * m_target.EffectWeight;
            EditorGUILayout.FloatField($"EffectWeight(withFadeValue)", e);
            GUI.color = Color.white;
        }

    }
}
