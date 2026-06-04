using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Prototype.Business.NPC.Combat
{
	public sealed class NPCWeaponTest : MonoBehaviour
	{
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private Transform muzzlePoint;
		[SerializeField] private int damage = 1;
		private readonly object m_shotLock = new();
		private UniTaskCompletionSource<bool> m_lastShotCompletion;

		public void Fire()
		{
			if (bulletPrefab == null || muzzlePoint == null)
			{
				UnityEngine.Debug.Log($"[Combat] '{name}' weapon fire failed: bulletPrefab or muzzlePoint missing.");
				return;
			}

			UnityEngine.Debug.Log($"[Combat] '{name}' weapon firing.");
			GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
			Component bulletBehaviour = bullet.GetComponent("BulletTest");
			if (bulletBehaviour == null)
			{
				UnityEngine.Debug.Log($"[Combat] '{name}' weapon fire failed: BulletTest component missing on prefab.");
				CompleteShot(false);
				return;
			}

			Type bulletType = ResolveType("Prototype.Player.BulletTest");
			Type ownerTypeType = ResolveType("Prototype.Player.BulletOwnerType");
			if (bulletType == null || ownerTypeType == null)
			{
				UnityEngine.Debug.Log($"[Combat] '{name}' weapon fire failed: BulletTest types missing.");
				CompleteShot(false);
				return;
			}

			MethodInfo initialize = bulletType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
			if (initialize == null)
			{
				UnityEngine.Debug.Log($"[Combat] '{name}' weapon fire failed: Initialize method missing.");
				CompleteShot(false);
				return;
			}

			object ownerEnum = Enum.Parse(ownerTypeType, "Npc");
			if (bulletType.IsInstanceOfType(bulletBehaviour))
			{
				initialize.Invoke(bulletBehaviour, new object[] { damage, ownerEnum, transform });
				MethodInfo setCompletion = bulletType.GetMethod("SetCompletionCallback", BindingFlags.Public | BindingFlags.Instance);
				if (setCompletion != null)
				{
					Action completed = () => CompleteShot(true);
					setCompletion.Invoke(bulletBehaviour, new object[] { completed });
				}
			}
			else
			{
				CompleteShot(false);
			}
		}

		public async UniTask<bool> FireAndWaitAsync(CancellationToken cancellationToken)
		{
			lock (m_shotLock)
			{
				m_lastShotCompletion = new UniTaskCompletionSource<bool>();
			}

			Fire();
			UniTaskCompletionSource<bool> completion;
			lock (m_shotLock)
			{
				completion = m_lastShotCompletion;
			}

			if (completion == null)
			{
				return false;
			}

			using (cancellationToken.Register(() => CompleteShot(false)))
			{
				return await completion.Task;
			}
		}

		private void CompleteShot(bool success)
		{
			UniTaskCompletionSource<bool> completion;
			lock (m_shotLock)
			{
				completion = m_lastShotCompletion;
				m_lastShotCompletion = null;
			}

			completion?.TrySetResult(success);
		}

		private static Type ResolveType(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName))
			{
				return null;
			}

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(fullName, false);
				if (type != null)
				{
					return type;
				}
			}

			return null;
		}

		public void StopFire()
		{
		}
	}
}
