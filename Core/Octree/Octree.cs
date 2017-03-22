 using Fusee.Math.Core;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents an octree where a voxel respectively an octree node contains some data.
/// Based on: https://github.com/Nition/UnityOctree
/// 
/// usage:
/// Octree<T> octree = new Octree<T>(float3 centerPosition, float minSideLength);
/// octree.Add(float3 position, T data)
/// 
/// octree.OnNodeAddedCallback += (OctreeNode<T> node) => { // do something }; 
/// </summary>

namespace Fusee.Tutorial.Core
{
    public enum OctreeNodeStates
    {
        Unknown, Occupied, Free
    }

    public class Octree<T>
    {
        public delegate void OnNewNodeAdded(OctreeNode<T> node);
        public OnNewNodeAdded OnNodeAddedCallback;

        public static float MinSideLength; // minimum side length
        public static float3 CenterPosition;

        private OctreeNode<T> _root; // root is not really a bounding box, it just contains the eight cube voxels around the center point as children.

        /// <summary>
        /// Initializes the octree with given center point and minimum side length.
        /// </summary>
        /// <param name="position">The ultimate center point.</param>
        /// <param name="sideLength">The minimum side length a voxel / octree node should have.</param>
        public Octree(float3 position, float sideLength) {

            CenterPosition = position;
            MinSideLength = sideLength;

            CreateRootNode();
        }

        /// <summary>
        /// Initializes the voxel with minimum side length positioned at the given position. If already existent, only the data will be transferred and overwritten.
        /// </summary>
        /// <param name="targetPosition">Random position in three space.</param>
        /// <param name="data">The data to apply to this voxel / octree node.</param>
        public void Add(float3 targetPosition, T data)
        {
            int count = 0; // do not grow infinitely

            // start from root node(s)
            // root nodes contain position?
            // => YES, intialize voxel beneath
            // => NO, then grow, and check again

            bool found = false;

            while(!found)
            {
                foreach (OctreeNode<T> child in _root.Children)
                {
                    List<OctreeNode<T>> nodesAdded = new List<OctreeNode<T>>();
                    found = child.Add(ref targetPosition, ref data, out nodesAdded);
                    
                    if(nodesAdded != null)
                    {
                        foreach (OctreeNode<T> nodeAdded in nodesAdded)
                        {
                            OnNodeAddedCallback?.Invoke(nodeAdded);
                        }
                    }

                    if (found)
                        break;
                }

                if(!found)
                {
                    _root.SideLength = _root.SideLength * 2;

                    for(var i=0; i<8; i++)
                    {
                        OctreeNode<T> childNode = _root.Children[i];
                        childNode = Grow(childNode);
                        _root.Children[i] = childNode;

                        OnNodeAddedCallback?.Invoke(childNode);
                    }

                    if (++count > 20)
                    {
                        // Error!
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the root node. Root is not really a bounding box, it just contains the eight cube voxels around the center point as children.
        /// </summary>
        private void CreateRootNode()
        {
            _root = new OctreeNode<T>(CenterPosition, MinSideLength * 2);
            OnNodeAddedCallback?.Invoke(_root);

            for (var i=0; i<8; i++)
            {
                float3 position = float3.Zero;

                switch(i)
                {
                    case 0:
                        position = new float3(1, 1, 1);
                        break;
                    case 1:
                        position = new float3(1, 1, -1);
                        break;
                    case 2:
                        position = new float3(1, -1, 1);
                        break;
                    case 3:
                        position = new float3(1, -1, -1);
                        break;
                    case 4:
                        position = new float3(-1, 1, 1);
                        break;
                    case 5:
                        position = new float3(-1, 1, -1);
                        break;
                    case 6:
                        position = new float3(-1, -1, 1);
                        break;
                    case 7:
                        position = new float3(-1, -1, -1);
                        break;
                }

                OctreeNode<T> node = new OctreeNode<T>(CenterPosition + position, MinSideLength);
                _root.AddChild(node);

                OnNodeAddedCallback?.Invoke(node);
            }
        }
        
        /// <summary>
        /// Takes an octree node and creates its parent by letting it grow around the center point.
        /// </summary>
        /// <param name="node">The octree node for which to create the parent</param>
        /// <returns>Returns the parent octree node.</returns>
        public static OctreeNode<T> Grow(OctreeNode<T> node)
        {
            OctreeNode<T> parentNode = CreateNodeWithSideLength(node.Position, node.SideLength * 2);
            parentNode.AddChild(node);

            return parentNode;
        }
        
        /// <summary>
        /// Creates a new octree node with minimum side length at the requested position.
        /// </summary>
        /// <param name="targetPosition">Position in three space, which resides in this voxel.</param>
        /// <returns>Returns the octree node with minimum side length.</returns>
        public static OctreeNode<T> CreateNodeWithMinSideLength(float3 targetPosition)
        {
            return CreateNodeWithSideLength(targetPosition, MinSideLength);
        }

        /// <summary>
        /// Creates a new octree node with given side length at the requested position.
        /// </summary>
        /// <param name="targetPosition">Position in three space, which resides in this voxel.</param>
        /// <param name="sideLength">The length of the bounding box of the required octree node.</param>
        /// <returns>Returns the octree node with specified side length.</returns>
        public static OctreeNode<T> CreateNodeWithSideLength(float3 targetPosition, float sideLength)
        {
            // first, set target position relative to origin of world space in relation to CenterPosition.
            targetPosition -= CenterPosition;

            // second, compute voxel
            // what is the next voxel position in the overall grid?
            // => multiple of voxel size

            int divisionX = (int)System.Math.Floor(targetPosition.x / sideLength);
            int divisionY = (int)System.Math.Floor(targetPosition.y / sideLength);
            int divisionZ = (int)System.Math.Floor(targetPosition.z / sideLength);

            float3 newPos = new float3(divisionX, divisionY, divisionZ) * sideLength; // lower corner of bounding box in x-, y- and z-direction
            newPos += float3.One * sideLength / 2; // center

            // third, add back the center position again, such that it keeps its origin.
            newPos += CenterPosition;

            OctreeNode<T> node = new OctreeNode<T>(newPos, sideLength);

            return node;
        }
    }
}
