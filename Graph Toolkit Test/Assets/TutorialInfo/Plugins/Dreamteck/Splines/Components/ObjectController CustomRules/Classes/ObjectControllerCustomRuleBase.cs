using UnityEngine;

namespace Dreamteck.Splines
{
	public class ObjectControllerCustomRuleBase : ScriptableObject
	{
		protected ObjectController m_currentController;
		protected int m_currentObjectIndex;
		protected SplineSample m_currentSample;
		protected int m_totalObjects;
		protected float currentObjectPercent => (float)m_currentObjectIndex / (m_totalObjects - 1);

		public void SetContext(ObjectController context, SplineSample sample, int currentObject, int totalObjects)
		{
			m_currentController = context;
			m_currentSample = sample;
			m_currentObjectIndex = currentObject;
			m_totalObjects = totalObjects;
		}

		/// <summary>
		///     Implement this method to create custom positioning behaviors. The returned offset should be in local coordinates.
		/// </summary>
		/// <returns>Vector3 offset in local coordinates</returns>
		public virtual Vector3 GetOffset()
		{
			return m_currentSample.position;
		}

		/// <summary>
		///     Implement this method to create custom rotation behaviors. The returned rotation is in world space
		/// </summary>
		/// <returns>Quaternion rotation in world coordinates</returns>
		public virtual Quaternion GetRotation()
		{
			return m_currentSample.rotation;
		}

		/// <summary>
		///     Implement this method to create custom scaling behaviors.
		/// </summary>
		/// <returns>Vector3 scale</returns>
		public virtual Vector3 GetScale()
		{
			return Vector3.one * m_currentSample.size;
		}
	}
}