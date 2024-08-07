using System.Collections;
using System.Collections.Generic;
using AORCore.Editor;
using UnityEditor;
using UnityEngine;
using UnityEditor.EditorTools;

namespace AORCore.Utility.Editor
{
    [EditorTool("Align Tool", typeof(GameObject))]
    public class TransformPlusAlignEditorTool : UnityEditor.EditorTools.EditorTool, IDrawSelectedHandles
    {

        protected static readonly HashSet<int> m_namesHash = new HashSet<int>();

        #region Inner Data

        private class InnerData
        {

            public InnerData(GameObject gameObject)
            {

                this.gameObject = gameObject;
                Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                if (renderers != null && renderers.Length > 0)
                {
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        Renderer renderer = renderers[i];
                        m_namesHash.Add(renderer.gameObject.GetHashCode());
                        if (i == 0)
                        {
                            m_bounds = renderer.bounds;
                        }
                        else
                        {
                            m_bounds.Encapsulate(renderer.bounds);
                        }


                    }
                    m_offset = m_bounds.center - gameObject.transform.position;
                    m_useBounds = true;
                }
                else
                    m_useBounds = false;
            }

            public void Dispose()
            {
                gameObject = null;
            }

            public GameObject gameObject;
            private bool m_useBounds;
            private Vector3 m_offset;
            private Bounds m_bounds;

            public Bounds bounds
            {
                get
                {
                    if (m_useBounds)
                        return m_bounds;
                    return new Bounds(gameObject.transform.position, Vector3.zero);
                }
            }

            public float GetPosX => gameObject.transform.position.x;
            public float GetPosY => gameObject.transform.position.y;
            public float GetPosZ => gameObject.transform.position.z;

            public void SetPosition(Vector3 pos)
            {
                gameObject.transform.position = pos - m_offset;
            }

        }

        #endregion

        protected GUIContent m_toolbarIcon;
        public override GUIContent toolbarIcon
        {
            get 
            {
                if(m_toolbarIcon == null)
                {
                    m_toolbarIcon = new GUIContent(EditorGUIUtility.IconContent("d_UnityEditor.SceneView").image, "Align Tool");
                }
                return m_toolbarIcon; 
            }
        }
        // Global tools (tools that do not specify a target type in the attribute) are lazy initialized and persisted by
        // a ToolManager. Component tools (like this example) are instantiated and destroyed with the current selection.
        void OnEnable()
        {
            // Allocate unmanaged resources or perform one-time set up functions here
        }

