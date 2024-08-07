using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AORCore;

namespace UnityEngine.Rendering.Universal.DynamicBones.Editor
{

    [CustomEditor(typeof(DynamicBoneLookAtPramsOverride))]
    public class DynamicBoneLookAtPramsOverrideEditor : UnityEditor.Editor
    {

        private DynamicBoneLookAtData m_OverrideTarget;
        private DynamicBoneLookAtPramsOverride m_target;
        private bool m_saveDirty;

        private void Awake()
        {
            m_target = target as DynamicBoneLookAtPramsOverride;
            DynamicBoneLookAt dynamicBoneLookAt = m_target.GetComponent<DynamicBoneLookAt>();
            if (dynamicBoneLookAt)
            {
                m_OverrideTarget = (DynamicBoneLookAtData)dynamicBoneLookAt.GetNonPublicField("m_SerializeData");
            }
            else
            {
                m_OverrideTarget = null;
            }
        }

        public override void OnInspectorGUI()
        {

            if(!Application.isPlaying && m_target.enabled)
            {
                GUI.color = Color.red;
                GUILayout.BeginVertical("box");
                {
                    GUILayout.Label("<建议>:");
                    GUILayout.Label("enabled属性默认为False, 否则不符合使用规则");
                }
                GUILayout.EndVertical();
                GUI.color= Color.white;
                GUILayout.Space(5);
            }
            
            //Target
            UnityEngine.Object n = EditorGUILayout.ObjectField("Target", m_target.Target, typeof(DynamicBoneLookAt), true);
            if (m_target.Target != n)
            {
                m_target.Target = (DynamicBoneLookAt)n;
                m_saveDirty = true;
            }

            //BindingAnimStateLabel
            string nBindingAnimStateLabel = EditorGUILayout.DelayedTextField("BindingAnimStateLabel", m_target.BindingAnimStateLabel);
            if (m_target.BindingAnimStateLabel != nBindingAnimStateLabel)
            {
                m_target.BindingAnimStateLabel = nBindingAnimStateLabel;
                m_saveDirty = true;
            }

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(12);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("OverrideDatas");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                //

                DynamicBoneLookAtOverrideData data = (DynamicBoneLookAtOverrideData)m_target.GetNonPublicField("m_OverrideData");
                if (data.CheckNoneOverrideDatas())
                {
                    GUI.color = Color.red;
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("[没有设置覆盖数据]");
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;
                    GUILayout.Space(5);
                }
                //BindingNode
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideBindingNode ? "Override " : "") + "BindingNode", data.OverrideBindingNode);
                    {
                        if (data.OverrideBindingNode != nToggle)
                        {
                            data.OverrideBindingNode = nToggle;
                            if(m_OverrideTarget != null && nToggle)
                            {
                                data.BindingNode = m_OverrideTarget.BindingNode;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {
                            Transform nBindingNode = (Transform)EditorGUILayout.ObjectField("BindingNode", data.BindingNode, typeof(Transform), true);
                            if (data.BindingNode != nBindingNode)
                            {
                                data.BindingNode = nBindingNode;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.BindingNode != null)
                            {
                                data.BindingNode = null;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //LookAtNode
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideLookAtNode ? "Override " : "") + "LookAtNode", data.OverrideLookAtNode);
                    {
                        if (data.OverrideLookAtNode != nToggle)
                        {
                            data.OverrideLookAtNode = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                data.LookAtNode = m_OverrideTarget.LookAtNode;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {
                            Transform nLookAtNode = (Transform)EditorGUILayout.ObjectField("LookAtNode", data.LookAtNode, typeof(Transform), true);
                            if (data.LookAtNode != nLookAtNode)
                            {
                                data.LookAtNode = nLookAtNode;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.LookAtNode != null)
                            {
                                data.LookAtNode = null;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //AngleOffset
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideAngleOffset ? "Override " : "") + "AngleOffset", data.OverrideAngleOffset);
                    {
                        if (data.OverrideAngleOffset != nToggle)
                        {
                            data.OverrideAngleOffset = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                data.AngleOffset = m_OverrideTarget.AngleOffset;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {
                            Vector3 nAngleOffset = EditorGUILayout.Vector3Field("LookAtNode", data.AngleOffset);
                            if (data.AngleOffset != nAngleOffset)
                            {
                                data.AngleOffset = nAngleOffset;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.AngleOffset != Vector3.zero)
                            {
                                data.AngleOffset = Vector3.zero;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //AngleOffsetInterpolationSpeed
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideAngleOffsetInterpolationSpeed ? "Override " : "") + "AngleOffsetInterpolationSpeed", data.OverrideAngleOffsetInterpolationSpeed);
                    {
                        if (data.OverrideAngleOffsetInterpolationSpeed != nToggle)
                        {
                            data.OverrideAngleOffsetInterpolationSpeed = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                data.AngleOffsetInterpolationSpeed = m_OverrideTarget.AngleOffsetInterpolationSpeed;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {
                            float nAngleOffsetInterpolationSpeed = EditorGUILayout.FloatField("AngleOffsetInterpolationSpeed", data.AngleOffsetInterpolationSpeed);
                            if (data.AngleOffsetInterpolationSpeed != nAngleOffsetInterpolationSpeed)
                            {
                                data.AngleOffsetInterpolationSpeed = nAngleOffsetInterpolationSpeed;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.AngleOffsetInterpolationSpeed != 10)
                            {
                                data.AngleOffsetInterpolationSpeed = 10;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //ForwardReference
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideForwardReference ? "Override " : "") + "ForwardReference", data.OverrideForwardReference);
                    {
                        if (data.OverrideForwardReference != nToggle)
                        {
                            data.OverrideForwardReference = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                data.ForwardReference = m_OverrideTarget.ForwardReference;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {
                            Vector3 nForwardReference = EditorGUILayout.Vector3Field("ForwardReference", data.ForwardReference);
                            if (data.ForwardReference != nForwardReference)
                            {
                                data.ForwardReference = nForwardReference;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.ForwardReference != Vector3.forward)
                            {
                                data.ForwardReference = Vector3.forward;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //ParentBonesDepth
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideParentBonesDepth ? "Override " : "") + "ParentBonesDepth", data.OverrideParentBonesDepth);
                    {
                        if (data.OverrideParentBonesDepth != nToggle)
                        {
                            data.OverrideParentBonesDepth = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                data.ParentBonesDepth = m_OverrideTarget.ParentBonesDepth;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {
                            int nParentBonesDepth = EditorGUILayout.IntField("ParentBonesDepth", data.ParentBonesDepth);
                            if (data.ParentBonesDepth != nParentBonesDepth)
                            {
                                data.ParentBonesDepth = nParentBonesDepth;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.ParentBonesDepth != 0)
                            {
                                data.ParentBonesDepth = 0;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //ParentBonesWeight
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideParentBonesWeight ? "Override " : "") + "ParentBonesWeight", data.OverrideParentBonesWeight);
                    {
                        if (data.OverrideParentBonesWeight != nToggle)
                        {
                            data.OverrideParentBonesWeight = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                data.ParentBonesWeight = m_OverrideTarget.ParentBonesWeight;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {
                            float nParentBonesWeight = EditorGUILayout.FloatField("ParentBonesWeight", data.ParentBonesWeight);
                            if (data.ParentBonesWeight != nParentBonesWeight)
                            {
                                data.ParentBonesWeight = nParentBonesWeight;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.ParentBonesWeight != 0)
                            {
                                data.ParentBonesWeight = 0;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //ParentBonesWeightCurve
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideParentBonesWeightCurve ? "Override " : "") + "ParentBonesWeightCurve", data.OverrideParentBonesWeightCurve);
                    {
                        if (data.OverrideParentBonesWeightCurve != nToggle)
                        {
                            data.OverrideParentBonesWeightCurve = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                AnimationCurve copy = new AnimationCurve(m_OverrideTarget.ParentBonesWeightCurve?.keys);
                                data.ParentBonesWeightCurve = copy;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {

                            if (data.OverrideParentBonesWeight)
                            {
                                data.OverrideParentBonesWeight = false;
                                data.ParentBonesWeight = 0;
                                m_saveDirty = true;
                            }

                            AnimationCurve nParentBonesWeightCurve = EditorGUILayout.CurveField("ParentBonesWeightCurve", data.ParentBonesWeightCurve);
                            if (data.ParentBonesWeightCurve != nParentBonesWeightCurve)
                            {
                                data.ParentBonesWeightCurve = nParentBonesWeightCurve;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.ParentBonesWeightCurve != null)
                            {
                                data.ParentBonesWeightCurve = null;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //AdditionLocalEulerOffsetX, AdditionLocalEulerOffsetY, AdditionLocalEulerOffsetZ
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideAdditionLocalEulerOffset ? "Override " : "") + "AdditionLocalEulerOffset", data.OverrideAdditionLocalEulerOffset);
                    {
                        if (data.OverrideAdditionLocalEulerOffset != nToggle)
                        {
                            data.OverrideAdditionLocalEulerOffset = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                AnimationCurve copyX = new AnimationCurve(m_OverrideTarget.AdditionLocalEulerOffsetX?.keys);
                                data.AdditionLocalEulerOffsetX = copyX;
                                AnimationCurve copyY = new AnimationCurve(m_OverrideTarget.AdditionLocalEulerOffsetY?.keys);
                                data.AdditionLocalEulerOffsetX = copyY;
                                AnimationCurve copyZ = new AnimationCurve(m_OverrideTarget.AdditionLocalEulerOffsetZ?.keys);
                                data.AdditionLocalEulerOffsetX = copyZ;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {

                            AnimationCurve nAdditionLocalEulerOffsetX = EditorGUILayout.CurveField("AdditionLocalEulerOffsetX", data.AdditionLocalEulerOffsetX);
                            if (data.AdditionLocalEulerOffsetX != nAdditionLocalEulerOffsetX)
                            {
                                data.AdditionLocalEulerOffsetX = nAdditionLocalEulerOffsetX;
                                m_saveDirty = true;
                            }
                            AnimationCurve nAdditionLocalEulerOffsetY = EditorGUILayout.CurveField("AdditionLocalEulerOffsetY", data.AdditionLocalEulerOffsetY);
                            if (data.AdditionLocalEulerOffsetY != nAdditionLocalEulerOffsetY)
                            {
                                data.AdditionLocalEulerOffsetY = nAdditionLocalEulerOffsetY;
                                m_saveDirty = true;
                            }
                            AnimationCurve nAdditionLocalEulerOffsetZ = EditorGUILayout.CurveField("AdditionLocalEulerOffsetX", data.AdditionLocalEulerOffsetZ);
                            if (data.AdditionLocalEulerOffsetZ != nAdditionLocalEulerOffsetZ)
                            {
                                data.AdditionLocalEulerOffsetZ = nAdditionLocalEulerOffsetZ;
                                m_saveDirty = true;
                            }

                        }
                        else
                        {
                            if (data.ParentBonesWeightCurve != null)
                            {
                                data.ParentBonesWeightCurve = null;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //EffectWeight
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nToggle = EditorGUILayout.BeginToggleGroup((data.OverrideEffectWeight ? "Override " : "") + "EffectWeight", data.OverrideEffectWeight);
                    {
                        if (data.OverrideEffectWeight != nToggle)
                        {
                            data.OverrideEffectWeight = nToggle;
                            if (m_OverrideTarget != null && nToggle)
                            {
                                data.EffectWeight = m_OverrideTarget.EffectWeight;
                            }
                            m_saveDirty = true;
                        }
                        if (nToggle)
                        {
                            float nEffectWeight = EditorGUILayout.FloatField("EffectWeight", data.EffectWeight);
                            if (data.EffectWeight != nEffectWeight)
                            {
                                data.EffectWeight = nEffectWeight;
                                m_saveDirty = true;
                            }
                        }
                        else
                        {
                            if (data.EffectWeight != 0)
                            {
                                data.EffectWeight = 0;
                                m_saveDirty = true;
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.EndVertical();

                //UseEffectWeightFadeInOut
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    bool nUseEffectWeightFadeInOut = EditorGUILayout.ToggleLeft("UseEffectWeightFadeInOut", data.UseEffectWeightFadeInOut);
                    if(data.UseEffectWeightFadeInOut != nUseEffectWeightFadeInOut)
                    {
                        data.UseEffectWeightFadeInOut = nUseEffectWeightFadeInOut;
                        m_saveDirty = true;
                    }
                }
                GUILayout.EndVertical();
                //
                GUILayout.Space(12);
            }
            GUILayout.EndVertical();

            if (m_saveDirty)
            {
                EditorUtility.SetDirty(m_target);
                m_saveDirty = false;
            }

        }


    }
}