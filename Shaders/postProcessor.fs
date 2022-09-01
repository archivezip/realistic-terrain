#version 330 core

uniform sampler2D scene;

in vec2 TexCoords;

out vec4 FragColor;

void main()
{
    vec4 orig = texture2D(scene, TexCoords);
    FragColor = orig;
}
