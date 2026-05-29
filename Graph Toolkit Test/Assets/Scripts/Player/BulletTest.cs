using UnityEngine;

namespace Prototype.Player
{
	public sealed class BulletTest : MonoBehaviour
	{
		[SerializeField] private float speed = 35f;
		[SerializeField] private float maxDistance = 20f;
		[SerializeField] private float maxLifetimeSeconds = 5f;

		private Vector3 m_startPosition;
		private float m_lifetime;

		private void Awake()
		{
			m_startPosition = transform.position;
		}

		private void Update()
		{
			float dt = UnityEngine.Time.deltaTime;
			transform.position += transform.forward * speed * dt;
			m_lifetime += dt;

			if (m_lifetime >= maxLifetimeSeconds)
			{
				Destroy(gameObject);
				return;
			}

			if ((transform.position - m_startPosition).sqrMagnitude >= maxDistance * maxDistance)
			{
				Destroy(gameObject);
			}
		}
	}
}
