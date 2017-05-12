attribute vec3 fuVertex;
attribute vec3 fuNormal;

uniform mat4 FUSEE_MV;
uniform mat4 FUSEE_MVP;
uniform vec2 particleSize;
/*
uniform vec2 zBounds;
uniform float zZoom;

varying float near;
varying float far;
varying vec4 newVertex;
varying float zoom;
        */
void main()
{
	vec3 modelpos = fuVertex;
	/*
	near = zBounds.x;
	far = zBounds.y;	
	zoom = zZoom;
		*/
	vec4 newVertex = FUSEE_MVP * vec4(modelpos, 1.0);	
	vec4 result = newVertex + vec4(fuNormal.xy * particleSize, 0, 0);
	
	gl_Position = result;
}