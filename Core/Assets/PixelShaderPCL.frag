#ifdef GL_ES
    precision highp float;     
#endif
/*
uniform int depthShading;

varying vec4 newVertex;
varying vec3 n_depthColor;
varying vec4 color;
*/

void main()
{

	//vec3 colorC = vec3(n_depthColor);
	/*		
	vec3 colorA = vec3(0.149,0.141,0.912);
	vec3 colorB = vec3(1.000,0.833,0.224);
	
	float  Pi = 3.1415926;
	float pct = abs(sin(Pi));
	
	//vec3 color = mix(colorA, colorB,pct);
	
	if(depthShading == 1) 
		gl_FragColor = vec4(n_depthColor, 1.0);
	else
		gl_FragColor = vec4(color.x, color.y, color.z, 1.0);
	*/
	vec3 color = vec3(1, 1, 1);
	gl_FragColor = vec4(color.x, color.y, color.z, 1.0);
}