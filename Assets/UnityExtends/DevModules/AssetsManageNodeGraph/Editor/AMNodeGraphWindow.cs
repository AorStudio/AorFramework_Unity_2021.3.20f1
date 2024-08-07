using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using AssetsManageNodeGraph.UIview;
using AorNodeGraph;

namespace AssetsManageNodeGraph
{

    public class AMNodeGraphWindow : EditorWindow
    {

        [MenuItem("Window/AssetsManageNodeGraph/CreateWindow")]
        private static AMNodeGraphWindow Create()
        {
            var window = EditorWindow.CreateInstance<AMNodeGraphWindow>();
            window.titleContent = new GUIContent("AMNodeGraphWindow");
            window.Show();
            return window;
        }

        #region VisualElement Creatives 

        private VisualElement m_ve_first;
        private VisualElement VE_first
        {
            get { 
                if (m_ve_first == null)
                {
                    m_ve_first = new VisualElement();
                    m_ve_first.name = "VE_first";
                }
                return m_ve_first; 
            }
        }

        private VisualElement m_ve_main;
        private VisualElement VE_main
        {
            get
            {
                if (m_ve_main == null)
                {
                    m_ve_main = new VisualElement();
                    m_ve_main.name = "VE_main";

                    m_ve_main.style.position = Position.Absolute;
                    m_ve_main.style.flexDirection = FlexDirection.Row;
                    m_ve_main.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    m_ve_main.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    var h0 = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
                    m_ve_main.Add(h0);

                    //tool Area
                    var toolArea = new ToolsAreaView();
                    toolArea.name = "ToolArea";
                    toolArea.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    toolArea.style.minWidth = new StyleLength(new Length(200, LengthUnit.Pixel));
                    toolArea.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    //toolArea.style.backgroundColor = Color.cyan;
                    h0.Add(toolArea);

                    var h1 = new TwoPaneSplitView(1, 300, TwoPaneSplitViewOrientation.Horizontal);
                    h0.Add(h1);

                    var mid = new VisualElement();
                    mid.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    mid.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    h1.Add(mid);

                    var menuArea = new VisualElement();
                    menuArea.name = "MenuArea";
                    menuArea.style.flexDirection = FlexDirection.Row;
                    menuArea.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    menuArea.style.height = new StyleLength(new Length(36, LengthUnit.Pixel));
                    //menuArea.style.backgroundColor = Color.blue;
                    mid.Add(menuArea);

                    //mainArea
                    var mainArea = new MainGraphView();
                    mainArea.name = "MainGraphView";
                    mainArea.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    mainArea.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    //mainArea.style.backgroundColor = Color.red;
                    mid.Add(mainArea);

                    //inspector
                    var inspectorArea = new VisualElement();
                    inspectorArea.name = "InspectorArea";
                    inspectorArea.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    inspectorArea.style.minWidth = new StyleLength(new Length(300, LengthUnit.Pixel));
                    inspectorArea.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    //inspectorArea.style.backgroundColor = Color.black;

                    h1.Add(inspectorArea);
                }
                return m_ve_main;
            }
        }

        #endregion

        private void CreateGUI()
        {
            var root = this.rootVisualElement;
            root.Add(VE_main);

            //Test 
            var mainGraphView = root.Q<MainGraphView>("MainGraphView");
            var node_a = BaseNode.CreateDefualt();
            var node_b = BaseNode.CreateDefualt();
            mainGraphView.AddNode(node_a);
            mainGraphView.AddNode(node_b);

            mainGraphView.Connect(node_a.outputContainer[0] as Port, node_b.inputContainer[0] as Port);

            mainGraphView.AddElement(new Group());


        }


    }

}
