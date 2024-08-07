using System;
using System.Collections.Generic;
#if FRAMEWORKDEF
using AorBaseUtility.Extends;
#else
using NodeGraphLibs;
#endif
namespace NodeGraph.Editor
{
    public class AssetFilterController : NodeController
    {
        public override void update(bool updateParentLoop = true)
        {
            //获取上级节点数据
            ConnectionPointGUI cpg = NodeGraphBase.Instance.GetConnectionPointGui(m_nodeGUI.id, 100, ConnectionPointInoutType.MutiInput);
            List<ConnectionGUI> clist = NodeGraphBase.Instance.GetContainsConnectionGUI(cpg);
            if (clist != null)
            {

                List<string> parentData = new List<string>();

                int i, len = clist.Count;
                for (i = 0; i < len; i++)
                {
                    string[] pd = (string[]) clist[i].GetConnectionValue(updateParentLoop);
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

                //计算过滤结果
                string[] fks = (string[])m_nodeGUI.data.GetPublicField("FilterKeys");
                bool[] ics = (bool[])m_nodeGUI.data.GetPublicField("IgnoreCase");
                if (fks != null)
                {

                    List<string> result = new List<string>();

                    len = parentData.Count;
                    int j, jlen = fks.Length;
                    for (i = 0; i < len; i++)
                    {

                        bool pass = true;

                        string data = parentData[i];

                        for(j = 0; j < jlen; j++)
                        {

                            string fkey = fks[j].Trim();

                            if(string.IsNullOrEmpty(fkey))
                                continue;

                            bool sx = fkey.StartsWith(".");
                            bool ig = ics[j];
                            bool nt = fkey.StartsWith("!");
                            if(nt)
                                fkey = fkey.Substring(1);

                            if(sx)
                            {
                                pass = data.ToLower().EndsWith(fkey.ToLower());
                                if(nt)
                                    pass = !pass;
                                if(!pass)
                                    break;
                            }
                            else
                            {
                                if(ig)
                                {
                                    pass = data.ToLower().Contains(fkey.ToLower());
                                    if(nt)
                                        pass = !pass;
                                    if(!pass)
                                        break;
                                }
                                else
                                {
                                    pass = data.Contains(fkey);
                                    if(nt)
                                        pass = !pass;
                                    if(!pass)
                                        break;
                                }
                            }

                        }

                        if(pass)
                            result.Add(data);

                    }

                    if (result.Count > 0)
                    {
                        m_nodeGUI.data.SetPublicField("AssetsPath", result.ToArray());
                    }
                    else
                    {
                        m_nodeGUI.data.SetPublicField("AssetsPath", null);
                    }
                }
                else
                {
                    m_nodeGUI.data.SetPublicField("AssetsPath", parentData.ToArray());
                }
            }
            else
            {
                m_nodeGUI.data.SetPublicField("AssetsPath", null);
            }

            NodeGraphBase.TimeInterval_Request_SAVESHOTCUTGRAPH = true; //申请延迟保存快照
            base.update(updateParentLoop);
        }
    }
}
