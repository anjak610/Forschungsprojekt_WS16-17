using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Octree;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the wireframe (for the octree). 
    /// Render context related programming is encapsulated in this class for better readability.
    /// </summary>
    public class Wireframe : RenderEntitiy
    {
        #region Fields

        private const float LineWidth = 1;
        private const float LineWidthEmphasized = 4;

        private const int MinSize = 8; // the minimum side length of a bounding box to render

        // 0 => smallest bounding box, 1 => second smallest size, e.g. 8 => biggest size etc.
        private int _level = -1; 
        private int _maxLevel = 8;

        private bool _showAllLevels = true;

        private OctreeNode _lastDebuggingNode;
        private DynamicAttributes _debuggingBoundingBox;

        // wireframe to render
        private Dictionary<int, List<DynamicAttributes>> _wireFramePerLevel = new Dictionary<int, List<DynamicAttributes>>();

        // nodes: position and sidelength, per level
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

        /// <summary>
        /// Show only the bounding boxes of a certain level. Step up this level, means larger nodes.
        /// </summary>
        public void LevelUp()
        {
            if(_level >= 0)
            {
                _level--;
                _showAllLevels = false;
            }
            else
            {
                _showAllLevels = true;
            }
        }

        /// <summary>
        /// Show only the bounding boxes of a certain level. Step down this level, means smaller nodes.
        /// </summary>
        public void LevelDown()
        {
            if (_level < _maxLevel && _level < _wireFramePerLevel.Count - 1)
            {
                _level++;
                _showAllLevels = false;
            }
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
        public override void Render()
        {
            base.Render();
            
            foreach(KeyValuePair<int, List<DynamicAttributes>> kvp in _wireFramePerLevel) // for each level
            {
                float lineWidth = LineWidth;

                if (PointCloud.GetCurrentViewMode() == PointCloud.ViewMode.PerLevel && !_showAllLevels && _level == kvp.Key)
                {
                    lineWidth = LineWidthEmphasized;
                    SetLineColor(true);
                }        
                else
                {
                    SetLineColor(false);
                }            

                foreach (DynamicAttributes wireframe in kvp.Value) // for each bounding box
                {
                    _rc.RenderAsLines(wireframe, lineWidth);
                }
            }

            // debugging node octree
            if(PointCloud.GetCurrentViewMode() == PointCloud.ViewMode.PerNode)
            {
                SetLineColor(true);

                if(_lastDebuggingNode == null || !_lastDebuggingNode.IsEqual(PointCloud.GetCurrentDebuggingNode()))
                {
                    _lastDebuggingNode = PointCloud.GetCurrentDebuggingNode();
                    _debuggingBoundingBox = CreateBoundingBox(_lastDebuggingNode.SideLength, _lastDebuggingNode.CenterPosition);
                }

                _rc.RenderAsLines(_debuggingBoundingBox, LineWidthEmphasized);
            }
        }

        #endregion

        /// <summary>
        /// Gets called when bounding box updates. Creates a new bounding box for this node.
        /// </summary>
        private void OnNewOctreeNodeAdded(OctreeNode node)
        {
            if(node.SideLength >= MinSize)
            {
                int level = node.GetLevel();
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
