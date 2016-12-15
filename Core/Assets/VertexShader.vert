//#version 120

attribute vec3 fuVertex;
attribute vec3 fuNormal;
attribute vec2 fuUV;
varying vec2 UV;
varying vec3 modelpos;
uniform vec2 particleSize;
uniform mat4 xForm;
varying vec3 normal;
varying float dist; // distance

//varying vec2 texCoords;
        
void main()
{
	vec4 newVertex = xForm * vec4(fuVertex, 1.0);

	dist = newVertex.z/newVertex.w;
	modelpos = fuVertex; 
	normal = fuNormal;

	UV = fuUV;

	vec4 result = newVertex  + vec4(fuNormal.xy* particleSize, 0, 0);

	gl_Position = result;
}