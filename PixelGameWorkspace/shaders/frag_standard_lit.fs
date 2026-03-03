#version 330 core

out vec4 FragColor;

in vec2 vUV;
in vec2 vUV2;
in vec4 vColor;
in vec3 vWorldPos;

const int MAX_TILES = 64;
const int MAX_LIGHTS = 32;
const int MAX_TILE_LIGHT_INDICES = 256;
const int MAX_LIGHT_TEXTURES = 8;

uniform vec4 u_Resolution;   // xy is resolution, zw is 1/resolution.
uniform sampler2D u_Albedo;
uniform sampler2D u_Normal;

uniform vec4 u_TileGridInfo; // x: tileSize, y: tilesX, z: tilesY, w: lightCount
uniform int u_TileOffsets[MAX_TILES];
uniform int u_TileCounts[MAX_TILES];
uniform int u_TileLightIndices[MAX_TILE_LIGHT_INDICES];
uniform vec4 u_LightPosRadius[MAX_LIGHTS]; // xyz: world position, w: radius
uniform vec4 u_LightColors[MAX_LIGHTS];    // rgb: color, a: intensity scale
uniform int u_LightTextureIndices[MAX_LIGHTS];
uniform sampler2D u_LightTextures[MAX_LIGHT_TEXTURES];

void main() {
	vec4 albedo = texture(u_Albedo, vUV) * vColor;
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
		vec3 contribution = lightColor.rgb * lightTexColor * (lightColor.a * NdotL);
		lightAccum += contribution;
	}
	vec3 finalColor = albedo.rgb + (lightAccum * lightingMask);
	FragColor = vec4(finalColor, albedo.a);
}