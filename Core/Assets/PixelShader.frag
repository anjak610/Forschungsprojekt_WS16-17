#ifdef GL_ES
    precision highp float;     
#endif

varying vec3 modelpos;

void main()
{
    gl_FragColor = vec4(1, 0.5, modelpos.z*0.01+ 0.8, 1);
}