#ifdef GL_ES
    precision highp float;     
#endif

varying float near;
varying float far;
varying vec4 newVertex;
varying float zoom;
varying vec3 n_depthColor;

//varying vec3 normalSpec;

//float LinearizeDepth(float depth) 
//{
//    float z = depth * 2.0 - 1.0; // Back to NDC 
//    return (2.0 * near * far) / (far + near - z * (far - near));	
//}

void main()
{
	
		
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

	//float zlength = distance(pointFar, pointNear);

	//zlength += zoom;
	vec3 pointFar = vec3(0.0,0.0,far);
	vec3 pointNear = vec3(0.0,0.0,near);

	vec3 point = vec3(0.0,0.0,far/2.0);
	vec3 normal = vec3(0.0,0.0,1.0);
	float d = dot(-point, normal);
	vec4 u_abcd = vec4 (normal.x, normal.y, normal.z, d);

	float zlength = distance(pointFar,pointNear) ;
	
	float depth = dot(u_abcd.xyz, newVertex.xyz)+ u_abcd.w+ (zoom+(zoom/2.0));	
	vec3 color = vec3(1.0-depth/zlength);
	// gl_FragColor = vec4(color, 1.0);
	gl_FragColor = vec4(n_depthColor, 1.0);


	//float intensity = dot(normalSpec, vec3(0,0,-1));
	//gl_FragColor = vec4(intensity,intensity,intensity,1.0);
	
	
	//float depth = (LinearizeDepth(zlength)*far); 	
	//float trueLinearDepth=(LinearizeDepth(zlength)-near)/(far-near);
	//vec4 color = vec4(vec3(trueLinearDepth), 1.0);



	//gl_FragColor = vec4(color);
	
	//if( zlength < 20.0)
	//	{
	//		gl_FragColor = vec4(0.0,0.0,1.0+zoom, 1.0); 	
	//	}
	//float depth = LinearizeDepth(gl_FragCoord.z)/far;
	//gl_FragColor = vec4(depth,depth,depth,1.0);
	
}