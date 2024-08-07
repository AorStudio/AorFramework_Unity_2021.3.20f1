using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.DynamicBones 
{

    /// <summary>
    /// 根据记录的key，运行时为DynamicBoneCollider设置DynamicBoneCollider (须配合DynamicBoneColliderCollector一起使用)
    /// 
    /// Author : Aorition
    /// Update : 2022_12_01
    /// </summary>
    [RequireComponent(typeof(DynamicBone))]
    public class DynamicBoneColliderLinker : MonoBehaviour
    {
        [Tooltip("指定相关关联的DynamicBone.m_Root")]
        public Transform BoneRoot;

        [Tooltip("DynamicBoneColliderCollector中的key值")]
        public string[] ColliderKeys;

        protected DynamicBone m_dynamicBone;
        protected DynamicBoneColliderCollector m_master;
        protected void Awake()
        {

            if (!BoneRoot)
                BoneRoot = transform;

            DynamicBone[] dynamicBones = GetComponents<DynamicBone>();
            foreach (var db in dynamicBones)
            {
                if(db.m_Root == BoneRoot)
                {
                    m_dynamicBone = db;
                    break;
                }
            }
        }

        protected void Start()
        {
            m_master = GetComponentInParent<DynamicBoneColliderCollector>();
            if (m_master)
            {
                m_master.Register(this);
            }
            else
            {
                //没有找到DynamicBoneColliderCollector
            }
        }

        protected void OnDestroy()
        {
            m_dynamicBone = null;
            if (m_master)
            {
                m_master.Unregister(this);
                m_master = null;
            }
        }

        public void SetColliders(List<DynamicBoneCollider> colliderList)
        {
            m_dynamicBone.m_Colliders = colliderList;
        }

        public void ClearColliders()
        {
            m_dynamicBone.m_Colliders = null;
        }

    }

}