        void OnDisable()
        {
            // Free unmanaged resources, state teardown.
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView sceneView))
                return;

            Handles.BeginGUI();
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        //Align
                        GUILayout.Label("Align", GUILayout.Width(60));
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("X:");
                            if (GUILayout.Button("|←"))
                            {
                                AlignX_min();
                            }
                            if (GUILayout.Button("|"))
                            {
                                AlignX_center();
                            }
                            if (GUILayout.Button("→|"))
                            {
                                AlignX_max();
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Y:");
                            if (GUILayout.Button("＿"))
                            {
                                AlignY_min();
                            }
                            if (GUILayout.Button("―"))
                            {
                                AlignY_center();
                            }
                            if (GUILayout.Button("￣"))
                            {
                                AlignY_max();
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Z:");
                            if (GUILayout.Button("|←"))
                            {
                                AlignZ_min();
                            }
                            if (GUILayout.Button("|"))
                            {
                                AlignZ_center();
                            }
                            if (GUILayout.Button("→|"))
                            {
                                AlignZ_max();
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(8);

                        //Array
                        GUILayout.Label("Array", GUILayout.Width(60));
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("ArrayX"))
                            {
                                ArrayX();
                            }
                            if (GUILayout.Button("ArrayY"))
                            {
                                ArrayY();
                            }
                            if (GUILayout.Button("ArrayZ"))
                            {
                                ArrayZ();
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.FlexibleSpace();
            }
            Handles.EndGUI();

        }
        public void OnDrawHandles()
        {
            //foreach (var obj in targets)
            //{
            //    if (obj is Platform platform)
            //        Handles.DrawLine(platform.start, platform.end, 6f);
            //}
        }

        private List<InnerData> BuildInnerDatasBySelection()
        {
            List<InnerData> datas = new List<InnerData>();
            foreach (var g in Selection.gameObjects)
            {
                if (!m_namesHash.Contains(g.GetHashCode()))
                    datas.Add(new InnerData(g));
            }
            return datas;
        }

        private Bounds CalculateMergeBounds(List<InnerData> datas)
        {
            Bounds b = default;
            for (int i = 0; i < datas.Count; i++)
            {
                if (i == 0)
                    b = datas[i].bounds;
                else
                    b.Encapsulate(datas[i].bounds);
            }
            return b;
        }

        private Bounds CalculateMergeBounds(List<InnerData> datas, out float sizeSumX, out float sizeSumY, out float sizeSumZ)
        {
            Bounds b = default;
            sizeSumX = 0;
            sizeSumY = 0;
            sizeSumZ = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                InnerData data = datas[i];
                if (i == 0)
                {
                    b = data.bounds;
                    sizeSumX = data.bounds.size.x;
                    sizeSumY = data.bounds.size.y;
                    sizeSumZ = data.bounds.size.z;
                }
                else
                {
                    b.Encapsulate(data.bounds);
                    sizeSumX += data.bounds.size.x;
                    sizeSumY += data.bounds.size.y;
                    sizeSumZ += data.bounds.size.z;
                }
            }
            return b;
        }

        private void DisposeInnerdatas(List<InnerData> datas)
        {
            foreach (var data in datas)
            {
                data.Dispose();
            }
            datas.Clear();
        }

        #region Align 实现

        private void AlignX_min()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align X MIN");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(b.min.x + data.bounds.extents.x, data.gameObject.transform.position.y, data.gameObject.transform.position.z));
            }
            DisposeInnerdatas(datas);
        }

        private void AlignX_center()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align X CENTER");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(b.center.x, data.gameObject.transform.position.y, data.gameObject.transform.position.z));
            }
            DisposeInnerdatas(datas);
        }

        private void AlignX_max()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align X MAX");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(b.max.x - data.bounds.extents.x, data.gameObject.transform.position.y, data.gameObject.transform.position.z));
            }
            DisposeInnerdatas(datas);
        }

        private void AlignY_min()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align Y MIN");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(data.gameObject.transform.position.x, b.min.y + data.bounds.extents.y, data.gameObject.transform.position.z));
            }
            DisposeInnerdatas(datas);
        }

        private void AlignY_center()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align Y CENTER");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(data.gameObject.transform.position.x, b.center.y, data.gameObject.transform.position.z));
            }
            DisposeInnerdatas(datas);
        }

        private void AlignY_max()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align Y MAX");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(data.gameObject.transform.position.x, b.max.y - data.bounds.extents.y, data.gameObject.transform.position.z));
            }
            DisposeInnerdatas(datas);
        }

        private void AlignZ_min()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align Z MIN");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(data.gameObject.transform.position.x, data.gameObject.transform.position.y, b.min.z + data.bounds.extents.z));
            }
            DisposeInnerdatas(datas);
        }

        private void AlignZ_center()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align Z CENTER");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(data.gameObject.transform.position.x, data.gameObject.transform.position.y, b.center.z));
            }
            DisposeInnerdatas(datas);
        }

        private void AlignZ_max()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "Align Z MAX");

            Bounds b = CalculateMergeBounds(datas);
            foreach (var data in datas)
            {
                data.SetPosition(new Vector3(data.gameObject.transform.position.x, data.gameObject.transform.position.y, b.max.z - data.bounds.extents.z));
            }
            DisposeInnerdatas(datas);
        }

        #endregion

        #region Array 实现

        private void SetUnDo(List<InnerData> datas, string undoTag)
        {
            List<UnityEngine.Object> list = new List<Object>();
            foreach (var data in datas)
            {
                list.Add(data.gameObject.transform);
            }
            Undo.RecordObjects(list.ToArray(), undoTag);
        }

        private void ArrayX()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "ArrayX");

            //排序
            datas.Sort((p, n) => 
            { 
                if(p.GetPosX > n.GetPosX)
                    return 1;
                else
                    return -1;
            });

            //计算间隔
            Bounds b = new Bounds();
            float sizeSumX = 0;
            float min = 0;
            float max = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                InnerData data = datas[i];
                if (i == 0)
                {
                    b = data.bounds;
                    sizeSumX = data.bounds.size.x;
                    min = data.bounds.min.x;
                    max = data.bounds.max.x;
                }
                else
                {
                    sizeSumX += data.bounds.size.x;
                    b.Encapsulate(data.bounds);
                    if(data.bounds.min.x < min)
                        min = data.bounds.min.x;
                    if(data.bounds.max.x > max)
                        max = data.bounds.max.x;
                }
            }
            float len = max - min;
            float interval =  (len - sizeSumX) / (datas.Count -1);
            float tmp = b.min.x;
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                if (i > 0)
                {
                    tmp += interval;
                }
                data.SetPosition(new Vector3(tmp + data.bounds.extents.x, data.GetPosY, data.GetPosZ));
                tmp += data.bounds.size.x;
            }
        }

        private void ArrayY()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "ArrayY");

            //排序
            datas.Sort((p, n) =>
            {
                if (p.GetPosY > n.GetPosY)
                    return 1;
                else
                    return -1;
            });

            //计算间隔
            Bounds b = new Bounds();
            float sizeSumY = 0;
            float min = 0;
            float max = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                InnerData data = datas[i];
                if (i == 0)
                {
                    b = data.bounds;
                    sizeSumY = data.bounds.size.y;
                    min = data.bounds.min.y;
                    max = data.bounds.max.y;
                }
                else
                {
                    sizeSumY += data.bounds.size.y;
                    b.Encapsulate(data.bounds);
                    if (data.bounds.min.y < min)
                        min = data.bounds.min.y;
                    if (data.bounds.max.y > max)
                        max = data.bounds.max.y;
                }
            }
            float len = max - min;
            float interval = (len - sizeSumY) / (datas.Count - 1);
            float tmp = b.min.y;
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                if(i > 0)
                {
                    tmp += interval;
                }
                data.SetPosition(new Vector3(data.GetPosX, tmp + data.bounds.extents.y, data.GetPosZ));
                tmp += data.bounds.size.y;
            }
        }

        private void ArrayZ()
        {
            m_namesHash.Clear();
            List<InnerData> datas = BuildInnerDatasBySelection();
            if (datas.Count == 0) return;

            SetUnDo(datas, "ArrayZ");

            //排序
            datas.Sort((p, n) =>
            {
                if (p.GetPosZ > n.GetPosZ)
                    return 1;
                else
                    return -1;
            });

            //计算间隔
            Bounds b = new Bounds();
            float sizeSumZ = 0;
            float min = 0;
            float max = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                InnerData data = datas[i];
                if (i == 0)
                {
                    b = data.bounds;
                    sizeSumZ = data.bounds.size.z;
                    min = data.bounds.min.z;
                    max = data.bounds.max.z;
                }
                else
                {
                    sizeSumZ += data.bounds.size.z;
                    b.Encapsulate(data.bounds);
                    if (data.bounds.min.z < min)
                        min = data.bounds.min.z;
                    if (data.bounds.max.z > max)
                        max = data.bounds.max.z;
                }
            }
            float len = max - min;
            float interval = (len - sizeSumZ) / (datas.Count - 1);
            float tmp = b.min.z; ;
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                if (i > 0)
                {
                    tmp += interval;
                }
                data.SetPosition(new Vector3(data.GetPosX, data.GetPosY, tmp + data.bounds.extents.z));
                tmp += data.bounds.size.z;
            }
        }

        #endregion

    }
}



