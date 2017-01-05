#ifdef GL_ES
    precision highp float;     
#endif
//#version 120

varying float dist;
varying vec3 modelpos;
varying vec3 normal;
//varying vec2 screenSize;
varying vec2 UV;

uniform sampler2D tex;
uniform vec2 camerarange;
 
varying vec2 texCoord;
 
void main()
{	


	//float _dist = 0.5 * dist + 0.5;
/*if (texture2D(tex,UV).z <= nearestZ)
{
  nearestZ = texture2D(tex,UV).z;
}*/
    float ColorMin = 0;
	float ColorMax = 1;
    //float depthColorValue =texture2D(tex,UV).z / gl_FragCoord.w; //sollte nicht von w abhängig sein sondern vom am nächsten liegenden z
	
	float nearZ = 1000;

    float colorR = texture2D(tex,UV).r;
	float colorG = texture2D(tex,UV).g;
    float colorB = texture2D(tex,UV).b;

	//float color = texture2D(tex,UV).rgb;
	float colorA = texture2D(tex,UV).a;

colorB = 0;
colorR = 0; 
colorG = 0;


//float ndcDepth= (2.0 * gl_FragCoord.z - Min - Max) /   (Max - Min);
//float clipDepth = ndcDepth / gl_FragCoord.w;
 
float z = (gl_FragCoord.w / gl_FragCoord.z)*30; // 1-(gl_FragCoord.z / gl_FragCoord.w)/ 80; 

float b = (ColorMax - ColorMin)* pow(z,0.2); 
//colorG = 1/clipDepth;
//gl_FragColor = vec4((clipDepth / 0.5) + 0.5); 
	//
 //gl_FragColor =  vec4(b, b ,b,colorA);
 gl_FragColor = vec4 (z,z,z,colorA);
 //gl_FragColor = vec4((clipDepth * 0.5) + 0.5); 

	if (gl_FragColor.a < 0.5)        
    {
       discard; // yes: discard this fragment
	   //gl_FragColor.a = 0.0;
    }



//gl_FragColor = vec4((clipDepth * 0.5) + 0.5,colorA); 
}

