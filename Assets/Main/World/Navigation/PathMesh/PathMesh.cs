using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
    [RequireComponent(typeof(MeshFilter))]
    public class PathMesh : MonoBehaviour
    {
        public ThreadedSplayedMesh threadMesh;
        private static readonly Dictionary<GetPathJob, JobHandle> openJobs = new Dictionary<GetPathJob, JobHandle>();

        private void Awake()
        {
            threadMesh = new ThreadedSplayedMesh(GetComponent<MeshFilter>().mesh, transform);

            Navigator.AddPathMesh(this);
        }

        private void OnDestroy()
        {

            foreach (var kvp in openJobs)
                kvp.Value.Complete();

            threadMesh.vertices.Dispose();
            threadMesh.triangles.Dispose();
            threadMesh.normals.Dispose();
            threadMesh.neighbors.Dispose();
            threadMesh.centers.Dispose();
        }

        public JobHandle RequestPath(Vector3 origin, Vector3 destination, Action<Vector3[]> getPathCallback, float height = 0)
        {
            GetPathJob startJob = new GetPathJob()
            {
                start = origin,
                end = destination,
                height = height,
                mesh = threadMesh,
                nPath = new NativeArray<Vector3>(50, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                nLength = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory)
            };

            void Callback(IJob job)
            {
                GetPathJob gpj = (GetPathJob)job;

                getPathCallback(gpj.nPath.GetSubArray(0, gpj.nLength[0]).ToArray());
                gpj.nPath.Dispose();
                gpj.nLength.Dispose();
                openJobs.Remove(gpj);
            }

            if (openJobs.TryGetValue(startJob, out JobHandle handle))
            {
                handle.Complete();
                openJobs.Remove(startJob);
            }

            handle = JobManager.Schedule(startJob, Callback);
            openJobs.Add(startJob, handle);

            return handle;
        }
    }
}
