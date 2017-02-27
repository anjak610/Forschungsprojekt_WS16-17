#ifdef GL_ES
    precision highp float;     
#endif

varying float dist;



void main()
{
	//vec3 color = vec3(1, 0.3, 1);
	//gl_FragColor = vec4(color,1.0);
	//float width=1920.0;
	//float height=1080.0;
	vec3 color = vec3(0.5, 1, 0.3);
	float _dist = 0.5 * dist + 0.5;

	//float z = (gl_FragCoord.z / gl_FragCoord.w)/100;

	gl_FragColor = vec4(_dist,0,0,1);
	//gl_FragColor = vec4((1/z), 0, 0,  1);
}
