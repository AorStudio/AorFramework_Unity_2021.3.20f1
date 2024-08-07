using System;
using System.Collections.Generic;
using System.Reflection;
#if FRAMEWORKDEF
using AorBaseUtility.Extends;
using Framework.Editor;
#else
using NodeGraphLibs;
using NodeGraphLibs.Editor;
#endif
using UnityEditor;
using UnityEngine;

namespace NodeGraph.Editor
{
    public class AssetPrefabProcessorController : NodeController
    {

        private Type _customScriptType;
        private object _customScript;
        private MethodInfo _customScriptMethodInfo;
        private MethodInfo _customScriptResetMethodInfo;
        private FieldInfo[] _customScriptFieldInfos; 

        private bool _getCustomScript(string GUID)
        {
            UnityEngine.Object cso = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(GUID));
            if (cso != null)
            {
                MonoScript ms = cso as MonoScript;
                _customScriptType = ms.GetClass();

                //检查 自定义脚本 是否是 IPrefabProcess
                if(_customScriptType.GetInterface("IPrefabProcess") != null)
                {
                    _customScript = _customScriptType.Assembly.CreateInstance(_customScriptType.FullName);
                    _customScriptMethodInfo = _customScriptType.GetMethod("PrefabProcess", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
                    _customScriptResetMethodInfo = _customScriptType.GetMethod("Reset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);

                    //获取设置字段
                    _customScriptFieldInfos = _customScriptType.GetFields(BindingFlags.Instance| BindingFlags.Public| BindingFlags.GetField);

                    //获取CustomScriptDescribeAttribute
                    Attribute[] attributes = Attribute.GetCustomAttributes(_customScriptMethodInfo);
                    if (attributes != null && attributes.Length > 0)
                    {

                        int i, len = attributes.Length;
                        for (i = 0; i < len; i++)
                        {
                            if (attributes[i].GetType() == typeof(CustomScriptDescribeAttribute))
                            {
                                string des = (string)attributes[i].GetPublicField("Describe");
                                if (!string.IsNullOrEmpty(des))
                                {
                                    m_nodeGUI.data.SetPublicField("CustomScriptDescribe", des);
                                }
                            }
                        }

                    }

                    //获取ResultInfoDescribe
                    MethodInfo ridMInfo = _customScriptType.GetMethod("ResultInfoDescribe", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
                    if (ridMInfo != null)
                    {
                        string rid = (string)ridMInfo.Invoke(_customScript, null);
                        if (!string.IsNullOrEmpty(rid))
                        {
                            m_nodeGUI.data.SetPublicField("ResultInfoDescribe", rid);
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        private bool _hasCustomScript
        {
            get { return (_customScriptType != null && _customScript != null && _customScriptMethodInfo != null); }
        }

        public override void update(bool updateParentLoop = true)
        {

            if(!EditorUtility.DisplayDialog("提示", "开始Prefab处理?", "开始", "取消"))
            {
                return;
            }

            List<string> resultPathList = new List<string>();
            List<string> resultInfoList = new List<string>();

            int i = 0;
            int len;
            
            //获取自定义脚本
            if (!_hasCustomScript)
            {
                string guid = (string)m_nodeGUI.data.GetPublicField("CustomScriptGUID");
                if (!string.IsNullOrEmpty(guid))
                {
                    _getCustomScript(guid);
                }
            }

            //获取ActionID
            int actionID = 0;
            MethodInfo PreActionMI;
            object PreActionTarget;
            ConnectionPointGUI cpg0 = NodeGraphBase.Instance.GetConnectionPointGui(m_nodeGUI.id, 101, ConnectionPointInoutType.Input);
            List<ConnectionGUI> clist0 = NodeGraphBase.Instance.GetContainsConnectionGUI(cpg0);
            if (clist0 != null)
            {
                actionID = (int) clist0[0].GetConnectionValue(false);
                PreActionTarget = clist0[0].OutputPointGui.node.controller;
                PreActionMI = PreActionTarget.GetType().GetMethod("PredefinedAction", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
            }
            else
            {
                PreActionMI = null;
                PreActionTarget = null;
            }

            //获取上级节点数据 (PathInput)
            ConnectionPointGUI cpg = NodeGraphBase.Instance.GetConnectionPointGui(m_nodeGUI.id, 100, ConnectionPointInoutType.MutiInput);
            List<ConnectionGUI> clist = NodeGraphBase.Instance.GetContainsConnectionGUI(cpg);
            if (clist != null)
            {

                List<string> parentData = new List<string>();

                len = clist.Count;
                for (i = 0; i < len; i++)
                {
                    string[] pd = (string[])clist[i].GetConnectionValue(updateParentLoop);
                    if (pd != null)
                    {
                        //去重复
                        for (int a = 0; a < pd.Length; a++)
                        {
                            if (!parentData.Contains(pd[a]))
                            {
                                parentData.Add(pd[a]);
                            }
                        }
                    }
                }
                
                //查找Prefab
                List<string> inputPathList = new List<string>();
                len = parentData.Count;
                for (i = 0; i < len; i++)
                {
                    EditorAssetInfo info = new EditorAssetInfo(parentData[i]);
                    if (info.suffix.ToLower() == ".prefab")
                    {
                        inputPathList.Add(info.path);
                    }
                }

                if (inputPathList.Count > 0)
                {
                    
                    if (_hasCustomScript)
                    {
                        _customScriptResetMethodInfo.Invoke(_customScript, null);
                    }

                    len = inputPathList.Count;
                    for (i = 0; i < len; i++)
                    {
                        //EditorUtility.DisplayProgressBar("Processing ...", "Processing ..." + (i+1) + " / " + len, (float)(i+1) / len);

                        if(!EditorUtility.DisplayCancelableProgressBar("Processing ...", "Processing ..." + (i + 1) + " / " + len, (float)(i + 1) / len))
                        {
                            resultInfoList.Add($"[处理过程被取消]");
                            break;
                        }

                        string path = inputPathList[i];

#if UNITY_2018_1_OR_NEWER
                        GameObject prefab = PrefabUtility.LoadPrefabContents(path);
#else
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif
                        if(prefab)
                        {
                            
                            if (_hasCustomScript)
                            {
                                //自定义脚本
                                if((bool) _customScriptMethodInfo.Invoke(_customScript, new object[] {path, prefab, resultInfoList}))
                                {
                                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                                    resultPathList.Add(inputPathList[i]);
                                }
                            }                           
                            else if(actionID > 0 && PreActionMI != null)
                            {
                                //预制动作 todo ??
                                if((bool)PreActionMI.Invoke(PreActionTarget, new object[] { actionID, prefab, resultInfoList }))
                                {
                                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                                    resultPathList.Add(inputPathList[i]);
                                }
                            }
                            //如果该处理器既没有预设动作也没有自定义脚本，则视为通过
                            else{
                                resultPathList.Add(inputPathList[i]);
                            }
                            
#if UNITY_2018_1_OR_NEWER
                            PrefabUtility.UnloadPrefabContents(prefab);
#else
                            EditorUtility.UnloadUnusedAssetsImmediate(true);
#endif
                        }
                    }

                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                }
                else
                {
                    m_nodeGUI.data.SetPublicField("AssetsPath", null);
                }

            }
            else
            {
                m_nodeGUI.data.SetPublicField("AssetsPath", null);
            }
            
            //输出 。。。
            if (resultInfoList.Count > 0)
            {
                m_nodeGUI.data.SetPublicField("CustomScriptResultInfo", resultInfoList.ToArray());
            }
            else
            {
                m_nodeGUI.data.SetPublicField("CustomScriptResultInfo", null);
            }

            if (resultPathList.Count > 0)
            {
                m_nodeGUI.data.SetPublicField("AssetsPath", resultPathList.ToArray());
            }

            NodeGraphBase.TimeInterval_Request_SAVESHOTCUTGRAPH = true; //申请延迟保存快照
            base.update(updateParentLoop);
        }
    }
}
