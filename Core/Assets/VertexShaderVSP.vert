attribute vec3 fuVertex;
attribute vec3 fuNormal;
attribute vec2 fuUV;
attribute vec3 fuInstance;

uniform mat4 FUSEE_MV;
uniform mat4 FUSEE_MVP;
uniform vec2 yBounds; // x = min, y = max

varying vec3 normal;
varying vec2 uv;
varying vec3 albedo;

vec3 hsv2rgb(in vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    vec3 result = c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);

    return result;
}

void main()
{
	vec3 modelpos = fuVertex + fuInstance;
	normal = normalize(mat3(FUSEE_MV) * fuNormal);

	uv = fuUV;
	
	// albedo depends on yBounds

	float hue = (modelpos.y - yBounds.x) / (yBounds.y - yBounds.x);
	albedo = hsv2rgb( vec3(hue, 1, 1) );

	gl_Position = FUSEE_MVP * vec4(modelpos, 1.0);
}