#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;

uniform mat4 model;

out vec2 texVS;
out vec3 posVS;

void main()
{
    texVS = aTexCoords;  
    posVS = ( model * vec4(aPos, 1.0)).xyz;
}