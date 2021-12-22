using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace MPCore
{
	/// <summary>
	/// Asynchronous A* path meth navigation algorithm
	/// </summary>
	public struct PathMeshNavJob : IJob
	{
		public Guid guid;
		public bool pathValid;
		public Vector3 startPosition;
		public Vector3 endPosition;
		public float height;
		public SplayedMesh mesh;
		public UnsafeRingQueue<int> heads;
		public NativeList<Vector3> nativePath;
		public NativeArray<int> flow;
		public NativeArray<float> flowDistance;
		public NativeList<PathNode> slidePath;

		struct Triangle
		{
			public Vector3 v0, v1, v2;

			public Vector3 this[int index]
			{
				get
				{
					switch (index % 3)
					{
						case 2:
							return v2;
						case 1:
							return v1;
						default:
							return v0;
					}
				}
				set
				{
					switch (index % 3)
					{
						case 2:
							v2 = value;
							break;
						case 1:
							v1 = value;
							break;
						default:
							v0 = value;
							break;
					}
				}
			}
		}

		public PathMeshNavJob(SplayedMesh splayedMesh)
		{
			guid = default;
			pathValid = default;
			startPosition = default;
			endPosition = default;
			height = default;
			mesh = splayedMesh;
			heads = new UnsafeRingQueue<int>(32, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			nativePath = new NativeList<Vector3>(Allocator.Persistent);
			flow = new NativeArray<int>(mesh.triangles.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			flowDistance = new NativeArray<float>(mesh.triangles.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			slidePath = new NativeList<PathNode>(Allocator.Persistent);
		}

		public JobHandle StartPathJob(Vector3 startPosition, Vector3 endPosition, float height, Action<IJob> result)
		{
			guid = Guid.NewGuid();
			this.startPosition = startPosition;
			this.endPosition = endPosition;
			this.height = height;

			return JobManager.Schedule(this, result);
		}

		public void Dispose()
		{
			heads.Dispose();
			nativePath.Dispose();
			flow.Dispose();
			flowDistance.Dispose();
			slidePath.Dispose();
		}

		public void Execute()
		{
			Vector3 localClampedStart = NearestPointOnMesh(startPosition, out int startTriangle);
			Vector3 localClampedEnd = NearestPointOnMesh(endPosition, out int endTriangle);

			pathValid = endTriangle >= 0;

			if (!pathValid)
				return;

			ComputeTriangleFlow(endTriangle);
			ComputePath(localClampedStart, startTriangle, localClampedEnd);
		}

		public static Vector3 BarycentricPosition(Vector3 point, Vector3 t0, Vector3 t1, Vector3 t2)
		{
			Vector3 areaNormal = Vector3.Cross(t1 - t0, t2 - t0);
			float sqrArea = Mathf.Pow(areaNormal.magnitude, 2);

			return new Vector3(
				Vector3.Dot(areaNormal, Vector3.Cross(t2 - t1, point - t1)) / sqrArea,
				Vector3.Dot(areaNormal, Vector3.Cross(t0 - t2, point - t2)) / sqrArea,
				Vector3.Dot(areaNormal, Vector3.Cross(t1 - t0, point - t0)) / sqrArea);
		}

		Vector3 NearestPointOnMesh(Vector3 worldPosition, out int nearestTriangle)
		{
			Vector3 localPosition = mesh.world2Local.MultiplyPoint(worldPosition);
			Vector3 meshPosition = Vector3.zero;
			float distance = float.MaxValue;
			Triangle triangle = default;

			nearestTriangle = -1;

			for(int t = 0; t < mesh.triangles.Length; t += 3)
			{
				Vector3 closestPoint;
				triangle[0] = mesh.vertices[mesh.triangles[t + 0]];
				triangle[1] = mesh.vertices[mesh.triangles[t + 1]];
				triangle[2] = mesh.vertices[mesh.triangles[t + 2]];
				Vector3 normArea = mesh.normals[t / 3];
				Vector3 planePoint = triangle[0] + Vector3.ProjectOnPlane(localPosition - triangle[0], normArea);
				float sqrArea = normArea.sqrMagnitude;
				Vector3 baryPosition = new Vector3(
					Vector3.Dot(normArea, Vector3.Cross(triangle[2] - triangle[1], planePoint - triangle[1])) / sqrArea,
					Vector3.Dot(normArea, Vector3.Cross(triangle[0] - triangle[2], planePoint - triangle[2])) / sqrArea,
					Vector3.Dot(normArea, Vector3.Cross(triangle[1] - triangle[0], planePoint - triangle[0])) / sqrArea);
				int firstOff = baryPosition.x > 0 ? baryPosition.y > 0 ? baryPosition.z > 0 ? 3 : 2 : 1 : 0;
				float sqrDist;

				if (firstOff < 3)
				{
					Vector3 origin = triangle[(firstOff + 1) % 3];
					Vector3 axis = triangle[(firstOff + 2) % 3] - origin;
					closestPoint = origin + Vector3.ClampMagnitude(axis, Mathf.Max(0, Vector3.Dot(planePoint - origin, axis.normalized)));
				}
				else
					closestPoint = triangle[0] * baryPosition.x + triangle[1] * baryPosition.y + triangle[2] * baryPosition.z;

				sqrDist = (closestPoint - localPosition).sqrMagnitude;

				if (sqrDist < distance && Vector3.Dot(localPosition - planePoint, mesh.normals[t / 3]) > 0 )
				{
					distance = sqrDist;
					meshPosition = closestPoint;
					nearestTriangle = t;
				}
			}

			return meshPosition;
		}

		/// <summary> A* flow to destination </summary>
		void ComputeTriangleFlow(int destTriangle)
		{
			Fill(-1, flow);
			Fill(float.NaN, flowDistance);

			flowDistance[destTriangle] = 0;

			for (int e = 0; e < 3; e++)
			{
				int neighboringTriangle = mesh.neighbors[destTriangle + e];

				if (neighboringTriangle >= 0)
					heads.Enqueue(neighboringTriangle - neighboringTriangle % 3);
			}

			while (heads.Length != 0)
			{
				for (int i = heads.Length; i > 0; i--)
				{
					int h = heads.Dequeue();

					if (flow[h] == -1)
					{
						int nextPoint = -1;
						float nextPointDistance = float.MaxValue;

						for (int e = 0; e < 3; e++)
						{
							int pointNeighbor = mesh.neighbors[h + e];

							if (pointNeighbor != -1)
							{
								int triangleNeighbor = pointNeighbor - pointNeighbor % 3; //convert edge to triangle

								if(!float.IsNaN(flowDistance[triangleNeighbor]))
								{
									float distance = flowDistance[triangleNeighbor] + Vector3.Distance(mesh.centers[h / 3], mesh.centers[pointNeighbor / 3]);

									if (distance < nextPointDistance)
									{
										nextPoint = pointNeighbor;
										nextPointDistance = distance;
									}
								}
								else
									heads.Enqueue(triangleNeighbor);
							}
							// else maybe negative pointers can do special actions?
						}

						flow[h] = nextPoint;
						flowDistance[h] = nextPointDistance;
					}
				}
			}
		}

		static void Fill<T>(T value, NativeArray<T> array) where T : struct
		{
			for (int i = 0; i < array.Length; i++)
				array[i] = value;
		}

		void ComputePath(Vector3 localStart, int startTriangle, Vector3 localEnd)
		{
			Stack<int> bendIndeces = new Stack<int>();
			int index = startTriangle;

			slidePath.Clear();
			slidePath.Add(new PathNode(localStart + mesh.normals[startTriangle / 3].normalized * height, Vector3.zero));
			bendIndeces.Push(0);

			// Follow flow until a dead end is reached
			while (flow[index] != -1)
			{
				int neighbor = flow[index];
				int vL = mesh.triangles[neighbor];
				int vR = mesh.triangles[neighbor - neighbor % 3 + (neighbor + 1) % 3];
				Vector3 heightOffset = mesh.normals[neighbor / 3].normalized * height;
				Vector3 leftPoint = mesh.vertices[vL] + heightOffset;
				Vector3 rightPoint = mesh.vertices[vR] + heightOffset;
				Vector3 bendPoint = slidePath[bendIndeces.Peek()];
				PathNode node = new PathNode(leftPoint, rightPoint - leftPoint);

				// Pull on the path until a bend is found
				if (PathNode.SlideAndCollide(bendPoint, localEnd, ref node))
				{
					for (int j = slidePath.Length - 1; j > 0; j--)
					{
						if (j == bendIndeces.Peek())
							bendIndeces.Pop();

						PathNode jNode = slidePath[j];

						if (PathNode.SlideAndCollide(slidePath[bendIndeces.Peek()], node, ref jNode))
						{
							slidePath[j] = jNode;
							bendIndeces.Push(j);
							break;
						}
						else
							slidePath[j] = jNode;
					}

					bendIndeces.Push(slidePath.Length);
				}

				slidePath.Add(node);

				index = neighbor - neighbor % 3;
			}

			if (pathValid)
			{
				nativePath.Clear();
				nativePath.Add(startPosition);

				foreach (Vector3 sp in slidePath)
				{
					Vector3 worldPoint = mesh.local2World.MultiplyPoint(sp);

					if (worldPoint != nativePath[nativePath.Length - 1])
						nativePath.Add(worldPoint);
				}

				nativePath.Add(mesh.local2World.MultiplyPoint(localEnd + mesh.normals[index / 3].normalized * height));
			}
		}
	}
}
