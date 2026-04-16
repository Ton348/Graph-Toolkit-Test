using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dreamteck.Splines
{
	[AddComponentMenu("Dreamteck/Splines/Users/Object Bender")]
	public class ObjectBender : SplineUser
	{
		public enum Axis
		{
			X,
			Y,
			Z
		}

		public enum ForwardMode
		{
			Spline,
			Custom
		}

		public enum NormalMode
		{
			Spline,
			Auto,
			Custom
		}

		[SerializeField]
		[HideInInspector]
		private bool m_bend;

		[HideInInspector]
		public BendProperty[] bendProperties = new BendProperty[0];

		[SerializeField]
		[HideInInspector]
		private bool m_parentIsTheSpline;

		[SerializeField]
		[HideInInspector]
		private TsBounds m_bounds;

		[SerializeField]
		[HideInInspector]
		private Axis m_axis = Axis.Z;

		[SerializeField]
		[HideInInspector]
		private NormalMode m_normalMode = NormalMode.Auto;

		[SerializeField]
		[HideInInspector]
		private ForwardMode m_forwardMode = ForwardMode.Spline;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_upVector")]
		private Vector3 m_customNormal = Vector3.up;

		[SerializeField]
		[HideInInspector]
		private Vector3 m_customForward = Vector3.forward;

		private Quaternion m_bendRotation = Quaternion.identity;

		private Matrix4x4 m_normalMatrix;

		public bool bend
		{
			get => m_bend;
			set
			{
				if (m_bend != value)
				{
					m_bend = value;
					if (value)
					{
						UpdateReferences();
						Rebuild();
					}
					else
					{
						Revert();
					}
				}
			}
		}

		public Axis axis
		{
			get => m_axis;
			set
			{
				if (spline != null && value != m_axis)
				{
					m_axis = value;
					UpdateReferences();
					Rebuild();
				}
				else
				{
					m_axis = value;
				}
			}
		}

		public NormalMode upMode
		{
			get => m_normalMode;
			set
			{
				if (spline != null && value != m_normalMode)
				{
					m_normalMode = value;
					Rebuild();
				}
				else
				{
					m_normalMode = value;
				}
			}
		}

		public Vector3 customNormal
		{
			get => m_customNormal;
			set
			{
				if (spline != null && value != m_customNormal)
				{
					m_customNormal = value;
					Rebuild();
				}
				else
				{
					m_customNormal = value;
				}
			}
		}

		public ForwardMode forwardMode
		{
			get => m_forwardMode;
			set
			{
				if (spline != null && value != m_forwardMode)
				{
					m_forwardMode = value;
					Rebuild();
				}
				else
				{
					m_forwardMode = value;
				}
			}
		}

		public Vector3 customForward
		{
			get => m_customForward;
			set
			{
				if (spline != null && value != m_customForward)
				{
					m_customForward = value;
					Rebuild();
				}
				else
				{
					m_customForward = value;
				}
			}
		}

		private void GetTransformsRecursively(Transform current, ref List<Transform> transformList)
		{
			transformList.Add(current);
			foreach (Transform child in current)
			{
				GetTransformsRecursively(child, ref transformList);
			}
		}

		private void GetObjects()
		{
			var found = new List<Transform>();
			GetTransformsRecursively(transform, ref found);
			var newProperties = new BendProperty[found.Count];
			for (var i = 0; i < found.Count; i++)
			{
				CreateProperty(ref newProperties[i], found[i]);
			}

			bendProperties = newProperties;
			var splineComponent = GetComponent<SplineComputer>();
			m_parentIsTheSpline = splineComponent == spline;
		}

		public TsBounds GetBounds()
		{
			return new TsBounds(m_bounds.min, m_bounds.max, m_bounds.center);
		}

#if UNITY_EDITOR
		public void EditorGenerateLightmapUvs()
		{
			for (var i = 0; i < bendProperties.Length; i++)
			{
				if (bendProperties[i].bendMesh)
				{
					if (bendProperties[i].filter == null)
					{
						continue;
					}

					if (bendProperties[i].filter.sharedMesh == null)
					{
						continue;
					}

					EditorUtility.DisplayProgressBar("Generating Lightmap UVS",
						bendProperties[i].filter.sharedMesh.name, (float)i / (bendProperties.Length - 1));
					Unwrapping.GenerateSecondaryUVSet(bendProperties[i].filter.sharedMesh);
				}
			}

			EditorUtility.ClearProgressBar();
		}
#endif

		private void CreateProperty(ref BendProperty property, Transform t)
		{
			property = new BendProperty(t, t == transform); //Create a new bend property for each child
			for (var i = 0; i < bendProperties.Length; i++)
			{
				//Search for properties that have the same trasform and copy their settings
				if (bendProperties[i].transform.transform == t)
				{
					property.enabled = bendProperties[i].enabled;
					property.applyRotation = bendProperties[i].applyRotation;
					property.applyScale = bendProperties[i].applyScale;
					property.bendMesh = bendProperties[i].bendMesh;
					property.bendCollider = bendProperties[i].bendCollider;
					property.generateLightmapUvs = bendProperties[i].generateLightmapUvs;
					property.colliderUpdateRate = bendProperties[i].colliderUpdateRate;
					break;
				}
			}

			if (t.transform != trs)
			{
				property.originalPosition = trs.InverseTransformPoint(t.position);
				property.originalRotation = Quaternion.Inverse(trs.rotation) * t.rotation;
			}
		}

		private void CalculateBounds()
		{
			if (m_bounds == null)
			{
				m_bounds = new TsBounds(Vector3.zero, Vector3.zero);
			}

			m_bounds.min = m_bounds.max = Vector3.zero;
			for (var i = 0; i < bendProperties.Length; i++)
			{
				CalculatePropertyBounds(ref bendProperties[i]);
			}

			for (var i = 0; i < bendProperties.Length; i++)
			{
				CalculatePercents(bendProperties[i]);
			}
		}

		private void CalculatePropertyBounds(ref BendProperty property)
		{
			if (!property.enabled)
			{
				return;
			}

			if (property.isParent && m_parentIsTheSpline)
			{
				return;
			}

			if (property.transform.transform == trs)
			{
				if (0f < m_bounds.min.x)
				{
					m_bounds.min.x = 0f;
				}

				if (0f < m_bounds.min.y)
				{
					m_bounds.min.y = 0f;
				}

				if (0f < m_bounds.min.z)
				{
					m_bounds.min.z = 0f;
				}

				if (0f > m_bounds.max.x)
				{
					m_bounds.max.x = 0f;
				}

				if (0f > m_bounds.max.y)
				{
					m_bounds.max.y = 0f;
				}

				if (0f > m_bounds.max.z)
				{
					m_bounds.max.z = 0f;
				}
			}
			else
			{
				if (property.originalPosition.x < m_bounds.min.x)
				{
					m_bounds.min.x = property.originalPosition.x;
				}

				if (property.originalPosition.y < m_bounds.min.y)
				{
					m_bounds.min.y = property.originalPosition.y;
				}

				if (property.originalPosition.z < m_bounds.min.z)
				{
					m_bounds.min.z = property.originalPosition.z;
				}

				if (property.originalPosition.x > m_bounds.max.x)
				{
					m_bounds.max.x = property.originalPosition.x;
				}

				if (property.originalPosition.y > m_bounds.max.y)
				{
					m_bounds.max.y = property.originalPosition.y;
				}

				if (property.originalPosition.z > m_bounds.max.z)
				{
					m_bounds.max.z = property.originalPosition.z;
				}
			}

			if (property.editMesh != null)
			{
				for (var n = 0; n < property.editMesh.vertices.Length; n++)
				{
					Vector3 localPos = property.transform.TransformPoint(property.editMesh.vertices[n]);
					localPos = trs.InverseTransformPoint(localPos);
					if (localPos.x < m_bounds.min.x)
					{
						m_bounds.min.x = localPos.x;
					}

					if (localPos.y < m_bounds.min.y)
					{
						m_bounds.min.y = localPos.y;
					}

					if (localPos.z < m_bounds.min.z)
					{
						m_bounds.min.z = localPos.z;
					}

					if (localPos.x > m_bounds.max.x)
					{
						m_bounds.max.x = localPos.x;
					}

					if (localPos.y > m_bounds.max.y)
					{
						m_bounds.max.y = localPos.y;
					}

					if (localPos.z > m_bounds.max.z)
					{
						m_bounds.max.z = localPos.z;
					}
				}
			}

			if (property.editColliderMesh != null)
			{
				for (var n = 0; n < property.editColliderMesh.vertices.Length; n++)
				{
					Vector3 localPos = property.transform.TransformPoint(property.editColliderMesh.vertices[n]);
					localPos = trs.InverseTransformPoint(localPos);
					if (localPos.x < m_bounds.min.x)
					{
						m_bounds.min.x = localPos.x;
					}

					if (localPos.y < m_bounds.min.y)
					{
						m_bounds.min.y = localPos.y;
					}

					if (localPos.z < m_bounds.min.z)
					{
						m_bounds.min.z = localPos.z;
					}

					if (localPos.x > m_bounds.max.x)
					{
						m_bounds.max.x = localPos.x;
					}

					if (localPos.y > m_bounds.max.y)
					{
						m_bounds.max.y = localPos.y;
					}

					if (localPos.z > m_bounds.max.z)
					{
						m_bounds.max.z = localPos.z;
					}
				}
			}

			if (property.originalSpline != null)
			{
				for (var n = 0; n < property.originalSpline.points.Length; n++)
				{
					Vector3 localPos = trs.InverseTransformPoint(property.originalSpline.points[n].position);
					if (localPos.x < m_bounds.min.x)
					{
						m_bounds.min.x = localPos.x;
					}

					if (localPos.y < m_bounds.min.y)
					{
						m_bounds.min.y = localPos.y;
					}

					if (localPos.z < m_bounds.min.z)
					{
						m_bounds.min.z = localPos.z;
					}

					if (localPos.x > m_bounds.max.x)
					{
						m_bounds.max.x = localPos.x;
					}

					if (localPos.y > m_bounds.max.y)
					{
						m_bounds.max.y = localPos.y;
					}

					if (localPos.z > m_bounds.max.z)
					{
						m_bounds.max.z = localPos.z;
					}
				}
			}

			m_bounds.CreateFromMinMax(m_bounds.min, m_bounds.max);
		}

		public void CalculatePercents(BendProperty property)
		{
			if (property.transform.transform != trs)
			{
				property.positionPercent = GetPercentage(trs.InverseTransformPoint(property.transform.position));
			}
			else
			{
				property.positionPercent = GetPercentage(Vector3.zero);
			}

			if (property.editMesh != null)
			{
				if (property.vertexPercents.Length != property.editMesh.vertexCount)
				{
					property.vertexPercents = new Vector3[property.editMesh.vertexCount];
				}

				if (property.editColliderMesh != null)
				{
					if (property.colliderVertexPercents.Length != property.editMesh.vertexCount)
					{
						property.colliderVertexPercents = new Vector3[property.editColliderMesh.vertexCount];
					}
				}

				for (var i = 0; i < property.editMesh.vertexCount; i++)
				{
					Vector3 localVertex = property.transform.TransformPoint(property.editMesh.vertices[i]);
					localVertex = trs.InverseTransformPoint(localVertex);
					property.vertexPercents[i] = GetPercentage(localVertex);
				}

				if (property.editColliderMesh != null)
				{
					for (var i = 0; i < property.editColliderMesh.vertexCount; i++)
					{
						Vector3 localVertex = property.transform.TransformPoint(property.editColliderMesh.vertices[i]);
						localVertex = trs.InverseTransformPoint(localVertex);
						property.colliderVertexPercents[i] = GetPercentage(localVertex);
					}
				}
			}

			if (property.splineComputer != null)
			{
				SplinePoint[] points = property.splineComputer.GetPoints();
				property.splinePointPercents = new Vector3[points.Length];
				property.primaryTangentPercents = new Vector3[points.Length];
				property.secondaryTangentPercents = new Vector3[points.Length];
				for (var i = 0; i < points.Length; i++)
				{
					property.splinePointPercents[i] = GetPercentage(trs.InverseTransformPoint(points[i].position));
					property.primaryTangentPercents[i] = GetPercentage(trs.InverseTransformPoint(points[i].tangent));
					property.secondaryTangentPercents[i] = GetPercentage(trs.InverseTransformPoint(points[i].tangent2));
				}
			}
		}

		private void Revert()
		{
			for (var i = 0; i < bendProperties.Length; i++)
			{
				bendProperties[i].Revert();
			}
		}


		public void UpdateReferences()
		{
			if (!hasTransform)
			{
				CacheTransform();
			}

			if (m_bend)
			{
				for (var i = 0; i < bendProperties.Length; i++)
				{
					bendProperties[i].Revert();
				}
			}

			GetObjects();
			CalculateBounds();
			if (m_bend)
			{
				Bend();
				for (var i = 0; i < bendProperties.Length; i++)
				{
					bendProperties[i].Apply(i > 0 || trs != spline.transform);
					bendProperties[i].Update();
				}
			}
		}

		private void GetevalResult(Vector3 percentage)
		{
			switch (axis)
			{
				case Axis.X: Evaluate(percentage.x, ref m_evalResult); break;
				case Axis.Y: Evaluate(percentage.y, ref m_evalResult); break;
				case Axis.Z: Evaluate(percentage.z, ref m_evalResult); break;
			}

			switch (m_normalMode)
			{
				case NormalMode.Auto: m_evalResult.up = Vector3.Cross(m_evalResult.forward, m_evalResult.right); break;
				case NormalMode.Custom: m_evalResult.up = m_customNormal; break;
			}

			if (m_forwardMode == ForwardMode.Custom)
			{
				m_evalResult.forward = customForward;
			}

			Vector3 right = m_evalResult.right;

			Quaternion axisRotation = Quaternion.identity;

			switch (axis)
			{
				case Axis.Z:
					m_evalResult.position += right * Mathf.Lerp(m_bounds.min.x, m_bounds.max.x, percentage.x) *
					                         m_evalResult.size;
					m_evalResult.position += m_evalResult.up *
					                         Mathf.Lerp(m_bounds.min.y, m_bounds.max.y, percentage.y) *
					                         m_evalResult.size;
					break;
				case Axis.X:
					axisRotation = Quaternion.Euler(0f, -90f, 0f);
					m_evalResult.position += right * Mathf.Lerp(m_bounds.max.z, m_bounds.min.z, percentage.z) *
					                         m_evalResult.size;
					m_evalResult.position += m_evalResult.up *
					                         Mathf.Lerp(m_bounds.min.y, m_bounds.max.y, percentage.y) *
					                         m_evalResult.size;
					break;
				case Axis.Y:
					axisRotation = Quaternion.Euler(90f, 0f, 0f);
					m_evalResult.position += right * Mathf.Lerp(m_bounds.min.x, m_bounds.max.x, percentage.x) *
					                         m_evalResult.size;
					m_evalResult.position += m_evalResult.up *
					                         Mathf.Lerp(m_bounds.min.z, m_bounds.max.z, percentage.z) *
					                         m_evalResult.size;
					break;
			}

			m_bendRotation = m_evalResult.rotation * axisRotation;
			m_normalMatrix = Matrix4x4.TRS(m_evalResult.position, m_bendRotation, Vector3.one * m_evalResult.size)
				.inverse.transpose;
		}

		private Vector3 GetPercentage(Vector3 point)
		{
			point.x = Mathf.InverseLerp(m_bounds.min.x, m_bounds.max.x, point.x);
			point.y = Mathf.InverseLerp(m_bounds.min.y, m_bounds.max.y, point.y);
			point.z = Mathf.InverseLerp(m_bounds.min.z, m_bounds.max.z, point.z);
			return point;
		}

		protected override void Build()
		{
			base.Build();
			if (m_bend)
			{
				Bend();
			}
		}

		private void Bend()
		{
			if (sampleCount <= 1)
			{
				return;
			}

			if (bendProperties.Length == 0)
			{
				return;
			}

			for (var i = 0; i < bendProperties.Length; i++)
			{
				BendObject(bendProperties[i]);
			}
		}

		public void BendObject(BendProperty p)
		{
			if (!p.enabled)
			{
				return;
			}

			if (p.isParent && m_parentIsTheSpline)
			{
				return;
			}

			GetevalResult(p.positionPercent);
			p.transform.position = m_evalResult.position;
			if (p.applyRotation)
			{
				//p.transform.rotation = evalResult.rotation * axisRotation * p.originalRotation;
				p.transform.rotation = m_bendRotation * (Quaternion.Inverse(p.parentRotation) * p.originalRotation);
			}
			else
			{
				p.transform.rotation = p.originalRotation;
			}

			if (p.applyScale)
			{
				p.transform.scale = p.originalScale * m_evalResult.size;
			}

			Matrix4x4 toLocalMatrix =
				Matrix4x4.TRS(p.transform.position, p.transform.rotation, p.transform.scale).inverse;
			if (p.editMesh != null)
			{
				BendMesh(p.vertexPercents, p.normals, p.editMesh, toLocalMatrix);
				p.editMesh.hasUpdate = true;
			}

			if (p._editColliderMesh != null)
			{
				BendMesh(p.colliderVertexPercents, p.colliderNormals, p.editColliderMesh, toLocalMatrix);
				p.editColliderMesh.hasUpdate = true;
			}

			if (p.originalSpline != null && !p.isParent)
			{
				for (var n = 0; n < p.splinePointPercents.Length; n++)
				{
					SplinePoint point = p.originalSpline.points[n];
					GetevalResult(p.splinePointPercents[n]);
					point.position = m_evalResult.position;
					GetevalResult(p.primaryTangentPercents[n]);
					point.tangent = m_evalResult.position;
					GetevalResult(p.secondaryTangentPercents[n]);
					point.tangent2 = m_evalResult.position;
					switch (axis)
					{
						case Axis.X:
							point.normal = Quaternion.LookRotation(m_evalResult.forward, m_evalResult.up) *
							               Quaternion.FromToRotation(Vector3.up, m_evalResult.up) * point.normal; break;
						case Axis.Y:
							point.normal = Quaternion.LookRotation(m_evalResult.forward, m_evalResult.up) *
							               Quaternion.FromToRotation(Vector3.up, m_evalResult.up) * point.normal; break;
						case Axis.Z:
							point.normal = Quaternion.LookRotation(m_evalResult.forward, m_evalResult.up) *
							               point.normal; break;
					}

					p.destinationSpline.points[n] = point;
				}
			}
		}

		private void BendMesh(
			Vector3[] vertexPercents,
			Vector3[] originalNormals,
			TsMesh mesh,
			Matrix4x4 worldToLocalMatrix)
		{
			if (mesh.vertexCount != vertexPercents.Length)
			{
				Debug.LogError("Vertex count mismatch");
				return;
			}

			for (var i = 0; i < mesh.vertexCount; i++)
			{
				Vector3 percent = vertexPercents[i];
				if (axis == Axis.Y)
				{
					percent.z = 1f - percent.z;
				}

				GetevalResult(percent);
				mesh.vertices[i] = worldToLocalMatrix.MultiplyPoint3x4(m_evalResult.position);
				mesh.normals[i] = worldToLocalMatrix.MultiplyVector(m_normalMatrix.MultiplyVector(originalNormals[i]));
			}
		}

		protected override void PostBuild()
		{
			base.PostBuild();
			if (!m_bend)
			{
				return;
			}

			for (var i = 0; i < bendProperties.Length; i++)
			{
				bendProperties[i].Apply(i > 0 || trs != spline.transform);
				bendProperties[i].Update();
			}
		}

		protected override void LateRun()
		{
			base.LateRun();
			for (var i = 0; i < bendProperties.Length; i++)
			{
				bendProperties[i].Update();
			}
		}


		[Serializable]
		public class BendProperty
		{
			public bool enabled = true;
			public TsTransform transform;
			public bool applyRotation = true;
			public bool applyScale = true;
			public bool generateLightmapUvs;

			[SerializeField]
			[HideInInspector]
			private bool m_bendMesh = true;

			[SerializeField]
			[HideInInspector]
			private bool m_bendSpline = true;

			[SerializeField]
			[HideInInspector]
			private bool m_bendCollider = true;

			public float colliderUpdateRate = 0.2f;

			public Vector3 originalPosition = Vector3.zero;
			public Vector3 originalScale = Vector3.one;
			public Quaternion originalRotation = Quaternion.identity;
			public Quaternion parentRotation = Quaternion.identity;
			public Vector3 positionPercent;

			public Vector3[] vertexPercents = new Vector3[0];
			public Vector3[] normals = new Vector3[0];
			public Vector3[] colliderVertexPercents = new Vector3[0];
			public Vector3[] colliderNormals = new Vector3[0];

			[SerializeField]
			[HideInInspector]
			private Mesh m_originalMesh;

			[SerializeField]
			[HideInInspector]
			private Mesh m_originalColliderMesh;

			[SerializeField]
			[HideInInspector]
			private Mesh m_destinationMesh;

			[SerializeField]
			[HideInInspector]
			private Mesh m_destinationColliderMesh;

			public Spline destinationSpline;

			public MeshFilter filter;
			public MeshCollider collider;
			public SplineComputer splineComputer;

			public Vector3[] splinePointPercents = new Vector3[0];
			public Vector3[] primaryTangentPercents = new Vector3[0];
			public Vector3[] secondaryTangentPercents = new Vector3[0];

			[SerializeField]
			[HideInInspector]
			private bool m_parent;

			public TsMesh _editColliderMesh;

			public TsMesh _editMesh;

			private float m_colliderUpdateDue;
			private Spline m_originalSpline;
			private bool m_updateCollider;


			public BendProperty(Transform t, bool parent = false)
			{
				m_parent = parent;
				transform = new TsTransform(t);
				originalPosition = t.localPosition;
				originalScale = t.localScale;
				originalRotation = t.localRotation;
				parentRotation = t.transform.rotation;
				if (t.transform.parent != null)
				{
					parentRotation = t.transform.parent.rotation;
				}

				filter = t.GetComponent<MeshFilter>();
				collider = t.GetComponent<MeshCollider>();
				if (filter != null && filter.sharedMesh != null)
				{
					m_originalMesh = filter.sharedMesh;
					normals = m_originalMesh.normals;
					for (var i = 0; i < normals.Length; i++)
					{
						normals[i] = transform.transform.TransformDirection(normals[i]).normalized;
					}
				}

				if (collider != null && collider.sharedMesh != null)
				{
					m_originalColliderMesh = collider.sharedMesh;
					colliderNormals = m_originalColliderMesh.normals;
					for (var i = 0; i < colliderNormals.Length; i++)
					{
						colliderNormals[i] = transform.transform.TransformDirection(colliderNormals[i]);
					}
				}

				if (!parent)
				{
					splineComputer = t.GetComponent<SplineComputer>();
				}

				if (splineComputer != null)
				{
					if (splineComputer.isClosed)
					{
						originalSpline.Close();
					}

					destinationSpline = new Spline(originalSpline.type);
					destinationSpline.points = new SplinePoint[originalSpline.points.Length];
					destinationSpline.points = splineComputer.GetPoints();
					if (splineComputer.isClosed)
					{
						destinationSpline.Close();
					}
				}
			}

			public bool isValid => transform != null && transform.transform != null;

			public bool bendMesh
			{
				get => m_bendMesh;
				set
				{
					if (value != m_bendMesh)
					{
						m_bendMesh = value;
						if (value)
						{
							if (filter != null && filter.sharedMesh != null)
							{
								normals = m_originalMesh.normals;
								for (var i = 0; i < normals.Length; i++)
								{
									normals[i] = transform.transform.TransformDirection(normals[i]);
								}
							}
						}
						else
						{
							RevertMesh();
						}
					}
				}
			}

			public bool bendCollider
			{
				get => m_bendCollider;
				set
				{
					if (value != m_bendCollider)
					{
						m_bendCollider = value;
						if (value)
						{
							if (collider != null && collider.sharedMesh != null &&
							    collider.sharedMesh != m_originalMesh)
							{
								colliderNormals = m_originalColliderMesh.normals;
							}
						}
						else
						{
							RevertCollider();
						}
					}
				}
			}

			public bool bendSpline
			{
				get => m_bendSpline;
				set
				{
					m_bendSpline = value;
					if (value)
					{
					}
				}
			}

			public TsMesh editMesh
			{
				get
				{
					if (!bendMesh || m_originalMesh == null)
					{
						_editMesh = null;
					}
					else if (_editMesh == null && m_originalMesh != null)
					{
						_editMesh = new TsMesh(m_originalMesh);
					}

					return _editMesh;
				}
			}

			public TsMesh editColliderMesh
			{
				get
				{
					if (!bendCollider || m_originalColliderMesh == null)
					{
						_editColliderMesh = null;
					}
					else if (_editColliderMesh == null && m_originalColliderMesh != null &&
					         m_originalColliderMesh != m_originalMesh)
					{
						_editColliderMesh = new TsMesh(m_originalColliderMesh);
					}

					return _editColliderMesh;
				}
			}

			public Spline originalSpline
			{
				get
				{
					if (!bendSpline || splineComputer == null)
					{
						m_originalSpline = null;
					}
					else if (m_originalSpline == null && splineComputer != null)
					{
						m_originalSpline = new Spline(splineComputer.type);
						m_originalSpline.points = splineComputer.GetPoints();
					}

					return m_originalSpline;
				}
			}

			public bool isParent => m_parent;

			public void Revert()
			{
				if (!isValid)
				{
					return;
				}

				RevertTransform();
				RevertCollider();
				RevertMesh();
				if (splineComputer != null)
				{
					splineComputer.SetPoints(m_originalSpline.points);
				}
			}

			private void RevertMesh()
			{
				if (filter != null)
				{
					filter.sharedMesh = m_originalMesh;
				}

				m_destinationMesh = null;
			}

			private void RevertTransform()
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					transform.transform.localPosition = originalPosition;
					transform.transform.localRotation = originalRotation;
				}
				else
				{
					transform.localPosition = originalPosition;
					transform.localRotation = originalRotation;
					transform.Update();
				}
#else
                transform.localPosition = originalPosition;
                transform.localRotation = originalRotation;
                transform.Update();
#endif
				transform.scale = originalScale;
				transform.Update();
			}

			private void RevertCollider()
			{
				if (collider != null)
				{
					collider.sharedMesh = m_originalColliderMesh;
				}

				m_destinationColliderMesh = null;
			}

			public void Apply(bool applyTransform)
			{
				if (!enabled)
				{
					return;
				}

				if (!isValid)
				{
					return;
				}

				if (applyTransform)
				{
					transform.Update();
				}

				if (editMesh != null && editMesh.hasUpdate)
				{
					ApplyMesh();
				}

				if (bendCollider && collider != null)
				{
					if (!m_updateCollider)
					{
						if ((editColliderMesh == null && editMesh != null) || editColliderMesh != null)
						{
							m_updateCollider = true;
							if (Application.isPlaying)
							{
								m_colliderUpdateDue = Time.time + colliderUpdateRate;
							}
						}
					}
				}

				if (splineComputer != null)
				{
					ApplySpline();
				}
			}

			public void Update()
			{
				if (Time.time >= m_colliderUpdateDue && m_updateCollider)
				{
					m_updateCollider = false;
					ApplyCollider();
				}
			}

			private void ApplyMesh()
			{
				if (filter == null)
				{
					return;
				}

				MeshUtility.CalculateTangents(editMesh);
				if (m_destinationMesh == null)
				{
					m_destinationMesh = new Mesh();
					m_destinationMesh.name = m_originalMesh.name;
				}

				editMesh.WriteMesh(ref m_destinationMesh);
				m_destinationMesh.RecalculateBounds();
				filter.sharedMesh = m_destinationMesh;
			}

			private void ApplyCollider()
			{
				if (collider == null)
				{
					return;
				}

				if (m_originalColliderMesh == m_originalMesh)
				{
					collider.sharedMesh =
						filter.sharedMesh; //if the collider has the same mesh as the filter - just copy it
				}
				else
				{
					MeshUtility.CalculateTangents(editColliderMesh);
					if (m_destinationColliderMesh == null)
					{
						m_destinationColliderMesh = new Mesh();
						m_destinationColliderMesh.name = m_originalColliderMesh.name;
					}

					editColliderMesh.WriteMesh(ref m_destinationColliderMesh);
					m_destinationColliderMesh.RecalculateBounds();
					collider.sharedMesh = m_destinationColliderMesh;
				}
			}

			private void ApplySpline()
			{
				if (destinationSpline == null)
				{
					return;
				}

				splineComputer.SetPoints(destinationSpline.points);
			}
		}
	}
}