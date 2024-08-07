
using System;
using System.Collections.Generic;
#if FRAMEWORKDEF
using AorBaseUtility.Config;
#else
using NodeGraphLibs;
#endif

namespace NodeGraph.Editor
{
    //NodeGraph基本设置
    public class NodeGraphBaseSetting : Config
    {
        public NodeGraphBaseSetting(){}

    }
}
