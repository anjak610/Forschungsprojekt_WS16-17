using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Octree;
using static Fusee.Tutorial.Core.PointVisualizationBase;

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the wireframe (for the octree). 
    /// Render context related programming is encapsulated in this class for better readability.
    /// </summary>
    public class Wireframe : RenderEntitiy
    {
        #region Fields
        
        // constants / settings

        private const float LINE_WIDTH = 1;
        private const float LINE_WIDTH_EMPHASIZED = 4;

        private const int MAX_NODE_LEVEL = 8; // the maximum level of a node for a bounding box to render
        
        // debugging
        
        private DynamicAttributes _debuggingBoundingBox;

        // data structure

        private Dictionary<int, List<DynamicAttributes>> _wireFramePerLevel = new Dictionary<int, List<DynamicAttributes>>();
        private Octree.Octree _octree;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs and sets some references.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="renderCanvas">Render canvas, PointVisualizationBase</param>
        public Wireframe(RenderContext rc, Octree.Octree octree) : base(rc)
        {
            Octree.Octree.OnNodeAddedCallback += OnNewOctreeNodeAdded;
            Octree.Octree.OnOctreeNodeLevelsChangedCallback += OnOctreeNodeLevelsChanged;

            _octree = octree;
        }

        #region Shader related methods

        /// <summary>
        /// Create shader program with vertex and fragment shader for this render entity.
        /// </summary>
        protected override ShaderProgram CreateShaderProgram()
        {
            string vertsh = AssetStorage.Get<string>("VertexShaderWF.vert");
            string pixsh = AssetStorage.Get<string>("PixelShaderWF.frag");

            return _rc.CreateShader(vertsh, pixsh);
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        public override void SetShaderParams()
        {
            var particleSizeParam = _rc.GetShaderParam(_shader, "color");
            _rc.SetShaderParam(particleSizeParam, new float3(0.33f, 0.33f, 0.33f));
        }

        /// <summary>
        /// Sets a shader param that the current bounding box should be rendered with a red-orange color.
        /// </summary>
        public void SetLineColor(bool active)
        {
            float3 color = active ? new float3(1, 0.5f, 0) : new float3(0.33f, 0.33f, 0.33f);

            var particleSizeParam = _rc.GetShaderParam(_shader, "color");
            _rc.SetShaderParam(particleSizeParam, color);
        }

        #endregion

        #region Render

        /// <summary>
        /// Gets called every frame. Takes care of rendering the point cloud.
        /// </summary>
        /// <param name="viewMode">The view mode currently set.</param>
        /// <param name="level">The level to render. -1 for all levels together.</param>
        /// <param name="debuggingNode">The node to debug for.</param>
        public void Render(ViewModeDebugging viewMode, int level, OctreeNode debuggingNode)
        {
            base.Render();
            
            foreach (KeyValuePair<int, List<DynamicAttributes>> kvp in _wireFramePerLevel) // for each level
            {
                float lineWidth = LINE_WIDTH;

                if ((viewMode == ViewModeDebugging.PerLevel || viewMode == ViewModeDebugging.PerNode) && level != kvp.Key)
                    continue;

                foreach (DynamicAttributes wireframe in kvp.Value) // for each bounding box
                {
                    _rc.RenderAsLines(wireframe, lineWidth);
                }
            }

            // debugging node octree

            if(viewMode == ViewModeDebugging.PerNode)
            {
                SetLineColor(true);
                _rc.RenderAsLines(_debuggingBoundingBox, LINE_WIDTH_EMPHASIZED);
            }
        }

        #endregion

        /// <summary>
        /// When a new debugging node is set, this function is called.
        /// </summary>
        /// <param name="node">The node that is currently debugged.</param>
        public void OnNewDebuggingNode(OctreeNode node)
        {
            if (_debuggingBoundingBox != null)
                _rc.Remove(_debuggingBoundingBox);
            
            _debuggingBoundingBox = CreateBoundingBox(node.SideLength, node.CenterPosition);
        }

        /// <summary>
        /// Gets called when bounding box updates. Creates a new bounding box for this node.
        /// </summary>
        private void OnNewOctreeNodeAdded(OctreeNode node)
        {
            int level = node.GetLevel();

            if (level <= MAX_NODE_LEVEL)
            {
                DynamicAttributes boundingBox = CreateBoundingBox(node.SideLength, node.CenterPosition);

                if (_wireFramePerLevel.ContainsKey(level))
                {
                    List<DynamicAttributes> boundingBoxes = _wireFramePerLevel[level];
                    boundingBoxes.Add(boundingBox);

                    _wireFramePerLevel[level] = boundingBoxes;
                }
                else
                {
                    List<DynamicAttributes> boundingBoxes = new List<DynamicAttributes>();
                    boundingBoxes.Add(boundingBox);

                    _wireFramePerLevel.Add(level, boundingBoxes);
                }
            }
        }

        /// <summary>
        /// Recalculate the whole wireframe.
        /// </summary>
        private void OnOctreeNodeLevelsChanged()
        {
            _wireFramePerLevel = new Dictionary<int, List<DynamicAttributes>>();
            _octree.Traverse(OnNewOctreeNodeAdded);
        }

        /// <summary>
        /// Creates a wireframe for a bounding box, from which all the nodes with the specified size are rendered.
        /// </summary>
        /// <param name="sideLength">The side length of the bounding box.</param>
        /// <param name="position">The center position of the bounding box.</param>
        private DynamicAttributes CreateBoundingBox(float sideLength, float3 position)
        {
            // first, store all vertices of a cube in a list
            List<float3> cubeVertices = new List<float3>();

            for (var i = 0; i < 8; i++)
            {
                float3 vertex = float3.Zero;

                switch (i)
                {
                    case 0:
                        vertex = new float3(0.5f, 0.5f, 0.5f);
                        break;
                    case 1:
                        vertex = new float3(0.5f, 0.5f, -0.5f);
                        break;
                    case 2:
                        vertex = new float3(0.5f, -0.5f, 0.5f);
                        break;
                    case 3:
                        vertex = new float3(0.5f, -0.5f, -0.5f);
                        break;
                    case 4:
                        vertex = new float3(-0.5f, 0.5f, 0.5f);
                        break;
                    case 5:
                        vertex = new float3(-0.5f, 0.5f, -0.5f);
                        break;
                    case 6:
                        vertex = new float3(-0.5f, -0.5f, 0.5f);
                        break;
                    case 7:
                        vertex = new float3(-0.5f, -0.5f, -0.5f);
                        break;
                }

                cubeVertices.Add(position + vertex * sideLength);
            }

            // second, combine these vertices, so that one path is given
            DynamicAttributes boundingBox = new DynamicAttributes(16);

            boundingBox.AddAttribute(cubeVertices[1]);
            boundingBox.AddAttribute(cubeVertices[0]);
            boundingBox.AddAttribute(cubeVertices[2]);
            boundingBox.AddAttribute(cubeVertices[3]);
            boundingBox.AddAttribute(cubeVertices[1]);
            boundingBox.AddAttribute(cubeVertices[5]);
            boundingBox.AddAttribute(cubeVertices[7]);
            boundingBox.AddAttribute(cubeVertices[3]);
            boundingBox.AddAttribute(cubeVertices[7]);
            boundingBox.AddAttribute(cubeVertices[6]);
            boundingBox.AddAttribute(cubeVertices[4]);
            boundingBox.AddAttribute(cubeVertices[5]);
            boundingBox.AddAttribute(cubeVertices[4]);
            boundingBox.AddAttribute(cubeVertices[0]);
            boundingBox.AddAttribute(cubeVertices[2]);
            boundingBox.AddAttribute(cubeVertices[6]);

            return boundingBox;
        }
        
        #endregion
    }
}
