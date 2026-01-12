#version 330 core

out vec4 FragColor;

in vec2 vUV;

uniform sampler2D uMainTexture;

vec3 gamma(vec3 color) {
    return vec3(
        pow(color.r, 1.0 / 2.2),
        pow(color.g, 1.0 / 2.2),
        pow(color.b, 1.0 / 2.2)
    );
}

void main() {
    vec3 color = texture(uMainTexture, vUV).rgb;
    // color = gamma(color);
    FragColor = vec4(color, 1.0);
}