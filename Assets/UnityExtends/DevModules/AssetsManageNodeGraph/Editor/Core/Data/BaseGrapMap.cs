using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AorNodeGraph
{
    //psd ?
    [Serializable]
    public class BaseGrapMap
    {
        public string Name;
        public string Description;
        public string UpdateDate;

        private readonly HashSet<BaseNode> Nodes = new HashSet<BaseNode>();

        //public List<BaseNode> Nodes;
        //public List<BaseGroup> Groups;
        //public List<BaseLineInfo> lineInfos;

        public void AddNode(BaseNode baseNode)
        {
            Nodes.Add(baseNode);
        }
        public bool HasNode(BaseNode baseNode)
        {
            return Nodes.Contains(baseNode);
        }

        public void RemoveNode(BaseNode baseNode)
        {
            Nodes.Remove(baseNode);
        }

        public void AddGroup() { }

        public bool HasGroup()
        {
            return true;
        }

        public void RemoveGroup() { }



    }
}
