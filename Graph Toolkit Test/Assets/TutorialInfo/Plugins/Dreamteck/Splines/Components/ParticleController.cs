namespace Dreamteck.Splines
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    [ExecuteInEditMode]
    [AddComponentMenu("Dreamteck/Splines/Users/Particle Controller")]
    public class ParticleController : SplineUser
    {
        public ParticleSystem particleSystemComponent
        {
            get { return m_particleSystem; }
            set {
                m_particleSystem = value;
                m_renderer = m_particleSystem.GetComponent<ParticleSystemRenderer>();
            }
        }

        [SerializeField]
        [HideInInspector]
        private ParticleSystem m_particleSystem;
        private ParticleSystemRenderer m_renderer;
        public enum EmitPoint { Beginning, Ending, Random, Ordered }
        public enum MotionType { None, UseParticleSystem, FollowForward, FollowBackward, ByNormal, ByNormalRandomized }
        public enum Wrap { Default, Loop }

        [HideInInspector]
        public bool pauseWhenNotVisible = false;
        [HideInInspector]
        public Vector2 offset = Vector2.zero;
        [HideInInspector]
        public bool volumetric = false;
        [HideInInspector]
        public bool emitFromShell = false;
        [HideInInspector]
        public bool apply3Drotation = false;
        [HideInInspector]
        public Vector2 scale = Vector2.one;
        [HideInInspector]
        public EmitPoint emitPoint = EmitPoint.Beginning;
        [HideInInspector]
        public MotionType motionType = MotionType.UseParticleSystem;
        [HideInInspector]
        public Wrap wrapMode = Wrap.Default;
        [HideInInspector]
        public float minCycles = 1f;
        [HideInInspector]
        public float maxCycles = 1f;

        private ParticleSystem.Particle[] m_particles = new ParticleSystem.Particle[0];
        private Particle[] m_controllers = new Particle[0];
        private int m_particleCount = 0;
        private int m_birthIndex = 0;
        private List<Vector4> m_customParticleData = new List<Vector4>();

        protected override void LateRun()
        {
            if (m_particleSystem == null) return;
            if (pauseWhenNotVisible)
            {
                if (m_renderer == null)
                {
                    m_renderer = m_particleSystem.GetComponent<ParticleSystemRenderer>();
                }
                if (!m_renderer.isVisible) return;
            }

            int maxParticles = m_particleSystem.main.maxParticles;
            if (m_particles.Length != maxParticles)
            {
                m_particles = new ParticleSystem.Particle[maxParticles];
                m_customParticleData = new List<Vector4>(maxParticles);
                Particle[] newControllers = new Particle[maxParticles];
                for (int i = 0; i < newControllers.Length; i++)
                {
                    if (i >= m_controllers.Length) break;
                    newControllers[i] = m_controllers[i];
                }
                m_controllers = newControllers;
            }
            m_particleCount = m_particleSystem.GetParticles(m_particles);
            m_particleSystem.GetCustomParticleData(m_customParticleData, ParticleSystemCustomData.Custom1);

            bool isLocal = m_particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local;

            Transform particleSystemTransform = m_particleSystem.transform;

            for (int i = 0; i < m_particleCount; i++)
            {
                if (m_controllers[i] == null)
                {
                    m_controllers[i] = new Particle();
                }
                if (isLocal)
                {
                    TransformParticle(ref m_particles[i], particleSystemTransform);
                }
                if (m_customParticleData[i].w < 1f)
                {
                    OnParticleBorn(i);
                }
                HandleParticle(i);
                if (isLocal)
                {
                    InverseTransformParticle(ref m_particles[i], particleSystemTransform);
                }
            }

            m_particleSystem.SetCustomParticleData(m_customParticleData, ParticleSystemCustomData.Custom1);
            m_particleSystem.SetParticles(m_particles, m_particleCount);
        }

        void TransformParticle(ref ParticleSystem.Particle particle, Transform trs)
        {
            particle.position = trs.TransformPoint(particle.position);
            if (apply3Drotation)
            {

            }
            particle.velocity = trs.TransformDirection(particle.velocity);
        }

        void InverseTransformParticle(ref ParticleSystem.Particle particle, Transform trs)
        {
            particle.position = trs.InverseTransformPoint(particle.position);
            particle.velocity = trs.InverseTransformDirection(particle.velocity);
        }

        protected override void Reset()
        {
            base.Reset();
            updateMethod = UpdateMethod.LateUpdate;
            if (m_particleSystem == null) m_particleSystem = GetComponent<ParticleSystem>();
        }

        void HandleParticle(int index)
        {
            float lifePercent = m_particles[index].remainingLifetime / m_particles[index].startLifetime;
            if (motionType == MotionType.FollowBackward || motionType == MotionType.FollowForward || motionType == MotionType.None)
            {
                Evaluate(m_controllers[index].GetSplinePercent(wrapMode, m_particles[index], motionType), ref m_evalResult);
                Vector3 resultRight = m_evalResult.right;
                m_particles[index].position = m_evalResult.position;
                if (apply3Drotation)
                {
                    m_particles[index].rotation3D = m_evalResult.rotation.eulerAngles;
                }
                Vector2 finalOffset = offset;
                if (volumetric)
                {
                    if (motionType != MotionType.None)
                    {
                        finalOffset += Vector2.Lerp(m_controllers[index].startOffset, m_controllers[index].endOffset, 1f - lifePercent);
                        finalOffset.x *= scale.x;
                        finalOffset.y *= scale.y;
                    } else
                    {
                        finalOffset += m_controllers[index].startOffset;
                    }
                }
                m_particles[index].position += resultRight * (finalOffset.x * m_evalResult.size) + m_evalResult.up * (finalOffset.y * m_evalResult.size);
                m_particles[index].velocity = m_evalResult.forward;
                m_particles[index].startColor = m_controllers[index].startColor * m_evalResult.color;
            }
        }

        private void OnParticleBorn(int index)
        {
            Vector4 custom = m_customParticleData[index];
            custom.w = 1;
            m_customParticleData[index] = custom;
            double percent = 0.0;
            float emissionRate = Mathf.Lerp(m_particleSystem.emission.rateOverTime.constantMin, m_particleSystem.emission.rateOverTime.constantMax, 0.5f);
            float expectedParticleCount = emissionRate * m_particleSystem.main.startLifetime.constantMax;
            m_birthIndex++;
            if (m_birthIndex > expectedParticleCount)
            {
                m_birthIndex = 0;
            }

            switch (emitPoint)
            {
                case EmitPoint.Beginning: percent = 0f; break;
                case EmitPoint.Ending: percent = 1f; break;
                case EmitPoint.Random: percent = Random.Range(0f, 1f); break;
                case EmitPoint.Ordered: percent = expectedParticleCount > 0 ? (float)m_birthIndex / expectedParticleCount : 0f;  break;
            }
            Evaluate(percent, ref m_evalResult);
            m_controllers[index].startColor = m_particles[index].startColor;
            m_controllers[index].startPercent = percent;

            m_controllers[index].cycleSpeed = Random.Range(minCycles, maxCycles);
            Vector2 circle = Vector2.zero;
            if (volumetric)
            {
                if (emitFromShell) circle = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Vector2.right;
                else circle = Random.insideUnitCircle;
            }
            m_controllers[index].startOffset = circle * 0.5f;
            m_controllers[index].endOffset = Random.insideUnitCircle * 0.5f;


            Vector3 right = Vector3.Cross(m_evalResult.forward, m_evalResult.up);
            m_particles[index].position = m_evalResult.position + right * m_controllers[index].startOffset.x * m_evalResult.size * scale.x + m_evalResult.up * m_controllers[index].startOffset.y * m_evalResult.size * scale.y;

            float forceX = m_particleSystem.forceOverLifetime.x.constantMax;
            float forceY = m_particleSystem.forceOverLifetime.y.constantMax;
            float forceZ = m_particleSystem.forceOverLifetime.z.constantMax;
            if (m_particleSystem.forceOverLifetime.randomized)
            {
                forceX = Random.Range(m_particleSystem.forceOverLifetime.x.constantMin, m_particleSystem.forceOverLifetime.x.constantMax);
                forceY = Random.Range(m_particleSystem.forceOverLifetime.y.constantMin, m_particleSystem.forceOverLifetime.y.constantMax);
                forceZ = Random.Range(m_particleSystem.forceOverLifetime.z.constantMin, m_particleSystem.forceOverLifetime.z.constantMax);
            }

            float time = m_particles[index].startLifetime - m_particles[index].remainingLifetime;
            Vector3 forceDistance = new Vector3(forceX, forceY, forceZ) * 0.5f * (time * time);

            float startSpeed = m_particleSystem.main.startSpeed.constantMax;

            if (motionType == MotionType.ByNormal)
            {
                m_particles[index].position += m_evalResult.up * startSpeed * (m_particles[index].startLifetime - m_particles[index].remainingLifetime);
                m_particles[index].position += forceDistance;
                m_particles[index].velocity = m_evalResult.up * startSpeed + new Vector3(forceX, forceY, forceZ) * time;
            }
            else if (motionType == MotionType.ByNormalRandomized)
            {
                Vector3 normal = Quaternion.AngleAxis(Random.Range(0f, 360f), m_evalResult.forward) * m_evalResult.up;
                m_particles[index].position += normal * startSpeed * (m_particles[index].startLifetime - m_particles[index].remainingLifetime);
                m_particles[index].position += forceDistance;
                m_particles[index].velocity = normal * startSpeed + new Vector3(forceX, forceY, forceZ) * time;
            }
            HandleParticle(index);
        }

        public class Particle
        {
            internal Vector2 startOffset = Vector2.zero;
            internal Vector2 endOffset = Vector2.zero;
            internal float cycleSpeed = 0f;
            internal Color startColor = Color.white;
            internal double startPercent = 0.0;

            internal double GetSplinePercent(Wrap wrap, ParticleSystem.Particle particle, MotionType motionType)
            {
                float lifePercent = particle.remainingLifetime / particle.startLifetime;
                if(motionType == MotionType.FollowBackward)
                {
                    lifePercent = 1f - lifePercent;
                }
                switch (wrap)
                {
                    case Wrap.Default: return Dmath.Clamp01(startPercent + (1f - lifePercent) * cycleSpeed);
                    case Wrap.Loop:
                        double loopPoint = startPercent + (1.0 - lifePercent) * cycleSpeed;
                        if(loopPoint > 1.0) loopPoint -= Mathf.FloorToInt((float)loopPoint);
                        return loopPoint;
                }
                return 0.0;
            }
        }
    }
}
