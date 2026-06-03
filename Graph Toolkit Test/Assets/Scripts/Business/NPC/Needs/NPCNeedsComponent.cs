using System;
using UnityEngine;
namespace Prototype.Business.NPC.Needs
{
	public sealed class NPCNeedsComponent : MonoBehaviour
	{
		[SerializeField] private float hunger = 20f;
		[SerializeField] private float thirst = 20f;
		[SerializeField] private float energy = 80f;
		[SerializeField] private float work = 20f;
		[SerializeField] private float safety = 100f;
		[SerializeField] private float fun = 50f;
		[SerializeField] private float toilet = 10f;
		[SerializeField] private float cityNeed = 50f;
		[SerializeField] private float natureNeed = 50f;
		[SerializeField] private float foodNeed = 20f;
		[SerializeField] private float energyNeed = 20f;
		[SerializeField] private float funNeed = 40f;
		[SerializeField] private float maxHealth = 100f;
		[SerializeField] private float currentHealth = 100f;
		[SerializeField] private float tickSeconds = 1f;
		private float m_timer;
		private bool m_isDead;

		public event Action<float, float> HealthChanged;
		public event Action Died;

		public float Hunger => hunger;
		public float Thirst => thirst;
		public float Energy => energy;
		public float Work => work;
		public float Safety => safety;
		public float Fun => fun;
		public float Toilet => toilet;
		public float CityNeed => cityNeed;
		public float NatureNeed => natureNeed;
		public float FoodNeed => foodNeed;
		public float EnergyNeed => energyNeed;
		public float FunNeed => funNeed;
		public float MaxHealth => maxHealth;
		public float CurrentHealth => currentHealth;
		public bool IsDead => m_isDead;

		private void Update()
		{
			m_timer += UnityEngine.Time.deltaTime;
			if (m_timer < Mathf.Max(0.1f, tickSeconds))
			{
				return;
			}

			m_timer = 0f;
			TickNeeds();
		}

		public void TickNeeds()
		{
			foodNeed = hunger;
			energyNeed = Clamp01x100(100f - energy);
			funNeed = fun;
		}

		public void SetSafety(float value)
		{
			safety = Clamp01x100(value);
		}

		public void SetCityNeed(float value)
		{
			cityNeed = Clamp01x100(value);
		}

		public void SetNatureNeed(float value)
		{
			natureNeed = Clamp01x100(value);
		}

		public void SetHealth(float value)
		{
			float clamped = Clamp01x100(value);
			if (Mathf.Approximately(currentHealth, clamped))
			{
				return;
			}

			currentHealth = clamped;
			HealthChanged?.Invoke(currentHealth, maxHealth);

			if (!m_isDead && currentHealth <= 0f)
			{
				m_isDead = true;
				Died?.Invoke();
			}
		}

		public bool TakeDamage(float amount)
		{
			if (m_isDead)
			{
				return false;
			}

			float delta = Mathf.Max(0f, amount);
			if (delta <= 0f)
			{
				return false;
			}

			SetHealth(currentHealth - delta);
			return true;
		}

		public void Heal(float amount)
		{
			if (m_isDead)
			{
				return;
			}

			float delta = Mathf.Max(0f, amount);
			if (delta <= 0f)
			{
				return;
			}

			SetHealth(currentHealth + delta);
		}

		public void Revive(float health = 100f)
		{
			maxHealth = Clamp01x100(Mathf.Max(maxHealth, health));
			currentHealth = Clamp01x100(health);
			m_isDead = false;
			HealthChanged?.Invoke(currentHealth, maxHealth);
		}

		public void ConsumeNeed(NPCNeedType needType, float amount)
		{
			float delta = Mathf.Max(0f, amount);
			switch (needType)
			{
				case NPCNeedType.Hunger: hunger = Clamp01x100(hunger - delta); break;
				case NPCNeedType.Thirst: thirst = Clamp01x100(thirst - delta); break;
				case NPCNeedType.Energy: energy = Clamp01x100(energy + delta); break;
				case NPCNeedType.Work: work = Clamp01x100(work - delta); break;
				case NPCNeedType.Fun: fun = Clamp01x100(fun - delta); break;
				case NPCNeedType.Toilet: toilet = Clamp01x100(toilet - delta); break;
			}
		}

		public void AddNeed(NPCNeedType needType, float amount)
		{
			float delta = Mathf.Max(0f, amount);
			switch (needType)
			{
				case NPCNeedType.Hunger: hunger = Clamp01x100(hunger + delta); break;
				case NPCNeedType.Thirst: thirst = Clamp01x100(thirst + delta); break;
				case NPCNeedType.Energy: energy = Clamp01x100(energy - delta); break;
				case NPCNeedType.Work: work = Clamp01x100(work + delta); break;
				case NPCNeedType.Fun: fun = Clamp01x100(fun + delta); break;
				case NPCNeedType.Toilet: toilet = Clamp01x100(toilet + delta); break;
			}
		}

		private static float Clamp01x100(float value)
		{
			return Mathf.Clamp(value, 0f, 100f);
		}
	}
}
