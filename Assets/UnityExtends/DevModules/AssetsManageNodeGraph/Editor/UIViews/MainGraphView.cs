using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using AorNodeGraph;

namespace AssetsManageNodeGraph.UIview
{
    public class MainGraphView : BaseGraphView
    {
        
        public new class UxmlFactory : UxmlFactory<MainGraphView, UxmlTraits> { }

        public AMNodeGraphWindow window;
        
    }
}