#ifdef GL_ES
    precision highp float;     
#endif

varying float near;
varying float far;
varying vec4 newVertex;
varying float zoom;

varying vec2 uv;
uniform sampler2D tex;
varying vec2 texcoord;

void main()
{	
	vec3 texRGB = texture2D(tex,uv).rgb;
	float texA = texture2D(tex,uv).a;

	vec3 pointFar = vec3(0.0,0.0,far);
	vec3 pointNear = vec3(0.0,0.0,near);

	vec3 point = vec3(0.0,0.0,far/2.0);
	vec3 normal = vec3(0.0,0.0,1.0);
	float d = dot(-point, normal);
	vec4 u_abcd = vec4 (normal.x, normal.y, normal.z, d);

	float zlength = distance(pointFar,pointNear) + (zoom+(zoom/2.0));
	
	float depth = dot(u_abcd.xyz, newVertex.xyz)+ u_abcd.w;	
	vec3 color = vec3(1.0-depth/zlength)*tex.rgb;
	gl_FragColor = vec4(color, 1.0);	

}
