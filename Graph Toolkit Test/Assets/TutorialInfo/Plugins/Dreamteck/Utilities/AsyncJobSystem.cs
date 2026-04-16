using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

namespace Dreamteck
{
    public class AsyncJobSystem : MonoBehaviour
    {
        private Queue<IJobData> m_jobs = new Queue<IJobData>();

        private IJobData m_currentJob = null;

        private bool m_isWorking = false;

        public AsyncJobOperation ScheduleJob<T>(JobData<T> data)
        {
            m_jobs.Enqueue(data);
            return new AsyncJobOperation(data);
        }

        private void Update()
        {
            if (m_jobs.Count > 0 && !m_isWorking)
            {
                StartCoroutine(JobCoroutine());
            }
        }

        private IEnumerator JobCoroutine()
        {
            m_isWorking = true;
            
            while (m_jobs.Count > 0)
            {
                m_currentJob = m_jobs.Dequeue();
                m_currentJob.Initialize();

                while (!m_currentJob.done)
                {
                    m_currentJob.Next();
                    yield return null;
                }

                m_currentJob.Complete();
                m_currentJob = null;

                yield return null;
            }

            m_isWorking = false;
        }


        public class AsyncJobOperation : CustomYieldInstruction
        {
            private IJobData m_job;
            
            public AsyncJobOperation(IJobData job)
            {
                m_job = job;
            }

            public override bool keepWaiting {
                get { return !m_job.done; }
            }
        }

        public interface IJobData
        {
            bool done { get; }

            void Initialize();

            void Next();

            void Complete();
        }

        public class JobData<T> : IJobData
        {
            private int m_index;

            private int m_iterations = 0;

            private IEnumerable<T> m_collection;

            private Action<JobData<T>> m_onComplete;

            private Action<JobData<T>> m_onIteration;

            private IEnumerator<T> m_enumerator;

            public T current { get { return m_enumerator.Current; } }

            public int index  { get  { return m_index; } }

            public IEnumerable<T> collection { get { return m_collection; } }

            public bool done { get; private set; }

            public JobData(IEnumerable<T> collection, int iterations, Action<JobData<T>> onIteration)
            {
                m_collection = collection;
                m_onIteration = onIteration;
                m_iterations = iterations;
                done = false;
            }

            public JobData(IEnumerable<T> collection, int iterations, Action<JobData<T>> onIteration, Action<JobData<T>> onComplete) :
                this(collection, iterations, onIteration)
            {
                m_onComplete = onComplete;
            }

            public void Initialize()
            {
                m_enumerator = m_collection.GetEnumerator();
                m_index = -1;
                done = !m_enumerator.MoveNext();
            }

            public void Complete()
            {
                m_enumerator.Dispose();

                try
                {
                    if (m_onComplete != null) {
                        m_onComplete(this);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            public void Next()
            {
                int counter = m_iterations;

                if (done)
                {
                    return;
                }
                do
                {
                    m_index++;

                    try
                    {
                        if(m_onIteration != null)
                        {
                            m_onIteration(this);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    done = !m_enumerator.MoveNext();
                }
                while (!done && --counter > 0);
            }
        }
    }
}