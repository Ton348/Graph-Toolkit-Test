using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Prototype.Player
{
	public sealed class BulletTest : MonoBehaviour
	{
		[SerializeField] private float speed = 35f;
		[SerializeField] private float maxDistance = 20f;
		[SerializeField] private float maxLifetimeSeconds = 5f;
		[SerializeField] private BulletOwnerType ownerType = BulletOwnerType.Player;
		[SerializeField] private int damage = 1;
		[SerializeField] private Transform ownerRoot;

		private Vector3 m_startPosition;
		private float m_lifetime;
		private Collider m_collider;
		private Rigidbody m_rigidbody;

		public void Initialize(int bulletDamage, BulletOwnerType owner, Transform ownerTransform)
		{
			damage = Mathf.Max(0, bulletDamage);
			ownerType = owner;
			ownerRoot = ownerTransform;
		}

		private void Awake()
		{
			m_startPosition = transform.position;
			m_collider = GetComponent<Collider>();
			if (m_collider == null)
			{
				m_collider = gameObject.AddComponent<SphereCollider>();
			}

			m_collider.isTrigger = true;

			m_rigidbody = GetComponent<Rigidbody>();
			if (m_rigidbody == null)
			{
				m_rigidbody = gameObject.AddComponent<Rigidbody>();
			}

			m_rigidbody.isKinematic = true;
			m_rigidbody.useGravity = false;
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

		private void OnTriggerEnter(Collider other)
		{
			TryHandleHit(other);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision == null)
			{
				return;
			}

			TryHandleHit(collision.collider);
		}

		private void TryHandleHit(Collider other)
		{
			if (other == null)
			{
				return;
			}

			if (ownerRoot != null && other.transform != null && other.transform.root == ownerRoot.root)
			{
				return;
			}

			Component npcHealth = FindComponentByTypeNameInParents(other.transform, "Prototype.Business.NPC.Needs.NPCNeedsComponent");
			if (npcHealth != null && ownerType == BulletOwnerType.Player)
			{
				if (InvokeBool(npcHealth, "TakeDamage", damage))
				{
					if (InvokeBoolProperty(npcHealth, "IsDead"))
					{
						object manager = FindDangerManagerInstance();
						if (manager != null)
						{
							MethodInfo murder = manager.GetType().GetMethod("RaiseMurderAt", BindingFlags.Public | BindingFlags.Instance);
							if (murder != null)
							{
								murder.Invoke(manager, new object[] { npcHealth.transform.position });
							}
						}

						NotifyNpcDeath(npcHealth);
					}
				}

				Destroy(gameObject);
				return;
			}

			if (ownerType == BulletOwnerType.Npc)
			{
				object bootstrap = FindBootstrapInstance();
				object playerStateSync = bootstrap != null ? GetPropertyValue(bootstrap, "PlayerStateSync") : null;
				object gameServer = bootstrap != null ? GetPropertyValue(bootstrap, "GameServer") : null;
				if (bootstrap != null && gameServer != null)
				{
					_ = ApplyPlayerDamageAsync(bootstrap, damage);
				}
				else if (playerStateSync != null)
				{
					InvokeBool(playerStateSync, "TryApplyDamage", damage);
					object runtimeState = GetPropertyValue(bootstrap, "RuntimeState");
					object runtimePlayer = runtimeState != null ? GetPropertyValue(runtimeState, "player") : null;
					if (runtimePlayer != null)
					{
						SetPropertyValue(runtimePlayer, "health", GetIntPropertyValue(playerStateSync, "Health"));
					}
				}

				Destroy(gameObject);
			}
		}

		private static async System.Threading.Tasks.Task ApplyPlayerDamageAsync(object bootstrap, int amount)
		{
			if (bootstrap == null)
			{
				return;
			}

			object gameServer = GetPropertyValue(bootstrap, "GameServer");
			if (gameServer == null)
			{
				object playerStateSync = GetPropertyValue(bootstrap, "PlayerStateSync");
				if (playerStateSync != null && InvokeBool(playerStateSync, "TryApplyDamage", amount))
				{
					object runtimeState = GetPropertyValue(bootstrap, "RuntimeState");
					object runtimePlayer = runtimeState != null ? GetPropertyValue(runtimeState, "player") : null;
					if (runtimePlayer != null)
					{
						SetPropertyValue(runtimePlayer, "health", GetIntPropertyValue(playerStateSync, "Health"));
					}
				}
				return;
			}

			Task task = gameServer.GetType().GetMethod("TryApplyPlayerDamageAsync", BindingFlags.Public | BindingFlags.Instance)?.Invoke(gameServer, new object[] { amount }) as Task;
			if (task == null)
			{
				return;
			}

			await task;
			object result = task.GetType().GetProperty("Result")?.GetValue(task);
			if (result == null || !InvokeBoolProperty(result, "Success"))
			{
				return;
			}

			object snapshot = GetPropertyValue(result, "ProfileSnapshot");
			object profileSync = GetPropertyValue(bootstrap, "ProfileSyncService");
			if (snapshot != null && profileSync != null)
			{
				profileSync.GetType().GetMethod("ApplySnapshot", BindingFlags.Public | BindingFlags.Instance)?.Invoke(profileSync, new object[] { snapshot });
			}
		}

		private static void NotifyNpcDeath(Component npcHealth)
		{
			if (npcHealth == null)
			{
				return;
			}

			Component npcManager = FindComponentByTypeName(npcHealth, "Prototype.Business.NPC.Npcmanager");
			if (npcManager != null)
			{
				npcManager.GetType().GetMethod("HandleNpcDied", BindingFlags.Public | BindingFlags.Instance)?.Invoke(npcManager, null);
			}

			Component workerBrain = FindComponentByTypeName(npcHealth, "Prototype.Business.NPC.Workers.WorkerNPCBrain");
			if (workerBrain != null)
			{
				workerBrain.GetType().GetMethod("HandleDeath", BindingFlags.Public | BindingFlags.Instance)?.Invoke(workerBrain, null);
			}
		}

		private static object FindDangerManagerInstance()
		{
			MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			for (int i = 0; i < behaviours.Length; i++)
			{
				MonoBehaviour behaviour = behaviours[i];
				if (behaviour != null && behaviour.GetType().FullName == "Prototype.Business.NPC.Danger.DangerManager")
				{
					return behaviour;
				}
			}

			return null;
		}

		private static object FindBootstrapInstance()
		{
			MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			for (int i = 0; i < behaviours.Length; i++)
			{
				MonoBehaviour behaviour = behaviours[i];
				if (behaviour != null && behaviour.GetType().FullName == "Prototype.Business.Bootstrap.GameBootstrap")
				{
					return behaviour;
				}
			}

			return null;
		}

		private static Component FindComponentByTypeNameInParents(Transform transform, string fullName)
		{
			Transform current = transform;
			while (current != null)
			{
				Component[] components = current.GetComponents<Component>();
				for (int i = 0; i < components.Length; i++)
				{
					Component component = components[i];
					if (component != null && component.GetType().FullName == fullName)
					{
						return component;
					}
				}

				current = current.parent;
			}

			return null;
		}

		private static Component FindComponentByTypeName(Component root, string fullName)
		{
			Component[] components = root.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				Component component = components[i];
				if (component != null && component.GetType().FullName == fullName)
				{
					return component;
				}
			}

			return null;
		}

		private static object GetPropertyValue(object target, string propertyName)
		{
			return target?.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(target);
		}

		private static void SetPropertyValue(object target, string propertyName, object value)
		{
			PropertyInfo property = target?.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			if (property != null && property.CanWrite)
			{
				property.SetValue(target, value);
			}
		}

		private static bool InvokeBool(object target, string methodName, params object[] args)
		{
			MethodInfo method = target?.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
			if (method == null)
			{
				return false;
			}

			object result = method.Invoke(target, args);
			return result is bool value && value;
		}

		private static bool InvokeBoolProperty(object target, string propertyName)
		{
			object value = GetPropertyValue(target, propertyName);
			return value is bool b && b;
		}

		private static int GetIntPropertyValue(object target, string propertyName)
		{
			object value = GetPropertyValue(target, propertyName);
			return value is int i ? i : 0;
		}
	}
}
