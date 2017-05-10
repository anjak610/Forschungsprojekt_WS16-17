using Fusee.Math.Core;
using System.Collections.Generic;

/// <summary>
/// Represents an octree node, which is in fact a voxel respectively a bounding box with some data.
/// Based on: https://github.com/Nition/UnityOctree
/// 
/// </summary>

namespace Fusee.Tutorial.Core.Octree
{
    public class OctreeNodeVSP<T>
    {
        public float3 Position;
        public float SideLength;
        public T Data;
        public OctreeNodeVSP<T>[] Children;

        /// <summary>
        /// Constructor which requires the center position and the side length of this node's bounding box.
        /// </summary>
        /// <param name="position">The center position of this node's bounding box.</param>
        /// <param name="sideLength">The side length of this node's bounding box.</param>
        public OctreeNodeVSP(float3 position, float sideLength)
        {
            this.Position = position;
            this.SideLength = sideLength;
        }

        /// <summary>
        /// Adds a voxel with minimum sideLength beneath this octree node.
        /// </summary>
        /// <param name="targetPosition">Random position in three space.</param>
        /// <param name="data">The data to apply to this voxel / octree node.</param>
        /// <param name="addedNodes">if nodes had to be added, they will be stored in here.</param>
        /// <returns>Returns false, when target position does not lie inside this voxel.</returns>
        public bool Add(ref float3 targetPosition, ref T data, out List<OctreeNodeVSP<T>> addedNodes)
        {
            // node contains position?
            // => YES, initialize voxel beneath
            // => NO, then return false

            addedNodes = null;

            if (!Contains(targetPosition))
                return false;

            SubAdd(ref targetPosition, ref data, out addedNodes);
            return true;
        }

        /// <summary>
        /// Private counterpart to Add(). Does recursive calculation.
        /// </summary>
        /// <param name="targetPosition">Random position in three space.</param>
        /// <param name="data">The data to apply to this voxel / octree node.</param>
        /// <param name="addedNodes">if nodes had to be added, they will be stored in here.</param>
        private void SubAdd(ref float3 targetPosition, ref T data, out List<OctreeNodeVSP<T>> addedNodes)
        {
            // does it have sideLength == 2?
            // => YES, then change data
            // => NO, does it have children?
            //    => YES, then examine children
            //       child1 contains position?
            //       => YES, does it have sideLength == 2?
            //          => YES, then change data
            //          => NO, then procede with children
            //       => NO, then procede with other children
            //    => NO, then create voxel, and let it grow until this node is its parent

            addedNodes = null;

            if (SideLength == OctreeVSP<T>.MinSideLength)
            {
                Data = data;
            }
            else
            {
                if (Children != null && Children.Length > 0)
                {
                    bool found = false;

                    foreach (OctreeNodeVSP<T> child in Children)
                    {
                        found = child.Add(ref targetPosition, ref data, out addedNodes);

                        if (found)
                            return;
                    }
                }

                // create new subvoxels until minimum side length is reached

                OctreeNodeVSP<T> currentNode = this;
                addedNodes = new List<OctreeNodeVSP<T>>();

                while (currentNode.SideLength > OctreeVSP<T>.MinSideLength)
                {
                    OctreeNodeVSP<T> newNode = OctreeVSP<T>.CreateNodeWithSideLength(targetPosition, currentNode.SideLength / 2);
                    currentNode.AddChild(newNode);

                    currentNode = newNode;
                    addedNodes.Add(currentNode);
                }

                // now current node is the node with minimum side length
                currentNode.Data = data;
            }
        }

        /// <summary>
        /// Adds an octree node as a child to this node. Called by Octree.Grow() for example.
        /// </summary>
        /// <param name="child">The octree node to add as a child.</param>
        public void AddChild(OctreeNodeVSP<T> childNode)
        {
            if (Children == null)
                Children = new OctreeNodeVSP<T>[0];

            List<OctreeNodeVSP<T>> childList = new List<OctreeNodeVSP<T>>();

            for (var i = 0; i < Children.Length; i++)
            {
                childList.Add(Children[i]);
            }

            childList.Add(childNode);

            Children = childList.ToArray();
        }

        /// <summary>
        /// Checks whether this node contains some children.
        /// </summary>
        /// <returns>True if children exist.</returns>
        public bool hasChildren()
        {
            return Children != null && Children.Length > 0;
        }

        /// <summary>
        /// Checks whether a single point resides in this voxel. The border, where points still are accepted, finds itself on the lower values
        /// of the bounding box, see Octree.CreateNodeWithSideLength().
        /// </summary>
        /// <param name="target">The point to examine whether it lies inside the bounds of this octree node.</param>
        /// <returns>True if lies within.</returns>
        public bool Contains(float3 target)
        {
            bool insideX = false, insideY = false, insideZ = false;
            float halfVoxelSize = SideLength / 2;

            float diffX = target.x - Position.x;
            float diffY = target.y - Position.y;
            float diffZ = target.z - Position.z;

            insideX = diffX < 0 ? System.Math.Abs(diffX) <= halfVoxelSize : diffX < halfVoxelSize;
            insideY = diffY < 0 ? System.Math.Abs(diffY) <= halfVoxelSize : diffY < halfVoxelSize;
            insideZ = diffZ < 0 ? System.Math.Abs(diffZ) <= halfVoxelSize : diffZ < halfVoxelSize;

            return insideX && insideY && insideZ;
        }

        /// <summary>
        /// Checks whether another node is equal to this node.
        /// </summary>
        /// <param name="node">The other node.</param>
        /// <returns>True if they are the same.</returns>
        public bool IsEqual(OctreeNodeVSP<T> node)
        {
            return node.Position == Position && node.SideLength == SideLength;
        }
    }
}