﻿using System;
using System.Collections.Generic;

namespace NodeGraph.Editor
{
    public class AssetDependData : NodeData
    {

        public AssetDependData()
        {
        }

        public AssetDependData(long id) : base(id)
        {
        }

        public readonly bool[] IgnoreCase;

        public readonly bool AdvancedOption = false;

        public readonly int FilterMode = 0;

        public readonly string[] FilterKeys;

        public readonly string[] AssetsPath;

    }
}
