using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.Rendering.Universal.DynamicBones.Editor
{

    [CustomEditor(typeof(DynamicBoneRootReplace))]
    public class DynamicBoneRootReplaceInspector : UnityEditor.Editor
    {

        private DynamicBoneRootReplace dynamicBoneRootReplace;
        private DynamicBoneRootReplace targetFBX;

        private DynamicBone sourceDynamicBone;
        private DynamicBone[] allsourceDynamicBone;

        public override void OnInspectorGUI()
        {
            dynamicBoneRootReplace = (DynamicBoneRootReplace)target;
            targetFBX = (DynamicBoneRootReplace)target;
            GUILayout.Space(10);
            EditorGUILayout.LabelField("                     ------动态骨骼引用替换工具------");

            GUILayout.Space(10);
            EditorGUILayout.LabelField("动态骨骼放这里，动态骨骼父节点需要有动态骨骼脚本");
            dynamicBoneRootReplace.DynamicBoneDate = EditorGUILayout.ObjectField(dynamicBoneRootReplace.DynamicBoneDate, typeof(GameObject), true, GUILayout.MaxWidth(200), GUILayout.MaxHeight(20)) as GameObject;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("新FBX模型放这里");
            dynamicBoneRootReplace.targetFBX = EditorGUILayout.ObjectField(dynamicBoneRootReplace.targetFBX, typeof(GameObject), true, GUILayout.MaxWidth(200), GUILayout.MaxHeight(20)) as GameObject;

            GUILayout.Space(10);
            if (GUILayout.Button("Replace"))
            {
                SetdynamicBoneRootName();
            }
        }
        public void GetdynamicBoneDate()
        {
            sourceDynamicBone = dynamicBoneRootReplace.DynamicBoneDate.GetComponent<DynamicBone>();
        }

        public void SetdynamicBoneRootName()
        {
            allsourceDynamicBone = dynamicBoneRootReplace.DynamicBoneDate.GetComponentsInChildren<DynamicBone>();
            foreach (DynamicBone dynamicboneRoot in allsourceDynamicBone)
            {
                //Debug.Log(dynamicboneRoot.m_Root);
                //GettargetFBXName();
                Transform[] targetFBXallName = dynamicBoneRootReplace.targetFBX.GetComponentsInChildren<Transform>();
                Debug.Log("1");
                for (int i = 0; i < targetFBXallName.Length; i++)
                {
                    Debug.Log("2");
                    if (dynamicboneRoot.m_Root.name == targetFBXallName[i].name)
                    {
                        Debug.Log("有相同的");
                        dynamicboneRoot.m_Root = targetFBXallName[i].transform;
                        Debug.Log(dynamicboneRoot.m_Root.name);
                    }
                }
            }
        }

        public void GetdynamicBoneRootName()
        {
            allsourceDynamicBone = dynamicBoneRootReplace.DynamicBoneDate.GetComponentsInChildren<DynamicBone>();
            foreach (DynamicBone dynamicboneRoot in allsourceDynamicBone)
            {
                Debug.Log(dynamicboneRoot.m_Root);
            }
        }


        public void GettargetFBXName()
        {
            Transform[] allName = dynamicBoneRootReplace.targetFBX.GetComponentsInChildren<Transform>();
            foreach (Transform allname in allName)
            {
                Debug.Log(allname.name);
            }
        }

    }

}
