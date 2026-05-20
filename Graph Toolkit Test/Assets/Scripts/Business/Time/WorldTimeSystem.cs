using System;
using UnityEngine;

namespace Prototype.Business.Time
{
	public class WorldTimeSystem : MonoBehaviour
	{
		public static WorldTimeSystem Instance;

		[Header("Time")]
		[Range(0f, 24f)]
		public float CurrentHour = 12f;
		public int CurrentDay = 1;
		[Tooltip("Сколько реальных минут длится 1 игровые сутки")]
		public float DayDurationMinutes = 24f;

		[Header("Runtime")]
		public DayPhase CurrentPhase;
		public float TimeScale = 1f;

		public event Action<int> OnHourChanged;
		public event Action<int> OnDayChanged;
		public event Action<DayPhase> OnPhaseChanged;

		private int m_lastHourInt = -1;
		private DayPhase m_lastPhase;

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}

			Instance = this;
			CurrentHour = NormalizeHour(CurrentHour);
			CurrentDay = Mathf.Max(1, CurrentDay);
			m_lastHourInt = Mathf.FloorToInt(CurrentHour) % 24;
			CurrentPhase = ResolvePhase(CurrentHour);
			m_lastPhase = CurrentPhase;
		}

		private void Update()
		{
			float daySeconds = Mathf.Max(1f, DayDurationMinutes) * 60f;
			float hoursPerSecond = 24f / daySeconds;
			float deltaHours = UnityEngine.Time.deltaTime * Mathf.Max(0f, TimeScale) * hoursPerSecond;
			if (deltaHours <= 0f)
			{
				return;
			}

			AdvanceHours(deltaHours);
		}

		private void AdvanceHours(float hours)
		{
			float next = CurrentHour + hours;
			while (next >= 24f)
			{
				next -= 24f;
				CurrentDay += 1;
				OnDayChanged?.Invoke(CurrentDay);
			}

			CurrentHour = NormalizeHour(next);
			int hourInt = Mathf.FloorToInt(CurrentHour) % 24;
			if (hourInt != m_lastHourInt)
			{
				m_lastHourInt = hourInt;
				OnHourChanged?.Invoke(hourInt);
			}

			DayPhase phase = ResolvePhase(CurrentHour);
			if (phase != m_lastPhase)
			{
				m_lastPhase = phase;
				CurrentPhase = phase;
				OnPhaseChanged?.Invoke(phase);
			}
		}

		public bool IsNight()
		{
			return CurrentPhase == DayPhase.Night;
		}

		public bool IsBusinessHours(float openHour, float closeHour)
		{
			float now = NormalizeHour(CurrentHour);
			float open = NormalizeHour(openHour);
			float close = NormalizeHour(closeHour);
			if (Mathf.Approximately(open, close))
			{
				return true;
			}

			if (open < close)
			{
				return now >= open && now < close;
			}

			return now >= open || now < close;
		}

		public float GetNormalizedTime01()
		{
			return NormalizeHour(CurrentHour) / 24f;
		}

		public string GetFormattedTime()
		{
			float normalized = NormalizeHour(CurrentHour);
			int h = Mathf.FloorToInt(normalized) % 24;
			float minutesFloat = (normalized - h) * 60f;
			int m = Mathf.Clamp(Mathf.FloorToInt(minutesFloat), 0, 59);
			return $"{h:00}:{m:00}";
		}

		private static float NormalizeHour(float hour)
		{
			float value = hour % 24f;
			if (value < 0f)
			{
				value += 24f;
			}

			return value;
		}

		private static DayPhase ResolvePhase(float hour)
		{
			float h = NormalizeHour(hour);
			if (h >= 6f && h < 10f) return DayPhase.Dawn;
			if (h >= 10f && h < 18f) return DayPhase.Day;
			if (h >= 18f && h < 22f) return DayPhase.Evening;
			return DayPhase.Night;
		}
	}
}
