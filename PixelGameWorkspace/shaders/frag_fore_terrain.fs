#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;
in vec3 vWorldPos;

const int MAX_TILES = 32;
const int MAX_LIGHTS = 64;
const int MAX_TILE_LIGHT_INDICES = 256;
const int TERRAIN_LIGHTING_LAYER = 1;

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_Texture;
uniform sampler2D u_NoiseBrightness;
uniform float u_Cutoff;
uniform vec4 u_BrightnessArgs; // x: brightness noise scale, y: brightness noise strength, z: brightness quantization steps, w: luminance threshold for all effects.
uniform vec4 u_BrightnessColor; // xyz: color of brightness effect, w: unused
uniform vec4 u_LightLayerDepth;
uniform vec4 u_EnvAmbient;
uniform vec4 u_EnvLight; // xyz: direction, w: intensity
uniform vec4 u_EnvLightColor;

uniform vec4 u_TileGridInfo; // x: tileSize, y: tilesX, z: tilesY, w: lightCount
uniform int u_TileOffsets[MAX_TILES];
uniform int u_TileCounts[MAX_TILES];
uniform int u_TileLightIndices[MAX_TILE_LIGHT_INDICES];
uniform vec4 u_LightPosRadius[MAX_LIGHTS]; // xyz: world position, w: radius
uniform vec4 u_LightColors[MAX_LIGHTS];    // rgb: color, a: intensity scale
uniform vec4 u_LightArgs[MAX_LIGHTS];      // xyz: unused, w: intensity
uniform int u_LightLayers[MAX_LIGHTS];
uniform int u_LightTextureIndices[MAX_LIGHTS];

float samplePixelLockedNoise(sampler2D tex, vec2 worldPos, float scale) {
    vec2 uv = worldPos * scale;
    return texture(tex, uv).r;
}

float quantizeBand(float v, float steps) {
    return floor(clamp(v, 0.0, 1.0) * steps) / max(1.0, steps - 1.0);
}

float luminance(vec3 c) {
	return dot(c, vec3(0.299, 0.587, 0.114));
}

void main() {
    vec4 texColor = texture(u_Texture, vUV);
    if (texColor.a < u_Cutoff) {
        discard;
    }
	float texLuma = step(u_BrightnessArgs.w, luminance(texColor.rgb));
    float nBrightness = samplePixelLockedNoise(u_NoiseBrightness, vWorldPos.xy, u_BrightnessArgs.x * 0.1);
    nBrightness = quantizeBand(nBrightness, u_BrightnessArgs.z);
    float brightnessMixFactor = clamp(nBrightness * u_BrightnessArgs.y * texLuma, 0.0, 1.0);
	vec3 baseColor = texColor.rgb * vColor.rgb * vColor.a;
	vec3 stripeColor = u_BrightnessColor.rgb;
	vec3 color = baseColor + stripeColor * brightnessMixFactor;

	float tileSize = max(u_TileGridInfo.x, 1.0);
	int tilesX = int(u_TileGridInfo.y);
	int tilesY = int(u_TileGridInfo.z);
	int lightCount = int(u_TileGridInfo.w);
	vec2 screenPos = vec2(gl_FragCoord.x, u_Resolution.y - gl_FragCoord.y);
	int tileX = int(floor(screenPos.x / tileSize));
	int tileY = int(floor(screenPos.y / tileSize));
	tileX = clamp(tileX, 0, max(tilesX - 1, 0));
	tileY = clamp(tileY, 0, max(tilesY - 1, 0));
	int tileId = tileY * tilesX + tileX;
	vec3 lightAccum = vec3(0.0);
	int start = u_TileOffsets[tileId];
	int count = u_TileCounts[tileId];
	int endExclusive = min(start + count, MAX_TILE_LIGHT_INDICES);
    vec3 N = vec3(0.0, 0.0, 1.0);
	for (int p = start; p < endExclusive; p++) {
		int lightIndex = u_TileLightIndices[p];
		int lightLayer = u_LightLayers[lightIndex];
		vec4 posRadius = u_LightPosRadius[lightIndex];
		float radius = max(posRadius.w, 0.0001);
		vec2 toLightXY = posRadius.xy - vWorldPos.xy;
		float toLightZ = u_LightLayerDepth[TERRAIN_LIGHTING_LAYER] - u_LightLayerDepth[lightLayer];
        vec3 toLight = vec3(toLightXY, toLightZ);
		vec3 L = normalize(toLight);
		float NdotL = max(dot(N, L), 0.0);
        float len = clamp(length(toLight) / radius, 0.0, 1.0);
		float lightAttenuation = max(1.0 - len, 0.0);
		vec4 lightColor = u_LightColors[lightIndex];
		float intensity = u_LightArgs[lightIndex].w;
		lightColor.rgb *= intensity;
		vec3 contribution = lightColor.rgb * lightAttenuation * NdotL;
		lightAccum += contribution;
    }
	{
		// Add environment lighting
		vec3 envLightDir = -normalize(u_EnvLight.xyz);
		vec3 envLightColor = u_EnvLightColor.rgb * u_EnvLight.w;
		float NdotL = max(dot(N, envLightDir), 0.0);
		vec3 envContribution = envLightColor * NdotL;
		lightAccum += envContribution;
	}
    vec3 unlitColor = color * u_EnvAmbient.rgb;
	vec3 litColor = unlitColor + color * lightAccum;
    FragColor = vec4(litColor, texColor.a);
}