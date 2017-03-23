#ifdef GL_ES
	precision highp float;
#endif

varying vec3 modelpos;
varying vec3 normal;

uniform vec3 albedo;

void main()
{		
	if( abs(modelpos.x) + abs(modelpos.y) > 1.975 || abs(modelpos.y) + abs(modelpos.z) > 1.975 || abs(modelpos.x) + abs(modelpos.z) > 1.975)
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