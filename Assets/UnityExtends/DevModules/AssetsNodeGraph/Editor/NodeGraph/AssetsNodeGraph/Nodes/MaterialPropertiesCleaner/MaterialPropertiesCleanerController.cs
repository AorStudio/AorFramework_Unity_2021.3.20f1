using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#if FRAMEWORKDEF
using AorBaseUtility.Extends;
#else
using NodeGraphLibs;
#endif

namespace NodeGraph.Editor
{
    public class MaterialPropertiesCleanerController : NodeController
    {
        public override void update(bool updateParentLoop = true)
        {
            
            //获取上级节点数据
            ConnectionPointGUI cpg = NodeGraphBase.Instance.GetConnectionPointGui(m_nodeGUI.id, 100, ConnectionPointInoutType.MutiInput);
            List<ConnectionGUI> clist = NodeGraphBase.Instance.GetContainsConnectionGUI(cpg);
            if(clist != null)
            {

                List<string> parentData = new List<string>();
                int i, len = clist.Count;
                for(i = 0; i < len; i++)
                {
                    string[] pd = (string[])clist[i].GetConnectionValue(updateParentLoop);
                    if(pd != null)
                    {
                        //去重复
                        for(int a = 0; a < pd.Length; a++)
                        {
                            if(!parentData.Contains(pd[a]))
                            {
                                parentData.Add(pd[a]);
                            }
                        }
                    }
                }
                
                List<string> result = new List<string>();
                bool justReport = (bool)m_nodeGUI.data.GetPublicField("JustReport");
                string report = string.Empty;
                foreach(var path in parentData)
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(material)
                    {
                        if(MaterialPropertiesCleanUtils.ParseMaterialProperties(material, ref report, justReport))
                        {
                            result.Add(path);
                        }
                    }
                }

                m_nodeGUI.data.SetPublicField("Report", report);
                m_nodeGUI.data.SetPublicField("AssetsPath", result.ToArray());

            }
            else
            {
                m_nodeGUI.data.SetPublicField("Report", null);
                m_nodeGUI.data.SetPublicField("AssetsPath", null);
            }


            NodeGraphBase.TimeInterval_Request_SAVESHOTCUTGRAPH = true; //申请延迟保存快照
            base.update(updateParentLoop);
        }

        
    }

}
