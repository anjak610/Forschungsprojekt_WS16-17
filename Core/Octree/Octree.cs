﻿ using Fusee.Math.Core;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents an octree where a point cloud is stored.
/// Based on: https://github.com/Nition/UnityOctree and 
/// Schuetz (2016): Potree: Rendering Large Point Clouds in Web Browsers
/// 
/// usage:
/// Octree octree = new Octree(float3 centerPosition, int bucketThreshold = 8);
/// octree.Add(float3 position)
/// 
/// octree.OnNodeAddedCallback += (OctreeNode node) => { // do something }; 
/// </summary>

namespace Fusee.Tutorial.Core.Octree
{
    public class Octree
    {
        public delegate void OnNewNodeAdded(OctreeNode node);
        public static OnNewNodeAdded OnNodeAddedCallback;
        
        public static int BucketThreshold; // The maximum number of items one node can hold
        public static float3 CenterPosition;

        private OctreeNode _root; // root contains the eight cubes around the center point as children.

        /// <summary>
        /// Initializes the octree with given center point and minimum side length.
        /// </summary>
        /// <param name="position">The ultimate center point.</param>
        /// <param name="initialSideLength">The side length of the root node. May be changed because of growing of the tree.</param>
        /// <param name="bucketThreshold">The maximum number of items one node can hold</param>
        public Octree(float3 position, float initialSideLength = 2, int bucketThreshold = 8) {
            
            BucketThreshold = bucketThreshold;
            CenterPosition = position;

            CreateRootNode(initialSideLength);
        }

        /// <summary>
        /// Adds another point to this octree. See also Schuetz (2016) for more details.
        /// </summary>
        /// <param name="position">Position of the point to add.</param>
        public void Add(float3 position)
        {
            int count = 0; // do not grow infinitely

            // start from root node(s)
            // root nodes contain position?
            // => YES, add it to root node
            // => NO, then let octree grow, and check again

            bool found = false;

            while(!found)
            {
                List<OctreeNode> nodesAdded = new List<OctreeNode>();
                found = _root.Add(ref position, out nodesAdded);

                if (nodesAdded != null)
                {
                    foreach (OctreeNode nodeAdded in nodesAdded)
                    {
                        OnNodeAddedCallback?.Invoke(nodeAdded);
                    }
                }

                if(!found)
                {
                    _root.SideLength = _root.SideLength * 2;

                    for(var i=0; i<8; i++)
                    {
                        OctreeNode childNode = _root.Children[i];
                        OctreeNode parentNode = Grow(childNode);
                        _root.Children[i] = parentNode;

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
        /// <param name="centerPosition">The center position of the root node.</param>
        /// <param name="sideLength">The side length of the root node (initially). May be gets changed later.</param>
        private void CreateRootNode(float sideLength)
        {
            _root = new OctreeNode(CenterPosition, sideLength);
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

                position *= sideLength / 4;

                OctreeNode node = new OctreeNode(CenterPosition + position, sideLength / 2);
                _root.AddChild(node);

                OnNodeAddedCallback?.Invoke(node);
            }
        }
        
        /// <summary>
        /// Takes an octree node and creates its parent by letting it grow around the center point.
        /// </summary>
        /// <param name="node">The octree node for which to create the parent</param>
        /// <returns>Returns the parent octree node.</returns>
        public OctreeNode Grow(OctreeNode node)
        {
            OctreeNode parentNode = CreateNodeWithSideLength(node.CenterPosition, node.SideLength * 2);
            parentNode.AddChild(node);

            return parentNode;
        }

        /// <summary>
        /// Creates a new octree node with given side length at the requested position.
        /// </summary>
        /// <param name="targetPosition">Position in three space, which resides in the requested node/bounding box.</param>
        /// <param name="sideLength">The length of the bounding box of the required octree node.</param>
        /// <returns>Returns the octree node with specified side length.</returns>
        public static OctreeNode CreateNodeWithSideLength(float3 targetPosition, float sideLength)
        {
            // first, set target position relative to origin of world space in relation to CenterPosition.
            targetPosition -= CenterPosition;

            // second, compute voxel
            // what is the next voxel position in the overall grid?
            // => multiple of voxel size

            int divisionX = (int) System.Math.Floor(targetPosition.x / sideLength);
            int divisionY = (int) System.Math.Floor(targetPosition.y / sideLength);
            int divisionZ = (int) System.Math.Floor(targetPosition.z / sideLength);

            float3 newPos = new float3(divisionX, divisionY, divisionZ) * sideLength; // lower corner of bounding box in x-, y- and z-direction
            newPos += float3.One * sideLength / 2; // center

            // third, add back the center position again, such that it keeps its origin.
            newPos += CenterPosition;

            OctreeNode node = new OctreeNode(newPos, sideLength);

            return node;
        }
    }
}
