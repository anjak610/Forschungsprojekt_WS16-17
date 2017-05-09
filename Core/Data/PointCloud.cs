using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the point cloud. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class PointCloud : RenderEntitiy
    {
        #region Fields
        
        private const int UPDATE_EVERY = 1000; // every xth point the point cloud should update its meshes
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation

        private static float _particleSize = 0.05f; // maybe gets changed from platform specific classes
        public const float ParticleSizeInterval = 0.025f;
        
        private int _pointCounter = 0;

        private IShaderParam _particleSizeParam;

        private IShaderParam _zBoundsParam;
        private float2 _zBounds = float2.Zero;

        private IShaderParam _zZoomParam;
        private float _zoom = 60f;

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private StaticMeshList _meshList = new StaticMeshList();

        #endregion
        
        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public PointCloud(RenderContext rc, BoundingBox boundingBox) : base(rc)
        {
            boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;
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
        /// Sets the shader params for the point cloud.
        /// </summary>
        public override void SetShaderParams()
        {
            _particleSizeParam = _rc.GetShaderParam(_shader, "particleSize");
            _rc.SetShaderParam(_particleSizeParam, _particleSize);

            // SetZNearFarPlane
            _zBoundsParam = _rc.GetShaderParam(_shader, "zBounds");
            _rc.SetShaderParam(_zBoundsParam, _zBounds);

            // SetZoomValue
            _zZoomParam = _rc.GetShaderParam(_shader, "zZoom");
            _rc.SetShaderParam(_zZoomParam, _zoom);
        }

        #endregion

        #region Setter and Getter of Fields

        /// <summary>
        /// Increases the particle size.
        /// </summary>
        /// <param name="interval">How much the particle size should increase.</param>
        public static void IncreaseParticleSize(float interval = ParticleSizeInterval)
        {
            _particleSize += interval;
        }

        /// <summary>
        /// Decreases the particle size.
        /// </summary>
        /// <param name="interval">How much the particle size should decrease.</param>
        public static void DecreaseParticleSize(float interval = ParticleSizeInterval)
        {
            _particleSize -= interval;
        }

        /// <summary>
        /// Sets directly the particle size.
        /// </summary>
        /// <param name="particleSize">The particle size to set.</param>
        public static void SetParticleSize(float particleSize)
        {
            _particleSize = particleSize;
        }

        /// <summary>
        /// Sets the zoom value.
        /// </summary>
        public void SetZoomValue(float zoom)
        {
            _zoom = zoom;
        }

        /// <summary>
        /// Adds another point to this point cloud.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(Common.Point point)
        {
            _pointCounter++;

            if (_pointCounter % COMPUTE_EVERY != 0 && COMPUTE_EVERY != 1)
                return;

            _meshList.AddMesh(new PointMesh(point.Position));

            //*
            if (_pointCounter % UPDATE_EVERY == 0)
            {
                _meshList.Apply();
            }
            //*/
        }

        #endregion

        #region Render

        /// <summary>
        /// Gets called every frame. Takes care of rendering the point cloud.
        /// </summary>
        public override void Render()
        {
            base.Render();

            List<Mesh> meshesToRemove = _meshList.GetMeshesToRemove();
            for (var i = 0; i < meshesToRemove.Count; i++)
            {
                _rc.Remove(meshesToRemove[i]);
            }

            List<Mesh> meshes = _meshList.GetMeshes();
            for (var i = 0; i < meshes.Count; i++)
            {
                _rc.Render(meshes[i]);
            }
        }

        #endregion

        #region Event Handler

        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _zBounds.x = boundingBox.GetMinValues().z;
            _zBounds.y = boundingBox.GetMaxValues().z;
        }

        #endregion

        #endregion
    }
}
