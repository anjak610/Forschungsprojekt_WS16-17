attribute vec3 fuVertex;
attribute vec3 fuNormal;
//attribute vec2 fuUV;
//attribute vec3 fuOffset;

uniform mat4 FUSEE_MV;
uniform mat4 FUSEE_MVP;
uniform vec2 particleSize;

uniform vec2 zBounds;
uniform float zZoom;

varying float near;
varying float far;
varying vec4 newVertex;
//varying float zZoom;
varying float zoom;

//varying vec3 normal;
//varying vec2 uv;
        
void main()
{
	vec3 modelpos = fuVertex;

	near = zBounds.x;
	far = zBounds.y;	
//uv = fuUV;
	zoom = zZoom;
	newVertex = FUSEE_MVP * vec4(modelpos, 1.0);
	//vec4 mvPosition = FUSEE_MV* newVertex;
	vec4 result = newVertex + vec4(fuNormal.xy * particleSize, 0, 0);

	gl_Position = result;
}