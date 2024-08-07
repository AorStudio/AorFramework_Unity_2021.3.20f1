using UnityEngine;
using System;
using System.Collections;


namespace UnityEngine.Rendering.Universal.module
{
    /// <summary>
    /// This class contains functions to create and edit bezier splines. 
    /// It also contains methods to walk the spline (or spline sections) at constant speed based on _easeCurve first derivative.
    /// Arc length reparameterization can be performed with a high cost accurate function or with a low cost real-time oriented approximate function.
    ///
    /// Update Date : 2021-05-25 Aorition
    ///
    /// </summary>
    public class Spline : MonoBehaviour
    {

        /// <summary>
        /// Creates an empty spline
        /// </summary>
        public static Spline Create()
        {
            return new GameObject("Spline", typeof(Spline)).GetComponent<Spline>();
        }

        public static Spline Create(Vector3[] RawPoints)
        {
            Spline sp = new GameObject("Spline", typeof(Spline)).GetComponent<Spline>();
            sp.Init(RawPoints);
            return sp;
        }

        [SerializeField]
        protected string m_id;
        public string Id
        {
            get { return m_id; }
        }

        [SerializeField]
        protected Vector3[] m_points;
        
        public int ControlPointLength 
        { 
            get {
                if(m_points != null)
                    return m_points.Length;
                return 0;
            } 
        }


        public bool HasDatas
        {
            get { return m_points?.Length > 0; }
        }

        /// <summary>
        /// The _mode for each control point (corner, aligned, smooth).
        /// </summary>
        [SerializeField]
        public BezierControlPointMode[] m_modes;

        /// <summary>
        /// Is this a looping spline
        /// </summary>
        [SerializeField]
        protected bool m_loop;
        
        /// <summary>
        /// The _easeCurve lengths cache.
        /// </summary>
        private float[] m_curveLengths;

        /// <summary>
        /// The arc lengths cache, each value represents the length of the arc from spline start to the _easeCurve end.
        /// </summary>
        private float[] m_arcLengths;

        private Vector3[] m_orientationVectors;

        /// <summary>
        /// Gets the total length of the spline.
        /// </summary>
        /// <value>
        /// The spline length.
        /// </value>
        public float Length
        {
            get
            {
                _UpdateLengths();

                return m_arcLengths[curveCount];
            }
        }
        
#if UNITY_EDITOR

        /// <summary>
        /// Show gizmo in viewport
        /// </summary>
        //public bool ShowGizmo = true;

        /// <summary>
        /// Show velocities in viewport.
        /// </summary>
        public bool ShowVelocities = false;

        /// <summary>
        /// Show accelerations in viewport.
        /// </summary>
        public bool ShowAccelerations = false;

        /// <summary>
        /// Show control point numbers in viewport.
        /// </summary>
        public bool ShowNumbers = false;

        /// <summary>
        /// The spline color showed in the viewport.
        /// </summary>
        public Color GizmosColor = Color.white;

        /// <summary>
        /// Resets this instance with an horizontal segment of length 3.
        /// </summary>
        public void Reset()
        {
            m_points = new Vector3[] {
                new Vector3(1f, 2f, 2f),
                new Vector3(2f, 2f, 2f),
                new Vector3(3f, 2f, 2f),
                new Vector3(4f, 2f, 2f)
            };

            m_modes = new BezierControlPointMode[] {
                BezierControlPointMode.Smooth,
                BezierControlPointMode.Smooth
            };
            Debug.Log("*Spline.Reset()");
        }

        void OnDrawGizmos()
        {
            DoGizmos(false);
        }

        void OnDrawGizmosSelected()
        {
            DoGizmos(true);
        }

        /// <summary>
        /// This function renders the spline in the viewport when it is not selected. 
        /// </summary>
        /// <remarks>
        /// It draws the spline with a set of selectable segments. 
        /// A gizmo is included as well to help the user in the selection.
        /// It is compiled only inside the unity editor. It is not included in the exported builds.
        /// </remarks>
        /// <param name="selected">True if the spline is selected.</param>
        void DoGizmos(bool selected)
        {

            if(m_points == null || m_points.Length == 0)
                return;

            Color c = GizmosColor;
            c.a = selected ? 1.0f : 0.5f;

            //if(ShowGizmo)
            //    Gizmos.DrawIcon(GetPointAtIndex(0), "spline-gizmo");

            // Draw spline
            Gizmos.color = c;
            for(int i = 0; i < curveCount; i++)
            {
                Vector3 point1 = GetPoint(i, i + 1, 0);
                float step = 1f / 20f;
                for(float t = step; ; t += step)
                {
                    if(t > 1)
                        t = 1;

                    Vector3 point2 = GetPoint(i, i + 1, t);

                    Gizmos.DrawLine(point1, point2);

                    point1 = point2;

                    if(t == 1)
                        break;
                }
            }
        }

#endif

        /// <summary>
        /// The distance between each sample to be used in the approximate arc length reparameterization algorithm.
        /// </summary>
        private float m_samplesDistance = 1;

        /// <summary>
        /// Gets or sets the distance between samples to be used in the approximate arc length reparameterization algorithm.
        /// </summary>
        /// <value>
        /// The n samples.
        /// </value>
        public float SamplesDistance
        {
            get { return m_samplesDistance; }
            set { m_samplesDistance = value; SetDirty(); }
        }

