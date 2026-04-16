using UnityEngine;

namespace Sample.Runtime.Compass
{
	public class CompassTarget : MonoBehaviour
	{
		[SerializeField]
		private string m_targetId;

		[SerializeField]
		private Transform m_markerPoint;

		private bool m_registered;

		public string TargetId => m_targetId;

		private void Start()
		{
			TryRegister();
		}

		private void OnEnable()
		{
			TryRegister();
		}

		private void OnDisable()
		{
			TryUnregister();
		}

		public Vector3 GetMarkerWorldPosition()
		{
			return m_markerPoint != null ? m_markerPoint.position : transform.position;
		}

		private void TryRegister()
		{
			if (m_registered)
			{
				return;
			}

			var registry = CompassTargetRegistry.Instance;
			if (registry == null)
			{
				registry = FindObjectOfType<CompassTargetRegistry>();
			}

			if (registry == null)
			{
				return;
			}

			registry.Register(this);
			m_registered = true;
		}

		private void TryUnregister()
		{
			if (!m_registered)
			{
				return;
			}

			var registry = CompassTargetRegistry.Instance;
			if (registry == null)
			{
				registry = FindObjectOfType<CompassTargetRegistry>();
			}

			if (registry == null)
			{
				return;
			}

			registry.Unregister(this);
			m_registered = false;
		}
	}
}