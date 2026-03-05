#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec2 vUV2;
in vec4 vColor;
in vec3 vWorldPos;

const int MAX_TILES = 32;
const int MAX_LIGHTS = 64;
const int MAX_TILE_LIGHT_INDICES = 256;
const int MAX_LIGHT_TEXTURES = 8;

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_Albedo;
uniform sampler2D u_Normal;
uniform float u_Cutoff;
uniform mat3 u_View;
uniform sampler2D u_LightOccluderSDF; // R = SignedDistance, G = NearestOccluderZ

uniform vec4 u_TileGridInfo; // x: tileSize, y: tilesX, z: tilesY, w: lightCount
uniform int u_TileOffsets[MAX_TILES];
uniform int u_TileCounts[MAX_TILES];
uniform int u_TileLightIndices[MAX_TILE_LIGHT_INDICES];
uniform vec4 u_LightPosRadius[MAX_LIGHTS]; // xyz: world position, w: radius
uniform vec4 u_LightColors[MAX_LIGHTS];    // rgb: color, a: intensity scale
uniform int u_LightTextureIndices[MAX_LIGHTS];
uniform sampler2D u_LightTextures[MAX_LIGHT_TEXTURES];

float computeShadowSDF(vec3 fragWorld, vec3 lightWorld, float lightRadius)
{
	const float DISTANCE_START_SOFTNESS = 256.0;
	const float DISTANCE_END_SOFTNESS = 384.0;
	const int MAX_STEPS = 16;
	float distToLight = length(lightWorld - fragWorld);
	float softness = clamp((distToLight - DISTANCE_START_SOFTNESS) / (DISTANCE_END_SOFTNESS - DISTANCE_START_SOFTNESS), 0.0, 1.0);
	if (softness >= 1.0) return 1.0;
	vec2 fragSS = (u_View * vec3(fragWorld.xy, 1.0)).xy;
	vec2 lightSS = (u_View * vec3(lightWorld.xy, 1.0)).xy;
	vec2 ray = lightSS - fragSS;
	float rayLen = length(ray);
	if (rayLen < 1.0) return 1.0;
	vec2 dir = ray / rayLen;
	vec2 uv = fragSS * u_Resolution.zw;
	uv.y = 1.0 - uv.y;
	vec2 sdf = texture(u_LightOccluderSDF, uv).rg;
	float lastZ = sdf.g;
	float t = 0.0;
	for (int i = 0; i < MAX_STEPS; i++)
	{
		float d = abs(sdf.x) + 4.0;
		t = clamp(t + d / rayLen, 0.0, 1.0);
		if (t >= 1.0) break;
		fragSS = mix(fragSS, lightSS, t);
		float currentZ = mix(fragWorld.z, lightWorld.z, t);
		uv = fragSS * u_Resolution.zw;
		uv.y = 1.0 - uv.y;
		sdf = texture(u_LightOccluderSDF, uv).rg;
		float occluderZ = sdf.y;
		if (occluderZ < lastZ - 1.0) return softness;
		lastZ = currentZ;
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
	vec3 N = normalize(vec3(normalSample.xy, max(normalSample.z, 0.0001)));
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
		vec4 posRadius = u_LightPosRadius[lightIndex];
		float radius = max(posRadius.w, 0.0001);
		vec2 toLightXY = posRadius.xy - vWorldPos.xy;
		float toLightZ = vWorldPos.z - posRadius.z;
        vec3 toLight = vec3(toLightXY, toLightZ);
		vec3 L = normalize(toLight);
		float NdotL = max(dot(N, L), 0.0);
        float len = clamp(length(toLight) / radius, 0.0, 1.0);
        vec2 lightUV = len * 0.5 + vec2(0.5);
		int texIndex = clamp(u_LightTextureIndices[lightIndex], 0, MAX_LIGHT_TEXTURES - 1);
		vec3 lightTexColor = texture(u_LightTextures[texIndex], lightUV).rgb;
		vec4 lightColor = u_LightColors[lightIndex];
		float shadow = computeShadowSDF(vWorldPos, posRadius.xyz, radius);
		vec3 contribution = lightColor.rgb * lightTexColor * (lightColor.a * NdotL * shadow);
		lightAccum += contribution;
	}
	float ambient = 0.02;
	vec3 unlitColor = albedo.rgb * ambient;
	vec3 litColor = unlitColor + albedo.rgb * lightAccum;
	vec3 finalColor = mix(unlitColor, litColor, lightingMask);
	FragColor = vec4(finalColor, albedo.a);
}