#version 330 core

out vec4 FragColor;

in vec2 vUV;

uniform sampler2D uMainTexture;
uniform vec2 uTextureSize;

const float WEIGHTS[5] = float[](0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162);

vec4 gaussian9(sampler2D tex, vec2 uv, vec2 texel, vec2 dir) {
    vec2 off = dir * texel;
    return texture(tex, uv) * WEIGHTS[0] +
        texture(tex, uv + off) * WEIGHTS[1] +
        texture(tex, uv - off) * WEIGHTS[1] +
        texture(tex, uv + off * 2.0) * WEIGHTS[2] +
        texture(tex, uv - off * 2.0) * WEIGHTS[2] +
        texture(tex, uv + off * 3.0) * WEIGHTS[3] + 
        texture(tex, uv - off * 3.0) * WEIGHTS[3] +
        texture(tex, uv + off * 4.0) * WEIGHTS[4] + 
        texture(tex, uv - off * 4.0) * WEIGHTS[4];
}

void main() {
    vec2 texel = vec2(1.0) / uTextureSize;
    vec4 color = vec4(0.0);
    for (int i = 0; i < 9; i++)
    {
        color += gaussian9(uMainTexture, vUV + vec2(0.0, i - 4) * texel, texel, vec2(1.0, 0.0)) * WEIGHTS[abs(4 - i)];
    }
    FragColor = color;
}