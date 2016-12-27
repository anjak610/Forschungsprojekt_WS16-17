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
    float Min = 0;
	float Max = 20000;
    float depthColorValue =texture2D(tex,UV).z / gl_FragCoord.w; //sollte nicht von w abh�ngig sein sondern vom am n�chsten liegenden z
	
	float nearZ = 1000;

    float colorR = texture2D(tex,UV).r;
	float colorG = texture2D(tex,UV).g;
    float colorB = texture2D(tex,UV).b;

	//float color = texture2D(tex,UV).rgb;
	float colorA = texture2D(tex,UV).a;

/*if(texture2D(tex,UV).z < nearZ)
{
	nearZ = texture2D(tex,UV).z;
}*/

  //float depthColorValue = texture2D(tex,UV).z - nearZ;

colorB = 0;
colorR = 0; 


//float ndcDepth= (2.0 * gl_FragCoord.z - Min - Max) /   (Max - Min);
//float clipDepth = ndcDepth / gl_FragCoord.w;
 
float z = 1.0 - (gl_FragCoord.z / gl_FragCoord.w) /80;
//colorG = 1/clipDepth;
//gl_FragColor = vec4((clipDepth / 0.5) + 0.5); 
	//
 gl_FragColor =  vec4(z, z ,z,colorA);
 //gl_FragColor = vec4((clipDepth * 0.5) + 0.5); 

	if (gl_FragColor.a < 0.5)        
    {
       discard; // yes: discard this fragment
	   //gl_FragColor.a = 0.0;
    }



//gl_FragColor = vec4((clipDepth * 0.5) + 0.5,colorA); 
}

