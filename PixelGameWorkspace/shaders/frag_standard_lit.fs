#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec2 vUV2;
in vec4 vColor;
in vec3 vWorldPos;
in vec4 vRotationScale; // x = rotation (radians), yz = scale, w is not used.

const int MAX_TILES = 32;
const int MAX_LIGHTS = 64;
const int MAX_TILE_LIGHT_INDICES = 256;
const int MAX_LIGHT_TEXTURES = 8;

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform vec4 u_SDFResolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_Albedo;
uniform sampler2D u_Normal;
uniform float u_Cutoff;
uniform mat3 u_View;
uniform int u_LightingLayer;
uniform sampler2D u_LightOccluderMask;
uniform vec4 u_LightLayerDepth;
uniform vec4 u_ShadowArguments; // x: fadeStart, y: fadeEnd, zw unused

uniform vec4 u_TileGridInfo; // x: tileSize, y: tilesX, z: tilesY, w: lightCount
uniform int u_TileOffsets[MAX_TILES];
uniform int u_TileCounts[MAX_TILES];
uniform int u_TileLightIndices[MAX_TILE_LIGHT_INDICES];
uniform vec4 u_LightPosRadius[MAX_LIGHTS]; // xyz: world position, w: radius
uniform vec4 u_LightColors[MAX_LIGHTS];    // rgb: color, a: intensity scale
uniform int u_LightLayers[MAX_LIGHTS];
uniform int u_LightTextureIndices[MAX_LIGHTS];
uniform sampler2D u_LightTextures[MAX_LIGHT_TEXTURES];

float computeShadow(vec2 fragPos, vec2 lightPos, int fragLayer, int lightLayer)
{
	int layerDiff = max(fragLayer - lightLayer, 0);
	vec2 fragSS = (u_View * vec3(fragPos, 1.0)).xy;
	vec2 lightSS = (u_View * vec3(lightPos, 1.0)).xy;
	float fragZ = u_LightLayerDepth[fragLayer];
	float lightZ = u_LightLayerDepth[lightLayer];
	vec3 fragWS = vec3(fragPos, fragZ);
	vec3 lightWS = vec3(lightPos, lightZ);
	float dist = length(fragWS - lightWS);
	float fade = clamp((dist - u_ShadowArguments.x) / (u_ShadowArguments.y - u_ShadowArguments.x), 0.0, 1.0);
	float zDiff = abs(fragZ - lightZ);
	float t = 0.0;
	for (int layer = fragLayer - 1; layer > lightLayer; layer--)
	{
		t += (u_LightLayerDepth[layer + 1] - u_LightLayerDepth[layer]) / zDiff;
		vec2 sampleUV = mix(fragSS, lightSS, t) * u_Resolution.zw;
		sampleUV.y = 1.0 - sampleUV.y;
		vec4 mask = texture(u_LightOccluderMask, sampleUV);
		if (mask[layer] > 0.0) return fade;
	}
	return 1.0;
}

void main() {
	vec4 albedo = texture(u_Albedo, vUV) * vColor;
	if (albedo.a < u_Cutoff) {
		discard;
	}
	vec4 normalTexel = texture(u_Normal, vUV);
	float lightingMask = clamp(normalTexel.a, 0.0, 1.0);
	vec3 normalSample = normalTexel.xyz * 2.0 - 1.0;
	normalSample.y = -normalSample.y;
	float ca = cos(vRotationScale.x);
	float sa = sin(vRotationScale.x);
	mat2 rot = mat2(ca, -sa, sa, ca);
	mat2 normalMat = rot * mat2(1.0 / vRotationScale.y, 0.0, 0.0, 1.0 / vRotationScale.z);
	vec2 transformedXY = normalMat * normalSample.xy;
	vec3 N = normalize(vec3(transformedXY, max(normalSample.z, 0.0001)));
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
	for (int p = start; p < endExclusive; p++) {
		int lightIndex = u_TileLightIndices[p];
		int lightLayer = u_LightLayers[lightIndex];
		vec4 posRadius = u_LightPosRadius[lightIndex];
		float radius = max(posRadius.w, 0.0001);
		vec2 toLightXY = posRadius.xy - vWorldPos.xy;
		float toLightZ = u_LightLayerDepth[u_LightingLayer] - u_LightLayerDepth[lightLayer];
        vec3 toLight = vec3(toLightXY, toLightZ);
		vec3 L = normalize(toLight);
		float NdotL = max(dot(N, L), 0.0);
        float len = clamp(length(toLight) / radius, 0.0, 1.0);
        vec2 lightUV = len * 0.5 + vec2(0.5);
		int texIndex = clamp(u_LightTextureIndices[lightIndex], 0, MAX_LIGHT_TEXTURES - 1);
		vec3 lightTexColor = texture(u_LightTextures[texIndex], lightUV).rgb;
		vec4 lightColor = u_LightColors[lightIndex];
		float shadow = computeShadow(vWorldPos.xy, posRadius.xy, u_LightingLayer, lightLayer);
		vec3 contribution = lightColor.rgb * lightTexColor * (NdotL * shadow);
		lightAccum += contribution;
	}
	float ambient = 0.02;
	vec3 unlitColor = albedo.rgb * ambient;
	vec3 litColor = unlitColor + albedo.rgb * lightAccum;
	vec3 finalColor = mix(unlitColor, litColor, lightingMask);
	FragColor = vec4(finalColor, albedo.a);
}