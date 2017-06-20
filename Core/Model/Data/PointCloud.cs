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
    public class PointCloud : RenderEntity
    {
        #region Fields
        
        private const int UPDATE_EVERY = 1000; // every xth point the point cloud should update its meshes
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation
        
        private static float _particleSize = 0.05f; // maybe gets changed from platform specific classes
        
        private int _pointCounter = 0;
        private sbyte _currentEchoId = -1; // render all dots, not only a single echo id

        private float _aspectRatio = 1; // needed for particle size to be square, aspect ratio of viewport => see OnWindowResize

        private float2 _zBounds = float2.Zero;

        private float3 n_cloudCenterWorld;
        private float n_cloudRadius;
        private static float _zoom = 60f;

        private IShaderParam _particleSizeParam;

        private IShaderParam n_cloudCenterWorldParam;
        private IShaderParam n_cloudRadiusParam;

        private int depthShading = 1; // acts as a bool
        private IShaderParam _shadingModeParam;

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        //private Dictionary<byte, StaticMeshList> _meshLists = new Dictionary<byte, StaticMeshList>();

        private PointMesh _pointMesh = new PointMesh(float3.Zero, 0.5f);
        private AttributesList _points = new AttributesList(65536);
        
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
            //_particleSizeParam = _rc.GetShaderParam(_shader, "particleSize");
            /*
            n_cloudCenterWorldParam = _rc.GetShaderParam(_shader, "n_cloudCenterWorld");
            n_cloudRadiusParam = _rc.GetShaderParam(_shader, "n_cloudRadius");
            _shadingModeParam = _rc.GetShaderParam(_shader, "depthShading");
            */
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        protected override void SetShaderParams()
        {
            //_rc.SetShaderParam(_particleSizeParam, new float2(_particleSize, _particleSize * _aspectRatio));

            /*
            // SetZNearFarPlane
            _zBounds = new float2(_zBounds.x, _zBounds.y);

            //Debug.WriteLine("zBounds" + _zBounds);

            //Debug.WriteLine("zBounds" + _zBounds);
            //Debug.WriteLine("Max z" + _boundingBox.GetMaxValues().z);

            _rc.SetShaderParam(n_cloudCenterWorldParam,n_cloudCenterWorld);
            _rc.SetShaderParam(n_cloudRadiusParam, n_cloudRadius);

            _rc.SetShaderParam(_shadingModeParam, depthShading);
            */
        }

        #endregion

        #region Setter and Getter of Fields

        /// <summary>
        /// Whether to render points in depth shading mode or by intensity.
        /// </summary>
        public void SetShadingMode(ShadingMode mode)
        {
            if (mode == ShadingMode.Depth_Map)
                depthShading = 1;
            else
                depthShading = 0;
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
        /// <param name="particleSize">The particle size to set.</param>
        public void SetParticleSize(float particleSize)
        {
            _particleSize = particleSize;
        }

        /// <summary>
        /// Sets the zoom value.
        /// </summary>
        public static void SetZoomValue(float zoom)
        {
           _zoom = zoom;
        }

        /// <summary>
        /// Gets called when viewport changes. Sets the aspect ratio
        /// </summary>
        /// <param name="aspectRatio">width / height</param>
        public void SetAspectRatio(float aspectRatio)
        {
            _aspectRatio = aspectRatio;
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

            _points.AddAttribute(point.Position);

            /*
            PointMesh pointMesh = new PointMesh(point.Position, point.Intensity);

            if (_meshLists.ContainsKey(point.EchoId))
            {
                StaticMeshList meshList = _meshLists[point.EchoId];
                meshList.AddMesh(pointMesh);
            }
            else
            {
                StaticMeshList meshList = new StaticMeshList();
                meshList.AddMesh(pointMesh);

                _meshLists.Add(point.EchoId, meshList);
            }
            
            //*
            if (_pointCounter % UPDATE_EVERY == 0)
            {
                foreach(KeyValuePair<byte, StaticMeshList> kvp in _meshLists)
                {
                    kvp.Value.Apply();
                }
            }
            //*/
        }

        /// <summary>
        /// Adds another point to this point cloud.
        /// </summary>
        /// <param name="position">The position to add.</param>
        public void AddPoint(float3 position)
        {
            /*
            _pointCounter++;

            if (_pointCounter % COMPUTE_EVERY != 0 && COMPUTE_EVERY != 1)
                return;

            _meshList.AddMesh(new PointMesh(position));

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

            List<DynamicAttributes> buffers = _points.GetAttributesList();
            foreach(DynamicAttributes buffer in buffers)
            {
                _rc.RenderAsPoints(buffer);
            }

            /*
            foreach(KeyValuePair<byte, StaticMeshList> kvp in _meshLists)
            {
                List<Mesh> meshesToRemove = kvp.Value.GetMeshesToRemove();
                for (var i = 0; i < meshesToRemove.Count; i++)
                {
                    _rc.Remove(meshesToRemove[i]);
                }
            }

            foreach (KeyValuePair<byte, StaticMeshList> kvp in _meshLists)
            {
                if (_currentEchoId != -1 && _currentEchoId != kvp.Key)
                    continue;

                List<Mesh> meshes = kvp.Value.GetMeshes();
                for (var i = 0; i < meshes.Count; i++)
                {
                    _rc.Render(meshes[i]);
                }
            }
            */
        }

        #endregion

        #region Event Handler
        
        /// <summary>
        /// Gets called when bounding box updates.
        /// </summary>
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _zBounds.x = boundingBox.GetMinValues().z;
            _zBounds.y = boundingBox.GetMaxValues().z;

            n_cloudCenterWorld = boundingBox.GetCenterPoint();
            n_cloudRadius = boundingBox.GetRadius();
        }

        /// <summary>
        /// Gets called when a text file is successfully loaded.
        /// </summary>
        public void OnAssetLoaded()
        {
            /*
            foreach (KeyValuePair<byte, StaticMeshList> kvp in _meshLists)
            {
                kvp.Value.Bake();
            }
            */
        }

        #endregion

        #endregion
    }
}
