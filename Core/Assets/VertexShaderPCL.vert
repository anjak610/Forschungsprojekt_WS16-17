attribute vec3 fuVertex;
attribute vec3 fuColor;

uniform mat4 FUSEE_MV;
uniform mat4 FUSEE_MVP;

uniform float pointSize;
uniform vec3 n_cloudCenterWorld;
uniform float n_cloudRadius;

varying vec3 n_depthColor;
varying vec3 color;

void main()
{
	color = fuColor;
	
	vec3 modelpos = fuVertex;
	vec4 result = FUSEE_MVP * vec4(modelpos, 1.0);	
	
	// New
	
	vec4 cloudCenterCam = FUSEE_MV  *  vec4(n_cloudCenterWorld, 1.0); //statt modelpos sollte hier zCenter hin
    vec4 particleCam = FUSEE_MV * vec4(modelpos, 1.0);
    float zToCenter = cloudCenterCam.z - particleCam.z;
	float scaledZ = (zToCenter / n_cloudRadius) * 0.8 + 1.0;
	
	n_depthColor = vec3(0.0, scaledZ,  0.7);
	
	gl_PointSize = pointSize;
	gl_Position = result;
}