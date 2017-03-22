#ifdef GL_ES
	precision highp float;
#endif

varying vec3 modelpos;
varying vec3 normal;

uniform vec2 yScale;
uniform vec3 voxelPos;

vec3 hsv2rgb(in vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    vec3 result = c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);

	return result;
}

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

		// albedo depends on y-Position, what is the scale? [minY, maxY] => [0, 1] (Hue)
		float minY = yScale.x;
		float maxY = yScale.y;
		
		float hue = ( voxelPos.y - minY ) / ( maxY - minY );
		vec3 albedo = hsv2rgb( vec3( hue, 1, 1 ) ); 

		gl_FragColor = vec4(intensity * albedo, 1);
	}
}