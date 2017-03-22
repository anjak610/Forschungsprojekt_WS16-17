using Fusee.Math.Core;
using System.Collections.Generic;

/// <summary>
/// Represents an octree node, which is in fact a voxel respectively a bounding box with some data.
/// Based on: https://github.com/Nition/UnityOctree
/// 
/// </summary>

namespace Fusee.Tutorial.Core
{
    public class OctreeNode<T>
    {
        public float3 Position;
        public float SideLength;
        public T Data;
        public List<OctreeNode<T>> Children;

        /// <summary>
        /// Constructor which requires the center position and the side length of this node's bounding box.
        /// </summary>
        /// <param name="position">The center position of this node's bounding box.</param>
        /// <param name="sideLength">The side length of this node's bounding box.</param>
        public OctreeNode(float3 position, float sideLength)
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
        public bool Add(float3 targetPosition, T data, out List<OctreeNode<T>> addedNodes)
        {
            // node contains position?
            // => YES, initialize voxel beneath
            // => NO, then return false

            addedNodes = null;

            if (!Contains(targetPosition))
                return false;

            SubAdd(targetPosition, data, out addedNodes);
            return true;
        }

        /// <summary>
        /// Private counterpart to Add(). Does recursive calculation.
        /// </summary>
        /// <param name="targetPosition">Random position in three space.</param>
        /// <param name="data">The data to apply to this voxel / octree node.</param>
        /// <param name="addedNodes">if nodes had to be added, they will be stored in here.</param>
        private void SubAdd(float3 targetPosition, T data, out List<OctreeNode<T>> addedNodes)
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

            if (SideLength == Octree<T>.MinSideLength)
            {
                Data = data;
            }
            else
            {
                if (Children != null && Children.Count > 0)
                {
                    bool found = false;

                    foreach(OctreeNode<T> child in Children)
                    {
                        found = child.Add(targetPosition, data, out addedNodes);

                        if (found)
                            return;
                    }
                }
                    
                // create new voxel and let it grow, until this node is its parent

                OctreeNode<T> newNode = Octree<T>.CreateNodeWithMinSideLength(targetPosition);
                newNode.Data = data;

                addedNodes = new List<OctreeNode<T>>();

                while (!IsEqual(newNode))
                {
                    addedNodes.Add(newNode); // important: before Grow()
                    newNode = Octree<T>.Grow(newNode);
                }

                // now they are equal
                AddChild(newNode.Children[0]); // because there must be only one child, otherwise it would have been found before
            }
        }

        /// <summary>
        /// Adds an octree node as a child to this node. Called by Octree.Grow() for example.
        /// </summary>
        /// <param name="child">The octree node to add as a child.</param>
        public void AddChild(OctreeNode<T> childNode)
        {
            if (Children == null)
                Children = new List<OctreeNode<T>>();

            Children.Add(childNode);
        }

        /// <summary>
        /// Checks whether this node contains some children.
        /// </summary>
        /// <returns>True if children exist.</returns>
        public bool hasChildren()
        {
            return Children != null && Children.Count > 0;
        }

        /// <summary>
        /// Checks whether a single point resides in this voxel.
        /// </summary>
        /// <param name="target">The point to examine whether it lies inside the bounds of this octree node.</param>
        /// <returns>True if lies within.</returns>
        public bool Contains(float3 target)
        {
            bool outlier = true;
            
            float diffX = System.Math.Abs(target.x - Position.x);
            float diffY = System.Math.Abs(target.y - Position.y);
            float diffZ = System.Math.Abs(target.z - Position.z);

            float halfVoxelSize = SideLength / 2;

            if (diffX < halfVoxelSize && diffY < halfVoxelSize && diffZ < halfVoxelSize) // point lies inside this voxel
            {
                outlier = false;
            }

            return !outlier;
        }

        /// <summary>
        /// Checks whether another node is equal to this node.
        /// </summary>
        /// <param name="node">The other node.</param>
        /// <returns>True if they are the same.</returns>
        public bool IsEqual(OctreeNode<T> node)
        {
            return node.Position == Position && node.SideLength == SideLength;
        }
    }
}
