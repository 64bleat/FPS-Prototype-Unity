using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class OctTree<T>
    {
        public OctTreeNode<T> root;
        public int maxDepth = 32;

        public OctTree()
        {
            root = new OctTreeNode<T>();
        }

        public void Clear()
        {
            root = new OctTreeNode<T>();
        }

        public T Get(Vector3Int octTreePosition)
        {
            return root.GetRecursive(octTreePosition, maxDepth);
        }

        public void Add(Vector3Int octTreePosition, T value)
        {
            root.AddRecursive(value, octTreePosition, maxDepth, 0);
        }
    }

    public class OctTreeNode<T>
    {
        public T value;

        private OctTreeNode<T> n0, n1, n2, n3, n4, n5, n6, n7, parent;
        //private int occupied = 0;

        public OctTreeNode()
        {
        }

        public OctTreeNode(OctTreeNode<T> parent)
        {
            this.parent = parent;
        }

        public OctTreeNode<T> this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return n0;
                    case 1: return n1;
                    case 2: return n2;
                    case 3: return n3;
                    case 4: return n4;
                    case 5: return n5;
                    case 6: return n6;
                    case 7: return n7;
                    default: throw new IndexOutOfRangeException();
                }
            }

            set
            {
                switch (i)
                {
                    case 0: n0 = value; break;
                    case 1: n1 = value; break;
                    case 2: n2 = value; break;
                    case 3: n3 = value; break;
                    case 4: n4 = value; break;
                    case 5: n5 = value; break;
                    case 6: n6 = value; break;
                    case 7: n7 = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }

        }

        public T GetRecursive(Vector3Int octTreePosition, int depth)
        {
            if (depth <= 0)
                return value;

            depth--;
            int index = GetIndex(octTreePosition, depth);

            if (this[index] != default)
                return this[index].GetRecursive(octTreePosition, depth);
            else
                throw new NullReferenceException($"Tree does not branch to index {index} depth {depth}");
        }

        public void Add(T value, Vector3Int treePosition, int depth, int targetDepth)
        {
            OctTreeNode<T> cursor = this;

            while(depth != targetDepth)
            {
                for(int i = 0; i < 8; i++)
                    cursor[i] ??= new OctTreeNode<T>(this);

                int nextDepth = depth - 1;
                int index = GetIndex(treePosition, nextDepth);
                cursor[index] ??= new OctTreeNode<T>(this);
                cursor = cursor[index];
                depth = nextDepth;
            }
                
            cursor.value = value;
        }
        public void AddRecursive(T value, Vector3Int treePosition, int depth, int targetDepth)
        {
            if (depth == targetDepth)
            {
                this.value = value;
                return;
            }



            int nextDepth = depth - 1;
            int index = GetIndex(treePosition, nextDepth);
            bool isNew = this[index] == null;

            // Direct Neighbors
            for (int i = 0; i < 8; i++)
                this[i] ??= new OctTreeNode<T>(this);

            this[index] ??= new OctTreeNode<T>(this);
            this[index].AddRecursive(value, treePosition, nextDepth, targetDepth);

            //if(isNew)
            //{
            //    int px, py, pz;
            //    Vector3Int tPos = treePosition;
            //    Vector3Int nPos = new Vector3Int(
            //         Neighbor(tPos.x, depth, out px),
            //         Neighbor(tPos.y, depth, out py),
            //         Neighbor(tPos.z, depth, out pz));

            //    // Get Greatest Common Parent
            //    int parentDepth = Mathf.Max(px, Mathf.Max(py, pz));
            //    var parent = this;
            //    int cursor = depth;

            //    while(parent.parent != null && cursor < parentDepth)
            //    {
            //        parent = parent.parent;
            //        cursor++;
            //    }

            //    if (parent != null)
            //    {
            //        parent.AddRecursive(value, new Vector3Int(nPos.x, nPos.y, nPos.z), parentDepth, depth);
            //        //parent.AddRecursive(value, new Vector3Int(tPos.x, nPos.y, tPos.z), parentDepth, depth);
            //        //parent.AddRecursive(value, new Vector3Int(tPos.x, tPos.y, nPos.z), parentDepth, depth);
            //        //parent.AddRecursive(value, new Vector3Int(nPos.x, nPos.y, tPos.z), parentDepth, depth);
            //        //parent.AddRecursive(value, new Vector3Int(tPos.x, nPos.y, nPos.z), parentDepth, depth);
            //        //parent.AddRecursive(value, new Vector3Int(nPos.x, tPos.y, nPos.z), parentDepth, depth);
            //        //parent.AddRecursive(value, new Vector3Int(nPos.x, nPos.y, nPos.z), parentDepth, depth);
            //    }

            //}
        }

        private static int GetIndex(Vector3Int treePosition, int depth)
        {
            return (((treePosition.x >> depth) & 1) << 0)
                 | (((treePosition.y >> depth) & 1) << 1)
                 | (((treePosition.z >> depth) & 1) << 2);
        }

        private static int LeftNeighbor(int n, int depth = 0)
        {
            return n ^ (1 << depth);
        }

        private static int Neighbor(int n, int currentDepth, out int parentDepth)
        {
            n >>= currentDepth;
            int depth = 1;
            int first = n & 1;

            while (depth < 32 && ((n & (1 << depth)) >> depth) == first)
                depth++;

            parentDepth = depth + currentDepth;

            while (depth >= 0)
            {
                n ^= 1 << depth;
                depth--;
            }

            n <<= currentDepth;


            return n;
        }
    }
}
