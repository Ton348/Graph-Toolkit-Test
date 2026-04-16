using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Dreamteck.Splines
{
	public partial class SplineMesh : MeshGenerator
	{
		[Serializable]
		public class Channel
		{
			public delegate float FloatHandler(double percent);

			public delegate Quaternion QuaternionHandler(double percent);

			public delegate Vector2 Vector2Handler(double percent);

			public delegate Vector3 Vector3Handler(double percent);

			public enum Type
			{
				Extrude,
				Place
			}

			public enum Uvoverride
			{
				None,
				ClampU,
				ClampV,
				UniformU,
				UniformV
			}

			public string name = "Channel";

			[SerializeField]
			[HideInInspector]
			private int m_iterationSeed;

			[SerializeField]
			[HideInInspector]
			private int m_offsetSeed;

			[SerializeField]
			[HideInInspector]
			private int m_rotationSeed;

			[SerializeField]
			[HideInInspector]
			private int m_scaleSeed;

			[SerializeField]
			internal SplineMesh owner;

			[SerializeField]
			[HideInInspector]
			private List<MeshDefinition> m_meshes = new();


			[SerializeField]
			[HideInInspector]
			private double m_clipFrom;

			[SerializeField]
			[HideInInspector]
			private double m_clipTo = 1.0;

			[SerializeField]
			[HideInInspector]
			private bool m_randomOrder;

			[SerializeField]
			[HideInInspector]
			private Uvoverride m_overrideUvs = Uvoverride.None;

			[SerializeField]
			[HideInInspector]
			private Vector2 m_uvScale = Vector2.one;

			[SerializeField]
			[HideInInspector]
			private Vector2 m_uvOffset = Vector2.zero;

			[SerializeField]
			[HideInInspector]
			private bool m_overrideNormal;

			[SerializeField]
			[HideInInspector]
			private Vector3 m_customNormal = Vector3.up;

			[SerializeField]
			[HideInInspector]
			private Type m_type = Type.Extrude;

			[SerializeField]
			[HideInInspector]
			private int m_count = 1;

			[SerializeField]
			[HideInInspector]
			private bool m_autoCount;

			[SerializeField]
			[HideInInspector]
			private double m_spacing;

			[SerializeField]
			[HideInInspector]
			private bool m_randomRotation;

			[SerializeField]
			[HideInInspector]
			private Vector3 m_minRotation = Vector3.zero;

			[SerializeField]
			[HideInInspector]
			private Vector3 m_maxRotation = Vector3.zero;

			[SerializeField]
			[HideInInspector]
			private bool m_randomOffset;

			[SerializeField]
			[HideInInspector]
			private Vector2 m_minOffset = Vector2.one;

			[SerializeField]
			[HideInInspector]
			private Vector2 m_maxOffset = Vector2.one;

			[SerializeField]
			[HideInInspector]
			private bool m_randomScale;

			[SerializeField]
			[HideInInspector]
			private bool m_uniformRandomScale;

			[SerializeField]
			[HideInInspector]
			private Vector3 m_minScale = Vector3.one;

			[SerializeField]
			[HideInInspector]
			private Vector3 m_maxScale = Vector3.one;

			[SerializeField]
			[HideInInspector]
			private bool m_overrideMaterialId;

			[SerializeField]
			[HideInInspector]
			private int m_targetMaterialId;

			[SerializeField]
			[HideInInspector]
			protected MeshScaleModifier m_scaleModifier = new();

			private FloatHandler m_extrudeRotationHandler;

			private Random m_iterationRandom;
			private int m_iterator;
			private Vector2Handler m_offsetHandler;
			private Random m_offsetRandom;
			private QuaternionHandler m_placeRotationHandler;
			private Random m_rotationRandom;
			private Vector3Handler m_scaleHandler;
			private Random m_scaleRandom;

			public Channel(string n, SplineMesh parent)
			{
				name = n;
				owner = parent;
				Init();
			}

			public Channel(string n, Mesh inputMesh, SplineMesh parent)
			{
				name = n;
				owner = parent;
				m_meshes.Add(new MeshDefinition(inputMesh));
				Init();
				Rebuild();
			}

			public double clipFrom
			{
				get => m_clipFrom;
				set
				{
					if (value != m_clipFrom)
					{
						m_clipFrom = value;
						Rebuild();
					}
				}
			}

			public double clipTo
			{
				get => m_clipTo;
				set
				{
					if (value != m_clipTo)
					{
						m_clipTo = value;
						Rebuild();
					}
				}
			}

			public bool randomOffset
			{
				get => m_randomOffset;
				set
				{
					if (value != m_randomOffset)
					{
						m_randomOffset = value;
						Rebuild();
					}
				}
			}

			public Vector2Handler offsetHandler
			{
				get => m_offsetHandler;
				set
				{
					if (value != m_offsetHandler)
					{
						m_offsetHandler = value;
						Rebuild();
					}
				}
			}

			public bool overrideMaterialID
			{
				get => m_overrideMaterialId;
				set
				{
					if (value != m_overrideMaterialId)
					{
						m_overrideMaterialId = value;
						Rebuild();
					}
				}
			}

			public int targetMaterialID
			{
				get => m_targetMaterialId;
				set
				{
					if (value != m_targetMaterialId)
					{
						m_targetMaterialId = value;
						Rebuild();
					}
				}
			}

			public bool randomRotation
			{
				get => m_randomRotation;
				set
				{
					if (value != m_randomRotation)
					{
						m_randomRotation = value;
						Rebuild();
					}
				}
			}

			public QuaternionHandler placeRotationHandler
			{
				get => m_placeRotationHandler;
				set
				{
					if (value != m_placeRotationHandler)
					{
						m_placeRotationHandler = value;
						Rebuild();
					}
				}
			}

			public FloatHandler extrudeRotationHandler
			{
				get => m_extrudeRotationHandler;
				set
				{
					if (value != m_extrudeRotationHandler)
					{
						m_extrudeRotationHandler = value;
						Rebuild();
					}
				}
			}

			public bool randomScale
			{
				get => m_randomScale;
				set
				{
					if (value != m_randomScale)
					{
						m_randomScale = value;
						Rebuild();
					}
				}
			}

			public Vector3Handler scaleHandler
			{
				get => m_scaleHandler;
				set
				{
					if (value != m_scaleHandler)
					{
						m_scaleHandler = value;
						Rebuild();
					}
				}
			}

			public bool uniformRandomScale
			{
				get => m_uniformRandomScale;
				set
				{
					if (value != m_uniformRandomScale)
					{
						m_uniformRandomScale = value;
						Rebuild();
					}
				}
			}

			public int offsetSeed
			{
				get => m_offsetSeed;
				set
				{
					if (value != m_offsetSeed)
					{
						m_offsetSeed = value;
						Rebuild();
					}
				}
			}

			public int rotationSeed
			{
				get => m_rotationSeed;
				set
				{
					if (value != m_rotationSeed)
					{
						m_rotationSeed = value;
						Rebuild();
					}
				}
			}

			public int scaleSeed
			{
				get => m_scaleSeed;
				set
				{
					if (value != m_scaleSeed)
					{
						m_scaleSeed = value;
						Rebuild();
					}
				}
			}

			public double spacing
			{
				get => m_spacing;
				set
				{
					if (value != m_spacing)
					{
						m_spacing = value;
						Rebuild();
					}
				}
			}

			public Vector2 minOffset
			{
				get => m_minOffset;
				set
				{
					if (value != m_minOffset)
					{
						m_minOffset = value;
						Rebuild();
					}
				}
			}

			public Vector2 maxOffset
			{
				get => m_maxOffset;
				set
				{
					if (value != m_maxOffset)
					{
						m_maxOffset = value;
						Rebuild();
					}
				}
			}

			public Vector3 minRotation
			{
				get => m_minRotation;
				set
				{
					if (value != m_minRotation)
					{
						m_minRotation = value;
						Rebuild();
					}
				}
			}

			public Vector3 maxRotation
			{
				get => m_maxRotation;
				set
				{
					if (value != m_maxRotation)
					{
						m_maxRotation = value;
						Rebuild();
					}
				}
			}

			public Vector3 minScale
			{
				get => m_minScale;
				set
				{
					if (value != m_minScale)
					{
						m_minScale = value;
						Rebuild();
					}
				}
			}

			public Vector3 maxScale
			{
				get => m_maxScale;
				set
				{
					if (value != m_maxScale)
					{
						m_maxScale = value;
						Rebuild();
					}
				}
			}

			public Type type
			{
				get => m_type;
				set
				{
					if (value != m_type)
					{
						m_type = value;
						Rebuild();
					}
				}
			}

			public bool randomOrder
			{
				get => m_randomOrder;
				set
				{
					if (value != m_randomOrder)
					{
						m_randomOrder = value;
						Rebuild();
					}
				}
			}

			public int randomSeed
			{
				get => m_iterationSeed;
				set
				{
					if (value != m_iterationSeed)
					{
						m_iterationSeed = value;
						if (m_randomOrder)
						{
							Rebuild();
						}
					}
				}
			}

			public int count
			{
				get => m_count;
				set
				{
					if (value != m_count)
					{
						m_count = value;
						if (m_count < 1)
						{
							m_count = 1;
						}

						Rebuild();
					}
				}
			}

			public bool autoCount
			{
				get => m_autoCount;
				set
				{
					if (value != m_autoCount)
					{
						m_autoCount = value;
						Rebuild();
					}
				}
			}

			public Uvoverride overrideUVs
			{
				get => m_overrideUvs;
				set
				{
					if (value != m_overrideUvs)
					{
						m_overrideUvs = value;
						Rebuild();
					}
				}
			}

			public Vector2 uvOffset
			{
				get => m_uvOffset;
				set
				{
					if (value != m_uvOffset)
					{
						m_uvOffset = value;
						Rebuild();
					}
				}
			}

			public Vector2 uvScale
			{
				get => m_uvScale;
				set
				{
					if (value != m_uvScale)
					{
						m_uvScale = value;
						Rebuild();
					}
				}
			}

			public bool overrideNormal
			{
				get => m_overrideNormal;
				set
				{
					if (value != m_overrideNormal)
					{
						m_overrideNormal = value;
						Rebuild();
					}
				}
			}

			public Vector3 customNormal
			{
				get => m_customNormal;
				set
				{
					if (value != m_customNormal)
					{
						m_customNormal = value;
						Rebuild();
					}
				}
			}

			public MeshScaleModifier scaleModifier => m_scaleModifier;

			private void Init()
			{
				m_minScale = m_maxScale = Vector3.one;
				m_minOffset = m_maxOffset = Vector3.zero;
				m_minRotation = m_maxRotation = Vector3.zero;
			}

			public void CopyTo(Channel target)
			{
				target.m_meshes.Clear();
				for (var i = 0; i < m_meshes.Count; i++)
				{
					target.m_meshes.Add(m_meshes[i].Copy());
				}

				target.m_clipFrom = m_clipFrom;
				target.m_clipTo = m_clipTo;
				target.m_customNormal = m_customNormal;
				target.m_iterationSeed = m_iterationSeed;
				target.m_minOffset = m_minOffset;
				target.m_minRotation = m_minRotation;
				target.m_minScale = m_minScale;
				target.m_maxOffset = m_maxOffset;
				target.m_maxRotation = m_maxRotation;
				target.m_maxScale = m_maxScale;
				target.m_randomOffset = m_randomOffset;
				target.m_randomRotation = m_randomRotation;
				target.m_randomScale = m_randomScale;
				target.m_offsetSeed = m_offsetSeed;
				target.m_offsetHandler = m_offsetHandler;
				target.m_rotationSeed = m_rotationSeed;
				target.m_placeRotationHandler = m_placeRotationHandler;
				target.m_extrudeRotationHandler = m_extrudeRotationHandler;
				target.m_scaleSeed = m_scaleSeed;
				target.m_scaleHandler = m_scaleHandler;
				target.m_iterationSeed = m_iterationSeed;
				target.m_count = m_count;
				target.m_spacing = m_spacing;
				target.m_overrideUvs = m_overrideUvs;
				target.m_type = m_type;
				target.m_overrideMaterialId = m_overrideMaterialId;
				target.m_targetMaterialId = m_targetMaterialId;
				target.m_overrideNormal = m_overrideNormal;
			}

			public int GetMeshCount()
			{
				return m_meshes.Count;
			}

			public void SwapMeshes(int a, int b)
			{
				if (a < 0 || a >= m_meshes.Count || b < 0 || b >= m_meshes.Count)
				{
					return;
				}

				MeshDefinition temp = m_meshes[b];
				m_meshes[b] = m_meshes[a];
				m_meshes[a] = temp;
				Rebuild();
			}

			public void DuplicateMesh(int index)
			{
				if (index < 0 || index >= m_meshes.Count)
				{
					return;
				}

				m_meshes.Add(m_meshes[index].Copy());
				Rebuild();
			}

			public MeshDefinition GetMesh(int index)
			{
				return m_meshes[index];
			}

			public void AddMesh(Mesh input)
			{
				m_meshes.Add(new MeshDefinition(input));
				Rebuild();
			}

			public void AddMesh(MeshDefinition meshDefinition)
			{
				if (!m_meshes.Contains(meshDefinition))
				{
					m_meshes.Add(meshDefinition);
					Rebuild();
				}
			}

			public void RemoveMesh(int index)
			{
				m_meshes.RemoveAt(index);
				Rebuild();
			}

			public void ResetIteration()
			{
				if (m_randomOrder)
				{
					m_iterationRandom = new Random(m_iterationSeed);
				}

				if (m_randomOffset)
				{
					m_offsetRandom = new Random(m_offsetSeed);
				}

				if (m_randomRotation)
				{
					m_rotationRandom = new Random(m_rotationSeed);
				}

				if (m_randomScale)
				{
					m_scaleRandom = new Random(m_scaleSeed);
				}

				m_iterator = 0;
			}

			public (Vector2, Quaternion, Vector3) GetCustomPlaceValues(double percent)
			{
				(Vector2, Quaternion, Vector3) values = (Vector2.zero, Quaternion.identity, Vector3.one);
				if (m_offsetHandler != null)
				{
					values.Item1 = m_offsetHandler(percent);
				}

				if (m_placeRotationHandler != null)
				{
					values.Item2 = m_placeRotationHandler(percent);
				}

				if (m_scaleHandler != null)
				{
					values.Item3 = m_scaleHandler(percent);
				}

				return values;
			}

			public (Vector2, float, Vector3) GetCustomExtrudeValues(double percent)
			{
				(Vector2, float, Vector3) values = (Vector2.zero, 0f, Vector3.one);
				if (m_offsetHandler != null)
				{
					values.Item1 = m_offsetHandler(percent);
				}

				if (m_extrudeRotationHandler != null)
				{
					values.Item2 = m_extrudeRotationHandler(percent);
				}

				if (m_scaleHandler != null)
				{
					values.Item3 = m_scaleHandler(percent);
				}

				return values;
			}

			public Vector2 NextRandomOffset()
			{
				if (m_randomOffset)
				{
					return new Vector2(Mathf.Lerp(m_minOffset.x, m_maxOffset.x, (float)m_offsetRandom.NextDouble()),
						Mathf.Lerp(m_minOffset.y, m_maxOffset.y, (float)m_offsetRandom.NextDouble()));
				}

				return m_minOffset;
			}

			public Quaternion NextRandomQuaternion()
			{
				if (m_randomRotation)
				{
					return Quaternion.Euler(new Vector3(
						Mathf.Lerp(m_minRotation.x, m_maxRotation.x, (float)m_rotationRandom.NextDouble()),
						Mathf.Lerp(m_minRotation.y, m_maxRotation.y, (float)m_rotationRandom.NextDouble()),
						Mathf.Lerp(m_minRotation.z, m_maxRotation.z, (float)m_rotationRandom.NextDouble())));
				}

				return Quaternion.Euler(m_minRotation);
			}

			public float NextRandomAngle()
			{
				if (m_randomRotation)
				{
					return Mathf.Lerp(m_minRotation.z, m_maxRotation.z, (float)m_rotationRandom.NextDouble());
				}

				return m_minRotation.z;
			}

			public Vector3 NextRandomScale()
			{
				if (m_randomScale)
				{
					if (m_uniformRandomScale)
					{
						return Vector3.Lerp(new Vector3(m_minScale.x, m_minScale.y, 1f),
							new Vector3(m_maxScale.x, m_maxScale.y, 1f), (float)m_scaleRandom.NextDouble());
					}

					return new Vector3(Mathf.Lerp(m_minScale.x, m_maxScale.x, (float)m_scaleRandom.NextDouble()),
						Mathf.Lerp(m_minScale.y, m_maxScale.y, (float)m_scaleRandom.NextDouble()), 1f);
				}

				return new Vector3(m_minScale.x, m_minScale.y, 1f);
			}

			public Vector3 NextPlaceScale()
			{
				if (m_randomScale)
				{
					if (m_uniformRandomScale)
					{
						return Vector3.Lerp(m_minScale, m_maxScale, (float)m_scaleRandom.NextDouble());
					}

					return new Vector3(Mathf.Lerp(m_minScale.x, m_maxScale.x, (float)m_scaleRandom.NextDouble()),
						Mathf.Lerp(m_minScale.y, m_maxScale.y, (float)m_scaleRandom.NextDouble()),
						Mathf.Lerp(m_minScale.z, m_maxScale.z, (float)m_scaleRandom.NextDouble()));
				}

				return m_minScale;
			}

			public MeshDefinition NextMesh()
			{
				if (m_randomOrder)
				{
					return m_meshes[m_iterationRandom.Next(m_meshes.Count)];
				}

				if (m_iterator >= m_meshes.Count)
				{
					m_iterator = 0;
				}

				return m_meshes[m_iterator++];
			}

			internal void Rebuild()
			{
				if (owner != null)
				{
					owner.Rebuild();
				}
			}

			private void Refresh()
			{
				for (var i = 0; i < m_meshes.Count; i++)
				{
					m_meshes[i].Refresh();
				}

				Rebuild();
			}

			[Serializable]
			public struct BoundsSpacing
			{
				public float front;
				public float back;
			}

			[Serializable]
			public class MeshDefinition
			{
				public enum MirrorMethod
				{
					None,
					X,
					Y,
					Z
				}

				[SerializeField]
				[HideInInspector]
				public Vector3[] vertices = new Vector3[0];

				[SerializeField]
				[HideInInspector]
				public Vector3[] normals = new Vector3[0];

				[SerializeField]
				[HideInInspector]
				public Vector4[] tangents = new Vector4[0];

				[SerializeField]
				[HideInInspector]
				public Color[] colors = new Color[0];

				[SerializeField]
				[HideInInspector]
				public Vector2[] uv = new Vector2[0];

				[SerializeField]
				[HideInInspector]
				public Vector2[] uv2 = new Vector2[0];

				[SerializeField]
				[HideInInspector]
				public Vector2[] uv3 = new Vector2[0];

				[SerializeField]
				[HideInInspector]
				public Vector2[] uv4 = new Vector2[0];

				[SerializeField]
				[HideInInspector]
				public int[] triangles = new int[0];

				[SerializeField]
				[HideInInspector]
				public List<Submesh> subMeshes = new();

				[SerializeField]
				[HideInInspector]
				public TsBounds bounds = new(Vector3.zero, Vector3.zero);

				[SerializeField]
				[HideInInspector]
				public List<VertexGroup> vertexGroups = new();

				[SerializeField]
				[HideInInspector]
				private Mesh m_mesh;

				[SerializeField]
				[HideInInspector]
				private Vector3 m_rotation = Vector3.zero;

				[SerializeField]
				[HideInInspector]
				private Vector3 m_offset = Vector3.zero;

				[SerializeField]
				[HideInInspector]
				private Vector3 m_scale = Vector3.one;

				[SerializeField]
				[HideInInspector]
				private Vector2 m_uvScale = Vector2.one;

				[SerializeField]
				[HideInInspector]
				private Vector2 m_uvOffset = Vector2.zero;

				[SerializeField]
				[HideInInspector]
				private float m_uvRotation;

				[SerializeField]
				[HideInInspector]
				private MirrorMethod m_mirror = MirrorMethod.None;

				[SerializeField]
				[HideInInspector]
				public BoundsSpacing _spacing;

				[SerializeField]
				[HideInInspector]
				private float m_vertexGroupingMargin;

				[SerializeField]
				[HideInInspector]
				private bool m_removeInnerFaces;

				[SerializeField]
				[HideInInspector]
				private bool m_flipFaces;

				[SerializeField]
				[HideInInspector]
				private bool m_doubleSided;

				public MeshDefinition(Mesh input)
				{
					m_mesh = input;
					Refresh();
				}

				public Mesh mesh
				{
					get => m_mesh;
					set
					{
						if (m_mesh != value)
						{
							m_mesh = value;
							Refresh();
						}
					}
				}

				public Vector3 rotation
				{
					get => m_rotation;
					set
					{
						if (rotation != value)
						{
							m_rotation = value;
							Refresh();
						}
					}
				}

				public Vector3 offset
				{
					get => m_offset;
					set
					{
						if (m_offset != value)
						{
							m_offset = value;
							Refresh();
						}
					}
				}

				public Vector3 scale
				{
					get => m_scale;
					set
					{
						if (m_scale != value)
						{
							m_scale = value;
							Refresh();
						}
					}
				}

				public BoundsSpacing spacing
				{
					get => _spacing;
					set
					{
						if (_spacing.back != value.back || _spacing.front != value.front)
						{
							_spacing = value;
							Refresh();
						}
					}
				}

				public Vector2 uvScale
				{
					get => m_uvScale;
					set
					{
						if (m_uvScale != value)
						{
							m_uvScale = value;
							Refresh();
						}
					}
				}

				public Vector2 uvOffset
				{
					get => m_uvOffset;
					set
					{
						if (m_uvOffset != value)
						{
							m_uvOffset = value;
							Refresh();
						}
					}
				}

				public float uvRotation
				{
					get => m_uvRotation;
					set
					{
						if (m_uvRotation != value)
						{
							m_uvRotation = value;
							Refresh();
						}
					}
				}

				public float vertexGroupingMargin
				{
					get => m_vertexGroupingMargin;
					set
					{
						if (m_vertexGroupingMargin != value)
						{
							m_vertexGroupingMargin = value;
							Refresh();
						}
					}
				}

				public MirrorMethod mirror
				{
					get => m_mirror;
					set
					{
						if (m_mirror != value)
						{
							m_mirror = value;
							Refresh();
						}
					}
				}

				public bool removeInnerFaces
				{
					get => m_removeInnerFaces;
					set
					{
						if (m_removeInnerFaces != value)
						{
							m_removeInnerFaces = value;
							Refresh();
						}
					}
				}

				public bool flipFaces
				{
					get => m_flipFaces;
					set
					{
						if (m_flipFaces != value)
						{
							m_flipFaces = value;
							Refresh();
						}
					}
				}

				public bool doubleSided
				{
					get => m_doubleSided;
					set
					{
						if (m_doubleSided != value)
						{
							m_doubleSided = value;
							Refresh();
						}
					}
				}

				internal MeshDefinition Copy()
				{
					var target = new MeshDefinition(m_mesh);
					target.vertices = new Vector3[vertices.Length];
					target.normals = new Vector3[normals.Length];
					target.colors = new Color[colors.Length];
					target.tangents = new Vector4[tangents.Length];
					target.uv = new Vector2[uv.Length];
					target.uv2 = new Vector2[uv2.Length];
					target.uv3 = new Vector2[uv3.Length];
					target.uv4 = new Vector2[uv4.Length];
					target.triangles = new int[triangles.Length];

					vertices.CopyTo(target.vertices, 0);
					normals.CopyTo(target.normals, 0);
					colors.CopyTo(target.colors, 0);
					tangents.CopyTo(target.tangents, 0);
					uv.CopyTo(target.uv, 0);
					uv2.CopyTo(target.uv2, 0);
					uv3.CopyTo(target.uv3, 0);
					uv4.CopyTo(target.uv4, 0);
					triangles.CopyTo(target.triangles, 0);

					target.bounds = new TsBounds(bounds.min, bounds.max);
					target.subMeshes = new List<Submesh>();
					for (var i = 0; i < subMeshes.Count; i++)
					{
						target.subMeshes.Add(new Submesh(new int[subMeshes[i].triangles.Length]));
						subMeshes[i].triangles.CopyTo(target.subMeshes[target.subMeshes.Count - 1].triangles, 0);
					}

					target.m_mirror = m_mirror;
					target.m_offset = m_offset;
					target.m_rotation = m_rotation;
					target.m_scale = m_scale;
					target.m_uvOffset = m_uvOffset;
					target.m_uvScale = m_uvScale;
					target.m_uvRotation = m_uvRotation;
					target.m_flipFaces = m_flipFaces;
					target.m_doubleSided = m_doubleSided;
					return target;
				}

				public void Refresh()
				{
					if (m_mesh == null)
					{
						vertices = new Vector3[0];
						normals = new Vector3[0];
						colors = new Color[0];
						uv = new Vector2[0];
						uv2 = new Vector2[0];
						uv3 = new Vector2[0];
						uv4 = new Vector2[0];
						tangents = new Vector4[0];
						triangles = new int[0];
						subMeshes = new List<Submesh>();
						vertexGroups = new List<VertexGroup>();
						return;
					}

					if (vertices.Length != m_mesh.vertexCount)
					{
						vertices = new Vector3[m_mesh.vertexCount];
					}

					if (normals.Length != m_mesh.normals.Length)
					{
						normals = new Vector3[m_mesh.normals.Length];
					}

					if (colors.Length != m_mesh.colors.Length)
					{
						colors = new Color[m_mesh.colors.Length];
					}

					if (uv.Length != m_mesh.uv.Length)
					{
						uv = new Vector2[m_mesh.uv.Length];
					}

					if (uv2.Length != m_mesh.uv2.Length)
					{
						uv2 = new Vector2[m_mesh.uv2.Length];
					}

					if (uv3.Length != m_mesh.uv3.Length)
					{
						uv3 = new Vector2[m_mesh.uv3.Length];
					}

					if (uv4.Length != m_mesh.uv4.Length)
					{
						uv4 = new Vector2[m_mesh.uv4.Length];
					}

					if (tangents.Length != m_mesh.tangents.Length)
					{
						tangents = new Vector4[m_mesh.tangents.Length];
					}

					if (triangles.Length != m_mesh.triangles.Length)
					{
						triangles = new int[m_mesh.triangles.Length];
					}

					vertices = m_mesh.vertices;
					normals = m_mesh.normals;
					colors = m_mesh.colors;
					uv = m_mesh.uv;
					uv2 = m_mesh.uv2;
					uv3 = m_mesh.uv3;
					uv4 = m_mesh.uv4;
					tangents = m_mesh.tangents;
					triangles = m_mesh.triangles;
					colors = m_mesh.colors;

					while (subMeshes.Count > m_mesh.subMeshCount)
					{
						subMeshes.RemoveAt(0);
					}

					while (subMeshes.Count < m_mesh.subMeshCount)
					{
						subMeshes.Add(new Submesh(new int[0]));
					}

					for (var i = 0; i < subMeshes.Count; i++)
					{
						subMeshes[i].triangles = m_mesh.GetTriangles(i);
					}


					if (colors.Length != vertices.Length)
					{
						colors = new Color[vertices.Length];
						for (var i = 0; i < colors.Length; i++)
						{
							colors[i] = Color.white;
						}
					}

					Mirror();
					if (m_doubleSided)
					{
						DoubleSided();
					}
					else if (m_flipFaces)
					{
						FlipFaces();
					}

					TransformVertices();
					CalculateBounds();
					if (m_removeInnerFaces)
					{
						RemoveInnerFaces();
					}

					GroupVertices();

					if (bounds.size.z < 0.002f || bounds.size.x < 0.002f || bounds.size.y < 0.002f)
					{
						Debug.LogWarning(
							$"The size of [{m_mesh.name}]'s bounds is too small! This could cause an issue if the [Auto Count] option is enabled!");
					}
				}

				private void RemoveInnerFaces()
				{
					float min = float.MaxValue, max = 0f;
					for (var i = 0; i < vertices.Length; i++)
					{
						if (vertices[i].z < min)
						{
							min = vertices[i].z;
						}

						if (vertices[i].z > max)
						{
							max = vertices[i].z;
						}
					}

					for (var i = 0; i < subMeshes.Count; i++)
					{
						var newTris = new List<int>();
						for (var j = 0; j < subMeshes[i].triangles.Length; j += 3)
						{
							bool innerMax = true, innerMin = true;
							for (int k = j; k < j + 3; k++)
							{
								int index = subMeshes[i].triangles[k];
								if (!Mathf.Approximately(vertices[index].z, max))
								{
									innerMax = false;
								}

								if (!Mathf.Approximately(vertices[index].z, min))
								{
									innerMin = false;
								}
							}

							if (!innerMax && !innerMin)
							{
								newTris.Add(subMeshes[i].triangles[j]);
								newTris.Add(subMeshes[i].triangles[j + 1]);
								newTris.Add(subMeshes[i].triangles[j + 2]);
							}
						}

						subMeshes[i].triangles = newTris.ToArray();
					}
				}

				private void FlipFaces()
				{
					var temp = new TsMesh();
					temp.normals = normals;
					temp.tangents = tangents;
					temp.triangles = triangles;
					for (var i = 0; i < subMeshes.Count; i++)
					{
						temp.subMeshes.Add(subMeshes[i].triangles);
					}

					MeshUtility.FlipFaces(temp);
				}

				private void DoubleSided()
				{
					var temp = new TsMesh();
					temp.vertices = vertices;
					temp.normals = normals;
					temp.tangents = tangents;
					temp.colors = colors;
					temp.uv = uv;
					temp.uv2 = uv2;
					temp.uv3 = uv3;
					temp.uv4 = uv4;
					temp.triangles = triangles;
					for (var i = 0; i < subMeshes.Count; i++)
					{
						temp.subMeshes.Add(subMeshes[i].triangles);
					}

					MeshUtility.MakeDoublesided(temp);
					vertices = temp.vertices;
					normals = temp.normals;
					tangents = temp.tangents;
					colors = temp.colors;
					uv = temp.uv;
					uv2 = temp.uv2;
					uv3 = temp.uv3;
					uv4 = temp.uv4;
					triangles = temp.triangles;
					for (var i = 0; i < subMeshes.Count; i++)
					{
						subMeshes[i].triangles = temp.subMeshes[i];
					}
				}

				public void Write(TsMesh target, int forceMaterialId = -1)
				{
					if (target.vertices.Length != vertices.Length)
					{
						target.vertices = new Vector3[vertices.Length];
					}

					if (target.normals.Length != normals.Length)
					{
						target.normals = new Vector3[normals.Length];
					}

					if (target.colors.Length != colors.Length)
					{
						target.colors = new Color[colors.Length];
					}

					if (target.uv.Length != uv.Length)
					{
						target.uv = new Vector2[uv.Length];
					}

					if (target.uv2.Length != uv2.Length)
					{
						target.uv2 = new Vector2[uv2.Length];
					}

					if (target.uv3.Length != uv3.Length)
					{
						target.uv3 = new Vector2[uv3.Length];
					}

					if (target.uv4.Length != uv4.Length)
					{
						target.uv4 = new Vector2[uv4.Length];
					}

					if (target.tangents.Length != tangents.Length)
					{
						target.tangents = new Vector4[tangents.Length];
					}

					if (target.triangles.Length != triangles.Length)
					{
						target.triangles = new int[triangles.Length];
					}

					vertices.CopyTo(target.vertices, 0);
					normals.CopyTo(target.normals, 0);
					colors.CopyTo(target.colors, 0);
					uv.CopyTo(target.uv, 0);
					uv2.CopyTo(target.uv2, 0);
					uv3.CopyTo(target.uv3, 0);
					uv4.CopyTo(target.uv4, 0);
					tangents.CopyTo(target.tangents, 0);
					triangles.CopyTo(target.triangles, 0);

					if (target.subMeshes == null)
					{
						target.subMeshes = new List<int[]>();
					}

					if (forceMaterialId >= 0)
					{
						while (target.subMeshes.Count > forceMaterialId + 1)
						{
							target.subMeshes.RemoveAt(0);
						}

						while (target.subMeshes.Count < forceMaterialId + 1)
						{
							target.subMeshes.Add(new int[0]);
						}

						for (var i = 0; i < target.subMeshes.Count; i++)
						{
							if (i != forceMaterialId)
							{
								if (target.subMeshes[i].Length > 0)
								{
									target.subMeshes[i] = new int[0];
								}
							}
							else
							{
								if (target.subMeshes[i].Length != triangles.Length)
								{
									target.subMeshes[i] = new int[triangles.Length];
								}

								triangles.CopyTo(target.subMeshes[i], 0);
							}
						}
					}
					else
					{
						while (target.subMeshes.Count > subMeshes.Count)
						{
							target.subMeshes.RemoveAt(0);
						}

						while (target.subMeshes.Count < subMeshes.Count)
						{
							target.subMeshes.Add(new int[0]);
						}

						for (var i = 0; i < subMeshes.Count; i++)
						{
							if (subMeshes[i].triangles.Length != target.subMeshes[i].Length)
							{
								target.subMeshes[i] = new int[subMeshes[i].triangles.Length];
							}

							subMeshes[i].triangles.CopyTo(target.subMeshes[i], 0);
						}
					}
				}

				private void CalculateBounds()
				{
					Vector3 min = Vector3.zero;
					Vector3 max = Vector3.zero;
					for (var i = 0; i < vertices.Length; i++)
					{
						if (vertices[i].x < min.x)
						{
							min.x = vertices[i].x;
						}
						else if (vertices[i].x > max.x)
						{
							max.x = vertices[i].x;
						}

						if (vertices[i].y < min.y)
						{
							min.y = vertices[i].y;
						}
						else if (vertices[i].y > max.y)
						{
							max.y = vertices[i].y;
						}

						if (vertices[i].z < min.z)
						{
							min.z = vertices[i].z;
						}
						else if (vertices[i].z > max.z)
						{
							max.z = vertices[i].z;
						}
					}

					min.z -= spacing.back;
					max.z += spacing.front;
					bounds.CreateFromMinMax(min, max);
				}

				private void Mirror()
				{
					if (m_mirror == MirrorMethod.None)
					{
						return;
					}

					switch (m_mirror)
					{
						case MirrorMethod.X:
							for (var i = 0; i < vertices.Length; i++)
							{
								vertices[i].x *= -1f;
								normals[i].x = -normals[i].x;
							}

							break;
						case MirrorMethod.Y:
							for (var i = 0; i < vertices.Length; i++)
							{
								vertices[i].y *= -1f;
								normals[i].y = -normals[i].y;
							}

							break;
						case MirrorMethod.Z:
							for (var i = 0; i < vertices.Length; i++)
							{
								vertices[i].z *= -1f;
								normals[i].z = -normals[i].z;
							}

							break;
					}

					for (var i = 0; i < triangles.Length; i += 3)
					{
						int temp = triangles[i];
						triangles[i] = triangles[i + 2];
						triangles[i + 2] = temp;
					}

					for (var i = 0; i < subMeshes.Count; i++)
					{
						for (var j = 0; j < subMeshes[i].triangles.Length; j += 3)
						{
							int temp = subMeshes[i].triangles[j];
							subMeshes[i].triangles[j] = subMeshes[i].triangles[j + 2];
							subMeshes[i].triangles[j + 2] = temp;
						}
					}

					CalculateTangents();
				}

				private void TransformVertices()
				{
					var vertexMatrix = new Matrix4x4();
					vertexMatrix.SetTRS(m_offset, Quaternion.Euler(m_rotation), m_scale);
					Matrix4x4 normalMatrix = vertexMatrix.inverse.transpose;
					for (var i = 0; i < vertices.Length; i++)
					{
						vertices[i] = vertexMatrix.MultiplyPoint3x4(vertices[i]);
						normals[i] = normalMatrix.MultiplyVector(normals[i]).normalized;
					}

					for (var i = 0; i < tangents.Length; i++)
					{
						tangents[i] = normalMatrix.MultiplyVector(tangents[i]);
					}

					for (var i = 0; i < uv.Length; i++)
					{
						uv[i].x *= m_uvScale.x;
						uv[i].y *= m_uvScale.y;
						uv[i] += m_uvOffset;
						uv[i] = Quaternion.AngleAxis(uvRotation, Vector3.forward) * uv[i];
					}
				}

				private void GroupVertices()
				{
					vertexGroups = new List<VertexGroup>();

					for (var i = 0; i < vertices.Length; i++)
					{
						float value = vertices[i].z;
						double percent = Dmath.Clamp01(Dmath.InverseLerp(bounds.min.z, bounds.max.z, value));
						int index = FindInsertIndex(vertices[i], value);
						if (index >= vertexGroups.Count)
						{
							vertexGroups.Add(new VertexGroup(value, percent, new[] { i }));
						}
						else
						{
							float valueDelta = Mathf.Abs(vertexGroups[index].value - value);
							if (valueDelta < vertexGroupingMargin ||
							    Mathf.Approximately(valueDelta, vertexGroupingMargin))
							{
								vertexGroups[index].AddId(i);
							}
							else if (vertexGroups[index].value < value)
							{
								vertexGroups.Insert(index, new VertexGroup(value, percent, new[] { i }));
							}
							else
							{
								if (index < vertexGroups.Count - 1)
								{
									vertexGroups.Insert(index + 1, new VertexGroup(value, percent, new[] { i }));
								}
								else
								{
									vertexGroups.Add(new VertexGroup(value, percent, new[] { i }));
								}
							}
						}
					}
				}

				private int FindInsertIndex(Vector3 pos, float value)
				{
					var lower = 0;
					int upper = vertexGroups.Count - 1;

					while (lower <= upper)
					{
						int middle = lower + (upper - lower) / 2;
						if (vertexGroups[middle].value == value)
						{
							return middle;
						}

						if (vertexGroups[middle].value < value)
						{
							upper = middle - 1;
						}
						else
						{
							lower = middle + 1;
						}
					}

					return lower;
				}

				private void CalculateTangents()
				{
					if (vertices.Length == 0)
					{
						tangents = new Vector4[0];
						return;
					}

					tangents = new Vector4[vertices.Length];
					var tan1 = new Vector3[vertices.Length];
					var tan2 = new Vector3[vertices.Length];
					for (var i = 0; i < subMeshes.Count; i++)
					{
						for (var j = 0; j < subMeshes[i].triangles.Length; j += 3)
						{
							int i1 = subMeshes[i].triangles[j];
							int i2 = subMeshes[i].triangles[j + 1];
							int i3 = subMeshes[i].triangles[j + 2];
							float x1 = vertices[i2].x - vertices[i1].x;
							float x2 = vertices[i3].x - vertices[i1].x;
							float y1 = vertices[i2].y - vertices[i1].y;
							float y2 = vertices[i3].y - vertices[i1].y;
							float z1 = vertices[i2].z - vertices[i1].z;
							float z2 = vertices[i3].z - vertices[i1].z;
							float s1 = uv[i2].x - uv[i1].x;
							float s2 = uv[i3].x - uv[i1].x;
							float t1 = uv[i2].y - uv[i1].y;
							float t2 = uv[i3].y - uv[i1].y;
							float div = s1 * t2 - s2 * t1;
							float r = div == 0f ? 0f : 1f / div;
							var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r,
								(t2 * z1 - t1 * z2) * r);
							var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r,
								(s1 * z2 - s2 * z1) * r);
							tan1[i1] += sdir;
							tan1[i2] += sdir;
							tan1[i3] += sdir;
							tan2[i1] += tdir;
							tan2[i2] += tdir;
							tan2[i3] += tdir;
						}
					}

					for (var i = 0; i < vertices.Length; i++)
					{
						Vector3 n = normals[i];
						Vector3 t = tan1[i];
						Vector3.OrthoNormalize(ref n, ref t);
						tangents[i].x = t.x;
						tangents[i].y = t.y;
						tangents[i].z = t.z;
						tangents[i].w = Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f ? -1.0f : 1.0f;
					}
				}

				[Serializable]
				public class Submesh
				{
					public int[] triangles = new int[0];

					public Submesh()
					{
					}

					public Submesh(int[] input)
					{
						triangles = new int[input.Length];
						input.CopyTo(triangles, 0);
					}
				}

				[Serializable]
				public class VertexGroup
				{
					public float value;
					public double percent;
					public int[] ids;

					public VertexGroup(float val, double perc, int[] vertIds)
					{
						percent = perc;
						value = val;
						ids = vertIds;
					}

					public void AddId(int id)
					{
						var newIds = new int[ids.Length + 1];
						ids.CopyTo(newIds, 0);
						newIds[newIds.Length - 1] = id;
						ids = newIds;
					}
				}
			}
		}
	}
}