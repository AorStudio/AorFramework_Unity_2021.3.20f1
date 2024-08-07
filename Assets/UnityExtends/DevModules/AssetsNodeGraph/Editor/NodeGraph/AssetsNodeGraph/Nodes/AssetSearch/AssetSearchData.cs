using System;
using System.Collections.Generic;

namespace NodeGraph.Editor
{
    public class AssetSearchData : NodeData
    {
        public AssetSearchData() {}

        public AssetSearchData(long id) : base(id) {}

        public readonly bool IgnoreMETAFile = true;

        public readonly bool AdvancedOption = false;

        public readonly string SearchPattern;

        public readonly string SearchPath;

        public readonly string[] AssetsPath;

    }
}
