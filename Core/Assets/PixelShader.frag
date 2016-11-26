#ifdef GL_ES
    precision highp float;     
#endif

varying float dist;

void main()
{
	vec3 color = vec3(1, 0.5, 0.8);
	//vec3 color = vec3(1, 1, 1);

	// map from interval [-1, 1] to [0, 1]
	float _dist = 0.5 * dist + 0.5;

	gl_FragColor = vec4( (1.5-dist) *  color, 1);
}