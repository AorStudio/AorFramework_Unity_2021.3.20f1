using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.DynamicBones
{

    [Serializable]
    public class DynamicBoneLookAtData
    {
        public Transform BindingNode;    //绑定骨骼节点
        public Transform LookAtNode;    //LookAt目标
        public Vector3 AngleOffset = Vector3.zero;     //旋转偏移角度
        public float AngleOffsetInterpolationSpeed = 10f;   //AngleOffset改变后的插值速度
        public Vector3 ForwardReference = new Vector3(0, 0, 1); //表示当前骨骼节点的前方向落在哪根轴上?
        public int ParentBonesDepth;        //向上影响骨骼深度
        public float ParentBonesWeight = 0.5f;       //向上影响骨骼权重
        public bool ParentBonesWeightUseCurve;      //是否使用曲线控制向上影响骨骼权重
        public AnimationCurve ParentBonesWeightCurve;   //向上影响骨骼权重曲线

        public bool UseAdditionLocalEulerOffset;
        public AnimationCurve AdditionLocalEulerOffsetX;
        public AnimationCurve AdditionLocalEulerOffsetY;
        public AnimationCurve AdditionLocalEulerOffsetZ;
        [Range(0, 1.0f)]
        public float EffectWeight = 1.0f;

        public void CopyForm(DynamicBoneLookAtData src)
        {
            BindingNode = src.BindingNode;
            LookAtNode = src.LookAtNode;
            AngleOffset = src.AngleOffset;
            AngleOffsetInterpolationSpeed = src.AngleOffsetInterpolationSpeed;
            ForwardReference = src.ForwardReference;
            ParentBonesDepth = src.ParentBonesDepth;
            ParentBonesWeight = src.ParentBonesWeight;
            ParentBonesWeightUseCurve = src.ParentBonesWeightUseCurve;
            ParentBonesWeightCurve = src.ParentBonesWeightCurve;

            UseAdditionLocalEulerOffset = src.UseAdditionLocalEulerOffset;
            AdditionLocalEulerOffsetX = src.AdditionLocalEulerOffsetX;
            AdditionLocalEulerOffsetY = src.AdditionLocalEulerOffsetY;
            AdditionLocalEulerOffsetZ = src.AdditionLocalEulerOffsetZ;
        }

        public DynamicBoneLookAtData Clone()
        {
            DynamicBoneLookAtData n = new DynamicBoneLookAtData();
            n.BindingNode = BindingNode;
            n.LookAtNode = LookAtNode;
            n.AngleOffset = AngleOffset;
            n.AngleOffsetInterpolationSpeed = AngleOffsetInterpolationSpeed;
            n.ForwardReference = ForwardReference;
            n.ParentBonesDepth = ParentBonesDepth;
            n.ParentBonesWeight = ParentBonesWeight;
            n.ParentBonesWeightUseCurve = ParentBonesWeightUseCurve;
            n.ParentBonesWeightCurve = ParentBonesWeightCurve;
            n.UseAdditionLocalEulerOffset = UseAdditionLocalEulerOffset;
            n.AdditionLocalEulerOffsetX = AdditionLocalEulerOffsetX;
            n.AdditionLocalEulerOffsetY = AdditionLocalEulerOffsetY;
            n.AdditionLocalEulerOffsetZ = AdditionLocalEulerOffsetZ;
            return n;
        }

#if UNITY_EDITOR

        public void SyncBaseData(DynamicBoneLookAtData src)
        {
            AngleOffset = src.AngleOffset;
            ForwardReference = src.ForwardReference;
            AngleOffsetInterpolationSpeed = src.AngleOffsetInterpolationSpeed;
            EffectWeight = src.EffectWeight;
            UseAdditionLocalEulerOffset = src.UseAdditionLocalEulerOffset;
        }

        public void SyncBaseData(DynamicBoneLookAtOverrideData src)
        {
            if (src.OverrideAngleOffset)
                AngleOffset = src.AngleOffset;
            if (src.OverrideForwardReference)
                ForwardReference = src.ForwardReference;
            if (src.OverrideAngleOffsetInterpolationSpeed)
                AngleOffsetInterpolationSpeed = src.AngleOffsetInterpolationSpeed;
            if (src.OverrideEffectWeight)
                EffectWeight = src.EffectWeight;
            if (src.OverrideAdditionLocalEulerOffset)
                UseAdditionLocalEulerOffset = src.OverrideAdditionLocalEulerOffset;
        }

        public bool CheckDirty(DynamicBoneLookAtData src)
        {
            bool dirty = false;
            if (BindingNode != src.BindingNode)
            {
                BindingNode = src.BindingNode;
                dirty = true;
            }
            if (LookAtNode != src.LookAtNode)
            {
                LookAtNode = src.LookAtNode;
                dirty = true;
            }
            if (ParentBonesDepth != src.ParentBonesDepth)
            {
                ParentBonesDepth = src.ParentBonesDepth;
                dirty = true;
            }
            if (ParentBonesWeightUseCurve != src.ParentBonesWeightUseCurve)
            {
                ParentBonesWeightUseCurve = src.ParentBonesWeightUseCurve;
                dirty = true;
            }
            if (!ParentBonesWeightUseCurve && ParentBonesWeight != src.ParentBonesWeight)
            {
                ParentBonesWeight = src.ParentBonesWeight;
                dirty = true;
            }
            return dirty;
        }

        public bool CheckDirty(DynamicBoneLookAtOverrideData src)
        {
            bool dirty = false;
            if (src.OverrideBindingNode && BindingNode != src.BindingNode)
            {
                BindingNode = src.BindingNode;
                dirty = true;
            }
            if (src.OverrideLookAtNode && LookAtNode != src.LookAtNode)
            {
                LookAtNode = src.LookAtNode;
                dirty = true;
            }
            if (src.OverrideParentBonesDepth && ParentBonesDepth != src.ParentBonesDepth)
            {
                ParentBonesDepth = src.ParentBonesDepth;
                dirty = true;
            }
            if (src.OverrideParentBonesWeightCurve && ParentBonesWeightUseCurve != src.OverrideParentBonesWeightCurve)
            {
                ParentBonesWeightUseCurve = src.OverrideParentBonesWeightCurve;
                dirty = true;
            }
            if (src.OverrideParentBonesWeight && !ParentBonesWeightUseCurve && ParentBonesWeight != src.ParentBonesWeight)
            {
                ParentBonesWeight = src.ParentBonesWeight;
                dirty = true;
            }
            return dirty;
        }

#endif

    }

    [DefaultExecutionOrder(-50)]
    public class DynamicBoneLookAt : MonoBehaviour
    {
        [SerializeField]
        private DynamicBoneLookAtData m_SerializeData;
        private DynamicBoneLookAtData m_Data;

        public float EffectWeightFadeSpeed = 10f;

        public Transform BindingNode
        {
            get { return m_Data.BindingNode; }
            set
            {
                if (m_Data.BindingNode != value)
                {
                    m_Data.BindingNode = value;
                    m_dataDirty = true;
                }
            }
        }

        public Transform LookAtNode
        {
            get { return m_Data.LookAtNode; }
            set
            {
                if (m_Data.LookAtNode != value)
                {
                    m_Data.LookAtNode = value;
                    m_dataDirty = true;
                }
            }
        }

        public Vector3 AngleOffset
        {
            get
            {
                return m_Data.AngleOffset;
            }
            set
            {
                m_Data.AngleOffset = value;
            }
        }

        public float AngleOffsetInterpolationSpeed
        {
            get { return m_Data.AngleOffsetInterpolationSpeed; }
            set
            {
                m_Data.AngleOffsetInterpolationSpeed = value;
            }
        }

        public Vector3 ForwardReference
        {
            get { return m_Data.ForwardReference; }
            set { m_Data.ForwardReference = value; }
        }

        public int ParentBonesDepth
        {
            get { return m_Data.ParentBonesDepth; }
            set
            {
                if (m_Data.ParentBonesDepth != value)
                {
                    m_Data.ParentBonesDepth = value;
                    m_dataDirty = true;
                }
            }
        }

        public float ParentBonesWeight
        {
            get { return m_Data.ParentBonesWeight; }
            set
            {
                if (m_Data.ParentBonesWeight != value)
                {
                    m_Data.ParentBonesWeight = value;
                    if (!m_Data.ParentBonesWeightUseCurve)
                        m_dataDirty = true;
                }
            }
        }

        public bool ParentBonesWeightUseCurve
        {
            get { return m_Data.ParentBonesWeightUseCurve; }
            set
            {
                if (m_Data.ParentBonesWeightUseCurve != value)
                {
                    m_Data.ParentBonesWeightUseCurve = value;
                    m_dataDirty = true;
                }
            }
        }

        public AnimationCurve ParentBonesWeightCurve
        {
            get { return m_Data.ParentBonesWeightCurve; }
            set
            {
                if (m_Data.ParentBonesWeightCurve != value)
                {
                    m_Data.ParentBonesWeightCurve = value;
#if UNITY_EDITOR
#else
                m_dataDirty = true;
#endif
                }
            }
        }

        public bool UseAdditionLocalEulerOffset
        {
            get { return m_Data.UseAdditionLocalEulerOffset; }
            set { m_Data.UseAdditionLocalEulerOffset = value; }
        }

        public AnimationCurve AdditionLocalEulerOffsetX { get { return m_Data.AdditionLocalEulerOffsetX; } set { m_Data.AdditionLocalEulerOffsetX = value; } }
        public AnimationCurve AdditionLocalEulerOffsetY { get { return m_Data.AdditionLocalEulerOffsetY; } set { m_Data.AdditionLocalEulerOffsetY = value; } }
        public AnimationCurve AdditionLocalEulerOffsetZ { get { return m_Data.AdditionLocalEulerOffsetZ; } set { m_Data.AdditionLocalEulerOffsetZ = value; } }

        public float EffectWeight
        {
            get {
                //特殊处理淡出时的调用值
                if (m_effectWeight_fadeOut)
                    return m_effectWeight_srcCache;
                return m_Data.EffectWeight; 
            }
            set { m_Data.EffectWeight = value; }
        }

        public void SetDataDirty()
        {
            m_dataDirty = true;
        }

        private bool m_UseOverrideData;
        public bool UseOverrideData
        {
            get { return m_UseOverrideData; }
        }

        private readonly List<DynamicBoneLookAtOverrideData> m_OverrideDatas = new List<DynamicBoneLookAtOverrideData>();

        public int GetOverrideDatasCount()
        {
            return m_OverrideDatas.Count;
        }

        public DynamicBoneLookAtOverrideData GetCurrentOverrideData()
        {
            if (m_OverrideDatas.Count > 0)
                return m_OverrideDatas[m_OverrideDatas.Count - 1];
            return null;
        }

        public void SetOverrideData(DynamicBoneLookAtOverrideData overrideData)
        {
            if (!m_OverrideDatas.Contains(overrideData))
            {
                m_OverrideDatas.Add(overrideData);
                m_isOverrideDataDitry = true;
            }
        }

        public void UnsetOverrideData(DynamicBoneLookAtOverrideData overrideData)
        {
            if (m_OverrideDatas.Contains(overrideData))
            {
                m_OverrideDatas.Remove(overrideData);
                m_isOverrideDataDitry = true;
            }
        }

        public void UnsetOverrideDataAt(int index)
        {
            if (index >= 0 && index < m_OverrideDatas.Count)
            {
                m_OverrideDatas.RemoveAt(index);
                m_isOverrideDataDitry = true;
            }
        }

        public void ClearOverrideDatas()
        {
            if (m_OverrideDatas.Count > 0)
            {
                m_OverrideDatas.Clear();
                m_isOverrideDataDitry = true;
            }
        }

        private readonly List<Transform> m_pBones = new List<Transform>();
        private readonly List<float> m_cSamples = new List<float>();
        private readonly List<float> m_pWeights = new List<float>();

        private bool m_dataDirty;
        private bool m_isOverrideDataDitry;

        private Vector3 m_AngleOffset_current;

        private bool m_use_effectWeightFadeInOut;
        private bool m_effectWeight_fadeIn;
        private bool m_effectWeight_fadeOut;
        private float m_effectWeight_srcCache;

        private float m_effectWeight_FadeValue = 1;


        private void Awake()
        {
            if (m_Data == null)
            {
                if (m_SerializeData == null)
                    m_Data = new DynamicBoneLookAtData();
                else
                    m_Data = m_SerializeData.Clone();

                m_AngleOffset_current = AngleOffset;
            }
            else
                m_Data.CopyForm(m_SerializeData);
        }

        private void OnEnable()
        {
            if (m_isStarted)
                ResetDatas();
        }

        private bool m_isStarted;
        private void Start()
        {
            ResetDatas();
            m_isStarted = true;
        }

        private void ResetDatas()
        {
            if (m_isOverrideDataDitry)
            {
                DynamicBoneLookAtOverrideData overrideData = GetCurrentOverrideData();
                if (overrideData != null)
                {
                    if (overrideData.UseEffectWeightFadeInOut)
                    {
                        m_effectWeight_FadeValue = 0;
                        m_effectWeight_fadeIn = true;
                        m_use_effectWeightFadeInOut = true;
                    }
                    else
                    {
                        m_effectWeight_FadeValue = 1;
                        m_use_effectWeightFadeInOut = false;
                    }
                    overrideData.ApplyTo(m_Data);
                    m_UseOverrideData = true;
                }
                else
                {

                    if (m_use_effectWeightFadeInOut)
                    {
                        m_effectWeight_srcCache = m_Data.EffectWeight;
                        m_effectWeight_FadeValue = 1;
                        m_effectWeight_fadeOut = true;
                        m_use_effectWeightFadeInOut = false;
                    }
                    m_Data.CopyForm(m_SerializeData);
                    m_UseOverrideData = false;
                }
                m_isOverrideDataDitry = false;
            }

            if (!m_Data.LookAtNode)
            {
                Debug.LogError($"*** DynamicBoneLookAt.Error :: 必要数据缺失，导致脚本运行失败。[LookAtNode为空]");
                m_Data = null;
                enabled = false;
                return;
            }

            m_pBones.Clear();
            m_cSamples.Clear();
            m_pWeights.Clear();

            if (!BindingNode)
                BindingNode = transform;

            m_pBones.Add(BindingNode);
            m_cSamples.Add(0);

            if (ParentBonesWeightUseCurve)
                m_pWeights.Add(ParentBonesWeightCurve.Evaluate(0));
            else
                m_pWeights.Add(1.0f);

            if (ParentBonesDepth > 0)
            {
                float r = ParentBonesWeight;
                int d = ParentBonesDepth;
                float inv = 1.0f / (d + 1);
                float w = 0;
                float s = 0;
                Transform pBone = BindingNode;
                for (int i = 0; i < ParentBonesDepth; i++)
                {
                    pBone = pBone.parent;
                    if (!pBone) return;

                    s += inv;
                    w = ParentBonesWeightUseCurve ? ParentBonesWeightCurve.Evaluate(s) : w * r;
                    m_pBones.Add(pBone);
                    m_cSamples.Add(s);
                    m_pWeights.Add(w);
                }
            }
        }

#if UNITY_EDITOR && !RUNTIME

        public bool DevDrawGizmos;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DevDrawGizmos || !m_Data.BindingNode || !m_Data.LookAtNode) return;

            if (m_pBones.Count > 0)
            {
                for (int i = 0; i < m_pBones.Count; i++)
                {
                    Transform pBone = m_pBones[i];
                    if (!pBone) return;
                    drawBoneGizmo(pBone);
                    pBone = pBone.parent;
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.yellow;
            Vector3 dir = (m_Data.LookAtNode.position - m_Data.BindingNode.position).normalized;
            Gizmos.DrawLine(m_Data.BindingNode.position + dir * 0.5f, m_Data.LookAtNode.position);
        }

        private void drawBoneGizmo(Transform bone)
        {
            Gizmos.matrix = bone.localToWorldMatrix;
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.05f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 0.25f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, Vector3.up * 0.25f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, Vector3.right * 0.25f);
        }

#endif


        private void Update()
        {
            if (m_effectWeight_fadeIn)
            {
                m_effectWeight_FadeValue += Time.smoothDeltaTime * EffectWeightFadeSpeed;
                if (m_effectWeight_FadeValue >= 1.0f)
                {
                    m_effectWeight_FadeValue = 1.0f;
                    m_effectWeight_fadeIn = false;
                }
            }

            if (m_effectWeight_fadeOut)
            {
                m_effectWeight_FadeValue -= Time.smoothDeltaTime * EffectWeightFadeSpeed;
                if (m_effectWeight_FadeValue <= 0)
                {
                    m_effectWeight_FadeValue = 0;
                    m_effectWeight_fadeOut = false;
                }
            }

        }

        private void LateUpdate()
        {

#if UNITY_EDITOR && !RUNTIME
            if (m_UseOverrideData)
            {
                DynamicBoneLookAtOverrideData overrideData = GetCurrentOverrideData();
                if (overrideData != null)
                {
                    m_Data.SyncBaseData(overrideData);
                    m_dataDirty = m_Data.CheckDirty(overrideData);
                }
                //else
                //{
                //    m_Data.SyncBaseData(m_SerializeData);
                //    m_dataDirty = m_Data.CheckDirty(m_SerializeData);
                //}
            }
            else
            {
                m_Data.SyncBaseData(m_SerializeData);
                m_dataDirty = m_Data.CheckDirty(m_SerializeData);
            }
#endif

            if (m_dataDirty || m_isOverrideDataDitry)
            {
                ResetDatas();
                m_dataDirty = false;
            }

            if (m_Data == null) return;

            if (!m_AngleOffset_current.Equals(AngleOffset))
            {
                m_AngleOffset_current = Vector3.Slerp(m_AngleOffset_current, AngleOffset, AngleOffsetInterpolationSpeed * Time.smoothDeltaTime);
            }

            Vector3 dir = (LookAtNode.position - BindingNode.position).normalized;
            Quaternion q = Quaternion.Lerp(BindingNode.rotation, Quaternion.LookRotation(dir, ForwardReference) * Quaternion.Euler(m_AngleOffset_current), EffectWeight * m_effectWeight_FadeValue);
            Transform pBone;
            for (int i = m_pBones.Count - 1; i >= 0; i--)
            {
                pBone = m_pBones[i];

                if (UseAdditionLocalEulerOffset)
                {
                    q *= Quaternion.Euler(AdditionLocalEulerOffsetX.Evaluate(m_cSamples[i]), AdditionLocalEulerOffsetY.Evaluate(m_cSamples[i]), AdditionLocalEulerOffsetZ.Evaluate(m_cSamples[i]));
                }

#if UNITY_EDITOR && !RUNTIME
                pBone.rotation = Quaternion.Lerp(pBone.rotation, q, ParentBonesWeightUseCurve ? ParentBonesWeightCurve.Evaluate(m_cSamples[i]) : m_pWeights[i]);
#else
            pBone.rotation = Quaternion.Lerp(pBone.rotation, q, m_pWeights[i]);
#endif
            }

        }


    }

}
