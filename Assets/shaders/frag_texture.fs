#version 330 core

out vec4 FragColor;

in vec2 vUV;

uniform sampler2D uMainTexture;

void main() {
    vec4 color = texture(uMainTexture, vUV);
    FragColor = color;
}