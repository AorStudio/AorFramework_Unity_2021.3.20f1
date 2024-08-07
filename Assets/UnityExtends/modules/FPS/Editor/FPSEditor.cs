using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace UnityEngine.Rendering.Universal.Editor 
{

    [CustomEditor(typeof(FPS))]
    public class FPSEditor : UnityEditor.Editor
    {

        private FPS m_target;
        private void Awake()
        {
            m_target = target as FPS;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();
            GUILayout.Space(5);

            GUILayout.BeginVertical("box");
            {
                UnityEngine.Object uguiText = serializedObject.FindProperty("m_UGUIText").objectReferenceValue;
                UnityEngine.Object nUguiText = EditorGUILayout.ObjectField("使用UGUIText显示", uguiText, typeof(Text), true);
                if(nUguiText != uguiText)
                    serializedObject.FindProperty("m_UGUIText").objectReferenceValue = nUguiText;
            }
            GUILayout.EndVertical();
            if (serializedObject.FindProperty("m_UGUIText").objectReferenceValue == null)
            {
                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label($"GUI显示配置项");
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_panelSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_panelPosOffset"));
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
            }
            if (!Application.isPlaying)
            {
                GUILayout.BeginVertical("Box");
                {
                    bool Foucs = serializedObject.FindProperty("FoucsFrameRateLimt").boolValue;
                    bool nFoucs = EditorGUILayout.ToggleLeft("强制锁定目标帧率", Foucs);
                    if (nFoucs != Foucs)
                        serializedObject.FindProperty("FoucsFrameRateLimt").boolValue = nFoucs;

                    if (nFoucs)
                    {
                        int FoucsTatgetFrameRate = serializedObject.FindProperty("FoucsTatgetFrameRate").intValue;
                        int nFoucsTatgetFrameRate = EditorGUILayout.IntSlider(FoucsTatgetFrameRate, -1, 300);
                        if (nFoucsTatgetFrameRate != FoucsTatgetFrameRate)
                            serializedObject.FindProperty("FoucsTatgetFrameRate").intValue = nFoucsTatgetFrameRate;
                    }

                }
                GUILayout.EndVertical();
            }
            else
            {
#if UNITY_EDITOR && !RUNTIME
                //isPlaying
                GUILayout.BeginVertical("Box");
                {

                    var Foucs = serializedObject.FindProperty("FoucsFrameRateLimt").boolValue;
                    if (Foucs)
                    {

                        Foucs = serializedObject.FindProperty("FoucsFrameRateLimt").boolValue;
                        bool nFoucs = EditorGUILayout.ToggleLeft("强制锁定目标帧率", Foucs);
                        if (nFoucs != Foucs)
                            serializedObject.FindProperty("FoucsFrameRateLimt").boolValue = nFoucs;

                        int FoucsTatgetFrameRate = serializedObject.FindProperty("FoucsTatgetFrameRate").intValue;
                        int nFoucsTatgetFrameRate = EditorGUILayout.IntSlider(FoucsTatgetFrameRate, -1, 300);
                        if (nFoucsTatgetFrameRate != FoucsTatgetFrameRate)
                            serializedObject.FindProperty("FoucsTatgetFrameRate").intValue = nFoucsTatgetFrameRate;

                    }
                    else
                    {
                        //非强制锁定模式
                        bool FRTenabled = m_target.FrameRateLimt;
                        bool nFRTenabled = EditorGUILayout.ToggleLeft("启用帧率限制", FRTenabled);
                        if (nFRTenabled != FRTenabled)
                        {
                            if (!FRTenabled)
                                m_target.SrcTatgetFrameRate = Application.targetFrameRate;

                            if (nFRTenabled)
                            {
                                //强制关闭垂直同步
                                if (QualitySettings.vSyncCount > 0)
                                    QualitySettings.vSyncCount = 0;
                            }
                            else
                            {
                                Application.targetFrameRate = m_target.SrcTatgetFrameRate;
                            }
                            m_target.FrameRateLimt = nFRTenabled;
                        }
                        if (nFRTenabled)
                            Application.targetFrameRate = EditorGUILayout.IntSlider(Application.targetFrameRate, -1, 300);
                    }
                }
                GUILayout.EndVertical();
#endif
            }
            serializedObject.ApplyModifiedProperties();

        }

    }

}
