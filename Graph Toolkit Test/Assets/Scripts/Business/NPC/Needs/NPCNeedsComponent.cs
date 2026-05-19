using UnityEngine;

namespace Prototype.Business.NPC.Needs
{
	public sealed class NPCNeedsComponent : MonoBehaviour
	{
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
			m_timer += Time.deltaTime;
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
		}

		private static float Clamp01x100(float value)
		{
			return Mathf.Clamp(value, 0f, 100f);
		}
	}
}
