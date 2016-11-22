attribute vec3 fuVertex;
attribute vec3 fuNormal;
uniform vec2 particleSize;
uniform mat4 xForm;
varying vec3 modelpos;
        
void main()
{
	modelpos = fuVertex;
	vec4 vScreen = xForm*vec4(fuVertex, 1.0);       
	gl_Position = vScreen + vec4(fuNormal.xy*particleSize, 0, 0);
}