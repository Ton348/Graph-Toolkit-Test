using UnityEngine;

namespace Dreamteck.Splines {
    [ExecuteInEditMode]
    public class SplineUser : MonoBehaviour, ISerializationCallbackReceiver, ISampleModifier
    {
        public enum UpdateMethod { Update, FixedUpdate, LateUpdate }
        [HideInInspector]
        public UpdateMethod updateMethod = UpdateMethod.Update;

        public SplineComputer spline
        {
            get {
                return m_spline;
            }
            set
            {
                if (value != m_spline)
                {
                    if (m_spline != null)
                    {
                        m_spline.Unsubscribe(this);
                    }
                    m_spline = value;
                    if (m_spline != null)
                    {
                        m_spline.Subscribe(this);
                        Rebuild();
                    }
                    OnSplineChanged();
                }
            }
        }

        public double clipFrom
        {
            get
            {
                return m_clipFrom;
            }
            set
            {
                if (value != m_clipFrom)
                {
                    m_animClipFrom = (float)m_clipFrom;
                    m_clipFrom = Dmath.Clamp01(value);
                    if (m_clipFrom > m_clipTo)
                    {
                        if (!m_spline.isClosed) m_clipTo = m_clipFrom;
                    }
                    m_getSamples = true;
                    Rebuild();
                }
            }
        }

        public double clipTo
        {
            get
            {
                return m_clipTo;
            }
            set
            {

                if (value != m_clipTo)
                {
                    m_animClipTo = (float)m_clipTo;
                    m_clipTo = Dmath.Clamp01(value);
                    if (m_clipTo < m_clipFrom)
                    {
                        if (!m_spline.isClosed) m_clipFrom = m_clipTo;
                    }
                    m_getSamples = true;
                    Rebuild();
                }
            }
        }

        public bool autoUpdate
        {
            get
            {
                return m_autoUpdate;
            }
            set
            {
                if (value != m_autoUpdate)
                {
                    m_autoUpdate = value;
                    if (value) Rebuild();
                }
            }
        }

        public bool loopSamples
        {
            get
            {
                return m_loopSamples;
            }
            set
            {
                if (value != m_loopSamples)
                {
                    m_loopSamples = value;
                    if(!m_loopSamples && m_clipTo < m_clipFrom)
                    {
                        double temp = m_clipTo;
                        m_clipTo = m_clipFrom;
                        m_clipFrom = temp;
                    }
                    Rebuild();
                }
            }
        }

        //The percent of the spline that we're traversing
        public double span
        {
            get
            {
                if (samplesAreLooped) return (1.0 - m_clipFrom) + m_clipTo;
                return m_clipTo - m_clipFrom;
            }
        }

        public bool samplesAreLooped
        {
            get
            {
                return m_loopSamples && m_clipFrom >= m_clipTo;
            }
        }

        public RotationModifier rotationModifier
        {
            get
            {
                return m_rotationModifier;
            }
        }

        public OffsetModifier offsetModifier
        {
            get
            {
                return m_offsetModifier;
            }
        }

        public ColorModifier colorModifier
        {
            get
            {
                return m_colorModifier;
            }
        }

        public SizeModifier sizeModifier
        {
            get
            {
                return m_sizeModifier;
            }
        }

        //Serialized values
        [SerializeField]
        [HideInInspector]
        private SplineComputer m_spline;
        [SerializeField]
        [HideInInspector]
        private bool m_autoUpdate = true;
        [SerializeField]
        [HideInInspector]
        protected RotationModifier m_rotationModifier = new RotationModifier();
        [SerializeField]
        [HideInInspector]
        protected OffsetModifier m_offsetModifier = new OffsetModifier();
        [SerializeField]
        [HideInInspector]
        protected ColorModifier m_colorModifier = new ColorModifier();
        [SerializeField]
        [HideInInspector]
        protected SizeModifier m_sizeModifier = new SizeModifier();
        [SerializeField]
        [HideInInspector]
        private SplineSample m_clipFromSample = new SplineSample(), m_clipToSample = new SplineSample();

        [SerializeField]
        [HideInInspector]
        private bool m_loopSamples = false;
        [SerializeField]
        [HideInInspector]
        private double m_clipFrom = 0.0;
        [SerializeField]
        [HideInInspector]
        private double m_clipTo = 1.0;

        //float values used for making animations
        [SerializeField]
        [HideInInspector]
        private float m_animClipFrom = 0f;
        [SerializeField]
        [HideInInspector]
        private float m_animClipTo = 1f;

