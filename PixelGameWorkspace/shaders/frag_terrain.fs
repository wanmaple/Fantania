#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;
in vec3 vWorldPos;

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_Texture;
uniform sampler2D u_NoiseBrightness;
uniform sampler2D u_NoiseGrain;
uniform float u_Cutoff;
uniform vec4 u_BrightnessGrain; // x: brightness noise scale, y: grain noise scale, z: brightness noise strength, w: grain shade strength
uniform vec4 u_Crack; // x: threshold min, y: threshold max, z: sharpness, w: strength

float samplePixelLockedNoise(sampler2D tex, vec2 worldPos, float scale) {
    vec2 uv = fract(worldPos * scale);
    vec2 texSize = vec2(textureSize(tex, 0));
    vec2 texel = 1.0 / texSize;
    vec2 snapped = (floor(uv * texSize) + 0.5) * texel;
    return texture(tex, snapped).r;
}

float quantizeBand(float v, float steps) {
    return floor(clamp(v, 0.0, 1.0) * steps) / max(1.0, steps - 1.0);
}

void main() {
    vec4 texColor = texture(u_Texture, vUV) * vColor;
    if (texColor.a < u_Cutoff) {
        discard;
    }

    float texLuma = dot(texColor.rgb, vec3(0.299, 0.587, 0.114));
    float mortarMask = 1.0 - smoothstep(0.18, 0.34, texLuma);
    float brickMask = 1.0 - mortarMask;

    float nBrightness = samplePixelLockedNoise(u_NoiseBrightness, vWorldPos.xy, u_BrightnessGrain.x);
    float nGrain = samplePixelLockedNoise(u_NoiseGrain, vWorldPos.xy, u_BrightnessGrain.y);
    float nCrack = samplePixelLockedNoise(u_NoiseGrain, vWorldPos.xy, u_BrightnessGrain.y * 0.35);

    // Pixel-art friendly: reduce continuous gradients into discrete tone bands.
    nBrightness = quantizeBand(nBrightness, 3.0);
    nGrain = quantizeBand(nGrain, 4.0);
    nCrack = quantizeBand(nCrack, 6.0);

    float shadeMul = 1.0 + (nBrightness - 0.5) * u_BrightnessGrain.z;
    float grainAdd = (nGrain - 0.5) * u_BrightnessGrain.w;

    // Ridged mask from grain noise, then thresholded into hard pixel cracks.
    float ridged = 1.0 - abs(nCrack * 2.0 - 1.0);
    ridged = pow(max(0.0, ridged), u_Crack.z);

    float crackThreshold = mix(u_Crack.x, u_Crack.y, 0.5);
    float crackMask = step(crackThreshold, ridged);
    crackMask *= brickMask;

    vec3 color = texColor.rgb;
    color *= mix(1.0, shadeMul, brickMask);
    color += grainAdd * brickMask;
    color *= (1.0 - crackMask * u_Crack.w);
    FragColor = vec4(clamp(color, 0.0, 1.0), texColor.a);
}