using System;
using System.Collections.Generic;
using NodeGraphLibs;

namespace NodeGraph.Editor
{
    public class HierarchyObjRenamerData : NodeData
    {
        public HierarchyObjRenamerData() {}

        public HierarchyObjRenamerData(long id) : base(id) {}
        
        public readonly bool UseEditorSelection = false;

        public readonly string RenameKey;

        public readonly int ShotNum;

        public readonly int[] InstancesPath;

        public readonly string[] ResultInfo;

    }
}
