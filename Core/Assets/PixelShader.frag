#ifdef GL_ES
    precision highp float;     
#endif

varying float dist;
varying vec3 modelpos;
varying vec3 normal;
uniform vec2 screenSize;

void main()
{
	vec3 color = vec3(1, 0.3, 1);
	//vec3 color = vec3(1, 1, 1);
	// map from interval [-1, 1] to [0, 1]
	float _dist = 0.5 * dist + 0.5;
	//gl_FragColor = vec4(((gl_FragCoord.x)/screenSize.x), (gl_FragCoord.y/screenSize.y), (1.5-dist) *  color.z,  1);
	gl_FragColor = vec4( (1.5-dist) *  color , 1);
}