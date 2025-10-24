#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;
in vec4 vTiling;
in vec4 vCustom;
in vec4 vCustom2;

uniform sampler2D uMainTexture;
uniform vec4 uGlowColor;
uniform vec4 uGlowParameters;   // x is max strength, y is glow width.

void main() {
    vec2 uv = vUV * vTiling.xy + vTiling.zw;
    vec4 color = texture(uMainTexture, uv) * vColor;
    float alpha = color.a;
    vec2 pixelSize = vec2(1.0) / abs(vCustom.xy);
    float minDist = 1.0;
    if (alpha < 0.99) {
        for (int i = 0; i < uGlowParameters.y; i++) {
            float offset = i * max(pixelSize.x, pixelSize.y);
            float aL = texture(uMainTexture, uv + vec2(-offset, 0.0)).a;
            float aR = texture(uMainTexture, uv + vec2(offset, 0.0)).a;
            float aT = texture(uMainTexture, uv + vec2(0.0, -offset)).a;
            float aB = texture(uMainTexture, uv + vec2(0.0, offset)).a;
            if (aL >= 0.99 || aR >= 0.99 || aT >= 0.99 || aB >= 0.99) {
                minDist = min(minDist, i / uGlowParameters.y);
            }
        }
    }
    color.a = max(color.a, 1.0 - minDist);
    float t = color.a * uGlowColor.a;
    t = t * t;
    vec3 mixColor = mix(color.rgb, uGlowColor.rgb, t);
    FragColor = vec4(mixColor, color.a);
}