        private SampleCollection m_sampleCollection = new SampleCollection();
        private bool m_rebuild = false, m_getSamples = false, m_postBuild = false;
        private Transform m_trs = null;
        private bool m_hasTransform = false;
        private SplineSample m_workSample = new SplineSample();
#if UNITY_EDITOR
        private bool m_isPlaying = false;
        protected bool isPlaying => m_isPlaying;
#endif

        protected Transform trs
        {
            get {  return m_trs;  }
        }
        protected bool hasTransform
        {
            get { return m_hasTransform; }
        }
        public int sampleCount
        {
            get { return m_sampleCount; }
        }

        private int m_sampleCount = 0, m_startSampleIndex = 0;
        /// <summary>
        /// Use this to work with the Evaluate and Project methods
        /// </summary>
        protected SplineSample m_evalResult = new SplineSample();

        //Threading values
        [HideInInspector]
        public volatile bool multithreaded = false;
        [HideInInspector]
        public bool buildOnAwake = true;
        [HideInInspector]
        public bool buildOnEnable = false;

        public event EmptySplineHandler onPostBuild;

#if UNITY_EDITOR
        public virtual void EditorAwake()
        {

        }
#endif

        protected virtual void Awake() {
#if UNITY_EDITOR
            m_isPlaying = Application.isPlaying;
            if (!m_isPlaying)
            {
                if (spline != null)
                {
                    if (!m_spline.IsSubscribed(this))
                    {
                        m_spline.Subscribe(this);
                        UnityEditor.EditorUtility.SetDirty(spline);
                    }
                }
            }
#endif

            CacheTransform();
            if (buildOnAwake && Application.isPlaying)
            {
                RebuildImmediate();
            } else
            {
                GetSamples();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                RebuildImmediate();
            }
#endif
        }

        protected void CacheTransform()
        {
            m_trs = transform;
            m_hasTransform = true;
        }

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            spline = GetComponent<SplineComputer>();
            Awake();
#endif
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            if (!m_isPlaying || buildOnEnable)
            {
                RebuildImmediate();
            }
#else
            if (buildOnEnable){
                RebuildImmediate();
            }
#endif
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
#if UNITY_EDITOR
            if (!m_isPlaying && spline != null)
            {
                m_spline.Unsubscribe(this); //Unsubscribe if DestroyImmediate is called
            }
#endif
        }

        protected virtual void OnDidApplyAnimationProperties()
        {
            bool clip = false;
            if (m_clipFrom != m_animClipFrom || m_clipTo != m_animClipTo) clip = true;
            m_clipFrom = m_animClipFrom;
            m_clipTo = m_animClipTo;
            Rebuild();
            if (clip) GetSamples();
        }

        /// <summary>
        /// Gets the sample at the given index without modifications
        /// </summary>
        /// <param name="index">Sample index</param>
        /// <returns></returns>
        public void GetSampleRaw(int index, ref SplineSample sample)
        {
            if (index == 0)
            {
                sample.FastCopy(ref m_clipFromSample);
                return;
            }
            if (index == m_sampleCount - 1)
            {
                sample.FastCopy(ref m_clipToSample);
                return;
            }

            ClampLoopSampleIndex(ref index);
            sample.FastCopy(ref m_sampleCollection.samples[index]);
        }

        public double GetSamplePercent(int index)
        {
            if (index == 0)
            {
                return m_clipFromSample.percent;
            }
            if (index == m_sampleCount - 1)
            {
                return m_clipToSample.percent;
            }

            ClampLoopSampleIndex(ref index);
            return m_sampleCollection.samples[index].percent;
        }

        private void ClampLoopSampleIndex(ref int index)
        {
            if (index >= m_sampleCount)
            {
                index = m_sampleCount - 1;
            }

            if (samplesAreLooped)
            {
                int start;
                double lerp;
                m_sampleCollection.GetSamplingValues(clipFrom, out start, out lerp);

                index = start + index;
                if (index >= m_sampleCollection.length)
                {
                    index -= m_sampleCollection.length;
                }
            }
            else
            {
                index = m_startSampleIndex + index;
            }
        }


        /// <summary>
        /// Returns the sample at the given index with modifiers applied
        /// </summary>
        /// <param name="index">Sample index</param>
        /// <param name="target">Sample to write to</param>
        public void GetSample(int index, ref SplineSample target)
        {
            GetSampleRaw(index, ref target);
            ApplySampleModifiers(ref target);
        }

