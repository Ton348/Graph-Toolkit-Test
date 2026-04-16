using UnityEngine;
using System.Collections;

namespace Dreamteck
{
    [System.Serializable]
    public class TsTransform
    {
        public Vector3 position
        {
            get { return new Vector3(m_posX, m_posY, m_posZ); }
            set
            {
                m_setPosition = true;
                m_setLocalPosition = false;
                m_posX = value.x;
                m_posY = value.y;
                m_posZ = value.z;
            }
        }
        public Quaternion rotation
        {
            get { return new Quaternion(m_rotX, m_rotY, m_rotZ, m_rotW); }
            set
            {
                m_setRotation = true;
                m_setLocalRotation = false;
                m_rotX = value.x;
                m_rotY = value.y;
                m_rotZ = value.z;
                m_rotW = value.w;
            }
        }
        public Vector3 scale
        {
            get { return new Vector3(m_scaleX, m_scaleY, m_scaleZ); }
            set
            {
                m_setScale = true;
                m_scaleX = value.x;
                m_scaleY = value.y;
                m_scaleZ = value.z;
            }
        }

        public Vector3 lossyScale
        {
            get { return new Vector3(m_lossyScaleX, m_lossyScaleY, m_lossyScaleZ); }
            set
            {
                m_setScale = true;
                m_lossyScaleX = value.x;
                m_lossyScaleY = value.y;
                m_lossyScaleZ = value.z;
            }
        }

        public Vector3 localPosition
        {
            get { return new Vector3(m_lposX, m_lposY, m_lposZ); }
            set
            {
                m_setLocalPosition = true;
                m_setPosition = false;
                m_lposX = value.x;
                m_lposY = value.y;
                m_lposZ = value.z;
            }
        }
        public Quaternion localRotation
        {
            get { return new Quaternion(m_lrotX, m_lrotY, m_lrotZ, m_lrotW); }
            set
            {
                m_setLocalRotation = true;
                m_setRotation = false;
                m_lrotX = value.x;
                m_lrotY = value.y;
                m_lrotZ = value.z;
                m_lrotW = value.w;
            }
        }

        private bool m_setPosition = false;
        private bool m_setRotation = false;
        private bool m_setScale = false;
        private bool m_setLocalPosition = false;
        private bool m_setLocalRotation = false;

        public Transform transform
        {
            get
            {
                return m_transform;
            }
        }

        [SerializeField]
        [HideInInspector]
        private Transform m_transform;

        [SerializeField]
        [HideInInspector]
        private volatile float m_posX = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_posY = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_posZ = 0f;

        [SerializeField]
        [HideInInspector]
        private volatile float m_scaleX = 1f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_scaleY = 1f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_scaleZ = 1f;

        [SerializeField]
        [HideInInspector]
        private volatile float m_lossyScaleX = 1f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_lossyScaleY = 1f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_lossyScaleZ = 1f;

        [SerializeField]
        [HideInInspector]
        private volatile float m_rotX = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_rotY = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_rotZ = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_rotW = 0f;


        [SerializeField]
        [HideInInspector]
        private volatile float m_lposX = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_lposY = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_lposZ = 0f;

        [SerializeField]
        [HideInInspector]
        private volatile float m_lrotX = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_lrotY = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_lrotZ = 0f;
        [SerializeField]
        [HideInInspector]
        private volatile float m_lrotW = 0f;
#if UNITY_EDITOR
        private volatile bool m_isPlaying = false;
#endif

        public TsTransform(Transform input)
        {
            SetTransform(input);
        }

