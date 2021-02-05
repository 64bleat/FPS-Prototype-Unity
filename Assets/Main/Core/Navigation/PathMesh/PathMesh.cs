using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace MPCore
{
    [RequireComponent(typeof(MeshFilter))]
    public class PathMesh : MonoBehaviour
    {
        public SplayedMesh threadMesh;

        public static readonly List<PathMesh> activeMeshes = new List<PathMesh>();

        private readonly Dictionary<Guid, (JobHandle handle, List<Vector3> path)> openJobs = new Dictionary<Guid, (JobHandle handle, List<Vector3> path)>();
        private readonly Stack<PathRequest> availableJobs = new Stack<PathRequest>();

        private void Awake()
        {
            threadMesh = new SplayedMesh(GetComponent<MeshFilter>().mesh, transform);

            if (TryGetComponent(out MeshRenderer render))
                render.enabled = false;
        }

        private void OnEnable()
        {
            activeMeshes.Add(this);
        }

        private void OnDisable()
        {
            activeMeshes.Remove(this);
        }

        private void OnDestroy()
        {
            foreach (var kvp in openJobs)
                kvp.Value.handle.Complete();

            threadMesh.vertices.Dispose();
            threadMesh.triangles.Dispose();
            threadMesh.normals.Dispose();
            threadMesh.neighbors.Dispose();
            threadMesh.centers.Dispose();

            foreach (PathRequest job in availableJobs)
                job.Dispose();
        }

        public JobHandle RequestPath(Vector3 startPosition, Vector3 endPosition, List<Vector3> fillPath, float height = 0)
        {
            PathRequest startJob;

            if (availableJobs.Count != 0)
            {
                startJob = availableJobs.Pop();
            }
            else
            {
                startJob = new PathRequest();
                startJob.mesh = threadMesh;
                startJob.Allocate();
            }

            startJob.guid = Guid.NewGuid();
            startJob.startPosition = startPosition;
            startJob.endPosition = endPosition;
            startJob.height = height;

            JobHandle handle = JobManager.Schedule(startJob, PathRequestCallback);
            openJobs.Add(startJob.guid, (handle, fillPath));

            return handle;
        }

        private void PathRequestCallback(IJob ijob)
        {
            PathRequest job = (PathRequest)ijob;

            if (job.path.IsCreated && openJobs.TryGetValue(job.guid, out var jobInfo))
            {
                jobInfo.path.Clear();

                for (int i = 0; i < job.path.Length; i++)
                    jobInfo.path.Add(job.path[i]);
            }

            openJobs.Remove(job.guid);
            availableJobs.Push(job);
        }

        private void OnDrawGizmosSelected()
        {
            if (TryGetComponent(out MeshRenderer mr))
            {
                
    
            }
        }
    }
}
