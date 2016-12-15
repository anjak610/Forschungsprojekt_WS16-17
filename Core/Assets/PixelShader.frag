#ifdef GL_ES
    precision highp float;     
#endif
//#version 120

varying float dist;
varying vec3 modelpos;
varying vec3 normal;
uniform vec2 screenSize;
varying vec2 UV;

uniform sampler2D tex;
varying vec2 texcoord;

void main()
{
	
	float width=1920.0;
	float height=1080.0;
	
	float _dist = 0.5 * dist + 0.5;
	
	vec3 color = texture2D(tex,UV).rgb;

	float colorA = texture2D(tex,UV).a;

	gl_FragColor =  vec4(color,colorA); 

	 if (gl_FragColor.a < 0.5)        
    {
       discard; // yes: discard this fragment
    }
}

