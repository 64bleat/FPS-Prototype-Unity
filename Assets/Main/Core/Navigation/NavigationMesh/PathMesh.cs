using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
	[RequireComponent(typeof(MeshFilter))]
	public class PathMesh : MonoBehaviour
	{
		readonly Dictionary<Guid, PathJobHandle> _activeJobs = new();
		readonly Stack<PathMeshNavJob> _inactiveJobs = new();
		MeshFilter _meshFilter;
		NavModel _navModel;
		SplayedMesh _splayedMesh;

		struct PathJobHandle
		{
			public JobHandle jobHandle;
			public List<Vector3> path;
		}

		public SplayedMesh SplayedMesh => _splayedMesh;

		void Awake()
		{
			_navModel = Models.GetModel<NavModel>();
			_meshFilter = GetComponent<MeshFilter>();
			_splayedMesh = new SplayedMesh(_meshFilter.mesh, transform);

			if (TryGetComponent(out MeshRenderer render))
				render.enabled = false;
		}

		void OnEnable()
		{
			_navModel.activeMeshes.Add(this);
		}

		void OnDisable()
		{
			_navModel.activeMeshes.Remove(this);
		}

		void OnDestroy()
		{
			foreach (PathJobHandle pathJobHandle in _activeJobs.Values)
				pathJobHandle.jobHandle.Complete();

			_splayedMesh.vertices.Dispose();
			_splayedMesh.triangles.Dispose();
			_splayedMesh.normals.Dispose();
			_splayedMesh.neighbors.Dispose();
			_splayedMesh.centers.Dispose();

			foreach (PathMeshNavJob job in _inactiveJobs)
				job.Dispose();
		}

		public JobHandle StartPathJob(Vector3 startPosition, Vector3 endPosition, List<Vector3> fillPath, float height = 0)
		{
			PathMeshNavJob startJob = _inactiveJobs.Count > 0 ? _inactiveJobs.Pop() : new PathMeshNavJob(_splayedMesh);
			PathJobHandle pathJobHandle = new PathJobHandle()
			{
				jobHandle = startJob.StartPathJob(startPosition, endPosition, height, PathJobResult),
				path = fillPath
			};

			_activeJobs.Add(startJob.guid, pathJobHandle);

			return pathJobHandle.jobHandle;
		}

		void PathJobResult(IJob iResult)
		{
			PathMeshNavJob result = (PathMeshNavJob)iResult;

			if (result.nativePath.IsCreated && _activeJobs.TryGetValue(result.guid, out var jobInfo))
			{
				jobInfo.path.Clear();

				if (jobInfo.path.Capacity < result.nativePath.Length)
					jobInfo.path.Capacity = result.nativePath.Length;

				for(int i = 0; i < result.nativePath.Length; i++)
					jobInfo.path.Add(result.nativePath[i]);
			}

			_activeJobs.Remove(result.guid);
			_inactiveJobs.Push(result);
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireMesh(_meshFilter.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
			Gizmos.color = Color.white;
		}
	}
}
