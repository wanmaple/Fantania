#version 330 core

out vec2 FragColor;

in vec4 vColor;
in vec2 vUV;
in float vWorldZ;

uniform sampler2D u_Texture;
uniform float u_Cutoff;

void main() {
    vec4 texel = texture(u_Texture, vUV) * vColor;
    if (texel.a < u_Cutoff) {
        discard;
    }
    FragColor = vec2(vWorldZ, 1.0);
}