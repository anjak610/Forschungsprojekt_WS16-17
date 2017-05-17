#ifdef GL_ES
    precision highp float;     
#endif

varying float near;
varying float far;
varying vec4 newVertex;
varying float zoom;


void main()
{
	vec3 pointFar = vec3(0.0,0.0,far);
	vec3 pointNear = vec3(0.0,0.0,near);
		
//if(near < 1.0)
//{
//	pointNear = vec3(0.0,0.0,1.0);
//
//}
//else
//	{
//		pointNear = vec3(0.0,0.0,near);
//	}

	//if(far > 20)
	//{
	//	pointFar = vec3(0.0,0.0,20.0);
	//}
	
	vec3 point = vec3(0.0,0.0,far/2.0);
	vec3 normal = vec3(0.0,0.0,1.0);
	float d = dot(point, normal);
	vec4 u_abcd = vec4 (normal.x, normal.y, normal.z, d);
	
	float zlength = distance(pointFar, pointNear)+zoom;
	//float zlength = abs(pointNear-pointFar)+zoom;
	
	
	float depth = dot(u_abcd.xyz, newVertex.xyz)+ u_abcd.w;	
	vec3 color = vec3(1.0- (depth/zlength));
	gl_FragColor = vec4(color, 1.0); 
	
	if( depth/zlength > 1.0)
		{
			gl_FragColor = vec4(0.0,0.0,1.0, 1.0); 
	
		}
	//float depth = LinearizeDepth(gl_FragCoord.z)/far;
	//gl_FragColor = vec4(depth,depth,depth,1.0);
	
}