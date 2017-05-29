#ifdef GL_ES
    precision highp float;     
#endif


varying vec4 newVertex;
varying vec3 n_depthColor;


void main()
{
			

	gl_FragColor = vec4(n_depthColor, 1.0);

	
	
}