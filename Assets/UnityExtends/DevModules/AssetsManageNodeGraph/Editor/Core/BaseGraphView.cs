using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System;

namespace AorNodeGraph
{

    public class BaseGraphView : GraphView
    {

        public new class UxmlFactory : UxmlFactory<BaseGraphView, UxmlTraits> { }

        public BaseGraphView()
        {
            m_map = new BaseGrapMap();
            init();
        }

        public BaseGraphView(BaseGrapMap map)
        {
            m_map = map;
            init();
            initMapData();
        }

        protected event Action<BaseNode> OnNodeSelected;

        protected void init()
        {
            this.AddManipulator(new ContentZoomer());
            var contentDragger = new ContentDragger();
            //contentDragger.
            this.AddManipulator(contentDragger);
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var gridBackground = new GridBackground();

            #region 想不通的地方 。。。
            //Todo ... 直接通过反射修改数值无效 ...
            //gridBackground.SetNonPublicField("m_GridBackgroundColor", new Color(0.1568627450980392f, 0.1568627450980392f, 0.1568627450980392f, 1f));
            // // gridBackground.SetNonPublicField("m_GridBackgroundColor", new Color(1,1, 1, 1f));
            //gridBackground.SetNonPublicField("m_LineColor", new Color(1, 1, 1, 0.18f));
            //gridBackground.SetNonPublicField("m_ThickLineColor", new Color(1, 1, 1, 0.38f));
            //gridBackground.SetNonPublicField("m_Spacing", 15f);
            #endregion

            gridBackground.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(BasePathsDefine.GridBackgroud_USS));
            Insert(0, gridBackground);
        }

        protected virtual void initMapData()
        {

        }

        protected readonly List<BaseNode> m_nodeList = new List<BaseNode>();

        /// <summary>
        /// !! graph的逻辑数据
        /// </summary>
        protected BaseGrapMap m_map;

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            foreach (var port in ports.ToList())
            {
                if (startPort.node == port.node
                     || startPort.direction == port.direction
                     || startPort.portType != port.portType
                )
                {
                    continue;
                }

                compatiblePorts.Add(port);
            }
            return compatiblePorts;
        }

        public void AddNode(BaseNode node)
        {
            if (!m_map.HasNode(node))
            {
                m_map.AddNode(node);
                AddElement(node);
            }
        }

        public void RemoveNode(BaseNode node)
        {
            if (m_map.HasNode(node))
            {
                m_map.RemoveNode(node);
                RemoveElement(node);
                node.Dispose();
            }
        }

        //public void AddGroup()
        //{

        //}

        //public void RemoveGroup()
        //{

        //}

        public void Connect(Port start, Port end)
        {
            var e = start.ConnectTo<Edge>(end);
            AddElement(e);
        }

        public void Disconnect(Edge edge)
        {
            edge.input.Disconnect(edge);
            edge.output.Disconnect(edge);
        }

    }

}