        /// <summary>
        /// Returns the sample at the given index with modifiers applied and
        /// applies compensation to the size parameter based on the angle between the samples
        /// </summary>
        public void GetSampleWithAngleCompensation(int index, ref SplineSample target)
        {
            GetSampleRaw(index, ref target);
            ApplySampleModifiers(ref target);
            if (index > 0 && index < sampleCount - 1)
            {
                GetSampleRaw(index - 1, ref m_workSample);
                ApplySampleModifiers(ref target);
                Vector3 prev = target.position - m_workSample.position;
                GetSampleRaw(index + 1, ref m_workSample);
                ApplySampleModifiers(ref target);
                Vector3 next = m_workSample.position - target.position;
                target.size *= 1 / Mathf.Sqrt(Vector3.Dot(prev.normalized, next.normalized) * 0.5f + 0.5f);
            }
        }


        /// <summary>
        /// Rebuild the SplineUser. This will cause Build and Build_MT to be called.
        /// </summary>
        /// <param name="sampleComputer">Should the SplineUser sample the SplineComputer</param>
        public virtual void Rebuild()
        {
#if UNITY_EDITOR
            if (!m_hasTransform)
            {
                CacheTransform();
            }

            //If it's the editor and it's not playing, then rebuild immediate
            if (m_isPlaying)
            {
                if (!autoUpdate) return;
                m_rebuild = m_getSamples = true;
            }
            else
            {
                RebuildImmediate();
            }
#else
             if (!autoUpdate) return;
             rebuild = getSamples = true;
#endif
        }

