using Fusee.Engine.Core;

namespace Fusee.Tutorial.Core.Common
{
    /// <summary>
    /// Interface which provides the concept of how different render entitites (such as voxels, points or drone paths)
    /// provide the data for the render engine.
    /// </summary>
    public interface IRenderEntitiy
    {
        // Shader is accessed when app decides which render entity to render.
        ShaderProgram Shader { get; } // create shader program via constructor

        /// <summary>
        /// This method gets called every frame and should define, how this entity should be rendered.
        /// </summary>
        void Render();

        /// <summary>
        /// When uniform variables exist in the shader programs, they should be set here.
        /// </summary>
        void SetShaderParams();
    }
}
