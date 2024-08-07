using System;
using System.Text;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal.module
{
    public class SplineUtils
    {

        public static string ToStringList(Vector3[] m_points, BezierControlPointMode[] m_modes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int i, length = m_points != null ? m_points.Length : 0;
            for(i = 0; i < length; i++)
            {
                if(i > 0)
                    stringBuilder.Append(",");
                stringBuilder.Append(m_points[i].x + ","
                  + m_points[i].y + ","
                  + m_points[i].z);
            }
            stringBuilder.Append("|");
            length = m_modes != null ? m_modes.Length : 0;
            for(i = 0; i < length; i++)
            {
                if(i > 0)
                    stringBuilder.Append(",");
                stringBuilder.Append((int)m_modes[i]);
            }
            return stringBuilder.ToString();
        }

        public static bool SplineInitByStringList(Spline spline, string StringList)
        {

            Vector3[] points = null;
            BezierControlPointMode[] modes = null;

            string[] tmp = StringList.Split('|');
            tmp[0] = tmp[0].Trim();
            int i, length;
            if(!string.IsNullOrEmpty(tmp[0]))
            {
                string[] pointStrs = tmp[0].Split(',');

                if(pointStrs.Length % 3 != 0)
                {
                    return false;
                }

                length = pointStrs.Length / 3;

                points = new Vector3[length];

                for(i = 0; i < length; i++)
                {
                    points[i] = new Vector3();
                    points[i].x = float.Parse(pointStrs[i * 3]);
                    points[i].y = float.Parse(pointStrs[i * 3 + 1]);
                    points[i].z = float.Parse(pointStrs[i * 3 + 2]);
                }
            }

            tmp[1] = tmp[1].Trim();
            if(!string.IsNullOrEmpty(tmp[1]))
            {
                string[] modeStrs = tmp[1].Split(',');
                length = modeStrs.Length;
                modes = new BezierControlPointMode[length];
                for( i = 0; i < length; i++)
                {
                    modes[i] = (BezierControlPointMode)int.Parse(modeStrs[i]);
                }
            }

            if(modes != null && modes.Length > 0)
            {
                return spline.Init(points, modes);
            }
            else
            {
                return spline.Init(points);
            }
        }

        [Serializable]
        private class SplineJsonWraper
        {

            public SplineJsonWraper(bool loop, Vector3[] points, BezierControlPointMode[] modes)
            {
                m_loop = loop;
                m_points = points;
                m_modes = modes;
            }

            public bool m_loop;
            public Vector3[] m_points;
            public BezierControlPointMode[] m_modes;
        }

        public static string ToJsonString(bool loop, Vector3[] m_points, BezierControlPointMode[] m_modes)
        {
            // 老方法
            //StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append("{\"points\":[");
            //int i, length = m_points != null ? m_points.Length : 0;
            //for(i = 0; i < length; i++)
            //{
            //    if(i > 0)
            //        stringBuilder.Append(",");
            //    stringBuilder.Append("{\"x\":\"" + m_points[i].x + "\","
            //      + "\"y\":\"" + m_points[i].y + "\","
            //      + "\"z\":\"" + m_points[i].z + "\"}");
            //}
            //stringBuilder.Append("],\"modes\":[");
            //length = m_modes != null ? m_modes.Length : 0;
            //for(i = 0; i < length; i++)
            //{
            //    if(i > 0)
            //        stringBuilder.Append(",");
            //    stringBuilder.Append((int)m_modes[i]);
            //}
            //stringBuilder.Append("]}");
            //return stringBuilder.ToString();

            return UnityEngine.JsonUtility.ToJson(new SplineJsonWraper(loop, m_points, m_modes));
        }

        public static bool SplineInitByJSON(Spline spline, string JsonStr)
        {
            SplineJsonWraper sjw = UnityEngine.JsonUtility.FromJson<SplineJsonWraper>(JsonStr);
            if(sjw != null)
            {
                return spline.Init(sjw.m_points, sjw.m_modes, sjw.m_loop);
            }
            return false;
        }

        /// <summary>
        /// 根据点集合(锚点集合)快速生成Spline
        /// </summary>
        public static void  FastBuildingUseBigPoints(Vector3[] bigPoints, Spline target)
        {

            if(bigPoints == null || bigPoints.Length < 2)
                return;

            List<Vector3> ptList = new List<Vector3>();
            int i, len = bigPoints.Length;
            for(i = 1; i < len; i++)
            {
                Vector3 s = bigPoints[i - 1];
                Vector3 e = bigPoints[i];
                Vector3 c1 = Vector3.Lerp(s, e, 0.3f);
                Vector3 c2 = Vector3.Lerp(s, e, 0.7f);
                if(i == 1)
                    ptList.Add(s);
                ptList.Add(c1);
                ptList.Add(c2);
                ptList.Add(e);
            }

            len = ptList.Count;
            for(i = 0; i < ptList.Count; i++)
            {
                if(i % 3 == 2)
                {
                    int midIdx = (i + 1) / 3;
                    int pIdx = midIdx - 1;
                    int nIdx = midIdx + 1;
                    if(pIdx >=0 && pIdx < bigPoints.Length && nIdx >= 0  && nIdx < bigPoints.Length)
                    {
                        Vector3 s = bigPoints[pIdx];
                        Vector3 e = bigPoints[nIdx];
                        Vector3 m = bigPoints[midIdx];

                        Vector3 offset = e - Vector3.Lerp(s, e, 0.7f);
                        ptList[i] = m - offset;
                    }

                }
            }

            target.Init(ptList.ToArray());
            target.SetDirty(true, true);
            len = ptList.Count;
            for(i = 0; i < len; i++)
            {
                if(i % 3 == 0)
                {
                    target.SetControlPoint(i, ptList[i]);
                }
            }
        }

        /// <summary>
        /// 根据spA和spB创建拼接Spline
        /// </summary>
        public static void BuildingSplicingSpline(Spline spA, Spline spB, Spline target)
        {
            Vector3 s = spA.transform.position + spA.GetControlPoint(spA.ControlPointLength - 1);
            Vector3 c1 = spA.transform.position + spA.GetControlPoint(spA.ControlPointLength - 2);
            c1 = s * 2 - c1;
            Vector3 c2 = spB.transform.position + spB.GetControlPoint(1);
            Vector3 e = spB.transform.position + spB.GetControlPoint(0);
            c2 = e * 2 - c2;

            target.Init(new Vector3[]
            {
                s - target.transform.position,
                c1 - target.transform.position,
                c2 - target.transform.position,
                e - target.transform.position
            });
            target.SetDirty(true, true);
        }

        /// <summary>
        /// 合并Spline
        /// </summary>
        public static void MergeSplinesToNewSpline(Spline[] splines, Spline target)
        {
            List<Vector3> wPosList = new List<Vector3>();
            for(int i = 0; i < splines.Length; i++)
            {
                Spline spline = splines[i];
                int clen = spline.ControlPointLength;
                for(int j = 0; j < clen; j++)
                {
                    if(i == 0)
                    {
                        wPosList.Add(spline.transform.position + spline.GetControlPoint(j) - target.transform.position);
                    }else if(i >0 && j > 0)
                    {
                        wPosList.Add(spline.transform.position + spline.GetControlPoint(j) - target.transform.position);
                    }
                }
            }
            target.Init(wPosList.ToArray());
            target.SetDirty(true, true);
        }

        /// <summary>
        /// 使用桥接方式合并Spline
        /// </summary>
        public static void MergeSplinesToNewSplineWithBridge(Spline[] splines, Spline target, float birdgeThreshold = 0.3f)
        {
            List<Spline> allSplineList = new List<Spline>();
            List<Spline> temp = new List<Spline>();
            int i, len = splines.Length;
            for(i = 0; i < len; i++)
            {
                if(i > 0)
                {
                    Spline p = splines[i - 1];
                    Spline n = splines[i];
                    Vector3 p1 = p.GetPoint(1.0f);
                    Vector3 p2 = n.GetPoint(0.0f);
                    if(Vector3.Distance(p1,p2) > birdgeThreshold)
                    {
                        Spline t = Spline.Create();
                        t.gameObject.hideFlags = HideFlags.HideAndDontSave;
                        SplineUtils.BuildingSplicingSpline(p, n, t);
                        temp.Add(t);
                        allSplineList.Add(t);
                    }
                }
                allSplineList.Add(splines[i]);
            }
            MergeSplinesToNewSpline(allSplineList.ToArray(), target);
            //remove
            if(temp.Count > 0)
            {
                len = temp.Count;
                for(i = 0; i < len; i++)
                {
                    GameObject.DestroyImmediate(temp[i].gameObject);
                }
            }
        }

        //--------------------------------------------------

        /// <summary>
        /// 三维空间点到直线的垂足
        /// </summary>
        /// <param name="pt">直线外一点</param>
        /// <param name="begin">直线开始点</param>
        /// <param name="end">直线结束点</param>
        /// <returns></returns>
        public Vector3 GetFootOfPerpendicular(Vector3 pt,Vector3 begin,Vector3 end)
        {
            Vector3 retVal;
            float dx = begin.x - end.x;
            float dy = begin.y - end.y;
            float dz = begin.z - end.z;
	        if(Mathf.Abs(dx) < 0.00000001 && Mathf.Abs(dy) < 0.00000001 && Mathf.Abs(dz) < 0.00000001 )
	        {
		        retVal = begin;
		        return retVal;
	        }

            float u = (pt.x - begin.x) * (begin.x - end.x) +
                (pt.y - begin.y) * (begin.y - end.y) + (pt.z - begin.z) * (begin.z - end.z);
            u = u/((dx* dx)+(dy* dy)+(dz* dz));
 
	        retVal.x = begin.x + u* dx;
            retVal.y = begin.y + u* dy;
            retVal.z = begin.z + u* dz;
　　
	        return retVal;
        }

        //--------------------------------------------------

    }

}