        /// <summary>
        /// Update the TS_Transform. Call this regularly on every frame you need it to update. Should ALWAYS be called from the main thread
        /// </summary>
        public void Update()
        {
            if (transform == null) return;
#if UNITY_EDITOR
            m_isPlaying = Application.isPlaying;
#endif
            if (m_setPosition) m_transform.position = position;
            else if (m_setLocalPosition) m_transform.localPosition = localPosition;
            else
            {
                position = m_transform.position;
                localPosition = m_transform.localPosition;
            }

            if (m_setScale) m_transform.localScale = scale;
            else scale = m_transform.localScale;
            lossyScale = m_transform.lossyScale;
            

            if (m_setRotation) m_transform.rotation = rotation;
            else if (m_setLocalRotation) m_transform.localRotation = localRotation;
            else
            {
                rotation = m_transform.rotation;
                localRotation = m_transform.localRotation;
            }
            m_setPosition = m_setLocalPosition = m_setRotation = m_setLocalRotation = m_setScale = false;
        }

        /// <summary>
        /// Set the transform reference. Should ALWAYS be called from the main thread
        /// </summary>
        /// <param name="input">Transform reference</param>
        public void SetTransform(Transform input)
        {
            m_transform = input;
            m_setPosition = m_setLocalPosition = m_setRotation = m_setLocalRotation = m_setScale = false;
            Update();
        }

        /// <summary>
        /// Returns true if there's any change in the transform. Should ALWAYS be called from the main thread
        /// </summary>
        /// <returns></returns>
        public bool HasChange()
        {
            return HasPositionChange() || HasRotationChange() || HasScaleChange();
        }

        /// <summary>
        /// Returns true if there's a change in the position. Should ALWAYS be called from the main thread
        /// </summary>
        /// <returns></returns>
        public bool HasPositionChange()
        {
            return m_posX != m_transform.position.x || m_posY != m_transform.position.y || m_posZ != m_transform.position.z;
        }

        /// <summary>
        /// Returns true if there is a change in the rotation. Should ALWAYS be called from the main thread
        /// </summary>
        /// <returns></returns>
        public bool HasRotationChange()
        {
            return m_rotX != m_transform.rotation.x || m_rotY != m_transform.rotation.y || m_rotZ != m_transform.rotation.z || m_rotW != m_transform.rotation.w;
        }

        /// <summary>
        /// Returns true if there is a change in the scale. Should ALWAYS be called from the main thread
        /// </summary>
        /// <returns></returns>
        public bool HasScaleChange()
        {
            return m_lossyScaleX != m_transform.lossyScale.x || m_lossyScaleY != m_transform.lossyScale.y || m_lossyScaleZ != m_transform.lossyScale.z;
        }

        /// <summary>
        /// Thread-safe TransformPoint
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 point)
        {
#if UNITY_EDITOR
            if (!m_isPlaying) return transform.TransformPoint(point);
#endif
            Vector3 scaled = new Vector3(point.x * m_lossyScaleX, point.y * m_lossyScaleY, point.z * m_lossyScaleZ);
            Vector3 rotated = rotation * scaled;
            return position + rotated;
        }

        /// <summary>
        /// Thread-safe TransformDirection
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 TransformDirection(Vector3 direction)
        {
#if UNITY_EDITOR
            if (!m_isPlaying) return transform.TransformDirection(direction);
#endif
            return TransformPoint(direction) - position;
        }

        /// <summary>
        /// Thread-safe InverseTransformPoint
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector3 InverseTransformPoint(Vector3 point)
        {
#if UNITY_EDITOR
            if (!m_isPlaying) return transform.InverseTransformPoint(point);
#endif
            return InverseTransformDirection(point - position);
        }

        /// <summary>
        /// Thread-safe InverseTransformDirection
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 InverseTransformDirection(Vector3 direction)
        {
#if UNITY_EDITOR
            if (!m_isPlaying) return transform.InverseTransformDirection(direction);
#endif
            Vector3 rotated = Quaternion.Inverse(rotation) * direction;
            return new Vector3(rotated.x / m_lossyScaleX, rotated.y / m_lossyScaleY, rotated.z / m_lossyScaleZ);
        }

        public T GetComponent<T>()
        {
            return m_transform.GetComponent<T>();
        }

    }
}