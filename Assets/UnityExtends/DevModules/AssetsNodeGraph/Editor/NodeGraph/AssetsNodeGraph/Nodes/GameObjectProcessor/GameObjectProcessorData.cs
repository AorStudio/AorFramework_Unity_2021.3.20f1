using System;
using System.Collections.Generic;

namespace NodeGraph.Editor
{
    public class GameObjectProcessorData : NodeData
    {

        public GameObjectProcessorData()
        {
        }

        public GameObjectProcessorData(long id):base(id)
        {
        }

        public readonly int ActionId;

        public readonly string Action1Param;

        public readonly string CustomScriptGUID;

        public readonly string CustomScriptDescribe;

        public readonly string ResultInfoDescribe;

        public readonly string[] CustomScriptResultInfo;

        public readonly int[] InstancesPath;

    }
}
