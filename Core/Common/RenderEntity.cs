using Fusee.Engine.Core;

namespace Fusee.Tutorial.Core.Common
{
    /// <summary>
    /// Abstract class which provides the concept of how different render entitites (such as voxels, points or drone paths)
    /// provide the data for the render engine.
    /// </summary>
    public abstract class RenderEntity
    {
        protected RenderContext _rc;
        protected ShaderProgram _shader; // set shader program before rendering
        
        /// <summary>
        /// Set the current render context and the shader related to the according render entity via base constructor.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="shader">A shader program created from fragment and vertex shader via rc.CreateShader(...);</param>
        protected RenderEntity(RenderContext rc)
        {
            _rc = rc;
            _shader = CreateShaderProgram();
            GetShaderParams();
        }

        /// <summary>
        /// This method gets called every frame and should define, how this entity should be rendered.
        /// Make sure to always call base.Render(); before render your stuff.
        /// </summary>
        public virtual void Render()
        {
            _rc.SetShader(_shader);
            SetShaderParams();
        }

        /// <summary>
        /// Should set member variables which store a handle to the uniform variables in the shader.
        /// </summary>
        protected abstract void GetShaderParams();

        /// <summary>
        /// When uniform variables exist in the shader programs, they should be set here.
        /// </summary>
        protected abstract void SetShaderParams();

        /// <summary>
        /// Gets called by the constructor. This method should contain the code in order
        /// to create the shader program via rc.CreateShader(vertsh, pixsh);.
        /// </summary>
        protected abstract ShaderProgram CreateShaderProgram();
    }
}
