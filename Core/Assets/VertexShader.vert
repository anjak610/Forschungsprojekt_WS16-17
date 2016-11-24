attribute vec3 fuVertex;
attribute vec3 fuNormal;
uniform vec2 particleSize;
uniform mat4 xForm;
varying vec3 modelpos;
        
void main()
{
	modelpos = fuVertex;

	// compute position of vertex
	vec4 newVertex = xForm * vec4(fuVertex, 1.0);

	// remove homogeneous coordinates => gets vertex in range from 0-1 => screen coordinates
	newVertex = vec4( newVertex.x/newVertex.w, newVertex.y/newVertex.w, newVertex.z/newVertex.w, 1);

	// then add the particleSize
	vec4 result = newVertex + vec4(fuNormal.xy * particleSize, 0, 0); 

	gl_Position = result;
}