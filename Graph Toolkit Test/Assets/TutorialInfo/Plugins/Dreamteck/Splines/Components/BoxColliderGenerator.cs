using System;
using UnityEngine;

namespace Dreamteck.Splines
{
	public class BoxColliderGenerator : SplineUser, ISerializationCallbackReceiver
	{
		[SerializeField]
		private Vector2 m_boxSize = Vector2.one;

		[SerializeField]
		private bool m_debugDraw;

		[SerializeField]
		private Color m_debugDrawColor = Color.white;


		[SerializeField]
		[HideInInspector]
		public ColliderObject[] colliders = new ColliderObject[0];


		public Vector2 boxSize
		{
			get => m_boxSize;
			set
			{
				if (value != m_boxSize)
				{
					m_boxSize = value;
					Rebuild();
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			for (var i = 0; i < colliders.Length; i++)
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					DestroyImmediate(colliders[i].transform.gameObject);
				}
				else
				{
					Destroy(colliders[i].transform.gameObject);
				}
#else
                Destroy(_colliders[i].transform.gameObject);
#endif
			}
		}

		private void OnDrawGizmos()
		{
			if (m_debugDraw)
			{
				for (var i = 0; i < colliders.Length; i++)
				{
					Gizmos.matrix = colliders[i].transform.localToWorldMatrix;
					Gizmos.color = m_debugDrawColor;
					Gizmos.DrawCube(Vector3.zero, colliders[i].collider.size);
				}

				Gizmos.matrix = Matrix4x4.identity;
			}
		}

		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			Build();
		}

		private void DestroyCollider(ColliderObject collider)
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				Destroy(collider.transform.gameObject);
			}
			else
			{
				DestroyImmediate(collider.transform.gameObject);
			}
#else
            Destroy(collider.transform.gameObject);
#endif
		}

		protected override void Build()
		{
			base.Build();

			if (sampleCount == 0)
			{
				for (var i = 0; i < colliders.Length; i++)
				{
					DestroyCollider(colliders[i]);
				}

				colliders = new ColliderObject[0];
				return;
			}

			int objectCount = sampleCount - 1;
			if (objectCount != colliders.Length)
			{
				var newColliders = new ColliderObject[objectCount];
				for (var i = 0; i < newColliders.Length; i++)
				{
					if (i < colliders.Length)
					{
						newColliders[i] = colliders[i];
					}
					else
					{
						var newObject = new GameObject("Collider " + i);
						newObject.layer = gameObject.layer;
						newObject.transform.parent = trs;
						newColliders[i] =
							new ColliderObject(newObject.transform, newObject.AddComponent<BoxCollider>());
					}
				}

				if (newColliders.Length < colliders.Length)
				{
					for (int i = newColliders.Length; i < colliders.Length; i++)
					{
						DestroyCollider(colliders[i]);
					}
				}

				colliders = newColliders;
			}

			var current = new SplineSample();
			var next = new SplineSample();
			Evaluate(0.0, ref current);

			for (var i = 0; i < objectCount; i++)
			{
				double nextPercent = (double)(i + 1) / (sampleCount - 1);
				Evaluate(nextPercent, ref next);
				colliders[i].transform.position = Vector3.Lerp(current.position, next.position, 0.5f);
				colliders[i].transform.rotation = Quaternion.LookRotation(next.position - current.position,
					Vector3.Slerp(current.up, next.up, 0.5f));
				float size = Mathf.Lerp(current.size, next.size, 0.5f);
				colliders[i].collider.size = new Vector3(m_boxSize.x * size, m_boxSize.y * size,
					Vector3.Distance(current.position, next.position));
				current = next;
			}
		}

		[Serializable]
		public class ColliderObject
		{
			public Transform transform;
			public BoxCollider collider;

			public ColliderObject(Transform transform, BoxCollider collider)
			{
				this.transform = transform;
				this.collider = collider;
			}
		}
	}
}