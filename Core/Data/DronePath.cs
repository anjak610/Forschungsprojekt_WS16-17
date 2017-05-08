﻿using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the drone path. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class DronePath : RenderEntitiy
    {
        #region Fields
        
        private const float LineWidth = 4; // pixel
                
        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private AttributesList _positions = new AttributesList(65000);
        
        private float3 _lastPosition = float3.Zero;
        
        #endregion

        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs (see base class).
        /// </summary>
        public DronePath(RenderContext rc) : base(rc)
        { 
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
        public override void Render()
        {
            base.Render();

            List<DynamicAttributes> buffers = _positions.GetAttributesList();
            for(var i=0; i<buffers.Count; i++)
            {
                _rc.RenderAsLines(buffers[i], LineWidth);
            }
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        public override void SetShaderParams()
        {
            // no uniform variables available. Line width is set in Render().
        }

        /// <summary>
        /// Create shader program with vertex and fragment shader for this render entity.
        /// </summary>
        protected override ShaderProgram CreateShaderProgram()
        {
            string vertsh = AssetStorage.Get<string>("VertexShaderDP.vert");
            string pixsh = AssetStorage.Get<string>("PixelShaderDP.frag");

            return _rc.CreateShader(vertsh, pixsh);
        }

        #endregion
    }
}