        /// <summary>
        /// The cache of _easeCurve parameters to be used in the approximate arc length reparameterization algorithm.
        /// For each _easeCurve _nSamples are saved.
        /// </summary>
        private float[] m_tSample;

        /// <summary>
        /// The cache of m_tSample to _sSample slopes to be used in the approximate arc length reparameterization algorithm.
        /// For each _easeCurve _nSamples are saved.
        /// </summary>
        private float[] m_tsSlope;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Spline"/> is loop.
        /// If set to true, it welds the first and last point of the spline.
        /// </summary>
        /// <value>
        ///   <c>true</c> if loop; otherwise, <c>false</c>.
        /// </value>
        public bool Loop
        {
            get
            {
                return m_loop;
            }
            set
            {
                m_loop = value;
                if (value == true)
                {
                    if(m_modes != null && m_modes.Length > 0)
                    {
                        m_modes[m_modes.Length - 1] = m_modes[0];
                    }
                    SetControlPoint(0, m_points[0]);
                }
            }
        }

        protected virtual void Awake() { 
            if (m_id == null)
            {
                m_id = GetHashCode().ToString();
            }
        }

        protected virtual void Start() { 
            _UpdateCache();
        }


        private void _UpdateCache()
        {
            _UpdateLengths();
            _InitSamples();
            _UpdateOrientations();
        }

        //private bool m_isInit;
        //public bool IsInit
        //{
        //    get { return m_isInit; }
        //}

        /// <summary>
        /// 按点集合创建spline
        /// </summary>
        /// <param name="RawPoints">点集合, 符合规则点数量为 (n*3)+1 (例: 4,7,10,13,16 ...)</param>
        public bool Init(Vector3[] RawPoints)
        {
            if(RawPoints == null 
                || RawPoints.Length < 4
                )
                return false;

            m_points = RawPoints;

            m_modes = new BezierControlPointMode[Mathf.CeilToInt((float)RawPoints.Length / 3)];
            for(int i = 0; i < m_modes.Length; i++)
            {
                m_modes[i] = BezierControlPointMode.Smooth;
            }

            //m_isInit = true;
            return true;
        }

        public bool Init(Vector3[] RawPoints, BezierControlPointMode[] modes)
        {

            if(RawPoints == null 
                || RawPoints.Length < 4
                || modes == null
                || Mathf.CeilToInt((float)RawPoints.Length / 3) != modes.Length
                )
                return false;

            m_points = RawPoints;
            m_modes = modes;
            //m_isInit = true;
            return true;
        }

        public bool Init(Vector3[] RawPoints, BezierControlPointMode[] modes, bool loop)
        {

            if(RawPoints == null
                || RawPoints.Length < 4
                || modes == null
                || Mathf.CeilToInt((float)RawPoints.Length / 3) != modes.Length
                )
                return false;

            m_points = RawPoints;
            m_modes = modes;
            m_loop = loop;
            //m_isInit = true;
            return true;
        }

        public void CopyForm(Spline src)
        {
            m_points = src.m_points;
            m_modes = src.m_modes;
        }

        /// <summary>
        /// Gets the total length of the _easeCurve.
        /// </summary>
        /// <param name="curveIndex">The _easeCurve index.</param>
        /// <returns>The length of the _easeCurve.</returns>
        private float _CurveLength(int curveIndex)
        {
            _UpdateLengths();

            return m_curveLengths[curveIndex];
        }

        /// <summary>
        /// Generates the _easeCurve lengths and arc lengths cache.
        /// </summary>
        /// <remarks>
        /// For each bezier _easeCurve of the spline its length is stored in _curveLengths and the arc length
        /// from the spline start to the _easeCurve end is stored in _arcLenghts.
        /// </remarks>
        private void _UpdateLengths()
        {
           // if (m_curveLengths != null && m_curveLengths.Length != 0) return;

            m_curveLengths = new float[curveCount];
            m_arcLengths = new float[curveCount + 1];

            float arcLen = 0;

            for (int i = 0; i < curveCount; i++)
            {
                m_curveLengths[i] = _GetCurveLength(i);
                m_arcLengths[i] = arcLen;
                arcLen += m_curveLengths[i];
            }

            m_arcLengths[curveCount] = arcLen;
        }

        private void _UpdateOrientations()
        {
            if (m_orientationVectors != null) return;

            // Compute initial orientation
            Vector3 tangent = GetDirection(0);
            Vector3 binormal, upVector;

            if (Vector3.Dot(tangent, Vector3.up) < 0.9f)
            {
                binormal = Vector3.Cross(Vector3.up, tangent);
                upVector = Vector3.Cross(tangent, binormal);
            }
            else if (Vector3.Dot(tangent, Vector3.right) < 0.9f)
            {
                upVector = Vector3.Cross(tangent, Vector3.right);
            }
            else
            {
                upVector = Vector3.Cross(tangent, Vector3.forward);
            }

            int nSamples = (int)(Length / SamplesDistance);

            m_orientationVectors = new Vector3[nSamples + 1];
            m_orientationVectors[0] = upVector;

            //for (int i = 1; i < nSamples; i++)
            //{

            //}
        }

