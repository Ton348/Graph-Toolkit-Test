using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace Dreamteck.Splines.Editor
{
	public class ObjectSpawnTool : SplineTool
	{
		private readonly List<GameObject> m_objects = new();

		internal List<SpawnCollection> collections = new();
		private bool m_applyRotation = true;
		private bool m_applyScale;
		private double m_clipFrom, m_clipTo = 1.0;

		private Iteration m_iteration = Iteration.Ordered;
		private Vector3 m_maxRotationOffset = Vector3.zero;
		private Vector3 m_maxScaleMultiplier = Vector3.one;
		private Vector3 m_minRotationOffset = Vector3.zero;
		private Vector3 m_minScaleMultiplier = Vector3.one;
		private Vector2 m_offset = Vector2.zero;
		private int m_offsetSeed;

		private Random m_orderRandom, m_offsetRandom, m_rotationRandom, m_scaleRandom;
		private int m_orderSeed;
		private float m_positionOffset;
		private bool m_randomizeOffset;
		private Vector2 m_randomSize = Vector2.one;

		private SplineSample m_result;
		private int m_rotationSeed;
		private int m_scaleSeed;
		private bool m_shellOffset = true;
		private int m_spawnCount = 1;
		private bool m_uniform;
		private bool m_useRandomOffsetRotation;

		public override string GetName()
		{
			return "Spawn Objects";
		}

		protected override string GetPrefix()
		{
			return "ObjectSpawnTool";
		}

		public override void Close()
		{
			base.Close();
			for (var i = 0; i < m_splines.Count; i++)
			{
				m_splines[i].onRebuild -= Rebuild;
			}

			if (m_promptSave)
			{
				if (EditorUtility.DisplayDialog("Save changes?",
					    "You are about to close the Object Spawn Tool, do you want to save the generated objects?",
					    "Yes", "No"))
				{
					Save();
				}
				else
				{
					Cancel();
				}
			}
			else
			{
				Cancel();
			}

			m_promptSave = false;
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif
		}

		public override void Open(EditorWindow window)
		{
			base.Open(window);
			GetSplines();
			collections.Clear();
			for (var i = 0; i < m_splines.Count; i++)
			{
				collections.Add(new SpawnCollection(m_splines[i]));
				m_splines[i].onRebuild += Rebuild;
			}

			Rebuild();
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif
		}

		private void OnScene(SceneView current)
		{
			for (var i = 0; i < collections.Count; i++)
			{
				if (collections[i].spline != null)
				{
					DssplineDrawer.DrawSplineComputer(collections[i].spline);
				}
			}
		}

		protected override void OnSplineAdded(SplineComputer spline)
		{
			base.OnSplineAdded(spline);
			collections.Add(new SpawnCollection(spline));
			spline.onRebuild += Rebuild;
			Rebuild();
		}

		protected override void OnSplineRemoved(SplineComputer spline)
		{
			base.OnSplineRemoved(spline);
			for (var i = 0; i < collections.Count; i++)
			{
				if (collections[i].spline == spline)
				{
					collections[i].Clear();
					collections.RemoveAt(i);
					spline.onRebuild -= Rebuild;
					Rebuild();
					return;
				}
			}
		}

		public override void Draw(Rect windowRect)
		{
			base.Draw(windowRect);
			if (m_splines.Count == 0)
			{
				EditorGUILayout.HelpBox("No spline selected! Select an object with a SplineComputer component.",
					MessageType.Warning);
				return;
			}

			EditorGUI.BeginChangeCheck();
			ClipUI(ref m_clipFrom, ref m_clipTo);
			m_uniform = EditorGUILayout.Toggle("Uniform Samples", m_uniform);
			EditorGUILayout.Space();
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = EditorGUIUtility.fieldWidth;
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;

			EditorGUILayout.BeginVertical();
			for (var i = 0; i < m_objects.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				m_objects[i] = (GameObject)EditorGUILayout.ObjectField(m_objects[i], typeof(GameObject), true);
				if (GUILayout.Button("x", GUILayout.Width(20)))
				{
					m_objects.RemoveAt(i);
					i--;
					Rebuild();
					Repaint();
					continue;
				}

				if (i > 0)
				{
					if (GUILayout.Button("▲", GUILayout.Width(20)))
					{
						GameObject temp = m_objects[i - 1];
						m_objects[i - 1] = m_objects[i];
						m_objects[i] = temp;
						Rebuild();
					}
				}

				if (i < m_objects.Count - 1)
				{
					if (GUILayout.Button("▼", GUILayout.Width(20)))
					{
						GameObject temp = m_objects[i + 1];
						m_objects[i + 1] = m_objects[i];
						m_objects[i] = temp;
						Rebuild();
					}
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
			GameObject newObj = null;
			newObj = (GameObject)EditorGUILayout.ObjectField("Add Object", newObj, typeof(GameObject), true);
			if (newObj != null)
			{
				m_objects.Add(newObj);
				Rebuild();
			}

			EditorGUILayout.Space();

			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUIUtility.fieldWidth = fieldWidth;
			var hasObj = false;
			for (var i = 0; i < m_objects.Count; i++)
			{
				if (m_objects[i] != null)
				{
					hasObj = true;
					break;
				}
			}

			if (hasObj)
			{
				m_spawnCount = EditorGUILayout.IntField("Spawn count", m_spawnCount);
			}
			else
			{
				m_spawnCount = 0;
			}

			m_iteration = (Iteration)EditorGUILayout.EnumPopup("Iteration", m_iteration);
			if (m_iteration == Iteration.Random)
			{
				m_orderSeed = EditorGUILayout.IntField("Order Seed", m_orderSeed);
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);
			m_applyRotation = EditorGUILayout.Toggle("Apply Rotation", m_applyRotation);
			if (m_applyRotation)
			{
				EditorGUI.indentLevel++;
				m_minRotationOffset = EditorGUILayout.Vector3Field("Min. Rotation Offset", m_minRotationOffset);
				m_maxRotationOffset = EditorGUILayout.Vector3Field("Max. Rotation Offset", m_maxRotationOffset);
				m_rotationSeed = EditorGUILayout.IntField("Rotation Seed", m_rotationSeed);
				EditorGUI.indentLevel--;
			}

			m_applyScale = EditorGUILayout.Toggle("Apply Scale", m_applyScale);
			if (m_applyScale)
			{
				EditorGUI.indentLevel++;
				m_minScaleMultiplier = EditorGUILayout.Vector3Field("Min. Scale Multiplier", m_minScaleMultiplier);
				m_maxScaleMultiplier = EditorGUILayout.Vector3Field("Max. Scale Multiplier", m_maxScaleMultiplier);
				m_scaleSeed = EditorGUILayout.IntField("Scale Seed", m_scaleSeed);
				EditorGUI.indentLevel--;
			}

			m_positionOffset = EditorGUILayout.Slider("Evaluate Offset", m_positionOffset, -1f, 1f);

			m_offset = EditorGUILayout.Vector2Field("Offset", m_offset);
			m_randomizeOffset = EditorGUILayout.Toggle("Randomize Offset", m_randomizeOffset);
			if (m_randomizeOffset)
			{
				m_randomSize = EditorGUILayout.Vector2Field("Size", m_randomSize);
				m_offsetSeed = EditorGUILayout.IntField("Offset Seed", m_offsetSeed);
				m_shellOffset = EditorGUILayout.Toggle("Shell", m_shellOffset);
				m_useRandomOffsetRotation = EditorGUILayout.Toggle("Apply offset rotation", m_useRandomOffsetRotation);
			}

			if (EditorGUI.EndChangeCheck())
			{
				m_promptSave = true;
				Rebuild();
			}

			EditorGUILayout.BeginHorizontal();
			if (collections.Count > 0)
			{
				if (GUILayout.Button("Save"))
				{
					Save();
				}

				if (GUILayout.Button("Cancel"))
				{
					Cancel();
				}
			}
			else
			{
				if (GUILayout.Button("New"))
				{
					Open(m_windowInstance);
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		protected override void Save()
		{
			base.Save();
			//register created object undo for each object in collections
			collections.Clear();
			//Set scene dirty
		}

		protected override void Cancel()
		{
			base.Cancel();
			foreach (SpawnCollection collection in collections)
			{
				collection.Clear();
			}

			collections.Clear();
		}

		private void InitializeRandomization()
		{
			m_orderRandom = new Random(m_orderSeed);
			if (m_randomizeOffset)
			{
				m_offsetRandom = new Random(m_offsetSeed);
			}

			if (m_applyRotation)
			{
				m_rotationRandom = new Random(m_rotationSeed);
			}

			if (m_applyScale)
			{
				m_scaleRandom = new Random(m_scaleSeed);
			}
		}

		protected override void Rebuild()
		{
			base.Rebuild();
			if (m_objects.Count == 0)
			{
				return;
			}

			InitializeRandomization();
			foreach (SpawnCollection c in collections)
			{
				if (c == null)
				{
					continue;
				}

				if (c.spline == null || m_spawnCount <= 0)
				{
					c.Clear();
					continue;
				}

				HandleCollection(c);
			}
		}

		private void HandleCollection(SpawnCollection collection)
		{
			collection.Clear();
			if (collection.spline == null)
			{
				return;
			}

			while (collection.objects.Count > m_spawnCount && collection.objects.Count >= 0)
			{
				collection.Destroy(collection.objects.Count - 1);
			}

			var orderIndex = 0;
			while (collection.objects.Count < m_spawnCount)
			{
				switch (m_iteration)
				{
					case Iteration.Ordered:
						collection.Spawn(m_objects[orderIndex], Vector3.zero, Quaternion.identity);
						orderIndex++;
						if (orderIndex >= m_objects.Count)
						{
							orderIndex = 0;
						}

						break;
					case Iteration.Random:
						collection.Spawn(m_objects[m_orderRandom.Next(m_objects.Count)], Vector3.zero,
							Quaternion.identity);
						break;
				}
			}

			var splineLength = 0f;
			if (m_uniform)
			{
				splineLength = collection.spline.CalculateLength() * (float)(m_clipTo - m_clipFrom);
			}

			for (var i = 0; i < m_spawnCount; i++)
			{
				var percent = 0.0;
				if (m_spawnCount > 1)
				{
					percent = (double)i / (m_spawnCount - 1);
				}

				var evaluate = 0.0;
				if (m_uniform)
				{
					evaluate = collection.spline.Travel(m_clipFrom, splineLength * (float)percent);
				}
				else
				{
					evaluate = Dmath.Lerp(m_clipFrom, m_clipTo, percent);
				}

				//Handle uniform splines
				evaluate += m_positionOffset;
				if (evaluate > 1f)
				{
					evaluate -= 1f;
				}
				else if (evaluate < 0f)
				{
					evaluate += 1f;
				}

				collection.spline.Evaluate(evaluate, ref m_result);
				HandleObject(collection.objects[i]);
			}
		}

		private void HandleObject(SpawnCollection.SpawnObject obj)
		{
			Transform instanceTransform = obj.instance.transform;
			Transform sourceTransform = obj.source.transform;
			Vector3 right = m_result.right;
			instanceTransform.position = m_result.position;
			instanceTransform.position += -right * m_offset.x + m_result.up * m_offset.y;
			Quaternion offsetRot = Quaternion.Euler(m_minRotationOffset);

			if (m_applyRotation)
			{
				offsetRot = Quaternion.Euler(
					Mathf.Lerp(m_minRotationOffset.x, m_maxRotationOffset.x, (float)m_rotationRandom.NextDouble()),
					Mathf.Lerp(m_minRotationOffset.y, m_maxRotationOffset.y, (float)m_rotationRandom.NextDouble()),
					Mathf.Lerp(m_minRotationOffset.z, m_maxRotationOffset.z, (float)m_rotationRandom.NextDouble()));
				instanceTransform.rotation = m_result.rotation * offsetRot;
			}

			if (m_randomizeOffset)
			{
				var distance = (float)m_offsetRandom.NextDouble();
				float angleInRadians = (float)m_offsetRandom.NextDouble() * 360f * Mathf.Deg2Rad;
				var randomCircle = new Vector2(distance * Mathf.Cos(angleInRadians),
					distance * Mathf.Sin(angleInRadians));
				if (m_shellOffset)
				{
					randomCircle.Normalize();
				}
				else
				{
					randomCircle = Vector2.ClampMagnitude(randomCircle, 1f);
				}

				instanceTransform.position += randomCircle.x * right * m_randomSize.x * m_result.size * 0.5f +
				                              randomCircle.y * m_result.up * m_randomSize.y * m_result.size * 0.5f;
				if (m_useRandomOffsetRotation)
				{
					instanceTransform.rotation =
						Quaternion.LookRotation(m_result.forward, instanceTransform.position - m_result.position) *
						offsetRot;
				}
			}

			if (m_applyScale)
			{
				Vector3 scale = sourceTransform.localScale * m_result.size;
				scale.x *= Mathf.Lerp(m_minScaleMultiplier.x, m_maxScaleMultiplier.x,
					(float)m_scaleRandom.NextDouble());
				scale.y *= Mathf.Lerp(m_minScaleMultiplier.y, m_maxScaleMultiplier.y,
					(float)m_scaleRandom.NextDouble());
				scale.z *= Mathf.Lerp(m_minScaleMultiplier.z, m_maxScaleMultiplier.z,
					(float)m_scaleRandom.NextDouble());
				instanceTransform.localScale = scale;
			}
			else
			{
				instanceTransform.localScale = sourceTransform.localScale;
			}
		}

		internal class SpawnCollection
		{
			internal List<SpawnObject> objects = new();

			internal SplineComputer spline;

			internal SpawnCollection(SplineComputer spline)
			{
				this.spline = spline;
			}

			internal void Clear()
			{
				for (var i = 0; i < objects.Count; i++)
				{
					Object.DestroyImmediate(objects[i].instance);
				}

				objects.Clear();
			}

			internal void Spawn(GameObject obj, Vector3 position, Quaternion rotation)
			{
				GameObject go = null;
				bool isPrefab = PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab;

				if (isPrefab)
				{
					go = (GameObject)PrefabUtility.InstantiatePrefab(obj);
				}
				else
				{
					go = Object.Instantiate(obj, position, rotation);
				}

				go.transform.parent = spline.transform;
				objects.Add(new SpawnObject(go, obj));
			}

			internal void Destroy(int index)
			{
				Object.DestroyImmediate(objects[index].instance);
				objects.RemoveAt(index);
			}

			public class SpawnObject
			{
				public GameObject instance;
				public GameObject source;

				public SpawnObject(GameObject instance, GameObject source)
				{
					this.instance = instance;
					this.source = source;
				}
			}
		}

		private enum Iteration
		{
			Ordered,
			Random
		}
	}
}