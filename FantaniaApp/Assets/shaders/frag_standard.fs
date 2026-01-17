#version 330 core

out vec4 FragColor;

in vec4 vColor;
in vec2 vUV;

uniform sampler2D u_Texture;

void main() {
    vec2 uv = vec2(vUV.x, 1.0 - vUV.y);
    vec4 color = texture(u_Texture, uv) * vColor;
    FragColor = color;
}