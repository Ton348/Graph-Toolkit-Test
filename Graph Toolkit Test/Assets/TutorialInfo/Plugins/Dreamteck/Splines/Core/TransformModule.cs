using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines
{

    [System.Serializable]
    public class TransformModule : ISerializationCallbackReceiver
    {
        public Vector2 offset
        {
            get { return m_offset; }
            set
            {
                if (value != m_offset)
                {
                    m_offset = value;
                    m_hasOffset = m_offset != Vector2.zero;
                    if (targetUser != null)
                    {
                        targetUser.Rebuild();
                    }
                }
            }
        }
        public Vector3 rotationOffset
        {
            get { return m_rotationOffset; }
            set
            {
                if (value != m_rotationOffset)
                {
                    m_rotationOffset = value;
                    m_hasRotationOffset = m_rotationOffset != Vector3.zero;
                    if (targetUser != null)
                    {
                        targetUser.Rebuild();
                    }
                }
            }
        }

        public bool hasOffset
        {
            get { return m_hasOffset; }
        }

        public bool hasRotationOffset
        {
            get { return m_hasRotationOffset; }
        }

        public Vector3 baseScale
        {
            get { return m_baseScale; }
            set
            {
                if (value != m_baseScale)
                {
                    m_baseScale = value;
                    if (targetUser != null)
                    {
                        targetUser.Rebuild();
                    }
                }
            }
        }

        public bool is2D
        {
            get { return m_2dMode; }
            set
            {
                m_2dMode = value;
            }
        }

        [SerializeField]
        [HideInInspector]
        private bool m_hasOffset = false;
        [SerializeField]
        [HideInInspector]
        private bool m_hasRotationOffset = false;

        [SerializeField]
        [HideInInspector]
        private Vector2 m_offset;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_rotationOffset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_baseScale = Vector3.one;
        [SerializeField]
        [HideInInspector]
        private bool m_2dMode = false;
        public enum VelocityHandleMode { Zero, Preserve, Align, AlignRealistic }
        public VelocityHandleMode velocityHandleMode = VelocityHandleMode.Zero;
        public SplineSample splineResult
        {
            get
            {
                return m_splineResult;
            }
            set
            {
                m_splineResult = value;
            }
        }
        private SplineSample m_splineResult;

        public bool applyPositionX = true;
        public bool applyPositionY = true;
        public bool applyPositionZ = true;
        public bool applyPosition2D = true;
        public bool retainLocalPosition = false;

        public Spline.Direction direction = Spline.Direction.Forward;
        public bool applyPosition
        {
            get
            {
                if (m_2dMode)
                {
                    return applyPosition2D;
                }
                return applyPositionX || applyPositionY || applyPositionZ;
            }
            set
            {
                applyPositionX = applyPositionY = applyPositionZ = applyPosition2D = value;
            }
        }

        public bool applyRotationX = true;
        public bool applyRotationY = true;
        public bool applyRotationZ = true;
        public bool applyRotation2D = true;
        public bool retainLocalRotation = false;
        public bool applyRotation
        {
            get
            {
                if (m_2dMode)
                {
                    return applyRotation2D;
                }
                return applyRotationX || applyRotationY || applyRotationZ;
            }
            set
            {
                applyRotationX = applyRotationY = applyRotationZ = applyRotation2D = value;
            }
        }

        public bool applyScaleX = false;
        public bool applyScaleY = false;
        public bool applyScaleZ = false;
        public bool applyScale
        {
            get
            {
                return applyScaleX || applyScaleY || applyScaleZ;
            }
            set
            {
                applyScaleX = applyScaleY = applyScaleZ = value;
            }
        }
        [HideInInspector]
        public SplineUser targetUser = null;

        //These are used to save allocations
        private static Vector3 s_position = Vector3.zero;
        private static Quaternion s_rotation = Quaternion.identity;

        public void ApplyTransform(Transform input)
        {
            input.position = GetPosition(input.position);
            input.rotation = GetRotation(input.rotation);
            input.localScale = GetScale(input.localScale);
        }

        public void ApplyRigidbody(Rigidbody input)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ApplyTransform(input.transform);
                return;
            }
#endif
            input.transform.localScale = GetScale(input.transform.localScale);
            input.MovePosition(GetPosition(input.position));
            if (!input.isKinematic)
            {
#if UNITY_6000_0_OR_NEWER
                input.linearVelocity = HandleVelocity(input.linearVelocity);
#else
                input.velocity = HandleVelocity(input.velocity);
#endif
            }
            input.MoveRotation(GetRotation(input.rotation));
            Vector3 angularVelocity = input.angularVelocity;
            if (applyRotationX)
            {
                angularVelocity.x = 0f;
            }
            if (applyRotationY)
            {
                angularVelocity.y = 0f;
            }
            if (applyRotationZ || applyRotation2D)
            {
                angularVelocity.z = 0f;
            }
            input.angularVelocity = angularVelocity;
        }

        public void ApplyRigidbody2D(Rigidbody2D input)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ApplyTransform(input.transform);
                input.transform.rotation = Quaternion.AngleAxis(GetRotation(Quaternion.Euler(0f, 0f, input.rotation)).eulerAngles.z, Vector3.forward);
                return;
            }