        /// <summary>
        /// Rebuild the SplineUser immediate. This method will call sample samples and call Build as soon as it's called even if the component is disabled.
        /// </summary>
        /// <param name="sampleComputer">Should the SplineUser sample the SplineComputer</param>
        public virtual void RebuildImmediate()
        {
#if UNITY_EDITOR
            if (!m_hasTransform)
            {
                CacheTransform();
            }
#endif
            try
            {
                GetSamples();
                Build();
                PostBuild();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            m_rebuild = false;
            m_getSamples = false;
        }

        private void Update()
        {
            if (updateMethod == UpdateMethod.Update)
            {
                Run();
                RunUpdate();
                LateRun();
            }
        }

        private void LateUpdate()
        {
            if (updateMethod == UpdateMethod.LateUpdate)
            {
                Run();
                RunUpdate();
                LateRun();
            }
#if UNITY_EDITOR
            if(!m_isPlaying && updateMethod == UpdateMethod.FixedUpdate)
            {
                Run();
                RunUpdate();
                LateRun();
            }
#endif
        }

        private void FixedUpdate()
        {
            if (updateMethod == UpdateMethod.FixedUpdate)
            {
                Run();
                RunUpdate();
                LateRun();
            }
        }

        //Update logic for handling threads and rebuilding
        private void RunUpdate()
        {
#if UNITY_EDITOR
            if (!m_isPlaying) return;
#endif
            //Handle rebuilding
            if (m_rebuild)
            {
                if (multithreaded)
                {
                    if (m_getSamples) SplineThreading.Run(ResampleAndBuildThreaded);
                    else SplineThreading.Run(BuildThreaded);
                }
                else
                {
                    if (m_getSamples || m_spline.sampleMode == SplineComputer.SampleMode.Optimized) GetSamples();
                    Build();
                    m_postBuild = true;
                }
                m_rebuild = false;
            }
            if (m_postBuild)
            {
                PostBuild();
                EmptySplineHandler postBuildHandler = onPostBuild;
                if(postBuildHandler != null)
                {
                    postBuildHandler();
                }
                m_postBuild = false;
            }
        }

        void BuildThreaded()
        {
            while (m_postBuild)
            {
                //Wait if the main thread is still running post build operations
            }
            Build();
            m_postBuild = true;
        }

        private void ResampleAndBuildThreaded()
        {
            while (m_postBuild)
            {
                //Wait if the main thread is still running post build operations
            }
            GetSamples();
            Build();
            m_postBuild = true;
        }

        /// Code to run every Update/FixedUpdate/LateUpdate before any building has taken place
        protected virtual void Run()
        {

        }

        /// Code to run every Update/FixedUpdate/LateUpdate after any rabuilding has taken place
        protected virtual void LateRun()
        {

        }

        //Used for calculations. Called on the main or the worker thread.
        protected virtual void Build()
        {
        }

        //Called on the Main thread only - used for applying the results from Build
        protected virtual void PostBuild()
        {
        }

        protected virtual void OnSplineChanged()
        {

        }

        /// <summary>
        /// Applies the SplineUser modifiers to the provided sample
        /// </summary>
        /// <param name="sample">The sample to modify</param>
        public void ApplySampleModifiers(ref SplineSample sample)
        {
            ApplyModifier(m_offsetModifier, ref sample);
            ApplyModifier(m_rotationModifier, ref sample);
            ApplyModifier(m_colorModifier, ref sample);
            ApplyModifier(m_sizeModifier, ref sample);
        }

        public Vector3 GetModifiedSamplePosition(ref SplineSample sample)
        {
            if (m_offsetModifier.hasKeys)
            {
                Vector2 offset = m_offsetModifier.Evaluate(sample.percent);
                return sample.position + sample.right * offset.x + sample.up * offset.y; ;
            }
            return sample.position;
        }

        private void ApplyModifier(SplineSampleModifier modifier, ref SplineSample sample)
        {
            if (!modifier.hasKeys) return;
            if (modifier.useClippedPercent)
            {
                ClipPercent(ref sample.percent);
            }

            modifier.Apply(ref sample);

            if (modifier.useClippedPercent)
            {
                UnclipPercent(ref sample.percent);
            }
        }

        /// <summary>
        /// Sets the clip range of the SplineUser. Same as setting clipFrom and clipTo
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void SetClipRange(double from, double to)
        {
            if (!m_spline.isClosed && to < from) to = from;
            m_clipFrom = Dmath.Clamp01(from);
            m_clipTo = Dmath.Clamp01(to);
            GetSamples();
            Rebuild();
        }

        /// <summary>
        /// Gets the clipped samples defined by clipFrom and clipTo
        /// </summary>
        private void GetSamples()
        {
            m_getSamples = false;
            if (spline == null)
            {
                m_sampleCollection.samples = new SplineSample[0];
                m_sampleCount = 0;
                return;
            }

            m_spline.GetSamples(m_sampleCollection);

            if (m_sampleCollection.length == 0)
            {
                m_sampleCount = 0;
                return;
            }

            if (m_clipFrom != 0.0)
            {
                m_sampleCollection.Evaluate(clipFrom, ref m_clipFromSample);
            } else
            {
                m_clipFromSample = m_sampleCollection.samples[0];
            }

            if(m_clipTo != 1.0)
            {
                m_sampleCollection.Evaluate(m_clipTo, ref m_clipToSample);
            } else
            {
                m_clipToSample = m_sampleCollection.samples[m_sampleCollection.length - 1];
            }

            int start, end;
            m_sampleCount = m_sampleCollection.GetClippedSampleCount(m_clipFrom, m_clipTo, out start, out end);
            double lerp;
            m_sampleCollection.GetSamplingValues(m_clipFrom, out m_startSampleIndex, out lerp);
        }

        /// <summary>
        /// Takes a regular 0-1 percent mapped to the start and end of the spline and maps it to the clipFrom and clipTo valies. Useful for working with clipped samples
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public double ClipPercent(double percent)
        {
            ClipPercent(ref percent);
            return percent;
        }

        /// <summary>
        /// Takes a regular 0-1 percent mapped to the start and end of the spline and maps it to the clipFrom and clipTo valies. Useful for working with clipped samples
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public void ClipPercent(ref double percent)
        {
            if (m_sampleCollection.length == 0)
            {
                percent = 0.0;
                return;
            }

            if (samplesAreLooped)
            {
                if (percent >= clipFrom && percent <= 1.0) { percent = Dmath.InverseLerp(clipFrom, clipFrom + span, percent); }//If in the range clipFrom - 1.0
                else if (percent <= clipTo) { percent = Dmath.InverseLerp(clipTo - span, clipTo, percent); } //if in the range 0.0 - clipTo
                else
                {
                    //Find the nearest clip start
                    if (Dmath.InverseLerp(clipTo, clipFrom, percent) < 0.5) percent = 1.0;
                    else percent = 0.0;
                }
            }
            else percent = Dmath.InverseLerp(clipFrom, clipTo, percent);
        }

        public double UnclipPercent(double percent)
        {
            UnclipPercent(ref percent);
            return percent;
        }

        public void UnclipPercent(ref double percent)
        {
            if (samplesAreLooped)
            {
                if (span <= 0.00001)
                {
                    percent = clipFrom;
                    return;
                }
                double fromRatio = (1.0 - clipFrom) / span;
                if (percent < fromRatio)
                {
                    percent = Dmath.Lerp(clipFrom, 1.0, percent / fromRatio);
                }
                else if (clipTo == 0.0)
                {
                    percent = 0.0;
                    return;
                }
                else percent = Dmath.Lerp(0.0, clipTo, (percent - fromRatio) / (clipTo / span));
            }
            else
            {
                if (percent == 0.0)
                {
                    percent = clipFrom;
                    return;
                }
                else if (percent == 1.0)
                {
                    percent = clipTo;
                    return;
                }

                percent = Dmath.Lerp(clipFrom, clipTo, percent);
            }
            percent = Dmath.Clamp01(percent);
        }

        private int GetSampleIndex(double percent)
        {
            int index;
            double lerp;
            m_sampleCollection.GetSamplingValues(UnclipPercent(percent), out index, out lerp);
            return index;
        }

        public Vector3 EvaluatePosition(double percent)
        {
            return m_sampleCollection.EvaluatePosition(UnclipPercent(percent));
        }

        public void Evaluate(double percent, ref SplineSample result)
        {
            m_sampleCollection.Evaluate(UnclipPercent(percent), ref result);
            result.percent = Dmath.Clamp01(percent);
            ApplySampleModifiers(ref result);
        }

        public SplineSample Evaluate(double percent)
        {
            SplineSample result = new SplineSample();
            Evaluate(percent, ref result);
            result.percent = Dmath.Clamp01(percent);
            ApplySampleModifiers(ref result);
            return result;
        }

        public void Evaluate(ref SplineSample[] results, double from = 0.0, double to = 1.0)
        {
            m_sampleCollection.Evaluate(ref results, UnclipPercent(from), UnclipPercent(to));
            for (int i = 0; i < results.Length; i++)
            {
                ClipPercent(ref results[i].percent);
                ApplySampleModifiers(ref results[i]);
            }
        }

        public void EvaluatePositions(ref Vector3[] positions, double from = 0.0, double to = 1.0)
        {
            m_sampleCollection.EvaluatePositions(ref positions, UnclipPercent(from), UnclipPercent(to));
        }

        public double Travel(double start, float distance, Spline.Direction direction, out float moved)
        {
            moved = 0f;
            if (direction == Spline.Direction.Forward && start >= 1.0)
            {
                return 1.0;
            }
            else if (direction == Spline.Direction.Backward && start <= 0.0)
            {
                return 0.0;
            }
            if (distance == 0f)
            {
                return Dmath.Clamp01(start);
            }

            double result = m_sampleCollection.Travel(UnclipPercent(start), distance, direction, out moved, clipFrom, clipTo);
            double clippedResult = ClipPercent(result);

            if (result > clipTo)
            {
                moved -= m_sampleCollection.CalculateLength(clipTo, result);
            }
            else if (result < clipFrom)
            {
                moved -= m_sampleCollection.CalculateLength(result, clipFrom);
            }

            return clippedResult;
        }

        public double Travel(double start, float distance, Spline.Direction direction = Spline.Direction.Forward)
        {
            float moved;
            return Travel(start, distance, direction, out moved);
        }

        public double TravelWithOffset(double start, float distance, Spline.Direction direction, Vector3 offset, out float moved)
        {
            moved = 0f;
            if (direction == Spline.Direction.Forward && start >= 1.0)
            {
                return 1.0;
            }
            else if (direction == Spline.Direction.Backward && start <= 0.0)
            {
                return 0.0;
            }
            if (distance == 0f)
            {
                return Dmath.Clamp01(start);
            }
            double result = m_sampleCollection.TravelWithOffset(UnclipPercent(start), distance, direction, offset, out moved, clipFrom, clipTo);
            return ClipPercent(result);
        }

        public virtual void Project(Vector3 position, ref SplineSample result, double from = 0.0, double to = 1.0)
        {
            if (m_spline == null) return;
            m_sampleCollection.Project(position, m_spline.pointCount, ref result, UnclipPercent(from), UnclipPercent(to), this);
            ClipPercent(ref result.percent);
        }

        public float CalculateLength(double from = 0.0, double to = 1.0, bool preventInvert = true)
        {
            return m_sampleCollection.CalculateLength(UnclipPercent(from), UnclipPercent(to), preventInvert);
        }

        public float CalculateLengthWithOffset(Vector3 offset, double from = 0.0, double to = 1.0)
        {
            return m_sampleCollection.CalculateLengthWithOffset(offset, UnclipPercent(from), UnclipPercent(to));
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// Returns the offset transformed by the sample
        /// </summary>
        /// <param name="sample">Source sample</param>
        /// <param name="localOffset">Local offset to apply</param>
        /// <returns></returns>
        protected static Vector3 TransformOffset(SplineSample sample, Vector3 localOffset)
        {
            return (sample.right * localOffset.x + sample.up * localOffset.y + sample.forward * localOffset.z) * sample.size;
        }
    }
}