        /// <summary>
        /// 获取Spline原始点数据
        /// </summary>
        public bool TryGetControlPoint(int index, out Vector3 rawPoint)
        {
            if(index >= 0 && index < m_points.Length)
            {
                rawPoint = m_points[index];
                return true;
            }
            rawPoint = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Gets the control point.
        /// </summary>
        /// <param name="pointIndex">The control point index.</param>
        /// <returns>The control point vector</returns>
        public Vector3 GetControlPoint(int pointIndex)
        {
            return m_points[pointIndex];
        }

        /// <summary>
        /// Sets the value for the control point at index position.
        /// </summary>
        /// <param name="index">The index of the control point.</param>
        /// <param name="point">The control point value.</param>
        public void SetControlPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                Vector3 delta = point - m_points[index];
                if (m_loop)
                {
                    if (index == 0)
                    {
                        m_points[1] += delta;
                        m_points[m_points.Length - 2] += delta;
                        m_points[m_points.Length - 1] = point;
                    }
                    else if (index == m_points.Length - 1)
                    {
                        m_points[0] = point;
                        m_points[1] += delta;
                        m_points[index - 1] += delta;
                    }
                    else
                    {
                        m_points[index - 1] += delta;
                        m_points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        m_points[index - 1] += delta;
                    }
                    if (index + 1 < m_points.Length)
                    {
                        m_points[index + 1] += delta;
                    }
                }
            }
            m_points[index] = point;
            _EnforceMode(index);

            SetDirty();
        }

        public void SetControlPointRaw(int index, Vector3 point)
        {
            m_points[index] = point;
            SetDirty();
        }

        /// <summary>
        /// Gets the control point _mode.
        /// </summary>
        /// <param name="pointIndex">The control point index.</param>
        /// <returns>The control point _mode.</returns>
        public BezierControlPointMode GetControlPointMode(int pointIndex)
        {
            if (m_modes == null || m_modes.Length == 0)
            {
                return BezierControlPointMode.Smooth;
            }
            return m_modes[(pointIndex + 1) / 3];
        }

        /// <summary>
        /// Sets the control point _mode at index position.
        /// </summary>
        /// <param name="index">The index of the control point.</param>
        /// <param name="_mode">The _mode.</param>
        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {

            if(m_modes == null || m_modes.Length == 0)
                return;

            int modeIndex = (index + 1) / 3;
            m_modes[modeIndex] = mode;
            if (m_loop)
            {
                if (modeIndex == 0)
                {
                    m_modes[m_modes.Length - 1] = mode;
                }
                else if (modeIndex == m_modes.Length - 1)
                {
                    m_modes[0] = mode;
                }
            }
            _EnforceMode(index);

            SetDirty();
        }