#endif
            input.transform.localScale = GetScale(input.transform.localScale);
            input.position = GetPosition(input.position);
            if (!input.isKinematic)
            {
#if UNITY_6000_OR_NEWER
            input.linearVelocity = HandleVelocity(input.linearVelocity);
#else
                input.linearVelocity = HandleVelocity(input.linearVelocity);
#endif
            }
            input.rotation = GetRotation(Quaternion.Euler(0f, 0f, input.rotation)).eulerAngles.z;
            if (applyRotationX)
            {
                input.angularVelocity = 0f;
            }
        }

        Vector3 HandleVelocity(Vector3 velocity)
        {
            Vector3 idealVelocity = Vector3.zero;
            Vector3 direction = Vector3.right;
            switch (velocityHandleMode)
            {
                case VelocityHandleMode.Preserve: idealVelocity = velocity; break;
                case VelocityHandleMode.Align:
                    direction = m_splineResult.forward;
                    if (Vector3.Dot(velocity, direction) < 0f) direction *= -1f;
                    idealVelocity = direction * velocity.magnitude; break;
                case VelocityHandleMode.AlignRealistic:
                    direction = m_splineResult.forward;
                    if (Vector3.Dot(velocity, direction) < 0f) direction *= -1f;
                    idealVelocity = direction * velocity.magnitude * Vector3.Dot(velocity.normalized, direction); break;
            }
            if (applyPositionX) velocity.x = idealVelocity.x;
            if (applyPositionY) velocity.y = idealVelocity.y;
            if (applyPositionZ) velocity.z = idealVelocity.z;
            return velocity;
        }

        private Vector3 GetPosition(Vector3 inputPosition)
        {
            s_position = m_splineResult.position;
            Vector2 finalOffset = m_offset;
            if (finalOffset != Vector2.zero)
            {
                s_position += m_splineResult.right * finalOffset.x * m_splineResult.size + m_splineResult.up * finalOffset.y * m_splineResult.size;
            }
            if (retainLocalPosition)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(s_position, m_splineResult.rotation, Vector3.one);
                Vector3 splineLocalPosition = matrix.inverse.MultiplyPoint3x4(targetUser.transform.position);
                splineLocalPosition.x = applyPositionX ? 0f : splineLocalPosition.x;
                splineLocalPosition.y = applyPositionY ? 0f : splineLocalPosition.y;
                splineLocalPosition.z = applyPositionZ ? 0f : splineLocalPosition.z;
                inputPosition = matrix.MultiplyPoint3x4(splineLocalPosition);
            } else
            {
                if (applyPositionX) inputPosition.x = s_position.x;
                if (applyPositionY) inputPosition.y = s_position.y;
                if (applyPositionZ) inputPosition.z = s_position.z;
            }
            return inputPosition;
        }

        private Quaternion GetRotation(Quaternion inputRotation)
        {
            s_rotation = Quaternion.LookRotation(m_splineResult.forward * (direction == Spline.Direction.Forward ? 1f : -1f), m_splineResult.up);
            if (m_2dMode)
            {
                if (applyRotation2D)
                {
                    s_rotation *= Quaternion.Euler(90, -90, 0);
                    inputRotation = Quaternion.AngleAxis(s_rotation.eulerAngles.z + m_rotationOffset.z, Vector3.forward);
                }
                return inputRotation;
            }
            else
            {
                if (m_rotationOffset != Vector3.zero)
                {
                    s_rotation = s_rotation * Quaternion.Euler(m_rotationOffset);
                }
            }

            if (retainLocalRotation)
            {
                Quaternion localRotation = Quaternion.Inverse(s_rotation) * inputRotation;
                Vector3 targetEuler = localRotation.eulerAngles;
                targetEuler.x = applyRotationX ? 0f : targetEuler.x;
                targetEuler.y = applyRotationY ? 0f : targetEuler.y;
                targetEuler.z = applyRotationZ ? 0f : targetEuler.z;
                inputRotation = s_rotation * Quaternion.Euler(targetEuler);
            } else
            {
                if (!applyRotationX || !applyRotationY || !applyRotationZ)
                {
                    Vector3 targetEuler = s_rotation.eulerAngles;
                    Vector3 sourceEuler = inputRotation.eulerAngles;
                    if (!applyRotationX) targetEuler.x = sourceEuler.x;
                    if (!applyRotationY) targetEuler.y = sourceEuler.y;
                    if (!applyRotationZ) targetEuler.z = sourceEuler.z;
                    inputRotation.eulerAngles = targetEuler;
                }
                else 
                {
                    inputRotation = s_rotation;
                }
            }

            return inputRotation;
        }

        private Vector3 GetScale(Vector3 inputScale)
        {
            if (applyScaleX) inputScale.x = m_baseScale.x * m_splineResult.size;
            if (applyScaleY) inputScale.y = m_baseScale.y * m_splineResult.size;
            if (applyScaleZ) inputScale.z = m_baseScale.z * m_splineResult.size;
            return inputScale;
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            m_hasRotationOffset = m_rotationOffset != Vector3.zero;
            m_hasOffset = m_offset != Vector2.zero;
        }
    }
}
