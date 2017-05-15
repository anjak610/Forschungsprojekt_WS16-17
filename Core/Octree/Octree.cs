using Fusee.Base.Core;
using Fusee.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public enum OctreeNodeStates
    {
        Visible, VisibleButUnloaded, NonVisible
    }

    public class Octree
    {
        // number of points that must have been added until to search for nodes of which bucket have been changed
        private const int PASSED_POINTS_UNTIL_SEARCH_FOR_BUCKET_CHANGE = 1000;

        private long _pointCounter = 0;

        public delegate void OnNewNodeAdded(OctreeNode node);

        public static OnNewNodeAdded OnNodeAddedCallback;
        public static Action OnOctreeNodeLevelsChangedCallback; // callback for when one or several nodes change their levels. See Wireframe
        public static Action<OctreeNode> OnNodeBucketChangedCallback;
        
        public static int BucketThreshold; // The maximum number of items one node can hold
        public static float3 CenterPosition;

        private OctreeNode _root; // root contains the eight cubes around the center point as children.

        /// <summary>
        /// Initializes the octree with given center point and minimum side length.
        /// </summary>
        /// <param name="position">The ultimate center point.</param>
        /// <param name="bucketThreshold">The maximum number of items one node can hold</param>
        public Octree(float3 position, int bucketThreshold = 8) {
            
            BucketThreshold = bucketThreshold;
            CenterPosition = position;
        }

        /// <summary>
        /// Starts creating the octree. Couldn't do it in the constructor, otherwise some classes would miss the callbacks.
        /// </summary>
        /// <param name="initialSideLength">The side length of the root node. May be changed because of growing of the tree.</param>
        public void Init(float initialSideLength = 2)
        {
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
            bool hasGrown = false;

            while (!found)
            {
                found = _root.Add(ref position);
                
                if(!found)
                {
                    _root.SideLength = _root.SideLength * 2;

                    for(var i=0; i<8; i++)
                    {
                        OctreeNode childNode = _root.Children[i];
                        OctreeNode parentNode = Grow(childNode);

                        _root.Children[i] = parentNode; // replace childNode

                        parentNode.SetParent(_root);
                        parentNode.AddChild(childNode, true);

                        OnNodeAddedCallback?.Invoke(parentNode);
                    }

                    hasGrown = true;

                    if (++count > 20)
                    {
                        // Error!
                        return;
                    }
                }
            }

            if(hasGrown)
            {
                OnOctreeNodeLevelsChangedCallback?.Invoke();
            }

            _pointCounter++;

            if(_pointCounter % PASSED_POINTS_UNTIL_SEARCH_FOR_BUCKET_CHANGE == 0)
            {
                //SearchForNodesWithChangedBuckets();
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
            _root.Path = new byte[] { 0 };

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
        /// BUT: Parent and Paths and children have not been set yet.
        /// </summary>
        /// <param name="node">The octree node for which to create the parent</param>
        /// <returns>Returns the parent octree node.</returns>
        public OctreeNode Grow(OctreeNode node)
        {
            OctreeNode newNode = CreateNodeWithSideLength(node.CenterPosition, node.SideLength * 2);
            return newNode;
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

            if(sideLength == 64)
            {
                Diagnostics.Log("yuchey!");
            }

            return node;
        }

        /// <summary>
        /// Returns the root node.
        /// </summary>
        public OctreeNode GetRootNode()
        {
            return _root;
        }

        /// <summary>
        /// Searches for nodes of which buckets have been changed since the last time this function was called.
        /// </summary>
        private void SearchForNodesWithChangedBuckets()
        {
            Traverse((OctreeNode node) =>
            {
                if (node.HasBucketChanged)
                {
                    node.HasBucketChanged = false;
                    OnNodeBucketChangedCallback?.Invoke(node);
                }   
            });
        }

        /// <summary>
        /// Traverses the octree, visits each node and calls the callback.
        /// </summary>
        /// <param name="callback">The function to call for each node.</param>
        public void Traverse(Action<OctreeNode> callback)
        {
            RecTraverse(_root, callback);
        }

        /// <summary>
        /// Traverses the octree, visits each node and calls the callback.
        /// </summary>
        /// <param name="startNode">The node to start from.</param>
        /// <param name="callback">The function to call for each node.</param>
        public static void Traverse(OctreeNode startNode, Action<OctreeNode> callback)
        {
            RecTraverse(startNode, callback);
        }

        /// <summary>
        /// Traverse function, where a custom traverse functionality can be defined (without callback).
        /// </summary>
        /// <param name="traverseFn">A function, which takes as input a node, and then decides which child nodes to traverse further.</param>
        public void TraverseWithoutCallback(Action<OctreeNode> traverseFn)
        {
            traverseFn(_root);
        }

        /// <summary>
        /// Traverse function, where a custom traverse functionality can be defined.
        /// </summary>
        /// <param name="traverseFn">A function, which takes as input a node, and then decides which child nodes to traverse further.</param>
        /// <param name="callback">Simple callback, which gets called for each visited node by traverseFn.</param>
        public void Traverse(Action<OctreeNode, Action<OctreeNode>> traverseFn, Action<OctreeNode> callback)
        {
            traverseFn(_root, callback);
        }

        /// <summary>
        /// Recursive function, which takes one node, and visits all children and so on.
        /// For each node, a callback gets called.
        /// </summary>
        private static void RecTraverse(OctreeNode node, Action<OctreeNode> callback)
        {
            callback(node);

            if(node.hasChildren())
            {
                foreach(OctreeNode child in node.Children)
                {
                    RecTraverse(child, callback);
                }
            }
        }
    }
}
