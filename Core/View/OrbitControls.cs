using Fusee.Math.Core;

namespace Fusee.Tutorial.Core.View
{
    /// <summary>
    /// Copied from the javascript framework threeJS to C#.
    /// </summary>
    class OrbitControls
    {
        // "target" sets the location of focus, where the object orbits around
        private float3 _target = float3.Zero;

        // position of the camera relative to the target
        private float3 _position = new float3(0, 0, -5.0f);

        // How far you can dolly in and out ( PerspectiveCamera only )
	    private float _minDistance = 0;
        private float _maxDistance = float.PositiveInfinity;

        // How far you can orbit vertically, upper and lower limits.
	    // Range is 0 to Math.PI radians.
	    private float _minPolarAngle = 0; // radians
	    private float _maxPolarAngle = (float) System.Math.PI; // radians

        // Set to true to enable damping (inertia)
	    // If damping is enabled, you must call controls.update() in your animation loop
	    private bool _enableDamping = true;
	    private float _dampingFactor = 0.25f;

        // Set to false to disable use of the keys
	    private bool _enableKeys = true;

        // for reset
        private float3 _target0;
	    private float3 _position0;

        /// <summary>
        /// Constructor which sets the initial position etc.
        /// </summary>
        public OrbitControls( float3 cameraPos )
        {
            _target0 = _target;
            _position0 = cameraPos;
        }
    }
}
