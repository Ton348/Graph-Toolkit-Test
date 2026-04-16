using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines
{
	public delegate void EmptySplineHandler();

	//MonoBehaviour wrapper for the spline class. It transforms the spline using the object's transform and provides thread-safe methods for sampling
	[AddComponentMenu("Dreamteck/Splines/Spline Computer")]
	[ExecuteInEditMode]
	public class SplineComputer : MonoBehaviour
	{
		public enum EvaluateMode
		{
			Cached,
			Calculate
		}

		public enum SampleMode
		{
			Default,
			Uniform,
			Optimized
		}

		public enum Space
		{
			World,
			Local
		}

		public enum UpdateMode
		{
			Update,
			FixedUpdate,
			LateUpdate,
			AllUpdate,
			None
		}

		[HideInInspector]
		public bool multithreaded;

		[HideInInspector]
		public UpdateMode updateMode = UpdateMode.Update;

		[HideInInspector]
		public TriggerGroup[] triggerGroups = new TriggerGroup[0];

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("spline")]
		private Spline m_spline = new(Spline.Type.CatmullRom);

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("originalSamplePercents")]
		private double[] m_originalSamplePercents = new double[0];

		[HideInInspector]
		[SerializeField]
		private bool m_is2D;

		[HideInInspector]
		[SerializeField]
		private bool m_hasSamples;

		[HideInInspector]
		[SerializeField]
		[Range(0.001f, 45f)]
		private float m_optimizeAngleThreshold = 0.5f;

		[HideInInspector]
		[SerializeField]
		private Space m_space = Space.Local;

		[HideInInspector]
		[SerializeField]
		private SampleMode m_sampleMode = SampleMode.Default;

		[HideInInspector]
		[SerializeField]
		private SplineUser[] m_subscribers = new SplineUser[0];

		[HideInInspector]
		[SerializeField]
		private SplineSample[] m_rawSamples = new SplineSample[0];

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("nodes")]
		private NodeLink[] m_nodes = new NodeLink[0];

		private readonly SampleCollection m_sampleCollection = new();

		private Matrix4x4 m_localToWorldMatrix = Matrix4x4.identity;

		private bool m_queueResample, m_queueRebuild;
		private bool m_rebuildPending;
		private Transform m_trs;
		private bool m_trsCached;
		private Matrix4x4 m_worldToLocalMatrix = Matrix4x4.identity;

		public Space space
		{
			get => m_space;
			set
			{
				if (value != m_space)
				{
					SplinePoint[] worldPoints = GetPoints();
					m_space = value;
					SetPoints(worldPoints);
				}
			}
		}

		public Spline.Type type
		{
			get => m_spline.type;

			set
			{
				if (value != m_spline.type)
				{
					m_spline.type = value;
					Rebuild(true);
				}
			}
		}

		public float knotParametrization
		{
			get => m_spline.knotParametrization;
			set
			{
				float last = m_spline.knotParametrization;
				m_spline.knotParametrization = value;
				if (last != m_spline.knotParametrization)
				{
					Rebuild(true);
				}
			}
		}

		public bool linearAverageDirection
		{
			get => m_spline.linearAverageDirection;

			set
			{
				if (value != m_spline.linearAverageDirection)
				{
					m_spline.linearAverageDirection = value;
					Rebuild(true);
				}
			}
		}

		public bool is2D
		{
			get => m_is2D;
			set
			{
				if (value != m_is2D)
				{
					m_is2D = value;
					SetPoints(GetPoints());
				}
			}
		}

		public int sampleRate
		{
			get => m_spline.sampleRate;
			set
			{
				if (value != m_spline.sampleRate)
				{
					if (value < 2)
					{
						value = 2;
					}

					m_spline.sampleRate = value;
					Rebuild(true);
				}
			}
		}

		public float optimizeAngleThreshold
		{
			get => m_optimizeAngleThreshold;
			set
			{
				if (value != m_optimizeAngleThreshold)
				{
					if (value < 0.001f)
					{
						value = 0.001f;
					}

					m_optimizeAngleThreshold = value;
					if (m_sampleMode == SampleMode.Optimized)
					{
						Rebuild(true);
					}
				}
			}
		}

		public SampleMode sampleMode
		{
			get => m_sampleMode;
			set
			{
				if (value != m_sampleMode)
				{
					m_sampleMode = value;
					Rebuild(true);
				}
			}
		}

		public AnimationCurve customValueInterpolation
		{
			get => m_spline.customValueInterpolation;
			set
			{
				m_spline.customValueInterpolation = value;
				Rebuild();
			}
		}

		public AnimationCurve customNormalInterpolation
		{
			get => m_spline.customNormalInterpolation;
			set
			{
				m_spline.customNormalInterpolation = value;
				Rebuild();
			}
		}

		public int iterations => m_spline.iterations;

		public double moveStep => m_spline.moveStep;

		public bool isClosed => m_spline.isClosed;

		public int pointCount => m_spline.points.Length;

		public int sampleCount => m_sampleCollection.length;

		/// <summary>
		///     Returns the sample at the index transformed by the object's matrix
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SplineSample this[int index]
		{
			get
			{
				UpdateSampleCollection();
				return m_sampleCollection.samples[index];
			}
		}

		/// <summary>
		///     The raw spline samples without transformation applied
		/// </summary>
		public SplineSample[] rawSamples => m_rawSamples;

		/// <summary>
		///     Thread-safe transform's position
		/// </summary>
		public Vector3 position
		{
			get
			{
#if UNITY_EDITOR
				if (!m_editorIsPlaying)
				{
					return transform.position;
				}
#endif
				return m_localToWorldMatrix.MultiplyPoint3x4(Vector3.zero);
			}
		}

		/// <summary>
		///     Thread-safe transform's rotation
		/// </summary>
		public Quaternion rotation
		{
			get
			{
#if UNITY_EDITOR
				if (!m_editorIsPlaying)
				{
					return transform.rotation;
				}
#endif
				return m_localToWorldMatrix.rotation;
			}
		}

		/// <summary>
		///     Thread-safe transform's scale
		/// </summary>
		public Vector3 scale
		{
			get
			{
#if UNITY_EDITOR
				if (!m_editorIsPlaying)
				{
					return transform.lossyScale;
				}
#endif
				return m_localToWorldMatrix.lossyScale;
			}
		}

		/// <summary>
		///     returns the number of subscribers this computer has
		/// </summary>
		public int subscriberCount => m_subscribers.Length;


		public Transform trs
		{
			get
			{
#if UNITY_EDITOR
				if (!m_editorIsPlaying)
				{
					return transform;
				}
#endif
				if (!m_trsCached)
				{
					m_trs = transform;
					m_trsCached = true;
				}

				return m_trs;
			}
		}

		private bool useMultithreading
		{
			get
			{
				return multithreaded
#if UNITY_EDITOR
				       && m_editorIsPlaying
#endif
					;
			}
		}

		private void Awake()
		{
#if UNITY_EDITOR
			m_editorIsPlaying = Application.isPlaying;
#endif
			ResampleTransform();
		}

#if UNITY_EDITOR
		private void Reset()
		{
			editorPathColor = SplinePrefs.defaultColor;
			editorDrawThickness = SplinePrefs.defaultShowThickness;
			is2D = SplinePrefs.default2D;
			editorAlwaysDraw = SplinePrefs.defaultAlwaysDraw;
			editorUpdateMode = SplinePrefs.defaultEditorUpdateMode;
			space = SplinePrefs.defaultComputerSpace;
			type = SplinePrefs.defaultType;
		}
