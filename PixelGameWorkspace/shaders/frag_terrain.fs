#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;
in vec3 vWorldPos;

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_Texture;
uniform sampler2D u_NoiseBrightness;
uniform float u_Cutoff;
uniform vec4 u_BrightnessArgs; // x: brightness noise scale, y: brightness noise strength, z: brightness quantization steps, w: luminance threshold for all effects.
uniform vec4 u_BrightnessColor; // xyz: color of brightness effect, w: unused

float samplePixelLockedNoise(sampler2D tex, vec2 worldPos, float scale) {
    vec2 uv = worldPos * scale;
    return texture(tex, uv).r;
}

float quantizeBand(float v, float steps) {
    return floor(clamp(v, 0.0, 1.0) * steps) / max(1.0, steps - 1.0);
}

void main() {
    vec4 texColor = texture(u_Texture, vUV) * vColor;
    if (texColor.a < u_Cutoff) {
        discard;
    }
    float texLuma = step(u_BrightnessArgs.w, dot(texColor.rgb, vec3(0.299, 0.587, 0.114)));
    float nBrightness = samplePixelLockedNoise(u_NoiseBrightness, vWorldPos.xy, u_BrightnessArgs.x * 0.1);
    nBrightness = quantizeBand(nBrightness, u_BrightnessArgs.z);
    float brightnessMixFactor = clamp(nBrightness * u_BrightnessArgs.y * texLuma, 0.0, 1.0);
    vec3 color = texColor.rgb;
    color = mix(color, u_BrightnessColor.rgb, brightnessMixFactor);
    FragColor = vec4(color, texColor.a);
}