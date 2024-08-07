using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.DynamicBones
{

    /// <summary>
    /// DynamicBoneCollider收集器,为DynamicBoneColliderSetter提供DynamicBoneCollider数据支持
    /// 
    /// Author : Aorition
    /// Update : 2022_12_01
    /// 
    /// </summary>
    public class DynamicBoneColliderCollector : MonoBehaviour, ISerializationCallbackReceiver
    {

        [SerializeField] private string[] m_keys;
        [SerializeField] private DynamicBoneCollider[] m_colliders;

        private readonly Dictionary<string, DynamicBoneCollider> m_collidersDic = new Dictionary<string, DynamicBoneCollider>();
        public Dictionary<string, DynamicBoneCollider> CollidersDic
        {
            get { return m_collidersDic; }
        }

        #region ISerializationCallbackReceiver 实现

        public void OnAfterDeserialize()
        {
            m_collidersDic.Clear();
            for (int i = 0; i < m_keys.Length; i++)
            {
                m_collidersDic.Add(m_keys[i], m_colliders[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            if(m_collidersDic.Count > 0)
            {
                int len = m_collidersDic.Count;
                string[] keys = new string[len];
                DynamicBoneCollider[] colliders = new DynamicBoneCollider[len];
                int i = 0;
                foreach (var kv in m_collidersDic)
                {
                    keys[i] = kv.Key;
                    colliders[i] = kv.Value;
                    i++;
                }
                m_keys = keys;
                m_colliders = colliders;
            }
        }

        #endregion

        protected bool m_isStarted;
        protected bool m_isProcessed;
        protected readonly HashSet<DynamicBoneColliderLinker> m_runtimeLinkers = new HashSet<DynamicBoneColliderLinker>();

        protected void Start()
        {
            DoProcess();
            m_isStarted = true;
        }

        protected void OnEnable()
        {
            if(m_isStarted)
                DoProcess();
        }

        protected void OnDisable()
        {
            m_isProcessed = false;
        }

        protected void OnDestroy()
        {
            m_runtimeLinkers.Clear();
        }

        public void Register(DynamicBoneColliderLinker linker)
        {
            if(!m_runtimeLinkers.Contains(linker))
                m_runtimeLinkers.Add(linker);

            if (m_isProcessed)
            {
                SetCollidersToLinker(linker);
            }
        }

        public void Unregister(DynamicBoneColliderLinker linker)
        {
            if (m_runtimeLinkers.Contains(linker))
                m_runtimeLinkers.Remove(linker);
        }

        public void DoProcess()
        {
            foreach (var linker in m_runtimeLinkers)
            {
                SetCollidersToLinker(linker);
            }
            m_isProcessed = true;
        }

        protected void SetCollidersToLinker(DynamicBoneColliderLinker linker)
        {
            var colliderList = new List<DynamicBoneCollider>();
            for (int j = 0; j < linker.ColliderKeys.Length; j++)
            {
                string key = linker.ColliderKeys[j];
                if (!string.IsNullOrEmpty(key) && m_collidersDic.ContainsKey(key))
                {
                    colliderList.Add(m_collidersDic[key]);
                }
            }

            if (colliderList.Count > 0)
            {
                linker.SetColliders(colliderList);
            }
        }

    }

}


