#ifdef GL_ES
    precision highp float;     
#endif

//varying vec3 normal;
//varying vec2 uv;
varying float near;
varying float far;
varying vec4 newVertex;
varying float zoom;

void main()
{	
	vec3 pointFar = vec3(0.0,0.0,far);
	vec3 pointNear = vec3(0.0,0.0,near);

	vec3 point = vec3(0.0,0.0,far/2.0);
	vec3 normal = vec3(0.0,0.0,1.0);
	float d = dot(-point, normal);
	vec4 u_abcd = vec4 (normal.x, normal.y, normal.z, d);

	float zlength = distance(pointFar,pointNear) + (zoom+(zoom/2.0));
	//float zlength = 150.0;//distance(pointFar,pointNear);//Muss immer mit der Mitte des Objektes mitgehen (Mittelpunkt von zmax und zmin in den shader übergeben und in worldspace koordinaten umrechnen!)

	//float zlength = far/2.0;
	//vec3 normal = vec3(0.0,0.0,1.0);
	//vec3 mid = vec3(0.0,0.0,zlength);

	//float refpoint = dot(pointFar, normal);
	//float testpoint = dot(newVertex.xyz, normal);

	float depth = dot(u_abcd.xyz, newVertex.xyz)+ u_abcd.w;	
	vec3 color = vec3(1.0-depth/zlength);
	gl_FragColor = vec4(color, 1.0);

	//vec3 color = vec3(depth/zlength);
	//gl_FragColor = vec4 (0.0,0.0,(depth/zlength), 1.0);	 

}
//vec4 color;
//float near = 1.0; 
//float far  = 100.0; 
  
//float LinearizeDepth(float depth) 
//{
//    float z = depth * 2.0 - 1.0; // Back to NDC 
//    return (2.0 * near * far) / (far + near - z * (far - near));	
//}
//
//void main()
//{             
//    float depth = LinearizeDepth(gl_FragCoord.z) / far; // divide by far for demonstration
//    color = vec4(vec3(depth), 1.0f);
//}