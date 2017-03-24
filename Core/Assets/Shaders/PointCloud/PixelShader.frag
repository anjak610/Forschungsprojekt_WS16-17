#ifdef GL_ES
    precision highp float;     
#endif

void main()
{
	vec3 color = vec3(0.5, 0.5, 1);
	gl_FragColor = vec4(color, 1);
}