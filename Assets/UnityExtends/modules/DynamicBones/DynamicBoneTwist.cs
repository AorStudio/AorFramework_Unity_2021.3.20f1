using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.DynamicBones
{

    [ExecuteInEditMode]
    public class DynamicBoneTwist : MonoBehaviour
    {

        public enum TwistType
        {
            X = 0x1,
            Y = 0x2,
            YX = 0x3,
            Z = 0x4,
            XZ = 0x5,
            yZ = 0x6,
            All = 0x7
        }

        public Transform BindingUpper;
        public Transform BindingLower;

        [Range(0, 1)]
        public float PosWeight = 1.0f;
        [Range(0, 1)]
        public float RotateWeight = 0.5f;

        public TwistType m_TwistType;

        public bool Mirror = false;

        private void LateUpdate()
        {
            if (!BindingUpper) return;
            if (!BindingLower)
                BindingLower = transform;

            transform.position = Vector3.Lerp(BindingLower.position, BindingUpper.position, PosWeight);

            if ((int)m_TwistType >= 7)
            {
                //
                transform.localRotation = Quaternion.SlerpUnclamped(BindingLower.localRotation, BindingUpper.localRotation, RotateWeight);
            }
            else
            {
                float x = transform.localEulerAngles.x;
                if ((m_TwistType & TwistType.X) != 0)
                {
                    x = Mathf.LerpAngle(BindingLower.localEulerAngles.x, BindingUpper.localEulerAngles.x, RotateWeight);
                    if (Mirror)
                        x = -x;
                }

                float y = transform.localEulerAngles.y;
                if ((m_TwistType & TwistType.Y) != 0)
                {
                    y = Mathf.LerpAngle(BindingLower.localEulerAngles.y, BindingUpper.localEulerAngles.y, RotateWeight);
                    if (Mirror)
                        y = -y;
                }

                float z = transform.localEulerAngles.z;
                if ((m_TwistType & TwistType.Z) != 0)
                {
                    z = Mathf.LerpAngle(BindingLower.localEulerAngles.z, BindingUpper.localEulerAngles.z, RotateWeight);
                    if (Mirror)
                        z = -z;
                }

                transform.localEulerAngles = new Vector3(x, y, z);
            }

        }

    }

}
