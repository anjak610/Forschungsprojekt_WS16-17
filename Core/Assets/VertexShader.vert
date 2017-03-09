attribute vec3 fuVertex;
attribute vec3 fuNormal;
uniform vec2 particleSize;
//uniform mat4 xForm;
uniform mat4 FUSEE_MVP;
uniform mat4 FUSEE_MV;
uniform mat4 FUSEE_P;
uniform mat4 FUSEE_ITMV;
 varying vec3 viewpos;
varying vec3 normal;
        
void main()
{
	
	//normal = normalize(mat3(FUSEE_ITMV) * fuNormal);
	//viewpos = (FUSEE_MV*vec4(fuVertex, 1.0)).xyz;
	vec4 newVertex = FUSEE_MVP * vec4(fuVertex, 1.0);
	vec4 result = newVertex  + vec4(fuNormal.xy * particleSize, 0, 0);
   	
	gl_Position =  result;//;FUSEE_MVP* vec4(fuVertex, 1.0);

	//vec4 newVertex = xForm * vec4(fuVertex, 1.0);
	//vec4 result = newVertex  + vec4(fuNormal.xy * particleSize, 0, 0);
	//
	//gl_Position = result;


}