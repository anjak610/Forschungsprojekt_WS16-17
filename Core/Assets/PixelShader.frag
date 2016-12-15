#ifdef GL_ES
    precision highp float;     
#endif

varying float dist;
varying vec3 modelpos;
varying vec3 normal;
uniform vec2 screenSize;
varying vec2 UV;

//varying vec2 texCoords;

uniform sampler2D tex;
varying vec2 texcoord;


void main()
{
	
	float width=1920.0;
	float height=1080.0;
	
	float _dist = 0.5 * dist + 0.5;
	
	vec3 color = texture2D(tex,UV).rgba;

	vec3 colorA = texture2D(tex,UV).a;

	gl_FragColor =  vec4(color,1.0)* vec4(colorA,0.5); //change last value to set trancparancy

	 if (gl_FragColor.a < 0.5)        
    {
       discard; // yes: discard this fragment
    }

	// map from interval [-1, 1] to [0, 1]
	//vec3 color = vec3(1, 0.3, 1);
	//vec3 color = vec3(0.5, 1, 0.3);
	//vec4 alpha = texture2D(texture,texcoord).aaaa;
	//gl_FragColor = texture(tex,texCoords);

	//gl_FragColor = vec4(((gl_FragCoord.x)/screenSize.x), (gl_FragCoord.y/screenSize.y), (1.5-dist) *  outputcolor.z,  1);
	//gl_FragColor = vec4(((gl_FragCoord.x)/width), (gl_FragCoord.y/height), (1.5-dist),  1);
	//gl_FragColor = vec4( (1.5-dist) *  color , 1);
}
