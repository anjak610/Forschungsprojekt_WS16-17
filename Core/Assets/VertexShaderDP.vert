attribute vec3 fuVertex;
attribute vec3 fuNormal;
attribute vec3 fuInstance;

uniform mat4 FUSEE_MVP;
        
void main()
{
	vec4 newVertex = FUSEE_MVP * vec4(fuVertex + fuInstance, 1.0);
	vec4 result = newVertex + vec4(fuNormal.xy * 0.1, 0.0, 0.0);

	gl_Position = result;
}