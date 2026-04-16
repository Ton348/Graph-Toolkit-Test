using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Users/Object Controller")]
    public class ObjectController : SplineUser
    {
        [System.Serializable]
        internal class ObjectControl
        {
            public bool isNull
            {
                get
                {
                    return gameObject == null;
                }
            }
            public Transform transform
            {
                get {
                    if (gameObject == null) return null;
                    return gameObject.transform;  
                }
            }
            public GameObject gameObject;
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;
            public bool active = true;

            public Vector3 baseScale = Vector3.one;

            public ObjectControl(GameObject input)
            {
                gameObject = input;
                baseScale = gameObject.transform.localScale;
            }

            public void Destroy()
            {
                if (gameObject == null) return;
                GameObject.Destroy(gameObject);
            }

            public void DestroyImmediate()
            {
                if (gameObject == null) return;
                GameObject.DestroyImmediate(gameObject);
            }

            public void Apply()
            {
                if (gameObject == null) return;
                transform.position = position;
                transform.rotation = rotation;
                transform.localScale = scale;
                gameObject.SetActive(active);
            }

        }

        public enum SpawnMethod { Count, Points }
        public enum ObjectMethod { Instantiate, GetChildren }
        public enum Positioning { Stretch, Clip }
        public enum Iteration { Ordered, Random }

        [SerializeField]
        [HideInInspector]
        public GameObject[] objects = new GameObject[0];

        public ObjectMethod objectMethod
        {
            get { return m_objectMethod; }
            set
            {
                if (value != m_objectMethod)
                {
                    if (value == ObjectMethod.GetChildren)
                    {
                        m_objectMethod = value;
                        Spawn();
                    }
                    else m_objectMethod = value;
                }
            }
        }

        public SpawnMethod spawnMethod
        {
            get { return m_spawnMethod; }
            set
            {
                if (value != m_spawnMethod)
                {
                    m_spawnMethod = value;
                    Rebuild();
                }
            }
        }

        public int spawnCount
        {
            get { return m_spawnCount; }
            set
            {
                if (value != m_spawnCount)
                {
                    if (value < 0) value = 0;
                    if (m_objectMethod == ObjectMethod.Instantiate)
                    {
                        if (value < m_spawnCount)
                        {
                            m_spawnCount = value;
                            Remove();
                        }
                        else
                        {
                            m_spawnCount = value;
                            Spawn();
                        }
                    }
                    else m_spawnCount = value;
                }
            }
        }

        public Positioning objectPositioning
        {
            get { return m_objectPositioning; }
            set
            {
                if (value != m_objectPositioning)
                {
                    m_objectPositioning = value;
                    Rebuild();
                }
            }
        }

        public Iteration iteration
        {
            get { return m_iteration; }
            set
            {
                if (value != m_iteration)
                {
                    m_iteration = value;
                    Rebuild();
                }
            }
        }

#if UNITY_EDITOR
        public bool retainPrefabInstancesInEditor
        {
            get { return m_retainPrefabInstancesInEditor; }
            set
            {
                if (value != m_retainPrefabInstancesInEditor)
                {
                    m_retainPrefabInstancesInEditor = value;
                    Clear();
                    Spawn();
                    Rebuild();
                }
            }
        }
#endif

        public int randomSeed
        {
            get { return m_randomSeed; }
            set
            {
                if (value != m_randomSeed)
                {
                    m_randomSeed = value;
                    Rebuild();
                }
            }
        }

        public Vector3 minOffset
        {
            get { return m_minOffset; }
            set
            {
                if (value != m_minOffset)
                {
                    m_minOffset = value;
                    Rebuild();
                }
            }
        }

        public Vector3 maxOffset
        {
            get { return m_maxOffset; }
            set
            {
                if (value != m_maxOffset)
                {
                    m_maxOffset = value;
                    Rebuild();
                }
            }
        }

        public bool offsetUseWorldCoords
        {
            get { return m_offsetUseWorldCoords; }
            set
            {
                if (value != m_offsetUseWorldCoords)
                {
                    m_offsetUseWorldCoords = value;
                    Rebuild();
                }
            }
        }

        public Vector3 minRotation
        {
            get { return m_minRotation; }
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
            get { return m_maxRotation; }
            set
            {
                if (value != m_maxRotation)
                {
                    m_maxRotation = value;
                    Rebuild();
                }
            }
        }

        public Vector3 rotationOffset
        {
            get { return (m_maxRotation+m_minRotation)/2f; }
            set
            {
                if (value != m_minRotation || value != m_maxRotation)
                {
                    m_minRotation = m_maxRotation = value;
                    Rebuild();
                }
            }
        }

        public Vector3 minScaleMultiplier
        {
            get { return m_minScaleMultiplier; }
            set
            {
                if (value != m_minScaleMultiplier)
                {
                    m_minScaleMultiplier = value;
                    Rebuild();
                }
            }
        }

        public Vector3 maxScaleMultiplier
        {
            get { return m_maxScaleMultiplier; }
            set
            {
                if (value != m_maxScaleMultiplier)
                {
                    m_maxScaleMultiplier = value;
                    Rebuild();
                }
            }
        }

        public bool uniformScaleLerp
        {
            get { return m_uniformScaleLerp; }
            set
            {
                if(value != m_uniformScaleLerp)
                {
                    m_uniformScaleLerp = value;
                    Rebuild();
                }
            }
        }

        public bool shellOffset
        {
            get { return m_shellOffset; }
            set
            {
                if (value != m_shellOffset)
                {
                    m_shellOffset = value;
                    Rebuild();
                }
            }
        }

        public bool applyRotation
        {
            get { return m_applyRotation; }
            set
            {
                if (value != m_applyRotation)
                {
                    m_applyRotation = value;
                    Rebuild();
                }
            }
        }

        public bool rotateByOffset
        {
            get { return m_rotateByOffset; }
            set
            {
                if (value != m_rotateByOffset)
                {
                    m_rotateByOffset = value;
                    Rebuild();
                }
            }
        }

        public bool applyScale
        {
            get { return m_applyScale; }
            set
            {
                if (value != m_applyScale)
                {
                    m_applyScale = value;
                    Rebuild();
                }
            }
        }

        public float evaluateOffset
        {
            get { return m_evaluateOffset; }
            set
            {
                if (value != m_evaluateOffset)
                {
                    m_evaluateOffset = value;
                    Rebuild();
                }
            }
        }

        public float minObjectDistance
        {
            get { return m_minObjectDistance; }
            set
            {
                if (value != m_minObjectDistance)
                {
                    m_minObjectDistance = value;
                    Rebuild();
                }
            }
        }

        public float maxObjectDistance
        {
            get { return m_maxObjectDistance; }
            set
            {
                if (value != m_maxObjectDistance)
                {
                    m_maxObjectDistance = value;
                    Rebuild();
                }
            }
        }

        public ObjectControllerCustomRuleBase customOffsetRule
        {
            get { return m_customOffsetRule; }
            set
            {
                if (value != m_customOffsetRule)
                {
                    m_customOffsetRule = value;
                    Rebuild();
                }
            }
        }

        public ObjectControllerCustomRuleBase customRotationRule
        {
            get { return m_customRotationRule; }
            set
            {
                if (value != m_customRotationRule)
                {
                    m_customRotationRule = value;
                    Rebuild();
                }
            }
        }

        public ObjectControllerCustomRuleBase customScaleRule
        {
            get { return m_customScaleRule; }
            set
            {
                if (value != m_customScaleRule)
                {
                    m_customScaleRule = value;
                    Rebuild();
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private float m_evaluateOffset = 0f;
        [SerializeField]
        [HideInInspector]
        private SpawnMethod m_spawnMethod = SpawnMethod.Count;
        [SerializeField]
        [HideInInspector]
        private int m_spawnCount = 0;
#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        private bool m_retainPrefabInstancesInEditor = true;
#endif
        [SerializeField]
        [HideInInspector]
        private Positioning m_objectPositioning = Positioning.Stretch;
        [SerializeField]
        [HideInInspector]
        private Iteration m_iteration = Iteration.Ordered;
        [SerializeField]
        [HideInInspector]
        private int m_randomSeed = 1;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_minOffset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_maxOffset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private bool m_offsetUseWorldCoords = false;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_minRotation = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_maxRotation = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private bool m_uniformScaleLerp = true;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_minScaleMultiplier = Vector3.one;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_maxScaleMultiplier = Vector3.one;
        [SerializeField]
        [HideInInspector]
        private bool m_shellOffset = false;
        [SerializeField]
        [HideInInspector]
        private bool m_applyRotation = true;
        [SerializeField]
        [HideInInspector]
        private bool m_rotateByOffset = false;
        [SerializeField]
        [HideInInspector]
        private bool m_applyScale = false;
        [SerializeField]
        [HideInInspector]
        private ObjectMethod m_objectMethod = ObjectMethod.Instantiate;
        [HideInInspector]
        public bool delayedSpawn = false;
        [HideInInspector]
        public float spawnDelay = 0.1f;
        [SerializeField]
        [HideInInspector]
        private int m_lastChildCount = 0;
        [SerializeField]
        [HideInInspector]
        private float m_lastPointCount = 0;
        [SerializeField]
        [HideInInspector]
        private ObjectControl[] m_spawned = new ObjectControl[0];
        [SerializeField]
        [HideInInspector]
        private bool m_useCustomObjectDistance = false;
        [SerializeField]
        [HideInInspector]
        private float m_minObjectDistance = 0f;
        [SerializeField]
        [HideInInspector]
        private float m_maxObjectDistance = 0f;
        
        [SerializeField]
        [HideInInspector]
        private ObjectControllerCustomRuleBase m_customOffsetRule;

        [SerializeField]
        [HideInInspector]
        private ObjectControllerCustomRuleBase m_customRotationRule;

        [SerializeField]
        [HideInInspector]
        private ObjectControllerCustomRuleBase m_customScaleRule;

        System.Random m_offsetRandomizer, m_shellRandomizer, m_rotationRandomizer, m_scaleRandomizer, m_distanceRandomizer;

        private int GetTargetCount()
        {
            switch (m_spawnMethod)
            {
                case SpawnMethod.Points:
                    return spline.pointCount;
                case SpawnMethod.Count:
                default:
                    return spawnCount;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < m_spawned.Length; i++)
            {
                if (m_spawned[i] == null || m_spawned[i].transform == null) continue;
                m_spawned[i].transform.localScale = m_spawned[i].baseScale;
                if (m_objectMethod == ObjectMethod.GetChildren) m_spawned[i].gameObject.SetActive(false);
                else
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying) m_spawned[i].DestroyImmediate();
                    else m_spawned[i].Destroy();
#else
                    spawned[i].Destroy();
#endif

                }
            }
            m_spawned = new ObjectControl[0];
        }

        private void OnValidate()
        {
            if (m_spawnCount < 0) m_spawnCount = 0;
        }

        private void Remove()
        {
            int targetCount = GetTargetCount();
            if (targetCount >= m_spawned.Length) return;
            for (int i = m_spawned.Length - 1; i >= targetCount; i--)
            {
                if (i >= m_spawned.Length) break;
                if (m_spawned[i] == null) continue;
                m_spawned[i].transform.localScale = m_spawned[i].baseScale;
                if (m_objectMethod == ObjectMethod.GetChildren) m_spawned[i].gameObject.SetActive(false);
                else
                {
                    if (Application.isEditor) m_spawned[i].DestroyImmediate();
                    else m_spawned[i].Destroy();

                }
            }
            ObjectControl[] newSpawned = new ObjectControl[targetCount];
            for (int i = 0; i < newSpawned.Length; i++)
            {
                newSpawned[i] = m_spawned[i];
            }
            m_spawned = newSpawned;
            // For consistency, I rebuild immediately here too. That way,
            // the ObjectController behaves without glitching in all cases.
            RebuildImmediate();
        }

        public void GetAll()
        {
            ObjectControl[] newSpawned = new ObjectControl[transform.childCount];
            int index = 0;
            foreach (Transform child in transform)
            {
                if (newSpawned[index] == null)
                {
                    newSpawned[index++] = new ObjectControl(child.gameObject);
                    continue;
                }
                bool found = false;
                for (int i = 0; i < m_spawned.Length; i++)
                {
                    if (m_spawned[i].gameObject == child.gameObject)
                    {
                        newSpawned[index++] = m_spawned[i];
                        found = true;
                        break;
                    }
                }
                if (!found) newSpawned[index++] = new ObjectControl(child.gameObject);
            }
            m_spawned = newSpawned;
        }

        public void Spawn()
        {
            if (m_objectMethod == ObjectMethod.Instantiate)
            {
                if (delayedSpawn && Application.isPlaying)
                {
                    StopCoroutine("InstantiateAllWithDelay");
                    StartCoroutine(InstantiateAllWithDelay());
                }
                else InstantiateAll();
            }
            else GetAll();
            Rebuild();
        }

        protected override void LateRun()
        {
            base.LateRun();
            if (m_spawnMethod == SpawnMethod.Points && spline && m_lastPointCount != spline.pointCount)
            {
                if (m_objectMethod != ObjectMethod.GetChildren) Remove();
                Spawn();
                m_lastPointCount = spline.pointCount;
            }
            if (m_objectMethod == ObjectMethod.GetChildren && m_lastChildCount != transform.childCount)
            {
                Spawn();
                m_lastChildCount = transform.childCount;
            }
        }


        IEnumerator InstantiateAllWithDelay()
        {
            if (spline == null) yield break;
            if (objects.Length == 0) yield break;

            int targetCount = GetTargetCount();
            for (int i = m_spawned.Length; i < targetCount; i++)
            {
                InstantiateSingle();
                // Visual artifacts occur if not rebuilding immediately. Normally this can be solved
                // by calling RebuildImmediate on the spline after modifying it,
                // however, with delay this becomes difficult to control.
                // The first object would position correctly, but the rest would
                // have the wrong position for one frame, and the user would have to jump through
                // some hoops to rebuild the user in sync with spawning.
                RebuildImmediate();
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private void InstantiateAll()
        {
            if (spline == null) return;
            if (objects.Length == 0) return;

            int targetCount = GetTargetCount();
            for (int i = m_spawned.Length; i < targetCount; i++) InstantiateSingle();
            // For consistency, I rebuild immediately here too. That way, there is no need for the user
            // to figure out if the ObjectController has delay or not and keeps the usage simple.
            RebuildImmediate();
        }

        private void InstantiateSingle()
        {
            if (objects.Length == 0) return;
            int index = 0;
            if (m_iteration == Iteration.Ordered)
            {
                index = m_spawned.Length - Mathf.FloorToInt(m_spawned.Length / objects.Length) * objects.Length;
            }
            else index = Random.Range(0, objects.Length);
            if (objects[index] == null) return;

            ObjectControl[] newSpawned = new ObjectControl[m_spawned.Length + 1];
            m_spawned.CopyTo(newSpawned, 0);
#if UNITY_EDITOR
            if (!Application.isPlaying && retainPrefabInstancesInEditor)
            {
                GameObject go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(objects[index]);
                go.transform.position = transform.position;
                go.transform.rotation = transform.rotation;
                newSpawned[newSpawned.Length - 1] = new ObjectControl(go);
            } else
            {
                newSpawned[newSpawned.Length - 1] = new ObjectControl((GameObject)Instantiate(objects[index], transform.position, transform.rotation));
            }
#else
            newSpawned[newSpawned.Length - 1] = new ObjectControl((GameObject)Instantiate(objects[index], transform.position, transform.rotation));
#endif
            newSpawned[newSpawned.Length - 1].transform.parent = transform;
            m_spawned = newSpawned;

#if UNITY_EDITOR
            // For prefabs, it is important that the spawned array gets marked as overridden.
            // Otherwise, the Object Controller will lose references to objects that were spawned
            // after prefab instantiation but before editor play/pause, causing it to leave behind
            // objects and instantiating extra ones.
            EditorUtility.SetDirty(this);
#endif
        }

        protected override void Build()
        {
            base.Build();
            m_offsetRandomizer = new System.Random(m_randomSeed);
            if(m_shellOffset) m_shellRandomizer = new System.Random(m_randomSeed + 1);
            m_rotationRandomizer = new System.Random(m_randomSeed + 2);
            m_scaleRandomizer = new System.Random(m_randomSeed + 3);
            m_distanceRandomizer = new System.Random(m_randomSeed + 4);

            bool hasCustomOffset = m_customOffsetRule != null;
            bool hasCustomRotation = m_customRotationRule != null;
            bool hasCustomScale = m_customScaleRule != null;

            bool randomScaleMultiplier = m_minScaleMultiplier != m_maxScaleMultiplier;
            double distancePercentAccum = 0.0;
            for (int i = 0; i < m_spawned.Length; i++)
            {
                if (m_spawned[i] == null)
                {
                    Clear();
                    Spawn();
                    break;
                }
                float percent = 0f;
                if (m_spawned.Length > 1)
                {
                    if(!m_useCustomObjectDistance)
                    {
                        if (spline.isClosed)
                        {
                            percent = (float)i / m_spawned.Length;
                        }
                        else
                        {
                            percent = (float)i / (m_spawned.Length - 1);
                        }
                    } else
                    {
                        percent = (float)distancePercentAccum;
                    }
                }

                percent += m_evaluateOffset;
                if (percent > 1f)
                {
                    percent -= 1f;
                }
                else if (percent < 0f)
                {
                    percent += 1f;
                }
                
                if (objectPositioning == Positioning.Clip)
                {
                    spline.Evaluate(percent, ref m_evalResult);
                }
                else
                {
                    Evaluate(percent, ref m_evalResult);
                }

                m_spawned[i].position = m_evalResult.position;

                if (m_applyScale)
                {
                    if (hasCustomScale)
                    {
                        m_customScaleRule.SetContext(this, m_evalResult, i, m_spawned.Length);
                        m_spawned[i].scale = m_customOffsetRule.GetScale();
                    } 
                    else
                    {
                        Vector3 scale = m_spawned[i].baseScale * m_evalResult.size;
                        Vector3 multiplier = m_minScaleMultiplier;

                        if (randomScaleMultiplier)
                        {

                            if (m_uniformScaleLerp)
                            {
                                multiplier = Vector3.Lerp(new Vector3(m_minScaleMultiplier.x, m_minScaleMultiplier.y, m_minScaleMultiplier.z), new Vector3(m_maxScaleMultiplier.x, m_maxScaleMultiplier.y, m_maxScaleMultiplier.z), (float)m_scaleRandomizer.NextDouble());
                            }
                            else
                            {
                                multiplier.x = Mathf.Lerp(m_minScaleMultiplier.x, m_maxScaleMultiplier.x, (float)m_scaleRandomizer.NextDouble());
                                multiplier.y = Mathf.Lerp(m_minScaleMultiplier.y, m_maxScaleMultiplier.y, (float)m_scaleRandomizer.NextDouble());
                                multiplier.z = Mathf.Lerp(m_minScaleMultiplier.z, m_maxScaleMultiplier.z, (float)m_scaleRandomizer.NextDouble());
                            }
                        }
                        scale.x *= multiplier.x;
                        scale.y *= multiplier.y;
                        scale.z *= multiplier.z;
                        m_spawned[i].scale = scale;
                    }
                }
                else
                {
                    m_spawned[i].scale = m_spawned[i].baseScale;
                }

                Vector3 right = Vector3.Cross(m_evalResult.forward, m_evalResult.up).normalized;

                Vector3 posOffset = m_minOffset;
                if (hasCustomOffset)
                {
                    m_customOffsetRule.SetContext(this, m_evalResult, i, m_spawned.Length);
                    posOffset = m_customOffsetRule.GetOffset();
                } 
                else if (m_minOffset != m_maxOffset)
                {
                    if(m_shellOffset)
                    {
                        float x = m_maxOffset.x - m_minOffset.x;
                        float y = m_maxOffset.y - m_minOffset.y;
                        float angleInRadians = (float)m_shellRandomizer.NextDouble() * 360f * Mathf.Deg2Rad;
                        posOffset = new Vector2(0.5f * Mathf.Cos(angleInRadians), 0.5f * Mathf.Sin(angleInRadians));
                        posOffset.x *= x;
                        posOffset.y *= y;
                    } else
                    {
                        float rnd = (float)m_offsetRandomizer.NextDouble();
                        posOffset.x = Mathf.Lerp(m_minOffset.x, m_maxOffset.x, rnd);
                        rnd = (float)m_offsetRandomizer.NextDouble();
                        posOffset.y = Mathf.Lerp(m_minOffset.y, m_maxOffset.y, rnd);
                        rnd = (float)m_offsetRandomizer.NextDouble();
                        posOffset.z = Mathf.Lerp(m_minOffset.z, m_maxOffset.z, rnd);
                    }
                }

                if (m_offsetUseWorldCoords)
                {
                    m_spawned[i].position += posOffset;
                }
                else
                {
                    m_spawned[i].position += right * posOffset.x * m_evalResult.size + m_evalResult.up * posOffset.y * m_evalResult.size;
                }

                if (m_applyRotation)
                {
                    if (hasCustomRotation)
                    {
                        m_customRotationRule.SetContext(this, m_evalResult, i, m_spawned.Length);
                        m_spawned[i].rotation = m_customRotationRule.GetRotation();
                    }
                    else
                    {
                        Quaternion offsetRot = Quaternion.Euler(Mathf.Lerp(m_minRotation.x, m_maxRotation.x, (float)m_rotationRandomizer.NextDouble()), Mathf.Lerp(m_minRotation.y, m_maxRotation.y, (float)m_rotationRandomizer.NextDouble()), Mathf.Lerp(m_minRotation.z, m_maxRotation.z, (float)m_rotationRandomizer.NextDouble()));
                        if (m_rotateByOffset) m_spawned[i].rotation = Quaternion.LookRotation(m_evalResult.forward, m_spawned[i].position - m_evalResult.position) * offsetRot;
                        else m_spawned[i].rotation = m_evalResult.rotation * offsetRot;
                    }
                }

                if (m_objectPositioning == Positioning.Clip)
                {
                    if (percent < clipFrom || percent > clipTo) m_spawned[i].active = false;
                    else m_spawned[i].active = true;
                }
                if (m_useCustomObjectDistance)
                {
                    if (objectPositioning == Positioning.Clip)
                    {
                        distancePercentAccum = spline.Travel(distancePercentAccum, Mathf.Lerp(m_minObjectDistance, m_maxObjectDistance, (float)m_distanceRandomizer.NextDouble()));
                    }
                    else
                    {
                        distancePercentAccum = Travel(distancePercentAccum, Mathf.Lerp(m_minObjectDistance, m_maxObjectDistance, (float)m_distanceRandomizer.NextDouble()));
                    }
                }
            }
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            for (int i = 0; i < m_spawned.Length; i++)
            {
                m_spawned[i].Apply();
            }
        }
    }
}
