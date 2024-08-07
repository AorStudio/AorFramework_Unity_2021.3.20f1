using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace AorNodeGraph
{
    [Serializable]
    public class BaseNode : Node
    {

        public static BaseNode CreateDefualt()
        {
            var node = new BaseNode();
            node.ThemeColor = Color.yellow;
            node.AddInputPort("IN", node.ThemeColor, typeof(float));
            node.AddOutputPort("OUT", node.ThemeColor, typeof(float));
            return node;
        }

        protected event Action<Color> OnThemeColorChanged;

        [SerializeField]
        protected Color m_ThemeColor = Color.white;
        public Color ThemeColor
        {
            get { return m_ThemeColor; }
            set
            {
                if (m_ThemeColor != value)
                {
                    m_ThemeColor = value;
                    OnThemeColorChanged?.Invoke(m_ThemeColor);
                }
            }
        }

        protected Label m_TitleLine;

        public BaseNode()
        {
            CreateGUIInternal();
        }

        protected bool m_isDisposed;
        public virtual void Dispose()
        {
            OnThemeColorChanged = null;
            m_isDisposed = true;
        }

        private void CreateGUIInternal()
        {
            this.style.minWidth = 200;
            m_TitleLine = new Label();
            m_TitleLine.style.backgroundColor = m_ThemeColor;
            m_TitleLine.style.height = 6;
            this.mainContainer.Insert(0, m_TitleLine);
            Action<Color> callback = null;
            callback = (c) =>
            {
                if (m_TitleLine != null)
                    m_TitleLine.style.backgroundColor = c;
                else
                    OnThemeColorChanged -= callback;
            };
            OnThemeColorChanged += callback;
            var titleElement = this.Q<Label>("title-label", (string)null);
            titleElement.style.fontSize = 20;
            titleElement.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        protected virtual void CreateGUI() { }

        #region Add/Reomve/Clear Ports

        protected Port AddInputPort(string name, Type type, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            return AddPortInternal(name, type, Direction.Input, capacity, orientation);
        }

        protected Port AddInputPort(string name, string tooltip, Type type, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            var port = AddPortInternal(name, type, Direction.Input, capacity, orientation);
            port.tooltip = tooltip;
            return port;
        }

        protected Port AddInputPort(string name, Color portColor, Type type, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            var port = AddPortInternal(name, type, Direction.Input, capacity, orientation);
            port.portColor = portColor;
            return port;
        }

        protected Port AddInputPort(string name, string tooltip, Color portColor, Type type, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            var port = AddPortInternal(name, type, Direction.Input, capacity, orientation);
            port.tooltip = tooltip;
            port.portColor = portColor;
            return port;
        }

        protected void RemoveInputPort(Port port) 
        {
            this.inputContainer.Remove(port);
        }

        protected void ClearInputPorts()
        {
            this.inputContainer.Clear();
        }

        protected Port AddOutputPort(string name, Type type, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            return AddPortInternal(name, type, Direction.Output, capacity, orientation);
        }

        protected Port AddOutputPort(string name, string tooltip, Type type, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            var port = AddPortInternal(name, type, Direction.Output, capacity, orientation);
            port.tooltip = tooltip;
            return port;
        }

        protected Port AddOutputPort(string name, Color portColor, Type type, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            var port = AddPortInternal(name, type, Direction.Output, capacity, orientation);
            port.portColor = portColor;
            return port;
        }

        protected Port AddOutputPort(string name, string tooltip, Color portColor, Type type, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            var port = AddPortInternal(name, type, Direction.Output, capacity, orientation);
            port.tooltip = tooltip;
            port.portColor = portColor;
            return port;
        }

        protected void RemoveOutputPort(Port port)
        {
            this.outputContainer.Remove(port);
        }

        protected void ClearOutputPorts()
        {
            this.outputContainer.Clear();
        }

        protected Port AddPortInternal(string name, Type type, Direction direction, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
        {
            var port = Port.Create<Edge>(orientation, direction, capacity, type);
            port.portName = name;
            if (direction == Direction.Input)
                this.inputContainer.Add(port);
            else
                this.outputContainer.Add(port);
            Action<Color> callback = null;
            callback = (c) =>
            {
                if (port != null)
                    port.portColor = c;
                else
                    OnThemeColorChanged -= callback;
            };
            OnThemeColorChanged += callback;
            return port;
        }

        #endregion

    }
}


