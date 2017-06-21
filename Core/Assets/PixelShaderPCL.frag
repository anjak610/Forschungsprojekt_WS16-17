#ifdef GL_ES
    precision highp float;     
#endif

uniform int depthShading;

varying vec3 n_depthColor;
varying vec3 color;

void main()
{	
	if(depthShading == 1) 
		gl_FragColor = vec4(n_depthColor, 1.0);
	else
		gl_FragColor = vec4(color.x, color.y, color.z, 1.0);
}