        /// <summary>
        /// Updates the handles (control points) of pointIndex according to current point _mode.
        /// </summary>
        /// <param name="pointIndex">The point index.</param>
        private void _EnforceMode(int pointIndex)
        {
            int modeIndex = (pointIndex + 1) / 3;
            //BezierControlPointMode mode = m_modes[modeIndex];
            BezierControlPointMode mode = GetControlPointMode(pointIndex);
            if (mode == BezierControlPointMode.Corner || !m_loop && (modeIndex == 0 || modeIndex == m_modes.Length - 1))
            {
                return;
            }

            int middleIndex = modeIndex * 3;
            int fixedIndex, enforcedIndex;
            if (pointIndex <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                {
                    fixedIndex = m_points.Length - 2;
                }
                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= m_points.Length)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= m_points.Length)
                {
                    fixedIndex = 1;
                }
                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = m_points.Length - 2;
                }
            }

            Vector3 middle = m_points[middleIndex];
            Vector3 enforcedTangent = middle - m_points[fixedIndex];
            if (mode == BezierControlPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, m_points[enforcedIndex]);
            }
            m_points[enforcedIndex] = middle + enforcedTangent;
        }

        /// <summary>
        /// Gets the number of bezier curves in the spline.
        /// </summary>
        /// <value>
        /// The _easeCurve count.
        /// </value>
        public int curveCount
        {
            get
            {
                return (m_points.Length - 1) / 3;
            }
        }

        /// <summary>
        /// Gets the _easeCurve vector at t parameter.
        /// </summary>
        /// <param name="t">The t parameter.</param>
        /// <returns>Curve position vector</returns>
        public Vector3 GetPoint(float t)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = m_points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * curveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }

            return transform.TransformPoint(Bezier.GetPoint(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t));
        }

        /// <summary>
        /// Gets the _easeCurve vector for the _easeCurve section from startIndex to endIndex at t parameter.
        /// </summary>
        /// <param name="startIndex">The start index of the _easeCurve section.</param>
        /// <param name="endIndex">The end index of the _easeCurve section.</param>
        /// <param name="t">The t parameter for the _easeCurve section, t in [0, 1].</param>
        /// <returns>The _easeCurve vector position for the _easeCurve section at t.</returns>
        public Vector3 GetPoint(int startIndex, int endIndex, float t)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = (endIndex - 1) * 3;
            }
            else
            {
                t = Mathf.Clamp01(t) * (endIndex - startIndex);
                i = (int)t;
                t -= i;
                i = (startIndex + i) * 3;
            }

            return transform.TransformPoint(Bezier.GetPoint(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t));
        }

        /// <summary>
        /// Gets the velocity of the _easeCurve at parameter t.
        /// </summary>
        /// <param name="t">The t parameter.</param>
        /// <returns>The velocity vector at t position.</returns>
        public Vector3 GetVelocity(float t)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = m_points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * curveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }
            return transform.TransformPoint(Bezier.GetFirstDerivative(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t)) - transform.position;
        }

        /// <summary>
        /// Gets the velocity of the _easeCurve section from start index to end index at t parameter.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="t">The t parameter for the _easeCurve section, t in [0, 1].</param>
        /// <returns>The velocity vector.</returns>
        public Vector3 GetVelocity(int startIndex, int endIndex, float t)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = (endIndex - 1) * 3;
            }
            else
            {
                t = Mathf.Clamp01(t) * (endIndex - startIndex);
                i = (int)t;
                t -= i;
                i = (startIndex + i) * 3;
            }

            return transform.TransformPoint(Bezier.GetFirstDerivative(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t)) - transform.position;
        }

        /// <summary>
        /// Gets the _easeCurve acceleration at t.
        /// </summary>
        /// <param name="t">The _easeCurve position parameter.</param>
        /// <returns>The acceleration vector</returns>
        public Vector3 GetAcceleration(float t)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = m_points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * curveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }
            return transform.TransformPoint(Bezier.GetSecondDerivative(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t)) - transform.position;
        }

        /// <summary>
        /// Gets the acceleration.
        /// </summary>
        /// <param name="startIndex">The start knot index.</param>
        /// <param name="endIndex">The end knot index.</param>
        /// <param name="t">The _easeCurve position parameter.</param>
        /// <returns>The acceleration vector.</returns>
        public Vector3 GetAcceleration(int startIndex, int endIndex, float t)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = (endIndex - 1) * 3;
            }
            else
            {
                t = Mathf.Clamp01(t) * (endIndex - startIndex);
                i = (int)t;
                t -= i;
                i = (startIndex + i) * 3;
            }

            return transform.TransformPoint(Bezier.GetSecondDerivative(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t)) - transform.position;
        }

        /// <summary>
        /// Gets the direction normalized vector of the _easeCurve at t position.
        /// </summary>
        /// <param name="t">The _easeCurve parameter t in [0, 1].</param>
        /// <returns>Normalized direction vector.</returns>
        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        /// <summary>
        /// Gets the direction normalized vector of the _easeCurve section from startIndex to endIndex at t position.
        /// </summary>
        /// <param name="startIndex">The start index of the _easeCurve section.</param>
        /// <param name="endIndex">The end index of the _easeCurve section.</param>
        /// <param name="t">The t parameter for the _easeCurve section, t in [0, 1].</param>
        /// <returns>Normalized direction vector.</returns>
        public Vector3 GetDirection(int startIndex, int endIndex, float t)
        {
            return GetVelocity(startIndex, endIndex, t).normalized;
        }

        /// <summary>
        /// Add a point to the spline. 
        /// </summary>
        /// <remarks>
        /// Depending on the current selected point, the new point will be added between two points, or at the end of the _easeCurve.
        /// If there's no point selected, or the selected point is the last point, the new point will be added at the end of the _easeCurve, at a distance of 1 in the direction of the last point of the spline.
        /// If the selected point is not the last point, the new point will be created between previous and next point. The bezier handles of the new, of the previous and of the next point will be created/modified in order to maintain the same spline shape.
        /// </remarks>
        /// <param name="selectedIndex">The current selected control point index.</param>
        public void AddPoint(int selectedIndex)
        {
            // Add point to the end of the spline
            if (selectedIndex == -1 || selectedIndex == m_points.Length - 1 || selectedIndex % 3 != 0)
            {
                Vector3 direction = Bezier.GetFirstDerivative(m_points[curveCount * 3 - 3], m_points[curveCount * 3 - 2], m_points[curveCount * 3 - 1], m_points[curveCount * 3], 1).normalized;
                Vector3 point = m_points[m_points.Length - 1];
                Array.Resize(ref m_points, m_points.Length + 3);

                point += direction;
                m_points[m_points.Length - 3] = point;
                point += direction;
                m_points[m_points.Length - 2] = point;
                point += direction;
                m_points[m_points.Length - 1] = point;

                Array.Resize(ref m_modes, m_modes.Length + 1);
                m_modes[m_modes.Length - 1] = BezierControlPointMode.Corner;
                m_modes[m_modes.Length - 2] = BezierControlPointMode.Aligned;
                _EnforceMode(m_points.Length - 4);

                if (m_loop)
                {
                    m_points[m_points.Length - 1] = m_points[0];
                    m_modes[m_modes.Length - 1] = m_modes[0];
                    _EnforceMode(0);
                }
            }
            // Add point between two points
            else if (selectedIndex % 3 == 0)
            {
                float s = 0;
                for (int i = 0; i < selectedIndex / 3; i++) s += _CurveLength(i);
                s += _CurveLength(selectedIndex / 3) / 2;

                float t0 = GetArcLengthParameter(s) * curveCount - selectedIndex / 3;

                Vector3 point = Bezier.GetPoint(m_points[selectedIndex], m_points[selectedIndex + 1], m_points[selectedIndex + 2], m_points[selectedIndex + 3], t0);
                Vector3 direction = Bezier.GetFirstDerivative(m_points[selectedIndex], m_points[selectedIndex + 1], m_points[selectedIndex + 2], m_points[selectedIndex + 3], t0);

                Array.Resize(ref m_points, m_points.Length + 3);
                for (int i = m_points.Length - 1; i >= selectedIndex + 5; i--)
                    m_points[i] = m_points[i - 3];

                int newIndex = selectedIndex + 3;

                m_points[newIndex] = point;
                m_points[newIndex - 1] = point - direction * t0 / 3;
                m_points[newIndex - 2] = m_points[newIndex - 3] + (m_points[newIndex - 2] - m_points[newIndex - 3]) * t0;

                m_points[newIndex + 1] = point + direction * (1 - t0) / 3;
                m_points[newIndex + 2] = m_points[newIndex + 3] + (m_points[newIndex + 2] - m_points[newIndex + 3]) * (1 - t0);

                Array.Resize(ref m_modes, m_modes.Length + 1);
                for (int i = m_modes.Length - 1; i >= selectedIndex / 3 + 1; i--)
                    m_modes[i] = m_modes[i - 1];

                m_modes[selectedIndex / 3] = BezierControlPointMode.Corner;
                m_modes[selectedIndex / 3 + 1] = BezierControlPointMode.Aligned;
                m_modes[selectedIndex / 3 + 2] = BezierControlPointMode.Corner;
            }

            SetDirty();
        }

        /// <summary>
        /// Removes a value from an array and returns the new array.
        /// </summary>
        /// <typeparam name="T">The array ResType.</typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="index">The index value to remove.</param>
        /// <returns>The new array with the removed value.</returns>
        private static T[] _RemoveAt<T>(T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        /// <summary>
        /// Removes a range of values from an array and returns the new array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="start">The start index.</param>
        /// <param name="end">The end index.</param>
        /// <returns>The new array with the range of values removed.</returns>
        private static T[] _RemoveRange<T>(T[] source, int start, int end)
        {
            if (end < start) return source;

            T[] dest = new T[source.Length - (end - start) - 1];

            if (start > 0)
                Array.Copy(source, 0, dest, 0, start);

            if (end < source.Length - 1)
                Array.Copy(source, end + 1, dest, start, source.Length - end - 1);

            return dest;
        }

        /// <summary>
        /// Deletes the point.
        /// </summary>
        /// <param name="pointIndex">Index of the point to delete.</param>
        public void DeletePoint(int pointIndex)
        {
            if (curveCount < 2) return;

            if (pointIndex != 0 && pointIndex != m_points.Length - 1)
                m_points = _RemoveRange(m_points, pointIndex - 1, pointIndex + 1);
            else if (pointIndex == 0)
                m_points = _RemoveRange(m_points, pointIndex, pointIndex + 2);
            else
                m_points = _RemoveRange(m_points, pointIndex - 2, pointIndex);

            m_modes = _RemoveAt(m_modes, (pointIndex + 1) / 3);

            SetDirty();
        }

        /// <summary>
        /// Empties the caches of _easeCurve lenghts and _easeCurve samples for arc length reparameterization.
        /// </summary>
        /// <remarks>
        /// The next time that a _easeCurve length will be requested, the arc length cache will be regenerated.
        /// The next time that an approximate arc length reparameterizationi will be requested, the arc length samples will be regenerated.
        /// This function is called automatically whenever a change is made to the spline (point added/deleted/moved, ...)
        /// </remarks>
        public void SetDirty(bool lenghts = true, bool orientations = true)
        {
            if (lenghts)
            {
                m_curveLengths = null;
                m_tSample = null;
            }

            if (orientations)
            {
                m_orientationVectors = null;
            }
        }

        public float GetProgressAtSpeedByTime(float currProgress, float velocity, int direction = 1)
        {
            return GetProgressAtSpeed(currProgress, velocity, Time.deltaTime, direction);
        }

        /// <summary>
        /// Update _easeCurve parameter progress in [0, 1] at constant speed using _easeCurve velocity (first derivative).
        /// </summary>
        /// <remarks>
        /// This function is useful for walking on the _easeCurve at fixed speed without arc length reparameterization. It must be called for each frame.
        /// </remarks>
        /// <param name="currProgress">The current _easeCurve parameter.</param>
        /// <param name="velocity">The velocity.</param>
        /// <param name="direction">The speed direction.</param>
        /// <returns>The new _easeCurve parameter</returns>
        public float GetProgressAtSpeed(float currProgress, float velocity, float deltaTime, int direction = 1)
        {
            float step = 1.0f / curveCount;
            float nextT = 0;

            float v = GetVelocity(currProgress).magnitude;

            if(v.Equals(0))
                v = 1.0f;

            nextT = currProgress + deltaTime * velocity / v * step * direction;

            if (direction == 1)
            {
                if (nextT > 1) nextT = 1;

                // Curve crossing
                if (nextT != 1 && currProgress % step > nextT % step)
                {
                    int nextCurve = (int)(nextT / step);
                    float currStep = nextCurve * step;
                    float perc = (currStep - currProgress) / (nextT - currProgress);

                    nextT = currStep + deltaTime * (1 - perc) * velocity / GetVelocity(nextCurve, nextCurve + 1, 0).magnitude * step;
                }

                return nextT;
            }
            else
            {
                if (nextT < 0) nextT = 0;

                // Curve crossing
                if (nextT != 0 && currProgress % step < nextT % step)
                {
                    int nextCurve = (int)(currProgress / step);
                    float currStep = nextCurve * step;
                    float perc = (currProgress - currStep) / (currProgress - nextT);

                    nextT = currStep - deltaTime * (1 - perc) * velocity / GetVelocity(nextCurve - 1, nextCurve, 1).magnitude * step;
                }

                return nextT;
            }
        }

        public float GetProgressAtSpeedByFixTime(float currProgress, float velocity, int direction = 1)
        {
            return GetProgressAtSpeed(currProgress, velocity, Time.fixedDeltaTime, direction);
        }

        /// <summary>
        /// This Delegate is called when the spline walk is complete.
        /// </summary>
        public delegate void WalkCompleteFunction();

        /// <summary>
        /// This delegate is called on each frame during a spline walk.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="progress">The progress.</param>
        public delegate void WalkUpdateFunction(Vector3 position, float progress);

        /// <summary>
        /// This is a coroutine for walking a spline section at constant speed in a defined time _velocity.
        /// </summary>
        /// <remarks>
        /// Based on the specified _velocity and the length of the specified spline section it calculates the required speed and then calls WalkAtSpeed.
        /// </remarks>
        /// <param name="startIndex">The start index of the spline section. Use start index greater than end index to invert the direction of motion.</param>
        /// <param name="endIndex">The end index of the spline section. Use end index lesser than start index to invert the direction of motion.</param>
        /// <param name="_velocity">The desired _velocity to cover the spline section.</param>
        /// <param name="transform">An optional transform object where to apply motion.</param>
        /// <param name="_mode">The SplineWalkerMode: once, loop or ping pong.</param>
        /// <param name="lookForward">If set to <c>true</c> the transform rotation is set to _easeCurve direction.</param>
        /// <param name="completeFunction">This function will be called upon motion completion (only form SplineWalkerMode.Once).</param>
        /// <param name="updateFunction">This function will be called every frame.</param>
        /// <seealso cref="WalkAtSpeed"/>
        public IEnumerator WalkDuration(
            int startIndex, int endIndex,
            float duration, Transform transform, SplineWalkerMode mode = SplineWalkerMode.Once, Boolean lookForward = true,
            WalkCompleteFunction completeFunction = null, WalkUpdateFunction updateFunction = null)
        {
            _UpdateLengths();

            // Calculate length from startPoint to endPoint
            int a = startIndex, b = endIndex;
            if (a > b)
            {
                int tmp = a;
                a = b;
                b = tmp;
            }

            float l = m_arcLengths[b] - m_arcLengths[a];

            return WalkAtSpeed(startIndex, endIndex, l / duration, transform, mode, lookForward, completeFunction, updateFunction);
        }

        /// <summary>
        /// This is a coroutine for walking a spline section at constant speed. 
        /// </summary>
        /// <remarks>
        /// It uses the function GetProgressAtSpeed in order to update the _easeCurve parameter at each frame mantaining a constant speed.
        /// </remarks>
        /// <param name="startIndex">The start index of the spline section. Use start index greater than end index to invert the direction of motion.</param>
        /// <param name="endIndex">The end index of the spline section. Use end index lesser than start index to invert the direction of motion.</param>
        /// <param name="velocity">The desired velocity.</param>
        /// <param name="transform">An optional transform object where to apply motion.</param>
        /// <param name="_mode">The SplineWalkerMode: once, loop or ping pong.</param>
        /// <param name="lookForward">If set to <c>true</c> the transform rotation is set to _easeCurve direction.</param>
        /// <param name="completeFunction">This function will be called upon motion completion (only form SplineWalkerMode.Once).</param>
        /// <param name="updateFunction">This function will be called every frame.</param>
        /// <seealso cref="WalkDuration"/>
        public IEnumerator WalkAtSpeed(
            int startIndex, int endIndex,
            float velocity, Transform transform = null, SplineWalkerMode mode = SplineWalkerMode.Once, Boolean lookForward = true,
            WalkCompleteFunction completeFunction = null, WalkUpdateFunction updateFunction = null)
        {
            float progress = startIndex / (float)curveCount;
            float limit = endIndex / (float)curveCount;

            yield return GetPoint(progress);

            int direction = endIndex > startIndex ? 1 : -1;

            while (true)
            {
                progress = GetProgressAtSpeed(progress, velocity, direction);

                if ((direction == 1 && progress >= limit) ||
                     (direction == -1 && progress <= limit))
                {
                    if (mode == SplineWalkerMode.Once)
                        break;
                    else if (mode == SplineWalkerMode.PingPong)
                    {
                        direction *= -1;

                        if (direction * (endIndex - startIndex) > 0)
                            limit = endIndex / (float)curveCount;
                        else
                            limit = startIndex / (float)curveCount;

                        continue;
                    }
                    else if (mode == SplineWalkerMode.Loop)
                    {
                        progress -= limit - startIndex / (float)curveCount;

                        continue;
                    }
                }

                Vector3 position = GetPoint(progress);

                if (transform != null)
                {
                    transform.position = position;
                    if (lookForward) transform.LookAt(transform.position + GetDirection(progress));
                }

                if (updateFunction != null) updateFunction(position, progress);

                yield return null;
            }

            if (completeFunction != null) completeFunction();
        }

        /// <summary>
        /// Gets the _easeCurve position at the index point.
        /// </summary>
        /// <param name="pointIndex">The index of the control point.</param>
        /// <returns>The _easeCurve vector position.</returns>
        /// <seealso cref="GetPoint(float)"/>
        /// <seealso cref="GetPoint(int,int,float)"/>
        public Vector3 GetPointAtIndex(int pointIndex)
        {
            return transform.TransformPoint(m_points[pointIndex * 3]);
        }

        /// <summary>
        /// Gets the length of the get _easeCurve.
        /// </summary>
        /// <param name="curveIndex">The _easeCurve index.</param>
        /// <returns>The _easeCurve length.</returns>
        private float _GetCurveLength(int curveIndex)
        {
            int i = curveIndex * 3;
            return Bezier.Integrate(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], 0, 1);
        }

        /// <summary>
        /// Gets the arc length of the arc in the interval [t0, t1].         
        /// </summary>
        /// <remarks>
        /// This function calculates an integration for the first and the last _easeCurve pieces of the arc. 
        /// Then adds the precalculated arc lengths for the curves in the middle.
        /// </remarks>
        /// <param name="t0">The start parameter in [0, 1].</param>
        /// <param name="t1">The end parameter in [0, 1].</param>
        /// <returns>The arc length.</returns>
        public float GetArcLength(float t0, float t1)
        {
            _UpdateLengths();

            if (t0 == t1) return 0;

            if (t0 > t1)
            {
                float tmp = t0;
                t0 = t1;
                t1 = tmp;
            }

            int curve0 = (int)(t0 * curveCount);
            int curve1 = t1 == 1 ? curveCount - 1 : (int)(t1 * curveCount);

            t0 = t0 * curveCount - curve0;
            t1 = t1 * curveCount - curve1;

            curve0 *= 3;
            curve1 *= 3;

            if (curve0 == curve1)
                return Bezier.Integrate(m_points[curve0], m_points[curve0 + 1], m_points[curve0 + 2], m_points[curve0 + 3], t0, t1);
            else
            {
                float result = 0;

                result += Bezier.Integrate(m_points[curve0], m_points[curve0 + 1], m_points[curve0 + 2], m_points[curve0 + 3], t0, 1);

                for (int i = curve0 / 3 + 1; i < curve1 / 3; i++)
                    result += m_curveLengths[i];

                result += Bezier.Integrate(m_points[curve1], m_points[curve1 + 1], m_points[curve1 + 2], m_points[curve1 + 3], 0, t1);

                return result;
            }
        }

        /// <summary>
        /// Gets the arc length between two knots.         
        /// </summary>
        /// <param name="startKnot">The start knot.</param>
        /// <param name="endKnot">The end knot.</param>
        /// <returns>The arc length.</returns>
        public float GetArcLengthBetweenKnots(int startKnot, int endKnot)
        {
            _UpdateLengths();

            if (startKnot == endKnot) return 0;

            if (startKnot > endKnot)
            {
                int tmp = startKnot;
                startKnot = endKnot;
                endKnot = tmp;
            }

            float result = 0;

            for (int i = startKnot; i < endKnot; i++)
                result += m_curveLengths[i];

            return result;
        }

        /// <summary>
        /// Gets the _easeCurve parameter corresponding to the requested s arc length.
        /// </summary>
        /// <remarks>
        /// This method performs an accurate arc length reparameterization of the _easeCurve. It is computational expensive. Use <see cref="GetArcLengthParameterApproximate"/> for real-time.
        /// It uses numerical integration and root-finding algorithms in order to find the parameter value that gives an arc length of s.
        /// </remarks>
        /// <param name="s">The desired spline arc length 0 &lt;= s &lt;= length.</param>
        /// <param name="epsilon">The maximum error ds for the computed parameter.</param>
        /// <returns>The _easeCurve parameter that gives an arc length equal to s.</returns>
        /// <seealso cref="GetArcLengthParameterApproximate"/>
        public float GetArcLengthParameter(float s, float epsilon = 0.0001f)
        {
            _UpdateLengths(); // make sure to calculate lengths

            if (s <= 0) return 0;
            if (s >= Length) return 1;

            // find the _easeCurve index containing s arc length
            int curveIndex;
            for (curveIndex = 0; curveIndex < curveCount - 1; curveIndex++)
                if (s < m_arcLengths[curveIndex + 1])
                    break;

            float length0 = s - m_arcLengths[curveIndex]; // the arc length portion inside the _easeCurve
            float t0 = length0 / m_curveLengths[curveIndex]; // the candidate t parameter inside the _easeCurve

            int p0 = curveIndex * 3, p1 = p0 + 1, p2 = p1 + 1, p3 = p2 + 1;

            return (
                curveIndex +
                Bezier.GetArcLengthParameter(
                    m_points[p0], m_points[p1], m_points[p2], m_points[p3],
                    length0, t0, epsilon)) / curveCount;
        }

        /// <summary>
        /// Inits an array of samples (s, t, s-t slope) to be used in the approximate arc length 
        /// reparameterization function: GetArcLengthParameterApproximate.
        /// </summary>
        private void _InitSamples()
        {
            if (m_tSample != null && m_tSample.Length != 0) return; // Skip if already done.

            _UpdateLengths(); // Make sure we have _easeCurve lengths values

            int nSamples = (int)(Length / SamplesDistance);

            // Allocating arrays. We need nSamples plus a sample for index 0 and one for index nSamples + 1.
            m_tSample = new float[nSamples + 2];
            m_tsSlope = new float[nSamples + 2];

            // First samples
            m_tSample[0] = 0;
            m_tsSlope[0] = 0;

            for (int i = 1; i <= nSamples + 1; i++)
            {
                m_tSample[i] = GetArcLengthParameter(i * SamplesDistance);
                m_tsSlope[i] = (m_tSample[i] - m_tSample[i - 1]) / SamplesDistance;
            }
        }

        /// <summary>
        /// Gets the _easeCurve parameter corresponding to the requested s arc length.
        /// </summary>
        /// <remarks>
        /// This method performs an arc length reparameterization of the _easeCurve. It is an approximate computation, good for real time use. 
        /// It is based on precomputed samples of s and t obtained with the accurate function GetArcLengthParameter. 
        /// This function interpolates between samplad values to obtain an approximation of the arc length parameter.
        /// </remarks>
        /// <param name="s">The desired spline arc length 0 &lt;= s &lt;= length.</param>
        /// <returns>The _easeCurve parameter that gives an arc length near to s.</returns>
        /// /// <seealso cref="GetArcLengthParameter"/>
        public float GetArcLengthParameterApproximate(float s)
        {
            _InitSamples(); // Make sure that samples have been calculated

            if (s <= 0) { return 0; }
            if (s >= Length) { return 1; }

            int sampleIndex = (int)(s / SamplesDistance);

            // Return linear interpolation between sampleIndex and sampleIndex + 1 
            return m_tSample[sampleIndex] + m_tsSlope[sampleIndex + 1] * (s % SamplesDistance);
        }

        public Vector3[] GetSubdivisionB(float s0, float s1)
        {
            Vector3[] result = new Vector3[4];

            float t0 = GetArcLengthParameter(s0);
            float t1 = GetArcLengthParameter(s1);

            Vector3 d0 = GetVelocity(t0).normalized;
            Vector3 d1 = -GetVelocity(t1).normalized;

            Vector3 p0 = GetPoint(t0);
            Vector3 p3 = GetPoint(t1) - p0;

            float l = s1 - s0;

            Vector3 pm = GetPoint(GetArcLengthParameter((s0 + s1) / 2)) - p0;

            float k0 = (8 * pm.y - 4 * p3.y - 2 * d1.y * l) / 3 / (d0 - d1).y;
            //float k0 = (8 * pm.x - 4 * p3.x - 2 * d1.x * l) / ((d0.x - d1.x) * 3);
            //float k0 = (8 * pm.z - 4 * p3.z - 2 * d1.z * l) / 3 / (d0 - d1).z;

            float k1 = 2 * l / 3 - k0;

            result[0] = p0;
            result[1] = p0 + d0 * k0;
            result[3] = p3 + p0;
            result[2] = result[3] + d1 * k1;

            return result;
        }

        public Vector3[] GetSubdivision(float s0, float s1)
        {
            Vector3[] result = new Vector3[4];

            float t0 = GetArcLengthParameter(s0);
            float t1 = GetArcLengthParameter(s1);

            Vector3 v0 = GetVelocity(t0);
            Vector3 v1 = GetVelocity(t1);

            result[0] = GetPoint(t0);
            result[3] = GetPoint(t1);

            if (Mathf.Floor(t0 * curveCount) == Mathf.Floor(t1 * curveCount))
            {
                result[1] = result[0] + v0 * (t1 - t0) / 3 * curveCount;
                result[2] = result[3] - v1 * (t1 - t0) / 3 * curveCount;
            }
            else
            {
                int kn0 = (int)Mathf.Floor(t0 * curveCount);
                int kn1 = (int)Mathf.Floor(t1 * curveCount);

                float l0 = GetArcLengthBetweenKnots(kn0, kn0 + 1);
                float l1 = GetArcLengthBetweenKnots(kn1, kn1 + 1);

                result[1] = result[0] + v0 / 3 * (s1 - s0) / l0;
                result[2] = result[3] - v1 / 3 * (s1 - s0) / l1;
            }

            return result;
        }

        public string ToStringList()
        {
            return SplineUtils.ToStringList(m_points, m_modes);
        }

        public string ToJsonString()
        {
            return SplineUtils.ToJsonString(m_loop, m_points, m_modes);
        }


#region Obsolete

        [Obsolete("已迁移为Id")]
        public string id => Id;

        [Obsolete("已迁移为Loop")]
        public bool loop => Loop;

        [Obsolete("已迁移为SamplesDistance")]
        public float samplesDistance => SamplesDistance;

        [Obsolete("已迁移为ToStringList")]
        public string toStringList()
        {
            return ToStringList();
        }

        [Obsolete("已迁移为ToJsonString")]
        public string toJsonString()
        {
            return ToJsonString();
        }

#endregion


    }

}
