using UnityEngine;
using Prototype.Business.NPC.AI;

namespace Prototype.Business.NPC.Danger
{
	public sealed class DangerManager : MonoBehaviour
	{
		public static DangerManager Instance { get; private set; }

		[SerializeField] private LayerMask npcLayerMask = ~0;

		private readonly Collider[] m_overlapBuffer = new Collider[256];

		private void Awake()
		{
			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		public void RaiseDangerEvent(Vector3 dangerPosition, float dangerRadius, int threatScore, float dangerTimer, DangerSourceType dangerSource)
		{
			UnityEngine.Debug.Log($"[DangerManager] RaiseDangerEvent pos={dangerPosition} radius={dangerRadius} threat={threatScore} timer={dangerTimer} source={dangerSource}");
			DangerEvent dangerEvent = new DangerEvent
			{
				dangerPosition = dangerPosition,
				dangerRadius = Mathf.Max(0f, dangerRadius),
				threatScore = threatScore,
				dangerTimer = dangerTimer,
				dangerSource = dangerSource
			};

			int count = Physics.OverlapSphereNonAlloc(
				dangerEvent.dangerPosition,
				dangerEvent.dangerRadius,
				m_overlapBuffer,
				npcLayerMask,
				QueryTriggerInteraction.Collide);
			UnityEngine.Debug.Log($"[DangerManager] Overlap count={count}");

			int receiverCount = 0;
			for (int i = 0; i < count; i++)
			{
				Collider c = m_overlapBuffer[i];
				if (c == null)
				{
					continue;
				}

				IDangerReceiver receiver = c.GetComponentInParent<IDangerReceiver>();
				if (receiver == null)
				{
					continue;
				}

				NPCBrain brain = c.GetComponentInParent<NPCBrain>();
				if (brain == null)
				{
					continue;
				}

				receiverCount++;
				receiver.ReceiveDanger(dangerEvent);
			}
			UnityEngine.Debug.Log($"[DangerManager] Receivers notified={receiverCount}");
		}
	}
}
