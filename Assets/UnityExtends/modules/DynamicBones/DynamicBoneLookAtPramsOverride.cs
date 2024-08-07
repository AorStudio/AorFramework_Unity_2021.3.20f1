using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.DynamicBones
{

    [Serializable]
    public class DynamicBoneLookAtOverrideData
    {
        public bool OverrideBindingNode;
        public Transform BindingNode;    //绑定骨骼节点
        public bool OverrideLookAtNode;
        public Transform LookAtNode;    //LookAt目标
        public bool OverrideAngleOffset;
        public Vector3 AngleOffset = Vector3.zero;     //旋转偏移角度
        public bool OverrideAngleOffsetInterpolationSpeed;
        public float AngleOffsetInterpolationSpeed = 10f;   //AngleOffset改变后的插值速度
        public bool OverrideForwardReference;
        public Vector3 ForwardReference = new Vector3(0, 0, 1); //表示当前骨骼节点的前方向落在哪根轴上?
        public bool OverrideParentBonesDepth;
        public int ParentBonesDepth;        //向上影响骨骼深度
        public bool OverrideParentBonesWeight;
        public float ParentBonesWeight;       //向上影响骨骼权重

        public bool OverrideParentBonesWeightCurve;
        public AnimationCurve ParentBonesWeightCurve;   //向上影响骨骼权重曲线

        public bool OverrideAdditionLocalEulerOffset;
        public AnimationCurve AdditionLocalEulerOffsetX;
        public AnimationCurve AdditionLocalEulerOffsetY;
        public AnimationCurve AdditionLocalEulerOffsetZ;

        public bool OverrideEffectWeight;
        [Range(0, 1.0f)]
        public float EffectWeight = 1.0f;

        public bool UseEffectWeightFadeInOut;

        public void ApplyTo(DynamicBoneLookAtData data)
        {
            if (OverrideBindingNode)
                data.BindingNode = BindingNode;
            if (OverrideLookAtNode)
                data.LookAtNode = LookAtNode;
            if (OverrideAngleOffset)
                data.AngleOffset = AngleOffset;
            if (OverrideAngleOffsetInterpolationSpeed)
                data.AngleOffsetInterpolationSpeed = AngleOffsetInterpolationSpeed;
            if (OverrideForwardReference)
                data.ForwardReference = ForwardReference;
            if (OverrideParentBonesDepth)
                data.ParentBonesDepth = ParentBonesDepth;
            if (OverrideParentBonesWeight)
                data.ParentBonesWeight = ParentBonesWeight;

            if (OverrideParentBonesWeightCurve)
            {
                data.ParentBonesWeightUseCurve = true;
                data.ParentBonesWeightCurve = ParentBonesWeightCurve;
            }

            if (OverrideAdditionLocalEulerOffset)
            {
                data.UseAdditionLocalEulerOffset = true;
                data.AdditionLocalEulerOffsetX = AdditionLocalEulerOffsetX;
                data.AdditionLocalEulerOffsetY = AdditionLocalEulerOffsetY;
                data.AdditionLocalEulerOffsetZ = AdditionLocalEulerOffsetZ;
            }
        }

        public bool CheckNoneOverrideDatas()
        {
            return !OverrideBindingNode
                && !OverrideLookAtNode
                && !OverrideAngleOffset
                && !OverrideAngleOffsetInterpolationSpeed
                && !OverrideForwardReference
                && !OverrideParentBonesDepth
                && !OverrideParentBonesWeight
                && !OverrideParentBonesWeightCurve
                && !OverrideAdditionLocalEulerOffset
                && !OverrideEffectWeight
                && !UseEffectWeightFadeInOut;
        }
    }

    [DefaultExecutionOrder(-55)]
    public class DynamicBoneLookAtPramsOverride : MonoBehaviour
    {
        public DynamicBoneLookAt Target;
        public string BindingAnimStateLabel;

        [SerializeField]
        private DynamicBoneLookAtOverrideData m_OverrideData;

        private void Awake()
        {
            if (!Target)
                Target = GetComponent<DynamicBoneLookAt>();
        }

        private void OnEnable()
        {
            if (m_isStarted)
                init();
        }

        private void Start()
        {
            init();
            m_isStarted = true;
        }

        private void OnDisable()
        {
            if (!Target) return;
            Target.UnsetOverrideData(m_OverrideData);
        }

        private bool m_isStarted;
        private void init()
        {
            if (!Target) return;
            Target.SetOverrideData(m_OverrideData);
        }


    }

}
