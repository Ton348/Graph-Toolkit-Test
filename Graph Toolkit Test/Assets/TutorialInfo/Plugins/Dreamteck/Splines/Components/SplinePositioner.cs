using UnityEngine;

namespace Dreamteck.Splines
{
	[AddComponentMenu("Dreamteck/Splines/Users/Spline Positioner")]
	[ExecuteInEditMode]
	public class SplinePositioner : SplineTracer
	{
		public enum Mode
		{
			Percent,
			Distance
		}

		[SerializeField]
		[HideInInspector]
		private GameObject m_targetObject;

		[SerializeField]
		[HideInInspector]
		private SplineTracer m_followTarget;

		[SerializeField]
		[HideInInspector]
		private float m_followTargetDistance;

		[SerializeField]
		[HideInInspector]
		private bool m_followLoop;

		[SerializeField]
		[HideInInspector]
		private Spline.Direction m_followTargetDirection = Spline.Direction.Backward;

		[SerializeField]
		[HideInInspector]
		private float m_position;

		[SerializeField]
		[HideInInspector]
		private Mode m_mode = Mode.Percent;

		private float m_lastPosition;

		public GameObject targetObject
		{
			get
			{
				if (m_targetObject == null)
				{
					return gameObject;
				}

				return m_targetObject;
			}

			set
			{
				if (value != m_targetObject)
				{
					m_targetObject = value;
					RefreshTargets();
					Rebuild();
				}
			}
		}

		public SplineTracer followTarget
		{
			get => m_followTarget;
			set
			{
				if (value != m_followTarget)
				{
					if (m_followTarget != null)
					{
						m_followTarget.onMotionApplied -= OnFollowTargetMotionApplied;
					}

					if (value == this)
					{
						Debug.Log("You should not be assigning a self-reference to the followTarget field.");
						return;
					}

					m_followTarget = value;
					if (m_followTarget != null)
					{
						m_followTarget.onMotionApplied += OnFollowTargetMotionApplied;
						OnFollowTargetMotionApplied();
					}
				}
			}
		}

		public float followTargetDistance
		{
			get => m_followTargetDistance;
			set
			{
				if (value != m_followTargetDistance)
				{
					m_followTargetDistance = value;
					if (followTarget != null)
					{
						OnFollowTargetMotionApplied();
					}
				}
			}
		}

		public bool followLoop
		{
			get => m_followLoop;
			set
			{
				if (value != m_followLoop)
				{
					m_followLoop = value;
					if (followTarget != null)
					{
						OnFollowTargetMotionApplied();
					}
				}
			}
		}

		public Spline.Direction followTargetDirection
		{
			get => m_followTargetDirection;
			set
			{
				if (value != m_followTargetDirection)
				{
					m_followTargetDirection = value;
					if (followTarget != null)
					{
						OnFollowTargetMotionApplied();
					}
				}
			}
		}

		public double position
		{
			get => m_result.percent;
			set
			{
				if (value != m_position)
				{
					m_position = (float)value;
					if (mode == Mode.Distance)
					{
						SetDistance(m_position, true, true);
					}
					else
					{
						SetPercent(value, true, true);
					}
				}
			}
		}

		public Mode mode
		{
			get => m_mode;
			set
			{
				if (value != m_mode)
				{
					m_mode = value;
					Rebuild();
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (m_followTarget != null)
			{
				m_followTarget.onMotionApplied += OnFollowTargetMotionApplied;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (m_followTarget != null)
			{
				m_followTarget.onMotionApplied -= OnFollowTargetMotionApplied;
			}
		}


		protected override void OnDidApplyAnimationProperties()
		{
			if (m_lastPosition != m_position)
			{
				m_lastPosition = m_position;
				if (mode == Mode.Distance)
				{
					SetDistance(m_position, true);
				}
				else
				{
					SetPercent(m_position, true);
				}
			}

			base.OnDidApplyAnimationProperties();
		}

		private void OnFollowTargetMotionApplied()
		{
			float moved;
			double percent = Travel(followTarget.result.percent, m_followTargetDistance, m_followTargetDirection,
				out moved);
			if (m_followLoop)
			{
				if (m_followTargetDistance - moved > 0.000001f)
				{
					if (percent <= 0.000001)
					{
						percent = Travel(1.0, m_followTargetDistance - moved, m_followTargetDirection, out moved);
					}
					else if (percent >= 0.999999)
					{
						percent = Travel(0.0, m_followTargetDistance - moved, m_followTargetDirection, out moved);
					}
				}
			}

			SetPercent(percent, true);
		}

		protected override Transform GetTransform()
		{
			return targetObject.transform;
		}

		protected override Rigidbody GetRigidbody()
		{
			return targetObject.GetComponent<Rigidbody>();
		}

		protected override Rigidbody2D GetRigidbody2D()
		{
			return targetObject.GetComponent<Rigidbody2D>();
		}

		protected override void PostBuild()
		{
			base.PostBuild();
			if (mode == Mode.Distance)
			{
				SetDistance(m_position, true);
			}
			else
			{
				SetPercent(m_position, true);
			}
		}

		public override void SetPercent(double percent, bool checkTriggers = false, bool handleJunctions = false)
		{
			base.SetPercent(percent, checkTriggers, handleJunctions);
			m_position = (float)percent;

			if (!handleJunctions)
			{
				return;
			}

			InvokeNodes();
		}

		public override void SetDistance(float distance, bool checkTriggers = false, bool handleJunctions = false)
		{
			double lastPercent = m_result.percent;
			double travel = Travel(0.0, distance);
			Evaluate(travel, ref m_result);
			ApplyMotion();

			if (checkTriggers)
			{
				CheckTriggers(lastPercent, m_result.percent);
				InvokeTriggers();
			}

			if (handleJunctions)
			{
				CheckNodes(lastPercent, m_result.percent);
			}

			m_position = mode == Mode.Distance ? distance : (float)travel;

			if (!handleJunctions)
			{
				return;
			}

			InvokeNodes();
		}
	}
}