using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.EditorSupportTool
{
    /// <summary>
    /// 用于绘制SkeletonBones Gizmos的辅助脚本
    /// </summary>
    [ExecuteAlways]
    public class SkeletonBonesInfoDrawer : MonoBehaviour
    {

#if UNITY_EDITOR && !RUNTIME

        public Color BoneColor = Color.white;

        public bool SimpleLineDraw = false;
        public float BoneHRadius = 0.01f;
        public float BoneCenterLerp = 0.08f;

        public bool DrawBoneTopSphere = true;
        public float BoneTopSphereScale = 0.5f;

        private int m_BonesCount;
        private int m_BonesCountTemp;
        public int BonesCount => m_BonesCount;

        private void OnDrawGizmos()
        {
            m_BonesCountTemp = 0;
            DrawBoneGizmoLoop(transform);
            m_BonesCount = m_BonesCountTemp;
        }

        private void DrawBoneGizmoLoop(Transform node)
        {
            if (node.childCount > 0)
            {
                for (int i = 0; i < node.childCount; i++)
                {
                    Transform child = node.GetChild(i);
                    DrawBoneGizmo(node, child);
                    DrawBoneGizmoLoop(child);
                }
            }
        }

        private void DrawBoneGizmo(Transform bp, Transform bc)
        {

            Vector3 s = bp.position;
            Vector3 e = bc.position;

            Gizmos.color = BoneColor;

            if (SimpleLineDraw)
            {
                Gizmos.DrawLine(s, e);
            }
            else
            {

                Vector3 c = Vector3.Lerp(s, e, BoneCenterLerp);
                Vector3 dir = (e - s).normalized;
                bool isUp = (dir == Vector3.up);
                Vector3 r = isUp ? Vector3.right : Vector3.Cross(dir, Vector3.up).normalized;
                Vector3 f = isUp ? Vector3.forward : Vector3.Cross(r, dir).normalized;

                r *= BoneHRadius;
                f *= BoneHRadius;

                Vector3 c0 = c + f;
                Vector3 c1 = c + r;
                Vector3 c2 = c - f;
                Vector3 c3 = c - r;

                Gizmos.DrawLine(s, c0);
                Gizmos.DrawLine(s, c1);
                Gizmos.DrawLine(s, c2);
                Gizmos.DrawLine(s, c3);

                Gizmos.DrawLine(c1, c0);
                Gizmos.DrawLine(c2, c1);
                Gizmos.DrawLine(c3, c2);
                Gizmos.DrawLine(c0, c3);

                Gizmos.DrawLine(c0, e);
                Gizmos.DrawLine(c1, e);
                Gizmos.DrawLine(c2, e);
                Gizmos.DrawLine(c3, e);
            }

            if (DrawBoneTopSphere)
            {
                Gizmos.matrix = Matrix4x4.TRS(e, bp.rotation, bp.lossyScale);
                Gizmos.DrawWireSphere(Vector3.zero, BoneHRadius * BoneTopSphereScale);
                Gizmos.matrix = Matrix4x4.identity;
            }

            Gizmos.color = Color.white;

            m_BonesCountTemp++;
        }

#endif

    }
}
