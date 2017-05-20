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
        public byte[] Path;
        public RenderFlag RenderFlag = RenderFlag.NonVisible;
        
        public bool HasBucketChanged = false; // some kind of flag whether points have been added to or removed from this node

        public OctreeNode Parent;
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

            Spacing = sideLength / 4; // see Schuetz (2016), 3.3
            Bucket = new List<float3>();
        }

        /// <summary>
        /// Adds another point to this node.
        /// </summary>
        /// <param name="position">Position of the point to add.</param>
        /// <returns>Returns false, when target position does not lie inside this voxel.</returns>
        public bool Add(ref float3 position)
        {
            // node contains position?
            // => YES, add point to this node
            // => NO, then return false

            if (!Contains(position))
                return false;
            
            SubAdd(ref position);
            return true;
        }

        /// <summary>
        /// Private counterpart to Add(). Does recursive calculation.
        /// </summary>
        /// <param name="position">Position of the point to add.</param>
        private void SubAdd(ref float3 position)
        {
            if (hasChildren()) // inner node
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
                            found = child.Add(ref position);

                            if (found)
                                break;
                        }

                        if(!found) // create new child node
                        {
                            CreateChildNode(ref position);
                        }
                    }
                    else // threshold not reached => add point to this node
                    {
                        Bucket.Add(position);
                        HasBucketChanged = true;
                    }
                }
                else // pass point to child node(s)
                {
                    // add position to corresponding child node

                    bool found = false;

                    foreach (OctreeNode child in Children)
                    {
                        found = child.Add(ref position);

                        if (found)
                            break;
                    }

                    if (!found) // create new child node
                    {
                        CreateChildNode(ref position);
                    }
                }
            } 
            else // leaf node
            {
                if(Bucket.Count + 1 > Octree.BucketThreshold) // threshold reached
                {
                    // become inner node by creating a child node for position
                    CreateChildNode(ref position);

                    float3[] pointsToPass = new float3[Bucket.Count];
                    Bucket.CopyTo(pointsToPass);
                    Bucket.Clear();

                    for (var i=0; i<pointsToPass.Length; i++)
                    {                        
                        SubAdd(ref pointsToPass[i]);
                    }

                    HasBucketChanged = true;
                }
                else // threshold not reached => keep point
                {
                    Bucket.Add(position);
                    HasBucketChanged = true;
                }
            }
        }

        /// <summary>
        /// Creates a new child node containing specified position.
        /// </summary>
        /// <param name="position">The position the child node should contain.</param>
        private void CreateChildNode(ref float3 position)
        {
            OctreeNode childNode = Octree.CreateNodeWithSideLength(position, SideLength / 2);
            AddChild(childNode);
            
            childNode.Add(ref position);

            Octree.OnNodeAddedCallback?.Invoke(childNode);
        }

        /// <summary>
        /// Adds an octree node as a child to this node. Called by Octree.Grow() for example.
        /// </summary>
        /// <param name="child">The octree node to add as a child.</param>
        /// <param name="recomputePath">wether to recompute the path for all children underneath.</param>
        public void AddChild(OctreeNode childNode, bool recomputePath = false)
        {
            if (Children == null)
                Children = new OctreeNode[0];

            List<OctreeNode> childList = Children.ToList();
            childList.Add(childNode);

            Children = childList.ToArray();

            childNode.SetParent(this);

            if (recomputePath && Children != null && Children.Length > 0)
            {
                foreach(OctreeNode child in Children)
                {
                    Octree.Traverse(child, (OctreeNode node) =>
                    {
                        node.SetParent(node.Parent); // causes the node to recompute its path
                    });
                }
            }
        }

        /// <summary>
        /// Sets the given node as its parent and computes the path.
        /// Be careful, because it does not recompute the paths of children.
        /// </summary>
        public void SetParent(OctreeNode parentNode)
        {
            Parent = parentNode;
            SetPath();
        }

        /// <summary>
        /// Sets the path for this node according to its parent node.
        /// BUT: not for all children underneath.
        /// </summary>
        private void SetPath()
        {
            int index = -1;
            for(var i=0; i<Parent.Children.Length; i++)
            {
                OctreeNode childNode = Parent.Children[i];

                if (childNode.IsEqual(this))
                {
                    index = i;
                    break;
                }
            }

            if(index != -1)
            {
                List<byte> byteArray = Parent.Path.ToList();
                byteArray.Add((byte) index); 
                Path = byteArray.ToArray();
            }
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
        /// Returns at which level this node resides in. 0 for root, 1 for his children and so on.
        /// </summary>
        public int GetLevel()
        {
            return Path.Length - 1;
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
