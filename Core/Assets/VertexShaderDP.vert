attribute vec3 fuVertex;

uniform mat4 FUSEE_MVP;
        
void main()
{
	vec4 result = FUSEE_MVP * vec4(fuVertex, 1.0);
	gl_Position = result;
}