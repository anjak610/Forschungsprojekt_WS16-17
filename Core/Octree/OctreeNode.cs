using Fusee.Math.Core;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an octree node, which is in fact a voxel respectively a bounding box with some data.
/// Based on: https://github.com/Nition/UnityOctree and 
/// Schuetz (2016): Potree: Rendering Large Point Clouds in Web Browsers 
/// </summary>

namespace Fusee.Tutorial.Core.Octree
{
    public class OctreeNode
    {
        public readonly float3 CenterPosition;
        public float SideLength;
        public readonly float Spacing;
        public readonly List<float3> Bucket; // where data/points gets stored

        public OctreeNode[] Children;
        
        /// <summary>
        /// Constructor which sets the initial settings.
        /// </summary>
        /// <param name="centerPos">The center position of this node's bounding box.</param>
        /// <param name="sideLength">The side length of this node's bounding box.</param>
        public OctreeNode(float3 centerPos, float sideLength)
        {
            CenterPosition = centerPos;
            SideLength = sideLength;

            Spacing = sideLength / 128; // see Schuetz (2016), 3.3
            Bucket = new List<float3>();
        }

        /// <summary>
        /// Adds another point to this node.
        /// </summary>
        /// <param name="position">Position of the point to add.</param>
        /// <param name="addedNodes">if nodes had to be added, they will be stored in here.</param>
        /// <returns>Returns false, when target position does not lie inside this voxel.</returns>
        public bool Add(ref float3 position, out List<OctreeNode> addedNodes)
        {
            // node contains position?
            // => YES, add point to this node
            // => NO, then return false

            addedNodes = null;

            if (!Contains(position))
                return false;

            SubAdd(ref position, out addedNodes);
            return true;
        }

        /// <summary>
        /// Private counterpart to Add(). Does recursive calculation.
        /// </summary>
        /// <param name="position">Position of the point to add.</param>
        /// <param name="addedNodes">if nodes had to be added, they will be stored in here.</param>
        private void SubAdd(ref float3 position, out List<OctreeNode> addedNodes)
        {
            addedNodes = null;

            if(hasChildren()) // inner node
            {
                // check whether requested position is above minimum distance with other points
                bool keepsMinDistance = true;

                foreach(float3 point in Bucket)
                {
                    if(float3.Subtract(position, point).Length < Spacing)
                    {
                        keepsMinDistance = false;
                        break;
                    }
                }

                if(keepsMinDistance) // keep point
                {
                    if (Bucket.Count + 1 > Octree.BucketThreshold) // threshold reached
                    {
                        // add position to corresponding child node

                        bool found = false;

                        foreach (OctreeNode child in Children)
                        {
                            found = child.Add(ref position, out addedNodes);

                            if (found)
                                break;
                        }

                        if(!found) // create new child node
                        {
                            CreateChildNode(ref position, out addedNodes);
                        }
                    }
                    else // threshold not reached => add point to this node
                    {
                        Bucket.Add(position);
                    }
                }
                else // pass point to child node(s)
                {
                    // add position to corresponding child node

                    bool found = false;

                    foreach (OctreeNode child in Children)
                    {
                        found = child.Add(ref position, out addedNodes);

                        if (found)
                            break;
                    }

                    if (!found) // create new child node
                    {
                        CreateChildNode(ref position, out addedNodes);
                    }
                }
            } 
            else // leaf node
            {
                if(Bucket.Count + 1 > Octree.BucketThreshold) // threshold reached
                {
                    // become inner node by creating a child node for position
                    CreateChildNode(ref position, out addedNodes);

                    for (var i=0; i<Bucket.Count; i++)
                    {
                        float3 point = Bucket[i];
                        Bucket.RemoveAt(i);

                        SubAdd(ref point, out addedNodes);
                    }
                }
                else // threshold not reached => keep point
                {
                    Bucket.Add(position);
                }
            }
        }

        /// <summary>
        /// Creates a new child node containing specified position.
        /// </summary>
        /// <param name="position">The position the child node should contain.</param>
        /// <param name="addedNodes">if nodes had to be added (e.g. the child node), they will be stored in here.</param>
        private void CreateChildNode(ref float3 position, out List<OctreeNode> addedNodes)
        {
            OctreeNode childNode = Octree.CreateNodeWithSideLength(position, SideLength / 2);
            childNode.Add(ref position, out addedNodes);
            AddChild(childNode);

            if(addedNodes == null)
                addedNodes = new List<OctreeNode>();

            addedNodes.Add(childNode);
        }

        /// <summary>
        /// Adds an octree node as a child to this node. Called by Octree.Grow() for example.
        /// </summary>
        /// <param name="child">The octree node to add as a child.</param>
        public void AddChild(OctreeNode childNode)
        {
            if (Children == null)
                Children = new OctreeNode[0];

            List<OctreeNode> childList = new List<OctreeNode>();

            for(var i=0; i<Children.Length; i++)
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

            float diffX = target.x - CenterPosition.x;
            float diffY = target.y - CenterPosition.y;
            float diffZ = target.z - CenterPosition.z;
            
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
        public bool IsEqual(OctreeNode node)
        {
            return node.CenterPosition == CenterPosition && node.SideLength == SideLength;
        }
    }
}
