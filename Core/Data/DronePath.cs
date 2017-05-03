using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the drone path. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class DronePath
    {
        #region Fields

        #region Constants

        private const float LineWidth = 2f;

        #endregion

        #region Shader Params

        private RenderContext _rc;

        public readonly ShaderProgram Shader;
        private IShaderParam _lineWidthParam;

        #endregion

        #region Data

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private AttributesList _positions = new AttributesList(65000);

        #endregion

        #region Other

        private float3 _lastPosition = float3.Zero;

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs.
        /// </summary>
        public DronePath(RenderContext rc)
        {
            _rc = rc;

            string vertsh = AssetStorage.Get<string>("VertexShaderDP.vert");
            string pixsh = AssetStorage.Get<string>("PixelShaderDP.frag");

            Shader = _rc.CreateShader(vertsh, pixsh);
        }

        /// <summary>
        /// Adds another point to the drone path.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPosition(float3 position)
        {
            if (position == _lastPosition)
                return;

            _positions.AddAttribute(position);
            _lastPosition = position;
        }

        /// <summary>
        /// Gets called every frame. Takes care of rendering the drone path.
        /// </summary>
        public void Render()
        {
            List<DynamicAttributes> buffers = _positions.GetAttributesList();
            for(var i=0; i<buffers.Count; i++)
            {
                _rc.RenderAsLines(buffers[i], LineWidth);
            }
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public void SetShaderParams()
        {
            /*
            _particleSizeParam = _rc.GetShaderParam(Shader, "particleSize");
            _rc.SetShaderParam(_particleSizeParam, ParticleSize);
            //*/
        }

        #endregion
    }
}
