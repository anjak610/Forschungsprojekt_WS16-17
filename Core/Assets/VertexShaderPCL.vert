attribute vec3 fuVertex;
attribute vec3 fuNormal;

uniform mat4 FUSEE_MVP;
uniform vec2 particleSize;
        
void main()
{
	vec4 newVertex = FUSEE_MVP * vec4(fuVertex, 1.0);
	vec4 result = newVertex  + vec4(fuNormal.xy * particleSize, 0, 0);

	gl_Position = result;
}