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
        private static readonly List<ActiveJob> activeJobs = new List<ActiveJob>();
        private static readonly Stack<ActiveJob> availableJobs = new Stack<ActiveJob>();

        private class ActiveJob
        {
            public JobHandle handle;
            public IJob job;
            public Action<IJob> callback;
            public int tick;
        }

        private void OnDestroy()
        {
            // Finish all jobs on destroy
            foreach (ActiveJob job in activeJobs)
            {
                job.handle.Complete();
                job.callback?.Invoke(job.job);
            }

            activeJobs.Clear();
            availableJobs.Clear();
        }

        private void LateUpdate()
        {
            // Process and remove completed jobs
            int i = 0;

            while (i < activeJobs.Count)
                if (activeJobs[i].handle.IsCompleted)
                {
                    activeJobs[i].handle.Complete();
                    activeJobs[i].callback?.Invoke(activeJobs[i].job);
                    availableJobs.Push(activeJobs[i]);
                    activeJobs.RemoveAt(i);
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

            if (availableJobs.Count > 0)
                jobData = availableJobs.Pop();
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
                activeJobs.Add(jobData);

            return jobData.handle;
        }
    }
}