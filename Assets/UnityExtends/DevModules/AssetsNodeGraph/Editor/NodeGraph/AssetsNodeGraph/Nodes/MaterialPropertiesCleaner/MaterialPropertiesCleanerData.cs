using System;
using System.Collections.Generic;

namespace NodeGraph.Editor
{
    public class MaterialPropertiesCleanerData : NodeData
    {
        public MaterialPropertiesCleanerData() {}

        public MaterialPropertiesCleanerData(long id) : base(id) {}

        public readonly bool JustReport;

        public readonly string Report;

        public readonly string[] AssetsPath;

    }
}
