#ifdef GL_ES
    precision highp float;     
#endif

void main()
{
	vec3 color = vec3(1, 0.3, 1);
	gl_FragColor = vec4(color,1.0);
}
