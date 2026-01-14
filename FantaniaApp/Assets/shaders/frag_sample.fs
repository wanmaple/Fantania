#version 330 core

out vec4 FragColor;

in vec2 vUV;

uniform sampler2D u_Texture;

void main() {
    vec2 uv = vec2(vUV.x, 1.0 - vUV.y);
    vec3 color = texture(u_Texture, uv).rgb;
    FragColor = vec4(color, 1.0);
}