#endif

		private void Update()
		{
			if (updateMode == UpdateMode.Update || updateMode == UpdateMode.AllUpdate)
			{
				RunUpdate();
			}
		}

		private void FixedUpdate()
		{
			if (updateMode == UpdateMode.FixedUpdate || updateMode == UpdateMode.AllUpdate)
			{
				RunUpdate();
			}
		}

		private void LateUpdate()
		{
			if (updateMode == UpdateMode.LateUpdate || updateMode == UpdateMode.AllUpdate)
			{
				RunUpdate();
			}
		}

		private void OnEnable()
		{
			if (m_rebuildPending)
			{
				m_rebuildPending = false;
				Rebuild();
			}
		}

		public event EmptySplineHandler onRebuild;

		private void RunUpdate(bool immediate = false)
		{
			bool transformChanged = ResampleTransformIfNeeded();
			if (m_sampleCollection.samples.Length != m_rawSamples.Length)
			{
				transformChanged = true;
			}

			if (useMultithreading)
			{
				//Rebuild users at the beginning of the next cycle if multithreaded
				if (m_queueRebuild)
				{
					RebuildUsers(immediate);
				}
			}

			if (m_queueResample)
			{
				if (useMultithreading)
				{
					if (transformChanged)
					{
						SplineThreading.Run(CalculateWithoutTransform);
					}
					else
					{
						SplineThreading.Run(CalculateWithTransform);
					}
				}
				else
				{
					CalculateSamples(!transformChanged);
				}
			}

			if (transformChanged)
			{
				if (useMultithreading)
				{
					SplineThreading.Run(TransformSamples);
				}
				else
				{
					TransformSamples();
				}
			}

			if (!useMultithreading)
			{
				//If not multithreaded, rebuild users here
				if (m_queueRebuild)
				{
					RebuildUsers(immediate);
				}
			}

			void CalculateWithTransform()
			{
				CalculateSamples();
			}

			void CalculateWithoutTransform()
			{
				CalculateSamples(false);
			}
		}

		public void GetSamples(SampleCollection collection)
		{
			UpdateSampleCollection();
			collection.samples = m_sampleCollection.samples;
			collection.optimizedIndices = m_sampleCollection.optimizedIndices;
			collection.sampleMode = m_sampleMode;
		}

		private void UpdateSampleCollection()
		{
			if (m_sampleCollection.samples.Length != m_rawSamples.Length)
			{
				TransformSamples();
			}
		}

		private bool ResampleTransformIfNeeded()
		{
			var changed = false;
			//This is used to skip comparing matrices on every frame during runtime
#if UNITY_EDITOR
			if (m_editorIsPlaying)
			{
#endif
				if (!trs.hasChanged)
				{
					return false;
				}

				trs.hasChanged = false;
#if UNITY_EDITOR
			}
#endif

			if (m_localToWorldMatrix != trs.localToWorldMatrix)
			{
				ResampleTransform();
				m_queueRebuild = true;
				changed = true;
			}

			return changed;
		}

		/// <summary>
		///     Immediately sample the computer's transform (thread-unsafe). Call this before SetPoint(s) if the transform has been
		///     modified in the same frame
		/// </summary>
		public void ResampleTransform()
		{
			m_localToWorldMatrix = trs.localToWorldMatrix;
			m_worldToLocalMatrix = trs.worldToLocalMatrix;
		}

		/// <summary>
		///     Subscribe a SplineUser to this computer. This will rebuild the user automatically when there are changes.
		/// </summary>
		/// <param name="input">The SplineUser to subscribe</param>
		public void Subscribe(SplineUser input)
		{
			if (!IsSubscribed(input))
			{
				ArrayUtility.Add(ref m_subscribers, input);
			}
		}

		/// <summary>
		///     Unsubscribe a SplineUser from this computer's updates
		/// </summary>
		/// <param name="input">The SplineUser to unsubscribe</param>
		public void Unsubscribe(SplineUser input)
		{
			for (var i = 0; i < m_subscribers.Length; i++)
			{
				if (m_subscribers[i] == input)
				{
					ArrayUtility.RemoveAt(ref m_subscribers, i);
					return;
				}
			}
		}

		/// <summary>
		///     Checks if a user is subscribed to that computer
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public bool IsSubscribed(SplineUser user)
		{
			for (var i = 0; i < m_subscribers.Length; i++)
			{
				if (m_subscribers[i] == user)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		///     Returns an array of subscribed users
		/// </summary>
		/// <returns></returns>
		public SplineUser[] GetSubscribers()
		{
			var subs = new SplineUser[m_subscribers.Length];
			m_subscribers.CopyTo(subs, 0);
			return subs;
		}

		/// <summary>
		///     Get the points from this computer's spline. All points are transformed in world coordinates.
		/// </summary>
		/// <returns></returns>
		public SplinePoint[] GetPoints(Space getSpace = Space.World)
		{
			var points = new SplinePoint[m_spline.points.Length];
			for (var i = 0; i < points.Length; i++)
			{
				points[i] = m_spline.points[i];
				if (m_space == Space.Local && getSpace == Space.World)
				{
					points[i].position = TransformPoint(points[i].position);
					points[i].tangent = TransformPoint(points[i].tangent);
					points[i].tangent2 = TransformPoint(points[i].tangent2);
					points[i].normal = TransformDirection(points[i].normal);
				}
			}

			return points;
		}

		/// <summary>
		///     Get a point from this computer's spline. The point is transformed in world coordinates.
		/// </summary>
		/// <param name="index">Point index</param>
		/// <returns></returns>
		public SplinePoint GetPoint(int index, Space getSpace = Space.World)
		{
			if (index < 0 || index >= m_spline.points.Length)
			{
				return new SplinePoint();
			}

			if (m_space == Space.Local && getSpace == Space.World)
			{
				ResampleTransformIfNeeded();
				SplinePoint point = m_spline.points[index];
				point.position = TransformPoint(point.position);
				point.tangent = TransformPoint(point.tangent);
				point.tangent2 = TransformPoint(point.tangent2);
				point.normal = TransformDirection(point.normal);
				return point;
			}

			return m_spline.points[index];
		}

		public Vector3 GetPointPosition(int index, Space getSpace = Space.World)
		{
			if (m_space == Space.Local && getSpace == Space.World)
			{
				ResampleTransformIfNeeded();
				return TransformPoint(m_spline.points[index].position);
			}

			return m_spline.points[index].position;
		}

		public Vector3 GetPointNormal(int index, Space getSpace = Space.World)
		{
			if (m_space == Space.Local && getSpace == Space.World)
			{
				ResampleTransformIfNeeded();
				return TransformDirection(m_spline.points[index].normal).normalized;
			}

			return m_spline.points[index].normal;
		}

		public Vector3 GetPointTangent(int index, Space getSpace = Space.World)
		{
			if (m_space == Space.Local && getSpace == Space.World)
			{
				ResampleTransformIfNeeded();
				return TransformPoint(m_spline.points[index].tangent);
			}

			return m_spline.points[index].tangent;
		}

		public Vector3 GetPointTangent2(int index, Space getSpace = Space.World)
		{
			if (m_space == Space.Local && getSpace == Space.World)
			{
				ResampleTransformIfNeeded();
				return TransformPoint(m_spline.points[index].tangent2);
			}

			return m_spline.points[index].tangent2;
		}

		public float GetPointSize(int index, Space getSpace = Space.World)
		{
			return m_spline.points[index].size;
		}

		public Color GetPointColor(int index, Space getSpace = Space.World)
		{
			return m_spline.points[index].color;
		}

		private void Make2D(ref SplinePoint point)
		{
			point.Flatten(LinearAlgebraUtility.Axis.Z);
		}

		/// <summary>
		///     Set the points of this computer's spline.
		/// </summary>
		/// <param name="points">The points array</param>
		/// <param name="setSpace">Use world or local space</param>
		public void SetPoints(SplinePoint[] points, Space setSpace = Space.World)
		{
			ResampleTransformIfNeeded();
			var rebuild = false;
			if (points.Length != m_spline.points.Length)
			{
				rebuild = true;
				if (points.Length < 3)
				{
					Break();
				}

				m_spline.points = new SplinePoint[points.Length];
				SetAllDirty();
			}

			for (var i = 0; i < points.Length; i++)
			{
				SplinePoint newPoint = points[i];
				if (m_spline.points.Length > i)
				{
					newPoint.isDirty = m_spline.points[i].isDirty;
				}

				if (m_space == Space.Local && setSpace == Space.World)
				{
					newPoint.position = InverseTransformPoint(points[i].position);
					newPoint.tangent = InverseTransformPoint(points[i].tangent);
					newPoint.tangent2 = InverseTransformPoint(points[i].tangent2);
					newPoint.normal = InverseTransformDirection(points[i].normal);
				}

				if (m_is2D)
				{
					Make2D(ref newPoint);
				}

				if (newPoint != m_spline.points[i])
				{
					newPoint.isDirty = true;
					rebuild = true;
				}

				m_spline.points[i] = newPoint;
			}

			if (rebuild)
			{
				Rebuild();
				UpdateConnectedNodes(points);
			}
		}

		/// <summary>
		///     Set the position of a control point. This is faster than SetPoint
		/// </summary>
		/// <param name="index"></param>
		/// <param name="pos"></param>
		/// <param name="setSpace"></param>
		public void SetPointPosition(int index, Vector3 pos, Space setSpace = Space.World)
		{
			if (index < 0)
			{
				return;
			}

			ResampleTransformIfNeeded();
			if (index >= m_spline.points.Length)
			{
				AppendPoints(index + 1 - m_spline.points.Length);
			}

			Vector3 newPos = pos;
			if (m_space == Space.Local && setSpace == Space.World)
			{
				newPos = InverseTransformPoint(pos);
			}

			if (newPos != m_spline.points[index].position)
			{
				SetDirty(index);
				m_spline.points[index].SetPosition(newPos);
				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}

		/// <summary>
		///     Set the tangents of a control point. This is faster than SetPoint
		/// </summary>
		/// <param name="index"></param>
		/// <param name="tan1"></param>
		/// <param name="tan2"></param>
		/// <param name="setSpace"></param>
		public void SetPointTangents(int index, Vector3 tan1, Vector3 tan2, Space setSpace = Space.World)
		{
			if (index < 0)
			{
				return;
			}

			ResampleTransformIfNeeded();
			if (index >= m_spline.points.Length)
			{
				AppendPoints(index + 1 - m_spline.points.Length);
			}

			Vector3 newTan1 = tan1;
			Vector3 newTan2 = tan2;
			if (m_space == Space.Local && setSpace == Space.World)
			{
				newTan1 = InverseTransformPoint(tan1);
				newTan2 = InverseTransformPoint(tan2);
			}

			var rebuild = false;
			if (newTan2 != m_spline.points[index].tangent2)
			{
				rebuild = true;
				m_spline.points[index].SetTangent2Position(newTan2);
			}

			if (newTan1 != m_spline.points[index].tangent)
			{
				rebuild = true;
				m_spline.points[index].SetTangentPosition(newTan1);
			}

			if (m_is2D)
			{
				Make2D(ref m_spline.points[index]);
			}

			if (rebuild)
			{
				SetDirty(index);
				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}

		/// <summary>
		///     Set the normal of a control point. This is faster than SetPoint
		/// </summary>
		/// <param name="index"></param>
		/// <param name="nrm"></param>
		/// <param name="setSpace"></param>
		public void SetPointNormal(int index, Vector3 nrm, Space setSpace = Space.World)
		{
			if (index < 0)
			{
				return;
			}

			ResampleTransformIfNeeded();
			if (index >= m_spline.points.Length)
			{
				AppendPoints(index + 1 - m_spline.points.Length);
			}

			Vector3 newNrm = nrm;
			if (m_space == Space.Local && setSpace == Space.World)
			{
				newNrm = InverseTransformDirection(nrm);
			}

			if (newNrm != m_spline.points[index].normal)
			{
				SetDirty(index);
				m_spline.points[index].normal = newNrm;
				if (m_is2D)
				{
					Make2D(ref m_spline.points[index]);
				}

				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}

		/// <summary>
		///     Set the size of a control point. This is faster than SetPoint
		/// </summary>
		/// <param name="index"></param>
		/// <param name="size"></param>
		public void SetPointSize(int index, float size)
		{
			if (index < 0)
			{
				return;
			}

			if (index >= m_spline.points.Length)
			{
				AppendPoints(index + 1 - m_spline.points.Length);
			}

			if (size != m_spline.points[index].size)
			{
				SetDirty(index);
				m_spline.points[index].size = size;
				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}

		/// <summary>
		///     Set the color of a control point. THis is faster than SetPoint
		/// </summary>
		/// <param name="index"></param>
		/// <param name="color"></param>
		public void SetPointColor(int index, Color color)
		{
			if (index < 0)
			{
				return;
			}

			if (index >= m_spline.points.Length)
			{
				AppendPoints(index + 1 - m_spline.points.Length);
			}

			if (color != m_spline.points[index].color)
			{
				SetDirty(index);
				m_spline.points[index].color = color;
				Rebuild();
				SetNodeForPoint(index, GetPoint(index));
			}
		}

		/// <summary>
		///     Set a control point in world coordinates
		/// </summary>
		/// <param name="index"></param>
		/// <param name="point"></param>
		public void SetPoint(int index, SplinePoint point, Space setSpace = Space.World)
		{
			if (index < 0)
			{
				return;
			}

			ResampleTransformIfNeeded();
			if (index >= m_spline.points.Length)
			{
				AppendPoints(index + 1 - m_spline.points.Length);
			}

			SplinePoint newPoint = point;
			if (m_space == Space.Local && setSpace == Space.World)
			{
				newPoint.position = InverseTransformPoint(point.position);
				newPoint.tangent = InverseTransformPoint(point.tangent);
				newPoint.tangent2 = InverseTransformPoint(point.tangent2);
				newPoint.normal = InverseTransformDirection(point.normal);
			}

			if (m_is2D)
			{
				Make2D(ref newPoint);
			}

			if (newPoint != m_spline.points[index])
			{
				newPoint.isDirty = true;
				m_spline.points[index] = newPoint;
				Rebuild();
				SetNodeForPoint(index, point);
			}
		}

		private void AppendPoints(int count)
		{
			var newPoints = new SplinePoint[m_spline.points.Length + count];
			m_spline.points.CopyTo(newPoints, 0);
			m_spline.points = newPoints;
			Rebuild(true);
		}

		/// <summary>
		///     Converts a point index to spline percent
		/// </summary>
		/// <param name="pointIndex">The point index</param>
		/// <returns></returns>
		public double GetPointPercent(int pointIndex)
		{
			double percent = Dmath.Clamp01((double)pointIndex / (m_spline.points.Length - 1));
			if (m_spline.isClosed)
			{
				percent = Dmath.Clamp01((double)pointIndex / m_spline.points.Length);
			}

			if (m_sampleMode != SampleMode.Uniform)
			{
				return percent;
			}

			if (m_originalSamplePercents.Length <= 1)
			{
				return 0.0;
			}

			for (int i = m_originalSamplePercents.Length - 2; i >= 0; i--)
			{
				if (m_originalSamplePercents[i] < percent)
				{
					double inverseLerp = Dmath.InverseLerp(m_originalSamplePercents[i], m_originalSamplePercents[i + 1],
						percent);
					return Dmath.Lerp(m_rawSamples[i].percent, m_rawSamples[i + 1].percent, inverseLerp);
				}
			}

			return 0.0;
		}

		public int PercentToPointIndex(double percent, Spline.Direction direction = Spline.Direction.Forward)
		{
			int count = m_spline.points.Length - 1;
			if (isClosed)
			{
				count = m_spline.points.Length;
			}

			if (m_sampleMode == SampleMode.Uniform)
			{
				int index;
				double lerp;
				GetSamplingValues(percent, out index, out lerp);
				if (lerp > 0.0 && index < m_originalSamplePercents.Length - 1)
				{
					lerp = Dmath.Lerp(m_originalSamplePercents[index], m_originalSamplePercents[index + 1], lerp);
					if (direction == Spline.Direction.Forward)
					{
						return Dmath.FloorInt(lerp * count);
					}

					return Dmath.CeilInt(lerp * count);
				}

				if (direction == Spline.Direction.Forward)
				{
					return Dmath.FloorInt(m_originalSamplePercents[index] * count);
				}

				return Dmath.CeilInt(m_originalSamplePercents[index] * count);
			}

			var point = 0;
			if (direction == Spline.Direction.Forward)
			{
				point = Dmath.FloorInt(percent * count);
			}
			else
			{
				point = Dmath.CeilInt(percent * count);
			}

			if (point >= m_spline.points.Length)
			{
				point = 0;
			}

			return point;
		}

		public Vector3 EvaluatePosition(double percent)
		{
			return EvaluatePosition(percent, EvaluateMode.Cached);
		}

		/// <summary>
		///     Same as Spline.EvaluatePosition but the result is transformed by the computer's transform
		/// </summary>
		/// <param name="percent">Evaluation percent</param>
		/// <param name="mode">
		///     Mode to use the method in. Cached uses the cached samples while Calculate is more accurate but
		///     heavier
		/// </param>
		/// <returns></returns>
		public Vector3 EvaluatePosition(double percent, EvaluateMode mode = EvaluateMode.Cached)
		{
			if (mode == EvaluateMode.Calculate)
			{
				return TransformPoint(m_spline.EvaluatePosition(percent));
			}

			UpdateSampleCollection();
			return m_sampleCollection.EvaluatePosition(percent);
		}

		public Vector3 EvaluatePosition(int pointIndex, EvaluateMode mode = EvaluateMode.Cached)
		{
			return EvaluatePosition(GetPointPercent(pointIndex), mode);
		}

		public SplineSample Evaluate(double percent)
		{
			return Evaluate(percent, EvaluateMode.Cached);
		}

		/// <summary>
		///     Same as Spline.Evaluate but the result is transformed by the computer's transform
		/// </summary>
		/// <param name="percent">Evaluation percent</param>
		/// <param name="mode">
		///     Mode to use the method in. Cached uses the cached samples while Calculate is more accurate but
		///     heavier
		/// </param>
		/// <returns></returns>
		public SplineSample Evaluate(double percent, EvaluateMode mode = EvaluateMode.Cached)
		{
			var result = new SplineSample();
			Evaluate(percent, ref result, mode);
			return result;
		}

		/// <summary>
		///     Evaluate the spline at the position of a given point and return a SplineSample
		/// </summary>
		/// <param name="pointIndex">Point index</param>
		/// <param name="mode">
		///     Mode to use the method in. Cached uses the cached samples while Calculate is more accurate but
		///     heavier
		/// </param>
		public SplineSample Evaluate(int pointIndex)
		{
			var result = new SplineSample();
			Evaluate(pointIndex, ref result);
			return result;
		}

		/// <summary>
		///     Evaluate the spline at the position of a given point and write in the SplineSample output
		/// </summary>
		/// <param name="pointIndex">Point index</param>
		public void Evaluate(int pointIndex, ref SplineSample result)
		{
			Evaluate(GetPointPercent(pointIndex), ref result);
		}

		public void Evaluate(double percent, ref SplineSample result)
		{
			Evaluate(percent, ref result, EvaluateMode.Cached);
		}

		/// <summary>
		///     Same as Spline.Evaluate but the result is transformed by the computer's transform
		/// </summary>
		/// <param name="result"></param>
		/// <param name="percent"></param>
		public void Evaluate(double percent, ref SplineSample result, EvaluateMode mode = EvaluateMode.Cached)
		{
			if (mode == EvaluateMode.Calculate)
			{
				m_spline.Evaluate(percent, ref result);
				TransformSample(ref result);
			}
			else
			{
				UpdateSampleCollection();
				m_sampleCollection.Evaluate(percent, ref result);
			}
		}

		/// <summary>
		///     Same as Spline.Evaluate but the results are transformed by the computer's transform
		/// </summary>
		/// <param name="from">Start position [0-1]</param>
		/// <param name="to">Target position [from-1]</param>
		/// <returns></returns>
		public void Evaluate(ref SplineSample[] results, double from = 0.0, double to = 1.0)
		{
			UpdateSampleCollection();
			m_sampleCollection.Evaluate(ref results, from, to);
		}

		/// <summary>
		///     Same as Spline.EvaluatePositions but the results are transformed by the computer's transform
		/// </summary>
		/// <param name="from">Start position [0-1]</param>
		/// <param name="to">Target position [from-1]</param>
		/// <returns></returns>
		public void EvaluatePositions(ref Vector3[] positions, double from = 0.0, double to = 1.0)
		{
			UpdateSampleCollection();
			m_sampleCollection.EvaluatePositions(ref positions, from, to);
		}

		/// <summary>
		///     Returns the percent from the spline at a given distance from the start point
		/// </summary>
		/// <param name="start">The start point</param>
		/// ///
		/// <param name="distance">The distance to travel</param>
		/// <param name="direction">The direction towards which to move</param>
		/// <returns></returns>
		public double Travel(
			double start,
			float distance,
			out float moved,
			Spline.Direction direction = Spline.Direction.Forward)
		{
			UpdateSampleCollection();
			return m_sampleCollection.Travel(start, distance, direction, out moved);
		}

		public double Travel(double start, float distance, Spline.Direction direction = Spline.Direction.Forward)
		{
			float moved;
			return Travel(start, distance, out moved, direction);
		}


		[Obsolete(
			"This project override is obsolete, please use Project(Vector3 position, ref SplineSample result, double from = 0.0, double to = 1.0, EvaluateMode mode = EvaluateMode.Cached, int subdivisions = 4) instead")]
		public void Project(
			ref SplineSample result,
			Vector3 position,
			double from = 0.0,
			double to = 1.0,
			EvaluateMode mode = EvaluateMode.Cached,
			int subdivisions = 4)
		{
			Project(position, ref result, from, to, mode, subdivisions);
		}

		/// <summary>
		///     Same as Spline.Project but the point is transformed by the computer's transform.
		/// </summary>
		/// <param name="worldPoint">Point in world space</param>
		/// <param name="subdivide">Subdivisions default: 4</param>
		/// <param name="from">Sample from [0-1] default: 0f</param>
		/// <param name="to">Sample to [0-1] default: 1f</param>
		/// <param name="mode">
		///     Mode to use the method in. Cached uses the cached samples while Calculate is more accurate but
		///     heavier
		/// </param>
		/// <param name="subdivisions">Subdivisions for the Calculate mode. Don't assign if not using Calculated mode.</param>
		/// <returns></returns>
		public void Project(
			Vector3 worldPoint,
			ref SplineSample result,
			double from = 0.0,
			double to = 1.0,
			EvaluateMode mode = EvaluateMode.Cached,
			int subdivisions = 4)
		{
			if (mode == EvaluateMode.Calculate)
			{
				worldPoint = InverseTransformPoint(worldPoint);
				double percent = m_spline.Project(InverseTransformPoint(worldPoint), subdivisions, from, to);
				m_spline.Evaluate(percent, ref result);
				TransformSample(ref result);
				return;
			}

			UpdateSampleCollection();
			m_sampleCollection.Project(worldPoint, m_spline.points.Length, ref result, from, to);
		}

		public SplineSample Project(Vector3 worldPoint, double from = 0.0, double to = 1.0)
		{
			var result = new SplineSample();
			Project(worldPoint, ref result, from, to);
			return result;
		}

		/// <summary>
		///     Same as Spline.CalculateLength but this takes the computer's transform into account when calculating the length.
		/// </summary>
		/// <param name="from">Calculate from [0-1] default: 0f</param>
		/// <param name="to">Calculate to [0-1] default: 1f</param>
		/// <param name="resolution">Resolution [0-1] default: 1f</param>
		/// <param name="address">Node address of junctions</param>
		/// <returns></returns>
		public float CalculateLength(double from = 0.0, double to = 1.0)
		{
			if (!m_hasSamples)
			{
				return 0f;
			}

			UpdateSampleCollection();
			return m_sampleCollection.CalculateLength(from, to);
		}

		private void TransformSample(ref SplineSample result)
		{
			result.position = m_localToWorldMatrix.MultiplyPoint3x4(result.position);
			result.forward = m_localToWorldMatrix.MultiplyVector(result.forward);
			result.up = m_localToWorldMatrix.MultiplyVector(result.up);
		}

		public void Rebuild(bool forceUpdateAll = false)
		{
			if (forceUpdateAll)
			{
				SetAllDirty();
			}

#if UNITY_EDITOR
			if (!m_editorIsPlaying)
			{
				if (editorUpdateMode == EditorUpdateMode.Default)
				{
					RebuildImmediate(true);
				}

				return;
			}
#endif

			m_queueResample = updateMode != UpdateMode.None;
		}

		public void RebuildImmediate()
		{
			RebuildImmediate(true, true);
		}

		public void RebuildImmediate(bool calculateSamples = true, bool forceUpdateAll = false)
		{
			if (calculateSamples)
			{
				m_queueResample = true;
				if (forceUpdateAll)
				{
					SetAllDirty();
				}
			}
			else
			{
				m_queueResample = false;
			}

			RunUpdate(true);
		}

		private void RebuildUsers(bool immediate = false)
		{
			for (int i = m_subscribers.Length - 1; i >= 0; i--)
			{
				if (m_subscribers[i] != null)
				{
					if (immediate)
					{
						m_subscribers[i].RebuildImmediate();
					}
					else
					{
						m_subscribers[i].Rebuild();
					}
				}
				else
				{
					ArrayUtility.RemoveAt(ref m_subscribers, i);
				}
			}

			if (onRebuild != null)
			{
				onRebuild();
			}

			m_queueRebuild = false;
		}

		private void SetAllDirty()
		{
			for (var i = 0; i < m_spline.points.Length; i++)
			{
				m_spline.points[i].isDirty = true;
			}
		}

		private void SetDirty(int index)
		{
			if (sampleMode == SampleMode.Uniform)
			{
				SetAllDirty();
				return;
			}

			m_spline.points[index].isDirty = true;
		}

		private void CalculateSamples(bool transformSamples = true)
		{
			m_queueResample = false;
			m_queueRebuild = true;
			if (m_spline.points.Length == 0)
			{
				if (m_rawSamples.Length != 0)
				{
					m_rawSamples = new SplineSample[0];
					if (transformSamples)
					{
						TransformSamples();
					}
				}

				return;
			}

			if (m_spline.points.Length == 1)
			{
				if (m_rawSamples.Length != 1)
				{
					m_rawSamples = new SplineSample[1];
					if (transformSamples)
					{
						TransformSamples();
					}
				}

				m_spline.Evaluate(0.0, ref m_rawSamples[0]);
				return;
			}

			if (m_sampleMode == SampleMode.Uniform)
			{
				m_spline.EvaluateUniform(ref m_rawSamples, ref m_originalSamplePercents);
				if (transformSamples)
				{
					TransformSamples();
				}
			}
			else
			{
				if (m_originalSamplePercents.Length > 0)
				{
					m_originalSamplePercents = new double[0];
				}

				if (m_rawSamples.Length != m_spline.iterations)
				{
					m_rawSamples = new SplineSample[m_spline.iterations];
					for (var i = 0; i < m_rawSamples.Length; i++)
					{
						m_rawSamples[i] = new SplineSample();
					}
				}

				if (m_sampleCollection.samples.Length != m_rawSamples.Length)
				{
					m_sampleCollection.samples = new SplineSample[m_rawSamples.Length];
				}

				for (var i = 0; i < m_rawSamples.Length; i++)
				{
					double percent = (double)i / (m_rawSamples.Length - 1);
					if (IsDirtySample(percent))
					{
						m_spline.Evaluate(percent, ref m_rawSamples[i]);
						m_sampleCollection.samples[i].FastCopy(ref m_rawSamples[i]);
						if (transformSamples && m_space == Space.Local)
						{
							TransformSample(ref m_sampleCollection.samples[i]);
						}
					}
				}

				if (m_sampleMode == SampleMode.Optimized && m_rawSamples.Length > 2)
				{
					OptimizeSamples(space == Space.Local);
				}
				else
				{
					if (m_sampleCollection.optimizedIndices.Length > 0)
					{
						m_sampleCollection.optimizedIndices = new int[0];
					}
				}
			}

			m_sampleCollection.sampleMode = m_sampleMode;
			m_hasSamples = m_sampleCollection.length > 0;

			for (var i = 0; i < m_spline.points.Length; i++)
			{
				m_spline.points[i].isDirty = false;
			}
		}

		private void OptimizeSamples(bool transformSamples)
		{
			if (m_sampleCollection.optimizedIndices.Length != m_rawSamples.Length)
			{
				m_sampleCollection.optimizedIndices = new int[m_rawSamples.Length];
			}

			Vector3 lastDirection = m_rawSamples[0].forward;
			var optimized = new List<SplineSample>();
			for (var i = 0; i < m_rawSamples.Length; i++)
			{
				SplineSample sample = m_rawSamples[i];
				if (transformSamples)
				{
					TransformSample(ref sample);
				}

				Vector3 direction = sample.forward;
				if (i < m_rawSamples.Length - 1)
				{
					Vector3 pos = m_rawSamples[i + 1].position;
					if (transformSamples)
					{
						pos = m_localToWorldMatrix.MultiplyPoint3x4(pos);
					}

					direction = pos - sample.position;
				}

				float angle = Vector3.Angle(lastDirection, direction);
				bool includeSample = angle >= m_optimizeAngleThreshold || i == 0 || i == m_rawSamples.Length - 1;

				if (includeSample)
				{
					optimized.Add(sample);
					lastDirection = direction;
				}

				m_sampleCollection.optimizedIndices[i] = optimized.Count - 1;
			}

			m_sampleCollection.samples = optimized.ToArray();
		}

		private void TransformSamples()
		{
			if (m_sampleCollection.samples.Length != m_rawSamples.Length)
			{
				m_sampleCollection.samples = new SplineSample[m_rawSamples.Length];
			}

			if (m_sampleMode == SampleMode.Optimized && m_rawSamples.Length > 2)
			{
				OptimizeSamples(m_space == Space.Local);
			}
			else
			{
				for (var i = 0; i < m_rawSamples.Length; i++)
				{
					m_sampleCollection.samples[i].FastCopy(ref m_rawSamples[i]);
					if (m_space == Space.Local)
					{
						TransformSample(ref m_sampleCollection.samples[i]);
					}
				}
			}
		}

		private bool IsDirtySample(double percent)
		{
			if (m_sampleMode == SampleMode.Uniform)
			{
				return true;
			}

			int currentPoint = PercentToPointIndex(percent);

			int from = currentPoint - 1;
			int to = currentPoint + 2;

			if (m_spline.type == Spline.Type.Bezier || m_spline.type == Spline.Type.Linear)
			{
				from = currentPoint;
				to = currentPoint + 1;
			}

			int fromClamped = Mathf.Clamp(from, 0, m_spline.points.Length - 1);
			int toClamped = Mathf.Clamp(to, 0, m_spline.points.Length - 1);

			for (int i = fromClamped; i <= toClamped; i++)
			{
				if (m_spline.points[i].isDirty)
				{
					return true;
				}
			}

			if (m_spline.isClosed)
			{
				if (from < 0)
				{
					for (int i = from + m_spline.points.Length; i < m_spline.points.Length; i++)
					{
						if (m_spline.points[i].isDirty)
						{
							return true;
						}
					}
				}

				if (to >= m_spline.points.Length)
				{
					for (var i = 0; i <= to - m_spline.points.Length; i++)
					{
						if (m_spline.points[i].isDirty)
						{
							return true;
						}
					}
				}
			}

			if (currentPoint > 0 && !m_spline.points[currentPoint].isDirty)
			{
				int count = m_spline.points.Length - 1;
				if (m_spline.isClosed)
				{
					count = m_spline.points.Length;
				}

				double currentPointPercent = (double)currentPoint / count;

				if (Mathf.Abs((float)(currentPointPercent - percent)) <= 0.00001f)
				{
					return m_spline.points[currentPoint - 1].isDirty;
				}
			}

			return false;
		}

		/// <summary>
		///     Same as Spline.Break() but it will update all subscribed users
		/// </summary>
		public void Break()
		{
			Break(0);
		}

		/// <summary>
		///     Same as Spline.Break(at) but it will update all subscribed users
		/// </summary>
		/// <param name="at"></param>
		public void Break(int at)
		{
			if (m_spline.isClosed)
			{
				m_spline.Break(at);
				SetAllDirty();
				Rebuild();
			}
		}

		/// <summary>
		///     Same as Spline.Close() but it will update all subscribed users
		/// </summary>
		public void Close()
		{
			if (!m_spline.isClosed)
			{
				if (m_spline.points.Length >= 3)
				{
					m_spline.Close();
					SetAllDirty();
					Rebuild();
				}
				else
				{
					Debug.LogError("Spline " + name +
					               " needs at least 3 points before it can be closed. Current points: " +
					               m_spline.points.Length);
				}
			}
		}

		/// <summary>
		///     Same as Spline.HermiteToBezierTangents() but it will update all subscribed users
		/// </summary>
		public void CatToBezierTangents()
		{
			m_spline.CatToBezierTangents();
			SetPoints(m_spline.points, Space.Local);
		}

		/// <summary>
		///     Casts a ray along the transformed spline against all scene colliders.
		/// </summary>
		/// <param name="hit">Hit information</param>
		/// <param name="hitPercent">The percent of evaluation where the hit occured</param>
		/// <param name="layerMask">Layer mask for the raycast</param>
		/// <param name="resolution">Resolution multiplier for precision [0-1] default: 1f</param>
		/// <param name="from">Raycast from [0-1] default: 0f</param>
		/// <param name="to">Raycast to [0-1] default: 1f</param>
		/// <param name="hitTriggers">Should hit triggers? (not supported in 5.1)</param>
		/// <param name="address">Node address of junctions</param>
		/// <returns></returns>
		public bool Raycast(
			out RaycastHit hit,
			out double hitPercent,
			LayerMask layerMask,
			double resolution = 1.0,
			double from = 0.0,
			double to = 1.0,
			QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal)
		{
			resolution = Dmath.Clamp01(resolution);
			Spline.FormatFromTo(ref from, ref to, false);
			double percent = from;
			Vector3 fromPos = EvaluatePosition(percent);
			hitPercent = 0f;
			while (true)
			{
				double prevPercent = percent;
				percent = Dmath.Move(percent, to, moveStep / resolution);
				Vector3 toPos = EvaluatePosition(percent);
				if (Physics.Linecast(fromPos, toPos, out hit, layerMask, hitTriggers))
				{
					double segmentPercent = (hit.point - fromPos).sqrMagnitude / (toPos - fromPos).sqrMagnitude;
					hitPercent = Dmath.Lerp(prevPercent, percent, segmentPercent);
					return true;
				}

				fromPos = toPos;
				if (percent == to)
				{
					break;
				}
			}

			return false;
		}

		/// <summary>
		///     Casts a ray along the transformed spline against all scene colliders and returns all hits. Order is not guaranteed.
		/// </summary>
		/// <param name="hit">Hit information</param>
		/// <param name="hitPercent">The percents of evaluation where each hit occured</param>
		/// <param name="layerMask">Layer mask for the raycast</param>
		/// <param name="resolution">Resolution multiplier for precision [0-1] default: 1f</param>
		/// <param name="from">Raycast from [0-1] default: 0f</param>
		/// <param name="to">Raycast to [0-1] default: 1f</param>
		/// <param name="hitTriggers">Should hit triggers? (not supported in 5.1)</param>
		/// <param name="address">Node address of junctions</param>
		/// <returns></returns>
		public bool RaycastAll(
			out RaycastHit[] hits,
			out double[] hitPercents,
			LayerMask layerMask,
			double resolution = 1.0,
			double from = 0.0,
			double to = 1.0,
			QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal)
		{
			resolution = Dmath.Clamp01(resolution);
			Spline.FormatFromTo(ref from, ref to, false);
			double percent = from;
			Vector3 fromPos = EvaluatePosition(percent);
			var hitList = new List<RaycastHit>();
			var percentList = new List<double>();
			var hasHit = false;
			while (true)
			{
				double prevPercent = percent;
				percent = Dmath.Move(percent, to, moveStep / resolution);
				Vector3 toPos = EvaluatePosition(percent);
				RaycastHit[] h = Physics.RaycastAll(fromPos, toPos - fromPos, Vector3.Distance(fromPos, toPos),
					layerMask, hitTriggers);
				for (var i = 0; i < h.Length; i++)
				{
					hasHit = true;
					double segmentPercent = (h[i].point - fromPos).sqrMagnitude / (toPos - fromPos).sqrMagnitude;
					percentList.Add(Dmath.Lerp(prevPercent, percent, segmentPercent));
					hitList.Add(h[i]);
				}

				fromPos = toPos;
				if (percent == to)
				{
					break;
				}
			}

			hits = hitList.ToArray();
			hitPercents = percentList.ToArray();
			return hasHit;
		}

		public TriggerGroup AddTriggerGroup()
		{
			var newGroup = new TriggerGroup();
			ArrayUtility.Add(ref triggerGroups, newGroup);
			return newGroup;
		}

		public SplineTrigger AddTrigger(int triggerGroup, double position, SplineTrigger.Type type)
		{
			return AddTrigger(triggerGroup, position, type, "API Trigger", Color.white);
		}

		public SplineTrigger AddTrigger(
			int triggerGroup,
			double position,
			SplineTrigger.Type type,
			string name,
			Color color)
		{
			while (triggerGroups.Length <= triggerGroup)
			{
				AddTriggerGroup();
			}

			return triggerGroups[triggerGroup].AddTrigger(position, type, name, color);
		}

		public void RemoveTrigger(int triggerGroup, int triggerIndex)
		{
			if (triggerGroups.Length <= triggerGroup || triggerGroup < 0)
			{
				Debug.LogError("Cannot delete trigger - trigger group " + triggerIndex + " does not exist");
				return;
			}

			triggerGroups[triggerGroup].RemoveTrigger(triggerIndex);
		}

		public void CheckTriggers(double start, double end, SplineUser user = null)
		{
			for (var i = 0; i < triggerGroups.Length; i++)
			{
				triggerGroups[i].Check(start, end);
			}
		}

		public void CheckTriggers(int group, double start, double end)
		{
			if (group < 0 || group >= triggerGroups.Length)
			{
				Debug.LogError("Trigger group " + group + " does not exist");
				return;
			}

			triggerGroups[group].Check(start, end);
		}

		public void ResetTriggers()
		{
			for (var i = 0; i < triggerGroups.Length; i++)
			{
				triggerGroups[i].Reset();
			}
		}

		public void ResetTriggers(int group)
		{
			if (group < 0 || group >= triggerGroups.Length)
			{
				Debug.LogError("Trigger group " + group + " does not exist");
				return;
			}

			for (var i = 0; i < triggerGroups[group].triggers.Length; i++)
			{
				triggerGroups[group].triggers[i].Reset();
			}
		}

		/// <summary>
		///     Get the available junctions for the given point
		/// </summary>
		/// <param name="pointIndex"></param>
		/// <returns></returns>
		public List<Node.Connection> GetJunctions(int pointIndex)
		{
			for (var i = 0; i < m_nodes.Length; i++)
			{
				if (m_nodes[i].pointIndex == pointIndex)
				{
					return m_nodes[i].GetConnections(this);
				}
			}

			return new List<Node.Connection>();
		}

		/// <summary>
		///     Get all junctions for all points in the given interval
		/// </summary>
		/// <param name="start"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public Dictionary<int, List<Node.Connection>> GetJunctions(double start = 0.0, double end = 1.0)
		{
			int index;
			double lerp;
			UpdateSampleCollection();
			m_sampleCollection.GetSamplingValues(start, out index, out lerp);
			var junctions = new Dictionary<int, List<Node.Connection>>();
			float startValue = (m_spline.points.Length - 1) * (float)start;
			float endValue = (m_spline.points.Length - 1) * (float)end;
			for (var i = 0; i < m_nodes.Length; i++)
			{
				var add = false;
				if (end > start && m_nodes[i].pointIndex > startValue && m_nodes[i].pointIndex < endValue)
				{
					add = true;
				}
				else if (m_nodes[i].pointIndex < startValue && m_nodes[i].pointIndex > endValue)
				{
					add = true;
				}

				if (!add && Mathf.Abs(startValue - m_nodes[i].pointIndex) <= 0.0001f)
				{
					add = true;
				}

				if (!add && Mathf.Abs(endValue - m_nodes[i].pointIndex) <= 0.0001f)
				{
					add = true;
				}

				if (add)
				{
					junctions.Add(m_nodes[i].pointIndex, m_nodes[i].GetConnections(this));
				}
			}

			return junctions;
		}

		/// <summary>
		///     Call this to connect a node to a spline's point
		/// </summary>
		/// <param name="node"></param>
		/// <param name="pointIndex"></param>
		public void ConnectNode(Node node, int pointIndex)
		{
			if (node == null)
			{
				Debug.LogError("Missing Node");
				return;
			}

			if (pointIndex < 0 || pointIndex >= m_spline.points.Length)
			{
				Debug.Log("Invalid point index " + pointIndex);
				return;
			}

			for (var i = 0; i < m_nodes.Length; i++)
			{
				if (m_nodes[i].node == null)
				{
					continue;
				}

				if (m_nodes[i].pointIndex == pointIndex || m_nodes[i].node == node)
				{
					Node.Connection[] connections = m_nodes[i].node.GetConnections();
					for (var j = 0; j < connections.Length; j++)
					{
						if (connections[j].spline == this)
						{
							Debug.LogError("Node " + node.name + " is already connected to spline " + name +
							               " at point " + m_nodes[i].pointIndex);
							return;
						}
					}

					AddNodeLink(node, pointIndex);
					Debug.Log("Node link already exists");
					return;
				}
			}

			node.AddConnection(this, pointIndex);
			AddNodeLink(node, pointIndex);
		}

		public void DisconnectNode(int pointIndex)
		{
			for (var i = 0; i < m_nodes.Length; i++)
			{
				if (m_nodes[i].pointIndex == pointIndex)
				{
					m_nodes[i].node.RemoveConnection(this, pointIndex);
					ArrayUtility.RemoveAt(ref m_nodes, i);
					return;
				}
			}
		}

		private void AddNodeLink(Node node, int pointIndex)
		{
			var newLink = new NodeLink();
			newLink.node = node;
			newLink.pointIndex = pointIndex;
			ArrayUtility.Add(ref m_nodes, newLink);
			UpdateConnectedNodes();
		}

		public Dictionary<int, Node> GetNodes(double start = 0.0, double end = 1.0)
		{
			int index;
			double lerp;
			UpdateSampleCollection();
			m_sampleCollection.GetSamplingValues(start, out index, out lerp);
			var nodeList = new Dictionary<int, Node>();
			float startValue = (m_spline.points.Length - 1) * (float)start;
			float endValue = (m_spline.points.Length - 1) * (float)end;
			for (var i = 0; i < m_nodes.Length; i++)
			{
				var add = false;
				if (end > start && m_nodes[i].pointIndex > startValue && m_nodes[i].pointIndex < endValue)
				{
					add = true;
				}
				else if (m_nodes[i].pointIndex < startValue && m_nodes[i].pointIndex > endValue)
				{
					add = true;
				}

				if (!add && Mathf.Abs(startValue - m_nodes[i].pointIndex) <= 0.0001f)
				{
					add = true;
				}

				if (!add && Mathf.Abs(endValue - m_nodes[i].pointIndex) <= 0.0001f)
				{
					add = true;
				}

				if (add)
				{
					nodeList.Add(m_nodes[i].pointIndex, m_nodes[i].node);
				}
			}

			return nodeList;
		}

		public Node GetNode(int pointIndex)
		{
			if (pointIndex < 0 || pointIndex >= m_spline.points.Length)
			{
				return null;
			}

			for (var i = 0; i < m_nodes.Length; i++)
			{
				if (m_nodes[i].pointIndex == pointIndex)
				{
					return m_nodes[i].node;
				}
			}

			return null;
		}

		public void TransferNode(int pointIndex, int newPointIndex)
		{
			if (newPointIndex < 0 || newPointIndex >= m_spline.points.Length)
			{
				Debug.LogError("Invalid new point index " + newPointIndex);
				return;
			}

			if (GetNode(newPointIndex) != null)
			{
				Debug.LogError("Cannot move node to point " + newPointIndex + ". Point already connected to a node");
				return;
			}

			Node node = GetNode(pointIndex);
			if (node == null)
			{
				Debug.LogError("No node connected to point " + pointIndex);
				return;
			}

			DisconnectNode(pointIndex);
			SplineSample sample = Evaluate(newPointIndex);
			node.transform.position = sample.position;
			node.transform.rotation = sample.rotation;
			ConnectNode(node, newPointIndex);
		}

		public void ShiftNodes(int startIndex, int endIndex, int shift)
		{
			int from = endIndex;
			int to = startIndex;
			if (startIndex > endIndex)
			{
				from = startIndex;
				to = endIndex;
			}

			for (int i = from; i >= to; i--)
			{
				Node node = GetNode(i);
				if (node != null)
				{
					TransferNode(i, i + shift);
				}
			}
		}

		/// <summary>
		///     Gets all connected computers along with the connected indices and connection indices
		/// </summary>
		/// <param name="computers">A list of the connected computers</param>
		/// <param name="connectionIndices">The point indices of this computer where the other computers are connected</param>
		/// <param name="connectedIndices">The point indices of the other computers where they are connected</param>
		/// <param name="percent"></param>
		/// <param name="direction"></param>
		/// <param name="includeEqual">Should point indices that are placed exactly at the percent be included?</param>
		public void GetConnectedComputers(
			List<SplineComputer> computers,
			List<int> connectionIndices,
			List<int> connectedIndices,
			double percent,
			Spline.Direction direction,
			bool includeEqual)
		{
			if (computers == null)
			{
				computers = new List<SplineComputer>();
			}

			if (connectionIndices == null)
			{
				connectionIndices = new List<int>();
			}

			if (connectedIndices == null)
			{
				connectionIndices = new List<int>();
			}

			computers.Clear();
			connectionIndices.Clear();
			connectedIndices.Clear();
			int pointValue = Mathf.FloorToInt((m_spline.points.Length - 1) * (float)percent);
			for (var i = 0; i < m_nodes.Length; i++)
			{
				var condition = false;
				if (includeEqual)
				{
					if (direction == Spline.Direction.Forward)
					{
						condition = m_nodes[i].pointIndex >= pointValue;
					}
					else
					{
						condition = m_nodes[i].pointIndex <= pointValue;
					}
				}

				if (condition)
				{
					Node.Connection[] connections = m_nodes[i].node.GetConnections();
					for (var j = 0; j < connections.Length; j++)
					{
						if (connections[j].spline != this)
						{
							computers.Add(connections[j].spline);
							connectionIndices.Add(m_nodes[i].pointIndex);
							connectedIndices.Add(connections[j].pointIndex);
						}
					}
				}
			}
		}

		/// <summary>
		///     Returns a list of all connected computers. This includes the base computer too.
		/// </summary>
		/// <returns></returns>
		public List<SplineComputer> GetConnectedComputers()
		{
			var computers = new List<SplineComputer>();
			computers.Add(this);
			if (m_nodes.Length == 0)
			{
				return computers;
			}

			GetConnectedComputers(ref computers);
			return computers;
		}

		public void GetSamplingValues(double percent, out int index, out double lerp)
		{
			UpdateSampleCollection();
			m_sampleCollection.GetSamplingValues(percent, out index, out lerp);
		}

		private void GetConnectedComputers(ref List<SplineComputer> computers)
		{
			SplineComputer comp = computers[computers.Count - 1];
			if (comp == null)
			{
				return;
			}

			for (var i = 0; i < comp.m_nodes.Length; i++)
			{
				if (comp.m_nodes[i].node == null)
				{
					continue;
				}

				Node.Connection[] connections = comp.m_nodes[i].node.GetConnections();
				for (var n = 0; n < connections.Length; n++)
				{
					var found = false;
					if (connections[n].spline == this)
					{
						continue;
					}

					for (var x = 0; x < computers.Count; x++)
					{
						if (computers[x] == connections[n].spline)
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						computers.Add(connections[n].spline);
						GetConnectedComputers(ref computers);
					}
				}
			}
		}

		private void RemoveNodeLinkAt(int index)
		{
			//Then remove the node link
			var newLinks = new NodeLink[m_nodes.Length - 1];
			for (var i = 0; i < m_nodes.Length; i++)
			{
				if (i == index)
				{
					continue;
				}

				if (i < index)
				{
					newLinks[i] = m_nodes[i];
				}
				else
				{
					newLinks[i - 1] = m_nodes[i];
				}
			}

			m_nodes = newLinks;
		}

		//This "magically" updates the Node's position and all other points, connected to it when a point, linked to a Node is changed.
		private void SetNodeForPoint(int index, SplinePoint worldPoint)
		{
			for (var i = 0; i < m_nodes.Length; i++)
			{
				if (m_nodes[i].pointIndex == index)
				{
					m_nodes[i].node.UpdatePoint(this, m_nodes[i].pointIndex, worldPoint);
					break;
				}
			}
		}

		private void UpdateConnectedNodes(SplinePoint[] worldPoints)
		{
			for (var i = 0; i < m_nodes.Length; i++)
			{
				if (m_nodes[i].node == null)
				{
					RemoveNodeLinkAt(i);
					i--;
					Rebuild();
					continue;
				}

				var found = false;
				foreach (Node.Connection connection in m_nodes[i].node.GetConnections())
				{
					if (connection.spline == this)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					RemoveNodeLinkAt(i);
					i--;
					Rebuild();
					continue;
				}

				m_nodes[i].node.UpdatePoint(this, m_nodes[i].pointIndex, worldPoints[m_nodes[i].pointIndex]);
				m_nodes[i].node.UpdateConnectedComputers(this);
			}
		}

		private void UpdateConnectedNodes()
		{
			for (var i = 0; i < m_nodes.Length; i++)
			{
				if (m_nodes[i] == null || m_nodes[i].node == null)
				{
					RemoveNodeLinkAt(i);
					Rebuild();
					i--;
					continue;
				}

				var found = false;
				Node.Connection[] connections = m_nodes[i].node.GetConnections();
				for (var j = 0; j < connections.Length; j++)
				{
					if (connections[j].spline == this && connections[j].pointIndex == m_nodes[i].pointIndex)
					{
						found = true;
						break;
					}
				}

				if (found)
				{
					m_nodes[i].node.UpdatePoint(this, m_nodes[i].pointIndex, GetPoint(m_nodes[i].pointIndex));
				}
				else
				{
					RemoveNodeLinkAt(i);
					Rebuild();
					i--;
				}
			}
		}

		public Vector3 TransformPoint(Vector3 point)
		{
#if UNITY_EDITOR
			if (!m_editorIsPlaying)
			{
				return transform.TransformPoint(point);
			}
#endif
			return m_localToWorldMatrix.MultiplyPoint3x4(point);
		}

		public Vector3 InverseTransformPoint(Vector3 point)
		{
#if UNITY_EDITOR
			if (!m_editorIsPlaying)
			{
				return transform.InverseTransformPoint(point);
			}
#endif
			return m_worldToLocalMatrix.MultiplyPoint3x4(point);
		}

		public Vector3 TransformDirection(Vector3 direction)
		{
#if UNITY_EDITOR
			if (!m_editorIsPlaying)
			{
				return transform.TransformDirection(direction);
			}
#endif
			return m_localToWorldMatrix.MultiplyVector(direction);
		}

		public Vector3 InverseTransformDirection(Vector3 direction)
		{
#if UNITY_EDITOR
			if (!m_editorIsPlaying)
			{
				return transform.InverseTransformDirection(direction);
			}
#endif
			return m_worldToLocalMatrix.MultiplyVector(direction);
		}

		[Serializable]
		internal class NodeLink
		{
			[SerializeField]
			internal Node node;

			[SerializeField]
			internal int pointIndex;

			internal List<Node.Connection> GetConnections(SplineComputer exclude)
			{
				Node.Connection[] connections = node.GetConnections();
				var connectionList = new List<Node.Connection>();
				for (var i = 0; i < connections.Length; i++)
				{
					if (connections[i].spline == exclude)
					{
						continue;
					}

					connectionList.Add(connections[i]);
				}

				return connectionList;
			}
		}
#if UNITY_EDITOR
		public enum EditorUpdateMode
		{
			Default,
			OnMouseUp
		}

		[HideInInspector]
		public bool editorDrawPivot = true;

		[HideInInspector]
		public Color editorPathColor = Color.white;

		[HideInInspector]
		public bool editorAlwaysDraw;

		[HideInInspector]
		public bool editorDrawThickness;

		[HideInInspector]
		public bool editorBillboardThickness = true;

		private bool m_editorIsPlaying;

		[HideInInspector]
		public bool isNewlyCreated = true;

		[HideInInspector]
		public EditorUpdateMode editorUpdateMode = EditorUpdateMode.Default;
#endif

#if UNITY_EDITOR
		/// <summary>
		///     Used by the editor - should not be called from the API
		/// </summary>
		public void EditorAwake()
		{
			UpdateConnectedNodes();
			RebuildImmediate(true, true);
		}

		/// <summary>
		///     Used by the editor - should not be called from the API
		/// </summary>
		public void EditorUpdateConnectedNodes()
		{
			UpdateConnectedNodes();
		}
#endif

#if UNITY_EDITOR
		public void EditorSetPointDirty(int index)
		{
			SetDirty(index);
		}

		public void EditorSetAllPointsDirty()
		{
			SetAllDirty();
		}

#endif
	}
}