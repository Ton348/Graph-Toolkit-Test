using UnityEngine;
using System;
using System.Reflection;

namespace Prototype.Player
{
	public sealed class PlayerShootingTest : MonoBehaviour
	{
		[SerializeField] private KeyCode shootKey = KeyCode.Mouse0;
		[SerializeField] private GameObject pistolObject;
		[SerializeField] private Transform muzzlePoint;
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private float dangerRadius = 30f;
		[SerializeField] private float dangerTimerSeconds = 8f;

		private bool m_weaponActive;

		private void Update()
		{
			if (Input.GetMouseButtonDown(1))
			{
				HideWeapon();
				return;
			}

			if (!IsShootPressed())
			{
				return;
			}

			if (!m_weaponActive)
			{
				ShowWeapon();
				return;
			}

			Fire();
		}

		private bool IsShootPressed()
		{
			if (shootKey == KeyCode.Mouse0)
			{
				return Input.GetMouseButtonDown(0);
			}

			if (shootKey == KeyCode.Mouse1)
			{
				return Input.GetMouseButtonDown(1);
			}

			if (shootKey == KeyCode.Mouse2)
			{
				return Input.GetMouseButtonDown(2);
			}

			return Input.GetKeyDown(shootKey);
		}

		private void Fire()
		{
			Debug.Log("[ShootTest] Fire() called.");
			if (bulletPrefab != null && muzzlePoint != null)
			{
				Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
				Debug.Log("[ShootTest] Bullet spawned.");
			}

			TryRaiseDangerEvent();
		}

		private void TryRaiseDangerEvent()
		{
			if (muzzlePoint == null)
			{
				Debug.LogError("[ShootTest] muzzlePoint is null.");
				return;
			}

			object manager = FindDangerManagerInstance();
			if (manager == null)
			{
				Debug.LogError("[ShootTest] DangerManager instance not found in scene.");
				return;
			}

			Type managerType = manager.GetType();
			Type sourceEnumType = Type.GetType("Prototype.Business.NPC.Danger.DangerSourceType, Prototype.Business");
			if (sourceEnumType == null)
			{
				Debug.LogError("[ShootTest] DangerSourceType not found.");
				return;
			}

			object gunshotSource = Enum.ToObject(sourceEnumType, 1);
			MethodInfo raiseMethod = managerType.GetMethod("RaiseDangerEvent", BindingFlags.Public | BindingFlags.Instance);
			if (raiseMethod == null)
			{
				Debug.LogError("[ShootTest] RaiseDangerEvent method not found.");
				return;
			}

			Debug.Log($"[ShootTest] Sending danger event. pos={muzzlePoint.position}, radius={dangerRadius}, timer={dangerTimerSeconds}, threat=1");
			raiseMethod.Invoke(manager, new object[]
			{
				muzzlePoint.position,
				dangerRadius,
				1,
				dangerTimerSeconds,
				gunshotSource
			});
		}

		private static object FindDangerManagerInstance()
		{
			MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			for (int i = 0; i < behaviours.Length; i++)
			{
				MonoBehaviour behaviour = behaviours[i];
				if (behaviour == null)
				{
					continue;
				}

				Type type = behaviour.GetType();
				if (type.FullName == "Prototype.Business.NPC.Danger.DangerManager")
				{
					return behaviour;
				}
			}

			return null;
		}

		private void ShowWeapon()
		{
			m_weaponActive = true;
			if (pistolObject != null)
			{
				pistolObject.SetActive(true);
			}
		}

		private void HideWeapon()
		{
			m_weaponActive = false;
			if (pistolObject != null)
			{
				pistolObject.SetActive(false);
			}
		}
	}
}
