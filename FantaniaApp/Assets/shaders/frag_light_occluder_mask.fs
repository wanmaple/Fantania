#version 330 core

out vec4 FragColor;

in vec4 vColor;
in vec2 vUV;

uniform sampler2D u_Texture;
uniform float u_Cutoff;
uniform int u_Layer;

void main() {
    vec4 texel = texture(u_Texture, vUV) * vColor;
    if (texel.a < u_Cutoff) {
        discard;
    }
    FragColor[u_Layer] = 1.0;
}