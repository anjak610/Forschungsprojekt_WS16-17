using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fusee.Tutorial.Core.Octree
{
    /// <summary>
    /// Takes the octree and provides a list of nodes ordered in screen-projected-size related to current camera perspective.
    /// </summary>

    class OctreeRenderer
    {
        public Action OnTraversalStartedCallbacks; // gets called every time a new traversal starts
        public Action<List<OctreeNode>> OnTraversalFinishedCallbacks; // gets called every time a traversal has finished => parameter is a list of visible nodes
        public Action<OctreeNode> OnTraversalVisibleNodeFoundCallbacks; // gets called every time a traversal has found a visible node
        public Action<OctreeNode> OnTraversalNonVisibleNodeFoundCallbacks; // gets called every time a traversal has found a non-visible node
        
        private const int POINT_BUDGET = 100000; // number of points that are visible at one frame, tradeoff between performance and quality
        private const float MIN_SCREEN_PROJECTED_SIZE = 5; // minimum screen size of the node

        private Octree _octree;
        private RenderCanvas _renderCanvas;

        private SortedDictionary<double, OctreeNode> _nodesOrderedByProjectionSize = new SortedDictionary<double, OctreeNode>(); // nodes ordered by screen-projected-size
        private List<OctreeNode> _visibleNodes;

        private int _visitPointCounter;

        private bool _traversalFinished = true;
        private bool _scheduleNewTraversal = false;

        private float4x4 _nextMVP;
        private Frustum _currentViewFrustum;
        private float3 _cameraPosition;

        private double _slope = System.Math.Tan((3.141592f * 0.25f) / 2); // fov / 2

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="octree">The octree which nodes should be traversed.</param>
        public OctreeRenderer(RenderCanvas renderCanvas, Octree octree)
        {
            _renderCanvas = renderCanvas;
            _octree = octree;
        }

        /// <summary>
        /// Tells the octree renderer to start a new octree traversal.
        /// </summary>
        /// <param name="modelViewProjection">The current model view projection to calculate the view frustum from.</param>
        /// <param name="cameraPosition">The camera position for distance computation.</param>
        public void ScheduleNewTraversal(float4x4 modelViewProjection, float3 cameraPosition)
        {
            if (_nextMVP != modelViewProjection)
            {
                _nextMVP = modelViewProjection;
                _cameraPosition = cameraPosition;
            }   

            _scheduleNewTraversal = true;
        }

        /// <summary>
        /// Call this function in order to check for new traversal schedules.
        /// </summary>
        public void OnRenderCycle()
        {
            if (_scheduleNewTraversal)
                StartNewTraversal();
        }

        /// <summary>
        /// Tries to start a new traversal. If not a traversal is currently running, it starts a new one.
        /// </summary>
        private void StartNewTraversal()
        {
            if(!_traversalFinished)
                return;

            _traversalFinished = false;
            _scheduleNewTraversal = false;

            Task task = new Task(() =>
            {
                Diagnostics.Log("Start Traversal");

                _visitPointCounter = 0;
                _visibleNodes = new List<OctreeNode>();

                _nodesOrderedByProjectionSize = new SortedDictionary<double, OctreeNode>();
                _currentViewFrustum = new Frustum(_nextMVP);

                OnTraversalStartedCallbacks?.Invoke();

                TraverseByProjectionSizeOrder();
                
                OnTraversalFinishedCallbacks?.Invoke(_visibleNodes);
                _traversalFinished = true;

                Diagnostics.Log("Finished Traversal");
            });

            task.Start();
        }

        /// <summary>
        /// Traverses the octree and searches for nodes in screen-projected-size order.
        /// </summary>
        private void TraverseByProjectionSizeOrder()
        {
            ProcessNode(_octree.GetRootNode());

            while (_nodesOrderedByProjectionSize.Count > 0 && _visitPointCounter < POINT_BUDGET)
            {
                // choose the nodes with the biggest screen size overall to process next

                KeyValuePair<double, OctreeNode> biggestNode = _nodesOrderedByProjectionSize.Last();
                _nodesOrderedByProjectionSize.Remove(biggestNode.Key);

                ProcessNode(biggestNode.Value);
            }
        }

        /// <summary>
        /// Sub function which calculates the screen-projected-size and adds it to the heap of nodesOrdered by pss.
        /// And call its callback (OnNodeVisited);
        /// </summary>
        /// <param name="node">The node to compute the pss for.</param>
        private void ProcessNode(OctreeNode node)
        {
            // process the node
            OnNodeVisited(node);

            // add child nodes to the heap of ordered nodes

            if (node.hasChildren())
            {
                foreach (OctreeNode childNode in node.Children)
                {
                    // compute screen projected size

                    var distance = float3.Subtract(childNode.CenterPosition, _cameraPosition).Length;
                    var projectedSize = _renderCanvas.Height / 2 * childNode.SideLength / (_slope * distance);

                    // is it below minimum or outside the view frustum => cancel

                    if (projectedSize < MIN_SCREEN_PROJECTED_SIZE || !_currentViewFrustum.sphereinfrustum(childNode.CenterPosition, childNode.SideLength))
                    {
                        OnTraversalNonVisibleNodeFoundCallbacks?.Invoke(childNode);
                        continue;
                    }

                    if (!_nodesOrderedByProjectionSize.ContainsKey(projectedSize)) // by chance two same nodes have the same screen-projected-size; it's such a pitty we can't add it (because it's not allowed to have the same key twice)
                    {
                        _nodesOrderedByProjectionSize.Add(projectedSize, childNode);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets called each time a node is visited by the octree traversal in StartCollectingNodes().
        /// </summary>
        /// <param name="node">The node currently visited.</param>
        private void OnNodeVisited(OctreeNode node)
        {
            _visitPointCounter += node.Bucket.Count;
            _visibleNodes.Add(node);

            OnTraversalVisibleNodeFoundCallbacks?.Invoke(node);
        }
    }
}
