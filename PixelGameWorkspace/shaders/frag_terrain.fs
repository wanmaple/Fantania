#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;
in vec3 vWorldPos;

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_Texture;
uniform sampler2D u_NoiseBrightness;
uniform sampler2D u_NoiseCrack;
uniform float u_Cutoff;
uniform vec4 u_BrightnessArgs; // x: brightness noise scale, y: brightness noise strength, z: brightness quantization steps, w: luminance threshold for all effects.
uniform vec4 u_BrightnessColor; // xyz: color of brightness effect, w: crack noise scale
uniform vec4 u_CrackArgs; // x: threshold min, y: threshold max, z: sharpness, w: strength

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
    // float texLuma = dot(texColor.rgb, vec3(0.299, 0.587, 0.114));
    // float mortarMask = 1.0 - smoothstep(0.18, 0.34, texLuma);
    // float brickMask = 1.0 - mortarMask;

    // float nBrightness = samplePixelLockedNoise(u_NoiseBrightness, vWorldPos.xy, u_BrightnessGrain.x);
    // float nGrain = samplePixelLockedNoise(u_NoiseGrain, vWorldPos.xy, u_BrightnessGrain.y);
    // float nCrack = samplePixelLockedNoise(u_NoiseGrain, vWorldPos.xy, u_BrightnessGrain.y * 0.35);

    // // Pixel-art friendly: reduce continuous gradients into discrete tone bands.
    // // nBrightness = quantizeBand(nBrightness, 3.0);
    // // nGrain = quantizeBand(nGrain, 4.0);
    // // nCrack = quantizeBand(nCrack, 6.0);

    // float shadeMul = 1.0 + (nBrightness - 0.5) * u_BrightnessGrain.z;
    // float grainAdd = (nGrain - 0.5) * u_BrightnessGrain.w;

    // // Ridged mask from grain noise, then thresholded into hard pixel cracks.
    // float ridged = 1.0 - abs(nCrack * 2.0 - 1.0);
    // ridged = pow(max(0.0, ridged), u_Crack.z);

    // float crackThreshold = mix(u_Crack.x, u_Crack.y, 0.5);
    // float crackMask = step(crackThreshold, ridged);
    // crackMask *= brickMask;

    // vec3 color = texColor.rgb;
    // color *= mix(1.0, shadeMul, brickMask);
    // color += grainAdd * brickMask;
    // color *= (1.0 - crackMask * u_Crack.w);
    // FragColor = vec4(clamp(color, 0.0, 1.0), texColor.a);
    float texLuma = step(u_BrightnessArgs.w, dot(texColor.rgb, vec3(0.299, 0.587, 0.114)));
    float nBrightness = samplePixelLockedNoise(u_NoiseBrightness, vWorldPos.xy, u_BrightnessArgs.x * 0.1);
    nBrightness = quantizeBand(nBrightness, u_BrightnessArgs.z);
    float brightnessMixFactor = clamp(nBrightness * u_BrightnessArgs.y * texLuma, 0.0, 1.0);
    float crackScale = u_BrightnessColor.w;
    vec2 crackUv = vWorldPos.xy * crackScale;
    vec2 crackTexel = 1.0 / vec2(textureSize(u_NoiseCrack, 0));
    float nCrackCenter = texture(u_NoiseCrack, crackUv).r;
    float nCrackX = texture(u_NoiseCrack, crackUv + vec2(crackTexel.x, 0.0)).r;
    float nCrackY = texture(u_NoiseCrack, crackUv + vec2(0.0, crackTexel.y)).r;

    // Use local noise gradient to extract thin, crack-like lines.
    float crackGradient = length(vec2(nCrackX - nCrackCenter, nCrackY - nCrackCenter));
    float crackSignal = clamp(crackGradient * 8.0, 0.0, 1.0);
    crackSignal = pow(crackSignal, max(0.0001, u_CrackArgs.z));

    float crackInRange = step(u_CrackArgs.x, crackSignal) * step(crackSignal, u_CrackArgs.y);
    float crackMask = smoothstep(u_CrackArgs.x, u_CrackArgs.y, crackSignal) * crackInRange;
    crackMask *= texLuma;
    vec3 color = texColor.rgb;
    color = mix(color, u_BrightnessColor.rgb, brightnessMixFactor);
    color *= (1.0 - crackMask * u_CrackArgs.w);
    // FragColor = vec4(vec3(crackGradient), 1.0);
    FragColor = vec4(color, texColor.a);
}