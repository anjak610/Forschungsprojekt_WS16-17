using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the point cloud. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class PointCloud
    {
        #region Fields

        #region Constants

        private const int UPDATE_EVERY = 1000; // every xth point the point cloud should update its meshes
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation

        private float ParticleSize = 0.05f; // maybe gets changed from platform specific classes
        public const float ParticleSizeInterval = 0.025f;

        private IShaderParam _zBoundsParam;
        private float2 _zBounds = float2.Zero;

        private IShaderParam _zZoomParam;
        private float _zoom = 60f;

        public IShaderParam _zZoom
        {
            get { return _zBoundsParam; }

            set { _zBoundsParam = value; }
        }

        #endregion

        #region Shader Params

        public readonly string VertexShader, PixelShader;
        private IShaderParam _particleSizeParam;

        #endregion

        #region Data

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private StaticMeshList _meshList = new StaticMeshList();

        #endregion

        #region Other

        public void SetZNearFarPlane(RenderContext rc, ShaderProgram shader)
        {
            _zBoundsParam = rc.GetShaderParam(shader, "zBounds");
            rc.SetShaderParam(_zBoundsParam, _zBounds);
        }

        public void GetZoomValue(RenderContext rc, ShaderProgram shader, float _zoom)
        {
            _zZoomParam = rc.GetShaderParam(shader, "zZoom");
            rc.SetShaderParam(_zZoomParam, _zoom);
        }

        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _zBounds.x = boundingBox.GetMinValues().z;
            _zBounds.y = boundingBox.GetMaxValues().z;
        }
        private int _pointCounter = 0;

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs.
        /// </summary>
        public PointCloud()
        {
            VertexShader = AssetStorage.Get<string>("VertexShaderPCL.vert");
            PixelShader = AssetStorage.Get<string>("PixelShaderPCL.frag");
        }

        /// <summary>
        /// Adds another point to this point cloud.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(Point point)
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

        /// <summary>
        /// Gets called every frame. Takes of rendering the point cloud.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public void Render(RenderContext rc)
        {
            List<Mesh> meshesToRemove = _meshList.GetMeshesToRemove();
            for (var i = 0; i < meshesToRemove.Count; i++)
            {
                rc.Remove(meshesToRemove[i]);
            }

            List<Mesh> meshes = _meshList.GetMeshes();
            for (var i = 0; i < meshes.Count; i++)
            {
                rc.Render(meshes[i]);
            }
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public void SetShaderParams(RenderContext rc, ShaderProgram shader)
        {
            _particleSizeParam = rc.GetShaderParam(shader, "particleSize");
            rc.SetShaderParam(_particleSizeParam, ParticleSize);
        }

        /// <summary>
        /// Increases the particle size.
        /// </summary>
        /// <param name="interval">How much the particle size should increase.</param>
        public void IncreaseParticleSize(float interval = ParticleSizeInterval)
        {
            ParticleSize += interval;
        }

        /// <summary>
        /// Decreases the particle size.
        /// </summary>
        /// <param name="interval">How much the particle size should decrease.</param>
        public void DecreaseParticleSize(float interval = ParticleSizeInterval)
        {
            ParticleSize -= interval;
        }

        /// <summary>
        /// Sets directly the particle size.
        /// </summary>
        /// <param name="particleSize">The particle size to set.</param>
        public void SetParticleSize(float particleSize)
        {
            ParticleSize = particleSize;
        }

        #endregion
    }
}
