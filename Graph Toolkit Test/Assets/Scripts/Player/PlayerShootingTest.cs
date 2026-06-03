using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Prototype.Player
{
	public sealed class PlayerShootingTest : MonoBehaviour
	{
		[SerializeField] private KeyCode shootKey = KeyCode.Mouse0;
		[SerializeField] private GameObject pistolObject;
		[SerializeField] private Transform muzzlePoint;
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private MonoBehaviour bootstrapBehaviour;

		private bool m_weaponActive;

		private void Update()
		{
			if (IsBlockedByUi())
			{
				return;
			}

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
			if (bulletPrefab != null && muzzlePoint != null)
			{
				GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
				BulletTest bulletTest = bullet.GetComponent<BulletTest>();
				if (bulletTest != null)
				{
					int damage = GetPlayerDamage();
					bulletTest.Initialize(damage, BulletOwnerType.Player, transform);
				}
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
			MethodInfo raiseMethod = managerType.GetMethod("RaiseDefaultDangerAt", BindingFlags.Public | BindingFlags.Instance);
			if (raiseMethod == null)
			{
				Debug.LogError("[ShootTest] RaiseDefaultDangerAt method not found.");
				return;
			}

			raiseMethod.Invoke(manager, new object[]
			{
				muzzlePoint.position
			});
		}

		private void Awake()
		{
			if (bootstrapBehaviour == null)
			{
				bootstrapBehaviour = FindBootstrapInstance();
			}
		}

		private int GetPlayerDamage()
		{
			object bootstrap = bootstrapBehaviour;
			if (bootstrap == null)
			{
				return 1;
			}

			object playerStateSync = bootstrap.GetType().GetProperty("PlayerStateSync", BindingFlags.Public | BindingFlags.Instance)?.GetValue(bootstrap);
			if (playerStateSync == null)
			{
				return 1;
			}

			PropertyInfo damageProperty = playerStateSync.GetType().GetProperty("Damage", BindingFlags.Public | BindingFlags.Instance);
			if (damageProperty == null)
			{
				return 1;
			}

			object value = damageProperty.GetValue(playerStateSync);
			return value is int damage ? damage : 1;
		}

		private static MonoBehaviour FindBootstrapInstance()
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

		private static bool IsBlockedByUi()
		{
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
			{
				return true;
			}

			MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			for (int i = 0; i < behaviours.Length; i++)
			{
				MonoBehaviour behaviour = behaviours[i];
				if (behaviour == null || !behaviour.gameObject.activeInHierarchy)
				{
					continue;
				}

				string fullName = behaviour.GetType().FullName;
				if (fullName == "Sample.Runtime.UI.DialogueUiservice" ||
				    fullName == "Sample.Runtime.UI.TradeOfferUiservice" ||
				    fullName == "Sample.Runtime.UI.TraderShopUIService")
				{
					PropertyInfo isOpen = behaviour.GetType().GetProperty("IsOpen", BindingFlags.Public | BindingFlags.Instance);
					if (isOpen != null && isOpen.PropertyType == typeof(bool))
					{
						object value = isOpen.GetValue(behaviour);
						if (value is bool open && open)
						{
							return true;
						}
					}

					FieldInfo panelField = behaviour.GetType().GetField("panel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if (panelField != null && panelField.GetValue(behaviour) is GameObject panel && panel.activeSelf)
					{
						return true;
					}
				}

			}

			return false;
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
