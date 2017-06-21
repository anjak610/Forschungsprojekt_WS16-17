using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core.RenderEntities
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the point cloud. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class PointCloud : RenderEntity
    {
        #region Fields
        
        private static float _pointSize = 1; // maybe gets changed from platform specific classes
        
        private sbyte _currentEchoId = -1; // render all dots, not only a single echo id
        
        private float3 n_cloudCenterWorld;
        private float n_cloudRadius;
        private static float _zoom = 60f;

        private IShaderParam _pointSizeParam;

        private IShaderParam n_cloudCenterWorldParam;
        private IShaderParam n_cloudRadiusParam;

        private int _depthShading = 1; // acts as a bool
        private IShaderParam _shadingModeParam;

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        //private Dictionary<byte, StaticMeshList> _meshLists = new Dictionary<byte, StaticMeshList>();

        private PointMesh _pointMesh = new PointMesh(float3.Zero, 0.5f);

        private AttributesList _points = new AttributesList(65536);
        private AttributesList _colors = new AttributesList(65536);
        
        #endregion
        
        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public PointCloud(RenderContext rc, BoundingBox boundingBox) : base(rc)
        {
            boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            n_cloudCenterWorld = boundingBox.GetCenterPoint();
            n_cloudRadius = boundingBox.GetRadius();
        }

        #region Shader related methods

        /// <summary>
        /// Create shader program with vertex and fragment shader for this render entity.
        /// </summary>
        protected override ShaderProgram CreateShaderProgram()
        {
            string vertsh = AssetStorage.Get<string>("VertexShaderPCL.vert");
            string pixsh = AssetStorage.Get<string>("PixelShaderPCL.frag");

            return _rc.CreateShader(vertsh, pixsh);
        }

        /// <summary>
        /// When the shader program is created, retrieve once at the beginning the handles for those params.
        /// </summary>
        protected override void GetShaderParams()
        {
            _pointSizeParam = _rc.GetShaderParam(_shader, "pointSize");
            
            n_cloudCenterWorldParam = _rc.GetShaderParam(_shader, "n_cloudCenterWorld");
            n_cloudRadiusParam = _rc.GetShaderParam(_shader, "n_cloudRadius");
            _shadingModeParam = _rc.GetShaderParam(_shader, "depthShading");
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        protected override void SetShaderParams()
        {
            _rc.SetShaderParam(_pointSizeParam, _pointSize);
            
            _rc.SetShaderParam(n_cloudCenterWorldParam,n_cloudCenterWorld);
            _rc.SetShaderParam(n_cloudRadiusParam, n_cloudRadius);

            _rc.SetShaderParam(_shadingModeParam, _depthShading);
        }

        #endregion
        
        #region Render

        /// <summary>
        /// Gets called every frame. Takes care of rendering the point cloud.
        /// </summary>
        public override void Render()
        {
            base.Render();

            List<DynamicAttributes> positions = _points.GetAttributesList();
            List<DynamicAttributes> colors = _colors.GetAttributesList();

            if (positions.Count != colors.Count)
                return;

            for (var i=0; i<positions.Count; i++)
            {
                _rc.RenderAsPoints(positions[i], colors[i]);
            }
        }

        #endregion

        #region Event Handler
        
        /// <summary>
        /// Gets called when bounding box updates.
        /// </summary>
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            n_cloudCenterWorld = boundingBox.GetCenterPoint();
            n_cloudRadius = boundingBox.GetRadius();
        }

        #endregion

        #region Setter and Getter of Fields

        /// <summary>
        /// Whether to render points in depth shading mode or by intensity.
        /// </summary>
        public void SetShadingMode(ShadingMode mode)
        {
            if (mode == ShadingMode.Depth_Map)
                _depthShading = 1;
            else
                _depthShading = 0;
        }

        /// <summary>
        /// Sets the current echo id to render.
        /// </summary>
        public void SetEchoId(sbyte echoId)
        {
            _currentEchoId = echoId;
        }

        /// <summary>
        /// Sets directly the particle size.
        /// </summary>
        /// <param name="pointSize">The particle size to set.</param>
        public void SetPointSize(float pointSize)
        {
            _pointSize = pointSize;
        }

        /// <summary>
        /// Sets the zoom value.
        /// </summary>
        public static void SetZoomValue(float zoom)
        {
            _zoom = zoom;
        }

        /// <summary>
        /// Adds another point to this point cloud.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(Common.Point point)
        {
            _points.AddAttribute(point.Position);
            _colors.AddAttribute(new float3(point.Intensity, point.Intensity, point.Intensity));
        }

        #endregion

        #endregion
    }
}
