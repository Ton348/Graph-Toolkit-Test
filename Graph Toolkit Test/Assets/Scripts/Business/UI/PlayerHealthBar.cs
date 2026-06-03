using Prototype.Business.Bootstrap;
using Prototype.Business.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype.Business.UI
{
	public sealed class PlayerHealthBar : MonoBehaviour
	{
		[SerializeField] private GameBootstrap bootstrap;
		[SerializeField] private Image fillImage;

		private PlayerStateSync m_playerStateSync;

		private void OnEnable()
		{
			ResolveDependencies();
			Subscribe();
			Refresh();
		}

		private void OnDisable()
		{
			Unsubscribe();
		}

		private void ResolveDependencies()
		{
			if (bootstrap == null)
			{
				bootstrap = FindFirstObjectByType<GameBootstrap>(FindObjectsInactive.Include);
			}

			if (bootstrap != null)
			{
				m_playerStateSync = bootstrap.PlayerStateSync;
			}
		}

		private void Subscribe()
		{
			if (m_playerStateSync != null)
			{
				m_playerStateSync.healthChanged -= OnHealthChanged;
				m_playerStateSync.healthChanged += OnHealthChanged;
			}
		}

		private void Unsubscribe()
		{
			if (m_playerStateSync != null)
			{
				m_playerStateSync.healthChanged -= OnHealthChanged;
			}
		}

		private void OnHealthChanged(int currentHealth, int maxHealth)
		{
			Refresh(currentHealth, maxHealth);
		}

		private void Refresh()
		{
			if (m_playerStateSync == null)
			{
				ResolveDependencies();
			}

			if (m_playerStateSync == null)
			{
				return;
			}

			Refresh(m_playerStateSync.Health, m_playerStateSync.MaxHealth);
		}

		private void Refresh(int currentHealth, int maxHealth)
		{
			if (fillImage == null)
			{
				return;
			}

			float ratio = maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;
			fillImage.fillAmount = ratio;
		}
	}
}
