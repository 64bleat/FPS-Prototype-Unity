using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Handles callbacks of job requests
    /// </summary>
    public class JobManager : MonoBehaviour
    {
        static readonly List<ActiveJob> _activeJobs = new();
        static readonly Stack<ActiveJob> _availableJobs = new();

        class ActiveJob
        {
            public JobHandle handle;
            public IJob job;
            public Action<IJob> callback;
            public int tick;
        }

        void OnDestroy()
        {
            // Finish all jobs on destroy
            foreach (ActiveJob job in _activeJobs)
            {
                job.handle.Complete();
                job.callback?.Invoke(job.job);
            }

            _activeJobs.Clear();
            _availableJobs.Clear();
        }

        void LateUpdate()
        {
            // Process and remove completed jobs
            int i = 0;

            while (i < _activeJobs.Count)
                if (_activeJobs[i].handle.IsCompleted)
                {
                    _activeJobs[i].handle.Complete();
                    _activeJobs[i].callback?.Invoke(_activeJobs[i].job);
                    _availableJobs.Push(_activeJobs[i]);
                    _activeJobs.RemoveAt(i);
                }
                else
                    i++;
        }

        /// <summary>
        /// Scedule an async job with an attached callback that will be called on the first LateUpdate
        /// the job is complete.
        /// </summary>
        public static JobHandle Schedule<T>(T job, Action<IJob> callback = null, JobHandle require = default) where T : struct, IJob
        {
            ActiveJob jobData;

            if (_availableJobs.Count > 0)
                jobData = _availableJobs.Pop();
            else
                jobData = new ActiveJob();

            jobData.handle = job.Schedule(require);
            jobData.job = job;
            jobData.callback = callback;
            jobData.tick = Time.frameCount;

            if (jobData.handle.IsCompleted)
            {
                jobData.handle.Complete();
                jobData.callback?.Invoke(jobData.job);
            }
            else
                _activeJobs.Add(jobData);

            return jobData.handle;
        }
    }
}