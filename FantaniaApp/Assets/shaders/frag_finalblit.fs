#version 330 core

out vec4 FragColor;

in vec2 vUV;

uniform sampler2D u_MainTexture;

void main() {
    vec3 color = texture(u_MainTexture, vUV).rgb;
    FragColor = vec4(color, 1.0);
}