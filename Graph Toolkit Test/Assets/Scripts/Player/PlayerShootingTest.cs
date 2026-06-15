using Prototype.Business.Bootstrap;
using Prototype.Business.NPC.Danger;
using Prototype.Business.Services;
using Sample.Runtime.UI;
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

		private GameBootstrap m_bootstrap;
		private DialogueUiservice m_dialogueUiService;
		private TradeOfferUiservice m_tradeOfferUiService;
		private TraderShopUIService m_traderShopUiService;
		private DangerManager m_dangerManager;
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

			if (m_dangerManager == null)
			{
				Debug.LogError("[ShootTest] DangerManager instance not found in scene.");
				return;
			}

			m_dangerManager.RaiseDefaultDangerAt(muzzlePoint.position);
		}

		private void Awake()
		{
			m_bootstrap = FindAnyObjectByType<GameBootstrap>(FindObjectsInactive.Include);
			m_dialogueUiService = FindAnyObjectByType<DialogueUiservice>(FindObjectsInactive.Include);
			m_tradeOfferUiService = FindAnyObjectByType<TradeOfferUiservice>(FindObjectsInactive.Include);
			m_traderShopUiService = FindAnyObjectByType<TraderShopUIService>(FindObjectsInactive.Include);
			m_dangerManager = FindAnyObjectByType<DangerManager>(FindObjectsInactive.Include);
		}

		private int GetPlayerDamage()
		{
			if (m_bootstrap == null || m_bootstrap.PlayerStateSync == null)
			{
				return 1;
			}

			return Mathf.Max(1, m_bootstrap.PlayerStateSync.Damage);
		}

		private bool IsBlockedByUi()
		{
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
			{
				return true;
			}

			if (m_dialogueUiService != null)
			{
				if ((m_dialogueUiService.panel != null && m_dialogueUiService.panel.activeSelf) ||
				    gameObject.activeInHierarchy && m_dialogueUiService.gameObject.activeInHierarchy)
				{
					return true;
				}
			}

			if (m_tradeOfferUiService != null && m_tradeOfferUiService.IsOpen)
			{
				return true;
			}

			if (m_traderShopUiService != null && m_traderShopUiService.IsOpen)
			{
				return true;
			}

			return false;
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
