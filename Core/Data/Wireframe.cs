using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Octree;
using Fusee.Engine.Common;

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the wireframe (for the octree). 
    /// Render context related programming is encapsulated in this class for better readability.
    /// </summary>
    public class Wireframe : RenderEntity
    {
        #region Fields
        
        // constants / settings

        private const float LINE_WIDTH = 1;
        private const float LINE_WIDTH_EMPHASIZED = 4;

        private const int MAX_NODE_LEVEL = 8; // the maximum level of a node for a bounding box to render

        // shader

        private IShaderParam _colorParam;

        // debugging
        
        private DynamicAttributes _debugWireframe;
        private List<DynamicAttributes> _snapshotWireframes;

        // data structure

        private Dictionary<byte[], DynamicAttributes> _wireframePerNode = new Dictionary<byte[], DynamicAttributes>();
        
        #endregion

        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs and sets some references.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public Wireframe(RenderContext rc) : base(rc)
        {
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
        /// Should set member variables which store a handle to the uniform variables in the shader.
        /// </summary>
        protected override void GetShaderParams()
        {
            _colorParam = _rc.GetShaderParam(_shader, "color");
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        protected override void SetShaderParams()
        {
            _rc.SetShaderParam(_colorParam, new float3(0.33f, 0.33f, 0.33f));
        }

        /// <summary>
        /// Sets a shader param that the current bounding box should be rendered with a red-orange color.
        /// </summary>
        public void SetLineColor(bool active)
        {
            float3 color = active ? new float3(1, 0.5f, 0) : new float3(0.33f, 0.33f, 0.33f);
            _rc.SetShaderParam(_colorParam, color);
        }

        #endregion

        #region Render

        /// <summary>
        /// Gets called every frame. Takes care of rendering the wireframe of all octree nodes.
        /// </summary>
        public override void Render()
        {
            base.Render();

            foreach (KeyValuePair<byte[], DynamicAttributes> kvp in _wireframePerNode) // for each level
            {
                _rc.RenderAsLines(kvp.Value, LINE_WIDTH);
            }
        }
        
        /// <summary>
        /// Renders the wireframes of the nodes of a specified level.
        /// </summary>
        /// <param name="level">The octree level to render nodes from.</param>
        public void Render(int level)
        {
            base.Render();

            foreach (KeyValuePair<byte[], DynamicAttributes> kvp in _wireframePerNode) // for each level
            {
                if (level != kvp.Key.Length - 1)
                    continue;

                _rc.RenderAsLines(kvp.Value, LINE_WIDTH);
            }
        }

        /// <summary>
        /// Render method that gets called when only a specific node is desired along with a specified octree level.
        /// </summary>
        /// <param name="level">The level of which other nodes should be rendered too.</param>
        /// <param name="debugNode">The node which should be highlighted.</param>
        public void Render(int level, OctreeNode debugNode)
        {
            base.Render();

            Render(level);

            SetLineColor(true);
            _rc.RenderAsLines(_debugWireframe, LINE_WIDTH_EMPHASIZED);
        }

        /// <summary>
        /// Takes the current snapshot and renders it.
        /// </summary>
        public void RenderSnapshot()
        {
            base.Render();
            
            if (_snapshotWireframes != null)
            {
                foreach (DynamicAttributes da in _snapshotWireframes)
                    _rc.RenderAsLines(da, LINE_WIDTH);
            }
        }

        #endregion

        #region Data Model
        
        /// <summary>
        /// Adds a new node to render. If already existent, it gets changed.
        /// </summary>
        public void AddNode(OctreeNode node)
        {
            if (node.GetLevel() > MAX_NODE_LEVEL)
                return;

            if (!_wireframePerNode.ContainsKey(node.Path))
            {
                DynamicAttributes boundingBox = CreateBoundingBox(node.SideLength, node.CenterPosition);
                _wireframePerNode.Add(node.Path, boundingBox);
            }
        }

        /// <summary>
        /// Removes the specified node from the list of rendered nodes.
        /// </summary>
        public void RemoveNode(OctreeNode node)
        {
            if (_wireframePerNode.ContainsKey(node.Path))
            {
                DynamicAttributes da = _wireframePerNode[node.Path];
                _rc.Remove(da);

                _wireframePerNode.Remove(node.Path);
            }
        }

        /// <summary>
        /// Takes the current state and keeps it until its released.
        /// </summary>
        public void TakeSnapshot()
        {
            _snapshotWireframes = new List<DynamicAttributes>();

            foreach (KeyValuePair<byte[], DynamicAttributes> kvp in _wireframePerNode)
            {
                // must have different buffer object id when removing
                DynamicAttributes da = new DynamicAttributes(16);
                da.AddAttributes(kvp.Value.GetOffsets());

                _snapshotWireframes.Add(da);
            }
        }
        
        /// <summary>
        /// Removes the old snapshot.
        /// </summary>
        public void ReleaseSnapshot()
        {
            if (_snapshotWireframes == null)
                return;

            foreach (DynamicAttributes da in _snapshotWireframes)
            {
                _rc.Remove(da);
            }

            _snapshotWireframes = null;
        }

        /// <summary>
        /// When a new debugging node is set, this function is called.
        /// </summary>
        /// <param name="node">The node that is currently debugged.</param>
        public void SetNewDebuggingNode(OctreeNode node)
        {
            if (_debugWireframe != null)
                _rc.Remove(_debugWireframe);

            _debugWireframe = CreateBoundingBox(node.SideLength, node.CenterPosition);
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

        #endregion
    }
}
