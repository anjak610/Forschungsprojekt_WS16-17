attribute vec3 fuVertex;
attribute vec3 fuNormal;

uniform mat4 FUSEE_MV;
uniform mat4 FUSEE_MVP;
uniform vec2 particleSize;

uniform vec2 zBounds;
uniform float zZoom;

varying float near;
varying float far;
varying vec4 newVertex;
varying float zoom;

uniform vec3 n_cloudCenterWorld;
uniform float n_cloudRadius;
varying vec3 n_depthColor;
//varying vec3 normalSpec;
        
void main()
{
	vec3 modelpos = fuVertex;
	
	near = zBounds.x;
	far = zBounds.y;	
	zoom = zZoom;
		
	//normalSpec = normalize(mat3(FUSEE_MV)* fuNormal);
	vec4 newVertex = FUSEE_MVP * vec4(modelpos, 1.0);
	//float n_zdist = newVertex.z / newVertex.w;	
	vec4 result = newVertex + vec4(fuNormal.xy * particleSize, 0, 0);

	// New
	vec4 cloudCenterCam = FUSEE_MV  *  vec4(n_cloudCenterWorld, 1.0); //statt modelpos sollte hier zCenter hin
    vec4 particleCam = FUSEE_MV * vec4(modelpos, 1.0);
    float zToCenter = cloudCenterCam.z - particleCam.z;
	float scaledZ = (zToCenter / n_cloudRadius) * 0.5 + 1;
	n_depthColor = vec3(0.0, scaledZ, scaledZ);
    
	
	gl_Position = result;
}