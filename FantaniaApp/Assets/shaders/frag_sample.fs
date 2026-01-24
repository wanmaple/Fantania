#version 330 core

out vec4 FragColor;

in vec2 vUV;

uniform sampler2D u_Texture;

vec3 gamma(vec3 color) {
    return vec3(
        pow(color.r, 1.0 / 2.2),
        pow(color.g, 1.0 / 2.2),
        pow(color.b, 1.0 / 2.2)
    );
}

void main() {
    vec2 uv = vec2(vUV.x, 1.0 - vUV.y);
    vec4 color = texture(u_Texture, uv);
    color.rgb = gamma(color.rgb);
    FragColor = color;
}