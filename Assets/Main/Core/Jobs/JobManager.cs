using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    public class JobManager : MonoBehaviour
    {
        private static readonly List<JobInfo> activeJobs = new List<JobInfo>();
        private static readonly Stack<JobInfo> availableJobs = new Stack<JobInfo>();

        private class JobInfo
        {
            public JobHandle handle;
            public IJob job;
            public Action<IJob> callback;
            public int tick;
        }

        private void OnDestroy()
        {
            // Finish all jobs on destroy
            foreach (JobInfo job in activeJobs)
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

        public static JobHandle Schedule<T>(T job, Action<IJob> callback = null, JobHandle require = default) where T : struct, IJob
        {
            JobInfo jobData = availableJobs.Count > 0 ? availableJobs.Pop() : new JobInfo();

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