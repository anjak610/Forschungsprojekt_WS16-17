#ifdef GL_ES
	precision highp float;
#endif

varying vec3 modelpos;
varying vec3 normal;

uniform vec3 albedo;

void main()
{
	float intensity = dot(normal, vec3(0, 0, -1));
			
	if( abs(modelpos.x) + abs(modelpos.y) > 1.975 || abs(modelpos.y) + abs(modelpos.z) > 1.975 || abs(modelpos.x) + abs(modelpos.z) > 1.975)
	{
		gl_FragColor = vec4(0, 0, 0, 1);    
	}
	else 
	{
		gl_FragColor = vec4(intensity * albedo, 1);
	}
}