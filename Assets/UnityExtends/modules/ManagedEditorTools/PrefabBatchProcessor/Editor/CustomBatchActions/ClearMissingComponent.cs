using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if FRAMEWORKDEF
using Framework.Extends;
#else
using AORCore;
#endif

namespace UnityEngine.Rendering.Universal.Utility.Editor
{

    /// <summary>
    /// Author : Aorition
    /// Update : 2023-11-21
    /// </summary>
    [PBPTagLabel("批量移除MissingComponent")]
    public class ClearMissingComponent : PrefabBatchActionBase
    {

        public override string DesInfo
        {
            get {
                return "批量移除MissingComponent";
            }
        }

        protected override void _foreachTransformProcess(Transform transform, int indent, ref bool dirty)
        {

#if UNITY_2019_3_OR_NEWER

            int r = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
            if(r > 0)
            {
                _infoAppendLine(indent, "Node>" + transform.GetHierarchyPath() + " ::  finded MissingComponents -> " + r);
                _trySetDirty(ref dirty);
            }
#else
            //老版本Unity 实现
            var components = transform.GetComponents<Component>();
            if (components != null && components.Length > 0)
            {
                var serializedObject = new SerializedObject(transform.gameObject);
                var prop = serializedObject.FindProperty("m_Component");
                int r = 0;
                for (int j = 0; j < components.Length; j++)
                {
                    if (components[j] == null)
                    {
                        prop.DeleteArrayElementAtIndex(j - r);
                        r++;
                    }
                }

                if (r > 0)
                {
                    serializedObject.ApplyModifiedProperties();
                    _infoAppendLine(indent, "Node>" + transform.GetHierarchyPath() + " ::  finded MissingComponents -> " + r);
                    _trySetDirty(ref dirty);
                }
            }
#endif
        }

    }

}


