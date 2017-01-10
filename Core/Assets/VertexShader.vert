attribute vec3 fuVertex;
attribute vec3 fuNormal;
uniform vec2 particleSize;
uniform mat4 xForm;
        
void main()
{
	vec4 newVertex = xForm * vec4(fuVertex, 1.0);
	vec4 result = newVertex  + vec4(fuNormal.xy * particleSize, 0, 0);

	gl_Position = result;
}