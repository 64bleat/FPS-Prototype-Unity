using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class OctTreeTest : MonoBehaviour
    {
        public Transform[] target;
        public int maxDepth = 4;

        private readonly OctTree<Transform> tree = new OctTree<Transform>();

        private void OnDrawGizmos()
        {
            tree.maxDepth = maxDepth;
            foreach (Transform targetss in target)
            {
                Vector3 tPosition = transform.InverseTransformPoint(targetss.position);
                Vector3Int tPos = new Vector3Int((int)tPosition.x, (int)tPosition.y, (int)tPosition.z);
                Vector3Int nPos = new Vector3Int(
                    Neighbor(tPos.x),
                    Neighbor(tPos.y),
                    Neighbor(tPos.z));

                tree.Add(tPos, targetss);
                //tree.Add(new Vector3Int(nPos.x, tPos.y, tPos.z), target);
                //tree.Add(new Vector3Int(tPos.x, nPos.y, tPos.z), target);
                //tree.Add(new Vector3Int(tPos.x, tPos.y, nPos.z), target);
                //tree.Add(new Vector3Int(nPos.x, nPos.y, tPos.z), target);
                //tree.Add(new Vector3Int(tPos.x, nPos.y, nPos.z), target);
                //tree.Add(new Vector3Int(nPos.x, tPos.y, nPos.z), target);
                //tree.Add(new Vector3Int(nPos.x, nPos.y, nPos.z), target);


                // Draw
                DrawNode(tree.root, new Vector3Int(), 0);
            }

            tree.Clear();
        }

        private void DrawNode(OctTreeNode<Transform> node, Vector3Int offset, int depth)
        {
            int invDepth = tree.maxDepth - depth;
            float width = 1 << invDepth;
            Vector3 gizmoScale = transform.localScale * width;
            Vector3 displayOffset = offset;
            displayOffset.x *= transform.localScale.x;
            displayOffset.y *= transform.localScale.y;
            displayOffset.z *= transform.localScale.z;

            Gizmos.DrawWireCube(transform.position + displayOffset + gizmoScale / 2 , gizmoScale);

            invDepth--;
            for (int i = 0; i < 8; i++)
                if(node[i] != null)
                {
                    Vector3Int newOffset = offset;

                    newOffset.x |= ((i & 1) >> 0) << invDepth;
                    newOffset.y |= ((i & 2) >> 1) << invDepth;
                    newOffset.z |= ((i & 4) >> 2) << invDepth;
                    DrawNode(node[i], newOffset, depth + 1);
                }    
        }

        private static int Neighbor(int n)
        {
            int backDepth = 1;
            int first = n & 1;

            while (backDepth < 30 && ((n & (1 << backDepth)) >> backDepth) == first)
                backDepth++;

            while (backDepth >= 0)
            {
                n ^= 1 << backDepth;
                backDepth--;
            }

            return n;
        }
    }
}