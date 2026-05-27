using UnityEngine;
using System;

namespace Prototype.Business.NPC.Needs
{
	public sealed class NPCNeedsComponent : MonoBehaviour
	{
		public event Action<NPCNeedType, float> OnNeedThresholdReached;
		[System.Serializable]
		public struct NeedDecay
		{
			public float hungerPerTick;
			public float thirstPerTick;
			public float toiletPerTick;
			public float workPerTick;
			public float funPerTick;
			public float energyPerTick;
		}

		[System.Serializable]
		public struct NeedThresholds
		{
			public float hungerCritical;
			public float thirstCritical;
			public float toiletCritical;
			public float workCritical;
			public float energyLow;
			public float safetyCritical;
			public float funLow;
		}

		[SerializeField] private float hunger = 20f;
		[SerializeField] private float thirst = 20f;
		[SerializeField] private float energy = 80f;
		[SerializeField] private float work = 20f;
		[SerializeField] private float safety = 100f;
		[SerializeField] private float fun = 50f;
		[SerializeField] private float toilet = 10f;
		[SerializeField] private float tickSeconds = 1f;
		[SerializeField] private NeedDecay decay = new NeedDecay
		{
			hungerPerTick = 1f,
			thirstPerTick = 1.2f,
			toiletPerTick = 1f,
			workPerTick = 0.7f,
			funPerTick = 0.5f,
			energyPerTick = 1f
		};
		[SerializeField] private NeedThresholds thresholds = new NeedThresholds
		{
			hungerCritical = 75f,
			thirstCritical = 75f,
			toiletCritical = 80f,
			workCritical = 70f,
			energyLow = 30f,
			safetyCritical = 35f,
			funLow = 30f
		};

		private float m_timer;
		private bool m_hungerThresholdRaised;
		private bool m_thirstThresholdRaised;
		private bool m_toiletThresholdRaised;
		private bool m_funThresholdRaised;
		private bool m_energyThresholdRaised;

		public float Hunger => hunger;
		public float Thirst => thirst;
		public float Energy => energy;
		public float Work => work;
		public float Safety => safety;
		public float Fun => fun;
		public float Toilet => toilet;
		public NeedThresholds Thresholds => thresholds;

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
			hunger = Clamp01x100(hunger + decay.hungerPerTick);
			thirst = Clamp01x100(thirst + decay.thirstPerTick);
			toilet = Clamp01x100(toilet + decay.toiletPerTick);
			work = Clamp01x100(work + decay.workPerTick);
			fun = Clamp01x100(fun + decay.funPerTick);
			energy = Clamp01x100(energy - decay.energyPerTick);
			EvaluateThresholdEvents();
		}

		public void SetSafety(float value)
		{
			safety = Clamp01x100(value);
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
			EvaluateThresholdEvents();
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
			EvaluateThresholdEvents();
		}

		private void EvaluateThresholdEvents()
		{
			float hungerCritical = thresholds.hungerCritical;
			if (!m_hungerThresholdRaised && hunger >= hungerCritical)
			{
				m_hungerThresholdRaised = true;
				OnNeedThresholdReached?.Invoke(NPCNeedType.Hunger, hunger);
			}
			else if (m_hungerThresholdRaised && hunger < hungerCritical)
			{
				m_hungerThresholdRaised = false;
			}

			float thirstCritical = thresholds.thirstCritical;
			if (!m_thirstThresholdRaised && thirst >= thirstCritical)
			{
				m_thirstThresholdRaised = true;
				OnNeedThresholdReached?.Invoke(NPCNeedType.Thirst, thirst);
			}
			else if (m_thirstThresholdRaised && thirst < thirstCritical)
			{
				m_thirstThresholdRaised = false;
			}

			float toiletCritical = thresholds.toiletCritical;
			if (!m_toiletThresholdRaised && toilet >= toiletCritical)
			{
				m_toiletThresholdRaised = true;
				OnNeedThresholdReached?.Invoke(NPCNeedType.Toilet, toilet);
			}
			else if (m_toiletThresholdRaised && toilet < toiletCritical)
			{
				m_toiletThresholdRaised = false;
			}

			float funLow = thresholds.funLow;
			if (!m_funThresholdRaised && fun >= funLow)
			{
				m_funThresholdRaised = true;
				OnNeedThresholdReached?.Invoke(NPCNeedType.Fun, fun);
			}
			else if (m_funThresholdRaised && fun < funLow)
			{
				m_funThresholdRaised = false;
			}

			float energyLow = thresholds.energyLow;
			if (!m_energyThresholdRaised && energy >= energyLow)
			{
				m_energyThresholdRaised = true;
				OnNeedThresholdReached?.Invoke(NPCNeedType.Energy, energy);
			}
			else if (m_energyThresholdRaised && energy < energyLow)
			{
				m_energyThresholdRaised = false;
			}
		}

		private static float Clamp01x100(float value)
		{
			return Mathf.Clamp(value, 0f, 100f);
		}
	}
}
