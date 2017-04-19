#ifdef GL_ES
	precision highp float;
#endif

varying vec3 normal;
varying vec2 uv;

varying vec3 albedo;

void main()
{		
	if( uv.x > 0.95 || uv.x < 0.05 || uv.y > 0.95 || uv.x < 0.05)
	{
		gl_FragColor = vec4(0, 0, 0, 1);  // black border    
	}
	else 
	{
		// cube color

		float intensity = dot(normal, vec3(0, 0, -1));
		intensity = 0.5 + intensity * 0.5; // [0, 1] => [0.5, 1]

		gl_FragColor = vec4(intensity * albedo, 1);
	}
}