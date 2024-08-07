using System;
using System.Collections.Generic;
using NodeGraphLibs;
using UnityEditor;
using UnityEngine;

namespace NodeGraph.Editor
{
    [NodeToolItem("#<20>",
        "NodeGraph.Editor",
        "MaterialPropertiesCleanerData|MaterialPropertiesCleanerController|MaterialPropertiesCleanerGUIController",
        "Assets", -100, true)]
    public class MaterialPropertiesCleanerGUIController : NodeGUIController
    {

        private GUIStyle _guiContentTextStyle;

        public override string GetNodeLabel()
        {
            return AssetNodeGraphLagDefind.GetLabelDefine(20);
        }

        private Vector2 _MinSizeDefind = new Vector2(180, 140);
        public override Vector2 GetNodeMinSizeDefind()
        {
            return _MinSizeDefind;
        }

        public override void DrawConnectionTip(Vector3 centerPos, ConnectionGUI connection)
        {
            //string
            string info = "0";
            object ConnectionValue = connection.GetConnectionValue(false);
            if (ConnectionValue != null)
            {
                if (ConnectionValue is Array)
                {
                    info = (ConnectionValue as Array).Length.ToString();
                }
            }

            //size
            Vector2 CTSzie = new Vector2(NodeGraphTool.GetConnectCenterTipLabelWidth(info) + 4, NodeGraphDefind.ConnectCenterTipLabelPreHeight);

            //rect
            connection.CenterRect = new Rect(centerPos.x - CTSzie.x * 0.5f, centerPos.y - CTSzie.y * 0.5f, CTSzie.x, CTSzie.y);

            //ConnectionTip
            GUI.Label(connection.CenterRect, info, GetConnectCenterTipStyle());

            //右键菜单检测
            if (Event.current.button == 1 && Event.current.isMouse && connection.CenterRect.Contains(Event.current.mousePosition))
            {
                DrawCenterTipContextMenu(connection);
                Event.current.Use();
            }
        }

        public override void DrawNodeInspector(float inspectorWidth)
        {
            if (m_nodeGUI == null) return;

            GUILayout.BeginVertical("box", GUILayout.Width(inspectorWidth));
            {
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            //base.DrawNodeInspector(inspectorWidth);
        }

        public override List<ConnectionPointGUI> GetConnectionPointInfo (GetConnectionPointMode GetMode)
        {
            if(_ConnectionPointGUIList == null)
            {
                ConnectionPointGUI p0 = new ConnectionPointGUI(100, 0, 1, typeof(string[]).Name, "input", m_nodeGUI, AssetNodeGraphLagDefind.GetLabelDefine(7), new Vector2(100, 60), ConnectionPointInoutType.MutiInput);
                ConnectionPointGUI p1 = new ConnectionPointGUI(101, 0, 2, typeof(string[]).Name, "AssetsPath", m_nodeGUI, AssetNodeGraphLagDefind.GetLabelDefine(8), new Vector2(120, 60), ConnectionPointInoutType.Output);
                ConnectionPointGUI p2 = new ConnectionPointGUI(102, 1, 2, typeof(string).Name, "Report", m_nodeGUI, AssetNodeGraphLagDefind.GetLabelDefine(21), new Vector2(120, 60), ConnectionPointInoutType.Output);
                _ConnectionPointGUIList = new List<ConnectionPointGUI>() { p0, p1, p2 };
            }

            return _GetConnectionPointsByMode(GetMode);
        }
    }
}
