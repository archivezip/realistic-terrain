#version 330 core

float linearizeDepth(float depth);

in vec2 TexCoords;

uniform sampler2D depthMap;

out vec4 FragColor;

const float near_plane = 1;
const float far_plane = 1000;

void main()
{
    float depth = texture(depthMap, TexCoords).r;
    FragColor = vec4(vec3(linearizeDepth(depth) / far_plane), 1.0);
    //FragColor = vec4(vec3(depth), 1.0);
}

float linearizeDepth(float depth)
{
   float z = depth * 2.0 - 1.0; // ndc
   return (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));
}
