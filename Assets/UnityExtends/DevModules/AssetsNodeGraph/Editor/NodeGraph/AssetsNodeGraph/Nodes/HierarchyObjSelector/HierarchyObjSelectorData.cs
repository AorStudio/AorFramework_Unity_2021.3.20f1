using System;
using System.Collections.Generic;
using NodeGraphLibs;

namespace NodeGraph.Editor
{
    public class HierarchyObjSelectorData : NodeData
    {
        public HierarchyObjSelectorData() {}

        public HierarchyObjSelectorData(long id) : base(id) {}

        //public readonly TreeNode<int> SelectedTreeNode; 

        public readonly int[] SelectedInstanceIDs;

    }
}
